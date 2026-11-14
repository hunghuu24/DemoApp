package com.ute.app_ban_do_cu.feature.exchange

import androidx.compose.foundation.*
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.selection.toggleable
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.draw.clip
import androidx.compose.ui.platform.testTag
import androidx.compose.ui.res.painterResource
import androidx.compose.ui.semantics.Role
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.dp
import coil.ImageLoader
import com.ute.app_ban_do_cu.R
import com.ute.app_ban_do_cu.core.designsystem.*
import com.ute.app_ban_do_cu.domain.model.*
import com.ute.app_ban_do_cu.feature.catalog.AnhSanPham
import com.ute.app_ban_do_cu.feature.catalog.tien
import com.ute.app_ban_do_cu.feature.commerce.ngayGio

val ExchangeBorder=Color(0xFFDCE4DE)
@Composable fun TraoDoiTheme(content:@Composable () -> Unit){MaterialTheme(colorScheme=MaterialTheme.colorScheme.copy(outline=ExchangeBorder),
    typography=MaterialTheme.typography.copy(labelMedium=rmText(13,FontWeight.SemiBold)),content=content)}
@Composable fun TdButton(text:String,click:()->Unit,modifier:Modifier=Modifier,enabled:Boolean=true,outlined:Boolean=false){
    RmButton(text,click,modifier,enabled=enabled,outlined=outlined)
}
@Composable fun TdCard(modifier:Modifier=Modifier,content:@Composable ColumnScope.()->Unit){
    RmCard(modifier) { Column(verticalArrangement=Arrangement.spacedBy(12.dp),content=content) }
}
@Composable fun TdNote(title:String,detail:String){Column(Modifier.fillMaxWidth().background(ReMarketPalette.Soft,RoundedCornerShape(14.dp)).padding(14.dp),verticalArrangement=Arrangement.spacedBy(6.dp)){
    Text(title,style=rmText(15,FontWeight.SemiBold),color=ReMarketPalette.Primary);Text(detail,style=rmText(12),color=ReMarketPalette.Muted)}}
@Composable fun TdSwitch(label:String,detail:String,value:Boolean,enabled:Boolean,tag:String,change:(Boolean)->Unit){
    Row(Modifier.fillMaxWidth().heightIn(min=56.dp).testTag(tag).toggleable(value,enabled=enabled,role=Role.Switch,onValueChange=change).padding(vertical=4.dp),verticalAlignment=Alignment.CenterVertically,horizontalArrangement=Arrangement.spacedBy(12.dp)){
        Column(Modifier.weight(1f),verticalArrangement=Arrangement.spacedBy(4.dp)){Text(label,style=rmText(14,FontWeight.SemiBold));if(detail.isNotBlank())Text(detail,style=rmText(12),color=ReMarketPalette.Muted)}
        Switch(value,onCheckedChange=null,enabled=enabled)
    }
}
@Composable fun TdCheck(text:String,value:Boolean,enabled:Boolean,tag:String,checkedIcon:Int?=null,change:(Boolean)->Unit){
    Row(Modifier.fillMaxWidth().heightIn(min=48.dp).testTag(tag).toggleable(value,enabled=enabled,role=Role.Checkbox,onValueChange=change).padding(vertical=6.dp),verticalAlignment=Alignment.CenterVertically){
        if(checkedIcon==null)Checkbox(value,null,enabled=enabled)else{
            Box(Modifier.size(19.dp).background(if(value)ReMarketPalette.Primary else Color.White,RoundedCornerShape(4.dp)).border(1.dp,if(value)ReMarketPalette.Primary else ExchangeBorder,RoundedCornerShape(4.dp)),contentAlignment=Alignment.Center){
                if(value)Icon(painterResource(checkedIcon),null,Modifier.size(13.dp),tint=Color.Unspecified)
            };Spacer(Modifier.width(9.dp))
        };Text(text,Modifier.weight(1f),style=rmText(13))}
}
@Composable fun TdSection(number:Int,title:String){Row(verticalAlignment=Alignment.CenterVertically,horizontalArrangement=Arrangement.spacedBy(8.dp)){
    Box(Modifier.size(30.dp).background(ReMarketPalette.Soft,RoundedCornerShape(10.dp)),contentAlignment=Alignment.Center){Text(number.toString(),style=rmText(14,FontWeight.Bold),color=ReMarketPalette.Primary)}
    Text(title,Modifier.weight(1f),style=rmText(17,FontWeight.SemiBold))}}
