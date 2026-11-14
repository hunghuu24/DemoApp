package com.ute.app_ban_do_cu.data.remote

import retrofit2.http.GET

interface ReMarketApi {
    @GET("api/hethong/trangthai")
    suspend fun layTrangThai(): TrangThaiHeThongDto
}

data class TrangThaiHeThongDto(
    val tenUngDung: String,
    val phienBan: String,
    val coSoDuLieuSanSang: Boolean,
    val thoiGianUtc: String
)
