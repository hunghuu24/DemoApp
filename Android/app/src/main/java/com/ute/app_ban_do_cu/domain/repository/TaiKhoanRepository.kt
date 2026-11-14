package com.ute.app_ban_do_cu.domain.repository

import com.ute.app_ban_do_cu.domain.model.*

interface TaiKhoanRepository {
    suspend fun khoiPhuc(): HoSo?
    suspend fun dangNhap(body: DangNhapBody): HoSo
    suspend fun dangKy(body: DangKyBody): HoSo
    suspend fun quen(body: QuenBody): String
    suspend fun datLai(body: DatLaiBody)
    suspend fun doiMatKhau(body: DoiMatKhauBody)
    suspend fun toi(): HoSo
    suspend fun capNhat(body: HoSoBody): HoSo
    suspend fun doiAnh(bytes: ByteArray): HoSo
    suspend fun dangXuat()
    suspend fun trangChu(): TrangChuDto
    suspend fun tongQuan(): TongQuanDto
    fun conPhien(): Boolean
}
