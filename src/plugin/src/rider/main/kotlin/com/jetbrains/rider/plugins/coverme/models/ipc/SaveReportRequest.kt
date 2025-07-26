package com.jetbrains.rider.plugins.coverme.models.ipc

import kotlinx.serialization.Serializable

@Serializable
data class SaveReportRequest(
    val reportFolder: String
)