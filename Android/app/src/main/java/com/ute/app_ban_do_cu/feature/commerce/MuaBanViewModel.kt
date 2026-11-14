package com.ute.app_ban_do_cu.feature.commerce

import androidx.lifecycle.*
import androidx.lifecycle.viewmodel.initializer
import androidx.lifecycle.viewmodel.viewModelFactory
import com.google.gson.JsonParser
import com.ute.app_ban_do_cu.domain.model.*
import com.ute.app_ban_do_cu.domain.repository.MuaBanRepository
import kotlinx.coroutines.*
import kotlinx.coroutines.flow.*
import retrofit2.HttpException
import java.io.IOException
import java.util.UUID

enum class ManMuaBan { NONE, HOP_THU, CHAT, PHIEU, CHECKOUT, DON_HANG, CHI_TIET_DON, DANH_GIA, DANH_GIA_XONG, HO_TRO, TAO_HO_TRO, CHI_TIET_HO_TRO }
data class MuaBanState(
    val man: ManMuaBan = ManMuaBan.NONE, val busy: Boolean = false, val error: String? = null, val notice: String? = null,
    val expired: Boolean = false, val inbox: List<HopThuMua>? = null, val unread: Boolean = false, val inboxPage: Int = 1,
    val chat: HoiThoai? = null, val tinNhan: String = "", val giamGia: String = "", val quote: BaoGia? = null,
    val ten: String = "", val phone: String = "", val diaChi: String = "", val formErrors: Map<String,String> = emptyMap(),
    val orders: TrangDonMua? = null, val orderRole: String = "mua", val orderStatus: String? = null, val order: DonMua? = null,
    val saoSanPham: Int = 0, val saoNguoiBan: Int = 0, val nhanXet: String = "", val supports: List<HoTroMua>? = null,
    val supportPage: Int = 1, val adminSupport: Boolean = false, val support: HoTroMua? = null, val supportOrderId: String? = null,
    val supportOrderCode: String? = null, val supportKind: String = "DonHangVanChuyen", val supportTitle: String = "", val supportText: String = "",
    val reply: String = "", val selectingSupportOrder: Boolean = false, val pollError: Boolean = false
)

