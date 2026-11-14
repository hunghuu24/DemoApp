package com.ute.app_ban_do_cu.data.repository
import com.ute.app_ban_do_cu.data.remote.*
import com.ute.app_ban_do_cu.domain.model.*
import com.ute.app_ban_do_cu.domain.repository.TraoDoiRepository
import okhttp3.MultipartBody
import okhttp3.MediaType.Companion.toMediaType
import okhttp3.RequestBody.Companion.toRequestBody

class TraoDoiRepositoryImpl(private val api:TraoDoiApi,private val catalog:TinDangApi):TraoDoiRepository {
    override suspend fun tim(filter:BoLocTraoDoi,page:Int)=catalog.tim(buildMap{
        put("loaiTin","TraoDoi");put("tuKhoa",filter.tuKhoa);put("tinhThanh",filter.tinhThanh);put("trang",page.toString());put("kichThuocTrang","6");put("sapXep",filter.sapXep)
        filter.danhMucId?.let{put("danhMucId",it)};filter.giaTu?.let{put("giaTu",it.toString())};filter.giaDen?.let{put("giaDen",it.toString())}
        filter.tinhTrang?.let{put("tinhTrang",it)};filter.choPhepBuTien?.let{put("choPhepBuTien",it.toString())};filter.choPhepMotMot?.let{put("choPhepMotMot",it.toString())};filter.choPhepSmartSwap?.let{put("choPhepSmartSwap",it.toString())}
    })
    override suspend fun cuaToi(status:String?,page:Int)=api.cuaToi(status=status,page=page)
    override suspend fun monChon(query:String,page:Int)=api.mon(query,page)
    override suspend fun deNghi(tab:String,page:Int)=api.list(tab,page)
    override suspend fun doc(id:String)=api.doc(id)
    override suspend fun tao(body:TaoDeNghiBody)=api.tao(body)
    override suspend fun phanHoi(id:String,body:PhanHoiDeNghiBody)=api.phanHoi(id,body)
    override suspend fun doi(id:String,body:DoiDeNghiBody)=api.doi(id,body)
    override suspend fun huy(id:String,body:HuyDeNghiBody)=api.huy(id,body)
    override suspend fun nhan(id:String,body:GiaoNhanTraoDoiBody)=api.nhan(id,body)
    override suspend fun chat(id:String)=api.chat(id)
    override suspend fun bangChung(id:String,key:String,bytes:ByteArray)=api.evidence(id,key.toRequestBody("text/plain".toMediaType()),MultipartBody.Part.createFormData("anh","evidence.png",bytes.toRequestBody("image/png".toMediaType())))
    override suspend fun tieuChi()=api.tieuChi()
    override suspend fun danhGia(id:String,body:DanhGiaTraoDoiBody)=api.danhGia(id,body)
}
