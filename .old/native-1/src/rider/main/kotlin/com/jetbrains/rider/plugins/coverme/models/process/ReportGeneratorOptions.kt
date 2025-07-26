package com.jetbrains.rider.plugins.coverme.models.process

import com.jetbrains.rider.plugins.coverme.enums.process.ReportGeneratorReportType

class ReportGeneratorOptions(
    val reportFilePath: String,
    val outputFolderPath: String,
    val reportType: ReportGeneratorReportType
)