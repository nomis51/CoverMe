package com.jetbrains.rider.plugins.coverme.ui.models.coverage

import com.jetbrains.rider.plugins.coverme.models.coverage.CoverageData
import javax.swing.tree.DefaultMutableTreeNode

class CoverageTreeNode(
    userObject: Any,
    val data: CoverageData
) : DefaultMutableTreeNode(userObject) {
}