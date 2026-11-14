package com.ute.app_ban_do_cu.core.designsystem

import androidx.annotation.DrawableRes
import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.text.BasicTextField
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.foundation.text.KeyboardActions
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.SolidColor
import androidx.compose.ui.platform.testTag
import androidx.compose.ui.platform.LocalFocusManager
import androidx.compose.ui.platform.LocalSoftwareKeyboardController
import androidx.compose.ui.focus.FocusDirection
import androidx.compose.ui.focus.onFocusChanged
import androidx.compose.ui.res.painterResource
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.text.input.ImeAction
import androidx.compose.ui.semantics.semantics
import androidx.compose.ui.semantics.contentDescription
import androidx.compose.ui.text.input.PasswordVisualTransformation
import androidx.compose.ui.text.input.VisualTransformation
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.Dp
import androidx.compose.ui.unit.dp
import com.ute.app_ban_do_cu.R

@Composable
fun RmIcon(@DrawableRes id: Int, modifier: Modifier = Modifier, size: Dp = 20.dp,
    color: Color = ReMarketPalette.Primary, description: String? = null) {
    Icon(painterResource(id), description, modifier.size(size), tint = color)
}

@Composable
fun RmButton(text: String, onClick: () -> Unit, modifier: Modifier = Modifier, enabled: Boolean = true,
    loading: Boolean = false, outlined: Boolean = false, @DrawableRes icon: Int? = null, heightDp:Int=52) {
    val colors = ButtonDefaults.buttonColors(containerColor = if (outlined) Color.Transparent else MaterialTheme.colorScheme.primary,
        contentColor = if (outlined) MaterialTheme.colorScheme.primary else Color.White,
        disabledContainerColor = ReMarketPalette.Border.copy(alpha = .65f), disabledContentColor = ReMarketPalette.Muted)
    Button(onClick, modifier.fillMaxWidth().heightIn(min = heightDp.dp).then(if (outlined) Modifier.border(1.dp, ReMarketPalette.Border, CircleShape) else Modifier),
        enabled = enabled && !loading, shape = CircleShape, colors = colors,
        elevation = ButtonDefaults.buttonElevation(0.dp, 0.dp), contentPadding = PaddingValues(horizontal = 16.dp, vertical = 13.dp)) {
        if (loading) CircularProgressIndicator(Modifier.size(20.dp), color = LocalContentColor.current, strokeWidth = 2.dp)
        else {
            if (icon != null) { RmIcon(icon, size = 19.dp, color = LocalContentColor.current); Spacer(Modifier.width(8.dp)) }
            Text(text, style = MaterialTheme.typography.labelLarge, textAlign = TextAlign.Center)
        }
    }
}

@Composable
fun RmField(label: String, value: String, change: (String) -> Unit, placeholder: String,
    tag: String, password: Boolean = false, keyboard: KeyboardType = KeyboardType.Text,
    enabled: Boolean = true, multiline: Boolean = false, minimumHeight: Int = 52, trailing: (@Composable () -> Unit)? = null,
    isError: Boolean = false, maxLines: Int = Int.MAX_VALUE, cornerRadius: Dp = ReMarketSpace.FieldRadius) {
    var visible by remember { mutableStateOf(false) }
    var focused by remember { mutableStateOf(false) }
    val focus = LocalFocusManager.current
    val keyboardController = LocalSoftwareKeyboardController.current
    val shape = RoundedCornerShape(cornerRadius)
    val borderColor = when { isError -> ReMarketPalette.Error; focused -> MaterialTheme.colorScheme.primary; else -> MaterialTheme.colorScheme.outline }
    Column(verticalArrangement = Arrangement.spacedBy(8.dp)) {
        if(label.isNotEmpty())Text(label, style = MaterialTheme.typography.labelMedium, color = MaterialTheme.colorScheme.onSurface)
        BasicTextField(value, onValueChange = change, enabled = enabled, singleLine = !multiline, maxLines = if (multiline) maxLines else 1,
            textStyle = rmText(14, height = 1.35f).copy(color = MaterialTheme.colorScheme.onSurface),
            cursorBrush = SolidColor(MaterialTheme.colorScheme.primary),
            keyboardOptions = KeyboardOptions(keyboardType = if (password) KeyboardType.Password else keyboard,
                autoCorrectEnabled = false, imeAction = if (multiline) ImeAction.Default else ImeAction.Next),
            keyboardActions = KeyboardActions(onNext = { focus.moveFocus(FocusDirection.Next) },
                onDone = { focus.clearFocus(); keyboardController?.hide() }),
            visualTransformation = if (password && !visible) PasswordVisualTransformation() else VisualTransformation.None,
            modifier = Modifier.fillMaxWidth().testTag(tag).onFocusChanged { focused = it.isFocused }
                .semantics { if (label.isNotBlank()) contentDescription = label }
                .background(if (enabled) Color.White else ReMarketPalette.SurfaceMuted, shape)
                .border(if (focused || isError) 1.5.dp else 1.dp, borderColor, shape),
            decorationBox = { inner ->
                Row(Modifier.heightIn(min = minimumHeight.dp).padding(horizontal = 14.dp, vertical = if (password || trailing != null) 2.dp else 15.dp),
                    verticalAlignment = if (multiline) Alignment.Top else Alignment.CenterVertically) {
                    Box(Modifier.weight(1f)) {
                        if (value.isEmpty()) Text(placeholder, style = rmText(14, height = 1.35f), color = MaterialTheme.colorScheme.onSurfaceVariant)
                        inner()
                    }
                    trailing?.invoke()
                    if (password) IconButton(onClick = { visible = !visible }, enabled = enabled,
                        modifier = Modifier.size(48.dp)) {
                        RmIcon(if (visible) R.drawable.ic_rm_eye else R.drawable.ic_rm_eye_off,
                            color = ReMarketPalette.Muted, description = if (visible) "Ẩn mật khẩu" else "Hiện mật khẩu")
                    }
                }
            })
    }
}

@Composable
fun RmNotice(text: String, modifier: Modifier = Modifier, warning: Boolean = false) {
    Row(modifier.fillMaxWidth().background(if (warning) ReMarketPalette.Warm else ReMarketPalette.Soft, RoundedCornerShape(16.dp)).padding(16.dp),
        horizontalArrangement = Arrangement.spacedBy(10.dp)) {
        RmIcon(if (warning) R.drawable.ic_rm_leaf else R.drawable.ic_rm_info, color = if (warning) ReMarketPalette.Warning else ReMarketPalette.Primary)
        Text(text, style = MaterialTheme.typography.bodySmall, color = if (warning) ReMarketPalette.Warning else ReMarketPalette.PrimaryDark, modifier = Modifier.weight(1f))
    }
}

@Composable
fun RmLink(text: String, onClick: () -> Unit, modifier: Modifier = Modifier, enabled: Boolean = true) {
    TextButton(onClick, modifier, enabled = enabled, contentPadding = PaddingValues(0.dp)) {
        Text(text, style = rmText(14), textAlign = TextAlign.Center)
    }
}

@Composable
fun RmCard(modifier: Modifier = Modifier, content: @Composable ColumnScope.() -> Unit) {
    Column(modifier.fillMaxWidth().background(Color.White, RoundedCornerShape(20.dp))
        .border(1.dp, ReMarketPalette.Border, RoundedCornerShape(20.dp)).padding(16.dp), content = content)
}
