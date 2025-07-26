package com.jetbrains.rider.plugins.coverme.models.coverage

import com.jetbrains.rider.plugins.coverme.enums.coverage.CoverageDataType

data class CoverageData(
    val symbol: String,
    val coverage: Int,
    val uncoveredLines: Int,
    val totalLines: Int,
    val type: CoverageDataType
) {
    var filePath: String = ""
    var lineNumber: Int = 1
    var level: Int = 0
}