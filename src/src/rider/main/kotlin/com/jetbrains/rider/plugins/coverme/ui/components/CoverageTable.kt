package com.jetbrains.rider.plugins.coverme.ui.components

import com.intellij.openapi.application.ApplicationManager
import com.intellij.openapi.editor.ScrollType
import com.intellij.openapi.fileEditor.FileEditorManager
import com.intellij.openapi.fileEditor.TextEditor
import com.intellij.openapi.project.Project
import com.intellij.openapi.vfs.LocalFileSystem
import com.intellij.ui.components.JBScrollPane
import com.intellij.ui.treeStructure.treetable.TreeTable
import com.intellij.util.ui.ColumnInfo
import com.jetbrains.rider.plugins.coverme.enums.coverage.CoverageDataType
import com.jetbrains.rider.plugins.coverme.models.coverage.CoverageData
import com.jetbrains.rider.plugins.coverme.models.coverage.CoverageProgressCellRenderer
import com.jetbrains.rider.plugins.coverme.models.coverage.CoverageTreeCellRenderer
import com.jetbrains.rider.plugins.coverme.models.coverage.CoverageTreeTableModel
import com.jetbrains.rider.plugins.coverme.ui.models.coverage.CoverageTreeNode
import java.awt.Component
import java.awt.event.MouseAdapter
import java.awt.event.MouseEvent
import javax.swing.JTree


class CoverageTable(val _project: Project) {
    private val _rootNode: CoverageTreeNode = CoverageTreeNode(
        "Solution",
        CoverageData(
            "Solution",
            0,
            0,
            0,
            CoverageDataType.SOLUTION
        )
    )
    private val _columns = arrayOf(
        object : ColumnInfo<Any, String>("Symbol") {
            override fun valueOf(o: Any): String {
                return (o as CoverageTreeNode).data.symbol
            }
        },
        object : ColumnInfo<Any, String>("Coverage") {
            override fun valueOf(o: Any): String {
                return "${(o as CoverageTreeNode).data.coverage}%"
            }
        },
        object : ColumnInfo<Any, String>("Statements") {
            override fun valueOf(o: Any): String {
                val node = o as CoverageTreeNode
                return "${node.data.uncoveredLines}/${node.data.totalLines}"
            }
        })
    private val _treeTableModel: CoverageTreeTableModel = CoverageTreeTableModel(
        _rootNode,
        _columns
    )
    private val _treeTable: TreeTable = TreeTable(_treeTableModel)

    init {
        _treeTable.setShowColumns(true)
        _treeTable.rowHeight = 25
        _treeTable.tree.cellRenderer = CoverageTreeCellRenderer()
        _treeTable.columnModel.getColumn(1).cellRenderer = CoverageProgressCellRenderer()
        _treeTable.columnModel.getColumn(0).preferredWidth = 250
        _treeTable.columnModel.getColumn(1).preferredWidth = 40
        _treeTable.columnModel.getColumn(2).preferredWidth = 40

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
                            openFile(
                                it.data.filePath,
                                it.data.lineNumber
                            )
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
            val node = CoverageTreeNode(
                it.symbol,
                it
            )
            nodes.add(node)
        }

        _rootNode.removeAllChildren()

        val levelNodes = mutableListOf<CoverageTreeNode>()
        _rootNode.data = nodes[0].data

        nodes.drop(1)
            .forEach { node ->
                val level = node.data.level - 1

                if (level == 0) {
                    _rootNode.add(node)
                } else {
                    val parent = levelNodes[level - 1]
                    parent.add(node)
                }

                if (level >= levelNodes.size) {
                    levelNodes.add(node)
                } else {
                    levelNodes[level] = node
                }
            }

        _treeTableModel.setRoot(_rootNode)
        _treeTableModel.reload()
    }

    private fun openFile(
        filePath: String,
        line: Int
    ) {
        try {
            val virtualFile = LocalFileSystem.getInstance()
                .findFileByPath(filePath) ?: return
            val fileEditor = FileEditorManager.getInstance(_project)
                .openFile(
                    virtualFile,
                    true
                )
                .firstOrNull() ?: return

            if (fileEditor is TextEditor) {
                if (line < 0 || fileEditor.editor.document.lineCount < line) return

                val lineStartOffset = fileEditor.editor.document.getLineStartOffset(line - 1)

                ApplicationManager.getApplication()
                    .invokeLaterOnWriteThread {
                        fileEditor.editor.caretModel.moveToOffset(lineStartOffset)
                        fileEditor.editor.scrollingModel.scrollToCaret(ScrollType.CENTER)
                    }
            } else {

            }
        } catch (e: Exception) {

        }
    }
}