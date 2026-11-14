package com.ute.app_ban_do_cu.feature.commerce

import androidx.compose.foundation.*
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.platform.testTag
import androidx.compose.ui.res.painterResource
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.dp
import coil.ImageLoader
import com.ute.app_ban_do_cu.R
import com.ute.app_ban_do_cu.core.designsystem.*
import com.ute.app_ban_do_cu.domain.model.*
import com.ute.app_ban_do_cu.feature.catalog.AnhSanPham
import java.time.OffsetDateTime
import java.time.ZoneId
import java.time.format.DateTimeFormatter

object MuaBanColors {
    val Green = ReMarketPalette.Primary; val Background = ReMarketPalette.Background; val Text = ReMarketPalette.Text
    val Muted = ReMarketPalette.Muted; val Border = ReMarketPalette.Border; val Soft = ReMarketPalette.Soft; val Heading = ReMarketPalette.PrimaryDark
}
@Composable
fun MuaBanTheme(content: @Composable () -> Unit) {
    MaterialTheme(colorScheme = MaterialTheme.colorScheme.copy(primary = MuaBanColors.Green, background = MuaBanColors.Background,
        surface = Color.White, onSurface = MuaBanColors.Text, onBackground = MuaBanColors.Text, secondaryContainer = MuaBanColors.Soft,
        onSecondaryContainer = MuaBanColors.Green, outline = MuaBanColors.Border, onSurfaceVariant = MuaBanColors.Muted), typography = MaterialTheme.typography.copy(labelMedium = rmText(12, FontWeight.SemiBold)), content = content)
}
fun ngayGio(value: String?): String = if (value == null) "Chưa cập nhật" else try {
    OffsetDateTime.parse(value).atZoneSameInstant(ZoneId.of("Asia/Ho_Chi_Minh")).format(DateTimeFormatter.ofPattern("dd/MM/yyyy · HH:mm"))
} catch (_: Exception) { value.take(16) }
val donStates = listOf("ChoThanhToan" to "Chờ thanh toán", "DaThanhToan" to "Đã thanh toán", "DangGiao" to "Đang giao", "DaNhan" to "Hoàn tất", "DaHuy" to "Đã hủy")
fun tenTrangThai(value: String) = (donStates + listOf("ChoLayHang" to "Chờ lấy hàng", "DaLayHang" to "Đã lấy hàng", "DangVanChuyen" to "Đang vận chuyển",
    "DaGiao" to "Đã giao", "GiaoThatBai" to "Giao thất bại", "Moi" to "Đã gửi", "DangXuLy" to "Đang xử lý", "DaGiaiQuyet" to "Đã giải quyết",
    "PhanHoi" to "Phản hồi", "ChoXuLy" to "Chờ xử lý", "ThanhCong" to "Thành công", "ThatBai" to "Thất bại", "HoanTien" to "Đã hoàn tiền mô phỏng")).firstOrNull { it.first == value }?.second ?: value

@Composable
fun MbButton(text: String, onClick: () -> Unit, modifier: Modifier = Modifier, enabled: Boolean = true, outlined: Boolean = false) {
    RmButton(text, onClick, modifier, enabled = enabled, outlined = outlined)
}
@Composable
fun MbCard(modifier: Modifier = Modifier, content: @Composable ColumnScope.() -> Unit) {
    RmCard(modifier) { Column(verticalArrangement = Arrangement.spacedBy(12.dp), content = content) }
}
@Composable
fun MbNote(title: String, detail: String) {
    Row(Modifier.fillMaxWidth().background(MuaBanColors.Soft,RoundedCornerShape(18.dp)).padding(16.dp),horizontalArrangement=Arrangement.spacedBy(12.dp)) {
        RmIcon(R.drawable.ic_rm_info, size = 20.dp)
        Column(Modifier.weight(1f), verticalArrangement=Arrangement.spacedBy(5.dp)) {
            Text(title,style=rmText(15,FontWeight.SemiBold),color=MuaBanColors.Heading)
            Text(detail,style=rmText(12),color=MuaBanColors.Muted)
        }
    }
}
@Composable
fun MonHangMini(product: SanPhamMua, loader: ImageLoader, detail: String? = null, framed: Boolean = true, trailing: (@Composable () -> Unit)? = null) {
    if (framed) MbCard { MonHangMini(product, loader, detail, framed = false, trailing = trailing) }
    else Row(horizontalArrangement=Arrangement.spacedBy(14.dp),verticalAlignment=Alignment.CenterVertically) {
        AnhSanPham(product.anhUrl,loader,Modifier.size(68.dp).clip(RoundedCornerShape(14.dp)),product.ten)
        Column(Modifier.weight(1f),verticalArrangement=Arrangement.spacedBy(5.dp)) {
            Text(product.ten,style=rmText(14,FontWeight.SemiBold), maxLines = 2, overflow = TextOverflow.Ellipsis)
            Text(detail ?: "Người bán: ${product.nguoiBan}",style=rmText(12),color=MuaBanColors.Muted)
        }
        trailing?.invoke()
    }
}
@Composable
fun TongTien(goc: Long, giam: Long, ship: Long, total: Long) {
    MbCard {
        DongTien("Giá sản phẩm",goc)
        if (giam > 0) DongTien("Phiếu giảm giá",-giam)
        DongTien("Phí vận chuyển mô phỏng",ship)
        HorizontalDivider(color=MuaBanColors.Border)
        RmKeyValue("Tổng thanh toán", com.ute.app_ban_do_cu.feature.catalog.tien(total), emphasized = true)
    }
}
@Composable
private fun DongTien(label:String,value:Long) { RmKeyValue(label, com.ute.app_ban_do_cu.feature.catalog.tien(value)) }
@Composable
fun MoPhongSelect(enabled:Boolean) {
    var expanded by remember { mutableStateOf(false) }
    Box {
        Row(Modifier.fillMaxWidth().heightIn(min=46.dp).background(Color.White,RoundedCornerShape(12.dp)).border(1.dp,MuaBanColors.Border,RoundedCornerShape(12.dp))
            .testTag("payment_method").clickable(enabled=enabled){expanded=true}.padding(14.dp),verticalAlignment=Alignment.CenterVertically) {
            Text("Thanh toán mô phỏng",Modifier.weight(1f),style=rmText(13),color=MuaBanColors.Muted)
            Icon(painterResource(R.drawable.ic_fg_checkout_chevron),null,Modifier.size(14.dp,8.dp),tint=Color.Unspecified)
        }
        DropdownMenu(expanded,{expanded=false}) { DropdownMenuItem({Text("Mô phỏng · không trừ tiền thật")},{expanded=false}) }
    }
}
@Composable
fun MocLichSu(title:String,detail:String?,done:Boolean=true) {
    RmTimelineItem(title, detail.orEmpty(), done, last = true)
}
