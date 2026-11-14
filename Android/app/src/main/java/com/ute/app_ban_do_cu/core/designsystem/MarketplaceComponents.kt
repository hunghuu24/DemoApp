package com.ute.app_ban_do_cu.core.designsystem

import androidx.annotation.DrawableRes
import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.dp
import com.ute.app_ban_do_cu.R

enum class RmTone { POSITIVE, PENDING, NEGATIVE, NEUTRAL }

@Composable
fun RmBadge(text: String, modifier: Modifier = Modifier, tone: RmTone = RmTone.POSITIVE) {
    val (background, foreground) = when (tone) {
        RmTone.POSITIVE -> ReMarketPalette.Soft to ReMarketPalette.PrimaryDark
        RmTone.PENDING -> ReMarketPalette.Warm to ReMarketPalette.Warning
        RmTone.NEGATIVE -> ReMarketPalette.ErrorSoft to ReMarketPalette.Error
        RmTone.NEUTRAL -> ReMarketPalette.SurfaceMuted to ReMarketPalette.Muted
    }
    Surface(modifier, color = background, contentColor = foreground, shape = CircleShape) {
        Text(text, Modifier.padding(horizontal = 10.dp, vertical = 5.dp), style = rmText(11, FontWeight.SemiBold))
    }
}

@Composable
fun RmIconTile(@DrawableRes icon: Int, modifier: Modifier = Modifier, warm: Boolean = false) {
    Box(modifier.size(44.dp).background(if (warm) ReMarketPalette.Warm else ReMarketPalette.Soft, RoundedCornerShape(14.dp)),
        contentAlignment = Alignment.Center) { RmIcon(icon, size = 22.dp, color = if (warm) ReMarketPalette.Warning else ReMarketPalette.Primary) }
}

@Composable
fun RmSectionTitle(title: String, subtitle: String? = null, @DrawableRes icon: Int? = null) {
    Row(Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.spacedBy(12.dp), verticalAlignment = Alignment.CenterVertically) {
        if (icon != null) RmIconTile(icon)
        Column(Modifier.weight(1f), verticalArrangement = Arrangement.spacedBy(3.dp)) {
            Text(title, style = rmText(17, FontWeight.SemiBold), color = ReMarketPalette.Text)
            if (!subtitle.isNullOrBlank()) Text(subtitle, style = rmText(12), color = ReMarketPalette.Muted)
        }
    }
}

@Composable
fun RmKeyValue(label: String, value: String, modifier: Modifier = Modifier, emphasized: Boolean = false) {
    Row(modifier.fillMaxWidth(), horizontalArrangement = Arrangement.spacedBy(12.dp), verticalAlignment = Alignment.Top) {
        Text(label, Modifier.weight(1f), style = rmText(12), color = ReMarketPalette.Muted)
        Text(value, Modifier.weight(1.2f), style = rmText(if (emphasized) 16 else 13, if (emphasized) FontWeight.Bold else FontWeight.Medium),
            color = if (emphasized) ReMarketPalette.Primary else ReMarketPalette.Text, textAlign = TextAlign.End)
    }
}

@Composable
fun RmProgress(completed: Int, total: Int, title: String, modifier: Modifier = Modifier, paused: Boolean = false) {
    val safeTotal = total.coerceAtLeast(1)
    val safeCompleted = completed.coerceIn(0, safeTotal)
    val accent = if (paused) ReMarketPalette.Warning else ReMarketPalette.Primary
    Surface(modifier.fillMaxWidth(), shape = RoundedCornerShape(20.dp), color = Color.White,
        border = BorderStroke(1.dp, ReMarketPalette.Border)) {
        Column(Modifier.padding(16.dp), verticalArrangement = Arrangement.spacedBy(12.dp)) {
            Row(verticalAlignment = Alignment.CenterVertically, horizontalArrangement = Arrangement.spacedBy(12.dp)) {
                Text(title, Modifier.weight(1f), style = rmText(14, FontWeight.SemiBold))
                Text("$safeCompleted/$safeTotal", style = rmText(20, FontWeight.Bold), color = accent)
            }
            LinearProgressIndicator(progress = { safeCompleted.toFloat() / safeTotal }, modifier = Modifier.fillMaxWidth().height(6.dp),
                color = accent, trackColor = ReMarketPalette.Soft)
        }
    }
}

