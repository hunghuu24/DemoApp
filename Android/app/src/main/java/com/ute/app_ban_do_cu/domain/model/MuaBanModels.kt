package com.ute.app_ban_do_cu.domain.model

data class SanPhamMua(val tinDangId: String, val sanPhamId: String, val ten: String, val giaNiemYet: Long,
    val anhUrl: String?, val nguoiBanId: String, val nguoiBan: String, val tinhTrang: String, val trangThaiTin: String)
data class PhieuGiam(val id: String, val tinNhanId: String, val nguoiMuaId: String, val nguoiBanId: String,
    val giaNiemYet: Long, val soTienGiam: Long, val giaSauGiam: Long, val trangThai: String, val ngayHetHan: String, val phienBan: String)
data class AnhChat(val id: String, val url: String)
data class TinNhanMua(val id: String, val soThuTu: Long, val nguoiGuiId: String, val loai: String, val noiDung: String?, val ngayGui: String, val anh: List<AnhChat>,val phienBanThoaThuanId:String?=null)
data class HoiThoai(val id: String, val tieuDe: String, val doiTacId: String, val doiTac: String, val nguoiMuaId: String?,
    val sanPham: SanPhamMua?, val tinNhan: List<TinNhanMua>, val phieu: List<PhieuGiam>, val conTinCu: Boolean, val coTheThuongLuong: Boolean)
data class HopThuMua(val id: String, val tieuDe: String, val tinCuoi: String?, val ngayTao: String, val chuaDoc: Int, val doiTac: String, val anhUrl: String?)
data class BaoGia(val sanPham: SanPhamMua, val phienBanTin: String, val giaNiemYet: Long, val soTienGiam: Long,
    val tienHang: Long, val phiVanChuyen: Long, val tongTien: Long, val phieuThuongLuongId: String?, val phieuKhaDung: List<PhieuGiam>)
data class MocDon(val trangThai: String, val moTa: String, val thoiDiem: String?)
data class ThanhToanMua(val trangThai: String, val laMoPhong: Boolean, val dangGiuTien: Boolean, val ngayThanhToan: String?, val ngayGiaiNgan: String?, val ngayHoanTien: String?)
data class VanChuyenMua(val maVanDon: String, val trangThai: String, val ngayGiaoDuKien: String, val lichSu: List<MocDon>)
data class DanhGiaMua(val id: String, val soSaoNguoiBan: Int, val soSaoSanPham: Int?, val noiDung: String, val ngayTao: String)
data class DonMua(val id: String, val ma: String, val phienBan: String, val trangThai: String, val nguoiMuaId: String,
    val nguoiBanId: String, val nguoiMua: String, val nguoiBan: String, val ngayTao: String, val hanThanhToan: String?, val lyDoHuy: String?,
    val sanPham: SanPhamMua, val giaNiemYet: Long, val soTienGiam: Long, val tienHang: Long, val phiVanChuyen: Long, val tongTien: Long,
    val tenNguoiNhan: String, val soDienThoai: String, val diaChi: String, val thanhToan: ThanhToanMua, val vanChuyen: VanChuyenMua?,
    val lichSu: List<MocDon>, val danhGia: DanhGiaMua?, val coTheThanhToan: Boolean, val coTheHuy: Boolean, val coTheNhanHang: Boolean,
    val coTheDanhGia: Boolean, val buocVanChuyen: List<String>)
data class TrangDonMua(val items: List<DonMua>, val tongSo: Int, val trang: Int, val tongTrang: Int)
data class MocHoTro(val trangThai: String, val noiDung: String, val nguoiGui: String, val ngayTao: String)
data class HoTroMua(val id: String, val ma: String, val phienBan: String, val nguoiGuiId: String, val donHangId: String?, val maDon: String?,
    val loai: String, val trangThai: String, val tieuDe: String, val noiDung: String, val ngayTao: String, val lichSu: List<MocHoTro>)
data class MoChatBody(val tinDangId: String)
data class GuiTinBody(val maPhiaClient: String, val noiDung: String)
data class TaoPhieuBody(val maPhiaClient: String, val soTienGiam: Long)
data class TraLoiPhieuBody(val phienBan: String, val chapNhan: Boolean)
data class BaoGiaBody(val tinDangId: String, val phieuThuongLuongId: String?)
data class TaoDonBody(val maYeuCau: String, val tinDangId: String, val phienBanTin: String, val phieuThuongLuongId: String?,
    val tenNguoiNhan: String, val soDienThoai: String, val diaChi: String, val phuongThuc: String = "MoPhong")
data class ThanhToanBody(val maYeuCau: String, val phienBan: String, val thanhCong: Boolean)
data class HuyDonBody(val phienBan: String, val lyDo: String)
data class ShipBody(val phienBan: String, val trangThai: String)
data class DanhGiaBody(val maYeuCau: String, val soSaoNguoiBan: Int, val soSaoSanPham: Int, val noiDung: String)
data class HoTroBody(val maYeuCau: String, val donHangId: String?, val loai: String, val tieuDe: String, val noiDung: String)
data class PhanHoiHoTroBody(val phienBan: String, val noiDung: String, val trangThai: String?)
