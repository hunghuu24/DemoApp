package com.ute.app_ban_do_cu.domain.repository
import com.ute.app_ban_do_cu.domain.model.*

interface SmartSwapRepository {
    suspend fun mine(page:Int):TinCuaToi
    suspend fun search(tin:String?,wanted:String?,cap:Long?,condition:String?,city:String?,verified:Boolean,page:Int):SsKetQua
    suspend fun preview(body:SsPreviewBody):SsPreview
    suspend fun invite(body:SsMoiBody):SsVong
    suspend fun list(tab:String,page:Int):SsTrangVong
    suspend fun detail(id:String):SsVong
    suspend fun respond(id:String,body:SsPhanHoiBody):SsVong
    suspend fun propose(id:String,body:SsDeXuatBody):SsVong
    suspend fun cancel(id:String,body:SsHuyBody):SsVong
    suspend fun notifications(page:Int):List<SsThongBao>
    suspend fun read(id:String)
    suspend fun chat(id:String):HoiThoai
    suspend fun delivery(id:String):SsGiaoNhan
    suspend fun contact(id:String,body:SsContactBody):SsGiaoNhan
    suspend fun book(id:String,body:SsBookBody):SsGiaoNhan
    suspend fun tracking(id:String,ship:String,body:SsTrackingBody):SsGiaoNhan
    suspend fun status(id:String,ship:String,body:SsStatusBody):SsGiaoNhan
    suspend fun receive(id:String,ship:String,body:SsReceiveBody):SsGiaoNhan
    suspend fun issue(id:String,body:SsIssueBody):SsGiaoNhan
    suspend fun reviews(id:String):List<SsReview>
    suspend fun rate(id:String,body:SsReviewBody):List<SsReview>
    suspend fun evidence(id:String,key:String,bytes:ByteArray):SsGiaoNhan
}
