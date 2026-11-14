package com.ute.app_ban_do_cu.feature.smartswap

import androidx.compose.foundation.layout.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.testTag
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.unit.dp
import coil.ImageLoader
import com.ute.app_ban_do_cu.core.designsystem.*
import com.ute.app_ban_do_cu.domain.model.*
import com.ute.app_ban_do_cu.feature.catalog.*
import com.ute.app_ban_do_cu.feature.commerce.ngayGio
import com.ute.app_ban_do_cu.feature.exchange.*

@Composable fun SsDeliveryScreen(s:SmartSwapState,vm:SmartSwapViewModel,profile:HoSo,loader:ImageLoader,addPhoto:()->Unit){val user=profile.id;val v=s.circle?:return;val d=s.delivery?:return
    SsContent("ss_delivery"){
        Text(v.ma,style=rmText(15,FontWeight.SemiBold));SsBadge(ssStatus(v),d.tienDo.tamDung)
        Text("${d.tienDo.tienDo} xác nhận nhận đúng hàng",Modifier.testTag("ss_delivery_progress"),style=rmText(18,FontWeight.SemiBold),color=ReMarketPalette.Primary)
        LinearProgressIndicator(progress={d.tienDo.soNguoiNhanDung/3f},modifier=Modifier.fillMaxWidth().height(6.dp),
            color=if(d.tienDo.tamDung)ReMarketPalette.Warning else ReMarketPalette.Primary,trackColor=ReMarketPalette.Soft)
        Text("Chỉ hoàn tất khi đủ 3 người nhận đúng hàng. Trạng thái ‘Đã giao’ của vận chuyển chưa phải xác nhận của người nhận.",style=rmText(12),color=ReMarketPalette.Muted)
        if(d.tienDo.tamDung)RmNotice("Có báo cáo sự cố: giao dịch tạm dừng, chưa thể gửi hoặc nhận hàng.",warning=true)
        d.suCo.forEach{issue->TdCard{SsBadge("${issue.ma} · ${ssSystemText(issue.trangThai)}",true);Text(issue.tieuDe,style=rmText(14,FontWeight.SemiBold));Text(issue.noiDung,style=rmText(12))}}
        if(d.vanChuyen.isEmpty()){
            RmProgress(d.soNguoiXacNhanThongTin,3,"Đã xác nhận thông tin giao nhận")
            d.lienHe.forEach{m->Text("${m.hoTen}: ${if(m.daXacNhan)"Đã xác nhận" else "Chờ xác nhận"}",style=rmText(13))}
            if(v.coTheBanGiao)SsButton("Xác nhận thông tin của tôi","ss_contact_open",!s.busy,true){vm.contact(profile)}
            Text("Đủ 3 đồng ý và 3 thông tin giao nhận mới tạo ba đơn vận chuyển mô phỏng.",style=rmText(12),color=ReMarketPalette.Muted)
        }
        d.vanChuyen.forEach{ship->SsShipment(ship,s,vm,v,user)}
        val incoming=d.vanChuyen.firstOrNull{it.nguoiNhanId==user}
        if(incoming?.coTheNhanDungHang==true||incoming?.ngayNhanDungHang!=null){
            SsButton("Thêm bằng chứng giao nhận","ss_evidence_add",!s.busy,true,click=addPhoto)
            Text("Ảnh món đồ / biên nhận · JPG, PNG · Tối đa 5 ảnh. Ảnh được lưu sau khi bạn xác nhận nhận đúng hàng.",style=rmText(11),color=ReMarketPalette.Muted)
            s.evidence.forEach{photo->Row{coil.compose.AsyncImage(photo.bytes,"Ảnh bằng chứng đang chọn",imageLoader=loader,modifier=Modifier.size(80.dp));TextButton({vm.removeEvidence(photo.id)},enabled=!s.busy){Text("Bỏ ảnh")}}}
            d.bangChung.orEmpty().forEach{photo->AnhSanPham(photo.url,loader,Modifier.size(96.dp),"Bằng chứng giao nhận")}
        }
        if(incoming?.coTheNhanDungHang==true){SsCheck("Tôi đã kiểm tra sản phẩm",s.checkedItem,!s.busy,"ss_check_item"){vm.receipt(item=it)}
            SsCheck("Tôi đã nhận đúng hàng từ ${v.names()[incoming.nguoiGuiId]}",s.received,!s.busy,"ss_check_received"){vm.receipt(received=it)}
            SsCheck("Tôi xác nhận khoản tiền bù của mình đã trả/nhận đủ nếu có",s.checkedMoney,!s.busy,"ss_check_money"){vm.receipt(money=it)}}
        v.phuongAnHienTai?.let{SsMoney(it.tienBu,v.names())}
    }
}
@Composable private fun SsShipment(ship:SsShip,s:SmartSwapState,vm:SmartSwapViewModel,v:SsVong,user:String){var code by remember(ship.id,ship.maVanDon){mutableStateOf(ship.maVanDon.orEmpty())};var dispatch by remember{mutableStateOf(false)}
    var showHistory by remember(ship.id){mutableStateOf(false)}
    TdCard(Modifier.testTag("ss_ship_${ship.id}")){
        RmBadge(when(user){ship.nguoiGuiId->"Bạn gửi";ship.nguoiNhanId->"Bạn nhận";else->"Chặng trong vòng"},tone=RmTone.NEUTRAL)
        Text("${v.names()[ship.nguoiGuiId]} → ${v.names()[ship.nguoiNhanId]}",style=rmText(14,FontWeight.SemiBold));Text(ship.tenSanPham,style=rmText(13))
        SsBadge(if(ship.ngayNhanDungHang!=null)"Đã nhận đúng hàng" else ssShipStatus(ship.trangThai))
        Text("Mã mô phỏng: ${ship.maMoPhong}",style=rmText(11),color=ReMarketPalette.Muted)
        if(ship.coTheNhapMa){RmField("Mã vận đơn",code,{code=it.take(40)},"Nhập mã vận đơn mô phỏng","ss_tracking_${ship.id}",enabled=!s.busy)
            SsButton("Lưu mã vận đơn","ss_tracking_save_${ship.id}",!s.busy,true){vm.tracking(ship,code)}}else ship.maVanDon?.let{Text("Vận đơn: $it",style=rmText(12))}
        if(ship.coTheCapNhat){val next=when(ship.trangThai){"ChoGui"->"DaGui";"DaGui"->"DangVanChuyen";else->"DaGiao"}
            SsButton(when(next){"DaGui"->"Xác nhận đã gửi";"DangVanChuyen"->"Mô phỏng đang vận chuyển";else->"Mô phỏng đã giao"},"ss_ship_next_${ship.id}",!s.busy){if(next=="DaGui")dispatch=true else vm.ship(ship,next)}}
        if(ship.nguoiGuiId==user||ship.nguoiNhanId==user){ship.diaChiGui?.let{Text("Gửi: ${ship.tenNguoiGui} · ${ship.dienThoaiGui}\n$it",style=rmText(11))};ship.diaChiNhan?.let{Text("Nhận: ${ship.tenNguoiNhan} · ${ship.dienThoaiNhan}\n$it",style=rmText(11))}}
        if(ship.lichSu.isNotEmpty())TextButton({showHistory=!showHistory},modifier=Modifier.testTag("ss_ship_history_${ship.id}")){
            Text(if(showHistory)"Ẩn lịch sử vận chuyển" else "Lịch sử vận chuyển · ${ship.lichSu.size} cập nhật",style=rmText(12))}
        if(showHistory)ship.lichSu.forEachIndexed{index,m->RmTimelineItem(ssShipStatus(m.trangThai),ngayGio(m.thoiDiem),true,last=index==ship.lichSu.lastIndex)}
        if(ship.coTheBaoSuCo)TextButton({vm.issue(ship)},enabled=!s.busy,modifier=Modifier.testTag("ss_issue_open_${ship.id}")){Text("Báo cáo vấn đề",color=ReMarketPalette.Error)}
    }
    if(dispatch)AlertDialog(onDismissRequest={dispatch=false},title={Text("Xác nhận đã gửi hàng")},text={Text("Bạn đã bàn giao đúng sản phẩm cho chặng này? Đây là vận chuyển mô phỏng.")},
        confirmButton={TextButton({dispatch=false;vm.ship(ship,"DaGui")},modifier=Modifier.testTag("ss_dispatch_confirm")){Text("Tôi đã gửi")}},dismissButton={TextButton({dispatch=false}){Text("Quay lại")}})
}
@Composable fun SsContactScreen(s:SmartSwapState,vm:SmartSwapViewModel){val f=s.contact;SsContent("ss_contact"){
    Text("Thông tin của bạn chỉ hiển thị cho người gửi/nhận có liên quan.",style=rmText(12),color=ReMarketPalette.Muted)
    RmField("Tên liên hệ",f.name,{v->vm.contactForm{it.copy(name=v.take(120))}},"Họ và tên","ss_contact_name",enabled=!s.busy)
    RmField("Số điện thoại",f.phone,{v->vm.contactForm{it.copy(phone=v.take(20))}},"09xxxxxxxx","ss_contact_phone",keyboard=KeyboardType.Phone,enabled=!s.busy)
    RmField("Địa chỉ gửi hàng",f.from,{v->vm.contactForm{it.copy(from=v.take(500))}},"Số nhà, đường, tỉnh/thành phố","ss_contact_from",multiline=true,minimumHeight=80,enabled=!s.busy)
    RmField("Địa chỉ nhận hàng",f.to,{v->vm.contactForm{it.copy(to=v.take(500))}},"Số nhà, đường, tỉnh/thành phố","ss_contact_to",multiline=true,minimumHeight=80,enabled=!s.busy)
    SsCheck("Tôi xác nhận thông tin giao nhận cho phiên bản hiện tại",f.confirmed,!s.busy,"ss_contact_check"){v->vm.contactForm{it.copy(confirmed=v)}}
}}
@Composable fun SsIssueScreen(s:SmartSwapState,vm:SmartSwapViewModel){SsContent("ss_issue"){
    RmNotice("Gửi báo cáo sẽ tạm dừng cả vòng. Không xác nhận nhận đúng hàng nếu món nhận có vấn đề.",warning=true)
    LuaChon("Vấn đề gặp phải",s.issueKind,listOf("SaiHang" to "Nhận sai hàng","ChuaNhanHang" to "Chưa nhận hàng"),{vm.issueForm(kind=it)},enabled=!s.busy,tag="ss_issue_kind")
    RmField("Tiêu đề",s.issueTitle,{vm.issueForm(title=it)},"Mô tả ngắn vấn đề","ss_issue_title",enabled=!s.busy)
    RmField("Nội dung báo cáo",s.issueText,{vm.issueForm(text=it)},"Thông tin để nhóm và quản trị kiểm tra","ss_issue_text",multiline=true,minimumHeight=140,enabled=!s.busy)
}}
@Composable fun SsReviewScreen(s:SmartSwapState,vm:SmartSwapViewModel,loader:ImageLoader,user:String){val v=s.circle?:return;SsContent("ss_review"){
    TdNote("Trao đổi thành công!","${v.ma} · Đủ 3/3 người nhận đúng hàng. Cả ba đã hoàn tất giao nhận.")
    v.chang.filter{it.nguoiGiaoId==user||it.nguoiNhanId==user}.forEach{leg->SsProduct(leg.sanPham,v.names()[leg.nguoiGiaoId].orEmpty(),v.names()[leg.nguoiNhanId].orEmpty(),v.thanhVien.first{it.nguoiDungId==leg.nguoiGiaoId}.anhUrl,loader)}
    v.phuongAnHienTai?.let{SsMoney(it.tienBu,v.names())}
    v.thanhVien.filter{it.nguoiDungId!=user}.forEach{m->val existing=s.reviews.firstOrNull{it.nguoiDuocDanhGiaId==m.nguoiDungId};val f=s.ratings[m.nguoiDungId]?:SsRatingForm()
        TdCard{Text("Đánh giá ${m.hoTen}",style=rmText(15,FontWeight.SemiBold))
            Row(Modifier.fillMaxWidth()){(1..5).forEach{star->TextButton({vm.rating(m.nguoiDungId){it.copy(stars=star)}},enabled=!s.busy&&existing==null,contentPadding=PaddingValues(0.dp),modifier=Modifier.weight(1f).heightIn(min=48.dp).testTag("ss_star_${m.nguoiDungId}_$star")){
                Text(if(star<=(existing?.soSao?:f.stars))"★" else "☆",style=rmText(28),color=ReMarketPalette.Primary)}}}
            if(existing==null)RmField("Nhận xét của bạn",f.text,{t->vm.rating(m.nguoiDungId){it.copy(text=t.take(2000))}},"Chia sẻ trải nghiệm trao đổi","ss_review_text_${m.nguoiDungId}",multiline=true,minimumHeight=96,enabled=!s.busy)
            else Text(existing.noiDung,style=rmText(13))
        }
    }
}}
