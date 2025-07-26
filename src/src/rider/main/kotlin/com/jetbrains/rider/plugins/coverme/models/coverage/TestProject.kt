package com.jetbrains.rider.plugins.coverme.models.coverage

import org.jsoup.Jsoup
import java.io.File

class TestProject {
    private var _filePath: String = ""

    constructor(filePath: String) {
        this._filePath = filePath
    }

    fun isVsTestPlatform(): Boolean {
        val xml = Jsoup.parse(
            File(this._filePath).readText(),
            org.jsoup.parser.Parser.xmlParser()
        )

        val itemGroups = xml.root()
            .select("ItemGroup")

        for (itemGroup in itemGroups) {
            val packageReferences = itemGroup.select("PackageReference")
            for (packageReference in packageReferences) {
                if (packageReference.attr("Include") == "TUnit") {
                    return true
                }
            }
        }
        // TODO: complete with xUnit 3+

        return false
    }

    fun getName(): String {
        return File(this._filePath).name
    }

    fun getFolderPath(): String {
        return File(this._filePath).parent
    }

    fun getTargetFramework(): String {
        val xml = Jsoup.parse(
            File(this._filePath).readText(),
            org.jsoup.parser.Parser.xmlParser()
        )
        val propertyGroup = xml.root()
            .select("PropertyGroup")
            .firstOrNull()
        if (propertyGroup == null) return ""

        val targetFramework = propertyGroup.select("TargetFramework").firstOrNull()
        if (targetFramework == null) return ""

        return targetFramework.text()

    }
}