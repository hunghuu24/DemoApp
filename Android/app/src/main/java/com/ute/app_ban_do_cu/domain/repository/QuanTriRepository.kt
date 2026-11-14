package com.ute.app_ban_do_cu.domain.repository
import com.ute.app_ban_do_cu.domain.model.*
interface QuanTriRepository {
    suspend fun report(from:String?,to:String?):QtReport
    suspend fun users(q:String,status:String?,page:Int):QtPage<QtUser>
    suspend fun user(id:String):QtUserDetail
    suspend fun lock(id:String,body:QtLockBody):QtUserDetail
    suspend fun unlock(id:String,body:QtUnlockBody):QtUserDetail
    suspend fun categories():List<QtCategory>
    suspend fun createCategory(body:QtCategoryBody):QtCategory
    suspend fun updateCategory(id:String,body:QtCategoryBody):QtCategory
    suspend fun listings(q:String,status:String?,page:Int):TrangTin
    suspend fun listing(id:String):QtListing
    suspend fun review(id:String,body:DuyetTinBody):QtListing
    suspend fun hide(id:String,body:QtHideBody):QtListing
    suspend fun trades(q:String,type:String?,status:String?,page:Int):QtPage<QtTrade>
    suspend fun trade(id:String,before:Long?):QtTradeDetail
    suspend fun complaints(q:String,status:String?,page:Int):QtPage<QtComplaint>
    suspend fun complaint(id:String):QtComplaintDetail
    suspend fun createComplaint(id:String,body:QtCreateComplaintBody):QtComplaintDetail
    suspend fun accept(id:String,body:QtAcceptBody):QtComplaintDetail
    suspend fun decide(id:String,body:QtDecisionBody):QtComplaintDetail
    suspend fun verify(id:String,body:QtVerifyBody):QtComplaintDetail
}