class MuaBanViewModel(private val repo: MuaBanRepository) : ViewModel() {
    private val state = MutableStateFlow(MuaBanState())
    val uiState = state.asStateFlow()
    private val history = ArrayDeque<ManMuaBan>()
    private var user: String? = null
    private var epoch = 0
    private var job: Job? = null
    private var refresh: Job? = null
    private var selectedChat: String? = null
    private var openChat: (suspend () -> HoiThoai)? = null
    private var selectedOrder: String? = null
    private var selectedSupport: String? = null
    private var quoteTin: String? = null
    private var messageKey = key()
    private var couponKey = key()
    private var orderKey = key()
    private var ratingKey = key()
    private var supportKey = key()
    private var payAttempt: Pair<Boolean,String>? = null
    private fun key() = UUID.randomUUID().toString()
    fun phienMoi(id: String?) {
        if (id == user) return
        epoch++; job?.cancel(); refresh?.cancel(); user = id; history.clear(); state.value = MuaBanState()
        selectedChat = null; openChat = null; selectedOrder = null; selectedSupport = null; quoteTin = null
    }
    private fun navigate(man: ManMuaBan) {
        if (state.value.man != man) history.addLast(state.value.man)
        state.update { it.copy(man = man, error = null, notice = null, pollError = false) }
    }
    fun dong() { if (!state.value.busy) { refresh?.cancel(); history.clear(); state.update { it.copy(man = ManMuaBan.NONE, error = null, notice = null) } } }
    fun back() {
        if (state.value.busy) return
        refresh?.cancel()
        val target = history.removeLastOrNull() ?: ManMuaBan.NONE
        state.update { it.copy(man = target, error = null, notice = null) }
        if (target in listOf(ManMuaBan.HOP_THU, ManMuaBan.DON_HANG, ManMuaBan.HO_TRO)) taiLai()
    }
    fun loi(text: String) { state.update { it.copy(error = text) } }
    private fun error(ex: Exception): String = when(ex) {
        is IOException -> "Chưa kết nối được ReMarket. Kiểm tra mạng rồi thử lại."
        is HttpException -> {
            if (ex.code() == 401) state.update { it.copy(expired = true) }
            try { JsonParser.parseString(ex.response()?.errorBody()?.string() ?: "{}").asJsonObject.get("detail")?.asString }
            catch (_: Exception) { null }
                ?: when(ex.code()) { 404 -> "Không tìm thấy dữ liệu của bạn hoặc món đồ không còn bán."; 403 -> "Bạn không có quyền thực hiện thao tác này."; 409 -> "Dữ liệu vừa thay đổi. Hãy tải lại."; 429 -> "Bạn thao tác quá nhanh. Hãy chờ và thử lại."; else -> "Chưa thực hiện được yêu cầu." }
        }
        else -> "Không đọc được dữ liệu từ máy chủ. Vui lòng thử lại."
    }
    private fun work(action: suspend () -> Unit) {
        if (state.value.busy) return
        val session = epoch
        job = viewModelScope.launch {
            state.update { it.copy(busy = true, error = null, notice = null) }
            try { action() }
            catch (ex: Exception) { if (ex is CancellationException) throw ex; if (epoch == session) { val message = error(ex); state.update { it.copy(error = message) } } }
            finally { if (epoch == session) state.update { it.copy(busy = false) } }
        }
    }
    private fun chatData(value: HoiThoai) {
        selectedChat = value.id
        state.update { s ->
            val same = s.chat?.id == value.id
            val messages = ((if (same) s.chat!!.tinNhan else emptyList()) + value.tinNhan).associateBy { it.id }.values.sortedBy { it.soThuTu }
            val coupons = ((if (same) s.chat!!.phieu else emptyList()) + value.phieu).associateBy { it.id }.values.toList()
            s.copy(chat = value.copy(tinNhan = messages, phieu = coupons), pollError = false)
        }
    }
    fun moChat(tin: String) {
        if (state.value.busy) return
        selectedChat = null; openChat = { repo.moChat(tin) }
        navigate(ManMuaBan.CHAT); state.update { it.copy(chat = null, tinNhan = "") }
        work { chatData(repo.moChat(tin)) }
    }
    fun chatDon() {
        val order = state.value.order ?: return
        if (state.value.busy) return
        selectedChat = null; openChat = { repo.moChatDon(order.id) }
        navigate(ManMuaBan.CHAT); state.update { it.copy(chat = null, tinNhan = "") }
        work { chatData(repo.moChatDon(order.id)) }
    }
    fun hopThu(unread: Boolean = false, page: Int = 1) {
        if (state.value.busy) return
        navigate(ManMuaBan.HOP_THU); state.update { it.copy(unread = unread, inboxPage = page) }
        work { val data = repo.hopThu(unread, page); state.update { it.copy(inbox = data) } }
    }
    fun docChat(id: String) {
        if (state.value.busy) return
        selectedChat = id; openChat = null; navigate(ManMuaBan.CHAT); state.update { it.copy(chat = null, tinNhan = "") }
        work { chatData(repo.chat(id)) }
    }
    fun tinCu() { val chat = state.value.chat ?: return; work { chatData(repo.chat(chat.id, chat.tinNhan.firstOrNull()?.soThuTu)) } }
    fun soanTin(text: String) { if (!state.value.busy) { messageKey = key(); state.update { it.copy(tinNhan = text.take(4000)) } } }
    fun guiTin() {
        val s = state.value; val id = s.chat?.id ?: return
        if (s.tinNhan.isBlank()) return loi("Vui lòng nhập nội dung tin nhắn.")
        work { chatData(repo.guiTin(id, GuiTinBody(messageKey, s.tinNhan.trim()))); messageKey = key(); state.update { it.copy(tinNhan = "") } }
    }
    fun guiAnh(bytes: ByteArray) {
        val id = state.value.chat?.id ?: return
        if (bytes.isEmpty() || bytes.size > 5*1024*1024) return loi("Chọn ảnh JPG/PNG tối đa 5 MB.")
        val messageId = key(); work { chatData(repo.guiAnh(id, messageId, bytes)) }
    }
    fun moPhieu() { if (!state.value.busy) { couponKey = key(); state.update { it.copy(giamGia = "") }; navigate(ManMuaBan.PHIEU) } }
    fun soTien(text: String) { couponKey = key(); state.update { it.copy(giamGia = text.filter(Char::isDigit).take(13)) } }
    fun taoPhieu() {
        val s = state.value; val chat = s.chat ?: return; val price = chat.sanPham?.giaNiemYet ?: return
        val amount = s.giamGia.toLongOrNull() ?: 0
        if (amount <= 0 || amount >= price) return loi("Số tiền giảm phải lớn hơn 0 và nhỏ hơn giá món đồ.")
        work { chatData(repo.taoPhieu(chat.id, TaoPhieuBody(couponKey, amount))); state.update { it.copy(man = ManMuaBan.CHAT) }; if (history.lastOrNull() == ManMuaBan.CHAT) history.removeLast() }
    }
    fun traLoi(phieu: PhieuGiam, accept: Boolean) {
        val id = state.value.chat?.id ?: return
        work { chatData(repo.traLoiPhieu(id, phieu.id, TraLoiPhieuBody(phieu.phienBan, accept))) }
    }
    fun checkout(tinId: String, profile: HoSo, coupon: String? = null) {
        if (state.value.busy) return
        quoteTin = tinId; orderKey = key(); navigate(ManMuaBan.CHECKOUT)
        state.update { it.copy(quote = null, ten = profile.hoTen, phone = profile.soDienThoai.orEmpty(), diaChi = profile.diaChi.orEmpty(), formErrors = emptyMap()) }
        work { val data = repo.baoGia(BaoGiaBody(tinId, coupon)); state.update { it.copy(quote = data) } }
    }
    fun apDungPhieu(id: String?) { val tin = quoteTin ?: return; work { val q = repo.baoGia(BaoGiaBody(tin,id)); orderKey = key(); state.update { it.copy(quote = q) } } }
    fun giaoHang(name: String = state.value.ten, phone: String = state.value.phone, address: String = state.value.diaChi) {
        orderKey = key(); state.update { it.copy(ten = name.take(120), phone = phone.take(20), diaChi = address.take(500), formErrors = emptyMap()) }
    }
    fun datHang() {
        val s = state.value; val q = s.quote ?: return
        val errors = buildMap {
            if (s.ten.trim().length < 2) put("ten", "Nhập tên người nhận.")
            if (!Regex("(?:0[0-9]{9}|\\+84[0-9]{9})").matches(s.phone.replace(" ", ""))) put("phone", "Nhập số điện thoại Việt Nam hợp lệ.")
            if (s.diaChi.trim().length < 10) put("diaChi", "Địa chỉ cần ít nhất 10 ký tự.")
        }
        if (errors.isNotEmpty()) { state.update { it.copy(formErrors = errors) }; return loi("Kiểm tra thông tin giao hàng.") }
        work {
            val order = repo.taoDon(TaoDonBody(orderKey,q.sanPham.tinDangId,q.phienBanTin,q.phieuThuongLuongId,s.ten.trim(),s.phone.trim(),s.diaChi.trim()))
            selectedOrder = order.id; payAttempt = null; history.clear(); history.addLast(ManMuaBan.NONE)
            state.update { it.copy(order = order, man = ManMuaBan.CHI_TIET_DON, notice = "Đã tạo đơn. Bạn có thể thanh toán mô phỏng trong 15 phút.") }
        }
    }
    fun donHang(role: String = "mua", status: String? = null, page: Int = 1, selectSupport: Boolean = false) {
        if (state.value.busy) return
        navigate(ManMuaBan.DON_HANG); state.update { it.copy(orderRole = role, orderStatus = status, selectingSupportOrder = selectSupport) }
        work { val data = repo.donHang(role,status,page); state.update { it.copy(orders = data) } }
    }
    fun docDon(id: String) {
        if (state.value.busy) return
        selectedOrder = id; payAttempt = null
        if (state.value.selectingSupportOrder) {
            work { val data = repo.don(id); state.update { it.copy(supportOrderId = data.id, supportOrderCode = data.ma, man = ManMuaBan.TAO_HO_TRO, selectingSupportOrder = false) }; if (history.lastOrNull() == ManMuaBan.TAO_HO_TRO) history.removeLast() }
        } else { navigate(ManMuaBan.CHI_TIET_DON); state.update { it.copy(order = null) }; work { val data = repo.don(id); state.update { it.copy(order = data) } } }
    }
    fun thanhToan(ok: Boolean) {
        val order = state.value.order ?: return
        val attempt = payAttempt?.takeIf { it.first == ok } ?: (ok to key()).also { payAttempt = it }
        work { val data = repo.thanhToan(order.id, ThanhToanBody(attempt.second,order.phienBan,ok)); payAttempt = null
            state.update { it.copy(order = data, notice = if (ok) "Thanh toán mô phỏng thành công. Không trừ tiền thật." else "Thanh toán mô phỏng thất bại; bạn có thể thử lại hoặc hủy đơn.") } }
    }
    fun huyDon(reason: String) { val o = state.value.order ?: return; if (reason.trim().length < 5) return loi("Lý do hủy cần ít nhất 5 ký tự."); work { val data = repo.huy(o.id,HuyDonBody(o.phienBan,reason.trim())); state.update { it.copy(order = data, notice = "Đã hủy đơn và giải phóng món đồ.") } } }
    fun chuyenShip(status: String) { val o = state.value.order ?: return; work { val data = repo.ship(o.id,ShipBody(o.phienBan,status)); state.update { it.copy(order = data) } } }
    fun nhanHang() { val o = state.value.order ?: return; work { val data = repo.nhan(o.id,o.phienBan); state.update { it.copy(order = data, notice = "Đã hoàn tất đơn hàng.") } } }
    fun moDanhGia() { if (!state.value.busy) { ratingKey = key(); state.update { it.copy(saoSanPham = 0, saoNguoiBan = 0, nhanXet = "") }; navigate(ManMuaBan.DANH_GIA) } }
    fun danhGiaForm(product: Int = state.value.saoSanPham, seller: Int = state.value.saoNguoiBan, text: String = state.value.nhanXet) {
        ratingKey = key(); state.update { it.copy(saoSanPham = product, saoNguoiBan = seller, nhanXet = text.take(2000)) }
    }
    fun guiDanhGia() {
        val s = state.value; val o = s.order ?: return
        if (s.saoSanPham !in 1..5 || s.saoNguoiBan !in 1..5 || s.nhanXet.trim().length < 5) return loi("Chọn số sao sản phẩm, người bán và nhận xét ít nhất 5 ký tự.")
        work { repo.danhGia(o.id,DanhGiaBody(ratingKey,s.saoNguoiBan,s.saoSanPham,s.nhanXet.trim())); val data = repo.don(o.id); state.update { it.copy(order = data, man = ManMuaBan.DANH_GIA_XONG) } }
    }
    fun veDon() { state.update { it.copy(man = ManMuaBan.CHI_TIET_DON) }; if (history.lastOrNull() == ManMuaBan.CHI_TIET_DON) history.removeLast() }
    fun hoTroList(page: Int = 1, admin: Boolean = false) {
        if (state.value.busy) return
        navigate(ManMuaBan.HO_TRO); state.update { it.copy(adminSupport = admin, supportPage = page) }
        work { val data = repo.hoTroList(page,admin); state.update { it.copy(supports = data) } }
    }
    fun moHoTro(order: DonMua? = null) {
        if (state.value.busy) return
        supportKey = key(); navigate(ManMuaBan.TAO_HO_TRO)
        state.update { it.copy(supportOrderId = order?.id, supportOrderCode = order?.ma, supportTitle = "", supportText = "", supportKind = "DonHangVanChuyen", adminSupport = false, formErrors = emptyMap()) }
    }
    fun supportForm(title: String = state.value.supportTitle, text: String = state.value.supportText, kind: String = state.value.supportKind) {
        supportKey = key(); state.update { it.copy(supportTitle = title.take(200), supportText = text.take(4000), supportKind = kind, formErrors = emptyMap()) }
    }
    fun boDonHoTro() { supportKey = key(); state.update { it.copy(supportOrderId = null, supportOrderCode = null) } }
    fun guiHoTro() {
        val s = state.value
        if (s.supportTitle.trim().length < 5 || s.supportText.trim().length < 10) return loi("Tiêu đề cần 5 ký tự, mô tả cần ít nhất 10 ký tự.")
        work { val data = repo.taoHoTro(HoTroBody(supportKey,s.supportOrderId,s.supportKind,s.supportTitle.trim(),s.supportText.trim()));
            selectedSupport = data.id; state.update { it.copy(support = data, man = ManMuaBan.CHI_TIET_HO_TRO, reply = "", notice = "Đã gửi yêu cầu hỗ trợ.") } }
    }
    fun docHoTro(id: String) { if (!state.value.busy) { selectedSupport = id; navigate(ManMuaBan.CHI_TIET_HO_TRO); state.update { it.copy(support = null, reply = "") }; work { val data = repo.hoTro(id,state.value.adminSupport); state.update { it.copy(support = data) } } } }
    fun traLoiText(text: String) { state.update { it.copy(reply = text.take(4000)) } }
    fun phanHoi(status: String? = null) {
        val s = state.value; val item = s.support ?: return
        if (s.reply.trim().length < 5) return loi("Nội dung cần ít nhất 5 ký tự.")
        work { val data = repo.phanHoi(item.id,PhanHoiHoTroBody(item.phienBan,s.reply.trim(),status),s.adminSupport); state.update { it.copy(support = data, reply = "") } }
    }
    fun taiLai() {
        when(state.value.man) {
            ManMuaBan.HOP_THU -> hopThu(state.value.unread,state.value.inboxPage)
            ManMuaBan.CHAT -> { val id = selectedChat; if (id != null) work { chatData(repo.chat(id)) } else openChat?.let { load -> work { chatData(load()) } } }
            ManMuaBan.CHECKOUT -> quoteTin?.let { id -> work { val data = repo.baoGia(BaoGiaBody(id,state.value.quote?.phieuThuongLuongId)); orderKey = key(); state.update { it.copy(quote = data) } } }
            ManMuaBan.DON_HANG -> donHang(state.value.orderRole,state.value.orderStatus,state.value.orders?.trang ?: 1,state.value.selectingSupportOrder)
            ManMuaBan.CHI_TIET_DON -> selectedOrder?.let { id -> work { val data = repo.don(id); state.update { it.copy(order = data) } } }
            ManMuaBan.HO_TRO -> hoTroList(state.value.supportPage,state.value.adminSupport)
            ManMuaBan.CHI_TIET_HO_TRO -> selectedSupport?.let { id -> work { val data = repo.hoTro(id,state.value.adminSupport); state.update { it.copy(support = data) } } }
            ManMuaBan.PHIEU -> taoPhieu()
            ManMuaBan.DANH_GIA -> guiDanhGia()
            ManMuaBan.TAO_HO_TRO -> guiHoTro()
            else -> Unit
        }
    }
    fun lamMoiAmTham() {
        val s = state.value; if (s.busy || refresh?.isActive == true) return
        val session = epoch
        refresh = viewModelScope.launch {
            try {
                when(s.man) {
                    ManMuaBan.CHAT -> s.chat?.id?.let { val data = repo.chat(it); if (epoch == session && state.value.man == s.man && state.value.chat?.id == it && !state.value.busy) chatData(data) }
                    ManMuaBan.CHI_TIET_DON -> s.order?.id?.let { val data = repo.don(it); if (epoch == session && state.value.man == s.man && state.value.order?.id == it && !state.value.busy) state.update { current -> current.copy(order = data, pollError = false) } }
                    ManMuaBan.HOP_THU -> { val data = repo.hopThu(s.unread,s.inboxPage); if (epoch == session && state.value.man == s.man && state.value.unread == s.unread && state.value.inboxPage == s.inboxPage && !state.value.busy) state.update { it.copy(inbox = data, pollError = false) } }
                    ManMuaBan.CHI_TIET_HO_TRO -> s.support?.id?.let { val data = repo.hoTro(it,s.adminSupport); if (epoch == session && state.value.man == s.man && state.value.support?.id == it && !state.value.busy) state.update { current -> current.copy(support = data, pollError = false) } }
                    else -> Unit
                }
            } catch (ex: Exception) { if (ex is CancellationException) throw ex; if (epoch == session && state.value.man == s.man) { if (ex is HttpException && ex.code() == 401) state.update { it.copy(expired = true) }; state.update { it.copy(pollError = true) } } }
        }
    }
    companion object { fun factory(repo: MuaBanRepository): ViewModelProvider.Factory = viewModelFactory { initializer { MuaBanViewModel(repo) } } }
}
