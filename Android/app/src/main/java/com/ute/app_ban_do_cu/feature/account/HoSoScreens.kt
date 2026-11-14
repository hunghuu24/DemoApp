package com.ute.app_ban_do_cu.feature.account

import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.runtime.saveable.rememberSaveable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.platform.testTag
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.Dp
import androidx.compose.ui.unit.dp
import coil.ImageLoader
import coil.compose.AsyncImage
import com.ute.app_ban_do_cu.BuildConfig
import com.ute.app_ban_do_cu.R
import com.ute.app_ban_do_cu.core.designsystem.*
import com.ute.app_ban_do_cu.domain.model.HoSo
import com.ute.app_ban_do_cu.domain.model.HoSoBody
import com.ute.app_ban_do_cu.feature.auth.*
import java.time.OffsetDateTime

@Composable
fun HoSoAvatar(profile: HoSo, imageLoader: ImageLoader, size: Dp = 88.dp, onEdit: (() -> Unit)? = null) {
    Box(Modifier.size(size)) {
        Box(Modifier.fillMaxSize().clip(CircleShape).background(ReMarketPalette.Soft), contentAlignment = Alignment.Center) {
            if (profile.anhDaiDienUrl != null) AsyncImage(BuildConfig.API_BASE_URL + profile.anhDaiDienUrl,
                "Ảnh đại diện", imageLoader = imageLoader, modifier = Modifier.fillMaxSize(), contentScale = ContentScale.Crop)
            else Text(profile.hoTen.trim().split(Regex("\\s+")).takeLast(2).joinToString("") { it.take(1) }.uppercase(),
                style = rmText(24, FontWeight.SemiBold), color = ReMarketPalette.Primary)
        }
        if (onEdit != null) IconButton(onClick = onEdit, modifier = Modifier.align(Alignment.BottomEnd).size(40.dp)
            .background(ReMarketPalette.Primary, CircleShape).border(2.dp, Color.White, CircleShape).testTag("avatar_pick")) {
            RmIcon(R.drawable.ic_rm_camera, size = 16.dp, color = Color.White, description = "Đổi ảnh đại diện")
        }
    }
}

@Composable
fun HoSoScreen(state: TaiKhoanUiState, vm: TaiKhoanViewModel, loader: ImageLoader) {
    val profile = state.hoSo ?: return
    BoxWithConstraints(Modifier.fillMaxSize().testTag("profile")) {
        val minHeight = maxHeight
        Column(Modifier.fillMaxWidth().verticalScroll(rememberScrollState()).heightIn(min = minHeight)
            .padding(horizontal = 24.dp, vertical = 24.dp), verticalArrangement = Arrangement.SpaceBetween) {
            Column(verticalArrangement = Arrangement.spacedBy(24.dp)) {
                Column(Modifier.fillMaxWidth(), horizontalAlignment = Alignment.CenterHorizontally, verticalArrangement = Arrangement.spacedBy(12.dp)) {
                    HoSoAvatar(profile, loader)
                    Text(profile.hoTen, style = rmText(22, FontWeight.Bold), textAlign = TextAlign.Center)
                    Text(if (profile.vaiTro.contains("ADMIN")) "Quản trị viên ReMarket" else "Thành viên ReMarket", style = rmText(12),
                        modifier = Modifier.background(ReMarketPalette.Soft, CircleShape).padding(horizontal = 12.dp, vertical = 6.dp), color = ReMarketPalette.Primary)
                }
                RmCard {
                    HoSoRow(R.drawable.ic_rm_mail, "Email", profile.email)
                    HorizontalDivider(color = ReMarketPalette.Border)
                    HoSoRow(R.drawable.ic_rm_phone, "Số điện thoại", profile.soDienThoai ?: "Chưa cập nhật")
                    HorizontalDivider(color = ReMarketPalette.Border)
                    HoSoRow(R.drawable.ic_rm_map_pin, "Địa chỉ", profile.diaChi?.takeIf { it.isNotBlank() } ?: "Chưa cập nhật")
                }
                RmNotice("Thông tin liên hệ của bạn được bảo mật. Bạn có thể cập nhật bất cứ lúc nào.")
            }
            RmButton(if ("ADMIN" in profile.vaiTro) "Chỉnh sửa hồ sơ" else "Chỉnh sửa", { vm.mo(ManHinh.SUA_HO_SO) }, Modifier.padding(top = 24.dp).testTag("profile_edit"),
                enabled = !state.dangTai, icon = R.drawable.ic_rm_pencil)
        }
    }
}

@Composable
private fun HoSoRow(icon: Int, label: String, value: String) {
    Row(Modifier.fillMaxWidth().padding(vertical = 16.dp), horizontalArrangement = Arrangement.spacedBy(12.dp)) {
        RmIcon(icon, color = ReMarketPalette.Muted)
        Column(Modifier.weight(1f), verticalArrangement = Arrangement.spacedBy(5.dp)) {
            Text(label, style = rmText(12), color = ReMarketPalette.Muted)
            Text(value, style = MaterialTheme.typography.bodyLarge)
        }
    }
}

