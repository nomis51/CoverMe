package com.jetbrains.rider.plugins.coverme.lineMarkers

import com.intellij.codeInsight.daemon.LineMarkerInfo
import com.intellij.codeInsight.daemon.LineMarkerProvider
import com.intellij.openapi.editor.Document
import com.intellij.openapi.editor.markup.GutterIconRenderer
import com.intellij.openapi.vfs.VirtualFile
import com.intellij.psi.PsiDocumentManager
import com.intellij.psi.PsiElement
import com.intellij.ui.JBColor
import com.jetbrains.rider.plugins.coverme.models.ipc.GetFileLineCoverageRequest
import com.jetbrains.rider.plugins.coverme.models.ipc.abstractions.ProtocolMessage
import com.jetbrains.rider.plugins.coverme.models.ipc.abstractions.ProtocolMessageTypes
import com.jetbrains.rider.plugins.coverme.services.AppService
import kotlinx.serialization.json.Json
import kotlinx.serialization.serializer
import java.awt.BasicStroke
import java.awt.Component
import java.awt.Graphics
import java.awt.Graphics2D
import java.util.*
import javax.swing.Icon

class CoverageLineMarkerProvider : LineMarkerProvider {
    override fun getLineMarkerInfo(element: PsiElement): LineMarkerInfo<*>? {
        val file = element.containingFile ?: return null
        val project = file.project

        val document = PsiDocumentManager.getInstance(project)
            .getDocument(file)
            ?: return null

        val lineNumber = document.getLineNumber(element.textRange.startOffset)
        if (!isFirstTokenInLine(element, document, lineNumber)) return null

        val virtualFile: VirtualFile? = file.virtualFile
        val absoluteFilePath: String = virtualFile?.canonicalPath ?: return null
        if (!absoluteFilePath.endsWith(".cs")) return null

        val response = AppService.getInstance()
            .dispatchMessageToBackendAndWaitResponse(
                ProtocolMessage(
                    UUID.randomUUID().toString(),
                    ProtocolMessageTypes.GET_FILE_LINE_COVERAGE,
                    Json.encodeToString(
                        Json.serializersModule.serializer(),
                        GetFileLineCoverageRequest(
                            absoluteFilePath,
                            lineNumber + 1,
                            project.basePath!!
                        )
                    )
                )
            )

        if (response?.data == null || response.data == "null") return null
        val isCovered = Json.decodeFromString<Boolean>(response.data)

        return LineMarkerInfo<PsiElement>(
            element,
            element.textRange,
            VerticalLineIcon(isCovered),
            { "Line coverage" },
            null,
            GutterIconRenderer.Alignment.LEFT
        )
    }

    override fun collectSlowLineMarkers(
        elements: MutableList<out PsiElement>,
        result: MutableCollection<in LineMarkerInfo<*>>
    ) {

    }

    private fun isFirstTokenInLine(element: PsiElement, document: Document, lineNumber: Int): Boolean {
        val lineStartOffset: Int = document.getLineStartOffset(lineNumber)
        val firstElementInLine: PsiElement? = element.containingFile
            .findElementAt(lineStartOffset)

        return firstElementInLine === element
    }

    class VerticalLineIcon(covered: Boolean) : Icon {
        private val _covered = covered
        override fun getIconWidth(): Int = 5
        override fun getIconHeight(): Int = 16

        override fun paintIcon(c: Component?, g: Graphics, x: Int, y: Int) {
            dashedLine(g, x, y, if (_covered) JBColor.GREEN else JBColor.RED)
        }

        private fun dashedLine(g: Graphics, x: Int, y: Int, color: JBColor) {
            val g2d = g as? Graphics2D ?: return
            g2d.color = color

            val oldStroke = g2d.stroke

            val dashPattern = floatArrayOf(3.0f, 3.0f)
            g2d.stroke = BasicStroke(
                3.0f,
                BasicStroke.CAP_BUTT,
                BasicStroke.JOIN_MITER,
                10.0f,
                dashPattern,
                0.0f
            )

            g2d.drawLine(
                x + getIconWidth() / 2,
                y,
                x + getIconWidth() / 2,
                y + getIconHeight()
            )

            g2d.stroke = oldStroke
        }

        private fun plainLine(g: Graphics, x: Int, y: Int, color: JBColor) {
            val g2d = g as? Graphics2D ?: return
            g2d.color = color
            val oldStroke = g2d.stroke
            g2d.stroke = BasicStroke(3.0f)

            g2d.drawLine(
                x + getIconWidth() / 2,
                y,
                x + getIconWidth() / 2,
                y + getIconHeight()
            )

            g2d.stroke = oldStroke
        }
    }
}
