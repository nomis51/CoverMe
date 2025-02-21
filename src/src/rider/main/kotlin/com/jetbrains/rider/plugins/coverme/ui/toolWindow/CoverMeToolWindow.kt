package com.jetbrains.rider.plugins.coverme.ui.toolWindow

import com.intellij.icons.AllIcons
import com.intellij.openapi.application.ApplicationManager
import com.intellij.openapi.components.service
import com.intellij.openapi.progress.ProgressIndicator
import com.intellij.openapi.progress.ProgressManager
import com.intellij.openapi.progress.Task
import com.intellij.openapi.project.Project
import com.intellij.openapi.ui.ComboBox
import com.intellij.openapi.ui.JBMenuItem
import com.intellij.openapi.ui.JBPopupMenu
import com.intellij.ui.components.JBTextField
import com.intellij.util.ui.AsyncProcessIcon
import com.jetbrains.rider.plugins.coverme.models.coverage.CoverageOptions
import com.jetbrains.rider.plugins.coverme.models.coverage.TestProject
import com.jetbrains.rider.plugins.coverme.services.CoverageService
import com.jetbrains.rider.plugins.coverme.ui.components.CoverageTable
import com.jetbrains.rider.plugins.coverme.ui.components.IconButton
import com.jetbrains.rider.plugins.coverme.ui.components.SettingsMenuButton
import java.awt.BorderLayout
import java.awt.FlowLayout
import javax.swing.Box
import javax.swing.BoxLayout
import javax.swing.JPanel

class CoverMeToolWindow(private val _project: Project) {
    private val _coverageService: CoverageService = _project.service<CoverageService>()

    private val _spinner: AsyncProcessIcon = AsyncProcessIcon("coverme").apply {
        isVisible = false
    }
    private val _testProjects: MutableList<TestProject> = mutableListOf()
    private val _testProjectsComboBox: ComboBox<String> = ComboBox<String>()
    private val _coverageTable: CoverageTable = CoverageTable()
    private val _filterTextfield: JBTextField = JBTextField().apply {
        emptyText.text = "Type to filter..."
    }
    private val _runCoverageButton: IconButton = IconButton(AllIcons.RunConfigurations.TestState.Run).apply {
        toolTipText = "Run coverage"
        addActionListener {
            runCoverage(false)
        }
    }
    private val _buildAndRunCoverageButton: IconButton = IconButton(AllIcons.Run.Widget.Build).apply {
        toolTipText = "Build and run coverage"
        addActionListener {
            runCoverage(true)
        }
    }
    private val _settingsMenuButton: SettingsMenuButton = SettingsMenuButton(
        AllIcons.Actions.More,
        JBPopupMenu()
            .apply {
                add(JBMenuItem("Refresh").apply {
                    addActionListener {
                        refresh()
                    }
                })
                add(JBMenuItem("Save report").apply {
                    addActionListener {
                        saveReport(false)
                    }
                })
                add(JBMenuItem("Save detailed report").apply {
                    addActionListener {
                        saveReport(true)
                    }
                })
            }
    )

    val content: JPanel = JPanel(BorderLayout())

    init {
        val topPanel = JPanel(BorderLayout())
        val leftToolbarPanel = JPanel(
            FlowLayout(
                FlowLayout.LEFT,
                0,
                0
            )
        ).apply {
            add(_testProjectsComboBox)
            add(_runCoverageButton)
            add(_buildAndRunCoverageButton)
        }
        val rightToolbarPanel = JPanel(
            FlowLayout(
                FlowLayout.RIGHT,
                0,
                0
            )
        ).apply {
            add(_spinner)
            add(_settingsMenuButton)
        }
        val toolbarPanel = JPanel()
            .apply {
                layout = BoxLayout(
                    this,
                    BoxLayout.X_AXIS
                )
                add(leftToolbarPanel)
                add(Box.createHorizontalGlue())
                add(rightToolbarPanel)
            }

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
    }

    private fun refresh() {
        retrieveTestProjects()
        retrieveLastCoverage()
    }

    private fun saveReport(detailed: Boolean) {
        _runCoverageButton.isEnabled = false
        _buildAndRunCoverageButton.isEnabled = false
        _settingsMenuButton.isEnabled = false
        _spinner.isVisible = true

        ProgressManager.getInstance()
            .run(object : Task.Backgroundable(
                _project,
                "Generating code coverage report",
                true
            ) {
                override fun run(indicator: ProgressIndicator) {
                    try {
                        val testProject = _testProjects.firstOrNull {
                            it.getName() == _testProjectsComboBox.selectedItem
                        }
                        if (testProject == null) return

                        _coverageService.generateReport(
                            detailed,
                            testProject
                        )
                    } finally {
                        _runCoverageButton.isEnabled = true
                        _buildAndRunCoverageButton.isEnabled = true
                        _settingsMenuButton.isEnabled = true
                        _spinner.isVisible = false
                    }
                }
            })
    }


    private fun runCoverage(rebuild: Boolean) {
        _runCoverageButton.isEnabled = false
        _buildAndRunCoverageButton.isEnabled = false
        _settingsMenuButton.isEnabled = false
        _spinner.isVisible = true

        ProgressManager.getInstance()
            .run(object : Task.Backgroundable(
                _project,
                "Running code coverage",
                true
            ) {
                override fun run(indicator: ProgressIndicator) {
                    try {
                        val testProject = _testProjects.firstOrNull {
                            it.getName() == _testProjectsComboBox.selectedItem
                        }
                        if (testProject == null) return

                        val data = _coverageService.runCoverage(
                            testProject,
                            CoverageOptions(
                                rebuild,
                                _filterTextfield.text
                            )
                        )

                        ApplicationManager.getApplication()
                            .invokeLater {
                                _coverageTable.updateData(data.toTypedArray())
                            }
                    } finally {
                        _runCoverageButton.isEnabled = true
                        _buildAndRunCoverageButton.isEnabled = true
                        _settingsMenuButton.isEnabled = true
                        _spinner.isVisible = false
                    }
                }
            })
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

                        testProjects.forEach {
                            _testProjectsComboBox.addItem(it.getName())
                            _testProjects.add(it)
                        }

                        if (testProjects.isNotEmpty()) {
                            _testProjectsComboBox.selectedItem = _testProjectsComboBox.getItemAt(0)
                        }
                    }
            }
    }
}