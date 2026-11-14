package com.ute.app_ban_do_cu.domain.model

data class DieuKienTraoDoi(val moTaMongMuon:String,val choPhepMotMot:Boolean,val choPhepSmartSwap:Boolean,
    val choPhepBuTien:Boolean,val soTienBuToiDa:Long,val ghiChu:String?=null)
data class BoLocTraoDoi(val tuKhoa:String="",val danhMucId:String?=null,val tinhThanh:String="",val giaTu:Long?=null,
    val giaDen:Long?=null,val tinhTrang:String?=null,val choPhepBuTien:Boolean?=null,val choPhepMotMot:Boolean?=null,
    val choPhepSmartSwap:Boolean?=null,val sapXep:String="moinhat")
data class MonTraoDoi(val sanPhamId:String,val tinDangId:String?,val ten:String,val giaTriThamKhao:Long,val tinhTrang:String,
    val nguoiDungId:String,val chuSoHuu:String,val anhUrl:String?)
data class DongYTraoDoi(val nguoiDungId:String,val hoTen:String,val quyetDinh:String,val ngayPhanHoi:String?)
data class PhienTraoDoi(val id:String,val soPhienBan:Int,val nguoiTaoId:String,val nguoiTao:String,val trangThai:String,val loiNhan:String,
    val diaDiemHen:String?,val thoiGianHen:String?,val soTienBu:Long,val nguoiTraId:String?,val nguoiNhanTienId:String?,
    val nguoiTraDaXacNhan:Boolean,val nguoiNhanDaXacNhan:Boolean,val ngayTao:String,val dongY:List<DongYTraoDoi>)
data class XacNhanTraoDoi(val nguoiDungId:String,val hoTen:String,val daGiao:Boolean,val daNhan:Boolean,val ngayNhan:String?)
data class BangChungTraoDoi(val id:String,val nguoiTaiLenId:String,val url:String)
data class DanhGiaTraoDoi(val id:String,val nguoiDanhGiaId:String,val soSao:Int,val noiDung:String,val ngayTao:String,val tieuChiIds:List<String>)
data class DeNghiTraoDoi(val id:String,val giaoDichId:String,val ma:String,val phienBan:String,val trangThai:String,val trangThaiGiaoDich:String,
    val nguoiGuiId:String,val nguoiNhanId:String,val ngayTao:String,val ngayHetHan:String,val ngayHoanTat:String?,val lyDoKetThuc:String?,
    val monGui:MonTraoDoi,val monNhan:MonTraoDoi,val thoaThuan:PhienTraoDoi?,val lichSu:List<PhienTraoDoi>,val xacNhan:List<XacNhanTraoDoi>,
    val bangChung:List<BangChungTraoDoi>,val danhGia:List<DanhGiaTraoDoi>,val coTheDongY:Boolean,val coTheTuChoi:Boolean,val coTheDeXuatLai:Boolean,
    val coTheHuy:Boolean,val coTheGiaoNhan:Boolean,val coTheDanhGia:Boolean,val sanPhamConKhaDung:Boolean,val daXacNhanCuaToi:Boolean)
data class TrangDeNghi(val items:List<DeNghiTraoDoi>,val tongSo:Int,val trang:Int,val tongTrang:Int)
data class TieuChiTraoDoi(val id:String,val ma:String,val ten:String)
data class TaoDeNghiBody(val maYeuCau:String,val tinGuiId:String,val tinNhanId:String,val phienBanTinGui:String,val phienBanTinNhan:String,
    val soTienBu:Long,val nguoiTraId:String?,val loiNhan:String,val diaDiemHen:String?,val thoiGianHen:String?,val dongYQuyDinh:Boolean)
data class DoiDeNghiBody(val phienBan:String,val soTienBu:Long,val nguoiTraId:String?,val loiNhan:String,val diaDiemHen:String?,val thoiGianHen:String?)
data class PhanHoiDeNghiBody(val phienBan:String,val dongY:Boolean,val lyDo:String?=null)
data class HuyDeNghiBody(val phienBan:String,val lyDo:String)
data class GiaoNhanTraoDoiBody(val phienBan:String,val daKiemTra:Boolean,val daGiaoMonCuaToi:Boolean,val daNhanDu:Boolean,val daXacNhanTienBu:Boolean)
data class DanhGiaTraoDoiBody(val maYeuCau:String,val soSao:Int,val noiDung:String,val tieuChiIds:List<String>)
