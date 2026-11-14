package com.ute.app_ban_do_cu.feature.catalog

import androidx.compose.foundation.clickable
import androidx.compose.foundation.horizontalScroll
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.testTag
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import coil.ImageLoader
import com.ute.app_ban_do_cu.core.designsystem.*
import com.ute.app_ban_do_cu.domain.model.TinDang

@Composable
fun TinCuaToiScreen(state: BanHangState, vm: BanHangViewModel, loader: ImageLoader, newListing: () -> Unit) {
    Column(Modifier.fillMaxSize().testTag("my_listings_screen").verticalScroll(rememberScrollState()).padding(24.dp), verticalArrangement = Arrangement.spacedBy(16.dp)) {
        state.mine?.thongKe?.let { stats -> Row(horizontalArrangement = Arrangement.spacedBy(12.dp)) {
            RmCard(Modifier.weight(1f)) {
                Text("Tin của bạn", style = rmText(12), color = ReMarketPalette.Muted)
                Text(stats.sumOf { it.soTin }.toString(), style = rmText(28, FontWeight.Bold), color = ReMarketPalette.Primary)
            }
            RmCard(Modifier.weight(1f)) {
                Text("Chờ duyệt", style = rmText(12), color = ReMarketPalette.Muted)
                Text((stats.firstOrNull { it.trangThai == "ChoDuyet" }?.soTin ?: 0).toString(), style = rmText(28, FontWeight.Bold), color = ReMarketPalette.Warning)
            }
        } }
        RmButton("Đăng tin mới", newListing, Modifier.testTag("new_listing"), enabled = !state.busy)
        Row(Modifier.horizontalScroll(rememberScrollState()), horizontalArrangement = Arrangement.spacedBy(8.dp)) {
            val stats = state.mine?.thongKe.orEmpty()
            FilterChip(state.mineStatus == null, { vm.taiTinToi(null) }, enabled = !state.busy, label = { Text("Tất cả ${stats.sumOf { it.soTin }}", style = rmText(11)) }, modifier = Modifier.testTag("status_all"))
            trangThaiOptions.forEach { (status, label) -> FilterChip(state.mineStatus == status, { vm.taiTinToi(status) }, enabled = !state.busy,
                label = { Text("$label ${stats.firstOrNull { it.trangThai == status }?.soTin ?: 0}", style = rmText(11)) }, modifier = Modifier.testTag("status_$status")) }
        }
        val page = state.mine?.danhSach
        Text("${state.mineStatus?.let(::trangThaiText) ?: "Tất cả tin"} · ${page?.tongSo ?: 0} tin", style = rmText(14, FontWeight.Medium))
        page?.items?.forEach { tin ->
            Column(Modifier.testTag("manage_${tin.id}"), verticalArrangement = Arrangement.spacedBy(6.dp)) {
                TheSanPham(tin, loader, compact = true) { vm.chiTiet(tin.id, true) }
                Row(Modifier.fillMaxWidth().padding(horizontal = 8.dp), horizontalArrangement = Arrangement.SpaceBetween) {
                    NhanTrangThai(tin.trangThai)
                    TextButton({ vm.chiTiet(tin.id, true) }, enabled = !state.busy) { Text("Quản lý ›", style = rmText(12)) }
                }
            }
        }
        if (page != null) {
            if (page.items.isEmpty()) TrangRong("Chưa có tin trong mục này", "Đăng món đồ bạn muốn tìm chủ mới.", Modifier.testTag("my_listings_empty"))
            PhanTrang(page.trang, page.tongTrang, !state.busy, { vm.taiTinToi(page = page.trang - 1) }, { vm.taiTinToi(page = page.trang + 1) })
        }
    }
}

@Composable
fun HopThuScreen(state: BanHangState) {
    Column(Modifier.fillMaxSize().testTag("inbox").verticalScroll(rememberScrollState()).padding(24.dp), verticalArrangement = Arrangement.spacedBy(16.dp)) {
        Text("Hộp thư của bạn", style = rmText(20, FontWeight.SemiBold))
        val items = state.inbox
        if (items != null && items.isEmpty()) TrangRong("Chưa có hội thoại", "Các cuộc trò chuyện của bạn sẽ xuất hiện tại đây.")
        items?.forEach { item -> RmCard {
            Text(item.tieuDe, style = rmText(15, FontWeight.SemiBold))
            Spacer(Modifier.height(8.dp))
            Text(item.tinCuoi ?: "Chưa có tin nhắn", style = rmText(13), color = ReMarketPalette.Muted)
        } }
        if (!items.isNullOrEmpty()) RmNotice("Danh sách hội thoại được lấy từ tài khoản của bạn. Soạn tin và chat realtime sẽ được triển khai ở giai đoạn chat.")
    }
}

@Composable
fun DuyetTinScreen(state: BanHangState, vm: BanHangViewModel, loader: ImageLoader) {
    var selected by remember { mutableStateOf<TinDang?>(null) }
    var accept by remember { mutableStateOf(true) }
    var reason by remember { mutableStateOf("") }
    Column(Modifier.fillMaxSize().testTag("review_queue").verticalScroll(rememberScrollState()).padding(24.dp), verticalArrangement = Arrangement.spacedBy(18.dp)) {
        Text("Tin chờ duyệt · ${state.review?.tongSo ?: 0}", style = rmText(17, FontWeight.SemiBold))
        state.review?.items?.forEach { tin -> RmCard {
            Text(tin.nguoiDang, style = rmText(12), color = ReMarketPalette.Muted)
            Spacer(Modifier.height(10.dp))
            TheSanPham(tin, loader, compact = true, onClick = {})
            Spacer(Modifier.height(10.dp))
            Text(tin.danhMuc + " · " + tinhTrangText(tin.tinhTrang), style = rmText(12))
            Text(tin.moTa, style = rmText(13))
            Spacer(Modifier.height(12.dp))
            Row(horizontalArrangement = Arrangement.spacedBy(10.dp)) {
                RmButton("Từ chối", { selected = tin; accept = false; reason = "" }, Modifier.weight(1f), enabled = !state.busy, outlined = true)
                RmButton("Duyệt tin", { selected = tin; accept = true }, Modifier.weight(1f).testTag("approve_${tin.id}"), enabled = !state.busy)
            }
        } }
        val page = state.review
        if (page != null && page.items.isEmpty()) TrangRong("Đã xử lý hết tin chờ duyệt", "Các tin mới được gửi sẽ xuất hiện ở đây.")
        if (page != null) PhanTrang(page.trang, page.tongTrang, !state.busy, { vm.choDuyet(page.trang - 1) }, { vm.choDuyet(page.trang + 1) })
    }
    selected?.let { tin -> AlertDialog(onDismissRequest = { selected = null }, title = { Text(if (accept) "Duyệt tin này?" else "Từ chối tin") },
        text = { Column(verticalArrangement = Arrangement.spacedBy(12.dp)) {
            Text(tin.ten)
            if (!accept) RmField("Lý do", reason, { reason = it.take(1000) }, "Nhập lý do từ chối", "review_reason", multiline = true)
            else Text("Tin sẽ hiển thị với người mua sau khi xác nhận.")
        } },
        confirmButton = { TextButton({ selected = null; vm.duyet(tin, accept, reason) }, enabled = accept || reason.isNotBlank(), modifier = Modifier.testTag("review_confirm")) { Text("Xác nhận") } },
        dismissButton = { TextButton({ selected = null }) { Text("Hủy") } }) }
}
