package com.ute.app_ban_do_cu.feature.bootstrap

import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import androidx.lifecycle.viewModelScope
import androidx.lifecycle.viewmodel.initializer
import androidx.lifecycle.viewmodel.viewModelFactory
import com.google.gson.JsonParseException
import com.ute.app_ban_do_cu.domain.repository.HeThongRepository
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch
import retrofit2.HttpException
import java.io.IOException

data class KhoiTaoUiState(val dangTai: Boolean = false, val thongBao: String = "Sẵn sàng kiểm tra kết nối máy chủ.")

class KhoiTaoViewModel(private val repository: HeThongRepository) : ViewModel() {
    private val state = MutableStateFlow(KhoiTaoUiState())
    val uiState = state.asStateFlow()

    fun kiemTraKetNoi() {
        if (state.value.dangTai) return
        viewModelScope.launch {
            state.value = KhoiTaoUiState(true, "Đang kết nối…")
            state.value = try {
                val result = repository.kiemTraKetNoi()
                KhoiTaoUiState(thongBao = if (result.coSoDuLieuSanSang) "Đã kết nối ReMarket và cơ sở dữ liệu."
                    else "Máy chủ đang chạy, cơ sở dữ liệu chưa sẵn sàng.")
            } catch (_: IOException) {
                KhoiTaoUiState(thongBao = "Chưa kết nối được máy chủ. Vui lòng thử lại.")
            } catch (_: HttpException) {
                KhoiTaoUiState(thongBao = "Máy chủ trả về lỗi. Vui lòng thử lại.")
            } catch (_: JsonParseException) {
                KhoiTaoUiState(thongBao = "Phản hồi máy chủ chưa đúng định dạng.")
            }
        }
    }

    companion object {
        fun factory(repository: HeThongRepository): ViewModelProvider.Factory = viewModelFactory {
            initializer { KhoiTaoViewModel(repository) }
        }
    }
}
