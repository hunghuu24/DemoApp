package com.ute.app_ban_do_cu.feature.admin

import androidx.compose.foundation.*
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
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
import com.ute.app_ban_do_cu.feature.smartswap.SsCheck

@Composable fun QtUsers(s:QuanTriState,vm:QuanTriViewModel){QtBody("qt_users"){
    QtSearch(s,vm,"Tên, email hoặc số điện thoại"){vm.users()}
    QtTabs(s.status,listOf(null to "Tất cả","HoatDong" to "Đang hoạt động","DaKhoa" to "Đã khóa","DaXoa" to "Đã xóa"),!s.busy){vm.users(it)}
    s.users?.let{p->Text("${p.tongSo} người dùng",style=rmText(13),color=ReMarketPalette.Muted);p.items.forEach{u->TdCard(Modifier.testTag("qt_user_${u.id}").clickable(enabled=!s.busy){vm.user(u.id)}){
        Row(horizontalArrangement=Arrangement.spacedBy(10.dp),verticalAlignment=Alignment.CenterVertically){QtAvatar(u.hoTen);Column(Modifier.weight(1f)){Text(u.hoTen,style=rmText(15,FontWeight.SemiBold));Text(u.soDienThoai?:u.email,style=rmText(11),color=ReMarketPalette.Muted)}}
        QtBadge(u.trangThai);Text("${u.soTin} tin · ${u.soGiaoDich} giao dịch · ${u.soLanKhoa} lần khóa",style=rmText(11));Text("Chi tiết ›",style=rmText(12),color=ReMarketPalette.Primary)
    }};if(p.items.isEmpty())TrangRong("Không có người dùng phù hợp","Thay từ khóa hoặc trạng thái.");PhanTrang(p.trang,p.tongTrang,!s.busy,{vm.users(page=p.trang-1)},{vm.users(page=p.trang+1)})}
}}
@Composable fun QtUserScreen(s:QuanTriState){val d=s.user?:return;val u=d.nguoiDung;QtBody("qt_user_detail"){
    Row(horizontalArrangement=Arrangement.spacedBy(12.dp),verticalAlignment=Alignment.CenterVertically){QtAvatar(u.hoTen);Column{Text(u.hoTen,style=rmText(18,FontWeight.SemiBold));Text(u.email,style=rmText(12),color=ReMarketPalette.Muted)}}
    QtBadge(u.trangThai);Text("Thành viên từ ${ngayGio(u.ngayThamGia)}",style=rmText(11),color=ReMarketPalette.Muted)
    TdCard{Text("Liên hệ",style=rmText(15,FontWeight.SemiBold));Text("Email: ${u.email}${if(u.daXacMinhEmail)" · Đã xác minh" else ""}",style=rmText(13));Text("Điện thoại: ${u.soDienThoai?:"Chưa có"}${if(u.daXacMinhDienThoai)" · Đã xác minh" else ""}",style=rmText(13));Text("Khu vực: ${u.tinhThanh?:"Chưa cập nhật"}",style=rmText(13));Text("Vai trò: ${u.vaiTro.joinToString()}",style=rmText(12))}
    TdCard{Text("Hoạt động tài khoản",style=rmText(16,FontWeight.SemiBold));Text("Tin đăng: ${u.soTin} tin",style=rmText(13));Text("Giao dịch: ${u.soGiaoDich}",style=rmText(13));Text("Đánh giá: ${u.diemDanhGia?.let{String.format(java.util.Locale.forLanguageTag("vi-VN"),"%.1f / 5",it)}?:"Chưa có"} · ${u.soDanhGia} đánh giá",style=rmText(13));Text("Lịch sử khóa: ${u.soLanKhoa} lần",style=rmText(13))}
    d.khoa.forEach{l->TdCard{Text("Lý do: ${l.lyDo}",style=rmText(13,FontWeight.SemiBold));Text("Ngày khóa: ${ngayGio(l.ngayBatDau)}",style=rmText(12));Text("Hết hạn: ${l.ngayKetThuc?.let(::ngayGio)?:"Vĩnh viễn"}",style=rmText(12));l.ngayMoKhoa?.let{Text("Đã mở: ${ngayGio(it)}",style=rmText(12),color=ReMarketPalette.Primary)};Text("Thực hiện: ${l.quanTriVien}",style=rmText(11),color=ReMarketPalette.Muted)}}
    Text("Nhật ký xử lý",style=rmText(16,FontWeight.SemiBold));QtTimeline(d.nhatKy);if(d.nhatKy.isEmpty())Text("Chưa có nhật ký quản trị.",style=rmText(12),color=ReMarketPalette.Muted)
    TdNote("Quyền và lịch sử","Khóa cần lý do, thời hạn và được lưu nhật ký. Không xóa các giao dịch đang có; mở khóa yêu cầu đăng nhập lại.")
}}
@Composable fun QtListings(s:QuanTriState,vm:QuanTriViewModel,loader:ImageLoader){QtBody("qt_listings"){
    QtSearch(s,vm,"Tên sản phẩm hoặc mã tin"){vm.listings()};QtTabs(s.status,listOf(null to "Tất cả","ChoDuyet" to "Chờ duyệt","DangHienThi" to "Đang hiển thị","TuChoi" to "Đã từ chối","DaAn" to "Đã ẩn"),!s.busy){vm.listings(it)}
    s.listings?.let{p->Text("${p.tongSo} tin · Cũ nhất trước",style=rmText(13),color=ReMarketPalette.Muted);p.items.forEach{t->TdCard(Modifier.testTag("qt_listing_${t.id}").clickable(enabled=!s.busy){vm.listing(t.id)}){
        Row(horizontalArrangement=Arrangement.spacedBy(12.dp)){AnhSanPham((t.anh.firstOrNull{it.laAnhBia}?:t.anh.firstOrNull())?.url,loader,Modifier.size(64.dp).clip(RoundedCornerShape(12.dp)),t.ten);Column(Modifier.weight(1f)){Text(t.ten,style=rmText(15,FontWeight.SemiBold));Text(tien(t.giaBan?:t.giaTriThamKhao),style=rmText(15,FontWeight.Bold),color=ReMarketPalette.Primary);Text("${t.nguoiDang} · ${t.tinhThanh}",style=rmText(11),color=ReMarketPalette.Muted)}}
        QtBadge(t.trangThai);Text("${qtStatus(t.loaiTin)} · Xem chi tiết ›",style=rmText(12),color=ReMarketPalette.Primary)
    }};if(p.items.isEmpty())TrangRong("Không có tin phù hợp","Chọn trạng thái hoặc từ khóa khác.");PhanTrang(p.trang,p.tongTrang,!s.busy,{vm.listings(page=p.trang-1)},{vm.listings(page=p.trang+1)})}
}}
@Composable fun QtListingScreen(s:QuanTriState,vm:QuanTriViewModel,loader:ImageLoader){val d=s.listing?:return;val t=d.tin;QtBody("qt_listing_detail"){
    QtBadge(t.trangThai);t.anh.forEach{photo->AnhSanPham(photo.url,loader,Modifier.fillMaxWidth().height(170.dp).clip(RoundedCornerShape(16.dp)),t.ten)}
    Text(t.ten,style=rmText(20,FontWeight.SemiBold));Text(tien(t.giaBan?:t.giaTriThamKhao),style=rmText(20,FontWeight.Bold),color=ReMarketPalette.Primary);Text(t.moTa,style=rmText(13))
    TdCard{Text("Chủ tin: ${t.nguoiDang}",style=rmText(14,FontWeight.SemiBold));Text("${tinhTrangText(t.tinhTrang)} · ${t.danhMuc}",style=rmText(13));Text(t.diaDiem,style=rmText(12));t.traoDoi?.let{Text("Muốn nhận: ${it.moTaMongMuon}",style=rmText(12))};TextButton({vm.user(t.nguoiDangId)},enabled=!s.busy,modifier=Modifier.testTag("qt_listing_owner")){Text("Chi tiết người đăng ›")}}
    if(d.coTheDuyet){Text("Checklist kiểm duyệt",style=rmText(16,FontWeight.SemiBold));listOf("Thông tin và danh mục hợp lệ","Không thuộc hàng cấm","Ảnh đầy đủ, không trùng tin khác").forEachIndexed{i,label->SsCheck(label,i in s.checks,!s.busy,"qt_moderation_check_$i"){vm.check(i)}}}
    t.lyDoTuChoi?.let{RmNotice(it,warning=true)}
    Text("Lịch sử kiểm duyệt",style=rmText(16,FontWeight.SemiBold));QtTimeline(d.lichSu);if(d.lichSu.isEmpty())Text("Chưa có quyết định kiểm duyệt.",style=rmText(12),color=ReMarketPalette.Muted)
}}
@Composable fun QtCategories(s:QuanTriState,vm:QuanTriViewModel){QtBody("qt_categories"){
    RmField("Tìm tên danh mục",s.query,vm::query,"Nhập tên danh mục","qt_query",enabled=!s.busy)
    s.categories?.let{rows->Text("${rows.count{it.danhMucChaId==null}} danh mục gốc · Sắp xếp thủ công",style=rmText(12),color=ReMarketPalette.Muted)
        val filtered=rows.filter{it.ten.contains(s.query,true)};filtered.forEach{c->TdCard(Modifier.testTag("qt_category_${c.id}").clickable(enabled=!s.busy){vm.category(c)}){
            Row(verticalAlignment=Alignment.CenterVertically,horizontalArrangement=Arrangement.spacedBy(12.dp)){RmIcon(qtCategoryIcon(c.bieuTuong,c.ten),size=24.dp);Column(Modifier.weight(1f)){Text(c.ten,style=rmText(16,FontWeight.SemiBold));Text("${c.soTin} tin · ${c.soDanhMucCon} danh mục con",style=rmText(12),color=ReMarketPalette.Muted)}}
            QtBadge(if(c.dangHienThi)"DangHienThi" else "DaAn");Text("Thứ tự ${c.thuTu}${c.danhMucChaId?.let{p->" · Trong ${rows.firstOrNull{it.id==p}?.ten.orEmpty()}"}.orEmpty()}",style=rmText(11));Text("Chỉnh sửa ›",style=rmText(12),color=ReMarketPalette.Primary)
        }};if(filtered.isEmpty())TrangRong("Không có danh mục phù hợp","Thử tên khác hoặc thêm danh mục.")}
}}
fun qtCategoryIcon(icon:String,name:String="")=iconDanhMuc(name,icon)
@Composable fun QtCategoryFormScreen(s:QuanTriState,vm:QuanTriViewModel){val f=s.category;QtBody("qt_category_form"){
    Text(if(f.id==null)"Tạo danh mục cho các tin đăng."else "Cập nhật danh mục, giữ nguyên các tin và lịch sử hiện có.",style=rmText(12),color=ReMarketPalette.Muted)
    RmField("Tên danh mục",f.ten,{v->vm.categoryForm{it.copy(ten=v.take(120))}},"Nhập tên danh mục","qt_category_name",enabled=!s.busy)
    LuaChon("Danh mục cha",f.parent.orEmpty(),listOf("" to "Không có · Danh mục gốc")+s.categories.orEmpty().filter{it.id!=f.id}.map{it.id to it.ten},{v->vm.categoryForm{it.copy(parent=v.ifBlank{null})}},tag="qt_category_parent",enabled=!s.busy)
    LuaChon("Biểu tượng danh mục",f.icon,listOf("recycle" to "Tái sử dụng","smartphone" to "Điện tử","armchair" to "Nội thất","shirt" to "Thời trang","cooking-pot" to "Gia dụng","book-open" to "Sách","bike" to "Thể thao"),{v->vm.categoryForm{it.copy(icon=v)}},tag="qt_category_icon",enabled=!s.busy)
    RmIcon(qtCategoryIcon(f.icon),size=30.dp)
    RmField("Thứ tự hiển thị",f.order,{v->vm.categoryForm{it.copy(order=v.filter(Char::isDigit).take(6))}},"0","qt_category_order",keyboard=KeyboardType.Number,enabled=!s.busy)
    TdSwitch("Hiển thị danh mục trong ứng dụng","Ẩn danh mục cha cũng ẩn nhánh con khỏi khám phá.",f.visible,!s.busy,"qt_category_visible"){v->vm.categoryForm{it.copy(visible=v)}}
    RmNotice("Các tin hiện có không tự chuyển sang danh mục mới. Không chọn cha là chính danh mục hoặc nhánh con của nó.")
}}
