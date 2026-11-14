package com.ute.app_ban_do_cu.feature.commerce

import androidx.compose.foundation.*
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.platform.testTag
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.dp
import coil.ImageLoader
import com.ute.app_ban_do_cu.core.designsystem.*
import com.ute.app_ban_do_cu.feature.catalog.*

@Composable
fun CheckoutScreen(s:MuaBanState,vm:MuaBanViewModel,loader:ImageLoader) {
    Column(Modifier.fillMaxSize().testTag("checkout").verticalScroll(rememberScrollState()).padding(24.dp),verticalArrangement=Arrangement.spacedBy(16.dp)) {
        MbNote("Giao dịch mô phỏng cùng ReMarket","Không trừ tiền thật. Khoản giữ và giao nhận chỉ được mô phỏng trong ứng dụng.")
        val q=s.quote
        if(q!=null) {
            MonHangMini(q.sanPham,loader,if(q.soTienGiam>0) "Đã chốt giá giảm" else "Giá niêm yết")
            RmSectionTitle("Thông tin giao hàng", "Kiểm tra thông tin trước khi tạo đơn.")
            RmField("Người nhận",s.ten,{vm.giaoHang(name=it)},"Nhập tên người nhận","checkout_name",enabled=!s.busy,isError="ten" in s.formErrors);LoiTruong(s.formErrors,"ten")
            RmField("Số điện thoại",s.phone,{vm.giaoHang(phone=it)},"09xx xxx xxx","checkout_phone",keyboard=KeyboardType.Phone,enabled=!s.busy,isError="phone" in s.formErrors);LoiTruong(s.formErrors,"phone")
            RmField("Địa chỉ nhận hàng",s.diaChi,{vm.giaoHang(address=it)},"Nhập địa chỉ chi tiết","checkout_address",multiline=true,minimumHeight=90,enabled=!s.busy,isError="diaChi" in s.formErrors);LoiTruong(s.formErrors,"diaChi")
            if(q.phieuKhaDung.isNotEmpty()) LuaChon("Phiếu giảm giá",q.phieuThuongLuongId.orEmpty(),
                listOf("" to "Không áp dụng")+q.phieuKhaDung.map{it.id to "Giảm ${tien(it.soTienGiam)}"},{vm.apDungPhieu(it.ifBlank{null})},tag="checkout_coupon",enabled=!s.busy)
            else Text("Chưa có phiếu giảm giá được người bán chấp nhận.",style=rmText(12),color=MuaBanColors.Muted)
            TongTien(q.giaNiemYet,q.soTienGiam,q.phiVanChuyen,q.tongTien)
            MoPhongSelect(!s.busy)
            Text("Món đồ được giữ tối đa 15 phút sau khi tạo đơn để bạn xác nhận thanh toán mô phỏng.",style=rmText(12),color=MuaBanColors.Muted)
        }
        TextButton({vm.donHang()}){Text("Đơn hàng của tôi")}
    }
}

