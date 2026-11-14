package com.ute.app_ban_do_cu.feature.commerce

import androidx.compose.foundation.*
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.lazy.rememberLazyListState
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.platform.testTag
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.unit.dp
import androidx.compose.ui.window.Dialog
import coil.ImageLoader
import coil.compose.AsyncImage
import com.ute.app_ban_do_cu.BuildConfig
import com.ute.app_ban_do_cu.core.designsystem.*
import com.ute.app_ban_do_cu.domain.model.*
import com.ute.app_ban_do_cu.feature.catalog.*

@Composable
fun HoiThoaiListScreen(s:MuaBanState,vm:MuaBanViewModel,loader:ImageLoader) {
    Box(Modifier.testTag("trade_inbox")) {
        Column(Modifier.fillMaxSize().testTag("inbox").verticalScroll(rememberScrollState()).padding(24.dp),verticalArrangement=Arrangement.spacedBy(16.dp)) {
            Row(horizontalArrangement=Arrangement.spacedBy(10.dp)) {
                FilterChip(!s.unread,{vm.hopThu(false)},enabled=!s.busy,label={Text("Tất cả")})
                FilterChip(s.unread,{vm.hopThu(true)},enabled=!s.busy,label={Text("Chưa đọc")})
            }
            s.inbox?.forEach{item->MbCard(Modifier.clickable(enabled=!s.busy){vm.docChat(item.id)}.testTag("chat_${item.id}")) {
                Row(horizontalArrangement=Arrangement.spacedBy(12.dp)) {
                    AnhSanPham(item.anhUrl,loader,Modifier.size(60.dp).clip(RoundedCornerShape(14.dp)))
                    Column(Modifier.weight(1f),verticalArrangement=Arrangement.spacedBy(5.dp)) {
                        Text(item.doiTac.ifBlank{item.tieuDe},style=rmText(15,FontWeight.SemiBold),maxLines=1,overflow=TextOverflow.Ellipsis)
                        Text(item.tinCuoi?:"Chưa có tin nhắn",style=rmText(13),maxLines=2,overflow=TextOverflow.Ellipsis,color=if(item.chuaDoc>0)MuaBanColors.Text else MuaBanColors.Muted)
                        Text(item.tieuDe,style=rmText(11),maxLines=1,overflow=TextOverflow.Ellipsis,color=MuaBanColors.Muted)
                    }
                    if(item.chuaDoc>0) Badge(containerColor=ReMarketPalette.Primary){Text(item.chuaDoc.toString())}
                }
            }}
            if(s.inbox?.isEmpty()==true) TrangRong("Chưa có hội thoại","Mở một sản phẩm và chọn Liên hệ để thương lượng.")
            Row(Modifier.fillMaxWidth(),horizontalArrangement=Arrangement.SpaceBetween) {
                TextButton({vm.hopThu(s.unread,s.inboxPage-1)},enabled=!s.busy&&s.inboxPage>1){Text("Trang trước")}
                TextButton({vm.hopThu(s.unread,s.inboxPage+1)},enabled=!s.busy&&s.inbox?.size==20){Text("Trang sau")}
            }
        }
    }
}
@Composable
fun ChatMuaScreen(s:MuaBanState,vm:MuaBanViewModel,loader:ImageLoader,profile:HoSo) {
    val chat=s.chat?:return
    val scroll=rememberLazyListState()
    var image by remember{mutableStateOf<String?>(null)}
    LaunchedEffect(chat.id,chat.tinNhan.lastOrNull()?.soThuTu){if(chat.tinNhan.isNotEmpty())scroll.animateScrollToItem(chat.tinNhan.size)}
    Column(Modifier.fillMaxSize().testTag("trade_chat").padding(horizontal=24.dp).padding(top=16.dp)) {
        if(chat.sanPham==null)MbCard{Text(chat.tieuDe,style=rmText(14,FontWeight.SemiBold));Text(chat.doiTac,style=rmText(12),color=MuaBanColors.Muted)}
        chat.sanPham?.let{product->MonHangMini(product,loader,"Giá niêm yết: ${tien(product.giaNiemYet)}") {
            if(chat.nguoiMuaId==profile.id&&chat.coTheThuongLuong) TextButton(vm::moPhieu,enabled=!s.busy,modifier=Modifier.testTag("chat_offer"),contentPadding=PaddingValues(6.dp)){Text("Trả giá",style=rmText(12))}
        }}
        LazyColumn(Modifier.weight(1f).fillMaxWidth(),state=scroll,contentPadding=PaddingValues(vertical=16.dp),verticalArrangement=Arrangement.spacedBy(12.dp)) {
            item{if(chat.conTinCu) TextButton(vm::tinCu,enabled=!s.busy){Text("Tin nhắn cũ")}}
            items(chat.tinNhan,key={it.id}){msg->
                val offer=chat.phieu.firstOrNull{it.tinNhanId==msg.id}
                if(offer!=null) {
                    MbCard(Modifier.testTag("coupon_${offer.id}")) {
                        Text("Phiếu giảm giá",style=rmText(14,FontWeight.SemiBold))
                        Text("−${tien(offer.soTienGiam)}",style=rmText(22,FontWeight.SemiBold),color=MuaBanColors.Green)
                        Text(when(offer.trangThai){"ChoPhanHoi"->"Đang chờ người bán phản hồi";"DaChapNhan"->"Người bán đã đồng ý";"TuChoi"->"Người bán đã từ chối";"DaHuy"->"Phiếu đã hủy";"DaSuDung"->"Phiếu đã gắn với đơn hàng";else->"Phiếu đã hết hạn"},style=rmText(12),color=MuaBanColors.Muted)
                        Text("Hạn: ${ngayGio(offer.ngayHetHan)}",style=rmText(11),color=MuaBanColors.Muted)
                        if(offer.trangThai=="ChoPhanHoi"&&profile.id==offer.nguoiBanId) Row(horizontalArrangement=Arrangement.spacedBy(10.dp)) {
                            MbButton("Từ chối",{vm.traLoi(offer,false)},Modifier.weight(1f),enabled=!s.busy,outlined=true)
                            MbButton("Chấp nhận",{vm.traLoi(offer,true)},Modifier.weight(1f).testTag("accept_coupon_${offer.id}"),enabled=!s.busy)
                        }
                        if(profile.id==offer.nguoiMuaId&&offer.trangThai in listOf("ChoPhanHoi","DaChapNhan")) Row(horizontalArrangement=Arrangement.spacedBy(10.dp)) {
                            MbButton("Hủy phiếu",{vm.traLoi(offer,false)},Modifier.weight(1f),enabled=!s.busy,outlined=true)
                            if(offer.trangThai=="DaChapNhan"&&chat.coTheThuongLuong) MbButton("Mua hàng",{chat.sanPham?.let{vm.checkout(it.tinDangId,profile,offer.id)}},Modifier.weight(1f).testTag("buy_coupon_${offer.id}"),enabled=!s.busy)
                        }
                    }
                } else if(msg.loai=="HeThong") Text(msg.noiDung.orEmpty(),Modifier.fillMaxWidth().padding(horizontal=12.dp,vertical=8.dp),style=rmText(12),textAlign=TextAlign.Center,color=MuaBanColors.Muted)
                else RmChatBubble(msg.nguoiGuiId==profile.id,ngayGio(msg.ngayGui)) {
                        if(msg.loai!="HinhAnh")Text(msg.noiDung.orEmpty(),style=rmText(14))
                        msg.anh.forEach{file->AnhSanPham(file.url,loader,Modifier.size(206.dp,150.dp).clickable{image=file.url},"Ảnh trong hội thoại")}
                }
            }
            item{if(chat.tinNhan.isEmpty())Text("Chưa có tin nhắn. Bắt đầu cuộc trò chuyện với người bán.",style=rmText(13),color=MuaBanColors.Muted)}
        }
    }
    image?.let{url->Dialog({image=null}) {Column(Modifier.background(Color.White).padding(12.dp)) {
        AsyncImage(BuildConfig.API_BASE_URL+url,"Ảnh hội thoại",imageLoader=loader,contentScale=ContentScale.Fit,modifier=Modifier.fillMaxWidth().heightIn(min=200.dp,max=500.dp))
        TextButton({image=null}){Text("Đóng")}
    }}}
}
@Composable
fun PhieuScreen(s:MuaBanState,vm:MuaBanViewModel,loader:ImageLoader,profile:HoSo) {
    val product=s.chat?.sanPham?:return
    Column(Modifier.fillMaxSize().testTag("coupon_form").verticalScroll(rememberScrollState()).padding(24.dp),verticalArrangement=Arrangement.spacedBy(20.dp)) {
        MbNote("Thương lượng cùng ReMarket","Người bán cần đồng ý trước khi phiếu được áp dụng vào đơn.")
        MonHangMini(product,loader)
        Text("Thông tin đề nghị",style=rmText(16,FontWeight.SemiBold))
        Text("Người nhận: ${product.nguoiBan}",style=rmText(13))
        Text("Người mua: ${profile.hoTen}",style=rmText(13),color=MuaBanColors.Muted)
        RmField("Số tiền muốn giảm",s.giamGia,vm::soTien,"Nhập số tiền muốn giảm","coupon_amount",keyboard=KeyboardType.Number,enabled=!s.busy)
        Text("Giá niêm yết: ${tien(product.giaNiemYet)}",style=rmText(14),color=MuaBanColors.Green)
    }
}
