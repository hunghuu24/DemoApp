package com.ute.app_ban_do_cu.data.remote

import com.ute.app_ban_do_cu.domain.model.*
import okhttp3.MultipartBody
import okhttp3.RequestBody
import retrofit2.http.*

interface TinDangApi {
    @GET("api/tindang/trangchu") suspend fun home(): HomeSanPham
    @GET("api/tindang/danhmuc") suspend fun danhMuc(): List<DanhMucBanHang>
    @GET("api/tindang") suspend fun tim(@QueryMap filter: Map<String, String>): TrangTin
    @GET("api/tindang/{id}") suspend fun chiTiet(@Path("id") id: String): TinDang
    @GET("api/tindang/{id}/quanly") suspend fun quanLy(@Path("id") id: String): TinDang
    @GET("api/tindang/cuatoi") suspend fun cuaToi(@Query("trangThai") status: String?, @Query("trang") page: Int): TinCuaToi
    @POST("api/tindang") suspend fun tao(@Body body: LuuTinBody): TinDang
    @PUT("api/tindang/{id}") suspend fun sua(@Path("id") id: String, @Body body: LuuTinBody): TinDang
    @Multipart @POST("api/tindang/{id}/anh") suspend fun themAnh(@Path("id") id: String, @Part("phienBan") version: RequestBody, @Part anh: MultipartBody.Part): TinDang
    @HTTP(method = "DELETE", path = "api/tindang/{id}/anh/{photo}", hasBody = true)
    suspend fun xoaAnh(@Path("id") id: String, @Path("photo") photo: String, @Body version: PhienBanTinBody): TinDang
    @PUT("api/tindang/{id}/anh/{photo}/bia") suspend fun anhBia(@Path("id") id: String, @Path("photo") photo: String, @Body version: PhienBanTinBody): TinDang
    @POST("api/tindang/{id}/{action}") suspend fun chuyen(@Path("id") id: String, @Path("action") action: String, @Body version: PhienBanTinBody): TinDang
    @HTTP(method = "DELETE", path = "api/tindang/{id}", hasBody = true)
    suspend fun xoa(@Path("id") id: String, @Body version: PhienBanTinBody)
    @GET("api/hopthu") suspend fun hopThu(): List<HopThuItem>
    @GET("api/admin/tindang") suspend fun choDuyet(@Query("trang") page: Int): TrangTin
    @POST("api/admin/tindang/{id}/duyet") suspend fun duyet(@Path("id") id: String, @Body form: DuyetTinBody): TinDang
}
