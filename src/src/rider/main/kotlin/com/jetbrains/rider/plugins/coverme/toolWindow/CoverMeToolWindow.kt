package com.jetbrains.rider.plugins.coverme.toolWindow

import java.awt.BorderLayout
import javax.swing.JLabel
import javax.swing.JPanel

class CoverMeToolWindow {
    val content: JPanel = JPanel(BorderLayout())

    init {
        content.add(JLabel("content here"), BorderLayout.CENTER)
    }
}