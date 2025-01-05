package com.jetbrains.rider.plugins.coverme.models.ipc

import kotlinx.serialization.Serializable

@Serializable
data class GetFileLineCoverageRequest(
    val filePath: String,
    val line: Int,
    val projectRootPath: String
)
