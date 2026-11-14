package com.ute.app_ban_do_cu.feature.admin

import androidx.compose.foundation.*
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import com.ute.app_ban_do_cu.core.designsystem.*
import com.ute.app_ban_do_cu.domain.model.*
import com.ute.app_ban_do_cu.feature.commerce.ngayGio
import com.ute.app_ban_do_cu.feature.exchange.TdCard

@Composable fun QtDashboard(s:QuanTriState,vm:QuanTriViewModel,profile:HoSo,support:()->Unit){val r=s.report
    QtBody("admin_home"){
        Text("Chào ${profile.hoTen}",style=rmText(20,FontWeight.SemiBold));Text("Tổng quan hoạt động cộng đồng",style=rmText(12),color=ReMarketPalette.Muted)
        r?.let{Text("${it.tuNgay} – ${it.denNgay} · 30 ngày",style=rmText(11),color=ReMarketPalette.Muted)}
        Row(horizontalArrangement=Arrangement.spacedBy(12.dp)){QtKpi("Tổng người dùng",r?.tongNguoiDung?.toString(),r?.let{"+${it.nguoiDungMoi} trong kỳ"},Modifier.weight(1f));QtKpi("Tin chờ duyệt",r?.tinChoDuyet?.toString(),r?.let{"${it.tinTiepNhan} tin tiếp nhận"},Modifier.weight(1f))}
        Row(horizontalArrangement=Arrangement.spacedBy(12.dp)){QtKpi("Mua bán / Trao đổi",r?.let{"${it.muaBan} / ${it.traoDoi}"},r?.let{"${it.muaBan+it.traoDoi} giao dịch trong kỳ"},Modifier.weight(1f));QtKpi("Tranh chấp trong kỳ",r?.let{(it.khieuNaiMoi+it.khieuNaiDangXuLy).toString()},r?.let{"${it.khieuNaiMoi} mới · ${it.khieuNaiDangXuLy} đang xử lý"},Modifier.weight(1f))}
        r?.let{QtChart(it){vm.section(QtScreen.REPORT)}}
        TdCard{Text("Việc cần làm",style=rmText(16,FontWeight.SemiBold));Text("${r?.tinChoDuyet?.toString()?:"—"} tin đang chờ kiểm duyệt",style=rmText(13));Text("Khiếu nại mới và đang xử lý trong kỳ: ${r?.let{it.khieuNaiMoi+it.khieuNaiDangXuLy}?:"—"}",style=rmText(12),color=ReMarketPalette.Muted)}
        QtButton("Kiểm duyệt tin đăng","admin_review",enabled=!s.busy){vm.section(QtScreen.LISTINGS)}
        QtButton("Quản lý yêu cầu hỗ trợ","admin_support",enabled=!s.busy,outline=true,click=support)
        r?.let{Text("Cập nhật ${ngayGio(it.capNhat)}",style=rmText(11),color=ReMarketPalette.Muted)}
    }
}
@Composable private fun QtKpi(label:String,value:String?,note:String?,modifier:Modifier){TdCard(modifier){Text(label,style=rmText(12),color=ReMarketPalette.Muted);Text(value?:"—",style=rmText(27,FontWeight.Bold),color=ReMarketPalette.Primary);note?.let{Text(it,style=rmText(11),color=ReMarketPalette.Muted)}}}
@Composable fun QtChart(r:QtReport,onReport:(()->Unit)?=null){val bins=r.bieuDo.chunked(7);val max=bins.maxOfOrNull{maxOf(it.sumOf{b->b.muaBan},it.sumOf{b->b.traoDoi})}?.coerceAtLeast(1)?:1
    TdCard{Row(Modifier.fillMaxWidth(),verticalAlignment=Alignment.CenterVertically){Text("Lượng giao dịch",Modifier.weight(1f),style=rmText(16,FontWeight.SemiBold));if(onReport!=null)TextButton(onReport){Text("Báo cáo ›",style=rmText(12))}}
        Row(Modifier.fillMaxWidth(),horizontalArrangement=Arrangement.spacedBy(12.dp)){
            Text("● Mua bán: ${r.muaBan}",Modifier.weight(1f),style=rmText(11),color=ReMarketPalette.Primary)
            Text("● Trao đổi: ${r.traoDoi}",Modifier.weight(1f),style=rmText(11),color=ReMarketPalette.Muted)
        }
        Row(Modifier.fillMaxWidth().horizontalScroll(rememberScrollState()).heightIn(min=170.dp),verticalAlignment=Alignment.Bottom,horizontalArrangement=Arrangement.spacedBy(12.dp)){
            Column(Modifier.height(145.dp),verticalArrangement=Arrangement.SpaceBetween){Text(max.toString(),style=rmText(11));Text((max/2).toString(),style=rmText(11));Text("0",style=rmText(11))}
            bins.forEach{bin->val buy=bin.sumOf{it.muaBan};val swap=bin.sumOf{it.traoDoi};Column(horizontalAlignment=Alignment.CenterHorizontally,verticalArrangement=Arrangement.spacedBy(4.dp)){
                Row(Modifier.heightIn(min=128.dp),verticalAlignment=Alignment.Bottom,horizontalArrangement=Arrangement.spacedBy(4.dp)){
                    Column(horizontalAlignment=Alignment.CenterHorizontally){Text(buy.toString(),style=rmText(11));Box(Modifier.width(16.dp).height((buy/max.toFloat()*96).coerceAtLeast(1f).dp).background(ReMarketPalette.Primary,RoundedCornerShape(4.dp)))}
                    Column(horizontalAlignment=Alignment.CenterHorizontally){Text(swap.toString(),style=rmText(11));Box(Modifier.width(16.dp).height((swap/max.toFloat()*96).coerceAtLeast(1f).dp).background(ReMarketPalette.Muted,RoundedCornerShape(4.dp)))}
                };Text(bin.first().ngay.takeLast(5),style=rmText(11))
            }}
        };Text("Theo tuần · Đơn vị: giao dịch",style=rmText(10),color=ReMarketPalette.Muted)
    }
}
@Composable fun QtReportScreen(s:QuanTriState,vm:QuanTriViewModel){QtBody("qt_report"){
    Row(horizontalArrangement=Arrangement.spacedBy(8.dp)){listOf(7,30).forEach{days->FilterChip(s.period==days,{vm.report(days)},label={Text("$days ngày")},enabled=!s.busy)};FilterChip(s.period==0,{vm.dates()},label={Text("Tùy chọn")},enabled=!s.busy)}
    Row(horizontalArrangement=Arrangement.spacedBy(10.dp)){Box(Modifier.weight(1f)){RmField("Từ ngày",s.from,{vm.dates(from=it)},"yyyy-MM-dd","qt_report_from",enabled=!s.busy)};Box(Modifier.weight(1f)){RmField("Đến ngày",s.to,{vm.dates(to=it)},"yyyy-MM-dd","qt_report_to",enabled=!s.busy)}}
    QtButton("Áp dụng khoảng ngày","qt_report_apply",enabled=!s.busy){vm.report()}
    s.report?.let{r->Text("${r.muaBan+r.traoDoi} giao dịch",style=rmText(24,FontWeight.Bold),color=ReMarketPalette.Primary)
        val previous=r.giaoDichKyTruoc;Text(if(previous==0)"Kỳ trước chưa có giao dịch để tính tỷ lệ."else String.format(java.util.Locale.forLanguageTag("vi-VN"),"%+.1f%% so kỳ trước",((r.muaBan+r.traoDoi-previous)*100.0/previous)),style=rmText(12),color=ReMarketPalette.Muted)
        QtChart(r);TdCard{Text("Phân bố danh mục · ${r.danhMuc.sumOf{it.soTin}} tin",style=rmText(15,FontWeight.SemiBold));val total=r.danhMuc.sumOf{it.soTin}.coerceAtLeast(1)
            r.danhMuc.forEach{d->Text("${d.ten}: ${d.soTin} tin · ${d.soTin*100/total}%",style=rmText(12));LinearProgressIndicator(progress={d.soTin.toFloat()/total},modifier=Modifier.fillMaxWidth())};if(r.danhMuc.isEmpty())Text("Chưa có tin trong khoảng ngày này.",style=rmText(12))}
        TdCard{Text("Hiệu quả xử lý",style=rmText(16,FontWeight.SemiBold));Text("Duyệt tin: ${r.tinDaDuyet} / ${r.tinTiepNhan} tin trong kỳ",style=rmText(13));Text("${r.tinTuChoi} từ chối trong kỳ · ${r.tinChoDuyet} tin đang chờ duyệt",style=rmText(12));Text("Khiếu nại: ${r.khieuNaiDaGiaiQuyet} đã giải quyết · ${r.khieuNaiMoi} mới · ${r.khieuNaiDangXuLy} đang xử lý",style=rmText(13))}
        Text("${r.tongNguoiDung} người dùng · Cập nhật ${ngayGio(r.capNhat)}",style=rmText(11),color=ReMarketPalette.Muted)
    }
}}