@Composable
fun DonHangListScreen(s:MuaBanState,vm:MuaBanViewModel,loader:ImageLoader) {
    Column(Modifier.fillMaxSize().testTag("orders").verticalScroll(rememberScrollState()).padding(24.dp),verticalArrangement=Arrangement.spacedBy(14.dp)) {
        if(s.selectingSupportOrder) MbNote("Chọn đơn cần hỗ trợ","Bạn có thể chọn trong đơn mua hoặc đơn bán của mình.")
        Row(Modifier.fillMaxWidth().background(ReMarketPalette.SurfaceMuted, RoundedCornerShape(16.dp)).padding(4.dp), horizontalArrangement=Arrangement.spacedBy(8.dp)) {
            listOf("mua" to "Đơn mua","ban" to "Đơn bán").forEach{(role,label)->FilterChip(s.orderRole==role,{vm.donHang(role,null,selectSupport=s.selectingSupportOrder)},
                enabled=!s.busy,label={Text(label,Modifier.fillMaxWidth(),textAlign=TextAlign.Center)},modifier=Modifier.weight(1f).testTag("orders_$role"))}
        }
        Row(Modifier.horizontalScroll(rememberScrollState()),horizontalArrangement=Arrangement.spacedBy(8.dp)) {
            FilterChip(s.orderStatus==null,{vm.donHang(s.orderRole,null,selectSupport=s.selectingSupportOrder)},label={Text("Tất cả")},enabled=!s.busy)
            donStates.forEach{(status,label)->FilterChip(s.orderStatus==status,{vm.donHang(s.orderRole,status,selectSupport=s.selectingSupportOrder)},enabled=!s.busy,
                label={Text(label,style=rmText(12))},modifier=Modifier.testTag("order_status_$status"))}
        }
        val page=s.orders
        page?.items?.forEach{order->MbCard(Modifier.clickable(enabled=!s.busy){vm.docDon(order.id)}.testTag("order_${order.id}")) {
            Row(Modifier.fillMaxWidth(), horizontalArrangement=Arrangement.spacedBy(8.dp), verticalAlignment=Alignment.CenterVertically) {
                Text(order.ma, Modifier.weight(1f), style=rmText(11,FontWeight.Medium), color=MuaBanColors.Muted,maxLines=1,overflow=TextOverflow.Ellipsis)
                RmBadge(tenTrangThai(order.trangThai),tone=when(order.trangThai){"ChoThanhToan"->RmTone.PENDING;"DaHuy"->RmTone.NEUTRAL;else->RmTone.POSITIVE})
            }
            MonHangMini(order.sanPham,loader,framed=false)
            HorizontalDivider(color=MuaBanColors.Border)
            RmKeyValue("Tổng thanh toán",tien(order.tongTien),emphasized=true)
        }}
        if(page!=null) {
            if(page.items.isEmpty()) TrangRong("Chưa có đơn hàng","Các đơn mua bán của bạn sẽ xuất hiện tại đây.",Modifier.testTag("orders_empty"))
            PhanTrang(page.trang,page.tongTrang,!s.busy,{vm.donHang(s.orderRole,s.orderStatus,page.trang-1,s.selectingSupportOrder)},{vm.donHang(s.orderRole,s.orderStatus,page.trang+1,s.selectingSupportOrder)})
        }
    }
}

@Composable
fun DonHangDetailScreen(s:MuaBanState,vm:MuaBanViewModel,loader:ImageLoader) {
    val o=s.order?:return
    Column(Modifier.fillMaxSize().testTag("order_detail").verticalScroll(rememberScrollState()).padding(24.dp),verticalArrangement=Arrangement.spacedBy(18.dp)) {
        MbNote(tenTrangThai(o.trangThai),when(o.trangThai){"ChoThanhToan"->"Chờ xác nhận thanh toán mô phỏng trước ${ngayGio(o.hanThanhToan)}";"DangGiao"->"Vận chuyển mô phỏng: ${tenTrangThai(o.vanChuyen?.trangThai.orEmpty())}.";"DaNhan"->"Người mua đã xác nhận nhận hàng. Đơn đã hoàn tất.";"DaHuy"->o.lyDoHuy?:"Đơn đã hủy.";else->"Thanh toán và vận chuyển trong đơn đều là mô phỏng."})
        MonHangMini(o.sanPham,loader,"${tien(o.tongTien)} · Số lượng: 1")
        Text("Mã đơn: ${o.ma}",style=rmText(12),color=MuaBanColors.Muted)
        MbCard { RmKeyValue("Người mua",o.nguoiMua); RmKeyValue("Người bán",o.nguoiBan) }
        TextButton(vm::chatDon,modifier=Modifier.testTag("order_contact"),enabled=!s.busy){Text("Liên hệ trong chat")}
        Text("Lịch sử đơn hàng",style=rmText(17,FontWeight.SemiBold),color=MuaBanColors.Heading)
        MbCard {
            val history = buildList {
                o.lichSu.filter{it.trangThai=="DaDatHang"||it.trangThai=="DaThanhToan"}.forEach{add(Triple(it.moTa,ngayGio(it.thoiDiem),true))}
                o.vanChuyen?.lichSu?.forEach{add(Triple(tenTrangThai(it.trangThai),ngayGio(it.thoiDiem),true))}
                o.lichSu.filter{it.trangThai=="DaNhan"||it.trangThai=="DaHuy"}.forEach{add(Triple(it.moTa,ngayGio(it.thoiDiem),true))}
                if(o.trangThai!in listOf("DaNhan","DaHuy"))add(Triple("Đã nhận hàng","Xác nhận sau khi vận chuyển báo đã giao.",false))
            }
            history.forEachIndexed{index,event->RmTimelineItem(event.first,event.second,event.third,last=index==history.lastIndex)}
        }
        o.vanChuyen?.let{Text("Vận đơn mô phỏng: ${it.maVanDon}\nDự kiến: ${ngayGio(it.ngayGiaoDuKien)}",style=rmText(12),color=MuaBanColors.Muted)}
        MbCard {
            Text("Thông tin giao hàng",style=rmText(14,FontWeight.SemiBold))
            Text("${o.tenNguoiNhan}\n${o.soDienThoai}\n${o.diaChi}",style=rmText(13))
        }
        TongTien(o.giaNiemYet,o.soTienGiam,o.phiVanChuyen,o.tongTien)
        MbCard {
            Text("Thanh toán mô phỏng: ${tenTrangThai(o.thanhToan.trangThai)}",style=rmText(13,FontWeight.SemiBold))
            Text(when{ o.thanhToan.ngayGiaiNgan!=null->"Đã giải ngân mô phỏng ${ngayGio(o.thanhToan.ngayGiaiNgan)}";
                o.thanhToan.ngayHoanTien!=null->"Đã hoàn tiền mô phỏng ${ngayGio(o.thanhToan.ngayHoanTien)}";
                o.thanhToan.dangGiuTien->"Khoản mô phỏng đang được giữ đến khi người mua nhận hàng.";else->"Không có giao dịch ngân hàng hoặc khoản tiền thật."},style=rmText(12),color=MuaBanColors.Muted)
        }
        o.danhGia?.let{MbCard{Text("Đánh giá đã gửi",style=rmText(14,FontWeight.SemiBold));Text("Sản phẩm: ${it.soSaoSanPham?:it.soSaoNguoiBan}/5 · Người bán: ${it.soSaoNguoiBan}/5",style=rmText(12));Text(it.noiDung,style=rmText(13))}}
    }
}

