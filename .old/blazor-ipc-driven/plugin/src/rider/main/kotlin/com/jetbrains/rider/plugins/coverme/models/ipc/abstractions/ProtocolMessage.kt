package com.jetbrains.rider.plugins.coverme.models.ipc.abstractions

import com.fasterxml.jackson.annotation.JsonIgnore
import kotlinx.serialization.Serializable
import kotlinx.serialization.json.Json
import kotlinx.serialization.serializer
import java.util.*

@Serializable
data class ProtocolMessage(
    val id: String,
    val type: String,
    val data: String?
) {
    lateinit var channelId: String

    @JsonIgnore
    override fun toString(): String {
        val json = Json.encodeToString(Json.serializersModule.serializer(), this)
        return Base64.getEncoder().encodeToString(json.toByteArray())
    }
}
