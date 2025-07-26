package com.jetbrains.rider.plugins.coverme.startupActivities

import com.intellij.openapi.project.Project
import com.intellij.openapi.startup.StartupActivity
import com.jetbrains.rider.plugins.coverme.services.AppService

class CoverMeStartupActivity : StartupActivity.Background {
    override fun runActivity(project: Project) {
        AppService.getInstance()
            .initialize(project)
    }
}