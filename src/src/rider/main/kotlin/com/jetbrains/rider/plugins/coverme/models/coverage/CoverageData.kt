package com.jetbrains.rider.plugins.coverme.models.coverage

data class CoverageData(
    var symbol: String,
    var coverage: Int,
    var uncovered: Int
)