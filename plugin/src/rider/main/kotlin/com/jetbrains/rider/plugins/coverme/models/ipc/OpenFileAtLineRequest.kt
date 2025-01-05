package com.jetbrains.rider.plugins.coverme.models.ipc

import com.intellij.openapi.project.Project
import kotlinx.serialization.Serializable

@Serializable
data class OpenFileAtLineRequest(
    val filePath: String,
    val line: Int
) {
    fun getRelativePath(project: Project): String {
        return filePath.replace(project.basePath!!, "")
    }
}
