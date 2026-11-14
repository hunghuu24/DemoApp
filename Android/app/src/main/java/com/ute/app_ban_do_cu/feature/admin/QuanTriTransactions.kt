package com.ute.app_ban_do_cu.feature.admin

import androidx.compose.foundation.*
import androidx.compose.foundation.layout.*
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.ui.platform.testTag
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import coil.ImageLoader
import com.ute.app_ban_do_cu.core.designsystem.*
import com.ute.app_ban_do_cu.domain.model.*
import com.ute.app_ban_do_cu.feature.catalog.*
import com.ute.app_ban_do_cu.feature.commerce.ngayGio
import com.ute.app_ban_do_cu.feature.exchange.TdCard
import com.ute.app_ban_do_cu.feature.smartswap.SsCheck

@Composable fun QtTrades(s:QuanTriState,vm:QuanTriViewModel){QtBody("qt_trades"){
    QtTabs(s.type,listOf(null to "Tất cả","MuaBan" to "Mua bán","TraoDoiMotMot" to "Trao đổi 1–1","SmartSwap" to "Smart Swap 3 bên"),!s.busy){vm.trades(type=it)}
    QtSearch(s,vm,"Tìm theo mã giao dịch"){vm.trades()}
    LuaChon("Trạng thái",s.status.orEmpty(),listOf("" to "Tất cả trạng thái","ChoDongY" to "Chờ đồng ý","DangThucHien" to "Đang thực hiện","HoanTat" to "Hoàn tất","TranhChap" to "Tranh chấp","DaHuy" to "Đã hủy"),{vm.trades(status=it.ifBlank{null})},tag="qt_trade_status",enabled=!s.busy)
    s.trades?.let{p->Text("${p.tongSo} giao dịch",style=rmText(13),color=ReMarketPalette.Muted);p.items.forEach{t->QtTradeCard(t,!s.busy){vm.trade(t.id)}};if(p.items.isEmpty())TrangRong("Chưa có giao dịch phù hợp","Chọn loại hoặc trạng thái khác.");PhanTrang(p.trang,p.tongTrang,!s.busy,{vm.trades(page=p.trang-1)},{vm.trades(page=p.trang+1)})}
}}
@Composable fun QtTradeScreen(s:QuanTriState,vm:QuanTriViewModel,loader:ImageLoader){val d=s.trade?:return;val t=d.giaoDich;val names=t.thanhVien.associate{it.nguoiDungId to it.hoTen}
    QtBody("qt_trade_detail"){
        Text(t.ma,style=rmText(18,FontWeight.SemiBold));QtBadge(t.trangThai);Text("${qtStatus(t.loai)} · ${ngayGio(t.ngayTao)}",style=rmText(12),color=ReMarketPalette.Muted)
        if(d.vongId!=null){Text("${t.soNguoiNhan}/${t.tongNguoiNhan} người nhận đúng hàng",Modifier.testTag("qt_receipt_progress"),style=rmText(16,FontWeight.SemiBold),color=ReMarketPalette.Primary)
            TdCard{Text("Sơ đồ giao sản phẩm",style=rmText(16,FontWeight.SemiBold));d.chang.forEach{leg->Text("${names[leg.nguoiGiaoId]} → ${names[leg.nguoiNhanId]}\n${t.sanPham.firstOrNull{it.id==leg.sanPhamId}?.ten.orEmpty()}",style=rmText(13));HorizontalDivider(color=ReMarketPalette.Border)}}}
        t.sanPham.forEach{p->TdCard{Row(horizontalArrangement=Arrangement.spacedBy(12.dp)){AnhSanPham(p.anhUrl,loader,Modifier.size(64.dp).clip(RoundedCornerShape(14.dp)),p.ten);Column(Modifier.weight(1f)){Text(p.ten,style=rmText(15,FontWeight.SemiBold));Text(tien(p.giaTri),style=rmText(14,FontWeight.Bold),color=ReMarketPalette.Primary);Text(p.chuSoHuu,style=rmText(11),color=ReMarketPalette.Muted)}}}}
        t.thanhVien.forEach{m->TdCard{Text(m.hoTen,style=rmText(14,FontWeight.SemiBold));Text("Giao: ${if(m.daGiao)"Đã xác nhận" else "Chưa xác nhận"} · Nhận: ${if(m.daNhan)"Đã nhận đúng" else "Chưa xác nhận"}",style=rmText(12));TextButton({vm.user(m.nguoiDungId)},enabled=!s.busy){Text("Chi tiết người dùng ›")}}}
        if(d.trangThaiDonHang!=null)TdCard{Text("Đơn hàng: ${qtStatus(d.trangThaiDonHang)}",style=rmText(14,FontWeight.SemiBold));Text("Thanh toán mô phỏng: ${qtStatus(d.trangThaiThanhToan.orEmpty())}",style=rmText(13));Text("Tổng tiền: ${tien(d.tongTienDon)}",style=rmText(13))}
        d.diaDiemHen?.let{Text("Hẹn giao nhận: $it",style=rmText(13))};d.thoiGianHen?.let{Text(ngayGio(it),style=rmText(12))}
        Text("Thỏa thuận và tiền bù",style=rmText(16,FontWeight.SemiBold));d.phienBan.forEach{p->TdCard{Text("Phiên bản ${p.soPhienBan} · ${qtStatus(p.trangThai)}",style=rmText(14,FontWeight.SemiBold));Text(p.noiDung,style=rmText(12))
            p.tienBu.forEach{m->Text("${names[m.nguoiTraId]} → ${names[m.nguoiNhanId]}: ${tien(m.soTien)}",style=rmText(13),color=ReMarketPalette.Primary);Text("Người trả: ${if(m.nguoiTraDaXacNhan)"Đã xác nhận" else "Chờ"} · Người nhận tiền: ${if(m.nguoiNhanDaXacNhan)"Đã xác nhận" else "Chờ"}",style=rmText(11))}
            if(p.tienBu.isEmpty())Text("Không bù tiền",style=rmText(12));p.dongY.forEach{vote->Text("${names[vote.nguoiDungId]}: ${when(vote.quyetDinh){"DongY"->"Đã đồng ý";"TuChoi"->"Từ chối";else->"Chờ"}}",style=rmText(12))}
        }}
        Text("Vận chuyển mô phỏng",style=rmText(16,FontWeight.SemiBold));d.vanChuyen.forEach{ship->TdCard{Text("${names[ship.nguoiGuiId].orEmpty()} → ${names[ship.nguoiNhanId].orEmpty()}",style=rmText(14,FontWeight.SemiBold));Text("Mã: ${ship.maVanDon}",style=rmText(12));Text(qtStatus(ship.trangThai),style=rmText(13),color=ReMarketPalette.Primary);Text(if(ship.ngayNhanDungHang!=null)"Người nhận đã tự xác nhận đúng hàng" else "Chưa có xác nhận đúng hàng của người nhận",style=rmText(12));QtTimeline(ship.lichSu)}}
        Text("Bằng chứng",style=rmText(16,FontWeight.SemiBold));QtEvidenceList(d.bangChung,vm,loader)
        Text("Lịch sử và chat",style=rmText(16,FontWeight.SemiBold));Text("${d.tinNhan.size}/${d.tongTinNhan} tin nhắn",style=rmText(11),color=ReMarketPalette.Muted)
        if(d.tinNhan.size<d.tongTinNhan)QtButton("Xem tin cũ hơn","qt_older_messages",enabled=!s.busy,outline=true){vm.older()}
        d.tinNhan.forEach{m->TdCard{Text("${m.nguoiGui} · ${ngayGio(m.ngayTao)}",style=rmText(11),color=ReMarketPalette.Muted);m.noiDung?.let{Text(it,style=rmText(13))};QtEvidenceList(m.anh,vm,loader)}}
        Text("Khiếu nại liên quan",style=rmText(16,FontWeight.SemiBold));d.khieuNai.forEach{k->QtComplaintCard(k,!s.busy){vm.complaint(k.id)}}
        if(d.khieuNai.isEmpty())Text("Chưa có khiếu nại cho giao dịch.",style=rmText(12),color=ReMarketPalette.Muted)
        if(t.trangThai !in listOf("ChoDongY","DaHuy"))QtButton("Tạo xử lý tranh chấp","qt_complaint_create",enabled=!s.busy,outline=true){vm.createComplaint()}
        RmNotice("Chỉ xem xác nhận của thành viên. Admin không xác nhận nhận hàng hoặc tiền bù thay người dùng.")
    }
}
@Composable fun QtComplaints(s:QuanTriState,vm:QuanTriViewModel){QtBody("qt_complaints"){
    QtTabs(s.status,listOf(null to "Tất cả","Moi" to "Mới","DangXuLy" to "Đang xử lý","ChoThucHien" to "Chờ thực hiện","DaGiaiQuyet" to "Đã giải quyết"),!s.busy){vm.complaints(it)}
    QtSearch(s,vm,"Tìm mã phiếu hoặc giao dịch"){vm.complaints()};Text("Ưu tiên hạn phản hồi gần nhất",style=rmText(12),color=ReMarketPalette.Muted)
    s.complaints?.let{p->Text("${p.tongSo} khiếu nại",style=rmText(13));p.items.forEach{k->QtComplaintCard(k,!s.busy){vm.complaint(k.id)}};if(p.items.isEmpty())TrangRong("Không có khiếu nại phù hợp","Thử trạng thái hoặc từ khóa khác.");PhanTrang(p.trang,p.tongTrang,!s.busy,{vm.complaints(page=p.trang-1)},{vm.complaints(page=p.trang+1)})}
}}
@Composable private fun QtComplaintCard(k:QtComplaint,enabled:Boolean,click:()->Unit){TdCard(Modifier.testTag("qt_complaint_${k.id}").clickable(enabled=enabled,onClick=click)){
    Text("${k.ma} · ${k.maGiaoDich}",style=rmText(14,FontWeight.SemiBold));QtBadge(k.trangThai);Text(k.tieuDe,style=rmText(14));Text("${k.nguoiGui} → ${k.nguoiBiKhieuNai}",style=rmText(12),color=ReMarketPalette.Muted);k.hanPhanHoi?.let{Text("Hạn phản hồi: ${ngayGio(it)}",style=rmText(11))};Text("Chi tiết ›",style=rmText(12),color=ReMarketPalette.Primary)
}}
@Composable fun QtEvidenceList(rows:List<QtEvidence>,vm:QuanTriViewModel,loader:ImageLoader){rows.forEach{e->TdCard(Modifier.testTag("qt_evidence_${e.id}").clickable{vm.evidence(e)}){if(e.loaiMime.startsWith("image/"))AnhSanPham(e.url,loader,Modifier.fillMaxWidth().height(150.dp),e.moTa)
    Text(e.moTa.ifBlank{if(e.loaiMime=="video/mp4")"Video bằng chứng ›" else "Ảnh bằng chứng ›"},style=rmText(13,FontWeight.SemiBold));Text(ngayGio(e.ngayTao),style=rmText(11),color=ReMarketPalette.Muted)}}}
