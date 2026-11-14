package com.ute.app_ban_do_cu.feature.catalog

import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.draw.rotate
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.platform.testTag
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.dp
import coil.ImageLoader
import coil.compose.SubcomposeAsyncImage
import coil.compose.SubcomposeAsyncImageContent
import coil.request.ImageRequest
import com.ute.app_ban_do_cu.BuildConfig
import com.ute.app_ban_do_cu.R
import com.ute.app_ban_do_cu.core.designsystem.*
import com.ute.app_ban_do_cu.domain.model.TinDang
import java.text.NumberFormat
import java.util.Locale

fun tien(value: Long?) = NumberFormat.getIntegerInstance(Locale.forLanguageTag("vi-VN")).format(value ?: 0) + " đ"
val tinhTrangOptions = listOf("NhuMoi" to "Như mới", "DaSuDungTot" to "Đã sử dụng · Tốt", "CoLoiNhe" to "Có lỗi nhẹ")
val trangThaiOptions = listOf("DangHienThi" to "Đang bán", "DangGiuCho" to "Đang giao dịch", "ChoDuyet" to "Chờ duyệt", "DaBan" to "Đã bán", "DaTraoDoi" to "Đã trao đổi", "BanNhap" to "Bản nháp", "DaAn" to "Đã ẩn", "TuChoi" to "Từ chối")
fun tinhTrangText(value: String) = tinhTrangOptions.firstOrNull { it.first == value }?.second ?: value
fun trangThaiText(value: String) = trangThaiOptions.firstOrNull { it.first == value }?.second ?: value
fun iconDanhMuc(name: String, icon:String=""): Int = when {
    icon=="smartphone" -> R.drawable.ic_fg_smartphone
    icon=="armchair" -> R.drawable.ic_fg_armchair
    icon=="shirt" -> R.drawable.ic_fg_shirt
    icon=="cooking-pot" -> R.drawable.ic_fg_cooking_pot
    icon=="book-open" -> R.drawable.ic_fg_book_open
    icon=="bike" -> R.drawable.ic_fg_bike
    icon=="recycle" -> R.drawable.ic_rm_recycle
    name.contains("Nội thất", true) -> R.drawable.ic_fg_armchair
    name.contains("Điện tử", true) -> R.drawable.ic_fg_smartphone
    name.contains("Thời trang", true) -> R.drawable.ic_fg_shirt
    name.contains("Gia dụng", true) -> R.drawable.ic_fg_cooking_pot
    name.contains("Sách", true) -> R.drawable.ic_fg_book_open
    name.contains("Thể thao", true) -> R.drawable.ic_fg_bike
    else -> R.drawable.ic_rm_recycle
}

@Composable
fun AnhSanPham(url: String?, loader: ImageLoader, modifier: Modifier = Modifier, description: String? = null) {
    var retry by remember(url) { mutableIntStateOf(0) }
    val resolved = url?.let { if (it.startsWith("http")) it else BuildConfig.API_BASE_URL + it.removePrefix("/") }
    BoxWithConstraints(modifier.background(ReMarketPalette.SurfaceMuted), contentAlignment = Alignment.Center) {
        val thumbnail = maxWidth < 96.dp
        if (resolved == null) Column(horizontalAlignment = Alignment.CenterHorizontally, verticalArrangement = Arrangement.spacedBy(6.dp)) {
            RmIcon(R.drawable.ic_rm_camera, size = 28.dp)
            if (!thumbnail) Text("Chưa có ảnh", style = rmText(12), color = ReMarketPalette.Muted)
        } else SubcomposeAsyncImage(model = ImageRequest.Builder(LocalContext.current)
            .data(if (retry == 0) resolved else resolved + (if (resolved.contains('?')) "&" else "?") + "reload=$retry").crossfade(true).build(),
            imageLoader = loader, contentDescription = description, contentScale = ContentScale.Crop, modifier = Modifier.fillMaxSize(),
            loading = { Box(Modifier.fillMaxSize(), contentAlignment = Alignment.Center) { CircularProgressIndicator(Modifier.size(22.dp), strokeWidth = 2.dp) } },
            success = { SubcomposeAsyncImageContent(Modifier.testTag("product_image_loaded")) },
            error = { Column(Modifier.fillMaxSize().clickable { retry++ }, horizontalAlignment = Alignment.CenterHorizontally, verticalArrangement = Arrangement.Center) {
                RmIcon(R.drawable.ic_rm_camera, description = "Ảnh chưa tải được. Chạm để thử lại")
                if (!thumbnail) {
                    Text("Ảnh chưa tải được", style = rmText(11), color = ReMarketPalette.Muted, textAlign = TextAlign.Center)
                    Text("Chạm để thử lại", style = rmText(11), color = ReMarketPalette.Primary, textAlign = TextAlign.Center)
                }
            } })
    }
}

