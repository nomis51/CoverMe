package com.jetbrains.rider.plugins.coverme.models.coverage

data class CoverageData(
    val symbol: String,
    val coverage: Int,
    val uncoveredLines: Int,
    val totalLines: Int,
    val level: Int
)