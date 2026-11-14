package com.ute.app_ban_do_cu.data.repository
import com.ute.app_ban_do_cu.data.remote.QuanTriApi
import com.ute.app_ban_do_cu.domain.repository.QuanTriRepository
class QuanTriRepositoryImpl(api:QuanTriApi):QuanTriRepository by api
