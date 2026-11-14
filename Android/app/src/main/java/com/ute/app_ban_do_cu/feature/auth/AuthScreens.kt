package com.ute.app_ban_do_cu.feature.auth

import androidx.compose.foundation.Image
import androidx.compose.foundation.background
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.*
import androidx.compose.runtime.saveable.rememberSaveable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.platform.testTag
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import com.ute.app_ban_do_cu.R
import com.ute.app_ban_do_cu.core.designsystem.*
import com.ute.app_ban_do_cu.domain.model.DangKyBody

@Composable
fun SplashScreen(state: TaiKhoanUiState, vm: TaiKhoanViewModel) {
    BoxWithConstraints(Modifier.fillMaxSize().testTag("splash").clickable(enabled = !state.dangTai && state.loi == null) { vm.mo(ManHinh.DANG_NHAP) }) {
        val minHeight = maxHeight
        Column(Modifier.fillMaxWidth().verticalScroll(rememberScrollState()).heightIn(min = minHeight)
            .padding(start = 24.dp, end = 24.dp, top = 80.dp, bottom = 28.dp),
            horizontalAlignment = Alignment.CenterHorizontally, verticalArrangement = Arrangement.SpaceBetween) {
            Column(horizontalAlignment = Alignment.CenterHorizontally, verticalArrangement = Arrangement.spacedBy(16.dp)) {
                Brand(large = true)
                Text("Đồ cũ, hành trình mới.", style = rmText(13), color = ReMarketPalette.Muted)
            }
            Box(Modifier.width(312.dp).height(280.dp), contentAlignment = Alignment.Center) {
                Box(Modifier.size(292.dp, 270.dp).background(ReMarketPalette.Soft, CircleShape))
                Image(androidx.compose.ui.res.painterResource(R.drawable.splash_illustration), "Đồ vật được yêu lại",
                    Modifier.size(280.dp, 230.dp), contentScale = ContentScale.Crop)
                Box(Modifier.align(Alignment.TopEnd).padding(top = 6.dp, end = 10.dp).size(52.dp).background(ReMarketPalette.Warm, CircleShape), contentAlignment = Alignment.Center) {
                    RmIcon(R.drawable.ic_rm_sprout, size = 28.dp)
                }
            }
            Column(horizontalAlignment = Alignment.CenterHorizontally, verticalArrangement = Arrangement.spacedBy(12.dp)) {
                Text("Trao một món đồ.\nThêm một vòng đời.", style = rmText(28, FontWeight.Bold, 1.2f), textAlign = TextAlign.Center)
                Text("Mua bán và trao đổi đồ cũ tại Việt Nam.\nCùng nhau sống xanh hơn mỗi ngày.",
                    style = MaterialTheme.typography.bodyLarge, color = ReMarketPalette.Muted, textAlign = TextAlign.Center)
            }
            if (state.loi != null) {
                Column { RmNotice(state.loi); RmButton("Thử lại", { vm.khoiPhuc() }, Modifier.testTag("restore_retry")) }
            } else Row(verticalAlignment = Alignment.CenterVertically, horizontalArrangement = Arrangement.spacedBy(6.dp)) {
                RmIcon(R.drawable.ic_rm_leaf, size = 14.dp)
                Text("Bớt lãng phí · Thêm giá trị", style = rmText(11), color = ReMarketPalette.Muted)
            }
        }
    }
}

@Composable
fun Brand(large: Boolean = false) {
    Row(verticalAlignment = Alignment.CenterVertically, horizontalArrangement = Arrangement.spacedBy(10.dp)) {
        Box(Modifier.size(if (large) 48.dp else 36.dp).background(ReMarketPalette.Primary, CircleShape), contentAlignment = Alignment.Center) {
            RmIcon(R.drawable.ic_rm_recycle, size = if (large) 30.dp else 24.dp, color = Color.White)
        }
        Text("ReMarket", style = rmText(if (large) 32 else 26, FontWeight.Bold, 1f), color = ReMarketPalette.Primary)
    }
}

