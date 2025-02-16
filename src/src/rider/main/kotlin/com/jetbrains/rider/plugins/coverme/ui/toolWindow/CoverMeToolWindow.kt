package com.jetbrains.rider.plugins.coverme.ui.toolWindow

import com.intellij.icons.AllIcons
import com.intellij.openapi.ui.ComboBox
import com.intellij.ui.components.JBTextField
import com.jetbrains.rider.plugins.coverme.models.coverage.CoverageData
import com.jetbrains.rider.plugins.coverme.ui.components.CoverageTable
import com.jetbrains.rider.plugins.coverme.ui.components.IconButton
import java.awt.BorderLayout
import java.awt.FlowLayout
import javax.swing.JPanel

class CoverMeToolWindow {
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

        val data = arrayOf(
            CoverageData("Circle(int)", 95, 3, 10, 0), CoverageData("Square(int)", 67, 18, 20, 1)
        )
        treeTable.updateData(data)
    }
}