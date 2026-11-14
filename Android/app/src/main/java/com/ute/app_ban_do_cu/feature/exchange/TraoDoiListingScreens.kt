package com.ute.app_ban_do_cu.feature.exchange

import androidx.compose.foundation.*
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.pager.HorizontalPager
import androidx.compose.foundation.pager.rememberPagerState
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.draw.drawWithCache
import androidx.compose.ui.geometry.CornerRadius
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.PathEffect
import androidx.compose.ui.graphics.drawscope.Stroke
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.platform.testTag
import androidx.compose.ui.res.painterResource
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.unit.dp
import coil.ImageLoader
import coil.compose.AsyncImage
import com.ute.app_ban_do_cu.R
import com.ute.app_ban_do_cu.core.designsystem.*
import com.ute.app_ban_do_cu.domain.model.HoSo
import com.ute.app_ban_do_cu.feature.catalog.*

@Composable fun FormTraoDoiScreen(s:TraoDoiState,vm:TraoDoiViewModel,loader:ImageLoader,pick:()->Unit){
    val f=s.form
    Column(Modifier.fillMaxSize().testTag("exchange_listing_form").verticalScroll(rememberScrollState()).padding(24.dp),verticalArrangement=Arrangement.spacedBy(18.dp)){
        TdSection(1,"Sản phẩm đang có")
        Text("Hình ảnh sản phẩm *",style=rmText(13,FontWeight.SemiBold))
        if(f.anh.isNotEmpty()||f.anhCho.isNotEmpty())Row(Modifier.horizontalScroll(rememberScrollState()),horizontalArrangement=Arrangement.spacedBy(10.dp)){
            f.anh.sortedByDescending{it.laAnhBia}.forEach{p->Column(Modifier.width(100.dp)){
                AnhSanPham(p.url,loader,Modifier.size(100.dp,90.dp).clip(RoundedCornerShape(12.dp)),"Ảnh sản phẩm đã tải")
                TextButton({vm.bia(p)},enabled=!s.busy&&!p.laAnhBia){Text(if(p.laAnhBia)"Ảnh bìa" else "Đặt ảnh bìa",style=rmText(10))}
                TextButton({vm.xoaAnh(p)},enabled=!s.busy){Text("Xóa ảnh",style=rmText(11))}
            }}
            f.anhCho.forEach{p->Column(Modifier.width(100.dp)){
                AsyncImage(p.bytes,"Ảnh đã chọn",imageLoader=loader,contentScale=ContentScale.Crop,modifier=Modifier.size(100.dp,90.dp).clip(RoundedCornerShape(12.dp)))
                TextButton({vm.boAnh(p.uri)},enabled=!s.busy){Text("Bỏ ảnh",style=rmText(11))}
            }}
        }
        if(f.anh.size+f.anhCho.size<6)Column(Modifier.fillMaxWidth().heightIn(min=107.dp).background(ReMarketPalette.Soft,RoundedCornerShape(12.dp)).drawWithCache{
            val stroke=Stroke(width=1.dp.toPx(),pathEffect=PathEffect.dashPathEffect(floatArrayOf(4.dp.toPx(),4.dp.toPx())))
            onDrawBehind{drawRoundRect(Color(0xFF9BBCA7),cornerRadius=CornerRadius(12.dp.toPx()),style=stroke)}
        }
            .testTag("exchange_add_photos").clickable(enabled=!s.busy,onClick=pick).padding(16.dp),horizontalAlignment=Alignment.CenterHorizontally,verticalArrangement=Arrangement.spacedBy(6.dp)){
            Icon(painterResource(R.drawable.ic_fg_exchange_camera),null,Modifier.size(28.dp),tint=Color.Unspecified)
            Text("Thêm ảnh sản phẩm",style=rmText(14,FontWeight.SemiBold),color=ReMarketPalette.Primary)
            Text("${f.anh.size+f.anhCho.size}/6 ảnh · JPG, PNG",style=rmText(11),color=ReMarketPalette.Muted)
        }
        Text("Tối đa 6 ảnh, 5 MB/ảnh. Ưu tiên ảnh thật, rõ tình trạng sản phẩm.",style=rmText(11),color=ReMarketPalette.Muted);LoiTruong(f.errors,"anh")
        RmField("Tên sản phẩm *",f.ten,{v->vm.form{it.copy(ten=v.take(200))}},"VD: Kindle Paperwhite 4","exchange_name",enabled=!s.busy);LoiTruong(f.errors,"ten")
        LuaChon("Danh mục sản phẩm *",f.danhMucId,s.categories.map{it.id to it.ten},{v->vm.form{it.copy(danhMucId=v)}},tag="exchange_category",enabled=!s.busy,icon=R.drawable.ic_fg_exchange_down);LoiTruong(f.errors,"danhMuc")
        LuaChon("Tình trạng sản phẩm *",f.tinhTrang,tinhTrangOptions,{v->vm.form{it.copy(tinhTrang=v)}},tag="exchange_condition",enabled=!s.busy,icon=R.drawable.ic_fg_exchange_down)
        RmField("Mô tả chi tiết sản phẩm *",f.moTa,{v->vm.form{it.copy(moTa=v.take(4000))}},"Thương hiệu, thời gian sử dụng, phụ kiện và khuyết điểm…","exchange_description",multiline=true,enabled=!s.busy,minimumHeight=108);LoiTruong(f.errors,"moTa")
        RmField("Giá trị trao đổi tham khảo *",f.gia,{v->vm.form{it.copy(gia=v.filter(Char::isDigit).take(13))}},"Nhập giá trị","exchange_value",keyboard=KeyboardType.Number,enabled=!s.busy,
            trailing={Text("VND",style=rmText(12),color=ReMarketPalette.Muted)});LoiTruong(f.errors,"gia")
        Text("Giá trị ước tính giúp tìm món đồ trao đổi phù hợp.",style=rmText(11),color=ReMarketPalette.Muted)
        HorizontalDivider(color=ExchangeBorder);TdSection(2,"Sản phẩm muốn nhận")
        RmField("Mô tả sản phẩm muốn nhận *",f.muonNhan,{v->vm.form{it.copy(muonNhan=v.take(2000))}},"Bạn muốn đổi lấy món gì? Mô tả loại sản phẩm, tình trạng và giá trị mong muốn…","exchange_wanted",multiline=true,enabled=!s.busy,minimumHeight=108);LoiTruong(f.errors,"muonNhan")
        HorizontalDivider(color=ExchangeBorder);TdSection(3,"Điều kiện trao đổi")
        TdSwitch("Cho phép trao đổi 1–1","Trao đổi trực tiếp giữa hai người.",f.motMot,!s.busy,"exchange_allow_one"){v->vm.form{it.copy(motMot=v)}}
        TdSwitch("Cho phép Smart Swap 3 bên","Cho phép món đồ tham gia vòng trao đổi 3 bên.",f.smart,!s.busy,"exchange_allow_smart"){v->vm.form{it.copy(smart=v)}};LoiTruong(f.errors,"hinhThuc")
        TdSwitch("Cho phép bù tiền chênh lệch","Linh hoạt khi giá trị hai món đồ khác nhau.",f.buTien,!s.busy,"exchange_allow_compensation"){v->vm.form{it.copy(buTien=v,buToiDa=if(v)it.buToiDa else "0")}}
        if(f.buTien){RmField("Số tiền tối đa có thể bù *",f.buToiDa,{v->vm.form{it.copy(buToiDa=v.filter(Char::isDigit).take(13))}},"Nhập số tiền","exchange_max_compensation",keyboard=KeyboardType.Number,enabled=!s.busy,
            trailing={Text("VND",style=rmText(12),color=ReMarketPalette.Muted)});LoiTruong(f.errors,"buToiDa")
            Text("Nhập 0 nếu không thể bù. Khoản bù được hai bên thỏa thuận và xác nhận khi gặp.",style=rmText(11),color=ReMarketPalette.Muted)}
        RmField("Ghi chú hoặc yêu cầu khác",f.ghiChu,{v->vm.form{it.copy(ghiChu=v.take(1000))}},"VD: Ưu tiên trao đổi trực tiếp, còn hộp và phụ kiện…","exchange_note",multiline=true,enabled=!s.busy,minimumHeight=108)
        HorizontalDivider(color=ExchangeBorder);TdSection(4,"Địa điểm")
        RmField("Tỉnh/Thành phố hiện tại *",f.tinhThanh,{v->vm.form{it.copy(tinhThanh=v.take(100))}},"Nhập Tỉnh/Thành phố","exchange_city",enabled=!s.busy,
            trailing={Icon(painterResource(R.drawable.ic_fg_exchange_city_down),null,Modifier.size(18.dp),tint=Color.Unspecified)});LoiTruong(f.errors,"tinhThanh")
        Text("Giúp kết nối với những người trao đổi gần bạn.",style=rmText(11),color=ReMarketPalette.Muted)
        HorizontalDivider(color=ExchangeBorder)
        Text("Kiểm tra thông tin và ảnh trước khi đăng. Bạn có thể sửa, ẩn hoặc xóa tin trong “Trao đổi của tôi”.",style=rmText(12),color=ReMarketPalette.Muted)
        TdButton("Đăng tin trao đổi",{vm.luu(true)},Modifier.testTag("exchange_publish"),!s.busy)
        TdButton("Lưu bản nháp",{vm.luu(false)},Modifier.testTag("exchange_save_draft"),!s.busy,true)
        Text("Bản nháp chỉ hiển thị với bạn. Tin đăng cần được quản trị duyệt trước khi xuất hiện công khai.",style=rmText(11),color=ReMarketPalette.Muted)
    }
}
@Composable fun ChiTietTraoDoiScreen(s:TraoDoiState,loader:ImageLoader,profile:HoSo){val tin=s.detail?:return;val need=tin.traoDoi
    Column(Modifier.fillMaxSize().testTag("exchange_detail").verticalScroll(rememberScrollState()).padding(24.dp),verticalArrangement=Arrangement.spacedBy(18.dp)){
        if(tin.anh.isNotEmpty()){val pager=rememberPagerState(pageCount={tin.anh.size});HorizontalPager(pager,Modifier.fillMaxWidth().height(190.dp).clip(RoundedCornerShape(20.dp))){i->AnhSanPham(tin.anh[i].url,loader,Modifier.fillMaxSize(),tin.ten)}
            if(tin.anh.size>1)Text("${pager.currentPage+1}/${tin.anh.size} ảnh · Vuốt để xem",style=rmText(11),color=ReMarketPalette.Muted)}
        Text(tin.ten,style=rmText(22,FontWeight.SemiBold));Text(tien(tin.giaTriThamKhao),style=rmText(22,FontWeight.Bold),color=ReMarketPalette.Primary)
        Text("${tinhTrangText(tin.tinhTrang)} · ${tin.danhMuc}",style=rmText(13));Text(tin.tinhThanh,style=rmText(12),color=ReMarketPalette.Muted)
        if(s.own)Text(trangThaiText(tin.trangThai),style=rmText(13,FontWeight.SemiBold),color=ReMarketPalette.Primary)
        tin.lyDoTuChoi?.let{RmNotice("Lý do từ chối: $it",warning=true)}
        Text("Mô tả",style=rmText(17,FontWeight.SemiBold));Text(tin.moTa,style=rmText(14))
        TdCard{Text("Sản phẩm muốn nhận",style=rmText(16,FontWeight.SemiBold));Text(need?.moTaMongMuon?:"Chưa có nhu cầu trao đổi.",style=rmText(14))
            if(need!=null){Text(listOfNotNull(if(need.choPhepMotMot)"1–1" else null,if(need.choPhepSmartSwap)"Smart Swap 3 bên" else null).joinToString(" · "),style=rmText(12),color=ReMarketPalette.Primary)
                Text(if(need.choPhepBuTien)"Cho phép bù tiền · Chủ tin có thể bù tối đa ${tien(need.soTienBuToiDa)}" else "Không bù tiền",style=rmText(12));need.ghiChu?.let{Text(it,style=rmText(12),color=ReMarketPalette.Muted)}}}
        TdCard{Text(tin.nguoiDang,style=rmText(16,FontWeight.SemiBold));Text(if(tin.daXacMinhDienThoai)"Đã xác minh liên hệ" else "Chưa xác minh liên hệ",style=rmText(12),color=ReMarketPalette.Muted)
            if(profile.id==tin.nguoiDangId)Text("Tin của bạn",style=rmText(12),color=ReMarketPalette.Primary)}
        if(s.own&&!tin.coTheChinhSua)RmNotice("Nội dung đã được giao dịch tham chiếu hoặc giữ chỗ; lịch sử được bảo toàn.")
    }
}
@Composable fun TinTraoDoiXong(s:TraoDoiState,vm:TraoDoiViewModel){Column(Modifier.fillMaxSize().testTag("exchange_listing_success").verticalScroll(rememberScrollState()).padding(24.dp),verticalArrangement=Arrangement.spacedBy(20.dp)){
    TdNote("Đã gửi tin trao đổi","${s.detail?.ten.orEmpty()} đang chờ quản trị duyệt. Tin sẽ xuất hiện khi được chấp thuận.")
    TdButton("Về tin trao đổi của tôi",{vm.tinToi()},Modifier.testTag("exchange_success_mine"));TdButton("Đăng thêm tin",{vm.taoTin()},outlined=true)
}}
