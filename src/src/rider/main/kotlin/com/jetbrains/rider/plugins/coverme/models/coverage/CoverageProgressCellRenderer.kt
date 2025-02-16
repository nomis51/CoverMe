package com.jetbrains.rider.plugins.coverme.models.coverage

import com.intellij.ui.JBColor
import java.awt.Component
import java.awt.Graphics
import java.awt.Graphics2D
import java.awt.RenderingHints
import javax.swing.JLabel
import javax.swing.JTable
import javax.swing.table.DefaultTableCellRenderer

class CoverageProgressCellRenderer : DefaultTableCellRenderer() {
    private val margin = 4

    override fun getTableCellRendererComponent(
        table: JTable, value: Any?, isSelected: Boolean,
        hasFocus: Boolean, row: Int, column: Int
    ): Component {
        val label =
            super.getTableCellRendererComponent(table, value, isSelected, hasFocus, row, column) as JLabel
        label.text = value?.toString() ?: "0%"
        return label
    }

    override fun paintComponent(g: Graphics) {
        super.paintComponent(g)
        if (text.isNullOrEmpty()) return

        val g2d = g as Graphics2D
        g2d.setRenderingHint(RenderingHints.KEY_ANTIALIASING, RenderingHints.VALUE_ANTIALIAS_ON)

        val percentage = text.replace("%", "").toIntOrNull() ?: 0
        val barWidth = (width * percentage) / 100

        val barHeight = height - (margin * 2)
        val barY = margin

        val progressColor = when {
            percentage > 75 -> JBColor.GREEN
            percentage > 50 -> JBColor.ORANGE
            else -> JBColor.RED
        }

        g2d.color = progressColor
        g2d.fillRect(0, barY, barWidth, barHeight)

        g2d.color = JBColor.BLACK
        g2d.drawString(
            text,
            width / 2 - g2d.fontMetrics.stringWidth(text) / 2,
            height / 2 + g2d.fontMetrics.ascent / 2 - 2
        )
    }
}