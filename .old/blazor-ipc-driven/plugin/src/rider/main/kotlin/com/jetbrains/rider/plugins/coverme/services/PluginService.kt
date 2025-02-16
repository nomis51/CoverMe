package com.jetbrains.rider.plugins.coverme.services

import com.intellij.openapi.application.ApplicationManager
import com.intellij.openapi.editor.ScrollType
import com.intellij.openapi.fileChooser.FileChooser
import com.intellij.openapi.fileChooser.FileChooserDescriptor
import com.intellij.openapi.fileEditor.FileEditorManager
import com.intellij.openapi.fileEditor.TextEditor
import com.intellij.openapi.project.Project
import com.intellij.openapi.vfs.LocalFileSystem
import com.intellij.openapi.vfs.VirtualFile
import com.jetbrains.rider.plugins.coverme.helpers.FileHelper
import com.jetbrains.rider.plugins.coverme.models.ipc.OpenFileAtLineRequest
import com.jetbrains.rider.plugins.coverme.models.ipc.SaveReportRequest
import com.jetbrains.rider.plugins.coverme.models.ipc.abstractions.ProtocolMessage
import com.jetbrains.rider.plugins.coverme.models.ipc.abstractions.ProtocolMessageTypes
import com.jetbrains.rider.plugins.coverme.services.abstractions.IProtocolService
import kotlinx.serialization.json.Json
import java.io.File

class PluginService(project: Project) : IProtocolService {
    private val _project: Project = project

    override fun handleMessage(message: ProtocolMessage) {
        when (message.type) {
            ProtocolMessageTypes.OPEN_FILE_AT_LINE -> handleOpenFileAtLine(message.data)
            ProtocolMessageTypes.SAVE_REPORT -> handleSaveReport(message.data)
        }
    }

    private fun handleSaveReport(data: String?) {
        try {
            if (data == null) {
                LoggingService.getInstance().warn("Save report: data is null")
                return
            }

            val saveReportData = Json.decodeFromString<SaveReportRequest>(data)
            val reportFolder = File(saveReportData.reportFolder)
            if (!reportFolder.exists() || !reportFolder.isDirectory) {
                LoggingService.getInstance().warn("Save report: file does not exist: ${saveReportData.reportFolder}")
                return
            }

            val descriptor = FileChooserDescriptor(
                false,
                true,
                false,
                false,
                false,
                false,
            )
                .withTitle("Select Save Location")
                .withDescription("Choose the location to save the report")

            ApplicationManager.getApplication()
                .invokeLater {
                    val reportFolderPath = File(reportFolder.path).toPath()
                   
                    try {
                        val selectedFile: VirtualFile? = FileChooser.chooseFile(descriptor, _project, null)
                        if (selectedFile == null) {
                            LoggingService.getInstance()
                                .warn("Save report: no save location selected")
                            return@invokeLater
                        }

                        val targetFolderPath = File(selectedFile.path).toPath()

                        FileHelper.copyFolderRecursively(reportFolderPath, targetFolderPath)
                    } catch (e: Exception) {
                        LoggingService.getInstance()
                            .error("Failed to save report: ${e.localizedMessage}")
                    } finally {
                        FileHelper.deleteFolderRecursively(reportFolderPath)
                    }
                }
        } catch (e: Exception) {
            LoggingService.getInstance()
                .error("Failed to save report: ${e.localizedMessage}")
        }
    }

    private fun handleOpenFileAtLine(data: String?) {
        try {
            if (data == null) {
                LoggingService.getInstance().warn("Open file at line: data is null")
                return
            }

            val openFileAtLineData = Json.decodeFromString<OpenFileAtLineRequest>(data)
            if (!File(openFileAtLineData.filePath).exists()) {
                LoggingService.getInstance()
                    .warn("Open file at line: file does not exist: ${openFileAtLineData.filePath}")
                return
            }

            val virtualFile = LocalFileSystem.getInstance().findFileByPath(openFileAtLineData.filePath) ?: return
            val fileEditor = FileEditorManager.getInstance(_project).openFile(virtualFile, true).firstOrNull() ?: return

            if (fileEditor is TextEditor) {
                if (openFileAtLineData.line < 0 || fileEditor.editor.document.lineCount < openFileAtLineData.line) return

                val lineStartOffset = fileEditor.editor.document.getLineStartOffset(openFileAtLineData.line - 1)

                ApplicationManager.getApplication()
                    .invokeLaterOnWriteThread {
                        fileEditor.editor.caretModel.moveToOffset(lineStartOffset)
                        fileEditor.editor.scrollingModel.scrollToCaret(ScrollType.CENTER)
                    }
            } else {
                LoggingService.getInstance().warn("Open file at line: file editor is not text editor")
            }
        } catch (e: Exception) {
            LoggingService.getInstance()
                .error("Failed to open file at line: ${e.localizedMessage}")
        }
    }
}
