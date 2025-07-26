package com.jetbrains.rider.plugins.coverme.models.settings

import kotlinx.serialization.Serializable

@Serializable
data class CoverageSettings(
    var testsProjectFilter: String,
    var testsFilter: String,
    var coverageFilter: String,
    var hideAutoProperties: Boolean
)