package com.jetbrains.rider.plugins.coverme.ui.toolWindow

import com.intellij.icons.AllIcons
import com.intellij.openapi.ui.ComboBox
import com.intellij.ui.components.JBTextField
import com.jetbrains.rider.plugins.coverme.ui.components.IconButton
import java.awt.BorderLayout
import java.awt.FlowLayout
import javax.swing.JLabel
import javax.swing.JPanel

class CoverMeToolWindow {
    val content: JPanel = JPanel(BorderLayout())

    init {
        val topPanel = JPanel(BorderLayout())
        val toolbarPanel = JPanel(FlowLayout(FlowLayout.LEFT))
        val testProjectSelect = ComboBox<String>()
        toolbarPanel.add(testProjectSelect)

        val runCoverageButton = IconButton(AllIcons.RunConfigurations.TestState.Run)
            .apply {
                toolTipText = "Run Coverage"
            }
        toolbarPanel.add(runCoverageButton)

        topPanel.add(toolbarPanel, BorderLayout.NORTH)

        val txtFilter = JBTextField()
            .apply {
                emptyText.text = "Type to filter..."
            }
        topPanel.add(txtFilter, BorderLayout.CENTER)

        content.add(topPanel, BorderLayout.NORTH)

        val label = JLabel("content")
        content.add(label, BorderLayout.CENTER)
    }
}