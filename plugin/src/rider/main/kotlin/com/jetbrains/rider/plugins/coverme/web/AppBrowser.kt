package com.jetbrains.rider.plugins.coverme.web

import com.intellij.openapi.Disposable
import com.intellij.openapi.application.ApplicationManager
import com.intellij.openapi.project.Project
import com.intellij.ui.jcef.JBCefApp
import com.intellij.ui.jcef.JBCefBrowser
import com.intellij.ui.jcef.JBCefBrowserBase
import com.intellij.ui.jcef.JBCefJSQuery
import com.jetbrains.rider.plugins.coverme.Configuration
import com.jetbrains.rider.plugins.coverme.Environments
import com.jetbrains.rider.plugins.coverme.helpers.toHex
import com.jetbrains.rider.plugins.coverme.models.ipc.abstractions.ProtocolMessage
import com.jetbrains.rider.plugins.coverme.services.LoggingService
import org.cef.browser.CefBrowser
import org.cef.browser.CefFrame
import org.cef.handler.CefLifeSpanHandlerAdapter
import org.cef.handler.CefLoadHandlerAdapter
import java.awt.Component
import javax.swing.JLabel
import javax.swing.UIManager


@Suppress("KotlinConstantConditions")
class AppBrowser : Disposable {
    private var _browser: JBCefBrowser? = null
    private lateinit var _jsQuery: JBCefJSQuery

    init {
        if (JBCefApp.isSupported()) {
            _browser = JBCefBrowser()
//            Disposer.register(this, _browser!!)

            _jsQuery = JBCefJSQuery.create(_browser as JBCefBrowserBase)

            if (Configuration.ENV != Environments.PRODUCTION) {
                _browser!!.jbCefClient.addLifeSpanHandler(object : CefLifeSpanHandlerAdapter() {
                    override fun onAfterCreated(browser: CefBrowser?) {
                        super.onAfterCreated(browser)
                        _browser!!.openDevtools()
                    }
                }, _browser!!.cefBrowser)
            }
        } else {
            LoggingService.getInstance().warn("AppBrowser: JCEF is not supported")
        }
    }

    fun reload() {
        _browser?.cefBrowser?.reload()
    }

    fun getComponent(): Component {
        if (_browser == null) {
            return JLabel("JCEF is not supported")
        }

        return _browser!!.component
    }

    override fun dispose() {
        _browser?.dispose()
    }

    fun sendMessage(message: ProtocolMessage) {
        try {
            _browser!!.cefBrowser.executeJavaScript(
                """
            window.${Configuration.JS_NAMESPACE}.receive("$message");
            """.trimIndent(), _browser!!.cefBrowser.url, 0
            )
        } catch (e: Exception) {
            e.printStackTrace()
            LoggingService.getInstance().error("AppBrowser: failed to send message: ${e.localizedMessage}")
        }
    }

    fun initialize(project: Project, channelId: String) {
        _browser!!.loadURL(Configuration.BACKEND_URL)
        addHandlers(project, channelId)
    }

    private fun addHandlers(project: Project, channelId: String) {
        _browser!!.jbCefClient.addLoadHandler(object : CefLoadHandlerAdapter() {
            override fun onLoadEnd(browser: CefBrowser?, frame: CefFrame?, httpStatusCode: Int) {
                super.onLoadEnd(browser, frame, httpStatusCode)
                injectProjectSettings(project, channelId)
            }
        }, _browser!!.cefBrowser)
    }

    private fun injectProjectSettings(project: Project, channelId: String) {
        ApplicationManager.getApplication().invokeLater {
            try {
                _jsQuery.let {
                    val panelBackground = UIManager.getColor("Panel.background")
                    val panelForeground = UIManager.getColor("Panel.foreground")
                    val editorBackground = UIManager.getColor("EditorPane.background")
                    val editorForeground = UIManager.getColor("EditorPane.foreground")
                    val textFieldBackground = UIManager.getColor("TextField.background")
                    val textFieldForeground = UIManager.getColor("TextField.foreground")
                    val accentColor = UIManager.getColor("Component.accentColor")

                    _browser!!.cefBrowser.executeJavaScript(
                        """
                        if(!window.intellij) {
                            window.intellij = {};
                        }
                        window.intellij.PROJECT_ROOT_PATH = "${project.basePath}";
                        window.intellij.CHANNEL_ID = "$channelId";
                        window.intellij.THEME = {
                            panel: {
                                background: "${panelBackground?.toHex() ?: ""}",
                                foreground: "${panelForeground?.toHex() ?: ""}"
                            },
                            button: {
                                background: "${UIManager.getColor("Button.background").toHex()}",
                                foreground: "${UIManager.getColor("Button.foreground").toHex()}"},
                            editor: {
                                background: "${editorBackground?.toHex() ?: ""}",
                                foreground: "${editorForeground?.toHex() ?: ""}"
                            },
                            textField: {
                                background: "${textFieldBackground?.toHex() ?: ""}",
                                foreground: "${textFieldForeground?.toHex() ?: ""}"
                            },
                            colors: {
                                accent: "${accentColor?.toHex() ?: ""}"
                            },
                        };
                     """.trimIndent(), _browser!!.cefBrowser.url, 0
                    )
                }
            } catch (e: Exception) {
                e.printStackTrace()
                LoggingService.getInstance()
                    .error("AppBrowser: failed to inject project root path: ${e.localizedMessage}")
            }
        }
    }
}
