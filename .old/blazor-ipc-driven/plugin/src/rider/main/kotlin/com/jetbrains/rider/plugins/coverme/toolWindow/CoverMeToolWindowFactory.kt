package com.jetbrains.rider.plugins.coverme.toolWindow

import com.intellij.openapi.project.DumbAware
import com.intellij.openapi.project.Project
import com.intellij.openapi.wm.ToolWindow
import com.intellij.openapi.wm.ToolWindowFactory
import com.intellij.ui.content.ContentFactory
import com.jetbrains.rider.plugins.coverme.services.AppService

class CoverMeToolWindowFactory : ToolWindowFactory, DumbAware {
    override fun createToolWindowContent(project: Project, toolWindow: ToolWindow) {
        AppService.getInstance()
            .createAppBrowser()

        val window = CoverMeToolWindow()
        val content = ContentFactory.getInstance()
            .createContent(window.content, null, false)
        toolWindow.contentManager
            .addContent(content)
    }
}
