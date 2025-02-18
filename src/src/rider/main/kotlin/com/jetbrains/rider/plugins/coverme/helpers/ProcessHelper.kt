package com.jetbrains.rider.plugins.coverme.helpers

import com.intellij.openapi.components.Service
import com.jetbrains.rdclient.util.idea.toIOFile
import com.jetbrains.rider.plugins.coverme.enums.process.DotCoverCliCommand
import com.jetbrains.rider.plugins.coverme.enums.process.DotCoverCliReportType
import com.jetbrains.rider.plugins.coverme.models.process.DotCoverCliOptions
import com.jetbrains.rider.plugins.coverme.models.process.ProcessResponse
import com.jetbrains.rider.plugins.coverme.models.process.ReportGeneratorOptions
import kotlinx.coroutines.sync.Semaphore
import java.io.File

@Service(Service.Level.PROJECT)
class ProcessHelper {
    private var _isDotCoverCliInstalled: Boolean = false
    private var _isReportGeneratorInstalled: Boolean = false
    private val _dotCoverCliInstallationLock = Semaphore(1)
    private val _reportGeneratorInstallationLock = Semaphore(1)

    fun dotCoverCli(options: DotCoverCliOptions): ProcessResponse {
        if (!_isDotCoverCliInstalled) {
            if (!ensureDotCoverCliInstalled()) {
                return ProcessResponse(1, "", "Failed to install dotCover CLI")
            }
        }

        val arguments = mutableListOf(
            "dotcover",
            when (options.command) {
                DotCoverCliCommand.COVER_DOTNET -> "cover-dotnet"
                else -> throw UnsupportedOperationException()
            },
            "--ReportType=${
                when (options.reportType) {
                    DotCoverCliReportType.HTML -> "HTML"
                    DotCoverCliReportType.DETAILED_XML -> "DetailedXML"
                    else -> throw UnsupportedOperationException()
                }
            }",
            "--Output=${options.outputPath}"
        )

        if (options.hideAutoProperties) {
            arguments.add("--HideAutoProperties")
        }

        arguments.add("--")
        arguments.add("dotnet")
        arguments.add("test")

        if (options.noBuild) {
            arguments.add("--no-build")
        }

        arguments.add("\"${options.projectFolderPath}\"")

        return execute(
            "dotnet",
            arguments.toTypedArray(),
            options.projectFolderPath
        )
    }

    fun reportGenerator(options: ReportGeneratorOptions): ProcessResponse {
        if (!_isReportGeneratorInstalled) {
            if (!ensureReportGeneratorInstalled()) {
                return ProcessResponse(1, "", "Failed to install report generator")
            }
        }

        val arguments = mutableListOf(
            "-reports:\"\"${options.reportFilePath}\"\"",
            "-targetdir:\"\"${options.outputFolderPath}\"\"",
        )

        return execute(
            "reportgenerator",
            arguments.toTypedArray(),
            File(options.reportFilePath).parent
        )
    }

    private fun ensureReportGeneratorInstalled(): Boolean {
        if (!_reportGeneratorInstallationLock.tryAcquire()) return false

        try {
            if (_isReportGeneratorInstalled) return true

            val response = execute(
                "dotnet",
                arrayOf(
                    "tool",
                    "install",
                    "--global",
                    "dotnet-reportgenerator-globaltool"
                ),
                "."
            )

            _isReportGeneratorInstalled = response.exitCode == 0
            return _isReportGeneratorInstalled
        } finally {
            _reportGeneratorInstallationLock.release()
        }
    }

    private fun ensureDotCoverCliInstalled(): Boolean {
        if (!_dotCoverCliInstallationLock.tryAcquire()) return false

        try {
            if (_isDotCoverCliInstalled) return true

            val response = execute(
                "dotnet",
                arrayOf(
                    "tool",
                    "install",
                    "--global",
                    "JetBrains.dotCover.CommandLineTools"
                ),
                "."
            )

            _isDotCoverCliInstalled = response.exitCode == 0
            return _isDotCoverCliInstalled
        } finally {
            _dotCoverCliInstallationLock.release()
        }
    }

    private fun execute(command: String, arguments: Array<String>, workingDirectory: String): ProcessResponse {
        val process = ProcessBuilder(command, *arguments).directory(workingDirectory.toIOFile())
            .redirectInput(ProcessBuilder.Redirect.INHERIT)
            .redirectError(ProcessBuilder.Redirect.INHERIT)
            .start()

        val outputLines: MutableList<String> = mutableListOf()
        process.inputStream.bufferedReader()
            .use {
                outputLines.addAll(it.readLines())
            }

        val errorLines: MutableList<String> = mutableListOf()
        process.errorStream.bufferedReader()
            .use {
                errorLines.addAll(it.readLines())
            }

        return ProcessResponse(
            process.exitValue(), outputLines.joinToString(), errorLines.joinToString()
        )
    }
}