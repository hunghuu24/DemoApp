package com.ute.app_ban_do_cu.feature.admin

import androidx.lifecycle.*
import androidx.lifecycle.viewmodel.initializer
import androidx.lifecycle.viewmodel.viewModelFactory
import com.google.gson.JsonParser
import com.ute.app_ban_do_cu.domain.model.*
import com.ute.app_ban_do_cu.domain.repository.QuanTriRepository
import kotlinx.coroutines.*
import kotlinx.coroutines.flow.*
import retrofit2.HttpException
import java.io.IOException
import java.time.LocalDate
import java.time.ZoneId
import java.util.UUID

enum class QtScreen { HOME,USERS,USER,LISTINGS,LISTING,CATEGORIES,CATEGORY_FORM,TRADES,TRADE,COMPLAINTS,COMPLAINT,CREATE_COMPLAINT,DECISION,VERIFY,REPORT,EVIDENCE }
data class QtComplaintForm(val reporter:String="",val accused:String="",val title:String="",val text:String="",val checked:Boolean=false)
data class QtCategoryForm(val id:String?=null,val phienBan:String?=null,val ten:String="",val parent:String?=null,val order:String="0",val visible:Boolean=true,val icon:String="recycle")
data class QtDecisionForm(val evidence:String="",val conclusion:String="",val action:String="",val message:String="",val direction:String="TiepTuc",val notify:Boolean=false)
data class QuanTriState(val screen:QtScreen=QtScreen.HOME,val busy:Boolean=false,val error:String?=null,val notice:String?=null,val expired:Boolean=false,val denied:Boolean=false,
    val report:QtReport?=null,val from:String="",val to:String="",val period:Int=30,val query:String="",val status:String?=null,val type:String?=null,
    val users:QtPage<QtUser>?=null,val user:QtUserDetail?=null,val listings:TrangTin?=null,val listing:QtListing?=null,
    val categories:List<QtCategory>?=null,val category:QtCategoryForm=QtCategoryForm(),val trades:QtPage<QtTrade>?=null,val trade:QtTradeDetail?=null,
    val complaints:QtPage<QtComplaint>?=null,val complaint:QtComplaintDetail?=null,val decision:QtDecisionForm=QtDecisionForm(),val verifyText:String="",val verified:Boolean=false,
    val evidence:QtEvidence?=null,val checks:Set<Int> = emptySet(),val complaintForm:QtComplaintForm=QtComplaintForm())

