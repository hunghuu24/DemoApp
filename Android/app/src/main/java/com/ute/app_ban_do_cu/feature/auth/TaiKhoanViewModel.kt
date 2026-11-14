package com.ute.app_ban_do_cu.feature.auth

import android.net.Uri
import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import androidx.lifecycle.viewModelScope
import androidx.lifecycle.viewmodel.initializer
import androidx.lifecycle.viewmodel.viewModelFactory
import com.google.gson.JsonParseException
import com.google.gson.JsonParser
import com.ute.app_ban_do_cu.data.remote.*
import com.ute.app_ban_do_cu.domain.model.*
import com.ute.app_ban_do_cu.domain.repository.TaiKhoanRepository
import kotlinx.coroutines.CancellationException
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch
import java.io.IOException
import retrofit2.HttpException

enum class ManHinh { SPLASH, DANG_NHAP, DANG_KY, QUEN_MAT_KHAU, DAT_LAI, DOI_MAT_KHAU, TRANG_CHU, QUAN_TRI, CAI_DAT, HO_SO, SUA_HO_SO }
data class TaiKhoanUiState(
    val manHinh: ManHinh = ManHinh.SPLASH, val dangTai: Boolean = true,
    val hoSo: HoSo? = null, val trangChu: TrangChuDto? = null, val tongQuan: TongQuanDto? = null,
    val loi: String? = null, val thongBao: String? = null, val emailDangNhap: String = "", val emailKhoiPhuc: String = ""
)

class TaiKhoanViewModel(private val repository: TaiKhoanRepository) : ViewModel() {
    private val state = MutableStateFlow(TaiKhoanUiState())
    val uiState = state.asStateFlow()
    private var resetToken: String? = null
    private var coLienKet = false

