package com.ute.app_ban_do_cu.data.repository

import com.ute.app_ban_do_cu.data.remote.TinDangApi
import com.ute.app_ban_do_cu.domain.model.*
import com.ute.app_ban_do_cu.domain.repository.TinDangRepository
import okhttp3.MediaType.Companion.toMediaType
import okhttp3.MultipartBody
import okhttp3.RequestBody.Companion.toRequestBody

class TinDangRepositoryImpl(private val api: TinDangApi) : TinDangRepository {
    override suspend fun home() = api.home()
    override suspend fun danhMuc() = api.danhMuc()
    override suspend fun tim(filter: BoLocSanPham, page: Int) = api.tim(buildMap {
        put("tuKhoa", filter.tuKhoa); put("trang", page.toString()); put("kichThuocTrang", "6"); put("sapXep", filter.sapXep)
        put("loaiTin", filter.loaiTin); put("tinhThanh", filter.tinhThanh)
        filter.danhMucId?.let { put("danhMucId", it) }; filter.giaTu?.let { put("giaTu", it.toString()) }
        filter.giaDen?.let { put("giaDen", it.toString()) }; filter.tinhTrang?.let { put("tinhTrang", it) }
    })
    override suspend fun chiTiet(id: String, own: Boolean) = if (own) api.quanLy(id) else api.chiTiet(id)
    override suspend fun cuaToi(status: String?, page: Int) = api.cuaToi(status, page)
    override suspend fun luu(id: String?, form: LuuTinBody) = if (id == null) api.tao(form) else api.sua(id, form)
    override suspend fun themAnh(id: String, version: String, bytes: ByteArray) = api.themAnh(id,
        version.toRequestBody("text/plain".toMediaType()), MultipartBody.Part.createFormData("anh", "product", bytes.toRequestBody("application/octet-stream".toMediaType())))
    override suspend fun xoaAnh(id: String, photo: String, version: String) = api.xoaAnh(id, photo, PhienBanTinBody(version))
    override suspend fun anhBia(id: String, photo: String, version: String) = api.anhBia(id, photo, PhienBanTinBody(version))
    override suspend fun chuyen(id: String, version: String, action: String) = api.chuyen(id, action, PhienBanTinBody(version))
    override suspend fun xoa(id: String, version: String) = api.xoa(id, PhienBanTinBody(version))
    override suspend fun hopThu() = api.hopThu()
    override suspend fun choDuyet(page: Int) = api.choDuyet(page)
    override suspend fun duyet(id: String, version: String, accept: Boolean, reason: String?) = api.duyet(id, DuyetTinBody(version, accept, reason))
}
