package com.ute.app_ban_do_cu.core.designsystem

import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.lightColorScheme
import androidx.compose.runtime.Composable
import androidx.compose.ui.graphics.Color
import androidx.compose.material3.Typography
import androidx.compose.material3.Shapes
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.ui.unit.dp
import androidx.compose.ui.text.ExperimentalTextApi
import androidx.compose.ui.text.PlatformTextStyle
import androidx.compose.ui.text.TextStyle
import androidx.compose.ui.text.font.Font
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.text.font.FontVariation
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.sp
import com.ute.app_ban_do_cu.R

object ReMarketPalette {
    val Primary = Color(0xFF28734F)
    val Text = Color(0xFF202E26)
    val Muted = Color(0xFF65746A)
    val Border = Color(0xFFDCE4DD)
    val Background = Color(0xFFFCFDFB)
    val Soft = Color(0xFFEAF3EC)
    val Warm = Color(0xFFFAEEE4)
    val Error = Color(0xFFAE413C)
    val Surface = Color.White
    val SurfaceMuted = Color(0xFFF3F7F3)
    val PrimaryDark = Color(0xFF1C5539)
    val Warning = Color(0xFF94602D)
    val ErrorSoft = Color(0xFFFCEDEA)
}

object ReMarketSpace {
    val Screen = 24.dp
    val Card = 16.dp
    val Gap = 12.dp
    val Section = 24.dp
    val FieldRadius = 14.dp
    val CardRadius = 20.dp
    val Touch = 48.dp
}

@OptIn(ExperimentalTextApi::class)
val ReMarketRoboto = FontFamily(*listOf(300, 400, 500, 600, 700).map { weight ->
    Font(R.font.roboto_variable, weight = FontWeight(weight),
        variationSettings = FontVariation.Settings(FontVariation.weight(weight)))
}.toTypedArray())

fun rmText(size: Int, weight: FontWeight = FontWeight.Normal, height: Float = 1.4f) = TextStyle(
    fontFamily = ReMarketRoboto, fontSize = size.sp, fontWeight = weight, lineHeight = (size * height).sp,
    letterSpacing = 0.sp, platformStyle = PlatformTextStyle(includeFontPadding = false)
)

private val ReMarketTypography = Typography(
    headlineLarge = rmText(28, FontWeight.Bold, 1.15f),
    headlineMedium = rmText(26, FontWeight.Bold), titleLarge = rmText(20, FontWeight.SemiBold, 1.2f),
    titleMedium = rmText(17, FontWeight.SemiBold), titleSmall = rmText(16, FontWeight.SemiBold),
    bodyLarge = rmText(15, height = 1.5f), bodyMedium = rmText(14, height = 1.45f), bodySmall = rmText(12, height = 1.5f),
    labelLarge = rmText(15, FontWeight.SemiBold, 1.25f), labelMedium = rmText(13, FontWeight.Medium, 1.3f),
    labelSmall = rmText(11, FontWeight.Medium)
)

private val ReMarketColors = lightColorScheme(
    primary = Color(0xFF28734F), onPrimary = Color.White,
    primaryContainer = ReMarketPalette.Soft, onPrimaryContainer = ReMarketPalette.Primary,
    secondary = ReMarketPalette.Primary, onSecondary = Color.White,
    secondaryContainer = ReMarketPalette.Soft, onSecondaryContainer = ReMarketPalette.Primary,
    surfaceVariant = ReMarketPalette.Soft, onSurfaceVariant = ReMarketPalette.Muted,
    surfaceContainer = Color.White, surfaceContainerLow = Color.White, surfaceContainerHigh = Color.White,
    surfaceContainerHighest = ReMarketPalette.Soft, surfaceContainerLowest = Color.White,
    surfaceTint = ReMarketPalette.Primary, outlineVariant = ReMarketPalette.Border,
    background = Color(0xFFFCFDFB), surface = Color.White,
    onBackground = Color(0xFF202E26), onSurface = Color(0xFF202E26),
    outline = Color(0xFFDCE4DD), error = Color(0xFFAE413C)
)

@Composable
fun ReMarketTheme(content: @Composable () -> Unit) {
    MaterialTheme(colorScheme = ReMarketColors, typography = ReMarketTypography,
        shapes = Shapes(extraSmall = RoundedCornerShape(8.dp), small = RoundedCornerShape(12.dp),
            medium = RoundedCornerShape(16.dp), large = RoundedCornerShape(20.dp), extraLarge = RoundedCornerShape(28.dp)), content = content)
}
