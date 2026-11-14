package com.ute.app_ban_do_cu.feature.auth

import android.net.Uri
import androidx.activity.compose.BackHandler
import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.activity.result.PickVisualMediaRequest
import androidx.activity.result.contract.ActivityResultContracts
import androidx.compose.foundation.background
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.platform.testTag
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.dp
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import coil.ImageLoader
import com.ute.app_ban_do_cu.R
import com.ute.app_ban_do_cu.core.designsystem.*
import com.ute.app_ban_do_cu.feature.account.*
import com.ute.app_ban_do_cu.feature.catalog.*
import com.ute.app_ban_do_cu.feature.commerce.*
import com.ute.app_ban_do_cu.feature.exchange.*
import com.ute.app_ban_do_cu.feature.smartswap.*
import com.ute.app_ban_do_cu.feature.admin.*
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch
import kotlinx.coroutines.withContext
import java.io.ByteArrayOutputStream
import java.io.IOException

@Composable
fun TaiKhoanApp(vm: TaiKhoanViewModel, loader: ImageLoader, link: Uri?, listings: BanHangViewModel, commerce: MuaBanViewModel, exchange:TraoDoiViewModel,smart:SmartSwapViewModel,admin:QuanTriViewModel) {
    val state by vm.uiState.collectAsStateWithLifecycle()
    val catalog by listings.uiState.collectAsStateWithLifecycle()
    val trade by commerce.uiState.collectAsStateWithLifecycle()
    val swap by exchange.uiState.collectAsStateWithLifecycle()
    val ss by smart.uiState.collectAsStateWithLifecycle()
    val management by admin.uiState.collectAsStateWithLifecycle()
    val isAdmin = state.hoSo?.vaiTro?.contains("ADMIN") == true
    LaunchedEffect(state.hoSo?.id,state.hoSo?.vaiTro){admin.session(state.hoSo?.id,state.hoSo?.vaiTro.orEmpty())}
    LaunchedEffect(management.expired,management.denied){if(management.expired||management.denied)vm.khoiPhuc()}
    LaunchedEffect(state.hoSo?.id) { listings.phienMoi(state.hoSo?.id); commerce.phienMoi(state.hoSo?.id);exchange.phienMoi(state.hoSo?.id);smart.session(state.hoSo?.id) }
    LaunchedEffect(ss.expired) {if(ss.expired)vm.khoiPhuc()}
    LaunchedEffect(catalog.expired) { if (catalog.expired) vm.khoiPhuc() }
    LaunchedEffect(trade.expired) { if (trade.expired) vm.khoiPhuc() }
    LaunchedEffect(swap.expired) { if (swap.expired) vm.khoiPhuc() }
    val snackbar = remember { SnackbarHostState() }
    val context = LocalContext.current
    val scope = rememberCoroutineScope()
    val picker = rememberLauncherForActivityResult(ActivityResultContracts.PickVisualMedia()) { uri ->
        if (uri != null) scope.launch {
            try {
                val bytes = withContext(Dispatchers.IO) {
                    context.contentResolver.openInputStream(uri)?.use { input ->
                        val output = ByteArrayOutputStream()
                        val buffer = ByteArray(16 * 1024)
                        while (output.size() <= 2 * 1024 * 1024) {
                            val read = input.read(buffer)
                            if (read < 0) break
                            output.write(buffer, 0, read)
                        }
                        output.toByteArray()
                    } ?: throw IOException("Cannot open selected image")
                }
                vm.doiAnh(bytes)
            } catch (_: IOException) { vm.baoLoi("Không đọc được ảnh đã chọn.") }
              catch (_: SecurityException) { vm.baoLoi("Chưa được cấp quyền đọc ảnh đã chọn.") }
        }
    }
    LaunchedEffect(link) { if (link != null) vm.nhanLienKet(link) }
    LaunchedEffect(state.loi, state.thongBao) {
        (state.loi ?: state.thongBao)?.let { snackbar.showSnackbar(it, duration = SnackbarDuration.Long) }
    }
    BackHandler(state.manHinh !in listOf(ManHinh.SPLASH, ManHinh.DANG_NHAP, ManHinh.TRANG_CHU, ManHinh.QUAN_TRI)) { vm.quayLai() }
    val title = when (state.manHinh) {
        ManHinh.DANG_KY -> "Đăng ký"
        ManHinh.QUEN_MAT_KHAU -> "Quên mật khẩu"
        ManHinh.DAT_LAI -> "Đặt lại mật khẩu"
        ManHinh.DOI_MAT_KHAU -> "Đổi mật khẩu"
        ManHinh.HO_SO -> if (isAdmin) "Hồ sơ quản trị" else "Thông tin cá nhân"
        ManHinh.SUA_HO_SO -> if (isAdmin) "Chỉnh sửa hồ sơ quản trị" else "Chỉnh sửa thông tin"
        ManHinh.CAI_DAT -> if (isAdmin) "Tài khoản quản trị" else "Cài đặt tài khoản"
        ManHinh.QUAN_TRI -> "Tổng quan"
        else -> null
    }
    if(state.manHinh==ManHinh.QUAN_TRI&&state.hoSo?.vaiTro?.contains("ADMIN")==true){
        QuanTriApp(admin,state.hoSo!!,loader,settings={vm.mo(ManHinh.CAI_DAT)},support={commerce.hoTroList(admin=true);vm.mo(ManHinh.TRANG_CHU)},logout=vm::dangXuat)
        return
    }
    if (state.manHinh == ManHinh.TRANG_CHU && state.hoSo != null) {
        val profile = state.hoSo!!
        if (trade.man != ManMuaBan.NONE) MuaBanApp(commerce, loader, profile, onTab = { index ->
            if (index == 3) commerce.hopThu() else {
                commerce.dong()
                exchange.dong()
                smart.close()
                when (index) {
                    0 -> listings.home()
                    1 -> exchange.khamPha()
                    2 -> listings.taoTin(profile.tinhThanh.orEmpty(), profile.diaChi.orEmpty())
                    4 -> vm.mo(ManHinh.CAI_DAT)
                }
            }
        }, onExit = { if (swap.man==ManTraoDoi.NONE&&profile.vaiTro.contains("ADMIN")) vm.taiTrangChinh() })
        else if(ss.screen!=SsScreen.NONE)SmartSwapApp(smart,loader,profile,onTab={index->
            smart.close();exchange.dong();when(index){0->listings.home();1->exchange.khamPha();2->listings.taoTin(profile.tinhThanh.orEmpty(),profile.diaChi.orEmpty());3->commerce.hopThu();4->vm.mo(ManHinh.CAI_DAT)}
        },onExit={if(swap.man==ManTraoDoi.NONE)vm.mo(ManHinh.CAI_DAT)})
        else if(swap.man!=ManTraoDoi.NONE)TraoDoiApp(exchange,loader,profile,onTab={index->
            if(index==1)exchange.khamPha()else{exchange.dong();when(index){0->listings.home();2->listings.taoTin(profile.tinhThanh.orEmpty(),profile.diaChi.orEmpty());3->commerce.hopThu();4->vm.mo(ManHinh.CAI_DAT)}}
        },onExit={if(profile.vaiTro.contains("ADMIN"))vm.taiTrangChinh()else listings.home()},onChat=commerce::docChat,onContact=commerce::moChat,
            onSupport={code->commerce.moHoTro();commerce.supportForm(title="Vấn đề giao nhận $code")},onSmart=smart::find)
        else BanHangApp(listings, loader, profile, account = { vm.mo(ManHinh.CAI_DAT) }, adminBack = { vm.taiTrangChinh() },
            contact = commerce::moChat, buy = { commerce.checkout(it, profile) }, inbox = { commerce.hopThu() }, exchange={exchange.khamPha()})
        return
    }
    Scaffold(containerColor = ReMarketPalette.Background,
        topBar = {
            if (title != null || state.manHinh == ManHinh.TRANG_CHU) {
                Row(Modifier.fillMaxWidth().background(ReMarketPalette.Background).statusBarsPadding().heightIn(min = 60.dp).padding(horizontal = 16.dp),
                    verticalAlignment = Alignment.CenterVertically, horizontalArrangement = Arrangement.spacedBy(12.dp)) {
                    when (state.manHinh) {
                        ManHinh.TRANG_CHU -> Brand()
                        ManHinh.QUAN_TRI -> {
                            IconButton(onClick = { vm.mo(ManHinh.CAI_DAT) }, Modifier.testTag("admin_menu")) { RmIcon(R.drawable.ic_rm_menu, description = "Tài khoản quản trị") }
                            Text(title.orEmpty(), style = MaterialTheme.typography.titleLarge)
                        }
                        else -> {
                            if (state.manHinh != ManHinh.CAI_DAT || isAdmin) IconButton(onClick = { vm.quayLai() }, enabled = !state.dangTai, modifier = Modifier.size(48.dp).testTag("toolbar_back")) {
                                RmIcon(R.drawable.ic_rm_arrow_left, size = 22.dp, description = "Quay lại")
                            }
                            Text(title.orEmpty(), Modifier.weight(1f), style = MaterialTheme.typography.titleLarge, maxLines = 2, overflow = TextOverflow.Ellipsis)
                        }
                    }
                }
            }
        },
        bottomBar = {
            if (state.manHinh == ManHinh.TRANG_CHU || (state.manHinh == ManHinh.CAI_DAT && state.hoSo?.vaiTro?.contains("ADMIN") != true))
                DieuHuongSanPham(4, !state.dangTai) { index ->
                    if(index!=4){exchange.dong();smart.close()}
                    when (index) {
                        0 -> listings.home()
                        1 -> exchange.khamPha()
                        2 -> listings.taoTin(state.hoSo?.tinhThanh.orEmpty(), state.hoSo?.diaChi.orEmpty())
                        3 -> commerce.hopThu()
                    }
                    if (index != 4) vm.mo(ManHinh.TRANG_CHU)
                }
        }, snackbarHost = { SnackbarHost(snackbar) }) { padding ->
        Box(Modifier.fillMaxSize().padding(padding).imePadding()) {
            when (state.manHinh) {
                ManHinh.SPLASH -> SplashScreen(state, vm)
                ManHinh.DANG_NHAP -> DangNhapScreen(state, vm)
                ManHinh.DANG_KY -> DangKyScreen(state, vm)
                ManHinh.QUEN_MAT_KHAU -> QuenMatKhauScreen(state, vm)
                ManHinh.DAT_LAI, ManHinh.DOI_MAT_KHAU -> MatKhauScreen(state, vm)
                ManHinh.HO_SO -> HoSoScreen(state, vm, loader)
                ManHinh.SUA_HO_SO -> SuaHoSoScreen(state, vm, loader) { picker.launch(PickVisualMediaRequest(ActivityResultContracts.PickVisualMedia.ImageOnly)) }
                ManHinh.CAI_DAT -> if (isAdmin) TaiKhoanQuanTriScreen(state, vm, loader) else CaiDatScreen(state, vm,
                    myListings = { commerce.dong(); exchange.dong();smart.close(); listings.tinToi(); vm.mo(ManHinh.TRANG_CHU) },
                    exchanges = { commerce.dong();smart.close(); exchange.deNghi();vm.mo(ManHinh.TRANG_CHU) },
                    smartSwaps={commerce.dong();exchange.dong();smart.list();vm.mo(ManHinh.TRANG_CHU)},
                    orders = { smart.close();commerce.donHang(); vm.mo(ManHinh.TRANG_CHU) },
                    support = { commerce.hoTroList(admin = state.hoSo?.vaiTro?.contains("ADMIN") == true); vm.mo(ManHinh.TRANG_CHU) })
                ManHinh.TRANG_CHU -> TrangChuScreen(state, vm)
                ManHinh.QUAN_TRI -> QuanTriScreen(state, vm,
                    review = { commerce.dong(); exchange.dong(); listings.choDuyet(); vm.mo(ManHinh.TRANG_CHU) },
                    support = { commerce.hoTroList(admin = true); vm.mo(ManHinh.TRANG_CHU) })
            }
            if (state.dangTai) LinearProgressIndicator(Modifier.fillMaxWidth().align(Alignment.TopCenter))
        }
    }
}

