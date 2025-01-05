package com.jetbrains.rider.plugins.coverme.services

import com.intellij.openapi.Disposable
import com.intellij.openapi.project.Project
import com.jetbrains.rider.plugins.coverme.models.ipc.abstractions.ProtocolMessage
import com.jetbrains.rider.plugins.coverme.web.AppBrowser
import java.awt.Component

class AppService : Disposable {
    companion object {
        private val instance = AppService()
        fun getInstance() = instance
    }

    private lateinit var _project: Project
    private lateinit var _pluginService: PluginService
    private val _backendService: BackendService = BackendService()
    private val _appBrowser: AppBrowser = AppBrowser()

    fun initialize(project: Project) {
        _project = project
        _pluginService = PluginService(_project)

        _backendService.createChannel()
    }

    override fun dispose() {
        _appBrowser.dispose()
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
}
