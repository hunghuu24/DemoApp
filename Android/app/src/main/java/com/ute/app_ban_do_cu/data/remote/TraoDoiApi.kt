package com.ute.app_ban_do_cu.data.remote
import com.ute.app_ban_do_cu.domain.model.*
import okhttp3.MultipartBody
import okhttp3.RequestBody
import retrofit2.http.*

interface TraoDoiApi {
    @GET("api/tindang/cuatoi") suspend fun cuaToi(@Query("loaiTin")type:String="TraoDoi",@Query("trangThai")status:String?,@Query("trang")page:Int):TinCuaToi
    @GET("api/traodoi/moncuatoi") suspend fun mon(@Query("tuKhoa")query:String,@Query("trang")page:Int):TrangTin
    @GET("api/traodoi") suspend fun list(@Query("tab")tab:String,@Query("trang")page:Int):TrangDeNghi
    @GET("api/traodoi/{id}") suspend fun doc(@Path("id")id:String):DeNghiTraoDoi
    @POST("api/traodoi") suspend fun tao(@Body body:TaoDeNghiBody):DeNghiTraoDoi
    @POST("api/traodoi/{id}/phanhoi") suspend fun phanHoi(@Path("id")id:String,@Body body:PhanHoiDeNghiBody):DeNghiTraoDoi
    @POST("api/traodoi/{id}/dexuatlai") suspend fun doi(@Path("id")id:String,@Body body:DoiDeNghiBody):DeNghiTraoDoi
    @POST("api/traodoi/{id}/huy") suspend fun huy(@Path("id")id:String,@Body body:HuyDeNghiBody):DeNghiTraoDoi
    @POST("api/traodoi/{id}/giaonhan") suspend fun nhan(@Path("id")id:String,@Body body:GiaoNhanTraoDoiBody):DeNghiTraoDoi
    @POST("api/traodoi/{id}/hoithoai") suspend fun chat(@Path("id")id:String):HoiThoai
    @Multipart @POST("api/traodoi/{id}/bangchung") suspend fun evidence(@Path("id")id:String,@Part("maYeuCau")key:RequestBody,@Part photo:MultipartBody.Part):DeNghiTraoDoi
    @GET("api/traodoi/tieuchi") suspend fun tieuChi():List<TieuChiTraoDoi>
    @POST("api/traodoi/{id}/danhgia") suspend fun danhGia(@Path("id")id:String,@Body body:DanhGiaTraoDoiBody):DeNghiTraoDoi
}
