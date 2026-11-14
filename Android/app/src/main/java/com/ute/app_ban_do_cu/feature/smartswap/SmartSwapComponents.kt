package com.ute.app_ban_do_cu.feature.smartswap

import androidx.compose.foundation.*
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.platform.testTag
import androidx.compose.ui.res.painterResource
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.ui.draw.clip
import androidx.compose.ui.draw.rotate
import androidx.compose.ui.unit.dp
import coil.ImageLoader
import com.ute.app_ban_do_cu.R
import com.ute.app_ban_do_cu.core.designsystem.*
import com.ute.app_ban_do_cu.domain.model.*
import com.ute.app_ban_do_cu.feature.catalog.AnhSanPham
import com.ute.app_ban_do_cu.feature.catalog.tien
import com.ute.app_ban_do_cu.feature.exchange.TdCard
import com.ute.app_ban_do_cu.feature.exchange.TdCheck

@Composable fun SsButton(text:String,tag:String,enabled:Boolean=true,outlined:Boolean=false,modifier:Modifier=Modifier,click:()->Unit){
    RmButton(text,click,modifier.testTag(tag),enabled=enabled,outlined=outlined)
}
@Composable fun SsContent(tag:String,content:@Composable ColumnScope.()->Unit){Column(Modifier.fillMaxSize().testTag(tag).verticalScroll(rememberScrollState()).padding(horizontal=24.dp,vertical=16.dp),verticalArrangement=Arrangement.spacedBy(16.dp),content=content)}
@Composable fun SsBadge(text:String,warning:Boolean=false){RmBadge(text,tone=when{
    warning->RmTone.NEGATIVE
    text.contains("Chờ",true)||text.contains("Chưa",true)->RmTone.PENDING
    text.contains("hủy",true)||text.contains("từ chối",true)||text.contains("hết hạn",true)->RmTone.NEUTRAL
    else->RmTone.POSITIVE
})}
@Composable fun SsCheck(text:String,value:Boolean,enabled:Boolean,tag:String,change:(Boolean)->Unit){TdCheck(text,value,enabled,tag,checkedIcon=R.drawable.ic_fg_smart_check,change=change)}
fun ssStatus(v:SsVong):String=when(v.trangThai){"ChoDongY"->"Chờ đồng ý";"SanSangBanGiao"->"Đã đồng ý · Chờ bàn giao";"DangBanGiao"->"Đang giao";"HoanTat"->"Hoàn tất";"TamDung"->"Sự cố · Tạm dừng";"DaHuy"->when(v.maKetThuc){"TuChoi"->"Đã từ chối";"HetHan"->"Hết hạn";else->"Đã hủy"};else->v.trangThai}
fun ssShipStatus(value:String)=when(value){"ChoGui"->"Chờ gửi";"DaGui"->"Đã gửi";"DangVanChuyen"->"Đang vận chuyển";"DaGiao"->"Đã giao · Chờ người nhận kiểm tra";else->value}
private val ssSystemLabels=mapOf("ChoGui" to "Chờ gửi","DaGui" to "Đã gửi","DangVanChuyen" to "Đang vận chuyển","DaGiao" to "Đã giao",
    "ChoDongY" to "Chờ đồng ý","SanSangBanGiao" to "Chờ bàn giao","DangBanGiao" to "Đang bàn giao","HoanTat" to "Hoàn tất","TamDung" to "Tạm dừng","DaHuy" to "Đã hủy",
    "Moi" to "Mới","DangXuLy" to "Đang xử lý","ChoThucHien" to "Chờ thực hiện","DaGiaiQuyet" to "Đã giải quyết","TuChoi" to "Đã từ chối")