@Composable
fun DangNhapScreen(state: TaiKhoanUiState, vm: TaiKhoanViewModel) {
    var account by rememberSaveable(state.emailDangNhap) { mutableStateOf(state.emailDangNhap) }
    var password by remember { mutableStateOf("") }
    BoxWithConstraints(Modifier.fillMaxSize().testTag("login")) {
        val minHeight = maxHeight
        Column(Modifier.fillMaxWidth().verticalScroll(rememberScrollState()).heightIn(min = minHeight)
            .padding(start = 24.dp, end = 24.dp, top = 44.dp, bottom = 32.dp), verticalArrangement = Arrangement.SpaceBetween) {
            Column(verticalArrangement = Arrangement.spacedBy(28.dp)) {
                Brand()
                Column(verticalArrangement = Arrangement.spacedBy(8.dp)) {
                    Text("Mừng bạn trở lại!", style = MaterialTheme.typography.headlineLarge)
                    Text("Đăng nhập để tiếp nối hành trình sống xanh cùng ReMarket.", style = MaterialTheme.typography.bodyLarge, color = ReMarketPalette.Muted)
                }
            }
            Column(Modifier.padding(vertical = 24.dp), verticalArrangement = Arrangement.spacedBy(20.dp)) {
                RmField("Email / Số điện thoại", account, { account = it }, "Nhập email hoặc số điện thoại", "login_account", enabled = !state.dangTai)
                RmField("Mật khẩu", password, { password = it }, "Nhập mật khẩu", "login_password", password = true, enabled = !state.dangTai)
                Row(Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.End) {
                    RmLink("Quên mật khẩu?", { vm.mo(ManHinh.QUEN_MAT_KHAU) }, Modifier.testTag("forgot_link"), !state.dangTai)
                }
                RmButton("Đăng nhập", { vm.dangNhap(account, password) }, Modifier.testTag("login_submit"), loading = state.dangTai)
                RmLink("Chưa có tài khoản? Đăng ký", { vm.mo(ManHinh.DANG_KY) }, Modifier.fillMaxWidth().testTag("register_link"), !state.dangTai)
            }
            RmNotice("Một món đồ được dùng lại là một lựa chọn tốt hơn cho hành tinh.", warning = true)
        }
    }
}

@Composable
fun DangKyScreen(state: TaiKhoanUiState, vm: TaiKhoanViewModel) {
    var name by rememberSaveable { mutableStateOf("") }
    var email by rememberSaveable { mutableStateOf("") }
    var phone by rememberSaveable { mutableStateOf("") }
    var password by remember { mutableStateOf("") }
    var confirm by remember { mutableStateOf("") }
    Column(Modifier.fillMaxSize().testTag("register").verticalScroll(rememberScrollState()).padding(horizontal = 24.dp, vertical = 10.dp),
        verticalArrangement = Arrangement.spacedBy(18.dp)) {
        Column(verticalArrangement = Arrangement.spacedBy(8.dp)) {
            Text("Bắt đầu vòng đời mới", style = MaterialTheme.typography.headlineLarge)
            Text("Tạo tài khoản và cùng lan tỏa lối sống xanh.", style = MaterialTheme.typography.bodyLarge, color = ReMarketPalette.Muted)
        }
        Column(verticalArrangement = Arrangement.spacedBy(12.dp)) {
            RmField("Họ tên", name, { name = it }, "Nhập họ tên của bạn", "register_name", enabled = !state.dangTai)
            RmField("Email", email, { email = it }, "vidu@email.com", "register_email", keyboard = KeyboardType.Email, enabled = !state.dangTai)
            RmField("Số điện thoại", phone, { phone = it }, "Nhập số điện thoại", "register_phone", keyboard = KeyboardType.Phone, enabled = !state.dangTai)
            RmField("Mật khẩu", password, { password = it }, "Tạo mật khẩu", "register_password", password = true, enabled = !state.dangTai)
            Text("Tối thiểu 8 ký tự, gồm chữ và số.", style = MaterialTheme.typography.bodySmall, color = ReMarketPalette.Muted)
            RmField("Xác nhận mật khẩu", confirm, { confirm = it }, "Nhập lại mật khẩu", "register_confirm", password = true, enabled = !state.dangTai)
        }
        Text("Bằng việc đăng ký, bạn đồng ý với Điều khoản sử dụng và Chính sách bảo mật của ReMarket.", style = MaterialTheme.typography.bodySmall, color = ReMarketPalette.Muted)
        RmButton("Đăng ký", { vm.dangKy(DangKyBody(name, email, phone, password, confirm)) }, Modifier.testTag("register_submit"), loading = state.dangTai)
        RmLink("Đã có tài khoản? Đăng nhập", { vm.mo(ManHinh.DANG_NHAP) }, Modifier.fillMaxWidth(), !state.dangTai)
        Spacer(Modifier.height(8.dp))
    }
}

