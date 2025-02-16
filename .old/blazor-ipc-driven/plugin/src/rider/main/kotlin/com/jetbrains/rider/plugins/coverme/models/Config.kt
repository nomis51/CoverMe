package com.jetbrains.rider.plugins.coverme.models

import kotlinx.serialization.Serializable

@Serializable
data class Config(
    val backendPort: Int = 5263
)