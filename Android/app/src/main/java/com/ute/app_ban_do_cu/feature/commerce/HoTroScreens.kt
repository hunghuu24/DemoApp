package com.ute.app_ban_do_cu.feature.commerce

import androidx.compose.foundation.*
import androidx.compose.foundation.layout.*
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.testTag
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import com.ute.app_ban_do_cu.core.designsystem.*
import com.ute.app_ban_do_cu.feature.catalog.TrangRong

@Composable
fun HoTroListScreen(s:MuaBanState,vm:MuaBanViewModel) {
    Column(Modifier.fillMaxSize().testTag("support_list").verticalScroll(rememberScrollState()).padding(24.dp),verticalArrangement=Arrangement.spacedBy(16.dp)) {
        if(!s.adminSupport) MbButton("Gửi yêu cầu mới",{vm.moHoTro()},Modifier.testTag("new_support"),enabled=!s.busy)
        s.supports?.forEach{item->MbCard(Modifier.clickable(enabled=!s.busy){vm.docHoTro(item.id)}.testTag("support_${item.id}")) {
            Text(item.maDon?:item.ma,style=rmText(14,FontWeight.SemiBold))
            Text(item.tieuDe,style=rmText(13))
            Text(tenTrangThai(item.trangThai),style=rmText(12),color=MuaBanColors.Green)
            Text(ngayGio(item.ngayTao),style=rmText(11),color=MuaBanColors.Muted)
        }}
        if(s.supports?.isEmpty()==true) TrangRong("Chưa có yêu cầu hỗ trợ","Các yêu cầu đã gửi sẽ xuất hiện tại đây.")
        Row(Modifier.fillMaxWidth(),horizontalArrangement=Arrangement.SpaceBetween) {
            TextButton({vm.hoTroList(s.supportPage-1,s.adminSupport)},enabled=!s.busy&&s.supportPage>1){Text("Trang trước")}
            TextButton({vm.hoTroList(s.supportPage+1,s.adminSupport)},enabled=!s.busy&&s.supports?.size==10){Text("Trang sau")}
        }
    }
}
@Composable
fun TaoHoTroScreen(s:MuaBanState,vm:MuaBanViewModel) {
    Column(Modifier.fillMaxSize().testTag("support_form").verticalScroll(rememberScrollState()).padding(24.dp),verticalArrangement=Arrangement.spacedBy(18.dp)) {
        MbNote("ReMarket luôn sẵn sàng hỗ trợ","Mô tả vấn đề để đội ngũ kiểm tra giúp bạn.")
        Text("Đơn hàng liên quan (không bắt buộc)",style=rmText(13,FontWeight.SemiBold))
        MbCard {
            Text(s.supportOrderCode?:"Không gắn đơn hàng",style=rmText(13),color=MuaBanColors.Muted)
            Row(horizontalArrangement=Arrangement.spacedBy(12.dp)) {
                TextButton({vm.donHang(selectSupport=true)},enabled=!s.busy,modifier=Modifier.testTag("support_choose_order")){Text("Chọn đơn")}
                if(s.supportOrderId!=null) TextButton(vm::boDonHoTro,enabled=!s.busy){Text("Bỏ gắn đơn")}
            }
        }
        Text("Nội dung cần hỗ trợ",style=rmText(14,FontWeight.SemiBold))
        listOf("DonHangVanChuyen" to "Đơn hàng / vận chuyển","ThanhToanHoanTien" to "Thanh toán / hoàn tiền","TaiKhoanKhac" to "Tài khoản / vấn đề khác").forEach{(id,label)->
            Row(Modifier.fillMaxWidth().clickable(enabled=!s.busy){vm.supportForm(kind=id)},horizontalArrangement=Arrangement.spacedBy(6.dp)) {
                RadioButton(s.supportKind==id,{vm.supportForm(kind=id)},enabled=!s.busy)
                Text(label,Modifier.padding(top=14.dp),style=rmText(13))
            }
        }
        RmField("Tiêu đề",s.supportTitle,{vm.supportForm(title=it)},"Tóm tắt vấn đề cần hỗ trợ","support_title",enabled=!s.busy)
        RmField("Mô tả chi tiết",s.supportText,{vm.supportForm(text=it)},"Nhập vấn đề bạn gặp phải...","support_description",multiline=true,enabled=!s.busy)
        Text("Bạn có thể bổ sung thông tin trong trang theo dõi yêu cầu.",style=rmText(12),color=MuaBanColors.Muted)
    }
}
@Composable
fun HoTroDetailScreen(s:MuaBanState,vm:MuaBanViewModel) {
    val item=s.support?:return
    Column(Modifier.fillMaxSize().testTag("support_detail").verticalScroll(rememberScrollState()).padding(24.dp),verticalArrangement=Arrangement.spacedBy(20.dp)) {
        Text("Mã yêu cầu: ${item.ma}",style=rmText(14,FontWeight.SemiBold))
        Text(tenTrangThai(item.trangThai),style=rmText(16,FontWeight.SemiBold),color=MuaBanColors.Green)
        Text(item.tieuDe,style=rmText(18,FontWeight.SemiBold))
        Text("Đã gửi ${ngayGio(item.ngayTao)}",style=rmText(12),color=MuaBanColors.Muted)
        item.maDon?.let{Text("Đơn hàng: $it",style=rmText(13))}
        MbCard{Text(item.noiDung,style=rmText(14))}
        MbCard {
            item.lichSu.forEach{MocLichSu("${tenTrangThai(it.trangThai)} · ${it.nguoiGui}","${it.noiDung}\n${ngayGio(it.ngayTao)}")}
            if(item.trangThai!="DaGiaiQuyet") MocLichSu("Đã giải quyết","Chưa hoàn tất",false)
        }
        if(item.trangThai!="DaGiaiQuyet") {
            RmField(if(s.adminSupport)"Phản hồi của quản trị" else "Bổ sung thông tin",s.reply,vm::traLoiText,"Nhập nội dung...","support_reply",multiline=true,enabled=!s.busy)
            if(s.adminSupport) Row(horizontalArrangement=Arrangement.spacedBy(10.dp)) {
                MbButton("Tiếp nhận",{vm.phanHoi("DangXuLy")},Modifier.weight(1f).testTag("support_process"),enabled=!s.busy&&s.reply.trim().length>=5,outlined=true)
                MbButton("Giải quyết",{vm.phanHoi("DaGiaiQuyet")},Modifier.weight(1f).testTag("support_resolve"),enabled=!s.busy&&s.reply.trim().length>=5)
            } else MbButton("Gửi bổ sung",{vm.phanHoi()},Modifier.testTag("support_reply_submit"),enabled=!s.busy&&s.reply.trim().length>=5,outlined=true)
        }
        MbNote("Lưu ý","Đội ngũ sẽ cập nhật kết quả tại đây. Bạn có thể gửi yêu cầu mới khi cần hỗ trợ thêm.")
    }
}
