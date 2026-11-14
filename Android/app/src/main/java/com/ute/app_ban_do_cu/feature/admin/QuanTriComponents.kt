package com.ute.app_ban_do_cu.feature.admin

import androidx.compose.foundation.*
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.platform.testTag
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.text.style.TextOverflow
import com.ute.app_ban_do_cu.R
import com.ute.app_ban_do_cu.core.designsystem.*
import com.ute.app_ban_do_cu.domain.model.*
import com.ute.app_ban_do_cu.feature.catalog.*
import com.ute.app_ban_do_cu.feature.commerce.ngayGio
import com.ute.app_ban_do_cu.feature.exchange.TdCard

@Composable fun QtBody(tag:String,content:@Composable ColumnScope.()->Unit){Column(Modifier.fillMaxSize().testTag(tag).verticalScroll(rememberScrollState()).padding(horizontal=24.dp,vertical=16.dp),verticalArrangement=Arrangement.spacedBy(16.dp),content=content)}
@Composable fun QtButton(text:String,tag:String,modifier:Modifier=Modifier,enabled:Boolean=true,outline:Boolean=false,click:()->Unit){RmButton(text,click,modifier.testTag(tag),enabled=enabled,outlined=outline)}
@Composable fun QtBadge(value:String){RmBadge(qtStatus(value),tone=when(value){
    "DaKhoa","TuChoi","TranhChap","TamDung"->RmTone.NEGATIVE
    "ChoDuyet","ChoDongY","ChoThucHien","DangXuLy","ChoThanhToan"->RmTone.PENDING
    "DaAn","DaXoa","DaHuy","DaThayThe","KhoiTao"->RmTone.NEUTRAL
    else->RmTone.POSITIVE
})}
fun qtStatus(value:String)=when(value){"HoatDong"->"Đang hoạt động";"DaKhoa"->"Đã khóa";"DaXoa"->"Đã xóa";"Moi"->"Mới";"DangXuLy"->"Đang xử lý";"ChoThucHien"->"Chờ thực hiện";"DaGiaiQuyet"->"Đã giải quyết";"TuChoi"->"Đã từ chối";"ChoDuyet"->"Chờ duyệt";"DangHienThi"->"Đang hiển thị";"DaAn"->"Đã ẩn";"DangGiuCho"->"Đang giao dịch";"ChoDongY"->"Chờ đồng ý";"DangThucHien"->"Đang thực hiện";"HoanTat"->"Hoàn tất";"DaHuy"->"Đã hủy";"TranhChap"->"Tranh chấp";"TamDung"->"Tạm dừng";"MuaBan"->"Mua bán";"TraoDoiMotMot"->"Trao đổi 1–1";"SmartSwap"->"Smart Swap 3 bên";"DaNhan"->"Đã nhận";"DangGiao"->"Đang giao";"DaThanhToan"->"Đã thanh toán";"ChoThanhToan"->"Chờ thanh toán";"DaChot"->"Đã chốt";"DeXuat"->"Đề xuất";"DaThayThe"->"Đã thay thế";"SanSangBanGiao"->"Chờ bàn giao";"DangBanGiao"->"Đang bàn giao";"KhoiTao"->"Khởi tạo";"TraoDoi"->"Trao đổi";"CaHai"->"Bán và trao đổi";else->com.ute.app_ban_do_cu.feature.commerce.tenTrangThai(com.ute.app_ban_do_cu.feature.smartswap.ssShipStatus(trangThaiText(value)))}
@Composable fun QtAvatar(name:String){Box(Modifier.size(44.dp).background(ReMarketPalette.Soft,CircleShape),contentAlignment=Alignment.Center){Text(name.trim().split(Regex("\\s+")).takeLast(2).mapNotNull{it.firstOrNull()}.joinToString("").uppercase(),style=rmText(14,FontWeight.SemiBold),color=ReMarketPalette.Primary)}}
@Composable fun QtSearch(s:QuanTriState,vm:QuanTriViewModel,label:String,search:()->Unit){RmField(label,s.query,vm::query,"Nhập để tìm kiếm","qt_query",enabled=!s.busy,
    trailing={IconButton(search,enabled=!s.busy,modifier=Modifier.testTag("qt_search")){RmIcon(R.drawable.ic_rm_search,description="Tìm kiếm")}})}
@Composable fun QtTabs(current:String?,options:List<Pair<String?,String>>,enabled:Boolean=true,select:(String?)->Unit){Row(Modifier.horizontalScroll(rememberScrollState()),horizontalArrangement=Arrangement.spacedBy(8.dp)){options.forEach{(value,label)->FilterChip(current==value,{select(value)},label={Text(label,style=rmText(12))},enabled=enabled,modifier=Modifier.testTag("qt_tab_${value?:"all"}"))}}}
private fun qtHuman(text:String):String=if(!text.startsWith("{"))text else runCatching{val json=com.google.gson.JsonParser.parseString(text).asJsonObject;listOf("LyDo","NoiDung","Ten","KetLuan","BienPhap","CanCu").mapNotNull{json.get(it)?.takeIf{v->v.isJsonPrimitive&&v.asJsonPrimitive.isString}?.asString}.joinToString(" · ").ifBlank{"Đã ghi nhận thao tác quản trị."}}.getOrDefault("Đã ghi nhận thao tác quản trị.")
@Composable fun QtTimeline(rows:List<QtMoc>){rows.forEachIndexed{index,r->RmTimelineItem(qtStatus(r.hanhDong),"${qtHuman(r.noiDung)}\n${ngayGio(r.ngay)} · ${r.nguoiThucHien}",true,last=index==rows.lastIndex)}}
@Composable fun QtTradeCard(t:QtTrade,enabled:Boolean=true,click:()->Unit){TdCard(Modifier.testTag("qt_trade_${t.id}").clickable(enabled=enabled,onClick=click)){
    Row(Modifier.fillMaxWidth(),verticalAlignment=Alignment.CenterVertically,horizontalArrangement=Arrangement.spacedBy(8.dp)){
        Text(t.ma,Modifier.weight(1f),style=rmText(13,FontWeight.SemiBold),maxLines=1,overflow=TextOverflow.Ellipsis);QtBadge(t.trangThai)}
    Text(qtStatus(t.loai),style=rmText(12,FontWeight.Medium),color=ReMarketPalette.Primary)
    Text(t.thanhVien.joinToString(" · "){it.hoTen},style=rmText(12));Text(t.sanPham.joinToString(" · "){it.ten},style=rmText(13))
    if(t.loai!="MuaBan"){RmKeyValue("Tiền bù",tien(t.tongTienBu));RmKeyValue("Đã nhận đúng hàng","${t.soNguoiNhan}/${t.tongNguoiNhan}")}
    Text("Xem chi tiết ›",style=rmText(12,FontWeight.SemiBold),color=ReMarketPalette.Primary)
}}
