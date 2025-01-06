package com.jetbrains.rider.plugins.coverme.helpers

import com.jetbrains.rider.plugins.coverme.Constants
import com.jetbrains.rider.plugins.coverme.models.github.Release
import com.jetbrains.rider.plugins.coverme.services.LoggingService
import kotlinx.serialization.json.Json
import okhttp3.OkHttpClient
import okhttp3.Request

class GithubHelper {
    companion object {
        fun getLatestBackendReleaseUrl(): String {
            try {
                val releases = getReleases()
            if (releases.isEmpty()) return ""

            return releases.maxByOrNull { it.published_at }!!
                .assets
                .first { it.name == Constants.BACKEND_ZIP_NAME }
                .browser_download_url
            } catch (e: Exception) {
                LoggingService.getInstance()
                    .error("GithubHelper: failed to get latest backend release url: ${e.message}")
            }

            return ""
        }

        private fun getReleases(): List<Release> {
            try {
                val client = OkHttpClient()
                val request = Request.Builder()
                    .url(Constants.BACKEND_RELEASES_URL)
                    .build()

                client.newCall(request)
                    .execute()
                    .use {
                        if (!it.isSuccessful) return emptyList()

                        val json = it.body!!.string()
                        val serializer = Json {
                            ignoreUnknownKeys = true
                        }
                        return serializer.decodeFromString<List<Release>>(json)
                    }
            } catch (e: Exception) {
                LoggingService.getInstance()
                    .error("GithubHelper: failed to get releases: ${e.message}")
            }

            return emptyList()
        }
    }
}