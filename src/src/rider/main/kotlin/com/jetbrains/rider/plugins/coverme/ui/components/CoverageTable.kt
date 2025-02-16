package com.jetbrains.rider.plugins.coverme.ui.components

import com.intellij.ui.components.JBScrollPane
import com.intellij.ui.treeStructure.treetable.TreeTable
import com.intellij.util.ui.ColumnInfo
import com.jetbrains.rider.plugins.coverme.models.coverage.CoverageData
import com.jetbrains.rider.plugins.coverme.models.coverage.CoverageProgressCellRenderer
import com.jetbrains.rider.plugins.coverme.models.coverage.CoverageTreeCellRenderer
import com.jetbrains.rider.plugins.coverme.models.coverage.CoverageTreeTableModel
import com.jetbrains.rider.plugins.coverme.ui.models.coverage.CoverageTreeNode
import java.awt.Component
import java.awt.event.MouseAdapter
import java.awt.event.MouseEvent
import javax.swing.JTree

class CoverageTable {
    private val _rootNode: CoverageTreeNode = CoverageTreeNode("Total", CoverageData("Total", 0, 0))
    private val _columns = arrayOf(
        object : ColumnInfo<Any, String>("Symbol") {
            override fun valueOf(o: Any): String {
                return (o as CoverageTreeNode).data.symbol
            }
        },
        object : ColumnInfo<Any, String>("Coverage (%)") {
            override fun valueOf(o: Any): String {
                return "${(o as CoverageTreeNode).data.coverage}%"
            }
        },
        object : ColumnInfo<Any, String>("Uncovered") {
            override fun valueOf(o: Any): String {
                return (o as CoverageTreeNode).data.uncovered.toString()
            }
        }
    )
    private val _treeTableModel: CoverageTreeTableModel = CoverageTreeTableModel(_rootNode, _columns)
    private val _treeTable: TreeTable = TreeTable(_treeTableModel)

    init {
        _treeTable.setShowColumns(true)
        _treeTable.rowHeight = 25
        _treeTable.tree.cellRenderer = CoverageTreeCellRenderer()
        _treeTable.columnModel.getColumn(1).cellRenderer = CoverageProgressCellRenderer()

        _treeTable.addMouseListener(object : MouseAdapter() {
            override fun mouseClicked(e: MouseEvent?) {
                if (e?.clickCount == 2) {
                    val row = _treeTable.rowAtPoint(e.point)
                    val column = _treeTable.columnAtPoint(e.point)

                    if (column == 0 && row != -1) {
                        val tree = _treeTable.tree as JTree
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
    }

    fun getComponent(): Component {
        return JBScrollPane(_treeTable)
    }

    fun updateData(data: Array<CoverageData>) {
        val nodes = mutableListOf<CoverageTreeNode>()
        data.forEach {
            val node = CoverageTreeNode(it.symbol, it)
            nodes.add(node)
        }

        _rootNode.removeAllChildren()
        nodes.forEachIndexed { index, node ->
            val parent = if (index == 0) _rootNode else nodes[index - 1]
            parent.add(node)
        }

        _treeTableModel.setRoot(_rootNode)
        _treeTableModel.reload()
    }
}