package com.ute.app_ban_do_cu.feature.catalog

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
import androidx.compose.ui.unit.dp
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.text.style.TextOverflow
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import coil.ImageLoader
import com.ute.app_ban_do_cu.R
import com.ute.app_ban_do_cu.core.designsystem.*
import com.ute.app_ban_do_cu.domain.model.HoSo
import com.ute.app_ban_do_cu.feature.auth.Brand
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch
import kotlinx.coroutines.withContext
import java.io.ByteArrayOutputStream
import java.io.IOException

@Composable
fun BanHangApp(vm: BanHangViewModel, loader: ImageLoader, profile: HoSo, account: () -> Unit, adminBack: () -> Unit,
    contact: (String) -> Unit, buy: (String) -> Unit, inbox: () -> Unit, exchange: () -> Unit) {
    val state by vm.uiState.collectAsStateWithLifecycle()
    val context = LocalContext.current
    val scope = rememberCoroutineScope()
    val snack = remember { SnackbarHostState() }
    var confirmation by remember { mutableStateOf<String?>(null) }
    var reading by remember { mutableStateOf(false) }
    val picker = rememberLauncherForActivityResult(ActivityResultContracts.PickMultipleVisualMedia(8)) { uris ->
        if (uris.isNotEmpty()) scope.launch {
            reading = true
            try {
                val photos = withContext(Dispatchers.IO) { uris.map { uri ->
                    val bytes = context.contentResolver.openInputStream(uri)?.use { input ->
                        val output = ByteArrayOutputStream(); val buffer = ByteArray(16 * 1024)
                        while (true) {
                            val read = input.read(buffer); if (read < 0) break
                            if (output.size() + read > 5 * 1024 * 1024) throw IOException("Mỗi ảnh tối đa 5 MB.")
                            output.write(buffer, 0, read)
                        }
                        output.toByteArray()
                    } ?: throw IOException("Không đọc được ảnh đã chọn.")
                    AnhChon(uri.toString(), bytes)
                } }
                vm.themAnh(photos)
            } catch (ex: IOException) { vm.loi(ex.message ?: "Không đọc được ảnh.") }
              catch (_: SecurityException) { vm.loi("Chưa có quyền đọc ảnh đã chọn.") }
            finally { reading = false }
        }
    }
    LaunchedEffect(Unit) { if (state.screen == ManSanPham.HOME && state.home == null) vm.home() }
    LaunchedEffect(state.notice) { state.notice?.let { snack.showSnackbar(it) } }
    val newListing = { vm.taoTin(profile.tinhThanh.orEmpty(), profile.diaChi.orEmpty()) }
    val roots = listOf(ManSanPham.HOME, ManSanPham.EXCHANGE, ManSanPham.INBOX)
    BackHandler(state.screen !in roots) { if (!state.busy && !reading) { if (state.screen == ManSanPham.REVIEW) adminBack() else vm.back() } }
    BackHandler(state.screen in listOf(ManSanPham.EXCHANGE, ManSanPham.INBOX)) { if (!state.busy) vm.home() }
    val title = when (state.screen) {
        ManSanPham.HOME -> "ReMarket"
        ManSanPham.CATEGORIES -> "Danh mục sản phẩm"
        ManSanPham.SEARCH -> "Tìm kiếm sản phẩm"
        ManSanPham.LIST -> state.categories.firstOrNull { it.id == state.filter.danhMucId }?.ten ?: "Danh sách sản phẩm"
        ManSanPham.EXCHANGE -> "Khám phá trao đổi"
        ManSanPham.DETAIL -> "Chi tiết sản phẩm"
        ManSanPham.FORM -> if (state.form.id == null) "Đăng bán sản phẩm" else "Chỉnh sửa tin"
        ManSanPham.PREVIEW -> "Xem trước tin đăng"
        ManSanPham.SUCCESS -> "Gửi tin thành công"
        ManSanPham.MINE -> "Quản lý tin"
        ManSanPham.MANAGE -> "Chi tiết quản lý tin"
        ManSanPham.SOLD_CONFIRM -> "Xác nhận đã bán"
        ManSanPham.INBOX -> "Tin nhắn"
        ManSanPham.REVIEW -> "Kiểm duyệt tin đăng"
    }
    val enabled = !state.busy && !reading
    Scaffold(containerColor = ReMarketPalette.Background,
        topBar = {
            Row(Modifier.fillMaxWidth().statusBarsPadding().heightIn(min = 60.dp).padding(horizontal = 16.dp), verticalAlignment = Alignment.CenterVertically) {
                if (state.screen == ManSanPham.HOME) Brand() else {
                    if (state.screen !in listOf(ManSanPham.EXCHANGE, ManSanPham.INBOX)) IconButton({ if (state.screen == ManSanPham.REVIEW) adminBack() else vm.back() }, enabled = enabled,
                        modifier = Modifier.size(48.dp).testTag("catalog_back")) { RmIcon(R.drawable.ic_rm_arrow_left, description = "Quay lại") }
                    Text(title, Modifier.weight(1f).padding(start = 4.dp), style = MaterialTheme.typography.titleLarge, maxLines = 2, overflow = TextOverflow.Ellipsis)
                }
                if (state.screen == ManSanPham.HOME) {
                    Spacer(Modifier.width(8.dp))
                    Text("Đồ cũ, hành trình mới", Modifier.weight(1f), style = rmText(11), color = ReMarketPalette.Muted, textAlign = TextAlign.End, maxLines = 2, overflow = TextOverflow.Ellipsis)
                }
                if (state.screen == ManSanPham.MANAGE) IconButton(vm::taiChiTiet, enabled = enabled, modifier = Modifier.testTag("reload_listing")) { RmIcon(R.drawable.ic_rm_eye, description = "Tải lại tin") }
            }
        },
        bottomBar = {
            when (state.screen) {
                ManSanPham.HOME, ManSanPham.EXCHANGE, ManSanPham.INBOX, ManSanPham.MINE -> DieuHuongSanPham(
                    if (state.screen == ManSanPham.EXCHANGE) 1 else if (state.screen == ManSanPham.INBOX) 3 else if (state.screen == ManSanPham.MINE) 2 else 0, enabled) { index ->
                    when (index) { 0 -> vm.home(); 1 -> exchange(); 2 -> newListing(); 3 -> inbox(); 4 -> account() }
                }
                ManSanPham.FORM -> ChanTrang {
                    RmButton(if (state.detail?.trangThai in listOf("DangHienThi", "ChoDuyet")) "Lưu thay đổi" else "Xem trước tin", {
                        vm.luu(state.detail?.trangThai !in listOf("DangHienThi", "ChoDuyet"))
                    }, Modifier.testTag("listing_preview_submit"), enabled = enabled, loading = state.busy || reading)
                }
                ManSanPham.PREVIEW -> ChanTrang {
                    RmButton("Xác nhận đăng tin", vm::guiDuyet, Modifier.testTag("listing_publish"), enabled = enabled, loading = state.busy)
                    RmLink("Quay lại chỉnh sửa", vm::suaPreview, Modifier.fillMaxWidth(), enabled)
                }
                ManSanPham.SUCCESS -> ChanTrang {
                    RmButton("Về quản lý tin", vm::tinToi, Modifier.testTag("success_my_listings"), enabled = enabled)
                    RmButton("Đăng thêm tin", newListing, enabled = enabled, outlined = true)
                }
                ManSanPham.MANAGE -> state.detail?.let { tin -> ChanTrang {
                    Row(horizontalArrangement = Arrangement.spacedBy(10.dp)) {
                        RmButton("Chỉnh sửa", vm::suaTin, Modifier.weight(1f).testTag("edit_listing"), enabled = enabled && tin.coTheChinhSua, outlined = true)
                        if (tin.trangThai in listOf("DangHienThi", "DaAn")) RmButton("Đã bán", vm::xacNhanBan, Modifier.weight(1f).testTag("mark_sold"), enabled = enabled)
                        else if (tin.trangThai in listOf("BanNhap", "TuChoi")) RmButton("Gửi duyệt", vm::guiDuyet, Modifier.weight(1f).testTag("send_review"), enabled = enabled)
                    }
                    Row(Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceEvenly) {
                        if (tin.trangThai in listOf("DangHienThi", "ChoDuyet")) RmLink("Ẩn tin", { confirmation = "hide" }, Modifier.testTag("hide_listing"), enabled)
                        if (tin.trangThai == "DaAn") RmLink("Gửi duyệt lại", vm::hienLai, Modifier.testTag("restore_listing"), enabled)
                        RmLink("Xóa tin", { confirmation = "delete" }, Modifier.testTag("delete_listing"), enabled)
                    }
                } }
                ManSanPham.SOLD_CONFIRM -> ChanTrang {
                    RmButton("Xác nhận đã bán", vm::daBan, Modifier.testTag("sold_confirm"), enabled = enabled, loading = state.busy)
                    RmButton("Hủy · Tiếp tục bán", vm::back, outlined = true, enabled = enabled)
                }
                ManSanPham.DETAIL -> state.detail?.let { tin -> ChanTrang {
                    Row(horizontalArrangement = Arrangement.spacedBy(10.dp)) {
                        RmButton("Liên hệ", { contact(tin.id) }, Modifier.weight(1f).testTag("contact_seller"), outlined = true, enabled = enabled && tin.nguoiDangId != profile.id)
                        if (tin.loaiTin in listOf("MuaBan", "CaHai")) RmButton("Mua ngay", { buy(tin.id) }, Modifier.weight(1f).testTag("buy_product"), enabled = enabled && tin.nguoiDangId != profile.id && tin.trangThai == "DangHienThi")
                    }
                } }
                else -> Unit
            }
        }, snackbarHost = { SnackbarHost(snack) }) { padding ->
        Column(Modifier.fillMaxSize().padding(padding).consumeWindowInsets(padding).imePadding()) {
            if (state.busy || reading) LinearProgressIndicator(Modifier.fillMaxWidth().testTag("catalog_loading"))
            state.progress?.let { Text(it, Modifier.padding(horizontal = 24.dp, vertical = 6.dp), style = rmText(12)) }
            state.error?.let { error -> Row(Modifier.fillMaxWidth().background(ReMarketPalette.Warm).padding(horizontal = 20.dp, vertical = 8.dp), verticalAlignment = Alignment.CenterVertically) {
                Text(error, Modifier.weight(1f).testTag("catalog_error"), style = rmText(12), color = ReMarketPalette.Error)
                TextButton(vm::thuLai, enabled = enabled) { Text("Thử lại", style = rmText(12)) }
            } }
            Box(Modifier.weight(1f)) {
                when (state.screen) {
                    ManSanPham.HOME -> HomeSanPhamScreen(state, vm, loader, exchange)
                    ManSanPham.CATEGORIES -> DanhMucScreen(state, vm)
                    ManSanPham.SEARCH, ManSanPham.LIST, ManSanPham.EXCHANGE -> DanhSachScreen(state, vm, loader)
                    ManSanPham.DETAIL, ManSanPham.MANAGE, ManSanPham.PREVIEW -> ChiTietTinScreen(state, vm, loader, profile.id)
                    ManSanPham.FORM -> FormTinScreen(state, vm, loader) { picker.launch(PickVisualMediaRequest(ActivityResultContracts.PickVisualMedia.ImageOnly)) }
                    ManSanPham.SUCCESS -> GuiThanhCongScreen(state, loader)
                    ManSanPham.SOLD_CONFIRM -> XacNhanBanScreen(state, loader)
                    ManSanPham.MINE -> TinCuaToiScreen(state, vm, loader, newListing)
                    ManSanPham.INBOX -> HopThuScreen(state)
                    ManSanPham.REVIEW -> DuyetTinScreen(state, vm, loader)
                }
            }
        }
    }
    if (confirmation != null) AlertDialog(onDismissRequest = { confirmation = null },
        title = { Text(if (confirmation == "hide") "Ẩn tin này?" else "Xóa tin này?") },
        text = { Text(if (confirmation == "hide") "Tin sẽ ngừng hiển thị với người mua. Bạn có thể gửi duyệt lại từ Quản lý tin." else "Tin sẽ biến mất khỏi danh sách của bạn và người mua. Lịch sử giao dịch liên quan được giữ lại.") },
        confirmButton = { TextButton({ val action = confirmation; confirmation = null; if (action == "hide") vm.anTin() else vm.xoaTin() }, modifier = Modifier.testTag("listing_action_confirm")) { Text("Xác nhận") } },
        dismissButton = { TextButton({ confirmation = null }) { Text("Hủy") } })
}