class QuanTriViewModel(private val repo:QuanTriRepository):ViewModel(){
    private val state=MutableStateFlow(QuanTriState());val uiState=state.asStateFlow();private val history=ArrayDeque<QtScreen>()
    private var user:String?=null;private var epoch=0;private var job:Job?=null;private var decisionKey=key();private var complaintKey=key();private var userId:String?=null;private var listingId:String?=null;private var tradeId:String?=null;private var complaintId:String?=null
    private fun key()=UUID.randomUUID().toString()
    fun session(id:String?,roles:List<String>){val active=id?.takeIf{"ADMIN" in roles};if(user==active)return;epoch++;job?.cancel();user=active;history.clear();userId=null;listingId=null;tradeId=null;complaintId=null;state.value=QuanTriState();if(active!=null)home()}
    private fun nav(screen:QtScreen){if(state.value.screen!=screen)history.addLast(state.value.screen);state.update{it.copy(screen=screen,error=null,notice=null)}}
    fun back(){if(state.value.busy)return;val next=history.removeLastOrNull()?:QtScreen.HOME;state.update{it.copy(screen=next,error=null,notice=null)};if(next==QtScreen.HOME)home()else if(next in listOf(QtScreen.USERS,QtScreen.LISTINGS,QtScreen.CATEGORIES,QtScreen.TRADES,QtScreen.COMPLAINTS,QtScreen.TRADE,QtScreen.COMPLAINT))reload()}
    private fun work(action:suspend()->Unit){if(state.value.busy||user==null)return;val session=epoch;job=viewModelScope.launch{state.update{it.copy(busy=true,error=null,notice=null)}
        try{action()}catch(ex:Exception){if(ex is CancellationException)throw ex;if(epoch==session){val message=when(ex){is IOException->"Chưa kết nối được ReMarket. Kiểm tra mạng rồi thử lại.";is HttpException->{if(ex.code()==401)state.update{it.copy(expired=true)};if(ex.code()==403)state.update{it.copy(denied=true)};runCatching{JsonParser.parseString(ex.response()?.errorBody()?.string()?:"{}").asJsonObject.get("detail")?.asString}.getOrNull()?:when(ex.code()){403->"Tài khoản không có quyền quản trị.";404->"Không tìm thấy dữ liệu.";409->"Dữ liệu vừa thay đổi, tải lại trước khi thao tác.";else->"Không thực hiện được yêu cầu."}};else->"Không đọc được dữ liệu từ máy chủ."};state.update{it.copy(error=message)}}}finally{if(epoch==session)state.update{it.copy(busy=false)}}}}
    fun error(text:String){state.update{it.copy(error=text)}}
    fun query(text:String){state.update{it.copy(query=text.take(100))}}
    fun home(){if(state.value.busy)return;history.clear();state.update{it.copy(screen=QtScreen.HOME,query="",status=null,type=null,period=30)};work{val report=repo.report(null,null);state.update{it.copy(report=report)}}}
    fun section(screen:QtScreen){if(state.value.busy)return;history.clear();history.addLast(QtScreen.HOME);state.update{it.copy(query="",status=null,type=null,screen=screen,error=null,notice=null)}
        when(screen){QtScreen.HOME->home();QtScreen.USERS->users();QtScreen.LISTINGS->listings("ChoDuyet");QtScreen.CATEGORIES->categories();QtScreen.TRADES->trades();QtScreen.COMPLAINTS->complaints();QtScreen.REPORT->report();else->Unit}}
    fun users(status:String?=state.value.status,page:Int=1){if(state.value.busy)return;nav(QtScreen.USERS);state.update{it.copy(status=status)};work{val rows=repo.users(state.value.query.trim(),status,page);state.update{it.copy(users=rows)}}}
    fun user(id:String){if(state.value.busy)return;userId=id;nav(QtScreen.USER);state.update{it.copy(user=null)};work{val row=repo.user(id);state.update{it.copy(user=row)}}}
    fun lock(reason:String,days:Int){val u=state.value.user?.nguoiDung?:return;if(reason.trim().length<5)return error("Nêu rõ lý do khóa ít nhất 5 ký tự.");work{val row=repo.lock(u.id,QtLockBody(u.phienBan,reason.trim(),days));state.update{it.copy(user=row,notice="Đã khóa tài khoản và thu hồi các phiên đăng nhập.")}}}
    fun unlock(reason:String,checked:Boolean){val u=state.value.user?.nguoiDung?:return;if(!checked||reason.trim().length<5)return error("Rà soát và nêu lý do mở khóa.");work{val row=repo.unlock(u.id,QtUnlockBody(u.phienBan,reason.trim(),true));state.update{it.copy(user=row,notice="Đã mở khóa. Người dùng cần đăng nhập lại.")}}}
    fun listings(status:String?=state.value.status,page:Int=1){if(state.value.busy)return;nav(QtScreen.LISTINGS);state.update{it.copy(status=status)};work{val rows=repo.listings(state.value.query.trim(),status,page);state.update{it.copy(listings=rows)}}}
    fun listing(id:String){if(state.value.busy)return;listingId=id;nav(QtScreen.LISTING);state.update{it.copy(listing=null,checks=emptySet())};work{val row=repo.listing(id);state.update{it.copy(listing=row)}}}
    fun check(id:Int){state.update{it.copy(checks=if(id in it.checks)it.checks-id else it.checks+id)}}
    fun review(accept:Boolean,reason:String?=null){val t=state.value.listing?.tin?:return;if(accept&&state.value.checks.size!=3)return error("Kiểm tra đủ ba tiêu chí trước khi duyệt.");if(!accept&&reason.orEmpty().trim().length<5)return error("Nhập lý do từ chối ít nhất 5 ký tự.")
        work{val row=repo.review(t.id,DuyetTinBody(t.phienBan,accept,reason?.trim()));state.update{it.copy(listing=row,checks=emptySet(),notice=if(accept)"Tin đã được duyệt." else "Đã từ chối tin và lưu lý do.")}}}
    fun hide(reason:String){val t=state.value.listing?.tin?:return;if(reason.trim().length<5)return error("Nhập lý do ẩn tin.");work{val row=repo.hide(t.id,QtHideBody(t.phienBan,reason.trim()));state.update{it.copy(listing=row,notice="Đã ẩn tin, giữ dữ liệu lịch sử giao dịch.")}}}
    fun categories(){if(state.value.busy)return;nav(QtScreen.CATEGORIES);work{val rows=repo.categories();state.update{it.copy(categories=rows)}}}
    fun category(row:QtCategory?=null){if(state.value.busy)return;nav(QtScreen.CATEGORY_FORM);state.update{it.copy(category=if(row==null)QtCategoryForm()else QtCategoryForm(row.id,row.phienBan,row.ten,row.danhMucChaId,row.thuTu.toString(),row.dangHienThi,row.bieuTuong.ifBlank{"recycle"}))};if(state.value.categories==null)work{val rows=repo.categories();state.update{it.copy(categories=rows)}}}
    fun categoryForm(change:(QtCategoryForm)->QtCategoryForm){state.update{it.copy(category=change(it.category),error=null)}}
    fun saveCategory(){val f=state.value.category;val order=f.order.toIntOrNull();if(f.ten.trim().length<2||order==null||order !in 0..100000)return error("Tên ít nhất 2 ký tự và thứ tự từ 0 đến 100.000.")
        work{val body=QtCategoryBody(f.ten.trim(),f.parent,order,f.visible,f.icon,f.phienBan);if(f.id==null)repo.createCategory(body)else repo.updateCategory(f.id,body);val rows=repo.categories();state.update{it.copy(categories=rows,screen=QtScreen.CATEGORIES,notice="Đã lưu danh mục.")};if(history.lastOrNull()==QtScreen.CATEGORIES)history.removeLast()}}
    fun trades(type:String?=state.value.type,status:String?=state.value.status,page:Int=1){if(state.value.busy)return;nav(QtScreen.TRADES);state.update{it.copy(type=type,status=status)};work{val rows=repo.trades(state.value.query.trim(),type,status,page);state.update{it.copy(trades=rows)}}}
    fun trade(id:String){if(state.value.busy)return;tradeId=id;nav(QtScreen.TRADE);state.update{it.copy(trade=null)};work{val row=repo.trade(id,null);state.update{it.copy(trade=row)}}}
    fun older(){val t=state.value.trade?:return;val first=t.tinNhan.firstOrNull()?.soThuTu?:return;work{val row=repo.trade(t.giaoDich.id,first);state.update{it.copy(trade=row.copy(tinNhan=(row.tinNhan+t.tinNhan).distinctBy{m->m.id}.sortedBy{m->m.soThuTu}))}}}
    fun complaints(status:String?=state.value.status,page:Int=1){if(state.value.busy)return;nav(QtScreen.COMPLAINTS);state.update{it.copy(status=status)};work{val rows=repo.complaints(state.value.query.trim(),status,page);state.update{it.copy(complaints=rows)}}}
    fun complaint(id:String){if(state.value.busy)return;complaintId=id;nav(QtScreen.COMPLAINT);state.update{it.copy(complaint=null)};work{val row=repo.complaint(id);state.update{it.copy(complaint=row)}}}
    fun createComplaint(){if(state.value.trade==null)return;complaintKey=key();nav(QtScreen.CREATE_COMPLAINT);state.update{it.copy(complaintForm=QtComplaintForm())}}
    fun complaintForm(change:(QtComplaintForm)->QtComplaintForm){complaintKey=key();state.update{it.copy(complaintForm=change(it.complaintForm),error=null)}}
    fun saveComplaint(){val t=state.value.trade?.giaoDich?:return;val f=state.value.complaintForm
        if(f.reporter.isBlank()||f.accused.isBlank()||f.reporter==f.accused||f.title.trim().length<5||f.text.trim().length<10||!f.checked)return error("Chọn hai thành viên khác nhau, nhập phản ánh và xác nhận đã đối chiếu.")
        work{val row=repo.createComplaint(t.id,QtCreateComplaintBody(complaintKey,f.reporter,f.accused,f.title.trim(),f.text.trim(),true));complaintId=row.khieuNai.id;state.update{it.copy(complaint=row,screen=QtScreen.COMPLAINT,notice="Đã ghi nhận phản ánh và thông báo các bên.")}}}
    fun accept(text:String){val k=state.value.complaint?.khieuNai?:return;if(text.trim().length<5)return error("Nhập nội dung tiếp nhận hoặc yêu cầu bổ sung.");work{val row=repo.accept(k.id,QtAcceptBody(k.phienBan,text.trim()));state.update{it.copy(complaint=row,notice="Đã lưu phản hồi và thông báo cho các bên.")}}}
    fun decision(){val k=state.value.complaint?.khieuNai?:return;decisionKey=key();nav(QtScreen.DECISION);state.update{it.copy(decision=QtDecisionForm(direction=if(k.loaiGiaoDich=="MuaBan")"GhiNhan" else "TiepTuc"))}}
    fun decisionForm(change:(QtDecisionForm)->QtDecisionForm){decisionKey=key();state.update{it.copy(decision=change(it.decision),error=null)}}
    fun decide(){val s=state.value;val k=s.complaint?.khieuNai?:return;val f=s.decision
        if(f.evidence.trim().length<10||f.conclusion.trim().length<5||f.action.trim().length<5||f.message.trim().length<10||!f.notify)return error("Nhập đủ căn cứ, kết luận, biện pháp và nội dung thông báo; xác nhận thông báo các bên.")
        work{val row=repo.decide(k.id,QtDecisionBody(decisionKey,k.phienBan,f.evidence.trim(),f.conclusion.trim(),f.action.trim(),f.message.trim(),f.direction,true));state.update{it.copy(complaint=row,screen=QtScreen.COMPLAINT,notice="Đã lưu quyết định. Khiếu nại chờ các bên thực hiện và được xác minh.")};if(history.lastOrNull()==QtScreen.COMPLAINT)history.removeLast()}}
    fun verify(){nav(QtScreen.VERIFY);state.update{it.copy(verifyText="",verified=false)}}
    fun verifyForm(text:String=state.value.verifyText,checked:Boolean=state.value.verified){state.update{it.copy(verifyText=text.take(2000),verified=checked)}}
    fun confirmVerification(){val s=state.value;val k=s.complaint?.khieuNai?:return;val p=s.complaint.quyetDinh.firstOrNull{!it.daXacMinhThucHien}?:return
        if(!s.verified||s.verifyText.trim().length<10)return error("Xác minh thực hiện và ghi căn cứ ít nhất 10 ký tự.")
        work{val row=repo.verify(k.id,QtVerifyBody(k.phienBan,p.id,true,s.verifyText.trim()));state.update{it.copy(complaint=row,screen=QtScreen.COMPLAINT,notice=if(row.khieuNai.trangThai=="DangXuLy")"Đã xác minh. Khiếu nại vẫn mở để tiếp tục xử lý."else "Đã xác minh quyết định. Thành viên vẫn tự xác nhận hàng.")};if(history.lastOrNull()==QtScreen.COMPLAINT)history.removeLast()}}
    fun evidence(row:QtEvidence){nav(QtScreen.EVIDENCE);state.update{it.copy(evidence=row)}}
    fun dates(from:String=state.value.from,to:String=state.value.to){state.update{it.copy(from=from.take(10),to=to.take(10),period=0)}}
    fun report(days:Int=state.value.period){if(state.value.busy)return;nav(QtScreen.REPORT);val today=LocalDate.now(ZoneId.of("Asia/Ho_Chi_Minh"));val s=state.value
        val from=if(days>0)today.minusDays(days.toLong()-1).toString()else s.from;val to=if(days>0)today.toString()else s.to
        try{val a=LocalDate.parse(from);val b=LocalDate.parse(to);if(a>b||java.time.temporal.ChronoUnit.DAYS.between(a,b)>365)return error("Khoảng ngày tối đa 366 ngày và ngày bắt đầu không sau ngày kết thúc.")}catch(_:Exception){return error("Nhập ngày theo yyyy-MM-dd.")}
        state.update{it.copy(from=from,to=to,period=days)};work{val row=repo.report(from,to);state.update{it.copy(report=row)}}}
    fun reload(){val s=state.value;when(s.screen){QtScreen.HOME->home();QtScreen.USERS->users(page=s.users?.trang?:1);QtScreen.USER->userId?.let{id->work{val row=repo.user(id);state.update{it.copy(user=row)}}};QtScreen.LISTINGS->listings(page=s.listings?.trang?:1);QtScreen.LISTING->listingId?.let{id->work{val row=repo.listing(id);state.update{it.copy(listing=row,checks=emptySet())}}};QtScreen.CATEGORIES->categories();QtScreen.CATEGORY_FORM->work{val rows=repo.categories();val id=state.value.category.id;val row=rows.firstOrNull{it.id==id};state.update{it.copy(categories=rows,category=if(row==null)it.category else QtCategoryForm(row.id,row.phienBan,row.ten,row.danhMucChaId,row.thuTu.toString(),row.dangHienThi,row.bieuTuong.ifBlank{"recycle"}),notice=if(row==null)null else "Đã tải lại danh mục mới nhất.")}};QtScreen.TRADES->trades(page=s.trades?.trang?:1);QtScreen.TRADE->tradeId?.let{id->work{val row=repo.trade(id,null);state.update{it.copy(trade=row)}}};QtScreen.COMPLAINTS->complaints(page=s.complaints?.trang?:1);QtScreen.COMPLAINT,QtScreen.DECISION,QtScreen.VERIFY->complaintId?.let{id->work{val row=repo.complaint(id);state.update{it.copy(complaint=row)}}};QtScreen.REPORT->report();else->Unit}}
    companion object{fun factory(repo:QuanTriRepository):ViewModelProvider.Factory=viewModelFactory{initializer{QuanTriViewModel(repo)}}}
}
