package com.jetbrains.rider.plugins.coverme.services.abstractions

import com.jetbrains.rider.plugins.coverme.models.ipc.abstractions.ProtocolMessage

interface IProtocolService {
    fun handleMessage(message: ProtocolMessage)
}
