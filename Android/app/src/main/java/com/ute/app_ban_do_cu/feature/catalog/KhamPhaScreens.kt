package com.ute.app_ban_do_cu.feature.catalog

import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.clickable
import androidx.compose.foundation.horizontalScroll
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.text.BasicTextField
import androidx.compose.foundation.text.KeyboardActions
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.draw.rotate
import androidx.compose.ui.focus.onFocusChanged
import androidx.compose.ui.platform.LocalSoftwareKeyboardController
import androidx.compose.ui.platform.testTag
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.text.input.ImeAction
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.unit.dp
import coil.ImageLoader
import com.ute.app_ban_do_cu.R
import com.ute.app_ban_do_cu.core.designsystem.*
import com.ute.app_ban_do_cu.domain.model.*

@Composable
fun HomeSanPhamScreen(state: BanHangState, vm: BanHangViewModel, loader: ImageLoader, exchange: () -> Unit = {}) {
    Column(Modifier.fillMaxSize().testTag("user_home").verticalScroll(rememberScrollState()).padding(horizontal = 24.dp, vertical = 12.dp),
        verticalArrangement = Arrangement.spacedBy(22.dp)) {
        TimKiemGon { vm.search() }
        Column(verticalArrangement = Arrangement.spacedBy(10.dp)) {
            TieuDeHang("Khám phá danh mục", "Tất cả", "all_categories") { vm.categories() }
            BoxWithConstraints(Modifier.fillMaxWidth()) {
                val tileWidth = ((maxWidth - 24.dp) / 4).coerceIn(60.dp, 88.dp)
                Row(Modifier.horizontalScroll(rememberScrollState()), horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                state.categories.filter { it.danhMucChaId == null }.forEach { category ->
                    Column(Modifier.width(tileWidth).clickable(enabled = !state.busy) { vm.search(category.id) },
                        horizontalAlignment = Alignment.CenterHorizontally, verticalArrangement = Arrangement.spacedBy(8.dp)) {
                        Box(Modifier.size(52.dp).background(ReMarketPalette.Soft, RoundedCornerShape(17.dp)), contentAlignment = Alignment.Center) { RmIcon(iconDanhMuc(category.ten,category.bieuTuong), size = 25.dp) }
                        Text(category.ten, Modifier.heightIn(min = 34.dp), style = rmText(12, FontWeight.Medium), maxLines = 2, overflow = TextOverflow.Ellipsis, textAlign = TextAlign.Center)
                    }
                }
            }
            }
        }
        val data = state.home
        Column(verticalArrangement = Arrangement.spacedBy(8.dp)) {
            TieuDeHang("Sản phẩm nổi bật", "Xem tất cả", "all_products") { vm.search() }
            if (data != null) {
                if (data.noiBat.isEmpty()) TrangRong("Chưa có tin đang bán", "Hãy đăng tin để món đồ bắt đầu hành trình mới.")
                else LuoiSanPham(data.noiBat.take(2), loader) { vm.chiTiet(it.id) }
            }
        }
        HomeSwapFeature(!state.busy, exchange)
        if (data != null) {
            Column(verticalArrangement = Arrangement.spacedBy(8.dp)) {
                TieuDeHang("Tin mới đăng", "Xem tất cả", "new_products") { vm.search() }
                data.moiDang.take(3).forEach { tin -> TheSanPham(tin, loader, compact = true) { vm.chiTiet(tin.id) } }
            }
        }
        RmButton("Tin của tôi", { vm.tinToi() }, Modifier.testTag("my_listings"), outlined = true)
        Spacer(Modifier.height(8.dp))
    }
}

