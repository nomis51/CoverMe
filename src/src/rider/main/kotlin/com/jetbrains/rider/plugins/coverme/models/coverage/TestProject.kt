package com.jetbrains.rider.plugins.coverme.models.coverage

import java.io.File

class TestProject(val filePath: String) {
    fun getName(): String {
        return File(filePath).name
    }

    fun getFolderPath(): String {
        return File(filePath).parent
    }
}