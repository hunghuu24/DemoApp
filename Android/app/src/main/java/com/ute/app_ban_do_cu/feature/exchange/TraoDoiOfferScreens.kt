package com.ute.app_ban_do_cu.feature.exchange

import androidx.compose.foundation.*
import androidx.compose.foundation.layout.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.testTag
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.unit.dp
import androidx.compose.ui.text.style.TextOverflow
import coil.ImageLoader
import coil.compose.AsyncImage
import com.ute.app_ban_do_cu.core.designsystem.*
import com.ute.app_ban_do_cu.domain.model.*
import com.ute.app_ban_do_cu.feature.catalog.*
import com.ute.app_ban_do_cu.feature.commerce.ngayGio

@Composable fun ChonMonScreen(s:TraoDoiState,vm:TraoDoiViewModel,loader:ImageLoader){
    Column(Modifier.fillMaxSize().testTag("exchange_select").verticalScroll(rememberScrollState()).padding(24.dp),verticalArrangement=Arrangement.spacedBy(16.dp)){
        Text("Chọn một tin của bạn để đổi lấy ${s.target?.ten.orEmpty()}.",style=rmText(14))
        RmField("Tìm trong tin của tôi",s.selectQuery,vm::queryMon,"Nhập tên món đồ","exchange_select_query",enabled=!s.busy)
        TextButton({vm.timMon()},enabled=!s.busy,modifier=Modifier.testTag("exchange_select_search")){Text("Tìm trong tin của tôi")}
        Text("${s.selectable?.tongSo?:0} món có thể trao đổi",style=rmText(13,FontWeight.SemiBold))
        s.selectable?.items?.forEach{tin->TdCard(Modifier.testTag("exchange_select_${tin.id}").clickable(enabled=!s.busy){vm.chon(tin)}){
            Row(verticalAlignment=Alignment.CenterVertically){RadioButton(tin.id==s.selected?.id,{vm.chon(tin)},enabled=!s.busy);Box(Modifier.weight(1f)){TdMon(tin.mon(),loader,if(tin.id==s.selected?.id)"Đã chọn" else tinhTrangText(tin.tinhTrang),framed=false)}}
        }}
        if(s.selectable?.items?.isEmpty()==true)TrangRong("Chưa có món đủ điều kiện","Đăng tin trao đổi và chờ duyệt trước khi gửi đề nghị.",Modifier.testTag("exchange_select_empty"))
        Text("Chỉ các tin đã đăng, đang hiển thị, cho phép 1–1 và chưa được giữ cho giao dịch khác mới được chọn.",style=rmText(12),color=ReMarketPalette.Muted)
        s.selectable?.let{PhanTrang(it.trang,it.tongTrang,!s.busy,{vm.timMon(page=it.trang-1)},{vm.timMon(page=it.trang+1)})}
    }
}
@Composable fun DeNghiFormScreen(s:TraoDoiState,vm:TraoDoiViewModel,loader:ImageLoader,counter:Boolean){
    Column(Modifier.fillMaxSize().testTag(if(counter)"exchange_counter" else "exchange_offer_form").verticalScroll(rememberScrollState()).padding(24.dp),verticalArrangement=Arrangement.spacedBy(18.dp)){
        if(counter)s.offer?.let{TdOfferPair(it,loader)}else{
            s.selected?.let{TdMon(it.mon(),loader,"Đồ gửi trao đổi")};s.target?.let{TdMon(it.mon(),loader,"Đồ nhận")}
        }
        RmField(if(counter)"Mức tiền bù mới (VND)" else "Tiền bù (VND)",s.amount,{vm.deNghiForm(amount=it)},"Nhập 0 nếu không bù tiền","exchange_offer_amount",keyboard=KeyboardType.Number,enabled=!s.busy)
        val sender=if(counter)s.offer?.monGui else s.selected?.mon();val receiver=if(counter)s.offer?.monNhan else s.target?.mon()
        if((s.amount.toLongOrNull()?:0)>0&&sender!=null&&receiver!=null){
            Text("Người trả tiền bù",style=rmText(13,FontWeight.SemiBold))
            listOf(sender,receiver).forEach{mon->Row(Modifier.fillMaxWidth().clickable(enabled=!s.busy){vm.deNghiForm(payer=mon.nguoiDungId)},verticalAlignment=Alignment.CenterVertically){
                RadioButton(s.payer==mon.nguoiDungId,{vm.deNghiForm(payer=mon.nguoiDungId)},enabled=!s.busy);Text(mon.chuSoHuu,style=rmText(13))}}
        }
        Text("Hai bên trao tiền bù trực tiếp khi gặp. Không thanh toán qua ứng dụng; mức bù phải nằm trong điều kiện tin của người trả.",style=rmText(12),color=ReMarketPalette.Muted)
        RmField("Lời nhắn",s.message,{vm.deNghiForm(message=it)},if(counter)"Giải thích đề xuất mới cho đối tác…" else "Giới thiệu món đồ và đề nghị trao đổi…","exchange_offer_message",multiline=true,enabled=!s.busy,minimumHeight=108)
        RmField("Địa điểm dự kiến",s.location,{vm.deNghiForm(location=it)},"Hai bên sẽ xác nhận lại trong chat","exchange_offer_location",enabled=!s.busy)
        RmField("Ngày hẹn (không bắt buộc)",s.date,{vm.deNghiForm(date=it)},"dd/MM/yyyy","exchange_offer_date",enabled=!s.busy)
        RmField("Giờ hẹn",s.time,{vm.deNghiForm(time=it)},"HH:mm","exchange_offer_time",enabled=!s.busy)
        if(counter)TdNote("Cần đối tác đồng ý","Đề xuất mới thay phiên bản cũ và khóa giao nhận cho đến khi hai bên cùng chấp nhận.")
        else TdCheck("Tôi đồng ý quy định trao đổi và mô tả đúng tình trạng món đồ.",s.terms,!s.busy,"exchange_offer_terms"){vm.deNghiForm(terms=it)}
    }
}
@Composable fun DeNghiListScreen(s:TraoDoiState,vm:TraoDoiViewModel,loader:ImageLoader){
    Column(Modifier.fillMaxSize().testTag("exchange_offers_list").verticalScroll(rememberScrollState()).padding(24.dp),verticalArrangement=Arrangement.spacedBy(16.dp)){
        Row(Modifier.horizontalScroll(rememberScrollState()),horizontalArrangement=Arrangement.spacedBy(8.dp)){
            listOf("gui" to "Đã gửi","nhan" to "Đã nhận","thuongluong" to "Thương lượng","hoantat" to "Hoàn thành","tatca" to "Tất cả").forEach{(tab,label)->
                FilterChip(s.tab==tab,{vm.deNghi(tab)},enabled=!s.busy,label={Text(label)},modifier=Modifier.testTag("exchange_tab_$tab"))}
        }
        s.offers?.items?.forEach{o->TdCard(Modifier.testTag("exchange_offer_${o.id}").clickable(enabled=!s.busy){vm.docDeNghi(o.id)}){
            Row(Modifier.fillMaxWidth(),horizontalArrangement=Arrangement.spacedBy(8.dp),verticalAlignment=Alignment.CenterVertically){Text(o.ma,Modifier.weight(1f),style=rmText(12,FontWeight.SemiBold),maxLines=1,overflow=TextOverflow.Ellipsis)
                RmBadge(tenDeNghi(o),Modifier.widthIn(max=170.dp),if(o.trangThai in listOf("TuChoi","DaHuy","HetHan"))RmTone.NEUTRAL else if(o.trangThai=="DeXuatLai")RmTone.PENDING else RmTone.POSITIVE)}
            TdMon(o.monNhan,loader,"Đổi với ${o.monGui.ten}",framed=false);Text(buText(o),style=rmText(13,FontWeight.SemiBold),color=ReMarketPalette.Primary)
            Text(if(o.trangThaiGiaoDich=="HoanTat")"Hoàn thành ${ngayGio(o.ngayHoanTat)}" else "Hạn phản hồi: ${ngayGio(o.ngayHetHan)}",style=rmText(11),color=ReMarketPalette.Muted)
            Text("Xem chi tiết ›",style=rmText(13,FontWeight.SemiBold),color=ReMarketPalette.Primary)
        }}
        if(s.offers?.items?.isEmpty()==true)TrangRong("Chưa có đề nghị","Đề nghị gửi hoặc nhận sẽ hiển thị ở đây.",Modifier.testTag("exchange_offers_empty"))
        s.offers?.let{PhanTrang(it.trang,it.tongTrang,!s.busy,{vm.deNghi(s.tab,it.trang-1)},{vm.deNghi(s.tab,it.trang+1)})}
    }
}
@Composable fun DeNghiDetailScreen(s:TraoDoiState,loader:ImageLoader){val o=s.offer?:return
    Column(Modifier.fillMaxSize().testTag("exchange_offer_detail").verticalScroll(rememberScrollState()).padding(24.dp),verticalArrangement=Arrangement.spacedBy(18.dp)){
        TdNote(tenDeNghi(o),when{ o.trangThaiGiaoDich=="HoanTat"->"Cả hai đã xác nhận nhận đủ đồ và tiền bù. Bạn có thể đánh giá đối tác.";
            o.xacNhan.any{it.daNhan}->"Giao dịch chưa hoàn tất cho đến khi cả hai xác nhận.";
            o.coTheGiaoNhan->"Hai bên đã đồng ý đúng phiên bản hiện tại. Hãy gặp, kiểm tra và bàn giao an toàn.";
            !o.sanPhamConKhaDung&&o.trangThaiGiaoDich!="HoanTat"->"Có món không còn khả dụng để chấp nhận đề nghị.";else->"Hạn phản hồi: ${ngayGio(o.ngayHetHan)}"})
        Text(o.ma,style=rmText(13,FontWeight.SemiBold));TdOfferPair(o,loader)
        o.lyDoKetThuc?.let{Text("Lý do: $it",style=rmText(13),color=ReMarketPalette.Muted)}
        o.thoaThuan?.let{p->TdCard{Text("Phiên bản ${p.soPhienBan} · ${if(p.trangThai=="DaChot")"Đã chốt" else "Cần phản hồi"}",style=rmText(14,FontWeight.SemiBold));Text(p.loiNhan,style=rmText(14))
            p.diaDiemHen?.let{Text("Địa điểm: $it",style=rmText(13))};p.thoiGianHen?.let{Text("Lịch hẹn: ${ngayGio(it)}",style=rmText(13))}}}
        TdTimeline(o)
        TdCard{Text("Xác nhận giao nhận",style=rmText(16,FontWeight.SemiBold));o.xacNhan.forEach{x->
            Text((if(x.daNhan)"✓ " else "○ ")+x.hoTen+(if(x.daNhan)" đã nhận đủ" else " · Chờ xác nhận"),style=rmText(13));x.ngayNhan?.let{Text(ngayGio(it),style=rmText(11),color=ReMarketPalette.Muted)}}
            if((o.thoaThuan?.soTienBu?:0)>0){Text("Xác nhận tiền bù: ${if(o.thoaThuan?.nguoiTraDaXacNhan==true)"Bên trả đã xác nhận" else "Chờ bên trả"} · ${if(o.thoaThuan?.nguoiNhanDaXacNhan==true)"Bên nhận đã xác nhận" else "Chờ bên nhận"}",style=rmText(12),color=ReMarketPalette.Muted)}}
        if(o.bangChung.isNotEmpty()){Text("Bằng chứng giao nhận",style=rmText(16,FontWeight.SemiBold));o.bangChung.forEach{p->AnhSanPham(p.url,loader,Modifier.fillMaxWidth().height(180.dp),"Bằng chứng giao nhận")}}
        o.danhGia.forEach{review->TdCard{Text("${if(review.nguoiDanhGiaId==o.nguoiGuiId)o.monGui.chuSoHuu else o.monNhan.chuSoHuu} · ${review.soSao}/5",style=rmText(14,FontWeight.SemiBold));Text(review.noiDung,style=rmText(13));Text(ngayGio(review.ngayTao),style=rmText(11),color=ReMarketPalette.Muted)}}
    }
}
@Composable fun GiaoNhanTraoDoiScreen(s:TraoDoiState,vm:TraoDoiViewModel,loader:ImageLoader,pick:()->Unit){val o=s.offer?:return
    Column(Modifier.fillMaxSize().testTag("exchange_handover").verticalScroll(rememberScrollState()).padding(24.dp),verticalArrangement=Arrangement.spacedBy(18.dp)){
        Text(o.ma,style=rmText(14,FontWeight.SemiBold));TdOfferPair(o,loader)
        TdCheck("Tôi đã kiểm tra món nhận, đúng tình trạng mô tả.",s.checked,!s.busy,"exchange_check_item"){vm.kiemTra(checked=it)}
        TdCheck("Tôi đã giao món của mình, nhận món của đối tác và trả/nhận đủ tiền bù nếu có.",s.received,!s.busy,"exchange_check_received"){vm.kiemTra(received=it)}
        TdButton("Thêm bằng chứng giao nhận",pick,Modifier.testTag("exchange_add_evidence"),!s.busy,outlined=true)
        Text("Ảnh món đồ / biên nhận · JPG, PNG · Tối đa 5 ảnh, 5 MB mỗi ảnh",style=rmText(11),color=ReMarketPalette.Muted)
        s.evidence.forEachIndexed{index,p->TdCard{AsyncImage(p.bytes,"Ảnh bằng chứng đã chọn",imageLoader=loader,modifier=Modifier.fillMaxWidth().height(140.dp))
            TextButton({vm.boBangChung(p.id)},enabled=!s.busy){Text("Bỏ ảnh ${index+1}")}}}
        if(o.daXacNhanCuaToi&&s.evidence.isNotEmpty())RmNotice("Phía bạn đã xác nhận. Tiếp tục tải các ảnh còn lại để bổ sung bằng chứng.")
        TdNote("Chỉ hoàn tất khi cả hai xác nhận","Giữ bằng chứng giao nhận. Ứng dụng ghi nhận xác nhận của hai bên, không chuyển tiền bù.")
    }
}
@Composable fun DanhGiaTraoDoiScreen(s:TraoDoiState,vm:TraoDoiViewModel,loader:ImageLoader){val o=s.offer?:return
    Column(Modifier.fillMaxSize().testTag("exchange_rating").verticalScroll(rememberScrollState()).padding(24.dp),verticalArrangement=Arrangement.spacedBy(20.dp)){
        TdNote("Trao đổi thành công!","${o.ma} · Hai bên đã xác nhận ${ngayGio(o.ngayHoanTat)}.");TdOfferPair(o,loader)
        Text("Bạn thấy trao đổi với đối tác thế nào?",style=rmText(17,FontWeight.SemiBold))
        Row(Modifier.fillMaxWidth(),horizontalArrangement=Arrangement.SpaceEvenly){(1..5).forEach{star->TextButton({vm.danhGiaForm(stars=star)},Modifier.size(48.dp).testTag("exchange_star_$star"),contentPadding=PaddingValues(0.dp),enabled=!s.busy){Text(if(star<=s.stars)"★" else "☆",style=rmText(32),color=ReMarketPalette.Primary)}}}
        s.criteria.forEach{tag->FilterChip(tag.id in s.tags,{vm.tag(tag.id)},label={Text(tag.ten)},enabled=!s.busy,modifier=Modifier.testTag("exchange_tag_${tag.ma}"))}
        RmField("Nhận xét của bạn",s.rating,{vm.danhGiaForm(text=it)},"Chia sẻ trải nghiệm thực tế…","exchange_rating_text",multiline=true,enabled=!s.busy,minimumHeight=108)
        Text("Đánh giá giúp cộng đồng trao đổi an tâm hơn.",style=rmText(12),color=ReMarketPalette.Muted)
    }
}
@Composable fun DanhGiaTraoDoiXong(s:TraoDoiState,loader:ImageLoader){Column(Modifier.fillMaxSize().testTag("exchange_rating_success").verticalScroll(rememberScrollState()).padding(24.dp),verticalArrangement=Arrangement.spacedBy(20.dp)){
    TdNote("Đã gửi đánh giá","Cảm ơn bạn đã chia sẻ trải nghiệm trao đổi với cộng đồng.");s.offer?.let{TdOfferPair(it,loader)}
}}
