package com.jetbrains.rider.plugins.coverme.ipc

import com.intellij.openapi.Disposable
import com.jetbrains.rider.plugins.coverme.models.ipc.abstractions.ProtocolMessage
import com.jetbrains.rider.plugins.coverme.services.LoggingService
import kotlinx.serialization.json.Json
import kotlinx.serialization.serializer
import java.util.*
import kotlin.concurrent.thread

abstract class IpcBase(pipeName: String) : Disposable {
    private lateinit var _messageReceivedCallback: (ProtocolMessage) -> Unit
    private val _pipeName = pipeName
    private lateinit var _pipe: NamedPipe
    private var _isConnected = false

    fun write(message: ProtocolMessage) {
        if (!_isConnected) return

        _pipe.write(message.toString())
    }

    override fun dispose() {
        disconnect()
    }

    protected fun initialize(messageReceivedCallback: (ProtocolMessage) -> Unit) {
        try {
            _messageReceivedCallback = messageReceivedCallback
            _pipe = NamedPipe(_pipeName)
            _isConnected = true
        } catch (e: Exception) {
            e.printStackTrace()
            LoggingService.getInstance()
                .error("IPC: failed to initialize pipe: ${e.localizedMessage}")
        }
    }

    private fun disconnect() {
        _isConnected = false
        _pipe.close()
    }

    protected fun listen() {
        thread(start = true) {
            while (_isConnected) {
                try {
                    val message = read() ?: continue
                    _messageReceivedCallback(message)

                    Thread.sleep(100)
                } catch (e: Exception) {
                    e.printStackTrace()
                    LoggingService.getInstance()
                        .error("IPC: failed to read message: ${e.localizedMessage}")
                }
            }
        }
    }

    private fun read(): ProtocolMessage? {
        if (!_isConnected || !_pipe.canRead()) return null

        val data = _pipe.read() ?: return null
        if (data.isEmpty()) return null

        try {
            val bytes = Base64.getDecoder().decode(data)
            val json = String(bytes)
            return Json.decodeFromString(Json.serializersModule.serializer(), json)
        } catch (e: Exception) {
            e.printStackTrace()
            LoggingService.getInstance()
                .error("IPC: failed to deserialize message: ${e.localizedMessage}")
            return null
        }
    }
}
