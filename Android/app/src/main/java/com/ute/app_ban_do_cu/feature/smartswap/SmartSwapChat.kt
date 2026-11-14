package com.ute.app_ban_do_cu.feature.smartswap

import androidx.compose.foundation.*
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.*
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.platform.testTag
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.window.Dialog
import coil.ImageLoader
import com.ute.app_ban_do_cu.core.designsystem.*
import com.ute.app_ban_do_cu.feature.catalog.AnhSanPham
import com.ute.app_ban_do_cu.feature.commerce.ngayGio
import com.ute.app_ban_do_cu.feature.exchange.TdCard

@Composable fun SsChatScreen(s:SmartSwapState,vm:SmartSwapViewModel,loader:ImageLoader,user:String){val v=s.circle?:return;val chat=s.chat?:return
    val scroll=rememberLazyListState();var image by remember{mutableStateOf<String?>(null)}
    LaunchedEffect(chat.id,chat.tinNhan.lastOrNull()?.id){if(chat.tinNhan.isNotEmpty())scroll.animateScrollToItem(chat.tinNhan.size)}
    Column(Modifier.fillMaxSize().testTag("ss_chat").padding(horizontal=24.dp,vertical=12.dp),verticalArrangement=Arrangement.spacedBy(10.dp)){
        Text(v.thanhVien.joinToString(" · "){it.hoTen+if(it.nguoiDungId==user)" (Tôi)" else ""},style=rmText(12),color=ReMarketPalette.Muted)
        Row(Modifier.fillMaxWidth(),verticalAlignment=Alignment.CenterVertically){Column(Modifier.weight(1f)){SsBadge("${v.phuongAnHienTai?.dongY?.count{it.quyetDinh=="DongY"}?:0}/3 đã đồng ý tham gia");Text("Phiên bản ${v.phuongAnHienTai?.soPhienBan}",style=rmText(11),color=ReMarketPalette.Muted)}
            TextButton(vm::compensation,enabled=!s.busy&&v.coTheDeXuat,modifier=Modifier.testTag("ss_chat_adjust")){Text("Điều chỉnh",style=rmText(12))}}
        if(v.trangThai=="TamDung")RmNotice("Giao dịch có sự cố. Nhóm vẫn có thể trao đổi để làm rõ.",warning=true)
        LazyColumn(Modifier.weight(1f).fillMaxWidth(),state=scroll,verticalArrangement=Arrangement.spacedBy(12.dp),contentPadding=PaddingValues(vertical=8.dp)){
            item{if(chat.conTinCu)TextButton(vm::older,enabled=!s.busy,modifier=Modifier.testTag("ss_chat_older")){Text("Tin nhắn cũ")}}
            items(chat.tinNhan,key={it.id}){m->if(m.loai=="HeThong")TdCard{
                Text(ssSystemText(m.noiDung.orEmpty()),style=rmText(12),color=ReMarketPalette.Muted)
                m.phienBanThoaThuanId?.let{id->val p=v.lichSu.firstOrNull{it.id==id};Text("Phương án ${p?.soPhienBan?:""}${if(id==v.phuongAnHienTai?.id)" · Hiện tại" else " · Đã thay thế"}",style=rmText(11),color=ReMarketPalette.Primary)
                    if(id==v.phuongAnHienTai?.id)TextButton(vm::consent,enabled=!s.busy,modifier=Modifier.testTag("ss_chat_plan_${p?.soPhienBan}")){Text("Xem xác nhận 3 bên")}}
            }else RmChatBubble(m.nguoiGuiId==user,ngayGio(m.ngayGui),v.names()[m.nguoiGuiId].orEmpty()+if(m.nguoiGuiId==user)" · Tôi" else ""){
                    if(m.loai!="HinhAnh")Text(m.noiDung.orEmpty(),style=rmText(14))
                    m.anh.forEach{photo->AnhSanPham(photo.url,loader,Modifier.size(206.dp,150.dp).clickable{image=photo.url},"Ảnh nhóm")}
            }}
            item{if(chat.tinNhan.isEmpty())Text("Chưa có tin nhắn. Bắt đầu trao đổi với nhóm.",style=rmText(12),color=ReMarketPalette.Muted)}
        }
    }
    image?.let{url->Dialog({image=null}){Column(Modifier.background(Color.White).padding(12.dp)){AnhSanPham(url,loader,Modifier.fillMaxWidth().height(350.dp),"Ảnh nhóm");TextButton({image=null}){Text("Đóng")}}}}
}
