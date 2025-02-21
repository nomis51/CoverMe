package com.jetbrains.rider.plugins.coverme.ui.toolWindow

import com.intellij.icons.AllIcons
import com.intellij.openapi.application.ApplicationManager
import com.intellij.openapi.components.service
import com.intellij.openapi.project.Project
import com.intellij.openapi.ui.ComboBox
import com.intellij.ui.components.JBTextField
import com.jetbrains.rider.plugins.coverme.models.coverage.CoverageOptions
import com.jetbrains.rider.plugins.coverme.services.CoverageService
import com.jetbrains.rider.plugins.coverme.ui.components.CoverageTable
import com.jetbrains.rider.plugins.coverme.ui.components.IconButton
import com.jetbrains.rider.plugins.coverme.ui.components.SettingsMenuButton
import java.awt.BorderLayout
import java.awt.FlowLayout
import javax.swing.JPanel

class CoverMeToolWindow(project: Project) {
    private val _coverageService: CoverageService = project.service<CoverageService>()

    private val _testProjectsComboBox: ComboBox<String> = ComboBox<String>()
    private val _coverageTable: CoverageTable = CoverageTable()
    private val _filterTextfield: JBTextField = JBTextField().apply {
        emptyText.text = "Type to filter..."
    }
    private val _runCoverageButton: IconButton = IconButton(AllIcons.RunConfigurations.TestState.Run).apply {
        toolTipText = "Run coverage"
    }
    private val _buildAndRunCoverageButton: IconButton = IconButton(AllIcons.Run.Widget.Build).apply {
        toolTipText = "Build and run coverage"
    }
    private val _settingsMenuButton: SettingsMenuButton = SettingsMenuButton(AllIcons.RunConfigurations.TestState.Red2)

    val content: JPanel = JPanel(BorderLayout())

    init {
        val topPanel = JPanel(BorderLayout())
        val toolbarPanel = JPanel(FlowLayout(FlowLayout.LEFT))
        toolbarPanel.add(_testProjectsComboBox)
        toolbarPanel.add(_runCoverageButton)
        toolbarPanel.add(_buildAndRunCoverageButton)
        toolbarPanel.add(_settingsMenuButton)

        topPanel.add(
            toolbarPanel,
            BorderLayout.NORTH
        )
        topPanel.add(
            _filterTextfield,
            BorderLayout.CENTER
        )

        content.add(
            topPanel,
            BorderLayout.NORTH
        )
        content.add(
            _coverageTable.getComponent(),
            BorderLayout.CENTER
        )

        retrieveTestProjects()
        retrieveLastCoverage()

//        val data = _coverageService.runCoverage(
//            TestProject("C:\\Users\\nomis51\\GitHub\\CoverMe\\samples\\TestApp.Tests\\TestApp.Tests.csproj"),
//            CoverageOptions(
//                false,
//                ""
//            )
//        )
//        val data = _coverageService.parseLastCoverage(
//            CoverageOptions(
//                false,
//                ""
//            )
//        )
//        treeTable.updateData(data.toTypedArray())
    }

    private fun retrieveLastCoverage() {
        ApplicationManager.getApplication()
            .executeOnPooledThread {
                val data = _coverageService.parseLastCoverage(
                    CoverageOptions(
                        false,
                        _filterTextfield.text
                    )
                )
                ApplicationManager.getApplication()
                    .invokeLater {
                        _coverageTable.updateData(data.toTypedArray())
                    }
            }
    }

    private fun retrieveTestProjects() {
        ApplicationManager.getApplication()
            .executeOnPooledThread {
                val testProjects = _coverageService.getTestsProjects()
                ApplicationManager.getApplication()
                    .invokeLater {
                        _testProjectsComboBox.removeAllItems()
                        testProjects.forEach { _testProjectsComboBox.addItem(it.getName()) }
                    }
            }
    }
}