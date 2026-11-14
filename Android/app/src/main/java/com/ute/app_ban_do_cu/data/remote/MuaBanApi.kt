package com.ute.app_ban_do_cu.data.remote

import com.ute.app_ban_do_cu.domain.model.*
import okhttp3.MultipartBody
import okhttp3.RequestBody
import retrofit2.http.*

interface MuaBanApi {
    @POST("api/hoithoai") suspend fun moChat(@Body body: MoChatBody): HoiThoai
    @POST("api/donhang/{id}/hoithoai") suspend fun chatDon(@Path("id") id: String): HoiThoai
    @GET("api/hoithoai") suspend fun hopThu(@Query("chuaDoc") unread: Boolean, @Query("trang") page: Int): List<HopThuMua>
    @GET("api/hoithoai/{id}") suspend fun chat(@Path("id") id: String, @Query("truoc") before: Long?): HoiThoai
    @POST("api/hoithoai/{id}/tinnhan") suspend fun gui(@Path("id") id: String, @Body body: GuiTinBody): HoiThoai
    @Multipart @POST("api/hoithoai/{id}/anh") suspend fun anh(@Path("id") id: String, @Part("maPhiaClient") key: RequestBody, @Part image: MultipartBody.Part): HoiThoai
    @POST("api/hoithoai/{id}/phieu") suspend fun taoPhieu(@Path("id") id: String, @Body body: TaoPhieuBody): HoiThoai
    @POST("api/hoithoai/{id}/phieu/{coupon}") suspend fun traLoi(@Path("id") id: String, @Path("coupon") coupon: String, @Body body: TraLoiPhieuBody): HoiThoai
    @POST("api/donhang/baogia") suspend fun baoGia(@Body body: BaoGiaBody): BaoGia
    @POST("api/donhang") suspend fun taoDon(@Body body: TaoDonBody): DonMua
    @GET("api/donhang") suspend fun donList(@Query("vaiTro") role: String, @Query("trangThai") state: String?, @Query("trang") page: Int): TrangDonMua
    @GET("api/donhang/{id}") suspend fun don(@Path("id") id: String): DonMua
    @POST("api/donhang/{id}/thanhtoan") suspend fun thanhToan(@Path("id") id: String, @Body body: ThanhToanBody): DonMua
    @POST("api/donhang/{id}/huy") suspend fun huy(@Path("id") id: String, @Body body: HuyDonBody): DonMua
    @POST("api/donhang/{id}/vanchuyen") suspend fun ship(@Path("id") id: String, @Body body: ShipBody): DonMua
    @POST("api/donhang/{id}/nhanhang") suspend fun nhan(@Path("id") id: String, @Body body: PhienBanTinBody): DonMua
    @POST("api/donhang/{id}/danhgia") suspend fun danhGia(@Path("id") id: String, @Body body: DanhGiaBody): DanhGiaMua
    @POST("api/hotro") suspend fun taoHoTro(@Body body: HoTroBody): HoTroMua
    @GET("api/hotro") suspend fun supportList(@Query("trang") page: Int): List<HoTroMua>
    @GET("api/hotro/{id}") suspend fun support(@Path("id") id: String): HoTroMua
    @POST("api/hotro/{id}/phanhoi") suspend fun phanHoi(@Path("id") id: String, @Body body: PhanHoiHoTroBody): HoTroMua
    @GET("api/admin/hotro") suspend fun adminList(@Query("trang") page: Int): List<HoTroMua>
    @GET("api/admin/hotro/{id}") suspend fun adminSupport(@Path("id") id: String): HoTroMua
    @POST("api/admin/hotro/{id}/phanhoi") suspend fun adminPhanHoi(@Path("id") id: String, @Body body: PhanHoiHoTroBody): HoTroMua
}
