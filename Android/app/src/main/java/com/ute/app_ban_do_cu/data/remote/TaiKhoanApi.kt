package com.ute.app_ban_do_cu.data.remote

import okhttp3.MultipartBody
import retrofit2.http.*
import com.ute.app_ban_do_cu.domain.model.*

interface TaiKhoanApi {
    @POST("api/taikhoan/dangky") suspend fun dangKy(@Body body: DangKyBody): HoSo
    @POST("api/taikhoan/dangnhap") suspend fun dangNhap(@Body body: DangNhapBody): PhienDto
    @POST("api/taikhoan/lam-moi") suspend fun lamMoi(@Body body: RefreshBody): PhienDto
    @POST("api/taikhoan/quenmatkhau") suspend fun quenMatKhau(@Body body: QuenBody): ThongBao
    @POST("api/taikhoan/datlaimatkhau") suspend fun datLaiMatKhau(@Body body: DatLaiBody)
    @POST("api/taikhoan/doimatkhau") suspend fun doiMatKhau(@Body body: DoiMatKhauBody)
    @POST("api/taikhoan/dangxuat") suspend fun dangXuat()
    @GET("api/taikhoan/toi") suspend fun toi(): HoSo
    @PUT("api/taikhoan/toi") suspend fun capNhat(@Body body: HoSoBody): HoSo
    @Multipart @POST("api/taikhoan/anh-dai-dien") suspend fun doiAnh(@Part anh: MultipartBody.Part): HoSo
    @GET("api/trangchu") suspend fun trangChu(): TrangChuDto
    @GET("api/admin/tongquan") suspend fun tongQuan(): TongQuanDto
}
