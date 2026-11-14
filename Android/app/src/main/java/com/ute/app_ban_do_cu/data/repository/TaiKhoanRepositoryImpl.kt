package com.ute.app_ban_do_cu.data.repository

import com.ute.app_ban_do_cu.data.local.PhienDataStore
import com.ute.app_ban_do_cu.data.remote.*
import com.ute.app_ban_do_cu.domain.model.*
import com.ute.app_ban_do_cu.domain.repository.TaiKhoanRepository
import okhttp3.MultipartBody
import okhttp3.RequestBody.Companion.toRequestBody
import retrofit2.HttpException

class TaiKhoanRepositoryImpl(private val publicApi: TaiKhoanApi, private val api: TaiKhoanApi,
    private val store: PhienDataStore) : TaiKhoanRepository {
    override fun conPhien() = store.hienTai.get() != null
    override suspend fun khoiPhuc(): HoSo? {
        if (store.doc() == null) return null
        return try { api.toi() } catch (ex: HttpException) {
            if (ex.code() == 401 && !conPhien()) null else throw ex
        }
    }
    override suspend fun dangNhap(body: DangNhapBody): HoSo {
        val response = publicApi.dangNhap(body)
        store.luu(response)
        return response.hoSo
    }
    override suspend fun dangKy(body: DangKyBody) = publicApi.dangKy(body)
    override suspend fun quen(body: QuenBody) = publicApi.quenMatKhau(body).thongBao
    override suspend fun datLai(body: DatLaiBody) { publicApi.datLaiMatKhau(body); store.xoa() }
    override suspend fun doiMatKhau(body: DoiMatKhauBody) { api.doiMatKhau(body); store.xoa() }
    override suspend fun toi() = api.toi()
    override suspend fun capNhat(body: HoSoBody) = api.capNhat(body)
    override suspend fun doiAnh(bytes: ByteArray) = api.doiAnh(MultipartBody.Part.createFormData("anh", "avatar", bytes.toRequestBody()))
    override suspend fun dangXuat() {
        // Keep the session if network fails: never claim server logout without revocation.
        try { api.dangXuat() } catch (ex: HttpException) {
            if (ex.code() != 401 || conPhien()) throw ex
        }
        store.xoa()
    }
    override suspend fun trangChu() = api.trangChu()
    override suspend fun tongQuan() = api.tongQuan()
}
