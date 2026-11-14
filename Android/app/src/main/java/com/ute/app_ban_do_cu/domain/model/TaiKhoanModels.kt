package com.ute.app_ban_do_cu.domain.model

data class HoSo(
    val id: String, val hoTen: String, val email: String, val soDienThoai: String?,
    val diaChi: String?, val tinhThanh: String?, val anhDaiDienUrl: String?,
    val daXacMinhEmail: Boolean, val daXacMinhDienThoai: Boolean,
    val vaiTro: List<String>, val phienBan: String, val ngayThamGia: String
)
data class PhienDto(val accessToken: String, val refreshToken: String,
    val accessTokenHetHan: String, val refreshTokenHetHan: String, val hoSo: HoSo)
data class DangKyBody(val hoTen: String, val email: String, val soDienThoai: String,
    val matKhau: String, val xacNhanMatKhau: String, val dongYDieuKhoan: Boolean = true)
data class DangNhapBody(val taiKhoan: String, val matKhau: String)
data class RefreshBody(val refreshToken: String)
data class QuenBody(val email: String)
data class DatLaiBody(val email: String, val token: String, val matKhauMoi: String, val xacNhanMatKhau: String)
data class DoiMatKhauBody(val matKhauCu: String, val matKhauMoi: String, val xacNhanMatKhau: String)
data class HoSoBody(val hoTen: String, val email: String, val soDienThoai: String,
    val diaChi: String?, val tinhThanh: String?, val phienBan: String)
data class ThongBao(val thongBao: String)
data class DanhMucTomTat(val id: String, val ten: String)
data class TinTomTat(val id: String, val ten: String, val giaBan: Long?, val tinhThanh: String)
data class TrangChuDto(val danhMuc: List<DanhMucTomTat>, val sanPham: List<TinTomTat>)
data class TongQuanDto(val tongNguoiDung: Int, val tinChoDuyet: Int, val muaBan: Int, val traoDoi: Int, val tranhChap: Int)

