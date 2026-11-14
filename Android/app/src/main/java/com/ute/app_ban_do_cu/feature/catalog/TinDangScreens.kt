package com.ute.app_ban_do_cu.feature.catalog

import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.clickable
import androidx.compose.foundation.horizontalScroll
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.pager.HorizontalPager
import androidx.compose.foundation.pager.rememberPagerState
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.platform.testTag
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.unit.dp
import androidx.compose.ui.text.style.TextAlign
import coil.ImageLoader
import coil.compose.AsyncImage
import com.ute.app_ban_do_cu.R
import com.ute.app_ban_do_cu.core.designsystem.*
import com.ute.app_ban_do_cu.domain.model.*

@Composable
fun FormTinScreen(state: BanHangState, vm: BanHangViewModel, loader: ImageLoader, pick: () -> Unit) {
    val f = state.form
    Column(Modifier.fillMaxSize().testTag("listing_form").verticalScroll(rememberScrollState()).padding(24.dp), verticalArrangement = Arrangement.spacedBy(16.dp)) {
        Row(Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.spacedBy(12.dp), verticalAlignment = Alignment.CenterVertically) {
            Box(Modifier.weight(1f)) { RmSectionTitle("Ảnh sản phẩm", "Chọn ảnh rõ nét, đúng với món đồ.") }
            RmBadge("${f.anh.size + f.anhChon.size} / 8 ảnh", tone = RmTone.NEUTRAL)
        }
        Row(Modifier.horizontalScroll(rememberScrollState()), horizontalArrangement = Arrangement.spacedBy(10.dp)) {
            f.anh.sortedByDescending { it.laAnhBia }.forEach { photo ->
                Column(Modifier.width(104.dp), horizontalAlignment = Alignment.CenterHorizontally) {
                    Box {
                        AnhSanPham(photo.url, loader, Modifier.size(104.dp).clip(RoundedCornerShape(16.dp)))
                        IconButton({ vm.xoaAnh(photo.id) }, enabled = !state.busy, modifier = Modifier.size(32.dp).align(Alignment.TopEnd).background(Color.White, CircleShape).testTag("remove_photo_${photo.id}")) {
                            RmIcon(R.drawable.ic_rm_trash_2, size = 16.dp, description = "Xóa ảnh")
                        }
                    }
                    TextButton({ vm.anhBia(photo.id) }, enabled = !state.busy && !photo.laAnhBia, contentPadding = PaddingValues(0.dp)) {
                        Text(if (photo.laAnhBia) "Ảnh bìa" else "Đặt làm ảnh bìa", style = rmText(10))
                    }
                }
            }
            f.anhChon.forEachIndexed { index, photo ->
                Column(Modifier.width(104.dp), horizontalAlignment = Alignment.CenterHorizontally) {
                    Box {
                        AsyncImage(photo.bytes, "Ảnh đã chọn", imageLoader = loader, contentScale = ContentScale.Crop, modifier = Modifier.size(104.dp).clip(RoundedCornerShape(16.dp)))
                        IconButton({ vm.boAnhChon(photo.uri) }, enabled = !state.busy, modifier = Modifier.size(32.dp).align(Alignment.TopEnd).background(Color.White, CircleShape)) { RmIcon(R.drawable.ic_rm_trash_2, size = 16.dp, description = "Bỏ ảnh đã chọn") }
                    }
                    TextButton({ vm.anhBiaChon(photo.uri) }, enabled = !state.busy && index != 0, contentPadding = PaddingValues(0.dp)) {
                        Text(if (f.anh.isEmpty() && index == 0) "Ảnh bìa" else "Ảnh mới ${index + 1}", style = rmText(10))
                    }
                }
            }
            if (f.anh.size + f.anhChon.size < 8) Column(Modifier.size(104.dp).background(ReMarketPalette.Soft, RoundedCornerShape(16.dp))
                .border(1.dp, ReMarketPalette.Primary.copy(alpha = .22f), RoundedCornerShape(16.dp))
                .testTag("add_photos").clickable(enabled = !state.busy, onClick = pick), horizontalAlignment = Alignment.CenterHorizontally, verticalArrangement = Arrangement.Center) {
                RmIcon(R.drawable.ic_rm_camera, size = 25.dp)
                Text("Thêm ảnh", style = rmText(12), color = ReMarketPalette.Primary)
            }
        }
        Text("JPG/PNG · tối đa 5 MB mỗi ảnh · tối đa 8 ảnh", style = rmText(11), color = ReMarketPalette.Muted)
        LoiTruong(f.errors, "anh")
        HorizontalDivider(Modifier.padding(vertical = 6.dp), color = ReMarketPalette.Border)
        RmSectionTitle("Thông tin món đồ", "Tên rõ ràng giúp người mua tìm thấy bạn.")
        RmField("Tên sản phẩm", f.ten, { text -> vm.form { it.copy(ten = text.take(200)) } }, "Nhập tên món đồ", "listing_name", enabled = !state.busy, isError = "ten" in f.errors)
        LoiTruong(f.errors, "ten")
        LuaChon("Danh mục", f.danhMucId, state.categories.map { category -> category.id to
            (if (category.danhMucChaId == null) category.ten else "${state.categories.firstOrNull { it.id == category.danhMucChaId }?.ten.orEmpty()} · ${category.ten}") },
            { value -> vm.form { it.copy(danhMucId = value) } }, tag = "listing_category", enabled = !state.busy)
        LoiTruong(f.errors, "danhMuc")
        RmField("Giá bán (VND)", f.gia, { text -> vm.form { it.copy(gia = text.filter(Char::isDigit).take(13)) } }, "Ví dụ: 450000", "listing_price", keyboard = KeyboardType.Number, enabled = !state.busy,
            trailing = { Text("đ", style = rmText(14), color = ReMarketPalette.Muted) }, isError = "gia" in f.errors)
        f.gia.toLongOrNull()?.let { Text(tien(it), style = rmText(12, FontWeight.Medium), color = ReMarketPalette.Primary) }
        LoiTruong(f.errors, "gia")
        LuaChon("Tình trạng", f.tinhTrang, tinhTrangOptions, { value -> vm.form { it.copy(tinhTrang = value) } }, tag = "listing_condition", enabled = !state.busy)
        RmField("Mô tả", f.moTa, { text -> vm.form { it.copy(moTa = text.take(4000)) } }, "Mô tả chất liệu, thời gian sử dụng, tình trạng món đồ…", "listing_description", multiline = true, minimumHeight = 130, enabled = !state.busy, isError = "moTa" in f.errors)
        Text("${f.moTa.length} / 4000 ký tự", style = rmText(11), color = ReMarketPalette.Muted)
        LoiTruong(f.errors, "moTa")
        HorizontalDivider(Modifier.padding(vertical = 6.dp), color = ReMarketPalette.Border)
        RmSectionTitle("Vị trí giao dịch", "Giúp người mua tìm các món đồ gần mình.", R.drawable.ic_rm_map_pin)
        RmField("Tỉnh / Thành phố", f.tinhThanh, { text -> vm.form { it.copy(tinhThanh = text.take(100)) } }, "Nhập tỉnh / thành phố", "listing_city", enabled = !state.busy, isError = "tinhThanh" in f.errors)
        LoiTruong(f.errors, "tinhThanh")
        RmField("Địa điểm", f.diaDiem, { text -> vm.form { it.copy(diaDiem = text.take(500)) } }, "Quận / Huyện, Tỉnh / Thành phố", "listing_location", enabled = !state.busy, isError = "diaDiem" in f.errors)
        LoiTruong(f.errors, "diaDiem")
        RmNotice("Đăng thông tin đúng với tình trạng thực tế. Tin mới hoặc nội dung chỉnh sửa sẽ được ReMarket kiểm duyệt.")
        RmButton("Lưu bản nháp / thay đổi", { vm.luu(false) }, Modifier.testTag("save_draft"), enabled = !state.busy, outlined = true)
        if (state.error != null && f.id != null) RmLink("Tải lại tin từ máy chủ", { vm.chiTiet(f.id, true) }, enabled = !state.busy)
    }
}

