package com.jetbrains.rider.plugins.coverme.models.coverage

import com.intellij.ui.JBColor
import java.awt.*
import javax.swing.JLabel
import javax.swing.JTable
import javax.swing.table.DefaultTableCellRenderer

class CoverageProgressCellRenderer : DefaultTableCellRenderer() {
    private val margin = 4
    private var percentage: Int = 0

    override fun getTableCellRendererComponent(
        table: JTable,
        value: Any?,
        isSelected: Boolean,
        hasFocus: Boolean,
        row: Int,
        column: Int
    ): Component {
        val label = super.getTableCellRendererComponent(
            table,
            value,
            isSelected,
            hasFocus,
            row,
            column
        ) as JLabel;
        percentage = value?.toString()
            ?.replace(
                "%",
                ""
            )
            ?.toInt() ?: 0
        label.text = ""
        return label
    }

    override fun paintComponent(g: Graphics) {
        super.paintComponent(g)
        val g2d = g as Graphics2D
        g2d.setRenderingHint(
            RenderingHints.KEY_ANTIALIASING,
            RenderingHints.VALUE_ANTIALIAS_ON
        )

        val barWidth = (width * percentage) / 100
        val barHeight = height - (margin * 2)
        val barY = margin

        val progressColor = when {
            percentage > 75 -> JBColor.GREEN
            percentage > 50 -> JBColor.ORANGE
            else -> JBColor.RED
        }

        g2d.color = progressColor
        g2d.fillRect(
            0,
            barY,
            barWidth,
            barHeight
        )

        val text = "${percentage}%"
        val fontMetrics = g2d.fontMetrics
        val textWidth = fontMetrics.stringWidth(text)
        val textHeight = fontMetrics.ascent

        g2d.color = JBColor.BLACK
        g2d.font = font.deriveFont(Font.BOLD)
        g2d.drawString(
            text,
            (width - textWidth) / 2,
            barY + (barHeight + textHeight) / 2
        )
    }
}