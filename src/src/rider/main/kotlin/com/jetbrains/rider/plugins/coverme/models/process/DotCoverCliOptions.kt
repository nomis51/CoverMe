package com.jetbrains.rider.plugins.coverme.models.process

import enums.process.DotCoverCliCommand
import enums.process.DotCoverCliReportType

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