@Composable
fun RmTimelineItem(title: String, detail: String, completed: Boolean, last: Boolean = false) {
    Row(Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.spacedBy(12.dp)) {
        Column(horizontalAlignment = Alignment.CenterHorizontally) {
            Box(Modifier.size(26.dp).background(if (completed) ReMarketPalette.Soft else ReMarketPalette.SurfaceMuted, CircleShape), contentAlignment = Alignment.Center) {
                if (completed) RmIcon(R.drawable.ic_rm_badge_check, size = 16.dp)
                else Box(Modifier.size(7.dp).background(ReMarketPalette.Border, CircleShape))
            }
            if (!last) Box(Modifier.width(1.dp).heightIn(min = 30.dp).background(ReMarketPalette.Border))
        }
        Column(Modifier.weight(1f).padding(bottom = if (last) 0.dp else 16.dp), verticalArrangement = Arrangement.spacedBy(3.dp)) {
            Text(title, style = rmText(13, FontWeight.SemiBold), color = if (completed) ReMarketPalette.Text else ReMarketPalette.Muted)
            if (detail.isNotBlank()) Text(detail, style = rmText(11), color = ReMarketPalette.Muted)
        }
    }
}

@Composable
fun RmActionBar(modifier: Modifier = Modifier, content: @Composable ColumnScope.() -> Unit) {
    Surface(modifier.fillMaxWidth(), color = Color.White, shadowElevation = 3.dp) {
        Column(Modifier.navigationBarsPadding().imePadding().padding(horizontal = 24.dp, vertical = 12.dp),
            verticalArrangement = Arrangement.spacedBy(8.dp), content = content)
    }
}

@Composable
fun RmEmptyIcon(@DrawableRes icon: Int = R.drawable.ic_rm_leaf) {
    Box(Modifier.size(76.dp).background(ReMarketPalette.Soft, CircleShape), contentAlignment = Alignment.Center) { RmIcon(icon, size = 32.dp) }
}

@Composable
fun RmChatBubble(own: Boolean, time: String, name: String? = null, content: @Composable ColumnScope.() -> Unit) {
    BoxWithConstraints(Modifier.fillMaxWidth()) {
        val bubbleWidth = maxWidth * .86f
        Row(Modifier.fillMaxWidth(), horizontalArrangement = if (own) Arrangement.End else Arrangement.Start) {
            Surface(Modifier.widthIn(max = bubbleWidth),
                shape = RoundedCornerShape(topStart = 20.dp, topEnd = 20.dp, bottomEnd = if (own) 6.dp else 20.dp, bottomStart = if (own) 20.dp else 6.dp),
                color = if (own) ReMarketPalette.Primary else Color.White, contentColor = if (own) Color.White else ReMarketPalette.Text,
                border = if (own) null else BorderStroke(1.dp, ReMarketPalette.Border)) {
                Column(Modifier.padding(14.dp), verticalArrangement = Arrangement.spacedBy(7.dp)) {
                    if (!name.isNullOrBlank()) Text(name, style = rmText(11, FontWeight.SemiBold),
                        color = if (own) Color.White.copy(alpha = .85f) else ReMarketPalette.Primary, maxLines = 2, overflow = TextOverflow.Ellipsis)
                    content()
                    Text(time, Modifier.align(Alignment.End), style = rmText(10), color = if (own) Color.White.copy(alpha = .75f) else ReMarketPalette.Muted)
                }
            }
        }
    }
}
