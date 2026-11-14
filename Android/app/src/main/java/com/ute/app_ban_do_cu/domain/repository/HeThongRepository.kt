package com.ute.app_ban_do_cu.domain.repository

import com.ute.app_ban_do_cu.domain.model.TrangThaiKetNoi

interface HeThongRepository {
    suspend fun kiemTraKetNoi(): TrangThaiKetNoi
}