@Composable
fun ChiTietTinScreen(state: BanHangState, vm: BanHangViewModel, loader: ImageLoader, ownerId: String) {
    val tin = state.detail ?: return
    val own = state.screen == ManSanPham.MANAGE
    val preview = state.screen == ManSanPham.PREVIEW
    Column(Modifier.fillMaxSize().testTag(if (preview) "listing_preview" else if (own) "listing_manage" else "product_detail")
        .verticalScroll(rememberScrollState()).padding(24.dp), verticalArrangement = Arrangement.spacedBy(18.dp)) {
        if (preview) RmNotice("Chỉ bạn nhìn thấy bản xem trước này. Tin sẽ được gửi duyệt sau khi xác nhận.")
        if (own) Row(Modifier.fillMaxWidth(), verticalAlignment = Alignment.CenterVertically, horizontalArrangement = Arrangement.SpaceBetween) {
            NhanTrangThai(tin.trangThai)
            Text(tin.ngayDang?.take(10) ?: "Chưa đăng", style = rmText(11), color = ReMarketPalette.Muted)
        }
        val photos = tin.anh.sortedByDescending { it.laAnhBia }
        val pager = rememberPagerState(pageCount = { maxOf(photos.size, 1) })
        Box(Modifier.fillMaxWidth().heightIn(max = 360.dp).aspectRatio(1.12f).clip(RoundedCornerShape(22.dp))) {
            HorizontalPager(pager) { index -> AnhSanPham(photos.getOrNull(index)?.url, loader, Modifier.fillMaxSize(), "${tin.ten}, ảnh ${index + 1}") }
            Text("Ảnh ${if (photos.isEmpty()) 0 else pager.currentPage + 1} / ${photos.size}", Modifier.align(Alignment.BottomEnd).padding(10.dp)
                .background(Color.White.copy(alpha = .9f), CircleShape).padding(horizontal = 10.dp, vertical = 5.dp), style = rmText(11))
            if (photos.size > 1) Row(Modifier.align(Alignment.BottomCenter).padding(bottom = 16.dp), horizontalArrangement = Arrangement.spacedBy(5.dp)) {
                photos.forEachIndexed { index, _ -> Box(Modifier.size(if (index == pager.currentPage) 7.dp else 5.dp)
                    .background(if (index == pager.currentPage) Color.White else Color.White.copy(alpha = .55f), CircleShape)) }
            }
        }
        Text(tin.ten, style = rmText(22, FontWeight.Bold), modifier = Modifier.testTag("detail_name"))
        Text(tien(tin.giaBan ?: tin.giaTriThamKhao), style = rmText(28, FontWeight.Bold), color = ReMarketPalette.Primary)
        Row(Modifier.horizontalScroll(rememberScrollState()), horizontalArrangement = Arrangement.spacedBy(8.dp), verticalAlignment = Alignment.CenterVertically) {
            RmBadge(tinhTrangText(tin.tinhTrang), tone = RmTone.NEUTRAL)
            SuggestionChip({ vm.search(tin.danhMucId) }, { Text(tin.danhMuc, style = rmText(12)) }, enabled = !preview && !own)
        }
        Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) { RmIcon(R.drawable.ic_rm_map_pin, size = 18.dp); Text(tin.diaDiem, style = rmText(13), color = ReMarketPalette.Muted) }
        if (own) {
            RmCard {
                Text("Thông tin quản lý", style = rmText(14, FontWeight.SemiBold))
                Text("${tin.anh.size} ảnh · ${tin.soCuocTroChuyen} cuộc trò chuyện", style = rmText(12), color = ReMarketPalette.Muted)
            }
            tin.lyDoTuChoi?.let { RmNotice("Lý do từ chối: $it", warning = true) }
            if (!tin.coTheChinhSua && tin.trangThai != "DaBan") RmNotice("Nội dung đang được giao dịch tham chiếu hoặc giữ chỗ; hãy giải quyết giao dịch trước khi sửa.", warning = true)
        }
        RmCard {
            Text("Mô tả", style = rmText(17, FontWeight.SemiBold))
            HorizontalDivider(Modifier.padding(vertical = 12.dp), color = ReMarketPalette.Border)
            Text(tin.moTa, style = rmText(14, height = 1.6f))
        }
        if (!preview && !own) RmCard {
            Row(horizontalArrangement = Arrangement.spacedBy(12.dp), verticalAlignment = Alignment.CenterVertically) {
                RmIconTile(R.drawable.ic_rm_user_round)
                Column(Modifier.weight(1f), verticalArrangement = Arrangement.spacedBy(5.dp)) {
                    Text(tin.nguoiDang, style = rmText(15, FontWeight.SemiBold))
                    Text("Thành viên từ ${tin.ngayThamGia.take(7)}", style = rmText(12), color = ReMarketPalette.Muted)
                    Text(if (tin.daXacMinhDienThoai) "Đã xác minh số điện thoại" else "Chưa xác minh số điện thoại", style = rmText(12), color = ReMarketPalette.Muted)
                }
            }
        }
        if (tin.nguoiDangId == ownerId && !own && !preview) RmButton("Quản lý tin của bạn", { vm.chiTiet(tin.id, true) }, outlined = true)
    }
}

