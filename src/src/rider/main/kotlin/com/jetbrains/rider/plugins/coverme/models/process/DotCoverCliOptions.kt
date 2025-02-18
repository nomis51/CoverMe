package com.jetbrains.rider.plugins.coverme.models.process

import com.jetbrains.rider.plugins.coverme.enums.process.DotCoverCliCommand
import com.jetbrains.rider.plugins.coverme.enums.process.DotCoverCliReportType

class DotCoverCliOptions(
    val command: DotCoverCliCommand,
    val reportType: DotCoverCliReportType,
    val outputPath: String,
    val projectFolderPath: String,
    val hideAutoProperties: Boolean,
    val noBuild: Boolean,
    val coverageFilter: String,
    val testsFilter: String
)