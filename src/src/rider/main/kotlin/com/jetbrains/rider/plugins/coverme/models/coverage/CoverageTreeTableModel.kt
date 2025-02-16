package com.jetbrains.rider.plugins.coverme.models.coverage

import com.intellij.ui.treeStructure.treetable.ListTreeTableModel
import com.intellij.ui.treeStructure.treetable.TreeTableModel
import com.intellij.util.ui.ColumnInfo
import javax.swing.tree.DefaultMutableTreeNode

class CoverageTreeTableModel(rootNode: DefaultMutableTreeNode, columnNames: Array<ColumnInfo<Any, String>>) :
    ListTreeTableModel(rootNode, columnNames) {
    override fun getColumnClass(column: Int): Class<*> {
        return if (column == 0) TreeTableModel::class.java else String::class.java
    }
}