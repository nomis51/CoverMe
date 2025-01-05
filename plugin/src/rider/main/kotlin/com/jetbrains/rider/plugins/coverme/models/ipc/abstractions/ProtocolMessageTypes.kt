package com.jetbrains.rider.plugins.coverme.models.ipc.abstractions

class ProtocolMessageTypes {
    companion object {
        const val OPEN_FILE_AT_LINE = "openFileAtLine"
        const val GET_FILE_LINE_COVERAGE = "getFileLineCoverage"
        const val SAVE_REPORT = "saveReport"
    }
}
