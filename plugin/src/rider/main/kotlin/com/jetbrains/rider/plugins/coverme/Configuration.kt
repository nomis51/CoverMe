package com.jetbrains.rider.plugins.coverme

class Configuration {
    companion object {
        const val ENV = Environments.PRODUCTION

        private const val BACKEND_PORT = "5263"
        const val BACKEND_URL = "http://localhost:$BACKEND_PORT"
        const val BACKEND_ZIP_NAME = "CoverMe.Backend.zip"
        const val BACKEND_CHECKSUM_NAME = "CoverMe.Backend.checksum"
        const val BACKEND_EXE_NAME = "CoverMe.Backend.exe"
        const val BACKEND_RELEASES_URL = "https://api.github.com/repos/nomis51/CoverMe/releases"

        const val JS_NAMESPACE = "coverme"

        const val APP_FOLDER_NAME = ".coverme"
        const val APP_LOGS_FOLDER_NAME = "logs"
        const val APP_BIN_FOLDER_NAME = "bin"
        const val APP_LOCK_FILE_NAME = ".lock"
    }
}