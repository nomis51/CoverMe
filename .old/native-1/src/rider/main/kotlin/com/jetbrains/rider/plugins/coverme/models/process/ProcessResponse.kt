package com.jetbrains.rider.plugins.coverme.models.process

class ProcessResponse(
    val exitCode: Int,
    val output: String,
    val error: String
)