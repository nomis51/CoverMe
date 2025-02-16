package com.jetbrains.rider.plugins.coverme.ui.toolWindow

import com.intellij.icons.AllIcons
import com.intellij.openapi.ui.ComboBox
import com.intellij.ui.ColoredTreeCellRenderer
import com.intellij.ui.JBColor
import com.intellij.ui.SimpleTextAttributes
import com.intellij.ui.components.JBTextField
import com.intellij.ui.treeStructure.treetable.ListTreeTableModel
import com.intellij.ui.treeStructure.treetable.TreeTable
import com.intellij.ui.treeStructure.treetable.TreeTableModel
import com.intellij.util.ui.ColumnInfo
import com.jetbrains.rider.plugins.coverme.models.coverage.CoverageData
import com.jetbrains.rider.plugins.coverme.ui.components.IconButton
import com.jetbrains.rider.plugins.coverme.ui.models.coverage.CoverageTreeNode
import java.awt.*
import java.awt.event.MouseAdapter
import java.awt.event.MouseEvent
import javax.swing.*
import javax.swing.table.DefaultTableCellRenderer
import javax.swing.tree.DefaultMutableTreeNode

class CoverMeToolWindow {
    val content: JPanel = JPanel(BorderLayout())

    init {
        val topPanel = JPanel(BorderLayout())
        val toolbarPanel = JPanel(FlowLayout(FlowLayout.LEFT))
        val testProjectSelect = ComboBox<String>()
        toolbarPanel.add(testProjectSelect)

        val runCoverageButton = IconButton(AllIcons.RunConfigurations.TestState.Run).apply {
            toolTipText = "Run Coverage"
        }
        toolbarPanel.add(runCoverageButton)

        topPanel.add(toolbarPanel, BorderLayout.NORTH)

        val txtFilter = JBTextField().apply {
            emptyText.text = "Type to filter..."
        }
        topPanel.add(txtFilter, BorderLayout.CENTER)

        content.add(topPanel, BorderLayout.NORTH)

        // table
        val rootNode = CoverageTreeNode("Total", CoverageData("Total", 100, 13))

        val columnNames = arrayOf(object : ColumnInfo<Any, String>("Symbol") {
            override fun valueOf(o: Any): String {
                return (o as CoverageTreeNode).data.symbol
            }
        }, object : ColumnInfo<Any, String>("Coverage (%)") {
            override fun valueOf(o: Any): String {
                return "${(o as CoverageTreeNode).data.coverage}%"
            }
        }, object : ColumnInfo<Any, String>("Uncovered") {
            override fun valueOf(o: Any): String {
                return (o as CoverageTreeNode).data.uncovered.toString()
            }
        })

        class CoverageTreeCellRenderer : ColoredTreeCellRenderer() {
            override fun customizeCellRenderer(
                tree: JTree,
                value: Any?,
                selected: Boolean,
                expanded: Boolean,
                leaf: Boolean,
                row: Int,
                hasFocus: Boolean
            ) {
                if (value is CoverageTreeNode) {
                    icon = if (value.childCount > 0) AllIcons.Nodes.Folder else AllIcons.Nodes.FilePrivate
                    append(value.data.symbol, SimpleTextAttributes.REGULAR_ATTRIBUTES)
                }
            }
        }

        class CoverageProgressRenderer : DefaultTableCellRenderer() {
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

        class CoverageTreeTableModel(rootNode: DefaultMutableTreeNode) : ListTreeTableModel(rootNode, columnNames) {
            override fun getColumnClass(column: Int): Class<*> {
                return if (column == 0) TreeTableModel::class.java else String::class.java
            }
        }

        val treeTableModel = CoverageTreeTableModel(rootNode)

        val treeTable = TreeTable(treeTableModel)
        treeTable.setShowColumns(true)
        treeTable.rowHeight = 25
        treeTable.addMouseListener(object : MouseAdapter() {
            override fun mouseClicked(e: MouseEvent?) {
                if (e?.clickCount == 2) {
                    val row = treeTable.rowAtPoint(e.point)
                    val column = treeTable.columnAtPoint(e.point)

                    if (column == 0 && row != -1) {
                        val tree = treeTable.tree as JTree
                        val path = tree.getPathForRow(row)
                        val node = path?.lastPathComponent as? CoverageTreeNode

                        node?.let {
                            println("Double-clicked on: ${it.data.symbol}")
                            // TODO: handle click
                        }
                    }
                }
            }
        })

        (treeTable.tree as JTree).cellRenderer = CoverageTreeCellRenderer()
        treeTable.columnModel.getColumn(1).cellRenderer = CoverageProgressRenderer()

        content.add(JScrollPane(treeTable), BorderLayout.CENTER)

        val data = arrayOf(
            CoverageData("Circle(int)", 95, 3), CoverageData("Square(int)", 67, 18)
        )

        val nodes = mutableListOf<CoverageTreeNode>()
        data.forEach {
            val node = CoverageTreeNode(it.symbol, it)
            nodes.add(node)
        }

        nodes.forEachIndexed { index, node ->
            val parent = if (index == 0) rootNode else nodes[index - 1]
            parent.add(node)
        }

        treeTableModel.setRoot(rootNode)
        treeTableModel.reload()
    }
}