@Composable
private fun HomeSwapFeature(enabled: Boolean, exchange: () -> Unit) {
    Row(Modifier.fillMaxWidth().background(ReMarketPalette.Soft, RoundedCornerShape(20.dp))
        .clickable(enabled = enabled, onClick = exchange).testTag("home_smart_swap").padding(16.dp),
        horizontalArrangement = Arrangement.spacedBy(12.dp), verticalAlignment = Alignment.CenterVertically) {
        RmIconTile(R.drawable.ic_rm_arrow_left_right)
        Column(Modifier.weight(1f), verticalArrangement = Arrangement.spacedBy(4.dp)) {
            Text("Khám phá Smart Swap", style = rmText(15, FontWeight.SemiBold), color = ReMarketPalette.PrimaryDark)
            Text("Ba món đồ, thêm cơ hội trao đổi.", style = rmText(12), color = ReMarketPalette.Muted)
        }
        RmIcon(R.drawable.ic_rm_chevron_right, size = 18.dp)
    }
}

@Composable
private fun TieuDeHang(title: String, action: String, tag: String, click: () -> Unit) {
    Row(Modifier.fillMaxWidth(), verticalAlignment = Alignment.CenterVertically, horizontalArrangement = Arrangement.SpaceBetween) {
        Text(title, Modifier.weight(1f), style = rmText(18, FontWeight.SemiBold))
        TextButton(click, Modifier.testTag(tag), contentPadding = PaddingValues(start = 12.dp, end = 0.dp)) {
            Text(action, style = rmText(12, FontWeight.Medium))
            RmIcon(R.drawable.ic_rm_chevron_right, size = 14.dp)
        }
    }
}

@Composable
private fun TimKiemGon(click: () -> Unit) {
    Row(Modifier.fillMaxWidth().heightIn(min = 54.dp).background(ReMarketPalette.SurfaceMuted, RoundedCornerShape(16.dp))
        .border(1.dp, ReMarketPalette.Border, RoundedCornerShape(16.dp)).clickable(onClick = click).testTag("open_search").padding(horizontal = 16.dp, vertical = 12.dp),
        verticalAlignment = Alignment.CenterVertically, horizontalArrangement = Arrangement.spacedBy(10.dp)) {
            RmIcon(R.drawable.ic_rm_search, size = 18.dp)
            Text("Tìm món đồ bạn cần", style = rmText(14), color = ReMarketPalette.Muted)
    }
}

@Composable
fun DanhMucScreen(state: BanHangState, vm: BanHangViewModel) {
    Column(Modifier.fillMaxSize().testTag("categories").verticalScroll(rememberScrollState()).padding(24.dp), verticalArrangement = Arrangement.spacedBy(20.dp)) {
        TimKiemGon { vm.search() }
        Text("Bạn đang tìm gì?", style = rmText(24, FontWeight.Bold))
        Text("Chọn danh mục để khám phá những món đồ còn nhiều giá trị.", style = rmText(14), color = ReMarketPalette.Muted)
        val roots = state.categories.filter { it.danhMucChaId == null }
        BoxWithConstraints {
            val columns = if (maxWidth >= 560.dp) 3 else 2
            Column(verticalArrangement = Arrangement.spacedBy(12.dp)) {
                roots.chunked(columns).forEach { row -> Row(horizontalArrangement = Arrangement.spacedBy(12.dp)) {
                    row.forEach { category -> RmCard(Modifier.weight(1f).clickable(enabled = !state.busy) { vm.search(category.id) }.testTag("category_${category.id}")) {
                        Box(Modifier.size(44.dp).background(ReMarketPalette.Soft, RoundedCornerShape(12.dp)), contentAlignment = Alignment.Center) {
                            RmIcon(iconDanhMuc(category.ten,category.bieuTuong), size = 30.dp)
                        }
                        Spacer(Modifier.height(14.dp))
                        Text(category.ten, style = rmText(15, FontWeight.SemiBold))
                        Text("${category.soTin} món đồ", style = rmText(12), color = ReMarketPalette.Primary)
                    } }
                    repeat(columns - row.size) { Spacer(Modifier.weight(1f)) }
                } }
            }
        }
        if (roots.isEmpty() && !state.busy) TrangRong("Chưa có danh mục", "Quay lại sau để khám phá thêm nhé.")
        RmNotice("Mỗi món đồ cũ là một hành trình mới đang chờ bạn.")
    }
}

