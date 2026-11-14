package com.ute.app_ban_do_cu.data.repository

import com.ute.app_ban_do_cu.data.remote.ReMarketApi
import com.ute.app_ban_do_cu.domain.model.TrangThaiKetNoi
import com.ute.app_ban_do_cu.domain.repository.HeThongRepository

class HeThongRepositoryImpl(private val api: ReMarketApi) : HeThongRepository {
    override suspend fun kiemTraKetNoi(): TrangThaiKetNoi {
        val response = api.layTrangThai()
        return TrangThaiKetNoi(response.tenUngDung, response.coSoDuLieuSanSang)
    }
}
