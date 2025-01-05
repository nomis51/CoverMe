package com.jetbrains.rider.plugins.coverme.services

import com.jetbrains.rider.plugins.coverme.Constants
import com.jetbrains.rider.plugins.coverme.Environments
import com.jetbrains.rider.plugins.coverme.helpers.FileHelper
import com.jetbrains.rider.plugins.coverme.ipc.IpcClient
import com.jetbrains.rider.plugins.coverme.models.ipc.abstractions.ProtocolMessage
import com.jetbrains.rider.plugins.coverme.models.ipc.abstractions.ProtocolMessageTypes
import com.jetbrains.rider.plugins.coverme.services.abstractions.IProtocolService
import okhttp3.OkHttpClient
import okhttp3.Request
import java.io.File
import java.io.FileOutputStream
import java.io.IOException
import java.net.URL
import java.util.concurrent.TimeUnit
import java.util.zip.ZipInputStream

@Suppress("KotlinConstantConditions")
class BackendService : IProtocolService {
    private var _ipcClient: IpcClient? = null
    private var _channelId: String? = null

    override fun handleMessage(message: ProtocolMessage) {
        message.channelId = _channelId ?: return
        _ipcClient?.writeMessage(message)
    }

    fun createChannel() {
        if (!ensureBackendStarted()) {
            LoggingService.getInstance()
                .error("BackendService: unable to start backend")
            return
        }

        val client = OkHttpClient()
        val url = "${Constants.BACKEND_URL}/api/channel"
        val request = Request.Builder().url(url).build()

        client.newCall(request).execute().use {
            if (!it.isSuccessful) {
                LoggingService.getInstance().error("BackendService: failed to create channel: ${it.message}")
            } else {
                _channelId = it.body!!.string()
                LoggingService.getInstance()
                    .info("BackendService: created channel: $_channelId")
                _ipcClient = IpcClient("\\\\.\\pipe\\${_channelId}", ::handleIpcMessage)
                AppService.getInstance()
                    .initializeAppBrowser(_channelId!!)
            }
        }
    }

    fun handleIpcMessage(message: ProtocolMessage) {
        AppService.getInstance().dispatchMessageToIntellij(message)
    }

    fun sendMessage(message: ProtocolMessage) {
        message.channelId = _channelId ?: return
        _ipcClient?.writeMessage(message)
    }

    fun sendMessageAndWaitResponse(message: ProtocolMessage): ProtocolMessage? {
        message.channelId = _channelId ?: return null

        if (Constants.ENV == Environments.HEADLESS) {
            if (message.type == ProtocolMessageTypes.GET_FILE_LINE_COVERAGE) {
                return ProtocolMessage(message.id, message.type, "true")
            }
        }

        return _ipcClient?.writeMessageAndWaitResponse(message)
    }

    private fun ensureBackendStarted(): Boolean {
        if (Constants.ENV != Environments.PRODUCTION) return true

        if (checkBackendStatus()) return true

        return FileHelper.lockFileAndPerformOperation(
            "${System.getProperty("user.home")}/${Constants.APP_FOLDER_NAME}/${Constants.APP_LOCK_FILE_NAME}",
            operation = {
                if (!downloadBackend()) return@lockFileAndPerformOperation false
                if (!startBackend()) return@lockFileAndPerformOperation false

                return@lockFileAndPerformOperation true
            },
            retryCount = 3
        )
    }

    private fun checkBackendStatus(): Boolean {
        val client = OkHttpClient
            .Builder()
            .connectTimeout(3, TimeUnit.SECONDS)
            .readTimeout(3, TimeUnit.SECONDS)
            .writeTimeout(3, TimeUnit.SECONDS)
            .build()
        val url = "${Constants.BACKEND_URL}/api/health-check"
        val request = Request.Builder()
            .url(url)
            .build()

        try {
            client.newCall(request)
                .execute()
                .use {
                    return it.isSuccessful
                }
        } catch (e: IOException) {
            LoggingService.getInstance()
                .info("BackendService: No existing backend found: ${e.message}")
            return false
        } catch (e: Exception) {
            LoggingService.getInstance()
                .error("BackendService: failed to check backend status: ${e.message}")
        }

        return false
    }

    private fun downloadBackend(): Boolean {
        try {
            val outputFile =
                File(System.getProperty("user.home") + "/${Constants.APP_FOLDER_NAME}/${Constants.APP_BIN_FOLDER_NAME}/${Constants.BACKEND_ZIP_NAME}")
            if (!outputFile.exists()) {
                URL(Constants.BACKEND_DOWNLOAD_URL)
                    .openStream()
                    .use { input ->
                        FileOutputStream(outputFile)
                            .use { output ->
                                input.copyTo(output)
                            }
                    }
            }

            ZipInputStream(outputFile.inputStream())
                .use { zipInputStream ->
                    var entry = zipInputStream.nextEntry
                    while (entry != null) {
                        val file = File(
                            System.getProperty("user.home") + "/${Constants.APP_FOLDER_NAME}/${Constants.APP_BIN_FOLDER_NAME}/",
                            entry.name
                        )

                        if (entry.isDirectory) {
                            file.mkdirs()
                        } else {
                            file.parentFile.mkdirs()
                            FileOutputStream(file)
                                .use { output ->
                                    zipInputStream.copyTo(output)
                                }
                        }

                        zipInputStream.closeEntry()
                        entry = zipInputStream.nextEntry
                    }
                }

            return true
        } catch (e: Exception) {
            LoggingService.getInstance()
                .error("BackendService: failed to download backend: ${e.message}")
        }

        return false
    }

    private fun startBackend(): Boolean {
        try {
            val exeFilePath =
                System.getProperty("user.home") + "/${Constants.APP_FOLDER_NAME}/${Constants.APP_BIN_FOLDER_NAME}/${Constants.BACKEND_EXE_NAME}"
            val processBuilder = ProcessBuilder(exeFilePath, "--urls", Constants.BACKEND_URL)
                .directory(File(System.getProperty("user.home") + "/${Constants.APP_FOLDER_NAME}/${Constants.APP_BIN_FOLDER_NAME}/"))
                .apply {
                    environment()["ASPNETCORE_ENVIRONMENT"] = "Production"
                }

            val process = processBuilder.start()

            Thread {
                try {
                    val exitCode = process.waitFor()

                    if (exitCode != 0) {
                        LoggingService.getInstance()
                            .error("BackendService: failed to start backend: ExitCode=$exitCode")
                    }
                } catch (e: Exception) {
                    LoggingService.getInstance()
                        .error("BackendService: failed to waitFor backend: ${e.message}")
                }
            }.start()

            Thread.sleep(1000)

            return true
        } catch (e: Exception) {
            LoggingService.getInstance()
                .error("BackendService: failed to start backend: ${e.message}")
        }

        return false
    }
}
