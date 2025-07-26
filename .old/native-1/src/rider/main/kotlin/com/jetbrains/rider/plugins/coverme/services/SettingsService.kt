package com.jetbrains.rider.plugins.coverme.services

import com.intellij.openapi.components.Service
import com.jetbrains.rider.plugins.coverme.models.settings.CoverageSettings
import com.jetbrains.rider.plugins.coverme.models.settings.Settings
import kotlinx.serialization.encodeToString
import kotlinx.serialization.json.Json
import java.io.File

@Service(Service.Level.PROJECT)
class SettingsService {
    private val SETTINGS_FILE_NAME = "settings.json"

    fun saveSettings(settings: Settings) {
        val filePath = getFilePath()
        File(filePath).writeText(Json.encodeToString(settings))
    }

    fun getSettings(): Settings {
        val filePath = getFilePath()
        if (!File(filePath).exists()) return getDefaultSettings()

        val data = File(filePath).readText()
        return Json.decodeFromString<Settings>(data)
    }

    private fun getFilePath(): String {
        return "${System.getProperty("user.home")}/.coverme/$SETTINGS_FILE_NAME"
    }

    private fun getDefaultSettings(): Settings {
        return Settings(
            CoverageSettings(
                "*.Tests.csproj",
                "FullyQualifiedName~.Tests",
                "-:module=testhost;-:module=*.Tests",
                true
            )
        )
    }
}