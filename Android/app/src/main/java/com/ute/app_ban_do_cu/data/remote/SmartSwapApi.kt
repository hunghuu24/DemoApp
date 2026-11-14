package com.ute.app_ban_do_cu.data.remote
import com.ute.app_ban_do_cu.domain.model.*
import com.ute.app_ban_do_cu.domain.repository.SmartSwapRepository
import retrofit2.http.*

interface SmartSwapApi:SmartSwapRepository {
    @GET("api/tindang/cuatoi?loaiTin=TraoDoi&trangThai=DangHienThi") override suspend fun mine(@Query("trang")page:Int):TinCuaToi
    @Multipart @POST("api/smart-swaps/{id}/evidence") suspend fun evidenceForm(@Path("id")id:String,@Part("maYeuCau")key:okhttp3.RequestBody,@Part photo:okhttp3.MultipartBody.Part):SsGiaoNhan
    @GET("api/smart-swaps/search") override suspend fun search(@Query("tinDangId")tin:String?,@Query("tuKhoaMuonNhan")wanted:String?,@Query("buToiDa")cap:Long?,@Query("tinhTrangToiThieu")condition:String?,@Query("tinhThanh")city:String?,@Query("chiThanhVienDaXacMinh")verified:Boolean,@Query("trang")page:Int):SsKetQua
    @POST("api/smart-swaps/preview") override suspend fun preview(@Body body:SsPreviewBody):SsPreview
    @POST("api/smart-swaps/invitations") override suspend fun invite(@Body body:SsMoiBody):SsVong
    @GET("api/smart-swaps") override suspend fun list(@Query("tab")tab:String,@Query("trang")page:Int):SsTrangVong
    @GET("api/smart-swaps/{id}") override suspend fun detail(@Path("id")id:String):SsVong
    @POST("api/smart-swaps/{id}/responses") override suspend fun respond(@Path("id")id:String,@Body body:SsPhanHoiBody):SsVong
    @POST("api/smart-swaps/{id}/proposals") override suspend fun propose(@Path("id")id:String,@Body body:SsDeXuatBody):SsVong
    @POST("api/smart-swaps/{id}/cancel") override suspend fun cancel(@Path("id")id:String,@Body body:SsHuyBody):SsVong
    @GET("api/smart-swaps/notifications") override suspend fun notifications(@Query("trang")page:Int):List<SsThongBao>
    @POST("api/smart-swaps/notifications/{id}/read") override suspend fun read(@Path("id")id:String)
    @GET("api/smart-swaps/{id}/chat") override suspend fun chat(@Path("id")id:String):HoiThoai
    @GET("api/smart-swaps/{id}/delivery") override suspend fun delivery(@Path("id")id:String):SsGiaoNhan
    @PUT("api/smart-swaps/{id}/delivery/contact") override suspend fun contact(@Path("id")id:String,@Body body:SsContactBody):SsGiaoNhan
    @POST("api/smart-swaps/{id}/shipments") override suspend fun book(@Path("id")id:String,@Body body:SsBookBody):SsGiaoNhan
    @PUT("api/smart-swaps/{id}/shipments/{ship}/tracking") override suspend fun tracking(@Path("id")id:String,@Path("ship")ship:String,@Body body:SsTrackingBody):SsGiaoNhan
    @POST("api/smart-swaps/{id}/shipments/{ship}/status") override suspend fun status(@Path("id")id:String,@Path("ship")ship:String,@Body body:SsStatusBody):SsGiaoNhan
    @POST("api/smart-swaps/{id}/shipments/{ship}/received") override suspend fun receive(@Path("id")id:String,@Path("ship")ship:String,@Body body:SsReceiveBody):SsGiaoNhan
    @POST("api/smart-swaps/{id}/issues") override suspend fun issue(@Path("id")id:String,@Body body:SsIssueBody):SsGiaoNhan
    @GET("api/smart-swaps/{id}/reviews") override suspend fun reviews(@Path("id")id:String):List<SsReview>
    @POST("api/smart-swaps/{id}/reviews") override suspend fun rate(@Path("id")id:String,@Body body:SsReviewBody):List<SsReview>
}
