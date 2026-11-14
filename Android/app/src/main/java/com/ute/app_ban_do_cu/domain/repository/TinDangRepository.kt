package com.ute.app_ban_do_cu.domain.repository

import com.ute.app_ban_do_cu.domain.model.*

interface TinDangRepository {
    suspend fun home(): HomeSanPham
    suspend fun danhMuc(): List<DanhMucBanHang>
    suspend fun tim(filter: BoLocSanPham, page: Int): TrangTin
    suspend fun chiTiet(id: String, own: Boolean): TinDang
    suspend fun cuaToi(status: String?, page: Int): TinCuaToi
    suspend fun luu(id: String?, form: LuuTinBody): TinDang
    suspend fun themAnh(id: String, version: String, bytes: ByteArray): TinDang
    suspend fun xoaAnh(id: String, photo: String, version: String): TinDang
    suspend fun anhBia(id: String, photo: String, version: String): TinDang
    suspend fun chuyen(id: String, version: String, action: String): TinDang
    suspend fun xoa(id: String, version: String)
    suspend fun hopThu(): List<HopThuItem>
    suspend fun choDuyet(page: Int): TrangTin
    suspend fun duyet(id: String, version: String, accept: Boolean, reason: String?): TinDang
}
