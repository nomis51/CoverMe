package com.jetbrains.rider.plugins.coverme.ipc

import com.jetbrains.rider.plugins.coverme.services.LoggingService
import java.io.RandomAccessFile

class NamedPipe(name: String) {
    private val _pipe = RandomAccessFile(name, "rw")

    fun canRead(): Boolean {
        return _pipe.length() > 0
    }

    fun close() {
        if (!_pipe.channel.isOpen) return
        _pipe.close()
    }

    fun write(data: String) {
        try {
            _pipe.writeUTF("$data\n")
            _pipe.channel.force(true)
        } catch (e: Exception) {
            e.printStackTrace()
            LoggingService.getInstance()
                .error("NamedPipe: failed to write data: ${e.localizedMessage}")
        }
    }

    fun read(): String? {
        try {
            return _pipe.readLine()
        } catch (e: Exception) {
            e.printStackTrace()
            LoggingService.getInstance()
                .error("NamedPipe: failed to read data: ${e.localizedMessage}")
            return null
        }
    }
}