@Composable
private fun DieuHuongChinh(settings: Boolean, enabled: Boolean, home: () -> Unit, account: () -> Unit) {
    val items = listOf(R.drawable.ic_rm_house to "Trang chủ", R.drawable.ic_rm_arrow_left_right to "Trao đổi",
        R.drawable.ic_rm_circle_plus to "Đăng bán", R.drawable.ic_rm_message_circle to "Tin nhắn", R.drawable.ic_rm_user_round to "Cá nhân")
    Row(Modifier.fillMaxWidth().background(Color.White).navigationBarsPadding().height(67.dp).padding(horizontal = 4.dp, vertical = 8.dp)) {
        items.forEachIndexed { index, (icon, label) ->
            val selected = if (settings) index == 4 else index == 0
            Column(Modifier.weight(1f).testTag(if (index == 4) "settings_tab" else "tab_$index")
                .clickable(enabled = enabled && (index == 0 || index == 4)) { if (index == 4) account() else home() },
                horizontalAlignment = Alignment.CenterHorizontally, verticalArrangement = Arrangement.spacedBy(4.dp)) {
                Box(Modifier.size(52.dp, 28.dp).background(if (selected) ReMarketPalette.Soft else Color.Transparent, CircleShape), contentAlignment = Alignment.Center) {
                    RmIcon(icon, color = if (selected) ReMarketPalette.Primary else ReMarketPalette.Muted)
                }
                Text(label, style = rmText(10), color = if (selected) ReMarketPalette.Primary else ReMarketPalette.Muted)
            }
        }
    }
}
