package com.ute.app_ban_do_cu.domain.model

data class SsMon(val tinDangId:String,val sanPhamId:String,val nguoiDungId:String,val ten:String,val giaTriThamKhao:Long,val tinhTrang:String,val tinhThanh:String,val phienBan:String)
data class SsHoSo(val nguoiDungId:String,val hoTen:String)
data class SsChang(val nguoiGiaoId:String,val nguoiNhanId:String,val sanPham:SsMon)
data class SsCanDoi(val nguoiDungId:String,val tinChoId:String,val tinNhanId:String,val giaTriCho:Long,val giaTriNhan:Long,val tienCanBu:Long,val tienDuocNhan:Long,val mucBuToiDa:Long)
data class SsTienBu(val nguoiTraId:String,val nguoiNhanId:String,val soTien:Long)
data class SsGoiY(val maDeXuat:String,val diemPhuHop:Double,val tongTienBu:Long,val chang:List<SsChang>,val thanhVien:List<SsCanDoi>,val tienBu:List<SsTienBu>)
data class SsKetQua(val items:List<SsGoiY>,val tongSo:Int,val trang:Int,val tongTrang:Int,val thongBao:String,val hoSo:List<SsHoSo>?)
data class SsPreview(val vong:SsGoiY,val hoSo:List<SsHoSo>,val bamPhuongAn:String,val coTheKhoiTao:Boolean)
data class SsDongY(val nguoiDungId:String,val hoTen:String,val quyetDinh:String,val ngayPhanHoi:String?)
data class SsPhuongAn(val id:String,val soPhienBan:Int,val nguoiTaoId:String,val trangThai:String,val loiNhan:String,val ngayTao:String,
    val tienBu:List<SsTienBu>,val tongTienTra:Long,val tongTienNhan:Long,val dongY:List<SsDongY>)
data class SsThanhVien(val id:String,val viTri:Int,val nguoiDungId:String,val hoTen:String,val sanPhamCho:SsMon,val nguoiGiaoChoToiId:String,
    val nguoiNhanMonCuaToiId:String,val sanPhamNhanId:String,val moTaSanPham:String,val nhuCau:DieuKienTraoDoi?,val anhUrl:String?)
data class SsTienDo(val soNguoiNhanDung:Int,val tongSoNguoi:Int,val tienDo:String,val tamDung:Boolean,val hoanTat:Boolean)
data class SsVong(val id:String,val giaoDichId:String,val ma:String,val nguoiKhoiTaoId:String?,val trangThai:String,val trangThaiGiaoDich:String,val ngayHetHan:String,
    val maKetThuc:String?,val lyDoKetThuc:String?,val hoiThoaiId:String?,val thanhVien:List<SsThanhVien>,val chang:List<SsChang>,
    val phuongAnHienTai:SsPhuongAn?,val lichSu:List<SsPhuongAn>,val coThePhanHoi:Boolean,val coTheDeXuat:Boolean,val coTheHuy:Boolean,val coTheBanGiao:Boolean,val tienDoGiaoNhan:SsTienDo?)
data class SsTrangVong(val items:List<SsVong>,val tongSo:Int,val trang:Int,val tongTrang:Int)
data class SsThongBao(val id:String,val vongId:String,val tieuDe:String,val noiDung:String,val daDoc:Boolean,val ngayTao:String)
data class SsLienHe(val nguoiDungId:String,val hoTen:String,val daXacNhan:Boolean,val phienBan:String?,val tenLienHe:String?,val soDienThoai:String?,val diaChiGui:String?,val diaChiNhan:String?)
data class SsMoc(val soThuTu:Long,val trangThai:String,val moTa:String,val thoiDiem:String)
data class SsShip(val id:String,val nguoiGuiId:String,val nguoiNhanId:String,val sanPhamId:String,val tenSanPham:String,val maMoPhong:String,val maVanDon:String?,val trangThai:String,val phienBan:String,
    val ngayNhanDungHang:String?,val tenNguoiGui:String?,val dienThoaiGui:String?,val diaChiGui:String?,val tenNguoiNhan:String?,val dienThoaiNhan:String?,val diaChiNhan:String?,val lichSu:List<SsMoc>,
    val coTheNhapMa:Boolean,val coTheCapNhat:Boolean,val coTheNhanDungHang:Boolean,val coTheBaoSuCo:Boolean)
data class SsSuCo(val id:String,val vanChuyenId:String,val nguoiBaoId:String,val ma:String,val loaiSuCo:String,val tieuDe:String,val noiDung:String,val trangThai:String,val ngayTao:String)
data class SsGiaoNhan(val vongId:String,val phienBanThoaThuanId:String?,val trangThaiVong:String,val trangThaiGiaoDich:String,val soNguoiXacNhanThongTin:Int,
    val tienDo:SsTienDo,val lienHe:List<SsLienHe>,val vanChuyen:List<SsShip>,val suCo:List<SsSuCo>,val coTheTaoVanChuyen:Boolean,val bangChung:List<BangChungTraoDoi>?)
data class SsReview(val id:String,val nguoiDuocDanhGiaId:String,val hoTen:String,val soSao:Int,val noiDung:String,val ngayTao:String)
data class SsTim(val tinDangId:String?=null,val tuKhoaMuonNhan:String="",val buToiDa:Long?=null,val tinhTrangToiThieu:String?=null,val tinhThanh:String="",val verified:Boolean=false)
data class SsPreviewBody(val maDeXuat:String)
data class SsMoiBody(val maYeuCau:String,val maDeXuat:String,val bamPhuongAn:String,val loiNhan:String,val dongYPhuongAn:Boolean)
data class SsPhanHoiBody(val phienBanThoaThuanId:String,val dongY:Boolean,val daDocPhuongAn:Boolean,val lyDo:String?=null)
data class SsDeXuatBody(val maYeuCau:String,val phienBanCoSoId:String,val tienBu:List<SsTienBu>,val loiNhan:String)
data class SsHuyBody(val phienBanThoaThuanId:String,val lyDo:String)
data class SsContactBody(val maYeuCau:String,val phienBanThoaThuanId:String,val tenLienHe:String,val soDienThoai:String,val diaChiGui:String,val diaChiNhan:String,val xacNhanThongTin:Boolean,val phienBan:String?)
data class SsBookBody(val phienBanThoaThuanId:String)
data class SsTrackingBody(val phienBan:String,val maVanDon:String)
data class SsStatusBody(val phienBan:String,val trangThai:String,val xacNhanDaGui:Boolean)
data class SsReceiveBody(val phienBanThoaThuanId:String,val daKiemTraHang:Boolean,val nhanDungHang:Boolean,val daXacNhanTienBuCuaToi:Boolean)
data class SsIssueBody(val maYeuCau:String,val vanChuyenId:String,val loaiSuCo:String,val tieuDe:String,val noiDung:String)
data class SsReviewItem(val nguoiDuocDanhGiaId:String,val soSao:Int,val noiDung:String)
data class SsReviewBody(val maYeuCau:String,val danhGia:List<SsReviewItem>)