@Composable
fun DanhGiaScreen(s:MuaBanState,vm:MuaBanViewModel,loader:ImageLoader) {
    val o=s.order?:return
    Column(Modifier.fillMaxSize().testTag("order_rating").verticalScroll(rememberScrollState()).padding(24.dp),verticalArrangement=Arrangement.spacedBy(24.dp)) {
        MonHangMini(o.sanPham,loader)
        Text("Bạn đánh giá sản phẩm thế nào?",style=rmText(17,FontWeight.SemiBold))
        HangSao(s.saoSanPham,"product_star"){vm.danhGiaForm(product=it)}
        Text("Đánh giá người bán: ${o.nguoiBan}",style=rmText(17,FontWeight.SemiBold))
        HangSao(s.saoNguoiBan,"seller_star"){vm.danhGiaForm(seller=it)}
        RmField("Nhận xét của bạn",s.nhanXet,{vm.danhGiaForm(text=it)},"Mô tả trải nghiệm thực tế...","rating_text",multiline=true,enabled=!s.busy)
        Text("Chỉ đánh giá sau khi bạn đã nhận và kiểm tra món đồ.",style=rmText(12),color=MuaBanColors.Muted)
    }
}
@Composable
private fun HangSao(value:Int,tag:String,select:(Int)->Unit) {
    Row(Modifier.fillMaxWidth(),horizontalArrangement=Arrangement.SpaceEvenly) {(1..5).forEach{star->
        TextButton({select(star)},modifier=Modifier.weight(1f).heightIn(min=48.dp).testTag("${tag}_$star"),contentPadding=PaddingValues(0.dp)) {
            Text(if(star<=value)"★" else "☆",style=rmText(32),color=if(star<=value) MuaBanColors.Green else MuaBanColors.Muted)
        }
    }}
}
@Composable
fun DanhGiaXongScreen() {
    Column(Modifier.fillMaxSize().testTag("rating_success").padding(24.dp),verticalArrangement=Arrangement.Center,horizontalAlignment=Alignment.CenterHorizontally) {
        Text("✓",style=rmText(64,FontWeight.Bold),color=MuaBanColors.Green)
        Text("Đánh giá thành công",style=rmText(22,FontWeight.SemiBold));Spacer(Modifier.height(18.dp))
        Text("Cảm ơn bạn đã dành thời gian chia sẻ trải nghiệm về sản phẩm.",style=rmText(14),color=MuaBanColors.Muted)
    }
}