@Composable
fun SuaHoSoScreen(state: TaiKhoanUiState, vm: TaiKhoanViewModel, loader: ImageLoader, pickPhoto: () -> Unit) {
    val profile = state.hoSo ?: return
    var name by rememberSaveable(profile.id) { mutableStateOf(profile.hoTen) }
    var email by rememberSaveable(profile.id) { mutableStateOf(profile.email) }
    var phone by rememberSaveable(profile.id) { mutableStateOf(profile.soDienThoai ?: "") }
    var address by rememberSaveable(profile.id) { mutableStateOf(profile.diaChi ?: "") }
    BoxWithConstraints(Modifier.fillMaxSize().testTag("edit_profile")) {
        val minHeight = maxHeight
        Column(Modifier.fillMaxWidth().verticalScroll(rememberScrollState()).heightIn(min = minHeight)
            .padding(start = 24.dp, end = 24.dp, top = 16.dp, bottom = 24.dp), verticalArrangement = Arrangement.SpaceBetween) {
            Column(Modifier.fillMaxWidth(), horizontalAlignment = Alignment.CenterHorizontally, verticalArrangement = Arrangement.spacedBy(10.dp)) {
                HoSoAvatar(profile, loader, 80.dp, if (state.dangTai) null else pickPhoto)
                RmLink("Đổi ảnh đại diện", pickPhoto, enabled = !state.dangTai)
            }
            Column(Modifier.padding(vertical = 24.dp), verticalArrangement = Arrangement.spacedBy(16.dp)) {
                RmField("Họ tên", name, { name = it }, "Nhập họ tên", "edit_name", enabled = !state.dangTai)
                RmField("Email", email, { email = it }, "Nhập email", "edit_email", keyboard = KeyboardType.Email, enabled = !state.dangTai)
                RmField("Số điện thoại", phone, { phone = it }, "Nhập số điện thoại", "edit_phone", keyboard = KeyboardType.Phone, enabled = !state.dangTai)
                RmField("Địa chỉ", address, { address = it }, "Nhập địa chỉ", "edit_address", multiline = true, enabled = !state.dangTai)
            }
            Text(if ("ADMIN" in profile.vaiTro) "Cập nhật thông tin liên hệ phục vụ hoạt động quản trị và bảo mật tài khoản." else "Kiểm tra email và số điện thoại để người mua, người bán có thể liên hệ đúng với bạn.",
                style = MaterialTheme.typography.bodySmall, color = ReMarketPalette.Muted)
            if (state.loi != null) RmLink("Tải lại hồ sơ", { vm.taiHoSo() }, enabled = !state.dangTai)
            RmButton("Lưu thay đổi", { vm.capNhat(HoSoBody(name, email, phone, address, profile.tinhThanh, profile.phienBan)) },
                Modifier.padding(top = 24.dp).testTag("edit_submit"), loading = state.dangTai)
        }
    }
}

