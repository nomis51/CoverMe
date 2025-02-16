package com.jetbrains.rider.plugins.coverme.ipc

import com.jetbrains.rider.plugins.coverme.models.ipc.abstractions.ProtocolMessage

class IpcClient(pipeName: String, messageReceivedCallback: (ProtocolMessage) -> Unit) : IpcBase(pipeName) {
    private val _messageReceivedCallback: (ProtocolMessage) -> Unit = messageReceivedCallback
    private var _waitingForResponseOfType: String? = null
    private var _responseOfType: ProtocolMessage? = null

    init {
        super.initialize(::handleMessage)
        super.listen()
    }

    private fun handleMessage(message: ProtocolMessage) {
        if (message.type == _waitingForResponseOfType) {
            _waitingForResponseOfType = null
            _responseOfType = message
            return
        }

        _messageReceivedCallback(message)
    }

    fun writeMessage(message: ProtocolMessage) {
        super.write(message)
    }

    fun writeMessageAndWaitResponse(message: ProtocolMessage): ProtocolMessage? {
        _waitingForResponseOfType = message.type
        super.write(message)

        while (_responseOfType == null || _responseOfType!!.id != message.id) {
            Thread.sleep(10)
        }

        return _responseOfType
    }
}