@Composable
fun NhanTrangThai(status: String, modifier: Modifier = Modifier) {
    val tone = when (status) {
        "TuChoi" -> RmTone.NEGATIVE
        "ChoDuyet", "DangGiuCho" -> RmTone.PENDING
        "BanNhap", "DaAn" -> RmTone.NEUTRAL
        else -> RmTone.POSITIVE
    }
    RmBadge(trangThaiText(status), modifier, tone)
}

@Composable
fun LuaChon(label: String, value: String, options: List<Pair<String, String>>, onSelect: (String) -> Unit,
    modifier: Modifier = Modifier, tag: String = "", enabled: Boolean = true, icon: Int? = null) {
    var expanded by remember { mutableStateOf(false) }
    Column(modifier, verticalArrangement = Arrangement.spacedBy(8.dp)) {
        if (label.isNotBlank()) Text(label, style = MaterialTheme.typography.labelMedium)
        Box {
            Row(Modifier.fillMaxWidth().heightIn(min = 52.dp).testTag(tag).background(if (enabled) Color.White else ReMarketPalette.SurfaceMuted, RoundedCornerShape(14.dp))
                .border(1.dp, if (expanded) ReMarketPalette.Primary else ReMarketPalette.Border, RoundedCornerShape(14.dp)).clickable(enabled = enabled) { expanded = true }.padding(16.dp),
                verticalAlignment = Alignment.CenterVertically) {
                Text(options.firstOrNull { it.first == value }?.second ?: "Chọn $label", Modifier.weight(1f), style = rmText(14),
                    color = if (value.isBlank()) ReMarketPalette.Muted else ReMarketPalette.Text)
                if (icon == null) RmIcon(R.drawable.ic_rm_chevron_right, Modifier.rotate(90f), size = 18.dp)
                else Icon(androidx.compose.ui.res.painterResource(icon),null,Modifier.size(18.dp),tint=Color.Unspecified)
            }
            DropdownMenu(expanded, { expanded = false }, modifier = Modifier.heightIn(max = 360.dp), containerColor = Color.White) {
                options.forEach { (id, text) -> DropdownMenuItem(text = { Text(text, style = rmText(14, if (id == value) FontWeight.SemiBold else FontWeight.Normal)) },
                    onClick = { expanded = false; onSelect(id) }, modifier = Modifier.testTag("${tag}_$id").background(if (id == value) ReMarketPalette.Soft else Color.Transparent)) }
            }
        }
    }
}

@Composable
fun LoiTruong(errors: Map<String, String>, key: String) {
    errors[key]?.let { Text(it, Modifier.testTag("error_$key"), style = rmText(12), color = ReMarketPalette.Error) }
}

@Composable
fun TrangRong(title: String, detail: String, modifier: Modifier = Modifier, action: (() -> Unit)? = null, actionLabel: String = "Thử lại") {
    Column(modifier.fillMaxWidth().padding(vertical = 32.dp, horizontal = 12.dp), horizontalAlignment = Alignment.CenterHorizontally, verticalArrangement = Arrangement.spacedBy(14.dp)) {
        RmEmptyIcon()
        Text(title, style = rmText(18, FontWeight.SemiBold), textAlign = TextAlign.Center)
        Text(detail, style = rmText(13, height = 1.5f), color = ReMarketPalette.Muted, textAlign = TextAlign.Center)
        if (action != null) RmButton(actionLabel, action, Modifier.widthIn(max = 260.dp), outlined = true)
    }
}

