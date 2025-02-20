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
import java.net.URLDecoder
import java.nio.charset.StandardCharsets
import java.nio.file.Files
import java.nio.file.Paths
import kotlin.io.path.getLastModifiedTime
import kotlin.io.path.name

@Service(Service.Level.PROJECT)
class CoverageService(private val _project: Project) {
    private val _settingsService = _project.service<SettingsService>()
    private val _processHelper = _project.service<ProcessHelper>()

    companion object {
        private val _regCleanType = Regex("\\b(?:\\w+\\.)+(\\w+)")
        private val _regMethodConstructor = Regex("\\.c*ctor")
    }

    fun getTestsProjects(): List<TestProject> {
        val settings = _settingsService.getSettings()
        val regex = Regex(
            settings.coverage.testsProjectFilter.replace(
                ".",
                "\\."
            )
                .replace(
                    "*",
                    ".*"
                )
        )

        return Files.walk(Paths.get(_project.basePath!!))
            .filter { it.toFile().isFile && it.fileName.name.matches(regex) }
            .map { TestProject(it.toString()) }
            .toList()
    }

    fun runCoverage(
        testProject: TestProject,
        options: CoverageOptions
    ): List<CoverageData> {
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

        return getCoverageTree(
            reportFilePath,
            options
        )
    }

    fun parseLastCoverage(options: CoverageOptions): List<CoverageData> {
        val filePath = getLastCoverageFilePath()
        if (filePath.isEmpty()) return emptyList()

        return getCoverageTree(
            filePath,
            options
        )
    }

    fun isLineCovered(
        filePath: String,
        lineNumber: Int
    ): Boolean {
// TODO: add caching
        return getLinesCoverage(
            filePath
        )[lineNumber] ?: false
    }

    private fun getLinesCoverage(windowsFilePath: String): Map<Int, Boolean?> {
        val lastCoverageFilePath = getLastCoverageFilePath()
        if (lastCoverageFilePath.isEmpty()) return emptyMap()

        val data = File(lastCoverageFilePath).readText()
        val xml = Jsoup.parse(
            data,
            org.jsoup.parser.Parser.xmlParser()
        )
        val root = xml.root()
            .select("root")
            .first()
        if (root == null) return emptyMap()

        val files = xml.root()
            .select("file")
        if (files.isEmpty()) return emptyMap()

        val statements = root.children()
            .filter { e ->
                e.tagName() == "statement"
            }
        if (statements.isEmpty()) return emptyMap()

        val lineStatus = mutableMapOf<Int, Boolean?>()

        statements.forEach { statement ->
            val fileIndex = statement.attr("FileIndex")
                .toInt()
            val file = files.first {
                it.attr("Index")
                    .toInt() == fileIndex
            }
            if (file == null) return@forEach

            val fileFilePath = file.attr("Name")
            if (fileFilePath.isNotEmpty()) return@forEach
            if (fileFilePath != windowsFilePath) return@forEach

            val statementLine = statement.attr("Line")
                .toInt()
            if (statementLine < 0) return@forEach

            val isCovered = statement.attr("Covered")
                .toBoolean()
            lineStatus[statementLine] = isCovered
        }

        return lineStatus.toMap()
    }

    private fun getLastCoverageFilePath(): String {
        val reportsFolder = "${_project.basePath}/.idea/coverme/reports"
        if (!File(reportsFolder).exists()) return ""

        val lastFile = Files.walk(File(reportsFolder).toPath())
            .filter {
                it.toFile().isFile
            }
            .sorted { a, b ->
                b.getLastModifiedTime()
                    .compareTo(a.getLastModifiedTime())
            }
            .findFirst()
        if (lastFile.isEmpty) return ""

        return lastFile.get()
            .toAbsolutePath()
            .toString()
    }

    private fun getCoverageTree(
        reportFilePath: String,
        options: CoverageOptions
    ): List<CoverageData> {
        // TODO: add some caching
        return parseCoverage(
            reportFilePath,
            options
        )
    }

    private fun parseCoverage(
        reportFilePath: String,
        options: CoverageOptions
    ): List<CoverageData> {
        val file = File(reportFilePath)
        if (!file.exists()) return emptyList()

        val data = file.readText()
        if (data.isEmpty()) return emptyList()

        val xml = Jsoup.parse(
            data,
            org.jsoup.parser.Parser.xmlParser()
        )
        val root = xml.root()
            .select("root")
            .first()
        if (root == null) return emptyList()

        val nodes: MutableList<CoverageData> = mutableListOf()
        nodes.add(
            CoverageData(
                "Solution",
                root.attr("CoveragePercent")
                    .toInt(),
                root.attr("CoveredStatements")
                    .toInt(),
                root.attr("TotalStatements")
                    .toInt(),
            )
        )

        val files = xml.root()
            .select("file")
        if (files.isEmpty()) return emptyList()

        val filesIndices = mutableMapOf<Int, String>()

        files.forEach {
            val index = it.attr("Index")
                .toInt()
            filesIndices[index] = it.attr("Name")
        }

        val assemblies = root.children()
            .filter { e ->
                e.tagName() == "Assembly"
            }

        assemblies.forEach { assembly ->
            parseAssembly(
                assembly,
                nodes,
                filesIndices,
                options
            )
        }

        return nodes.toList()
    }

