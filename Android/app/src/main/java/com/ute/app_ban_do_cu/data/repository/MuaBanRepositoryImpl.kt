package com.ute.app_ban_do_cu.data.repository

import com.ute.app_ban_do_cu.data.remote.MuaBanApi
import com.ute.app_ban_do_cu.domain.model.*
import com.ute.app_ban_do_cu.domain.repository.MuaBanRepository
import okhttp3.MediaType.Companion.toMediaType
import okhttp3.MultipartBody
import okhttp3.RequestBody.Companion.toRequestBody

class MuaBanRepositoryImpl(private val api: MuaBanApi) : MuaBanRepository {
    override suspend fun moChat(tinId: String) = api.moChat(MoChatBody(tinId))
    override suspend fun moChatDon(donId: String) = api.chatDon(donId)
    override suspend fun hopThu(unread: Boolean, page: Int) = api.hopThu(unread, page)
    override suspend fun chat(id: String, before: Long?) = api.chat(id, before)
    override suspend fun guiTin(id: String, body: GuiTinBody) = api.gui(id, body)
    override suspend fun guiAnh(id: String, key: String, bytes: ByteArray) = api.anh(id, key.toRequestBody("text/plain".toMediaType()),
        MultipartBody.Part.createFormData("anh", "chat.jpg", bytes.toRequestBody("application/octet-stream".toMediaType())))
    override suspend fun taoPhieu(id: String, body: TaoPhieuBody) = api.taoPhieu(id, body)
    override suspend fun traLoiPhieu(id: String, coupon: String, body: TraLoiPhieuBody) = api.traLoi(id, coupon, body)
    override suspend fun baoGia(body: BaoGiaBody) = api.baoGia(body)
    override suspend fun taoDon(body: TaoDonBody) = api.taoDon(body)
    override suspend fun don(id: String) = api.don(id)
    override suspend fun donHang(role: String, status: String?, page: Int) = api.donList(role, status, page)
    override suspend fun thanhToan(id: String, body: ThanhToanBody) = api.thanhToan(id, body)
    override suspend fun huy(id: String, body: HuyDonBody) = api.huy(id, body)
    override suspend fun ship(id: String, body: ShipBody) = api.ship(id, body)
    override suspend fun nhan(id: String, version: String) = api.nhan(id, PhienBanTinBody(version))
    override suspend fun danhGia(id: String, body: DanhGiaBody) = api.danhGia(id, body)
    override suspend fun taoHoTro(body: HoTroBody) = api.taoHoTro(body)
    override suspend fun hoTro(id: String, admin: Boolean) = if (admin) api.adminSupport(id) else api.support(id)
    override suspend fun hoTroList(page: Int, admin: Boolean) = if (admin) api.adminList(page) else api.supportList(page)
    override suspend fun phanHoi(id: String, body: PhanHoiHoTroBody, admin: Boolean) = if (admin) api.adminPhanHoi(id, body) else api.phanHoi(id, body)
}
