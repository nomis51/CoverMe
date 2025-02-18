package com.jetbrains.rider.plugins.coverme.services

import com.intellij.openapi.components.Service
import com.intellij.openapi.components.service
import com.intellij.openapi.project.Project
import com.jetbrains.rd.util.UUID
import com.jetbrains.rider.plugins.coverme.enums.process.DotCoverCliCommand
import com.jetbrains.rider.plugins.coverme.enums.process.DotCoverCliReportType
import com.jetbrains.rider.plugins.coverme.helpers.ProcessHelper
import com.jetbrains.rider.plugins.coverme.models.coverage.CoverageData
import com.jetbrains.rider.plugins.coverme.models.coverage.CoverageOptions
import com.jetbrains.rider.plugins.coverme.models.coverage.TestProject
import com.jetbrains.rider.plugins.coverme.models.process.DotCoverCliOptions
import org.jsoup.Jsoup
import java.io.File
import java.nio.file.Files
import java.nio.file.Paths
import kotlin.io.path.name

@Service(Service.Level.PROJECT)
class CoverageService(private val _project: Project) {
    private val _settingsService = _project.service<SettingsService>()
    private val _processHelper = _project.service<ProcessHelper>()

    fun getTestsProjects(): List<TestProject> {
        val settings = _settingsService.getSettings()
        val regex = Regex(
            settings.coverage.testsProjectFilter.replace(".", "\\.")
                .replace("*", ".*")
        )

        return Files.walk(Paths.get(_project.basePath!!))
            .filter { it.toFile().isFile && it.fileName.name.matches(regex) }
            .map { TestProject(it.toString()) }
            .toList()
    }

    fun runCoverage(testProject: TestProject, options: CoverageOptions): List<CoverageData> {
        val outputFolder = "${_project.basePath}/.idea/coverme/reports"
        val fileOutputFolder = File(outputFolder)
        if (!fileOutputFolder.exists()) {
            fileOutputFolder.mkdirs()
        }

        val reportFilePath = "$outputFolder/${UUID.randomUUID()}.xml"
        val settings = _settingsService.getSettings()

        val response = _processHelper.dotCoverCli(
            DotCoverCliOptions(
                DotCoverCliCommand.COVER_DOTNET,
                DotCoverCliReportType.DETAILED_XML,
                reportFilePath,
                testProject.getFolderPath(),
                settings.coverage.hideAutoProperties,
                !options.rebuild,
                settings.coverage.coverageFilter,
                settings.coverage.testsFilter
            )
        )
        if (response.exitCode != 0) return emptyList()

        return getCoverageTree(reportFilePath, options)
    }

    private fun getCoverageTree(reportFilePath: String, options: CoverageOptions): List<CoverageData> {
        // TODO: add some caching
        return parseCoverage(reportFilePath, options)
    }

    private fun parseCoverage(reportFilePath: String, options: CoverageOptions): List<CoverageData> {
        val file = File(reportFilePath)
        if (!file.exists()) return emptyList()

        val data = file.readText()
        if (data.isEmpty()) return emptyList()

        val xml = Jsoup.parse(data, org.jsoup.parser.Parser.xmlParser())
        val root = xml.root()
            .select("root")
            .first()
        if (root == null) return emptyList()

        return emptyList()
    }

}