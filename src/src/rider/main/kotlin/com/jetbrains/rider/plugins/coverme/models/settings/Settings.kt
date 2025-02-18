package com.jetbrains.rider.plugins.coverme.models.settings

import kotlinx.serialization.Serializable

@Serializable
data class Settings(
    var coverage: CoverageSettings
)