package com.ute.app_ban_do_cu.data.remote
import com.ute.app_ban_do_cu.domain.model.*
import com.ute.app_ban_do_cu.domain.repository.QuanTriRepository
import retrofit2.http.*
interface QuanTriApi:QuanTriRepository {
    @GET("api/admin/bao-cao") override suspend fun report(@Query("tuNgay")from:String?,@Query("denNgay")to:String?):QtReport
    @GET("api/admin/nguoi-dung") override suspend fun users(@Query("q")q:String,@Query("trangThai")status:String?,@Query("trang")page:Int):QtPage<QtUser>
    @GET("api/admin/nguoi-dung/{id}") override suspend fun user(@Path("id")id:String):QtUserDetail
    @POST("api/admin/nguoi-dung/{id}/khoa") override suspend fun lock(@Path("id")id:String,@Body body:QtLockBody):QtUserDetail
    @POST("api/admin/nguoi-dung/{id}/mo-khoa") override suspend fun unlock(@Path("id")id:String,@Body body:QtUnlockBody):QtUserDetail
    @GET("api/admin/danh-muc") override suspend fun categories():List<QtCategory>
    @POST("api/admin/danh-muc") override suspend fun createCategory(@Body body:QtCategoryBody):QtCategory
    @PUT("api/admin/danh-muc/{id}") override suspend fun updateCategory(@Path("id")id:String,@Body body:QtCategoryBody):QtCategory
    @GET("api/admin/san-pham") override suspend fun listings(@Query("q")q:String,@Query("trangThai")status:String?,@Query("trang")page:Int):TrangTin
    @GET("api/admin/san-pham/{id}") override suspend fun listing(@Path("id")id:String):QtListing
    @POST("api/admin/san-pham/{id}/duyet") override suspend fun review(@Path("id")id:String,@Body body:DuyetTinBody):QtListing
    @POST("api/admin/san-pham/{id}/an") override suspend fun hide(@Path("id")id:String,@Body body:QtHideBody):QtListing
    @GET("api/admin/giao-dich") override suspend fun trades(@Query("q")q:String,@Query("loai")type:String?,@Query("trangThai")status:String?,@Query("trang")page:Int):QtPage<QtTrade>
    @GET("api/admin/giao-dich/{id}") override suspend fun trade(@Path("id")id:String,@Query("truoc")before:Long?):QtTradeDetail
    @GET("api/admin/khieu-nai") override suspend fun complaints(@Query("q")q:String,@Query("trangThai")status:String?,@Query("trang")page:Int):QtPage<QtComplaint>
    @GET("api/admin/khieu-nai/{id}") override suspend fun complaint(@Path("id")id:String):QtComplaintDetail
    @POST("api/admin/giao-dich/{id}/khieu-nai") override suspend fun createComplaint(@Path("id")id:String,@Body body:QtCreateComplaintBody):QtComplaintDetail
    @POST("api/admin/khieu-nai/{id}/tiep-nhan") override suspend fun accept(@Path("id")id:String,@Body body:QtAcceptBody):QtComplaintDetail
    @POST("api/admin/khieu-nai/{id}/quyet-dinh") override suspend fun decide(@Path("id")id:String,@Body body:QtDecisionBody):QtComplaintDetail
    @POST("api/admin/khieu-nai/{id}/xac-minh") override suspend fun verify(@Path("id")id:String,@Body body:QtVerifyBody):QtComplaintDetail
}
