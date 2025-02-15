package com.jetbrains.rider.plugins.coverme.services

import com.jetbrains.rider.plugins.coverme.Configuration
import com.jetbrains.rider.plugins.coverme.Environments
import com.jetbrains.rider.plugins.coverme.helpers.FileHelper
import com.jetbrains.rider.plugins.coverme.helpers.GithubHelper
import com.jetbrains.rider.plugins.coverme.ipc.IpcClient
import com.jetbrains.rider.plugins.coverme.models.ipc.abstractions.ProtocolMessage
import com.jetbrains.rider.plugins.coverme.models.ipc.abstractions.ProtocolMessageTypes
import com.jetbrains.rider.plugins.coverme.services.abstractions.IProtocolService
import okhttp3.OkHttpClient
import okhttp3.Request
import java.io.File
import java.io.IOException
import java.util.concurrent.Executors
import java.util.concurrent.TimeUnit

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
        val url = "${AppService.getInstance().getBackendUrl()}/api/channel"
        val request = Request.Builder().url(url).build()

        client.newCall(request)
            .execute()
            .use {
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

    fun sendMessageAndWaitResponse(message: ProtocolMessage): ProtocolMessage? {
        message.channelId = _channelId ?: return null

        if (Configuration.ENV == Environments.HEADLESS) {
            if (message.type == ProtocolMessageTypes.GET_FILE_LINE_COVERAGE) {
                return ProtocolMessage(message.id, message.type, "true")
            }
        }

        return _ipcClient?.writeMessageAndWaitResponse(message)
    }

    private fun ensureBackendStarted(retryCount: Int = 0): Boolean {
        if (Configuration.ENV != Environments.PRODUCTION) return true

        if (checkBackendStatus()) return true

        val ok = FileHelper.lockFileAndPerformOperation(
            "${System.getProperty("user.home")}/${Configuration.APP_FOLDER_NAME}/${Configuration.APP_LOCK_FILE_NAME}",
            operation = {

                if (checkBackendStatus()) return@lockFileAndPerformOperation true
                if (!downloadBackend()) return@lockFileAndPerformOperation false
                if (!startBackend()) return@lockFileAndPerformOperation false

                return@lockFileAndPerformOperation true
            },
            retryCount = 3
        )

        if (!ok) return false

        var count = 0
        while (!checkBackendStatus()) {
            Thread.sleep(1000)
            ++count

            if (count >= 10) {
                if (retryCount >= 3) {
                    LoggingService.getInstance()
                        .error("BackendService: failed to start backend after 3 reties over 30 seconds")
                    return false
                }

                return ensureBackendStarted(retryCount + 1)
            }
        }

        return true
    }

    private fun checkBackendStatus(): Boolean {
        val client = OkHttpClient
            .Builder()
            .connectTimeout(3, TimeUnit.SECONDS)
            .readTimeout(3, TimeUnit.SECONDS)
            .writeTimeout(3, TimeUnit.SECONDS)
            .build()
        val url = "${AppService.getInstance().getBackendUrl()}/api/health-check"
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
            val binFolder = File(
                System.getProperty("user.home") +
                        "/${Configuration.APP_FOLDER_NAME}/${Configuration.APP_BIN_FOLDER_NAME}/"
            )
            if (!binFolder.exists()) {
                binFolder.mkdirs()
            }

            val outputFile = File("${binFolder.absolutePath}/${Configuration.BACKEND_ZIP_NAME}")
            val fileSha = FileHelper.calculateFileSHA(outputFile)

            if (!outputFile.exists() || fileSha.isEmpty() || fileSha != GithubHelper.getLatestBackendChecksum()) {
                val backendUrl = GithubHelper.getLatestBackendReleaseUrl()
                if (backendUrl.isEmpty()) return false

                FileHelper.downloadFile(backendUrl, outputFile)
            }

            FileHelper.unzipFile(outputFile, File(binFolder.absolutePath))

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
                System.getProperty("user.home") + "/${Configuration.APP_FOLDER_NAME}/${Configuration.APP_BIN_FOLDER_NAME}/${Configuration.BACKEND_EXE_NAME}"
            val processBuilder = ProcessBuilder(exeFilePath, "--urls", AppService.getInstance().getBackendUrl())
                .directory(File(System.getProperty("user.home") + "/${Configuration.APP_FOLDER_NAME}/${Configuration.APP_BIN_FOLDER_NAME}/"))
                .apply {
                    environment()["ASPNETCORE_ENVIRONMENT"] = "Production"
                }

            val process = processBuilder.start()

            val executor = Executors.newFixedThreadPool(2)
            executor.submit { process.inputStream.bufferedReader().use { it.readLines() } }
            executor.submit { process.errorStream.bufferedReader().use { it.readLines() } }

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

            return true
        } catch (e: Exception) {
            LoggingService.getInstance()
                .error("BackendService: failed to start backend: ${e.message}")
        }

        return false
    }
}
