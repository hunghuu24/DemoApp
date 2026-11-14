package com.ute.app_ban_do_cu.feature.smartswap

import androidx.compose.foundation.*
import androidx.compose.foundation.layout.*
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.testTag
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.unit.dp
import androidx.compose.ui.draw.clip
import androidx.compose.foundation.shape.RoundedCornerShape
import coil.ImageLoader
import com.ute.app_ban_do_cu.R
import com.ute.app_ban_do_cu.core.designsystem.*
import com.ute.app_ban_do_cu.domain.model.*
import com.ute.app_ban_do_cu.feature.catalog.*
import com.ute.app_ban_do_cu.feature.commerce.ngayGio
import com.ute.app_ban_do_cu.feature.exchange.*

@Composable fun SsFind(s:SmartSwapState,vm:SmartSwapViewModel,loader:ImageLoader){SsContent("ss_find"){
    SsBadge("Smart Swap · Trao đổi 3 bên")
    RmSectionTitle("Một vòng đổi, ba món đồ","Chọn đồ bạn có và món bạn cần. ReMarket tìm vòng phù hợp cho cả 3 người.",R.drawable.ic_rm_arrow_left_right)
    Text("Sản phẩm của tôi",style=rmText(13,FontWeight.Medium))
    s.selected?.let{tin->TdCard{
        Row(horizontalArrangement=Arrangement.spacedBy(10.dp),verticalAlignment=Alignment.CenterVertically){
            AnhSanPham((tin.anh.firstOrNull{it.laAnhBia}?:tin.anh.firstOrNull())?.url,loader,Modifier.size(54.dp).clip(RoundedCornerShape(12.dp)),tin.ten)
            Column(Modifier.weight(1f),verticalArrangement=Arrangement.spacedBy(2.dp)){Text(tin.ten,style=rmText(13,FontWeight.SemiBold));Text(tien(tin.giaTriThamKhao),style=rmText(14,FontWeight.Bold),color=ReMarketPalette.Primary);Text("${tin.nguoiDang} · ${tinhTrangText(tin.tinhTrang)}",style=rmText(11),color=ReMarketPalette.Muted)}
        }
        Row(Modifier.fillMaxWidth(),verticalAlignment=Alignment.CenterVertically,horizontalArrangement=Arrangement.SpaceBetween){
            RmBadge("Đã chọn")
            TextButton({vm.select()},enabled=!s.busy,modifier=Modifier.testTag("ss_change_product")){Text("Đổi sản phẩm",style=rmText(12,FontWeight.Medium))}
        }
    };Text("Nhu cầu đã đăng: ${tin.traoDoi?.moTaMongMuon.orEmpty()}",style=rmText(11),color=ReMarketPalette.Muted)}?:SsButton("Chọn sản phẩm đã đăng","ss_select_product",!s.busy,true){vm.select()}
    RmField("Tôi muốn nhận",s.filter.tuKhoaMuonNhan,{v->vm.filter{it.copy(tuKhoaMuonNhan=v.take(100))}},"Lọc thêm tên món muốn nhận","ss_wanted",enabled=!s.busy)
    RmField("Tiền bù tối đa của tôi (VND)",s.cap,vm::cap,"Theo hạn mức tin đăng","ss_search_cap",keyboard=KeyboardType.Number,enabled=!s.busy)
    Text("Chỉ trả trực tiếp khi gặp, không thanh toán qua ứng dụng.",style=rmText(11),color=ReMarketPalette.Primary)
    RmField("Địa điểm",s.filter.tinhThanh,{v->vm.filter{it.copy(tinhThanh=v.take(100))}},"Tất cả tỉnh/thành phố","ss_search_city",enabled=!s.busy)
    LuaChon("Tình trạng chấp nhận",s.filter.tinhTrangToiThieu.orEmpty(),listOf("" to "Theo nhu cầu đã đăng")+tinhTrangOptions,{v->vm.filter{it.copy(tinhTrangToiThieu=v.ifBlank{null})}},enabled=!s.busy,tag="ss_search_condition",icon=R.drawable.ic_fg_smart_down)
    SsCheck("Chỉ ghép với thành viên đã xác minh email và điện thoại",s.filter.verified,!s.busy,"ss_verified"){v->vm.filter{it.copy(verified=v)}}
    Row(horizontalArrangement=Arrangement.spacedBy(8.dp)){TextButton({vm.list()},enabled=!s.busy,modifier=Modifier.testTag("ss_my_cycles")){Text("Vòng của tôi")};TextButton({vm.notifications()},enabled=!s.busy,modifier=Modifier.testTag("ss_notifications")){Text("Thông báo")}}
}}
@Composable fun SsSelect(s:SmartSwapState,vm:SmartSwapViewModel,loader:ImageLoader){SsContent("ss_select"){
    Text("Chỉ dùng sản phẩm của bạn đã đăng, đang hiển thị và cho phép Smart Swap.",style=rmText(12),color=ReMarketPalette.Muted)
    val eligible=s.mine?.items?.filter{it.traoDoi?.choPhepSmartSwap==true}
    eligible?.forEach{tin->TdCard(Modifier.testTag("ss_product_${tin.id}").clickable(enabled=!s.busy){vm.choose(tin)}){TdMon(tin.mon(),loader,"${tinhTrangText(tin.tinhTrang)} · Chọn món này",framed=false)}}
    if(eligible?.isEmpty()==true)TrangRong("Chưa có sản phẩm Smart Swap ở trang này","Đăng tin trao đổi, bật Smart Swap và chờ duyệt. Bạn cũng có thể xem trang tiếp theo.",Modifier.testTag("ss_select_empty"))
    s.mine?.let{p->PhanTrang(p.trang,p.tongTrang,!s.busy,{vm.select(p.trang-1)},{vm.select(p.trang+1)})}
}}
@Composable fun SsMatches(s:SmartSwapState,vm:SmartSwapViewModel,user:String){SsContent("ss_matches"){
    Row(Modifier.horizontalScroll(rememberScrollState()),horizontalArrangement=Arrangement.spacedBy(8.dp)){SsBadge(s.filter.tuKhoaMuonNhan.ifBlank{"Theo nhu cầu đã đăng"});if(s.cap.isNotBlank())SsBadge("≤ ${tien(s.cap.toLongOrNull()?:0)}")}
    s.results?.let{r->
        Text("${r.tongSo} vòng phù hợp với bạn",style=rmText(17,FontWeight.SemiBold))
        Text(r.thongBao,style=rmText(12),color=ReMarketPalette.Muted)
        val names=r.hoSo.orEmpty().associate{it.nguoiDungId to it.hoTen}
        r.items.forEachIndexed{index,v->TdCard(Modifier.testTag("ss_match_${v.maDeXuat}")){
            Row(Modifier.fillMaxWidth(),verticalAlignment=Alignment.CenterVertically,horizontalArrangement=Arrangement.spacedBy(8.dp)){
                Text("Vòng ${((r.trang-1)*20)+index+1} · 3 người",Modifier.weight(1f),style=rmText(15,FontWeight.SemiBold))
                RmBadge("${v.diemPhuHop}% phù hợp")
            }
            ssOrder(v.chang,user).forEachIndexed{i,leg->val received=v.chang.first{it.nguoiNhanId==leg.nguoiGiaoId}.sanPham
                Text("${listOf("A","C","B")[i]} · ${names[leg.nguoiGiaoId].orEmpty()}${if(leg.nguoiGiaoId==user)" (Tôi)" else ""}",style=rmText(13,FontWeight.SemiBold))
                Text("Đưa ${leg.sanPham.ten} → Nhận ${received.ten}",style=rmText(12),color=ReMarketPalette.Muted)}
            HorizontalDivider(color=ReMarketPalette.Border)
            SsMoney(v.tienBu,names,framed=false)
            SsButton("Xem chi tiết","ss_match_open_$index",!s.busy,true){vm.preview(v.maDeXuat)}
        }}
        if(r.items.isEmpty())TrangRong("Chưa tìm thấy vòng phù hợp",r.thongBao,Modifier.testTag("ss_empty"))
        PhanTrang(r.trang,r.tongTrang,!s.busy,{vm.search(r.trang-1)},{vm.search(r.trang+1)})
    }
}}
@Composable fun SsCycleList(s:SmartSwapState,vm:SmartSwapViewModel){SsContent("ss_list"){
    Row(horizontalArrangement=Arrangement.spacedBy(8.dp)){SsButton("Tìm vòng","ss_list_find",!s.busy,modifier=Modifier.weight(1f)){vm.find()};SsButton("Thông báo","ss_list_notifications",!s.busy,true,Modifier.weight(1f)){vm.notifications()}}
    Row(Modifier.horizontalScroll(rememberScrollState()),horizontalArrangement=Arrangement.spacedBy(8.dp)){
        listOf("tatca" to "Tất cả","gui" to "Đã gửi","nhan" to "Đã nhận","chodongy" to "Chờ đồng ý","chinhthuc" to "Đang giao","hoantat" to "Hoàn tất","tamdung" to "Sự cố","dahuy" to "Đã dừng").forEach{(tab,title)->
            FilterChip(s.tab==tab,{vm.list(tab)},label={Text(title)},enabled=!s.busy,modifier=Modifier.testTag("ss_tab_$tab"))}
    }
    s.cycles?.let{page->Text("${page.tongSo} vòng trao đổi",style=rmText(13),color=ReMarketPalette.Muted)
        page.items.forEach{v->TdCard(Modifier.testTag("ss_cycle_${v.id}").clickable(enabled=!s.busy){vm.detail(v.id)}){
            Text(v.ma,style=rmText(14,FontWeight.SemiBold));SsBadge(ssStatus(v),v.trangThai=="TamDung")
            Text(v.thanhVien.joinToString(" · "){it.hoTen},style=rmText(12))
            v.chang.forEach{leg->Text("${v.names()[leg.nguoiGiaoId]} → ${v.names()[leg.nguoiNhanId]}: ${leg.sanPham.ten}",style=rmText(11),color=ReMarketPalette.Muted)}
            Text("${v.phuongAnHienTai?.dongY?.count{it.quyetDinh=="DongY"}?:0}/3 đồng ý · Nhận đúng: ${v.tienDoGiaoNhan?.tienDo?:"0/3"}",style=rmText(12),color=ReMarketPalette.Primary)
            Text("Tiền bù: ${tien(v.phuongAnHienTai?.tongTienTra?:0)}",style=rmText(12))
        }}
        if(page.items.isEmpty())TrangRong("Chưa có vòng trao đổi","Tìm vòng phù hợp hoặc chờ lời mời từ thành viên khác.",Modifier.testTag("ss_list_empty"))
        PhanTrang(page.trang,page.tongTrang,!s.busy,{vm.list(page=page.trang-1)},{vm.list(page=page.trang+1)})
    }
}}
@Composable fun SsNotifications(s:SmartSwapState,vm:SmartSwapViewModel){SsContent("ss_notification_list"){
    s.notifications?.let{rows->rows.forEach{n->TdCard(Modifier.testTag("ss_notification_${n.id}").clickable(enabled=!s.busy){vm.openNotification(n)}){
        Text(n.tieuDe,style=rmText(14,FontWeight.SemiBold));if(!n.daDoc)SsBadge("Chưa đọc");Text(n.noiDung,style=rmText(12));Text(ngayGio(n.ngayTao),style=rmText(11),color=ReMarketPalette.Muted)
    }};if(rows.isEmpty())TrangRong("Chưa có thông báo","Các cập nhật Smart Swap sẽ xuất hiện tại đây.")
        Row{TextButton({vm.notifications(s.notificationPage-1)},enabled=!s.busy&&s.notificationPage>1){Text("Trước")};TextButton({vm.notifications(s.notificationPage+1)},enabled=!s.busy&&rows.size==20){Text("Tiếp")}}
    }
}}
