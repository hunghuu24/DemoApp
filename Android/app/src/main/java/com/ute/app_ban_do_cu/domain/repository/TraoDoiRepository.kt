package com.ute.app_ban_do_cu.domain.repository
import com.ute.app_ban_do_cu.domain.model.*

interface TraoDoiRepository {
    suspend fun tim(filter:BoLocTraoDoi,page:Int):TrangTin
    suspend fun cuaToi(status:String?,page:Int):TinCuaToi
    suspend fun monChon(query:String,page:Int):TrangTin
    suspend fun deNghi(tab:String,page:Int):TrangDeNghi
    suspend fun doc(id:String):DeNghiTraoDoi
    suspend fun tao(body:TaoDeNghiBody):DeNghiTraoDoi
    suspend fun phanHoi(id:String,body:PhanHoiDeNghiBody):DeNghiTraoDoi
    suspend fun doi(id:String,body:DoiDeNghiBody):DeNghiTraoDoi
    suspend fun huy(id:String,body:HuyDeNghiBody):DeNghiTraoDoi
    suspend fun nhan(id:String,body:GiaoNhanTraoDoiBody):DeNghiTraoDoi
    suspend fun chat(id:String):HoiThoai
    suspend fun bangChung(id:String,key:String,bytes:ByteArray):DeNghiTraoDoi
    suspend fun tieuChi():List<TieuChiTraoDoi>
    suspend fun danhGia(id:String,body:DanhGiaTraoDoiBody):DeNghiTraoDoi
}