    init { khoiPhuc() }
    fun khoiPhuc() = viewModelScope.launch {
        state.update { it.copy(dangTai = true, loi = null) }
        try {
            val profile = repository.khoiPhuc()
            if (!coLienKet) {
                if (profile != null) vaoTrangChinh(profile)
                else state.value = TaiKhoanUiState(dangTai = false)
            }
        } catch (ex: Exception) { if (ex is CancellationException) throw ex; state.update { it.copy(loi = moTaLoi(ex)) } }
        finally { state.update { it.copy(dangTai = false) } }
    }
    fun mo(manHinh: ManHinh) {
        if (state.value.dangTai) return
        state.update { it.copy(manHinh = manHinh, loi = null, thongBao = null) }
    }
    fun quayLai() {
        val target = when (state.value.manHinh) {
            ManHinh.SUA_HO_SO -> ManHinh.HO_SO
            ManHinh.HO_SO, ManHinh.DOI_MAT_KHAU -> ManHinh.CAI_DAT
            ManHinh.CAI_DAT -> if (state.value.hoSo?.vaiTro?.contains("ADMIN") == true) ManHinh.QUAN_TRI else ManHinh.TRANG_CHU
            else -> ManHinh.DANG_NHAP
        }
        mo(target)
    }
    private fun tacVu(action: suspend () -> Unit) {
        if (state.value.dangTai) return
        viewModelScope.launch {
            state.update { it.copy(dangTai = true, loi = null, thongBao = null) }
            try { action() }
            catch (ex: Exception) {
                if (ex is CancellationException) throw ex
                if (ex is HttpException && ex.code() == 401 && state.value.hoSo != null && !repository.conPhien())
                    state.value = TaiKhoanUiState(dangTai = false, manHinh = ManHinh.DANG_NHAP, loi = "Phiên đã hết hạn. Vui lòng đăng nhập lại.")
                else state.update { it.copy(loi = moTaLoi(ex)) }
            } finally { state.update { it.copy(dangTai = false) } }
        }
    }
    private suspend fun vaoTrangChinh(profile: HoSo) {
        val admin = profile.vaiTro.contains("ADMIN")
        state.value = TaiKhoanUiState(manHinh = if (admin) ManHinh.QUAN_TRI else ManHinh.TRANG_CHU, hoSo = profile, dangTai = true)
        if (admin) { val data = repository.tongQuan(); state.update { it.copy(tongQuan = data) } }
        // The catalog ViewModel now loads the full home with images and category counts.
    }
    fun dangNhap(account: String, password: String) {
        if (account.isBlank() || password.isBlank()) { baoLoi("Vui lòng nhập tài khoản và mật khẩu."); return }
        tacVu { vaoTrangChinh(repository.dangNhap(DangNhapBody(account.trim(), password))) }
    }
    fun dangKy(body: DangKyBody) {
        if (body.hoTen.trim().length < 2 || !android.util.Patterns.EMAIL_ADDRESS.matcher(body.email.trim()).matches()) {
            baoLoi("Vui lòng nhập họ tên và email hợp lệ."); return
        }
        if (!matKhauHopLe(body.matKhau, body.xacNhanMatKhau)) return
        tacVu {
            repository.dangKy(body)
            state.value = TaiKhoanUiState(manHinh = ManHinh.DANG_NHAP, dangTai = false,
                emailDangNhap = body.email, thongBao = "Đăng ký thành công. Mời bạn đăng nhập.")
        }
    }
    fun quen(email: String) {
        if (!android.util.Patterns.EMAIL_ADDRESS.matcher(email.trim()).matches()) { baoLoi("Vui lòng nhập email hợp lệ."); return }
        tacVu { val message = repository.quen(QuenBody(email.trim())); state.update { it.copy(thongBao = message) } }
    }
    fun nhanLienKet(uri: Uri?) {
        if (uri?.scheme != "remarket" || uri.host != "tai-khoan" || uri.path != "/dat-lai-mat-khau") return
        val email = uri.getQueryParameter("email") ?: return
        val token = uri.getQueryParameter("token") ?: return
        if (token.length !in 32..128) return
        coLienKet = true; resetToken = token
        state.value = TaiKhoanUiState(manHinh = ManHinh.DAT_LAI, dangTai = false, emailKhoiPhuc = email)
    }
    fun datLai(password: String, confirm: String) {
        if (!matKhauHopLe(password, confirm)) return
        val token = resetToken ?: return baoLoi("Vui lòng mở lại liên kết trong thư khôi phục.")
        tacVu {
            repository.datLai(DatLaiBody(state.value.emailKhoiPhuc, token, password, confirm))
            resetToken = null; coLienKet = false
            state.value = TaiKhoanUiState(manHinh = ManHinh.DANG_NHAP, dangTai = false, thongBao = "Đã đặt lại mật khẩu. Hãy đăng nhập bằng mật khẩu mới.")
        }
    }
    fun doiMatKhau(old: String, password: String, confirm: String) {
        if (old.isBlank()) return baoLoi("Vui lòng nhập mật khẩu hiện tại.")
        if (!matKhauHopLe(password, confirm)) return
        tacVu {
            repository.doiMatKhau(DoiMatKhauBody(old, password, confirm))
            state.value = TaiKhoanUiState(manHinh = ManHinh.DANG_NHAP, dangTai = false, thongBao = "Mật khẩu đã đổi. Vui lòng đăng nhập lại.")
        }
    }
    fun taiHoSo() = tacVu { val profile = repository.toi(); state.update { it.copy(hoSo = profile, manHinh = ManHinh.HO_SO, thongBao = "Đã tải lại hồ sơ.") } }
    fun capNhat(body: HoSoBody) = tacVu {
        val profile = repository.capNhat(body)
        state.update { it.copy(hoSo = profile, manHinh = ManHinh.HO_SO, thongBao = "Đã lưu thay đổi.") }
    }
    fun doiAnh(bytes: ByteArray) {
        if (bytes.isEmpty() || bytes.size > 2 * 1024 * 1024) return baoLoi("Vui lòng chọn ảnh JPG/PNG tối đa 2 MB.")
        tacVu {
            val profile = repository.doiAnh(bytes)
            state.update { it.copy(hoSo = profile, thongBao = "Đã đổi ảnh đại diện.") }
        }
    }
    fun dangXuat() = tacVu {
        repository.dangXuat()
        state.value = TaiKhoanUiState(manHinh = ManHinh.DANG_NHAP, dangTai = false, thongBao = "Bạn đã đăng xuất.")
    }
    fun taiTrangChinh() = tacVu { state.value.hoSo?.let { vaoTrangChinh(repository.toi()) } }
    fun baoLoi(message: String) { state.update { it.copy(loi = message) } }
    private fun matKhauHopLe(password: String, confirm: String): Boolean {
        val error = when {
            password.length !in 8..128 || !password.any(Char::isLetter) || !password.any(Char::isDigit) -> "Mật khẩu từ 8 đến 128 ký tự, gồm chữ và số."
            password != confirm -> "Xác nhận mật khẩu chưa khớp."
            else -> null
        }
        if (error != null) baoLoi(error)
        return error == null
    }
    private fun moTaLoi(error: Exception): String = when (error) {
        is IOException -> "Chưa kết nối được ReMarket. Kiểm tra mạng và thử lại."
        is JsonParseException -> "Chưa đọc được phản hồi từ máy chủ. Vui lòng thử lại."
        is HttpException -> {
            val text = error.response()?.errorBody()?.string()
            val detail = try {
                val json = JsonParser.parseString(text ?: "{}").asJsonObject
                json.get("detail")?.asString ?: json.getAsJsonObject("errors")?.entrySet()?.firstOrNull()?.value?.asJsonArray?.firstOrNull()?.asString
            } catch (_: Exception) { null }
            detail ?: when (error.code()) { 401 -> "Thông tin đăng nhập không đúng hoặc phiên đã hết hạn."; 403 -> "Bạn không có quyền thực hiện thao tác này."; 429 -> "Bạn đã thử nhiều lần. Vui lòng chờ trước khi thử lại."; else -> "Chưa thực hiện được yêu cầu. Vui lòng thử lại." }
        }
        else -> "Đã có lỗi xảy ra. Vui lòng thử lại."
    }
    companion object {
        fun factory(repository: TaiKhoanRepository): ViewModelProvider.Factory = viewModelFactory { initializer { TaiKhoanViewModel(repository) } }
    }
}
