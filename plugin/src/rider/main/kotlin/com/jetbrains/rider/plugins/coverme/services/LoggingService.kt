package com.jetbrains.rider.plugins.coverme.services

import com.jetbrains.rider.plugins.coverme.Constants
import java.io.File
import java.time.LocalDateTime
import java.util.logging.FileHandler
import java.util.logging.Logger
import java.util.logging.SimpleFormatter

class LoggingService {
    companion object {
        private val _instance = LoggingService()

        fun getInstance() = _instance
    }

    private val _logger = Logger.getLogger("CoverMe")
    private lateinit var _fileHandler: FileHandler

    init {
        try {
            val time =
                LocalDateTime.now()
                    .toString()
                    .replace("-", "")
                    .replace(":", "")
                    .replace(".", "")
                    .replace("T", "")
                    .substring(0, 14)

            val dir =
                "${System.getProperty("user.home")}/${Constants.APP_FOLDER_NAME}/${Constants.APP_LOGS_FOLDER_NAME}"
            if (!File(dir).exists()) {
                File(dir).mkdir()
            }

            val filePath = "$dir/intellij-$time.txt"
            if (!File(filePath).exists()) {
                File(filePath).createNewFile()
            }

            _fileHandler = FileHandler(filePath, true)
            _fileHandler.formatter = SimpleFormatter()
            _logger.addHandler(_fileHandler)

            info("LoggingService: initialized")
        } catch (e: Exception) {
            e.printStackTrace()
        }
    }

    fun info(message: String) = _logger.info(message)
    fun warn(message: String) = _logger.warning(message)
    fun error(message: String) = _logger.severe(message)
}
