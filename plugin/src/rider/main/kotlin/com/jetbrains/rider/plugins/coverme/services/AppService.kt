package com.jetbrains.rider.plugins.coverme.services

import com.intellij.openapi.Disposable
import com.intellij.openapi.project.Project
import com.jetbrains.rider.plugins.coverme.Configuration
import com.jetbrains.rider.plugins.coverme.models.Config
import com.jetbrains.rider.plugins.coverme.models.ipc.abstractions.ProtocolMessage
import com.jetbrains.rider.plugins.coverme.web.AppBrowser
import kotlinx.serialization.json.Json
import kotlinx.serialization.serializer
import java.awt.Component
import java.io.File
import java.io.ObjectInputFilter

class AppService : Disposable {
    companion object {
        private val instance = AppService()
        fun getInstance() = instance
    }

    private lateinit var _project: Project
    private lateinit var _pluginService: PluginService
    private lateinit var _backendService: BackendService
    private lateinit var _appBrowser: AppBrowser
    private lateinit var _config: Config

    fun initialize(project: Project) {
        _project = project
        readConfig()

        _pluginService = PluginService(_project)
        _backendService = BackendService()

        _backendService.createChannel()
    }

    fun createAppBrowser() {
        _appBrowser = AppBrowser()
    }

    override fun dispose() {
        _appBrowser.dispose()
    }

    fun reloadAppBrowser() {
        _appBrowser.reload();
    }

    fun getBrowserComponent(): Component {
        return _appBrowser.getComponent()
    }

    fun dispatchMessageToIntellij(message: ProtocolMessage) {
        _pluginService.handleMessage(message)
    }

    fun dispatchMessageToFrontend(message: ProtocolMessage) {
        _appBrowser.sendMessage(message)
    }

    fun dispatchMessageToBackend(message: ProtocolMessage) {
        _backendService.handleMessage(message)
    }

    fun dispatchMessageToBackendAndWaitResponse(message: ProtocolMessage): ProtocolMessage? {
        return _backendService.sendMessageAndWaitResponse(message)
    }

    fun initializeAppBrowser(channelId: String) {
        _appBrowser.initialize(_project, channelId)
    }

    fun getBackendUrl(): String {
        return Configuration.getBackendUrl(_config.backendPort)
    }

    private fun readConfig() {
        val filePath = "${System.getProperty("user.home")}/${Configuration.APP_FOLDER_NAME}/${Configuration.APP_CONFIG_FILE_NAME}"
        val file = File(filePath)
        if(!file.exists()) {
            _config = Config(Configuration.DEFAULT_BACKEND_PORT)
            val json = Json.encodeToString(Json.serializersModule.serializer(), _config)
            file.writeText(json)
            return
        }

        val data = file.readText()
        val config = Json.decodeFromString<Config>(data)
        _config = config
    }
}
