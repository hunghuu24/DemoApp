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

@Composable fun SsPreviewScreen(s:SmartSwapState,vm:SmartSwapViewModel,loader:ImageLoader,user:String){val p=s.preview?:return;val v=p.vong;val names=p.hoSo.associate{it.nguoiDungId to it.hoTen}
    SsContent("ss_preview"){
        SsBadge("3 bên · Phù hợp ${v.diemPhuHop}%")
        SsDiagram(v.chang,names,user)
        TdCard{ssOrder(v.chang,user).forEachIndexed{i,leg->if(i>0)HorizontalDivider(color=ReMarketPalette.Border);SsProduct(leg.sanPham,names[leg.nguoiGiaoId].orEmpty(),names[leg.nguoiNhanId].orEmpty(),s.photos[leg.sanPham.sanPhamId],loader)}}
        SsMoney(v.tienBu,names)
        Text("Gửi đề nghị chưa mở bàn giao. Cần cả 3 thành viên đồng ý trước khi giao nhận.",style=rmText(12),color=ReMarketPalette.Muted)
        RmField("Lời nhắn cho hai thành viên",s.message,{vm.invitation(text=it)},"Giới thiệu đề nghị của bạn","ss_invitation_message",multiline=true,minimumHeight=80,enabled=!s.busy)
        SsCheck("Tôi đã xem ba sản phẩm, chiều trao đổi và đồng ý phương án tiền bù",s.checked,!s.busy,"ss_invitation_consent"){vm.invitation(checked=it)}
    }
}
@Composable fun SsCircleScreen(s:SmartSwapState,vm:SmartSwapViewModel,loader:ImageLoader,user:String,consent:Boolean){val v=s.circle?:return;val p=v.phuongAnHienTai
    SsContent(if(consent)"ss_consent" else "ss_detail"){
        Text(v.ma,style=rmText(15,FontWeight.SemiBold));SsBadge(ssStatus(v),v.trangThai=="TamDung")
        Text("${p?.dongY?.count{it.quyetDinh=="DongY"}?:0}/3 đã đồng ý",Modifier.testTag("ss_consent_progress"),style=rmText(17,FontWeight.SemiBold),color=ReMarketPalette.Primary)
        LinearProgressIndicator(progress={(p?.dongY?.count{it.quyetDinh=="DongY"}?:0)/3f},modifier=Modifier.fillMaxWidth().height(6.dp),trackColor=ReMarketPalette.Soft)
        if(v.trangThai=="ChoDongY")Text("Hết hạn: ${ngayGio(v.ngayHetHan)}",style=rmText(11),color=ReMarketPalette.Muted)
        v.lyDoKetThuc?.let{RmNotice(it,warning=true)}
        if(v.trangThai=="TamDung")RmNotice("Giao dịch đang tạm dừng do báo cáo sự cố. Chưa thể tiếp tục gửi hoặc xác nhận nhận hàng.",warning=true)
        if(consent)Text("Kiểm tra sản phẩm và tiền bù trước khi đồng ý. Chỉ mở bàn giao sau khi đủ 3/3.",style=rmText(12),color=ReMarketPalette.Muted)
        SsDiagram(v.chang,v.names(),v.nguoiKhoiTaoId?:user)
        if(!consent)TdCard{ssOrder(v.chang,v.nguoiKhoiTaoId?:user).forEachIndexed{i,leg->if(i>0)HorizontalDivider(color=ReMarketPalette.Border);SsProduct(leg.sanPham,v.names()[leg.nguoiGiaoId].orEmpty(),v.names()[leg.nguoiNhanId].orEmpty(),v.thanhVien.first{it.nguoiDungId==leg.nguoiGiaoId}.anhUrl,loader)}}
        v.thanhVien.forEach{m->val vote=p?.dongY?.firstOrNull{it.nguoiDungId==m.nguoiDungId}
            TdCard{Row(Modifier.fillMaxWidth(),verticalAlignment=androidx.compose.ui.Alignment.CenterVertically,horizontalArrangement=Arrangement.spacedBy(8.dp)){
                Text("${m.hoTen}${if(m.nguoiDungId==user)" (Bạn)" else ""}",Modifier.weight(1f),style=rmText(14,FontWeight.SemiBold))
                SsBadge(when(vote?.quyetDinh){"DongY"->"Đã đồng ý";"TuChoi"->"Từ chối";else->"Chờ"},vote?.quyetDinh=="TuChoi")
            }
                Text("Giao ${m.sanPhamCho.ten} → ${v.names()[m.nguoiNhanMonCuaToiId]}",style=rmText(12))
                Text("Nhận ${v.chang.first{it.nguoiNhanId==m.nguoiDungId}.sanPham.ten} từ ${v.names()[m.nguoiGiaoChoToiId]}",style=rmText(12),color=ReMarketPalette.Muted)}
        }
        p?.let{SsMoney(it.tienBu,v.names());Text("Phiên bản ${it.soPhienBan} · ${it.loiNhan}",style=rmText(12))}
        if(consent&&v.coThePhanHoi)SsCheck("Tôi đã đọc và đồng ý sản phẩm, tiền bù của phiên bản ${p?.soPhienBan}",s.checked,!s.busy,"ss_response_consent",vm::checked)
        if(consent&&v.trangThai=="ChoDongY")Text("Nếu một người từ chối, vòng sẽ dừng. Bàn giao đang khóa cho đến khi đủ 3/3 đồng ý.",style=rmText(12),color=ReMarketPalette.Muted)
        if(!consent){Text("Lịch sử phương án tiền bù",style=rmText(15,FontWeight.SemiBold))
            v.lichSu.forEach{version->TdCard{Text("Phiên bản ${version.soPhienBan}${if(version.id==p?.id)" · Hiện tại" else " · Đã thay thế"}",style=rmText(13,FontWeight.SemiBold));Text(version.loiNhan,style=rmText(12))
                version.tienBu.forEach{m->Text("${v.names()[m.nguoiTraId]} → ${v.names()[m.nguoiNhanId]}: ${tien(m.soTien)}",style=rmText(12))}
                Text("${version.dongY.count{it.quyetDinh=="DongY"}}/3 đồng ý · ${ngayGio(version.ngayTao)}",style=rmText(11),color=ReMarketPalette.Muted)}}
        }
    }
}
@Composable fun SsCompensation(s:SmartSwapState,vm:SmartSwapViewModel,user:String){val v=s.circle?:return
    val partners=v.thanhVien.filter{it.nguoiDungId!=user};var direction by remember{mutableStateOf("Tra")};var partner by remember{mutableStateOf(partners.firstOrNull()?.nguoiDungId.orEmpty())};var amount by remember{mutableStateOf("")}
    SsContent("ss_compensation"){
        Text("Điều chỉnh tiền bù",style=rmText(20,FontWeight.SemiBold));Text("Chọn số tiền và thành viên liên quan đến khoản bù của bạn.",style=rmText(12),color=ReMarketPalette.Muted)
        if(v.phuongAnHienTai?.id!=s.baseVersion)RmNotice("Đã có phiên bản mới. Quay lại xem phương án trước khi điều chỉnh.",warning=true)
        s.compensation.forEachIndexed{i,m->TdCard{Text("${v.names()[m.nguoiTraId]} → ${v.names()[m.nguoiNhanId]}: ${tien(m.soTien)}",style=rmText(13))
            if(m.nguoiTraId==user||m.nguoiNhanId==user)TextButton({vm.money(m.nguoiTraId,m.nguoiNhanId,"0")},enabled=!s.busy,modifier=Modifier.testTag("ss_money_remove_$i")){Text("Xóa khoản bù của tôi")}
            else Text("Khoản giữa hai thành viên khác được giữ nguyên.",style=rmText(11),color=ReMarketPalette.Muted)
        }}
        LuaChon("Hướng khoản bù",direction,listOf("Tra" to "Tôi bù cho thành viên","Nhan" to "Thành viên bù cho tôi"),{direction=it},enabled=!s.busy,tag="ss_money_direction")
        LuaChon(if(direction=="Tra")"Bạn bù cho ai?" else "Bạn nhận bù từ ai?",partner,partners.map{it.nguoiDungId to it.hoTen},{partner=it},enabled=!s.busy,tag="ss_money_partner")
        RmField("Số tiền (VND)",amount,{amount=it.filter(Char::isDigit).take(13)},"Nhập số tiền, 0 để xóa khoản này","ss_money_amount",keyboard=KeyboardType.Number,enabled=!s.busy)
        SsButton("Cập nhật phương án đang soạn","ss_money_apply",!s.busy,true){val value=amount.toLongOrNull();if(value==null||value !in 0..1_000_000_000_000L)vm.error("Nhập mức bù hợp lệ.")else if(direction=="Tra")vm.money(user,partner,amount)else vm.money(partner,user,amount)}
        RmField("Lý do điều chỉnh",s.proposalText,vm::proposalText,"Trao đổi với nhóm về phương án mới","ss_proposal_message",multiline=true,minimumHeight=80,enabled=!s.busy)
        SsMoney(s.compensation,v.names())
        TdNote("Cần cả ba xác nhận lại","Gửi phiên bản mới sẽ xóa trạng thái đồng ý của cả ba. Mức bù phải trong hạn mức đã đăng; bạn chỉ thay đổi khoản liên quan đến mình.")
    }
}
