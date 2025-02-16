package com.jetbrains.rider.plugins.coverme.services

import com.intellij.openapi.components.Service
import com.intellij.openapi.project.Project
import com.jetbrains.rider.plugins.coverme.models.coverage.CoverageData

@Service(Service.Level.PROJECT)
class CoverageService(private val _project: Project) {
    fun runCoverage(): List<CoverageData> {
        // TODO: do some work
        return listOf(
            CoverageData("Project1", 45, 4, 15, 0),
            CoverageData("Feature1", 13, 2, 5, 1),
            CoverageData("Sum(int,int)", 58, 4, 13, 2),
            CoverageData("Substract(int,int)", 89, 4, 13, 2),
            CoverageData("Feature2", 54, 4, 13, 1),
            CoverageData("Divide(int,int)", 78, 4, 13, 2),
        )
    }
}