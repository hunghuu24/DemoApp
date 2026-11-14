package com.ute.app_ban_do_cu.feature.catalog

import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import androidx.lifecycle.viewModelScope
import androidx.lifecycle.viewmodel.initializer
import androidx.lifecycle.viewmodel.viewModelFactory
import com.google.gson.JsonParser
import com.ute.app_ban_do_cu.domain.model.*
import com.ute.app_ban_do_cu.domain.repository.TinDangRepository
import kotlinx.coroutines.CancellationException
import kotlinx.coroutines.Job
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch
import retrofit2.HttpException
import java.io.IOException

enum class ManSanPham { HOME, CATEGORIES, SEARCH, LIST, DETAIL, FORM, PREVIEW, SUCCESS, MINE, MANAGE, SOLD_CONFIRM, EXCHANGE, INBOX, REVIEW }
data class AnhChon(val uri: String, val bytes: ByteArray)
data class FormTin(
    val id: String? = null, val phienBan: String? = null, val ten: String = "", val danhMucId: String = "",
    val gia: String = "", val tinhTrang: String = "NhuMoi", val moTa: String = "", val tinhThanh: String = "", val diaDiem: String = "",
    val anh: List<AnhTin> = emptyList(), val anhChon: List<AnhChon> = emptyList(), val errors: Map<String, String> = emptyMap()
)
data class BanHangState(
    val screen: ManSanPham = ManSanPham.HOME, val busy: Boolean = false, val error: String? = null, val notice: String? = null,
    val home: HomeSanPham? = null, val categories: List<DanhMucBanHang> = emptyList(), val filter: BoLocSanPham = BoLocSanPham(),
    val results: TrangTin? = null, val detail: TinDang? = null, val form: FormTin = FormTin(),
    val mine: TinCuaToi? = null, val mineStatus: String? = null, val inbox: List<HopThuItem>? = null,
    val review: TrangTin? = null, val expired: Boolean = false, val progress: String? = null
)

class BanHangViewModel(private val repo: TinDangRepository) : ViewModel() {
    private val state = MutableStateFlow(BanHangState())
    val uiState = state.asStateFlow()
    private var userId: String? = null
    private var job: Job? = null
    private var detailId: String? = null
    private var detailOwn = false
    private val history = ArrayDeque<ManSanPham>()