@Composable
fun QuenMatKhauScreen(state: TaiKhoanUiState, vm: TaiKhoanViewModel) {
    var email by rememberSaveable { mutableStateOf("") }
    BoxWithConstraints(Modifier.fillMaxSize().testTag("forgot")) {
        val minHeight = maxHeight
        Column(Modifier.fillMaxWidth().verticalScroll(rememberScrollState()).heightIn(min = minHeight)
            .padding(horizontal = 24.dp, vertical = 28.dp), verticalArrangement = Arrangement.SpaceBetween) {
            Column(verticalArrangement = Arrangement.spacedBy(24.dp)) {
                Box(Modifier.size(88.dp).background(ReMarketPalette.Soft, androidx.compose.foundation.shape.RoundedCornerShape(20.dp)), contentAlignment = Alignment.Center) {
                    RmIcon(R.drawable.ic_rm_lock_keyhole, size = 36.dp)
                }
                Column(verticalArrangement = Arrangement.spacedBy(8.dp)) {
                    Text("Lấy lại mật khẩu", style = MaterialTheme.typography.headlineLarge)
                    Text("Nhập email đã đăng ký. Chúng tôi sẽ gửi liên kết để bạn đặt lại mật khẩu.", style = MaterialTheme.typography.bodyLarge, color = ReMarketPalette.Muted)
                }
                RmField("Email đã đăng ký", email, { email = it }, "Nhập email của bạn", "forgot_email", keyboard = KeyboardType.Email, enabled = !state.dangTai)
                RmButton("Gửi liên kết khôi phục", { vm.quen(email) }, Modifier.testTag("forgot_submit"), loading = state.dangTai)
                RmLink("Quay lại đăng nhập", { vm.mo(ManHinh.DANG_NHAP) }, Modifier.fillMaxWidth(), !state.dangTai)
            }
            Column(Modifier.padding(top = 24.dp), verticalArrangement = Arrangement.spacedBy(14.dp)) {
                RmNotice("Liên kết có hiệu lực trong 15 phút. Kiểm tra cả thư mục spam nếu bạn chưa nhận được email.")
                Text("Để bảo vệ tài khoản, không chia sẻ liên kết khôi phục với bất kỳ ai.", style = MaterialTheme.typography.bodySmall, color = ReMarketPalette.Muted, textAlign = TextAlign.Center)
            }
        }
    }
}

@Composable
fun MatKhauScreen(state: TaiKhoanUiState, vm: TaiKhoanViewModel) {
    val reset = state.manHinh == ManHinh.DAT_LAI
    var old by remember { mutableStateOf("") }
    var password by remember { mutableStateOf("") }
    var confirm by remember { mutableStateOf("") }
    Column(Modifier.fillMaxSize().verticalScroll(rememberScrollState()).padding(24.dp), verticalArrangement = Arrangement.spacedBy(24.dp)) {
        Text(if (reset) "Đặt lại mật khẩu" else "Đổi mật khẩu", style = MaterialTheme.typography.headlineLarge)
        if (reset) Text(state.emailKhoiPhuc, style = MaterialTheme.typography.bodyLarge, color = ReMarketPalette.Muted)
        if (!reset) RmField("Mật khẩu cũ", old, { old = it }, "Nhập mật khẩu cũ", "change_old", password = true, enabled = !state.dangTai)
        RmField("Mật khẩu mới", password, { password = it }, "Nhập mật khẩu mới", "change_password", password = true, enabled = !state.dangTai)
        RmField("Xác nhận mật khẩu mới", confirm, { confirm = it }, "Xác nhận mật khẩu mới", "change_confirm", password = true, enabled = !state.dangTai)
        RmButton("Xác nhận", { if (reset) vm.datLai(password, confirm) else vm.doiMatKhau(old, password, confirm) },
            Modifier.testTag("change_submit"), loading = state.dangTai)
        RmNotice("Để bảo vệ tài khoản, không chia sẻ mật khẩu mới với bất kỳ ai.")
    }
}