private val ssSystemCodes=Regex("\\b(${ssSystemLabels.keys.joinToString("|")})\\b")
fun ssSystemText(value:String)=ssSystemCodes.replace(value){ssSystemLabels.getValue(it.value)}
fun SsVong.names()=thanhVien.associate{it.nguoiDungId to it.hoTen}
@Composable fun SsMoney(money:List<SsTienBu>,names:Map<String,String>,framed:Boolean=true){
    if(framed)TdCard{SsMoney(money,names,framed=false)}else Column(Modifier.fillMaxWidth(),verticalArrangement=Arrangement.spacedBy(12.dp)){
    Text("Tiền bù đã đề xuất",style=rmText(15,FontWeight.SemiBold))
    if(money.isEmpty())Text("Không bù tiền",style=rmText(13),color=ReMarketPalette.Primary)
    money.forEach{m->Row(Modifier.fillMaxWidth(),horizontalArrangement=Arrangement.spacedBy(12.dp),verticalAlignment=Alignment.Top){
        Text("${names[m.nguoiTraId].orEmpty()} → ${names[m.nguoiNhanId].orEmpty()}",Modifier.weight(1.3f),style=rmText(12),maxLines=3,overflow=TextOverflow.Ellipsis)
        Text(tien(m.soTien),Modifier.weight(1f),style=rmText(15,FontWeight.SemiBold),color=ReMarketPalette.Primary,textAlign=TextAlign.End)
    }}
    if(money.isNotEmpty()){HorizontalDivider(color=ReMarketPalette.Border);RmKeyValue("Tổng trả = tổng nhận",tien(money.sumOf{it.soTien}),emphasized=true)}
    Text("Trả trực tiếp khi gặp. Không thanh toán qua ứng dụng.",style=rmText(11),color=ReMarketPalette.Muted)
}}
@Composable fun SsProduct(mon:SsMon,name:String,receiver:String,photo:String?,loader:ImageLoader){Row(Modifier.fillMaxWidth(),verticalAlignment=Alignment.CenterVertically,horizontalArrangement=Arrangement.spacedBy(10.dp)){
    AnhSanPham(photo,loader,Modifier.size(64.dp).clip(RoundedCornerShape(14.dp)),mon.ten)
    Column(Modifier.weight(1f),verticalArrangement=Arrangement.spacedBy(5.dp)){Text(mon.ten,style=rmText(14,FontWeight.SemiBold),maxLines=2,overflow=TextOverflow.Ellipsis);Text(tien(mon.giaTriThamKhao),style=rmText(16,FontWeight.Bold),color=ReMarketPalette.Primary)
        Text("$name giao → $receiver",style=rmText(12),color=ReMarketPalette.Muted)}
}}
// A is the initiator (or current user before invitation); direction comes only from API legs.
fun ssOrder(legs:List<SsChang>,user:String):List<SsChang>{if(legs.size!=3)return legs;val first=legs.firstOrNull{it.nguoiGiaoId==user}?:legs.first();val second=legs.first{it.nguoiGiaoId==first.nguoiNhanId};val third=legs.first{it.nguoiGiaoId==second.nguoiNhanId};return listOf(first,second,third)}
@Composable fun SsDiagram(legs:List<SsChang>,names:Map<String,String>,user:String){val ordered=ssOrder(legs,user);if(ordered.size!=3)return
    TdCard(Modifier.testTag("ss_diagram")){
        Text("Chiều giao sản phẩm: A → C → B → A",style=rmText(12,FontWeight.SemiBold))
        Row(Modifier.fillMaxWidth(),verticalAlignment=Alignment.Top){ordered.forEachIndexed{i,leg->
            Column(Modifier.weight(1f),horizontalAlignment=Alignment.CenterHorizontally,verticalArrangement=Arrangement.spacedBy(6.dp)){
                Box(Modifier.size(36.dp).background(if(i==0)ReMarketPalette.Primary else ReMarketPalette.Soft,CircleShape),contentAlignment=Alignment.Center){
                    Text(listOf("A","C","B")[i],style=rmText(13,FontWeight.Bold),color=if(i==0)Color.White else ReMarketPalette.Primary)}
                Text(names[leg.nguoiGiaoId].orEmpty().trim().split(Regex("\\s+")).takeLast(2).joinToString(" "),style=rmText(11,FontWeight.Medium),textAlign=TextAlign.Center,minLines=2,maxLines=2,overflow=TextOverflow.Ellipsis)
                Text(leg.sanPham.ten,style=rmText(11),color=ReMarketPalette.Muted,textAlign=TextAlign.Center,maxLines=2,overflow=TextOverflow.Ellipsis)
            }
            if(i<2)Box(Modifier.width(20.dp).height(36.dp),contentAlignment=Alignment.Center){RmIcon(R.drawable.ic_rm_arrow_left,Modifier.rotate(180f),size=16.dp)}
        }}
        Row(Modifier.fillMaxWidth().background(ReMarketPalette.Soft,RoundedCornerShape(12.dp)).padding(10.dp),verticalAlignment=Alignment.CenterVertically,horizontalArrangement=Arrangement.spacedBy(8.dp)){
            RmIcon(R.drawable.ic_rm_arrow_left,size=16.dp)
            Text("B → A · ${ordered[2].sanPham.ten}",Modifier.weight(1f),style=rmText(11,FontWeight.Medium),color=ReMarketPalette.PrimaryDark,maxLines=2,overflow=TextOverflow.Ellipsis)
        }
    }
}
