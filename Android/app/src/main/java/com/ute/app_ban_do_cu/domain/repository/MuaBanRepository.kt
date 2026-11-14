package com.ute.app_ban_do_cu.domain.repository

import com.ute.app_ban_do_cu.domain.model.*

interface MuaBanRepository {
    suspend fun moChat(tinId: String): HoiThoai
    suspend fun moChatDon(donId: String): HoiThoai
    suspend fun hopThu(unread: Boolean, page: Int): List<HopThuMua>
    suspend fun chat(id: String, before: Long? = null): HoiThoai
    suspend fun guiTin(id: String, body: GuiTinBody): HoiThoai
    suspend fun guiAnh(id: String, key: String, bytes: ByteArray): HoiThoai
    suspend fun taoPhieu(id: String, body: TaoPhieuBody): HoiThoai
    suspend fun traLoiPhieu(id: String, coupon: String, body: TraLoiPhieuBody): HoiThoai
    suspend fun baoGia(body: BaoGiaBody): BaoGia
    suspend fun taoDon(body: TaoDonBody): DonMua
    suspend fun don(id: String): DonMua
    suspend fun donHang(role: String, status: String?, page: Int): TrangDonMua
    suspend fun thanhToan(id: String, body: ThanhToanBody): DonMua
    suspend fun huy(id: String, body: HuyDonBody): DonMua
    suspend fun ship(id: String, body: ShipBody): DonMua
    suspend fun nhan(id: String, version: String): DonMua
    suspend fun danhGia(id: String, body: DanhGiaBody): DanhGiaMua
    suspend fun taoHoTro(body: HoTroBody): HoTroMua
    suspend fun hoTro(id: String, admin: Boolean): HoTroMua
    suspend fun hoTroList(page: Int, admin: Boolean): List<HoTroMua>
    suspend fun phanHoi(id: String, body: PhanHoiHoTroBody, admin: Boolean): HoTroMua
}