@Composable
fun DanhSachScreen(state: BanHangState, vm: BanHangViewModel, loader: ImageLoader) {
    var showFilter by remember { mutableStateOf(false) }
    var searchFocused by remember { mutableStateOf(false) }
    val keyboard = LocalSoftwareKeyboardController.current
    val filter = state.filter
    Column(Modifier.fillMaxSize().testTag("catalog_results").verticalScroll(rememberScrollState()).padding(24.dp), verticalArrangement = Arrangement.spacedBy(16.dp)) {
        Row(Modifier.fillMaxWidth().heightIn(min = 54.dp).background(Color.White, RoundedCornerShape(16.dp))
            .border(1.dp, if (searchFocused) ReMarketPalette.Primary else ReMarketPalette.Border, RoundedCornerShape(16.dp)).padding(start = 16.dp), verticalAlignment = Alignment.CenterVertically) {
            BasicTextField(filter.tuKhoa, vm::tuKhoa, Modifier.weight(1f).onFocusChanged { searchFocused = it.isFocused }.testTag("catalog_query"), singleLine = true, textStyle = rmText(14).copy(color = ReMarketPalette.Text),
                keyboardOptions = KeyboardOptions(imeAction = ImeAction.Search), keyboardActions = KeyboardActions(onSearch = { keyboard?.hide(); vm.tim() }),
                decorationBox = { inner -> Box { if (filter.tuKhoa.isEmpty()) Text("Tìm món đồ bạn cần", style = rmText(14), color = ReMarketPalette.Muted); inner() } })
            if (filter.tuKhoa.isNotEmpty()) IconButton({ vm.tuKhoa(""); keyboard?.hide(); vm.tim() }, enabled = !state.busy,
                modifier = Modifier.testTag("catalog_clear")) { RmIcon(R.drawable.ic_rm_circle_plus, Modifier.rotate(45f), size = 18.dp, color = ReMarketPalette.Muted, description = "Xóa từ khóa") }
            IconButton({ keyboard?.hide(); vm.tim() }, enabled = !state.busy, modifier = Modifier.testTag("catalog_search")) { RmIcon(R.drawable.ic_rm_search, description = "Tìm kiếm") }
        }
        Row(Modifier.horizontalScroll(rememberScrollState()), horizontalArrangement = Arrangement.spacedBy(8.dp)) {
            FilterChip(filter.danhMucId != null, { showFilter = true }, label = { Text(state.categories.firstOrNull { it.id == filter.danhMucId }?.ten ?: "Danh mục", style = rmText(12)) })
            FilterChip(filter.giaTu != null || filter.giaDen != null, { showFilter = true }, label = { Text("Khoảng giá", style = rmText(12)) })
            FilterChip(filter.tinhTrang != null, { showFilter = true }, label = { Text(filter.tinhTrang?.let(::tinhTrangText) ?: "Tình trạng", style = rmText(12)) })
            FilterChip(filter.tinhThanh.isNotBlank(), { showFilter = true }, label = { Text(filter.tinhThanh.ifBlank { "Khu vực" }, style = rmText(12)) }, modifier = Modifier.testTag("open_filters"))
        }
        Row(Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween, verticalAlignment = Alignment.CenterVertically) {
            Text(if (state.results == null && state.busy) "Đang tìm sản phẩm…" else "${state.results?.tongSo ?: 0} sản phẩm", Modifier.weight(1f), style = rmText(14, FontWeight.SemiBold))
            var expanded by remember { mutableStateOf(false) }
            val sorts = listOf("moinhat" to "Mới nhất", "giatang" to "Giá tăng dần", "giagiam" to "Giá giảm dần")
            Box {
                TextButton({ expanded = true }, enabled = !state.busy, modifier = Modifier.testTag("catalog_sort")) { Text(sorts.first { it.first == filter.sapXep }.second, style = rmText(12)) }
                DropdownMenu(expanded, { expanded = false }) { sorts.forEach { (key, title) ->
                    DropdownMenuItem({ Text(title) }, { expanded = false; vm.tim(filter.copy(sapXep = key)) }, modifier = Modifier.testTag("sort_$key"))
                } }
            }
        }
        val page = state.results
        if (page != null) {
            if (page.items.isEmpty()) TrangRong("Không tìm thấy sản phẩm", "Thử thay đổi từ khóa hoặc mở rộng bộ lọc.", Modifier.testTag("catalog_empty"),
                { vm.tim(BoLocSanPham(loaiTin = filter.loaiTin)) }, "Xóa bộ lọc")
            else {
                LuoiSanPham(page.items, loader) { vm.chiTiet(it.id) }
                PhanTrang(page.trang, page.tongTrang, !state.busy, { vm.trang(page.trang - 1) }, { vm.trang(page.trang + 1) })
                if (page.trang == page.tongTrang) Text("Bạn đã xem hết kết quả", style = rmText(13), color = ReMarketPalette.Muted)
            }
        }
    }
    if (showFilter) BoLocSheet(state, { showFilter = false }) { value -> showFilter = false; vm.tim(value) }
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
private fun BoLocSheet(state: BanHangState, dismiss: () -> Unit, apply: (BoLocSanPham) -> Unit) {
    var category by remember { mutableStateOf(state.filter.danhMucId.orEmpty()) }
    var min by remember { mutableStateOf(state.filter.giaTu?.toString().orEmpty()) }
    var max by remember { mutableStateOf(state.filter.giaDen?.toString().orEmpty()) }
    var condition by remember { mutableStateOf(state.filter.tinhTrang.orEmpty()) }
    var city by remember { mutableStateOf(state.filter.tinhThanh) }
    var error by remember { mutableStateOf<String?>(null) }
    ModalBottomSheet(dismiss, sheetState = rememberModalBottomSheetState(skipPartiallyExpanded = true), containerColor = ReMarketPalette.Background) {
        Column(Modifier.fillMaxWidth().verticalScroll(rememberScrollState()).imePadding().padding(horizontal = 24.dp).padding(bottom = 24.dp), verticalArrangement = Arrangement.spacedBy(16.dp)) {
            RmSectionTitle("Bộ lọc sản phẩm", "Tìm món đồ phù hợp với nhu cầu của bạn.", R.drawable.ic_rm_search)
            LuaChon("Danh mục", category, listOf("" to "Tất cả danh mục") + state.categories.map { it.id to it.ten }, { category = it }, tag = "filter_category")
            Row(horizontalArrangement = Arrangement.spacedBy(12.dp)) {
                Box(Modifier.weight(1f)) { RmField("Giá từ (VND)", min, { min = it.filter(Char::isDigit).take(13) }, "0", "filter_min", keyboard = KeyboardType.Number) }
                Box(Modifier.weight(1f)) { RmField("Giá đến (VND)", max, { max = it.filter(Char::isDigit).take(13) }, "Không giới hạn", "filter_max", keyboard = KeyboardType.Number) }
            }
            LuaChon("Tình trạng", condition, listOf("" to "Tất cả tình trạng") + tinhTrangOptions, { condition = it }, tag = "filter_condition")
            RmField("Tỉnh / Thành phố", city, { city = it.take(100) }, "Tất cả khu vực", "filter_city")
            error?.let { Text(it, color = ReMarketPalette.Error, style = rmText(12)) }
            RmButton("Áp dụng", {
                val lower = min.toLongOrNull(); val upper = max.toLongOrNull()
                if ((lower ?: 0) > (upper ?: 1_000_000_000_000) || (lower ?: 0) > 1_000_000_000_000 || (upper ?: 0) > 1_000_000_000_000)
                    error = "Khoảng giá không hợp lệ (tối đa 1.000 tỷ đồng)."
                else apply(state.filter.copy(danhMucId = category.ifBlank { null }, giaTu = lower, giaDen = upper, tinhTrang = condition.ifBlank { null }, tinhThanh = city.trim()))
            }, Modifier.testTag("filter_apply"))
            RmLink("Đặt lại bộ lọc", { category = ""; min = ""; max = ""; condition = ""; city = ""; error = null })
        }
    }
}
