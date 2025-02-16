package com.jetbrains.rider.plugins.coverme.models.coverage

import com.intellij.icons.AllIcons
import com.intellij.ui.ColoredTreeCellRenderer
import com.intellij.ui.SimpleTextAttributes
import com.jetbrains.rider.plugins.coverme.ui.models.coverage.CoverageTreeNode
import javax.swing.JTree

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