    private fun parseAssembly(
        assembly: org.jsoup.nodes.Element,
        nodes: MutableList<CoverageData>,
        filesIndices: Map<Int, String>,
        options: CoverageOptions
    ) {
        val name = assembly.attr("Name")
        if (!Regex(options.filter).containsMatchIn(name)) return;

        val node = CoverageData(
            name,
            assembly.attr("CoveragePercent")
                .toInt(),
            assembly.attr("CoveredStatements")
                .toInt(),
            assembly.attr("TotalStatements")
                .toInt(),
        )
        node.level = 1
        nodes.add(node)

        val namespaces = assembly.children()
            .filter { e ->
                e.tagName() == "Namespace"
            }
        namespaces.forEach { namespace ->
            parseNamespace(
                name,
                namespace,
                nodes,
                filesIndices,
                options,
                2
            )
        }

        val types = assembly.children()
            .filter { e ->
                e.tagName() == "Type"
            }
        types.forEach { type ->
            parseType(
                type,
                nodes,
                filesIndices,
                options,
                2
            )
        }
    }

    private fun parseNamespace(
        assemblyName: String,
        namespace: org.jsoup.nodes.Element,
        nodes: MutableList<CoverageData>,
        filesIndices: Map<Int, String>,
        options: CoverageOptions,
        level: Int
    ) {
        val name = namespace.attr("Name")

        if (name != assemblyName) {
            if (!Regex(options.filter).containsMatchIn(name)) return;

            val node = CoverageData(
                name,
                namespace.attr("CoveragePercent")
                    .toInt(),
                namespace.attr("CoveredStatements")
                    .toInt(),
                namespace.attr("TotalStatements")
                    .toInt(),
            )
            node.level = level
            nodes.add(node)
        }

        val types = namespace.children()
            .filter { e ->
                e.tagName() == "Type"
            }
        types.forEach { type ->
            parseType(
                type,
                nodes,
                filesIndices,
                options,
                level + 1
            )
        }
    }

    private fun parseType(
        type: org.jsoup.nodes.Element,
        nodes: MutableList<CoverageData>,
        filesIndices: Map<Int, String>,
        options: CoverageOptions,
        level: Int
    ) {
        val name = type.attr("Name")
        if (!Regex(options.filter).containsMatchIn(name)) return;

        val node = CoverageData(
            name,
            type.attr("CoveragePercent")
                .toInt(),
            type.attr("CoveredStatements")
                .toInt(),
            type.attr("TotalStatements")
                .toInt(),
        )
        node.level = level
        nodes.add(node)

        val methods = type.children()
            .filter { e ->
                e.tagName() == "Method"
            }
        methods.forEach { method ->
            node.filePath = parseMethod(
                method,
                nodes,
                filesIndices,
                level + 1
            )
        }
    }

    private fun parseMethod(
        method: org.jsoup.nodes.Element,
        nodes: MutableList<CoverageData>,
        filesIndices: Map<Int, String>,
        level: Int
    ): String {
        val name = mapEncodedCharacters(method.attr("Name"))
        val methodName = parseMethodName(name)
        val arguments = parseArguments(name)
        val returnTypeIndex = name.lastIndexOf(":")
        val returnType = if (returnTypeIndex == -1) "" else _regCleanType
            .replace(
                name.substring(
                    returnTypeIndex
                ),
                "$1"
            )

        val firstStatement = method.children()
            .first {
                it.tagName() == "Statement"
            }

        var filePath = ""
        var lineNumber = 0

        if (firstStatement != null) {
            val fileIndex = firstStatement.attr("FileIndex")
                .toInt()
            filePath = filesIndices.get(fileIndex) ?: ""
            lineNumber = firstStatement.attr("Line")
                .toInt() - 1
        }

        val node = CoverageData(
            "$methodName($arguments)${if (returnType.isNotEmpty()) ": $returnType" else ""}",
            method.attr("CoveragePercent")
                .toInt(),
            method.attr("CoveredStatements")
                .toInt(),
            method.attr("TotalStatements")
                .toInt(),
        )
        node.level = level
        node.filePath = filePath
        node.lineNumber = lineNumber
        nodes.add(node)

        return filePath
    }

    private fun mapEncodedCharacters(name: String): String {
        return URLDecoder.decode(
            name,
            StandardCharsets.UTF_8
        )
    }

    private fun parseMethodName(name: String): String {
        val index = name.indexOf("(")
        val methodName = if (index == -1) name else name.substring(
            0,
            index
        )
        return _regMethodConstructor
            .replace(
                methodName,
                "ctor"
            )
    }

    private fun parseArguments(name: String): String {
        val openParenthesisIndex = name.indexOf("(")
        if (openParenthesisIndex < 0) return ""

        val closeParenthesisIndex = name.lastIndexOf(")")
        if (closeParenthesisIndex < 0 || closeParenthesisIndex <= openParenthesisIndex) return ""

        val arguments = name.substring(
            openParenthesisIndex + 1,
            closeParenthesisIndex
        )
            .split(",")

        return arguments.joinToString(", ") {
            _regCleanType.replace(
                it,
                "$1"
            )
        }
    }
}