@Composable
fun GuiThanhCongScreen(state: BanHangState, loader: ImageLoader) {
    val tin = state.detail ?: return
    Column(Modifier.fillMaxSize().testTag("listing_success").verticalScroll(rememberScrollState()).padding(24.dp), verticalArrangement = Arrangement.spacedBy(24.dp), horizontalAlignment = Alignment.CenterHorizontally) {
        Box(Modifier.padding(top = 18.dp).size(88.dp).background(ReMarketPalette.Soft, CircleShape), contentAlignment = Alignment.Center) { RmIcon(R.drawable.ic_rm_badge_check, size = 44.dp) }
        Text("Tin đã được gửi duyệt!", style = rmText(22, FontWeight.Bold), textAlign = TextAlign.Center)
        Text("Cảm ơn bạn đã cho món đồ một hành trình mới. ReMarket sẽ thông báo khi tin được duyệt.", style = rmText(14), color = ReMarketPalette.Muted, textAlign = TextAlign.Center)
        Text("Tin vừa gửi", Modifier.fillMaxWidth(), style = rmText(17, FontWeight.SemiBold))
        TheSanPham(tin, loader, compact = true, onClick = {})
        NhanTrangThai(tin.trangThai)
        RmNotice("Tin chưa hiển thị với người mua. Bạn có thể theo dõi trạng thái tại mục Quản lý tin.", warning = true)
    }
}

@Composable
fun XacNhanBanScreen(state: BanHangState, loader: ImageLoader) {
    val tin = state.detail ?: return
    Column(Modifier.fillMaxSize().testTag("sold_confirmation").verticalScroll(rememberScrollState()).padding(24.dp), verticalArrangement = Arrangement.spacedBy(24.dp)) {
        Text("Bạn đã bán được món đồ này?", style = rmText(22, FontWeight.Bold))
        Text("Chỉ xác nhận khi bạn đã hoàn tất việc bán cho người mua.", style = rmText(14), color = ReMarketPalette.Muted)
        TheSanPham(tin, loader, compact = true, onClick = {})
        RmNotice("Sau khi đánh dấu đã bán\n\nTin chuyển sang trạng thái Đã bán và không còn hiển thị với người mua. Bạn vẫn xem lại được trong mục Đã bán tại Quản lý tin.", warning = true)
    }
}
