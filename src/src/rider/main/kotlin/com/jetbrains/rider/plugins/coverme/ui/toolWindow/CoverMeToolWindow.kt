package com.jetbrains.rider.plugins.coverme.ui.toolWindow

import com.intellij.icons.AllIcons
import com.intellij.openapi.components.service
import com.intellij.openapi.project.Project
import com.intellij.openapi.ui.ComboBox
import com.intellij.ui.components.JBTextField
import com.jetbrains.rider.plugins.coverme.services.CoverageService
import com.jetbrains.rider.plugins.coverme.ui.components.CoverageTable
import com.jetbrains.rider.plugins.coverme.ui.components.IconButton
import java.awt.BorderLayout
import java.awt.FlowLayout
import javax.swing.JPanel

class CoverMeToolWindow(project: Project) {
    private val _coverageService: CoverageService = project.service<CoverageService>()
    val content: JPanel = JPanel(BorderLayout())

    init {
        val topPanel = JPanel(BorderLayout())
        val toolbarPanel = JPanel(FlowLayout(FlowLayout.LEFT))
        val testProjectSelect = ComboBox<String>()
        toolbarPanel.add(testProjectSelect)

        val runCoverageButton = IconButton(AllIcons.RunConfigurations.TestState.Run).apply {
            toolTipText = "Run Coverage"
        }
        toolbarPanel.add(runCoverageButton)

        topPanel.add(toolbarPanel, BorderLayout.NORTH)

        val txtFilter = JBTextField().apply {
            emptyText.text = "Type to filter..."
        }
        topPanel.add(txtFilter, BorderLayout.CENTER)

        content.add(topPanel, BorderLayout.NORTH)

        // table
        val treeTable = CoverageTable()
        content.add(treeTable.getComponent(), BorderLayout.CENTER)

        val data = _coverageService.runCoverage()
        treeTable.updateData(data.toTypedArray())
    }
}