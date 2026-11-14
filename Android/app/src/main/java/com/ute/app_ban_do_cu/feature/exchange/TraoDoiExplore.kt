package com.ute.app_ban_do_cu.feature.exchange

import androidx.compose.foundation.*
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Modifier
import androidx.compose.ui.Alignment
import androidx.compose.ui.text.style.TextOverflow
import com.ute.app_ban_do_cu.R
import androidx.compose.ui.draw.clip
import androidx.compose.ui.platform.testTag
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.unit.dp
import coil.ImageLoader
import com.ute.app_ban_do_cu.core.designsystem.*
import com.ute.app_ban_do_cu.domain.model.*
import com.ute.app_ban_do_cu.feature.catalog.*

@Composable fun TraoDoiExplore(s:TraoDoiState,vm:TraoDoiViewModel,loader:ImageLoader,onSmart:()->Unit){
    var filter by remember{mutableStateOf(false)}
    Column(Modifier.fillMaxSize().testTag("exchange_explore").verticalScroll(rememberScrollState()).padding(24.dp),verticalArrangement=Arrangement.spacedBy(16.dp)){
        RmField("Tìm đồ bạn muốn trao đổi",s.filter.tuKhoa,vm::query,"Nhập món đồ hoặc món muốn nhận","exchange_query",enabled=!s.busy)
        Row(horizontalArrangement=Arrangement.spacedBy(10.dp)){TdButton("Tìm kiếm",{vm.tim()},Modifier.weight(1f).testTag("exchange_search"),!s.busy);TdButton("Bộ lọc",{filter=true},Modifier.weight(1f).testTag("exchange_filters"),!s.busy,true)}
        Row(horizontalArrangement=Arrangement.spacedBy(8.dp)){FilterChip(true,{},label={Text("Khám phá")});FilterChip(false,{vm.tinToi()},label={Text("Trao đổi của tôi")},enabled=!s.busy,modifier=Modifier.testTag("exchange_my_listings"))}
        Text("Đồ cũ, giá trị mới",style=rmText(17,FontWeight.SemiBold));Text("${s.results?.tongSo?:0} món phù hợp với tìm kiếm của bạn",style=rmText(12),color=ReMarketPalette.Muted)
        Row(horizontalArrangement=Arrangement.spacedBy(8.dp)){
            FilterChip(s.filter.choPhepMotMot==true,{vm.tim(s.filter.copy(choPhepMotMot=if(s.filter.choPhepMotMot==true)null else true))},label={Text("Trao đổi 1–1")},enabled=!s.busy)
            FilterChip(s.filter.choPhepSmartSwap==true,{vm.tim(s.filter.copy(choPhepSmartSwap=if(s.filter.choPhepSmartSwap==true)null else true))},label={Text("Cho phép 3 bên")},enabled=!s.busy)
        }
        TdButton("Đề nghị đã gửi / đã nhận",{vm.deNghi()},Modifier.testTag("exchange_offers"),!s.busy,true)
        Column(Modifier.fillMaxWidth().background(ReMarketPalette.Soft,RoundedCornerShape(20.dp)).padding(16.dp),verticalArrangement=Arrangement.spacedBy(12.dp)){
            RmSectionTitle("Smart Swap 3 bên","Thêm cơ hội đổi đúng món bạn cần.",R.drawable.ic_rm_arrow_left_right)
            TdButton("Tìm vòng trao đổi",onSmart,Modifier.testTag("smart_swap_entry"),!s.busy)
        }
        s.results?.items?.forEach{tin->TdCard(Modifier.testTag("exchange_listing_${tin.id}").clickable(enabled=!s.busy){vm.chiTiet(tin.id)}){
            Box {
                AnhSanPham(tin.anh.firstOrNull{it.laAnhBia}?.url?:tin.anh.firstOrNull()?.url,loader,Modifier.fillMaxWidth().aspectRatio(1.65f).clip(RoundedCornerShape(16.dp)),tin.ten)
                if(tin.traoDoi?.choPhepSmartSwap==true)RmBadge("Smart Swap",Modifier.align(Alignment.TopStart).padding(10.dp))
            }
            Text(tin.ten,style=rmText(16,FontWeight.SemiBold));Text("Tham khảo: ${tien(tin.giaTriThamKhao)}",style=rmText(17,FontWeight.Bold),color=ReMarketPalette.Primary)
            Text("Muốn đổi: ${tin.traoDoi?.moTaMongMuon.orEmpty()}",style=rmText(13),maxLines=2,overflow=TextOverflow.Ellipsis);Text("${tinhTrangText(tin.tinhTrang)} · ${tin.tinhThanh}",style=rmText(12),color=ReMarketPalette.Muted)
            RmBadge(if(tin.traoDoi?.choPhepBuTien==true)"Có bù tiền" else "Không bù tiền",tone=RmTone.NEUTRAL)
        }}
        s.results?.let{page->if(page.items.isEmpty())TrangRong("Chưa có món phù hợp","Thay từ khóa hoặc bộ lọc để khám phá thêm.",Modifier.testTag("exchange_empty"))
            PhanTrang(page.trang,page.tongTrang,!s.busy,{vm.tim(page=page.trang-1)},{vm.tim(page=page.trang+1)})}
    }
    if(filter)TraoDoiFilter(s,{filter=false}){vm.tim(it);filter=false}
}
@OptIn(ExperimentalMaterial3Api::class)
@Composable private fun TraoDoiFilter(s:TraoDoiState,close:()->Unit,apply:(BoLocTraoDoi)->Unit){
    var category by remember{mutableStateOf(s.filter.danhMucId.orEmpty())};var city by remember{mutableStateOf(s.filter.tinhThanh)}
    var min by remember{mutableStateOf(s.filter.giaTu?.toString().orEmpty())};var max by remember{mutableStateOf(s.filter.giaDen?.toString().orEmpty())};var bu by remember{mutableStateOf(s.filter.choPhepBuTien==true)}
    var condition by remember{mutableStateOf(s.filter.tinhTrang.orEmpty())};var error by remember{mutableStateOf<String?>(null)}
    ModalBottomSheet(close,sheetState=rememberModalBottomSheetState(skipPartiallyExpanded=true),containerColor=ReMarketPalette.Background){
        Column(Modifier.fillMaxWidth().verticalScroll(rememberScrollState()).imePadding().padding(24.dp),verticalArrangement=Arrangement.spacedBy(16.dp)){
            Text("Bộ lọc trao đổi",style=rmText(20,FontWeight.SemiBold))
            LuaChon("Danh mục",category,listOf("" to "Tất cả danh mục")+s.categories.map{it.id to it.ten},{category=it},tag="exchange_filter_category")
            RmField("Tỉnh / Thành phố",city,{city=it.take(100)},"Tất cả khu vực","exchange_filter_city")
            RmField("Giá trị từ (VND)",min,{min=it.filter(Char::isDigit).take(13)},"0","exchange_filter_min",keyboard=KeyboardType.Number)
            RmField("Giá trị đến (VND)",max,{max=it.filter(Char::isDigit).take(13)},"Không giới hạn","exchange_filter_max",keyboard=KeyboardType.Number)
            LuaChon("Tình trạng",condition,listOf("" to "Tất cả")+tinhTrangOptions,{condition=it},tag="exchange_filter_condition")
            TdSwitch("Chấp nhận bù tiền","",bu,true,"exchange_filter_compensation"){bu=it}
            error?.let{Text(it,color=ReMarketPalette.Error,style=rmText(12))}
            TdButton("Áp dụng",{if(min.isNotBlank()&&max.isNotBlank()&&min.toLong()>max.toLong())error="Giá trị từ không được lớn hơn giá trị đến." else apply(s.filter.copy(danhMucId=category.ifBlank{null},tinhThanh=city.trim(),giaTu=min.toLongOrNull(),giaDen=max.toLongOrNull(),tinhTrang=condition.ifBlank{null},choPhepBuTien=if(bu)true else null))},Modifier.testTag("exchange_filter_apply"))
            TdButton("Xóa lọc",{apply(BoLocTraoDoi(tuKhoa=s.filter.tuKhoa))},Modifier.testTag("exchange_filter_clear"),outlined=true)
        }
    }
}
@Composable fun TraoDoiMyListings(s:TraoDoiState,vm:TraoDoiViewModel,loader:ImageLoader,city:String){
    Column(Modifier.fillMaxSize().testTag("exchange_mine").verticalScroll(rememberScrollState()).padding(24.dp),verticalArrangement=Arrangement.spacedBy(16.dp)){
        Row(horizontalArrangement=Arrangement.spacedBy(8.dp)){FilterChip(false,vm::khamPha,label={Text("Khám phá")},enabled=!s.busy);FilterChip(true,{},label={Text("Trao đổi của tôi")})}
        TdButton("Đăng tin trao đổi",{vm.taoTin(city)},Modifier.testTag("exchange_new_listing"),!s.busy)
        TdButton("Đề nghị / giao dịch của tôi",{vm.deNghi()},Modifier.testTag("exchange_my_offers"),!s.busy,true)
        Row(Modifier.horizontalScroll(rememberScrollState()),horizontalArrangement=Arrangement.spacedBy(8.dp)){
            FilterChip(s.mineStatus==null,{vm.tinToi()},label={Text("Tất cả")},enabled=!s.busy)
            listOf("BanNhap" to "Bản nháp","ChoDuyet" to "Chờ duyệt","DangHienThi" to "Đang hiển thị","DaAn" to "Đã ẩn","DangGiuCho" to "Đang giao dịch","DaTraoDoi" to "Đã trao đổi").forEach{(value,label)->FilterChip(s.mineStatus==value,{vm.tinToi(value)},label={Text(label)},enabled=!s.busy)}
        }
        s.mine?.danhSach?.items?.forEach{tin->TdCard(Modifier.testTag("exchange_mine_${tin.id}").clickable(enabled=!s.busy){vm.chiTiet(tin.id,true)}){
            TdMon(tin.mon(),loader,"Tin của bạn",framed=false);NhanTrangThai(tin.trangThai)
            Text("Muốn nhận: ${tin.traoDoi?.moTaMongMuon.orEmpty()}",style=rmText(12));Text(listOfNotNull(if(tin.traoDoi?.choPhepMotMot==true)"1–1" else null,if(tin.traoDoi?.choPhepSmartSwap==true)"Smart Swap" else null,if(tin.traoDoi?.choPhepBuTien==true)"Có bù tiền" else null).joinToString(" · "),style=rmText(11),color=ReMarketPalette.Muted)
            Text("Xem / Sửa / Ẩn / Xóa ›",style=rmText(13,FontWeight.SemiBold),color=ReMarketPalette.Primary)
        }}
        s.mine?.danhSach?.let{if(it.items.isEmpty())TrangRong("Chưa có tin trao đổi","Đăng món đồ của bạn để bắt đầu.",Modifier.testTag("exchange_mine_empty"));PhanTrang(it.trang,it.tongTrang,!s.busy,{vm.tinToi(s.mineStatus,it.trang-1)},{vm.tinToi(s.mineStatus,it.trang+1)})}
    }
}
