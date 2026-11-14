package com.ute.app_ban_do_cu.domain.model

data class DanhMucBanHang(val id: String, val ten: String, val ma: String, val danhMucChaId: String?, val soTin: Int, val bieuTuong:String="")
data class AnhTin(val id: String, val url: String, val thuTu: Int, val laAnhBia: Boolean)
data class TinDang(
    val id: String, val sanPhamId: String, val ten: String, val danhMucId: String, val danhMuc: String,
    val giaBan: Long?, val giaTriThamKhao: Long, val tinhTrang: String, val trangThai: String, val loaiTin: String,
    val tinhThanh: String, val diaDiem: String, val moTa: String, val nguoiDangId: String, val nguoiDang: String,
    val daXacMinhDienThoai: Boolean, val ngayThamGia: String, val ngayDang: String?, val phienBan: String,
    val anh: List<AnhTin>, val luotXem: Int, val soCuocTroChuyen: Int, val lyDoTuChoi: String?, val coTheChinhSua: Boolean,
    val traoDoi: DieuKienTraoDoi? = null
)
data class TrangTin(val items: List<TinDang>, val tongSo: Int, val trang: Int, val kichThuocTrang: Int, val tongTrang: Int)
data class HomeSanPham(val danhMuc: List<DanhMucBanHang>, val noiBat: List<TinDang>, val moiDang: List<TinDang>)
data class DemTrangThai(val trangThai: String, val soTin: Int)
data class TinCuaToi(val danhSach: TrangTin, val thongKe: List<DemTrangThai>)
data class LuuTinBody(val ten: String, val danhMucId: String, val giaBan: Long, val tinhTrang: String,
    val moTa: String, val tinhThanh: String, val diaDiem: String, val phienBan: String?, val traoDoi: DieuKienTraoDoi? = null)
data class PhienBanTinBody(val phienBan: String)
data class DuyetTinBody(val phienBan: String, val chapNhan: Boolean, val lyDo: String?)
data class BoLocSanPham(val tuKhoa: String = "", val danhMucId: String? = null, val giaTu: Long? = null,
    val giaDen: Long? = null, val tinhTrang: String? = null, val tinhThanh: String = "", val sapXep: String = "moinhat",
    val loaiTin: String = "MuaBan")
data class HopThuItem(val id: String, val tieuDe: String, val tinCuoi: String?, val ngayTao: String)
