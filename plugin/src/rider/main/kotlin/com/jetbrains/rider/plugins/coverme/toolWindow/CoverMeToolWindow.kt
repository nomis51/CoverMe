package com.jetbrains.rider.plugins.coverme.toolWindow

import com.intellij.openapi.Disposable
import com.jetbrains.rider.plugins.coverme.services.AppService
import java.awt.BorderLayout
import javax.swing.JPanel

class CoverMeToolWindow : Disposable {
    val content: JPanel = JPanel(BorderLayout())

    init {
        content.add(
            AppService.getInstance()
                .getBrowserComponent(),
            BorderLayout.CENTER
        )
    }

    override fun dispose() {
        AppService.getInstance()
            .dispose()
    }
}
