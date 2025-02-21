package com.jetbrains.rider.plugins.coverme.ui.components

import com.intellij.openapi.ui.JBMenuItem
import com.intellij.openapi.ui.JBPopupMenu
import com.intellij.ui.JBColor
import com.intellij.util.ui.JBUI
import java.awt.*
import java.awt.event.MouseAdapter
import java.awt.event.MouseEvent
import javax.swing.Icon
import javax.swing.JButton

@Suppress("UseJBColor")
class SettingsMenuButton(
    icon: Icon,
    val activatedColor: JBColor? = null,
    private val hoverLightLevel: Int = 50
) :
    JButton(icon) {
    companion object {
        private val defaultActivatedColor: JBColor = JBColor(
            Color(
                0,
                0,
                0,
                80
            ),
            Color(
                0,
                0,
                0,
                80
            )
        )
    }

    private var _isHovered: Boolean = false
    private lateinit var _activatedColor: JBColor
    var isActivated: Boolean = false

    init {
        if (activatedColor == null) {
            _activatedColor = defaultActivatedColor
        }

        isContentAreaFilled = false
        isBorderPainted = false
        border = JBUI.Borders.empty(5)
        preferredSize = Dimension(
            30,
            30
        )
        minimumSize = preferredSize
        maximumSize = preferredSize


        addMouseListener(object : MouseAdapter() {
            override fun mouseEntered(e: MouseEvent?) {
                _isHovered = true
                repaint()
            }

            override fun mouseExited(e: MouseEvent?) {
                _isHovered = false
                repaint()
            }
        })

        val menu = JBPopupMenu()
        menu.add(
            JBMenuItem("Refesh")
                .apply {
                    addActionListener {
                        // TODO:
                    }
                }
        )

        addActionListener {
            menu.show(
                this,
                0,
                height
            )
        }
    }

    override fun paintComponent(g: Graphics?) {
        val g2 = g as Graphics2D

        g2.setRenderingHint(
            RenderingHints.KEY_ANTIALIASING,
            RenderingHints.VALUE_ANTIALIAS_ON
        )

        if (isActivated) {
            g2.color = _activatedColor
            g2.fillRoundRect(
                0,
                0,
                width,
                height,
                JBUI.scale(10),
                JBUI.scale(10)
            )
        }

        if (_isHovered) {
            g2.color = createTransparentColor(
                Color(
                    255,
                    255,
                    255,
                    hoverLightLevel
                ),
                Color(
                    200,
                    200,
                    200,
                    hoverLightLevel
                )
            )
            g2.fillRoundRect(
                0,
                0,
                width,
                height,
                JBUI.scale(10),
                JBUI.scale(10)
            )
        }

        super.paintComponent(g)
    }

    private fun createTransparentColor(
        light: Color,
        dark: Color
    ): Color {
        return object : JBColor(
            light,
            dark
        ) {
            override fun getRGB(): Int {
                return if (isBright()) light.rgb else dark.rgb
            }
        }
    }
}