    fun phienMoi(id: String?) {
        if (id == userId) return
        job?.cancel(); userId = id; detailId = null; history.clear(); state.value = BanHangState()
    }
    private fun screen(target: ManSanPham, push: Boolean = true) {
        if (push && target != state.value.screen) history.addLast(state.value.screen)
        state.update { it.copy(screen = target, error = null, notice = null) }
    }
    fun back() {
        if (state.value.busy) return
        val target = history.removeLastOrNull() ?: ManSanPham.HOME
        state.update { it.copy(screen = target, error = null, notice = null) }
        if (target == ManSanPham.MINE) taiTinToi()
    }
    fun loi(message: String) { state.update { it.copy(error = message) } }
    fun thongBao(message: String) { state.update { it.copy(notice = message) } }
    private fun work(action: suspend () -> Unit) {
        if (state.value.busy) return
        job = viewModelScope.launch {
            state.update { it.copy(busy = true, error = null, notice = null) }
            try { action() }
            catch (ex: Exception) {
                if (ex is CancellationException) throw ex
                val message = when (ex) {
                    is IOException -> "Chưa kết nối được ReMarket. Vui lòng kiểm tra mạng và thử lại."
                    is HttpException -> {
                        val detail = try { JsonParser.parseString(ex.response()?.errorBody()?.string() ?: "{}").asJsonObject.get("detail")?.asString } catch (_: Exception) { null }
                        if (ex.code() == 401) state.update { it.copy(expired = true) }
                        detail ?: when (ex.code()) { 404 -> "Tin không còn hiển thị hoặc đã bị xóa."; 403 -> "Bạn không có quyền thực hiện thao tác này."; 429 -> "Bạn thao tác quá nhanh. Vui lòng chờ rồi thử lại."; else -> "Không thực hiện được yêu cầu. Vui lòng thử lại." }
                    }
                    else -> "Chưa đọc được dữ liệu. Vui lòng thử lại."
                }
                state.update { it.copy(error = message) }
            } finally { state.update { it.copy(busy = false, progress = null) } }
        }
    }
    fun home() {
        if (state.value.busy) return
        history.clear(); screen(ManSanPham.HOME, false)
        work { val data = repo.home(); state.update { it.copy(home = data, categories = data.danhMuc) } }
    }
    fun categories() {
        if (state.value.busy) return
        screen(ManSanPham.CATEGORIES)
        work { val data = repo.danhMuc(); state.update { it.copy(categories = data) } }
    }
    fun search(category: String? = null, exchange: Boolean = false) {
        if (state.value.busy) return
        if (exchange) history.clear()
        screen(if (exchange) ManSanPham.EXCHANGE else if (category == null) ManSanPham.SEARCH else ManSanPham.LIST)
        state.update { it.copy(filter = BoLocSanPham(danhMucId = category, loaiTin = if (exchange) "TraoDoi" else "MuaBan"), results = null) }
        work { if (state.value.categories.isEmpty()) { val data = repo.danhMuc(); state.update { it.copy(categories = data) } }; timTrang(1) }
    }
    fun tuKhoa(text: String) { state.update { it.copy(filter = it.filter.copy(tuKhoa = text)) } }
    fun tim(filter: BoLocSanPham = state.value.filter) {
        if (state.value.busy) return
        state.update { it.copy(filter = filter, results = null) }
        work { timTrang(1) }
    }
    private suspend fun timTrang(page: Int) { val data = repo.tim(state.value.filter, page); state.update { it.copy(results = data) } }
    fun trang(page: Int) = work { timTrang(page) }
    fun chiTiet(id: String, own: Boolean = false) {
        if (state.value.busy) return
        screen(if (own) ManSanPham.MANAGE else ManSanPham.DETAIL)
        detailId = id; detailOwn = own
        state.update { it.copy(detail = null) }
        work { val data = repo.chiTiet(id, own); state.update { it.copy(detail = data) } }
    }
    fun taiChiTiet() { val id = detailId ?: state.value.detail?.id ?: return; work { val data = repo.chiTiet(id, detailOwn); state.update { it.copy(detail = data) } } }
    fun tinToi() {
        if (state.value.busy) return
        screen(ManSanPham.MINE); taiTinToi()
    }
    fun taiTinToi(status: String? = state.value.mineStatus, page: Int = 1) {
        if (state.value.busy) return
        state.update { it.copy(mineStatus = status) }
        work { val data = repo.cuaToi(status, page); state.update { it.copy(mine = data) } }
    }
    fun taoTin(city: String = "", address: String = "") {
        if (state.value.busy) return
        screen(ManSanPham.FORM)
        state.update { it.copy(form = FormTin(tinhThanh = city, diaDiem = address), detail = null) }
        if (state.value.categories.isEmpty()) work { val data = repo.danhMuc(); state.update { it.copy(categories = data) } }
    }
    fun suaTin() {
        val tin = state.value.detail ?: return
        if (state.value.busy) return
        screen(ManSanPham.FORM)
        state.update { it.copy(form = FormTin(tin.id, tin.phienBan, tin.ten, tin.danhMucId, (tin.giaBan ?: 0).toString(), tin.tinhTrang,
            tin.moTa, tin.tinhThanh, tin.diaDiem, tin.anh)) }
        if (state.value.categories.isEmpty()) work { val data = repo.danhMuc(); state.update { it.copy(categories = data) } }
    }
    fun form(change: (FormTin) -> FormTin) { if (!state.value.busy) state.update { it.copy(form = change(it.form).copy(errors = emptyMap()), error = null) } }
    fun themAnh(photos: List<AnhChon>) {
        if (state.value.form.anh.size + state.value.form.anhChon.size + photos.size > 8) return loi("Tối đa 8 ảnh sản phẩm.")
        if (photos.any { it.bytes.isEmpty() || it.bytes.size > 5 * 1024 * 1024 }) return loi("Mỗi ảnh JPG/PNG tối đa 5 MB.")
        form { it.copy(anhChon = (it.anhChon + photos).distinctBy { p -> p.uri }) }
    }
    fun boAnhChon(uri: String) = form { it.copy(anhChon = it.anhChon.filterNot { p -> p.uri == uri }) }
    fun anhBiaChon(uri: String) = form { f -> f.copy(anhChon = f.anhChon.sortedBy { if (it.uri == uri) 0 else 1 }) }
    private fun dongBoTin(tin: TinDang) { detailId = tin.id; detailOwn = true; state.update { it.copy(detail = tin, form = it.form.copy(id = tin.id, phienBan = tin.phienBan, anh = tin.anh)) } }
    fun xoaAnh(photoId: String) {
        val f = state.value.form
        if (f.id == null || f.phienBan == null) return
        work { dongBoTin(repo.xoaAnh(f.id, photoId, f.phienBan)) }
    }
    fun anhBia(photoId: String) {
        val f = state.value.form
        if (f.id == null || f.phienBan == null) return
        work { dongBoTin(repo.anhBia(f.id, photoId, f.phienBan)) }
    }
    fun luu(preview: Boolean) {
        val f = state.value.form
        val errors = buildMap {
            if (f.ten.trim().length !in 3..200) put("ten", "Tên sản phẩm từ 3 đến 200 ký tự.")
            if (f.danhMucId.isBlank()) put("danhMuc", "Vui lòng chọn danh mục.")
            if ((f.gia.toLongOrNull() ?: 0) !in 1L..1_000_000_000_000L) put("gia", "Giá phải từ 1 đến 1.000.000.000.000 đồng.")
            if (f.moTa.trim().length !in 10..4000) put("moTa", "Mô tả từ 10 đến 4000 ký tự.")
            if (f.tinhThanh.trim().length !in 2..100) put("tinhThanh", "Nhập tỉnh/thành phố.")
            if (f.diaDiem.trim().length !in 5..500) put("diaDiem", "Địa điểm từ 5 đến 500 ký tự.")
            if (preview && f.anh.isEmpty() && f.anhChon.isEmpty()) put("anh", "Cần ít nhất một ảnh sản phẩm.")
        }
        if (errors.isNotEmpty()) { state.update { it.copy(form = f.copy(errors = errors), error = "Vui lòng kiểm tra các trường được đánh dấu.") }; return }
        work {
            val body = LuuTinBody(f.ten.trim(), f.danhMucId, f.gia.toLong(), f.tinhTrang, f.moTa.trim(), f.tinhThanh.trim(), f.diaDiem.trim(), f.phienBan)
            var tin = repo.luu(f.id, body); dongBoTin(tin)
            val photos = f.anhChon.toList()
            photos.forEachIndexed { index, photo ->
                state.update { it.copy(progress = "Đang tải ảnh ${index + 1}/${photos.size}") }
                tin = repo.themAnh(tin.id, tin.phienBan, photo.bytes)
                dongBoTin(tin)
                // Preserve unsent files if any later upload fails, so retry cannot duplicate successful uploads.
                state.update { it.copy(form = it.form.copy(anhChon = it.form.anhChon.filterNot { p -> p.uri == photo.uri })) }
            }
            if (preview) screen(ManSanPham.PREVIEW) else {
                screen(ManSanPham.MANAGE); state.update { it.copy(notice = "Đã lưu tin. Thay đổi tin đang bán cần được duyệt lại.") }
            }
        }
    }
    fun suaPreview() { if (!state.value.busy) screen(ManSanPham.FORM) }
    fun guiDuyet() {
        val tin = state.value.detail ?: return
        work { val saved = if (tin.trangThai == "ChoDuyet") tin else repo.chuyen(tin.id, tin.phienBan, "gui-duyet"); state.update { it.copy(detail = saved) }; history.clear(); screen(ManSanPham.SUCCESS, false) }
    }
    fun anTin() = chuyen("an")
    fun hienLai() = chuyen("gui-duyet")
    private fun chuyen(action: String) {
        val tin = state.value.detail ?: return
        work { val saved = repo.chuyen(tin.id, tin.phienBan, action); state.update { it.copy(detail = saved, notice = "Đã cập nhật trạng thái tin.") } }
    }
    fun xacNhanBan() { if (!state.value.busy) screen(ManSanPham.SOLD_CONFIRM) }
    fun daBan() {
        val tin = state.value.detail ?: return
        work { val saved = repo.chuyen(tin.id, tin.phienBan, "da-ban"); state.update { it.copy(detail = saved, notice = "Tin đã chuyển sang Đã bán.") }; screen(ManSanPham.MANAGE) }
    }
    fun xoaTin() {
        val tin = state.value.detail ?: return
        work { repo.xoa(tin.id, tin.phienBan); history.clear(); screen(ManSanPham.MINE, false); val data = repo.cuaToi(null, 1); state.update { it.copy(detail = null, mine = data, mineStatus = null, notice = "Đã xóa tin.") } }
    }
    fun hopThu() {
        if (state.value.busy) return
        history.clear(); screen(ManSanPham.INBOX, false)
        work { val data = repo.hopThu(); state.update { it.copy(inbox = data) } }
    }
    fun choDuyet(page: Int = 1) {
        if (state.value.busy) return
        screen(ManSanPham.REVIEW)
        work { val data = repo.choDuyet(page); state.update { it.copy(review = data) } }
    }
    fun duyet(tin: TinDang, accept: Boolean, reason: String?) = work {
        repo.duyet(tin.id, tin.phienBan, accept, reason)
        val data = repo.choDuyet(1); state.update { it.copy(review = data, notice = if (accept) "Đã duyệt tin." else "Đã từ chối tin.") }
    }
    fun thuLai() {
        when (state.value.screen) {
            ManSanPham.HOME -> home()
            ManSanPham.CATEGORIES -> categories()
            ManSanPham.SEARCH, ManSanPham.LIST, ManSanPham.EXCHANGE -> tim()
            ManSanPham.MINE -> taiTinToi()
            ManSanPham.INBOX -> hopThu()
            ManSanPham.REVIEW -> choDuyet()
            ManSanPham.DETAIL, ManSanPham.MANAGE -> taiChiTiet()
            else -> work { val data = repo.danhMuc(); state.update { it.copy(categories = data) } }
        }
    }
    companion object {
        fun factory(repo: TinDangRepository): ViewModelProvider.Factory = viewModelFactory { initializer { BanHangViewModel(repo) } }
    }
}
