package com.jetbrains.rider.plugins.coverme.ui.toolWindow

import com.intellij.icons.AllIcons
import com.intellij.openapi.ui.ComboBox
import com.intellij.ui.components.JBTextField
import com.intellij.ui.treeStructure.treetable.ListTreeTableModel
import com.intellij.ui.treeStructure.treetable.TreeTable
import com.intellij.ui.treeStructure.treetable.TreeTableModel
import com.intellij.util.ui.ColumnInfo
import com.jetbrains.rider.plugins.coverme.models.coverage.CoverageData
import com.jetbrains.rider.plugins.coverme.ui.components.IconButton
import com.jetbrains.rider.plugins.coverme.ui.models.coverage.CoverageTreeNode
import java.awt.BorderLayout
import java.awt.FlowLayout
import javax.swing.JPanel
import javax.swing.JScrollPane
import javax.swing.tree.DefaultMutableTreeNode

class CoverMeToolWindow {
    val content: JPanel = JPanel(BorderLayout())

    init {
        val topPanel = JPanel(BorderLayout())
        val toolbarPanel = JPanel(FlowLayout(FlowLayout.LEFT))
        val testProjectSelect = ComboBox<String>()
        toolbarPanel.add(testProjectSelect)

        val runCoverageButton = IconButton(AllIcons.RunConfigurations.TestState.Run)
            .apply {
                toolTipText = "Run Coverage"
            }
        toolbarPanel.add(runCoverageButton)

        topPanel.add(toolbarPanel, BorderLayout.NORTH)

        val txtFilter = JBTextField()
            .apply {
                emptyText.text = "Type to filter..."
            }
        topPanel.add(txtFilter, BorderLayout.CENTER)

        content.add(topPanel, BorderLayout.NORTH)

        // table
        val rootNode = CoverageTreeNode("Total", CoverageData("Total", 100, 13))

        val columnNames = arrayOf(
            object : ColumnInfo<Any, String>("Symbol") {
                override fun valueOf(o: Any): String {
                    return (o as CoverageTreeNode).data.symbol
                }
            },
            object : ColumnInfo<Any, String>("Coverage (%)") {
                override fun valueOf(o: Any): String {
                    return (o as CoverageTreeNode).data.coverage.toString()
                }
            },
            object : ColumnInfo<Any, String>("Uncovered") {
                override fun valueOf(o: Any): String {
                    return (o as CoverageTreeNode).data.uncovered.toString()
                }
            }
        )

        class CoverageTreeTableModel(rootNode: DefaultMutableTreeNode) :
            ListTreeTableModel(rootNode, columnNames) {
            override fun getColumnClass(column: Int): Class<*> {
                return if (column == 0) TreeTableModel::class.java else String::class.java
            }
        }

        val treeTableModel = CoverageTreeTableModel(rootNode)

        val treeTable = TreeTable(treeTableModel)
        treeTable.setShowColumns(true)
        treeTable.rowHeight = 25

        content.add(JScrollPane(treeTable), BorderLayout.CENTER)

        val data = arrayOf(
            CoverageData("Circle(int)", 95, 3),
            CoverageData("Square(int)", 67, 18)
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