package com.jetbrains.rider.plugins.coverme.actions

import com.intellij.openapi.actionSystem.AnAction
import com.intellij.openapi.actionSystem.AnActionEvent
import com.jetbrains.rider.plugins.coverme.services.AppService

class ReloadAppBrowserAction : AnAction() {
    override fun actionPerformed(e: AnActionEvent) {
       AppService.getInstance()
           .reloadAppBrowser()
    }
}