@Composable
fun CaiDatScreen(state: TaiKhoanUiState, vm: TaiKhoanViewModel, myListings: () -> Unit = {}, orders: () -> Unit = {}, support: () -> Unit = {}, exchanges:()->Unit={},smartSwaps:()->Unit={}) {
    val profile = state.hoSo ?: return
    Column(Modifier.fillMaxSize().testTag("settings").verticalScroll(rememberScrollState()).padding(24.dp), verticalArrangement = Arrangement.spacedBy(24.dp)) {
        RmCard(Modifier.testTag("settings_profile_header").clickable(enabled=!state.dangTai){vm.mo(ManHinh.HO_SO)}) {
            Row(horizontalArrangement=Arrangement.spacedBy(12.dp),verticalAlignment=Alignment.CenterVertically){
                RmIconTile(R.drawable.ic_rm_user_round)
                Column(Modifier.weight(1f),verticalArrangement=Arrangement.spacedBy(4.dp)){
                    Text(profile.hoTen,style=rmText(17,FontWeight.SemiBold),maxLines=2,overflow=TextOverflow.Ellipsis)
                    Text(profile.email,style=rmText(12),color=ReMarketPalette.Muted,maxLines=1,overflow=TextOverflow.Ellipsis)
                }
                RmIcon(R.drawable.ic_rm_chevron_right,size=18.dp)
            }
        }
        NhomCaiDat("Tiện ích") {
            CaiDatRow(R.drawable.ic_rm_pencil, "Tin của tôi", "Chỉnh sửa và quản lý tin đăng", "my_listings_row", !state.dangTai, onClick = myListings)
            CaiDatRow(R.drawable.ic_rm_user_round, "Đơn hàng của tôi", "Theo dõi các đơn mua bán", "orders_row", !state.dangTai && profile.vaiTro.contains("USER"), onClick = orders)
            CaiDatRow(R.drawable.ic_rm_arrow_left_right, "Trao đổi của tôi", "Các đề nghị và giao dịch trao đổi", "exchanges_row", !state.dangTai && profile.vaiTro.contains("USER"), onClick=exchanges)
            CaiDatRow(R.drawable.ic_rm_arrow_left_right, "Smart Swap của tôi", "Vòng trao đổi, thông báo và bàn giao 3 bên", "smart_swaps_row", !state.dangTai && profile.vaiTro.contains("USER"), onClick=smartSwaps)
            CaiDatRow(R.drawable.ic_rm_lock_keyhole, "Yêu cầu hỗ trợ", "Các yêu cầu hỗ trợ của bạn", "support_row", !state.dangTai, divider = false, onClick = support)
        }
        NhomCaiDat("Tài khoản & bảo mật") {
            CaiDatRow(R.drawable.ic_rm_user_round, "Thông tin cá nhân", "Xem và cập nhật hồ sơ của bạn", "profile_row", !state.dangTai) { vm.mo(ManHinh.HO_SO) }
            CaiDatRow(R.drawable.ic_rm_lock_keyhole, "Đổi mật khẩu", "Bảo vệ tài khoản bằng mật khẩu mới", "change_password_row", !state.dangTai) { vm.mo(ManHinh.DOI_MAT_KHAU) }
            CaiDatRow(R.drawable.ic_rm_badge_check, "Xác minh liên hệ",
                if (profile.daXacMinhEmail && profile.daXacMinhDienThoai) "Email và số điện thoại đã xác minh" else "Liên hệ chưa xác minh đầy đủ",
                enabled = false, divider = false)
        }
        NhomCaiDat("Quyền riêng tư") {
            CaiDatRow(R.drawable.ic_rm_shield_check, "Chính sách bảo mật", "Cách ReMarket bảo vệ dữ liệu của bạn", enabled = false)
            CaiDatRow(R.drawable.ic_rm_trash_2, "Xóa tài khoản", "Xóa vĩnh viễn tài khoản và dữ liệu", enabled = false, divider = false)
        }
        Column(verticalArrangement = Arrangement.spacedBy(12.dp)) {
            RmButton("Đăng xuất", { vm.dangXuat() }, Modifier.testTag("logout_submit"), loading = state.dangTai, outlined = true, icon = R.drawable.ic_rm_log_out)
            Text("ReMarket · Phiên bản 1.0.0", Modifier.fillMaxWidth(), style = rmText(11), color = ReMarketPalette.Muted, textAlign = TextAlign.Center)
        }
    }
}

@Composable
private fun NhomCaiDat(title: String, content: @Composable ColumnScope.() -> Unit) {
    Column(verticalArrangement = Arrangement.spacedBy(14.dp)) {
        Text(title, style = rmText(15, FontWeight.SemiBold), color = ReMarketPalette.Text)
        Column(Modifier.fillMaxWidth().background(Color.White, androidx.compose.foundation.shape.RoundedCornerShape(20.dp))
            .border(1.dp, ReMarketPalette.Border, androidx.compose.foundation.shape.RoundedCornerShape(20.dp)).padding(horizontal = 14.dp), content = content)
    }
}

@Composable
fun CaiDatRow(icon: Int, title: String, subtitle: String, tag: String = "", enabled: Boolean = true,
    divider: Boolean = true, onClick: () -> Unit = {}) {
    Row(Modifier.fillMaxWidth().testTag(tag).clickable(enabled = enabled, onClick = onClick).heightIn(min = 76.dp).padding(vertical=12.dp),
        verticalAlignment = Alignment.CenterVertically, horizontalArrangement = Arrangement.spacedBy(12.dp)) {
        Box(Modifier.size(40.dp).background(if(enabled)ReMarketPalette.Soft else ReMarketPalette.SurfaceMuted, androidx.compose.foundation.shape.RoundedCornerShape(12.dp)), contentAlignment = Alignment.Center) {
            RmIcon(icon, color = if (enabled) ReMarketPalette.Primary else ReMarketPalette.Muted)
        }
        Column(Modifier.weight(1f), verticalArrangement = Arrangement.spacedBy(4.dp)) {
            Text(title, style = rmText(14, FontWeight.SemiBold), color = if (enabled) ReMarketPalette.Text else ReMarketPalette.Muted)
            Text(subtitle, style = rmText(12), color = ReMarketPalette.Muted)
        }
        if(enabled)RmIcon(R.drawable.ic_rm_chevron_right, size = 16.dp, color = ReMarketPalette.Muted)
    }
    if (divider) HorizontalDivider(color = ReMarketPalette.Border)
}