@Composable
private fun ChanTrang(content: @Composable ColumnScope.() -> Unit) {
    RmActionBar(content = content)
}

@Composable
fun DieuHuongSanPham(selected: Int, enabled: Boolean, onTab: (Int) -> Unit) {
    val items = listOf(R.drawable.ic_rm_house to "Trang chủ", R.drawable.ic_rm_arrow_left_right to "Trao đổi", R.drawable.ic_rm_circle_plus to "Đăng bán",
        R.drawable.ic_rm_message_circle to "Tin nhắn", R.drawable.ic_rm_user_round to "Cá nhân")
    NavigationBar(containerColor = Color.White, tonalElevation = 0.dp) {
        items.forEachIndexed { index, (icon, label) ->
            NavigationBarItem(selected = selected == index, onClick = { onTab(index) }, enabled = enabled,
                modifier = Modifier.testTag(if (index == 4) "settings_tab" else "tab_$index"),
                icon = { RmIcon(icon, size = 23.dp, color = if (selected == index) ReMarketPalette.Primary else ReMarketPalette.Muted) },
                label = { Text(label, style = rmText(11, if (selected == index) FontWeight.SemiBold else FontWeight.Medium), maxLines = 2, textAlign = TextAlign.Center) },
                colors = NavigationBarItemDefaults.colors(indicatorColor = ReMarketPalette.Soft,
                    selectedTextColor = ReMarketPalette.Primary, unselectedTextColor = ReMarketPalette.Muted))
        }
    }
}