@Composable fun QtComplaintScreen(s:QuanTriState,vm:QuanTriViewModel,loader:ImageLoader){val d=s.complaint?:return;val k=d.khieuNai;QtBody("qt_complaint_detail"){
    Text("${k.ma} · ${k.maGiaoDich}",style=rmText(17,FontWeight.SemiBold));QtBadge(k.trangThai);TdCard{Text("Người gửi: ${k.nguoiGui}",style=rmText(14));Text("Bị phản ánh: ${k.nguoiBiKhieuNai}",style=rmText(14));Text(k.tieuDe,style=rmText(15,FontWeight.SemiBold));Text(k.noiDung,style=rmText(13))}
    QtButton("Xem giao dịch và chat","qt_complaint_trade",enabled=!s.busy,outline=true){vm.trade(k.giaoDichId)}
    Text("Bằng chứng",style=rmText(16,FontWeight.SemiBold));QtEvidenceList(d.bangChung,vm,loader);if(d.bangChung.isEmpty())Text("Chưa có tệp bằng chứng. Xem chat/giao dịch hoặc yêu cầu các bên bổ sung.",style=rmText(12),color=ReMarketPalette.Muted)
    Text("Lịch sử phản hồi",style=rmText(16,FontWeight.SemiBold));QtTimeline(d.phanHoi)
    d.quyetDinh.forEach{p->TdCard{Text("Kết luận: ${p.ketLuan}",style=rmText(14,FontWeight.SemiBold));Text("Căn cứ: ${p.canCu}",style=rmText(12));Text("Biện pháp: ${p.bienPhap}",style=rmText(13));Text(p.thongBao,style=rmText(12));Text(if(p.daXacMinhThucHien)"Đã xác minh thực hiện" else "Chờ xác minh thực hiện",style=rmText(12),color=ReMarketPalette.Primary)}}
}}
@Composable fun QtDecisionScreen(s:QuanTriState,vm:QuanTriViewModel){val f=s.decision;val k=s.complaint?.khieuNai?:return;QtBody("qt_decision"){
    Text("${k.maGiaoDich} · ${k.nguoiGui} và ${k.nguoiBiKhieuNai}",style=rmText(13),color=ReMarketPalette.Muted)
    RmField("Căn cứ đã xác minh",f.evidence,{v->vm.decisionForm{it.copy(evidence=v.take(2000))}},"Đối chiếu tin, chat và bằng chứng","qt_decision_evidence",multiline=true,minimumHeight=100,enabled=!s.busy)
    RmField("Kết luận",f.conclusion,{v->vm.decisionForm{it.copy(conclusion=v.take(2000))}},"Kết luận về khiếu nại","qt_decision_conclusion",multiline=true,enabled=!s.busy)
    RmField("Biện pháp xử lý",f.action,{v->vm.decisionForm{it.copy(action=v.take(2000))}},"Nêu rõ các bên cần thực hiện","qt_decision_action",multiline=true,minimumHeight=80,enabled=!s.busy)
    val directions=if(k.loaiGiaoDich=="MuaBan")listOf("GhiNhan" to "Ghi nhận xử lý thủ công")else listOf("TiepTuc" to "Tiếp tục bàn giao sau xác minh","KetThuc" to "Kết thúc sau khi xác minh hoàn trả","GhiNhan" to "Giữ tạm dừng để xử lý tiếp")
    LuaChon("Sau khi xác minh thực hiện",f.direction,directions,{v->vm.decisionForm{it.copy(direction=v)}},tag="qt_decision_direction",enabled=!s.busy)
    RmField("Nội dung thông báo cho các bên",f.message,{v->vm.decisionForm{it.copy(message=v.take(2000))}},"Hướng dẫn và thời hạn thực hiện","qt_decision_message",multiline=true,minimumHeight=100,enabled=!s.busy)
    SsCheck("Gửi thông báo cho tất cả thành viên liên quan",f.notify,!s.busy,"qt_decision_notify"){v->vm.decisionForm{it.copy(notify=v)}}
    RmNotice("Quyết định được ghi nhật ký. Chỉ đóng khi đã xác minh thực hiện; ứng dụng không tự chuyển/hoàn tiền hoặc xác nhận nhận hàng.")
}}
@Composable fun QtVerifyScreen(s:QuanTriState,vm:QuanTriViewModel){QtBody("qt_verify"){
    Text("Xác minh thực hiện quyết định",style=rmText(18,FontWeight.SemiBold));RmNotice("Đây là xác minh việc thực hiện biện pháp tranh chấp. Người nhận vẫn phải tự kiểm tra và xác nhận hàng trong giao dịch.")
    s.complaint?.quyetDinh?.firstOrNull{!it.daXacMinhThucHien}?.let{Text(it.bienPhap,style=rmText(14,FontWeight.SemiBold))}
    RmField("Căn cứ xác minh",s.verifyText,{vm.verifyForm(text=it)},"Bằng chứng các bên đã thực hiện quyết định","qt_verify_text",multiline=true,minimumHeight=140,enabled=!s.busy)
    SsCheck("Tôi đã rà soát bằng chứng thực hiện của các bên",s.verified,!s.busy,"qt_verify_check"){vm.verifyForm(checked=it)}
}}
@Composable fun QtCreateComplaintScreen(s:QuanTriState,vm:QuanTriViewModel){val t=s.trade?.giaoDich?:return;val f=s.complaintForm;QtBody("qt_complaint_create_form"){
    Text(t.ma,style=rmText(17,FontWeight.SemiBold))
    LuaChon("Người phản ánh",f.reporter,t.thanhVien.map{it.nguoiDungId to it.hoTen},{v->vm.complaintForm{it.copy(reporter=v)}},tag="qt_complaint_reporter",enabled=!s.busy)
    LuaChon("Người bị phản ánh",f.accused,t.thanhVien.filter{it.nguoiDungId!=f.reporter}.map{it.nguoiDungId to it.hoTen},{v->vm.complaintForm{it.copy(accused=v)}},tag="qt_complaint_accused",enabled=!s.busy)
    RmField("Vấn đề cần xử lý",f.title,{v->vm.complaintForm{it.copy(title=v.take(200))}},"Nêu vấn đề","qt_complaint_title",enabled=!s.busy)
    RmField("Nội dung đã đối chiếu",f.text,{v->vm.complaintForm{it.copy(text=v.take(4000))}},"Phản ánh của thành viên và căn cứ tiếp nhận","qt_complaint_text",multiline=true,minimumHeight=140,enabled=!s.busy)
    SsCheck("Đã đối chiếu phản ánh với các thành viên",f.checked,!s.busy,"qt_complaint_create_check"){v->vm.complaintForm{it.copy(checked=v)}}
    RmNotice("Phản ánh được ghi nhận bởi quản trị và gửi đến các thành viên. Trao đổi đang thực hiện sẽ tạm dừng để xử lý.")
}}