@Composable fun TdMon(mon:MonTraoDoi,loader:ImageLoader,label:String,framed:Boolean=true){
    if(framed) TdCard { TdMon(mon,loader,label,framed=false) }
    else Row(horizontalArrangement=Arrangement.spacedBy(12.dp),verticalAlignment=Alignment.CenterVertically){
    AnhSanPham(mon.anhUrl,loader,Modifier.size(68.dp).clip(RoundedCornerShape(14.dp)),mon.ten)
    Column(Modifier.weight(1f),verticalArrangement=Arrangement.spacedBy(4.dp)){Text(mon.ten,style=rmText(15,FontWeight.SemiBold),maxLines=2,overflow=TextOverflow.Ellipsis)
        Text(tien(mon.giaTriThamKhao),style=rmText(16,FontWeight.Bold),color=ReMarketPalette.Primary);Text("${mon.chuSoHuu} · $label",style=rmText(12),color=ReMarketPalette.Muted)}
}}
fun TinDang.mon()=MonTraoDoi(sanPhamId,id,ten,giaTriThamKhao,tinhTrang,nguoiDangId,nguoiDang,anh.firstOrNull{it.laAnhBia}?.url?:anh.firstOrNull()?.url)
fun buText(o:DeNghiTraoDoi,p:PhienTraoDoi?=o.thoaThuan):String{
    if(p==null||p.soTienBu==0L)return "Không bù tiền"
    val payer=if(p.nguoiTraId==o.nguoiGuiId)o.monGui.chuSoHuu else o.monNhan.chuSoHuu
    val receiver=if(p.nguoiTraId==o.nguoiGuiId)o.monNhan.chuSoHuu else o.monGui.chuSoHuu
    return "$payer bù cho $receiver: ${tien(p.soTienBu)}"
}
fun tenDeNghi(o:DeNghiTraoDoi)=if(o.trangThaiGiaoDich=="HoanTat")"Hoàn tất" else when(o.trangThai){
    "ChoPhanHoi"->"Chờ phản hồi";"DeXuatLai"->"Đang thương lượng";"DaChapNhan"->if(o.xacNhan.any{it.daNhan})"Chờ đối tác xác nhận" else "Đã đồng ý · Chờ giao nhận";"TuChoi"->"Đã từ chối";"DaHuy"->"Đã hủy";"HetHan"->"Hết hạn";else->o.trangThai}
@Composable fun TdOfferPair(o:DeNghiTraoDoi,loader:ImageLoader){TdCard{
    Text("Hai món đồ trao đổi",style=rmText(16,FontWeight.SemiBold))
    TdMon(o.monGui,loader,"Bên đề nghị",framed=false)
    Row(Modifier.fillMaxWidth(),verticalAlignment=Alignment.CenterVertically,horizontalArrangement=Arrangement.spacedBy(12.dp)){
        HorizontalDivider(Modifier.weight(1f),color=ReMarketPalette.Border);RmIcon(R.drawable.ic_rm_arrow_left_right,size=20.dp);HorizontalDivider(Modifier.weight(1f),color=ReMarketPalette.Border)
    }
    TdMon(o.monNhan,loader,"Bên nhận đề nghị",framed=false)
    Column(Modifier.fillMaxWidth().background(ReMarketPalette.Soft,RoundedCornerShape(14.dp)).padding(14.dp),verticalArrangement=Arrangement.spacedBy(4.dp)){
        Text("Phương án tiền bù",style=rmText(11,FontWeight.Medium),color=ReMarketPalette.Muted)
        Text(buText(o),style=rmText(14,FontWeight.SemiBold),color=ReMarketPalette.PrimaryDark)
    }
}}
@Composable fun TdTimeline(o:DeNghiTraoDoi){TdCard{
    Text("Lịch sử thương lượng",style=rmText(16,FontWeight.SemiBold))
    o.lichSu.forEach{p->Column(verticalArrangement=Arrangement.spacedBy(4.dp)){
        Text("Phiên bản ${p.soPhienBan} · ${p.nguoiTao}",style=rmText(13,FontWeight.SemiBold));Text(buText(o,p),style=rmText(12),color=ReMarketPalette.Primary)
        Text(p.loiNhan,style=rmText(12));Text(ngayGio(p.ngayTao),style=rmText(11),color=ReMarketPalette.Muted)
        if(p.trangThai=="DaThayThe")Text("Đã được thay bằng đề xuất mới",style=rmText(11),color=ReMarketPalette.Muted)
        else p.dongY.forEach{Text(it.hoTen+when(it.quyetDinh){"DongY"->" đã đồng ý";"TuChoi"->" đã từ chối";else->" · Chờ phản hồi"},style=rmText(12),color=if(it.quyetDinh=="TuChoi")ReMarketPalette.Error else ReMarketPalette.Primary)}
        HorizontalDivider(color=ExchangeBorder)
    }}
}}
