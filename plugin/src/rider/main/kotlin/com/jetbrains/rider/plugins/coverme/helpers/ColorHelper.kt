package com.jetbrains.rider.plugins.coverme.helpers

import java.awt.Color

fun Color.toHex(): String{
    return String.format("#%02x%02x%02x", this.red, this.green, this.blue)
}