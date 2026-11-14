package com.ute.app_ban_do_cu.feature.admin

import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.testTag
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.dp
import coil.ImageLoader
import com.ute.app_ban_do_cu.R
import com.ute.app_ban_do_cu.core.designsystem.*
import com.ute.app_ban_do_cu.feature.account.CaiDatRow
import com.ute.app_ban_do_cu.feature.account.HoSoAvatar
import com.ute.app_ban_do_cu.feature.auth.*

@Composable
fun TaiKhoanQuanTriScreen(state: TaiKhoanUiState, vm: TaiKhoanViewModel, loader: ImageLoader) {
    val profile = state.hoSo?.takeIf { "ADMIN" in it.vaiTro } ?: return
    Column(Modifier.fillMaxSize().testTag("admin_account").verticalScroll(rememberScrollState()).padding(24.dp),
        verticalArrangement = Arrangement.spacedBy(24.dp)) {
        RmCard(Modifier.testTag("admin_account_profile").clickable(enabled = !state.dangTai) { vm.mo(ManHinh.HO_SO) }) {
            Row(Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.spacedBy(14.dp), verticalAlignment = Alignment.CenterVertically) {
                HoSoAvatar(profile, loader, 64.dp)
                Column(Modifier.weight(1f), verticalArrangement = Arrangement.spacedBy(6.dp)) {
                    Text(profile.hoTen, style = rmText(18, FontWeight.SemiBold), maxLines = 2, overflow = TextOverflow.Ellipsis)
                    Text(profile.email, style = rmText(12), color = ReMarketPalette.Muted, maxLines = 2, overflow = TextOverflow.Ellipsis)
                    RmBadge("Quản trị viên · ADMIN")
                }
                RmIcon(R.drawable.ic_rm_chevron_right, size = 16.dp)
            }
        }
        Column(verticalArrangement = Arrangement.spacedBy(12.dp)) {
            RmSectionTitle("Hồ sơ & bảo mật", "Quản lý thông tin và bảo vệ tài khoản quản trị")
            RmCard {
                CaiDatRow(R.drawable.ic_rm_user_round, "Hồ sơ quản trị", "Thông tin cá nhân và ảnh đại diện", "profile_row", !state.dangTai) {
                    vm.mo(ManHinh.HO_SO)
                }
                CaiDatRow(R.drawable.ic_rm_lock_keyhole, "Đổi mật khẩu", "Cập nhật mật khẩu đăng nhập quản trị", "change_password_row", !state.dangTai, divider = false) {
                    vm.mo(ManHinh.DOI_MAT_KHAU)
                }
            }
        }
        RmNotice("Thông tin liên hệ trong hồ sơ được dùng cho hoạt động quản trị và bảo mật tài khoản.")
        Column(verticalArrangement = Arrangement.spacedBy(12.dp)) {
            RmButton("Trở lại quản trị", vm::quayLai, Modifier.testTag("admin_account_back"), enabled = !state.dangTai,
                icon = R.drawable.ic_fg_admin_layout_dashboard)
            RmButton("Đăng xuất", vm::dangXuat, Modifier.testTag("logout_submit"), loading = state.dangTai, outlined = true,
                icon = R.drawable.ic_rm_log_out)
            Text("ReMarket Admin · Phiên bản 1.0.0", Modifier.fillMaxWidth(), style = rmText(11),
                color = ReMarketPalette.Muted, textAlign = TextAlign.Center)
        }
    }
}