@Composable
fun TheSanPham(tin: TinDang, loader: ImageLoader, modifier: Modifier = Modifier, compact: Boolean = false, onClick: () -> Unit) {
    val cover = tin.anh.firstOrNull { it.laAnhBia } ?: tin.anh.firstOrNull()
    val card = modifier.clip(RoundedCornerShape(20.dp)).background(Color.White).border(1.dp, ReMarketPalette.Border, RoundedCornerShape(20.dp))
        .clickable(onClick = onClick).testTag("listing_${tin.id}")
    if (compact) Row(card.fillMaxWidth().padding(12.dp), horizontalArrangement = Arrangement.spacedBy(14.dp), verticalAlignment = Alignment.CenterVertically) {
        AnhSanPham(cover?.url, loader, Modifier.size(92.dp).clip(RoundedCornerShape(14.dp)), tin.ten)
        Column(Modifier.weight(1f), verticalArrangement = Arrangement.spacedBy(6.dp)) {
            Text(tin.ten, style = rmText(14, FontWeight.SemiBold), maxLines = 2, overflow = TextOverflow.Ellipsis)
            Text(tien(tin.giaBan ?: tin.giaTriThamKhao), style = rmText(17, FontWeight.Bold), color = ReMarketPalette.Primary)
            Text(tinhTrangText(tin.tinhTrang), style = rmText(11), color = ReMarketPalette.Muted, maxLines = 1, overflow = TextOverflow.Ellipsis)
            Row(horizontalArrangement = Arrangement.spacedBy(4.dp), verticalAlignment = Alignment.CenterVertically) {
                RmIcon(R.drawable.ic_rm_map_pin, size = 12.dp, color = ReMarketPalette.Muted)
                Text(tin.diaDiem, style = rmText(11), color = ReMarketPalette.Muted, maxLines = 1, overflow = TextOverflow.Ellipsis)
            }
        }
    } else Column(card.fillMaxWidth()) {
        Box {
            AnhSanPham(cover?.url, loader, Modifier.fillMaxWidth().aspectRatio(1.08f), tin.ten)
            if (tin.traoDoi?.choPhepSmartSwap == true) RmBadge("Smart Swap", Modifier.align(Alignment.TopStart).padding(8.dp))
            else if (tin.loaiTin == "TraoDoi") RmBadge("Trao đổi", Modifier.align(Alignment.TopStart).padding(8.dp))
            if (tin.anh.size > 1) RmBadge("${tin.anh.size} ảnh", Modifier.align(Alignment.BottomEnd).padding(8.dp), RmTone.NEUTRAL)
        }
        Column(Modifier.padding(12.dp), verticalArrangement = Arrangement.spacedBy(7.dp)) {
            Text(tin.ten, style = rmText(14, FontWeight.Medium), minLines = 2, maxLines = 2, overflow = TextOverflow.Ellipsis)
            Text(tien(tin.giaBan ?: tin.giaTriThamKhao), style = rmText(17, FontWeight.Bold), color = ReMarketPalette.Primary)
            Text(tinhTrangText(tin.tinhTrang), Modifier.background(ReMarketPalette.SurfaceMuted, RoundedCornerShape(6.dp)).padding(horizontal = 6.dp, vertical = 3.dp),
                style = rmText(11, FontWeight.Medium), color = ReMarketPalette.Muted, maxLines = 1, overflow = TextOverflow.Ellipsis)
            Row(horizontalArrangement = Arrangement.spacedBy(3.dp), verticalAlignment = Alignment.CenterVertically) {
                RmIcon(R.drawable.ic_rm_map_pin, size = 12.dp, color = ReMarketPalette.Muted)
                Text(tin.diaDiem, style = rmText(11), color = ReMarketPalette.Muted, maxLines = 1, overflow = TextOverflow.Ellipsis)
            }
        }
    }
}

@Composable
fun LuoiSanPham(items: List<TinDang>, loader: ImageLoader, onClick: (TinDang) -> Unit) {
    BoxWithConstraints {
        val columns = if (maxWidth >= 560.dp) 3 else 2
        Column(verticalArrangement = Arrangement.spacedBy(12.dp)) {
            items.chunked(columns).forEach { row -> Row(horizontalArrangement = Arrangement.spacedBy(12.dp)) {
                row.forEach { tin -> TheSanPham(tin, loader, Modifier.weight(1f)) { onClick(tin) } }
                repeat(columns - row.size) { Spacer(Modifier.weight(1f)) }
            } }
        }
    }
}

@Composable
fun PhanTrang(page: Int, total: Int, enabled: Boolean, previous: () -> Unit, next: () -> Unit) {
    if (total <= 1) return
    Row(Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween, verticalAlignment = Alignment.CenterVertically) {
        TextButton(previous, enabled = enabled && page > 1, modifier = Modifier.testTag("page_previous")) { Text("Trang trước") }
        Text("$page / $total", style = rmText(12))
        TextButton(next, enabled = enabled && page < total, modifier = Modifier.testTag("page_next")) { Text("Trang sau") }
    }
}
