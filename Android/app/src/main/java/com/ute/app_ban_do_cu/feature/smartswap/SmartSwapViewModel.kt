package com.ute.app_ban_do_cu.feature.smartswap

import androidx.lifecycle.*
import androidx.lifecycle.viewmodel.initializer
import androidx.lifecycle.viewmodel.viewModelFactory
import com.google.gson.JsonParser
import com.ute.app_ban_do_cu.domain.model.*
import com.ute.app_ban_do_cu.domain.repository.*
import com.ute.app_ban_do_cu.feature.exchange.AnhBangChungCho
import kotlinx.coroutines.*
import kotlinx.coroutines.flow.*
import retrofit2.HttpException
import java.io.IOException
import java.util.UUID

enum class SsScreen { NONE,FIND,SELECT,MATCHES,PREVIEW,LIST,DETAIL,CONSENT,CHAT,COMPENSATION,DELIVERY,CONTACT,ISSUE,REVIEW,REVIEW_DONE,NOTIFICATIONS }
data class SsContactForm(val name:String="",val phone:String="",val from:String="",val to:String="",val confirmed:Boolean=false)
data class SsRatingForm(val stars:Int=0,val text:String="")
data class SmartSwapState(val screen:SsScreen=SsScreen.NONE,val busy:Boolean=false,val error:String?=null,val notice:String?=null,val expired:Boolean=false,val pollError:Boolean=false,
    val filter:SsTim=SsTim(),val cap:String="",val mine:TrangTin?=null,val selected:TinDang?=null,val results:SsKetQua?=null,
    val preview:SsPreview?=null,val photos:Map<String,String?> = emptyMap(),val cycles:SsTrangVong?=null,val tab:String="tatca",val circle:SsVong?=null,
    val message:String="",val checked:Boolean=false,val chat:HoiThoai?=null,val draft:String="",val baseVersion:String?=null,
    val compensation:List<SsTienBu> = emptyList(),val proposalText:String="",val delivery:SsGiaoNhan?=null,val contact:SsContactForm=SsContactForm(),
    val checkedItem:Boolean=false,val received:Boolean=false,val checkedMoney:Boolean=false,val issueShip:String?=null,val issueKind:String="SaiHang",val issueTitle:String="",val issueText:String="",
    val reviews:List<SsReview> = emptyList(),val ratings:Map<String,SsRatingForm> = emptyMap(),val evidence:List<AnhBangChungCho> = emptyList(),val notifications:List<SsThongBao>?=null,val notificationPage:Int=1)

class SmartSwapViewModel(private val repo:SmartSwapRepository,private val catalog:TinDangRepository,private val messages:MuaBanRepository):ViewModel(){
    private val state=MutableStateFlow(SmartSwapState());val uiState=state.asStateFlow()
    private val history=ArrayDeque<SsScreen>();private var user:String?=null;private var epoch=0;private var job:Job?=null;private var poll:Job?=null
    private var inviteKey=key();private var proposalKey=key();private var contactKey=key();private var issueKey=key();private var reviewKey=key();private var messageKey=key()
    private var previewCode:String?=null;private var circleId:String?=null
    private fun key()=UUID.randomUUID().toString()
    fun session(id:String?){if(user==id)return;epoch++;job?.cancel();poll?.cancel();user=id;history.clear();previewCode=null;circleId=null;state.value=SmartSwapState()}
    private fun nav(screen:SsScreen){if(state.value.screen!=screen)history.addLast(state.value.screen);poll?.cancel();state.update{it.copy(screen=screen,error=null,notice=null,pollError=false)}}
    fun close(){if(!state.value.busy){poll?.cancel();history.clear();state.update{it.copy(screen=SsScreen.NONE,error=null,notice=null)}}}
    fun back(){if(state.value.busy)return;poll?.cancel();val screen=history.removeLastOrNull()?:SsScreen.NONE;state.update{it.copy(screen=screen,error=null,notice=null)};if(screen==SsScreen.LIST)reload()}
    fun error(text:String){state.update{it.copy(error=text)}}
    private fun errorText(ex:Exception):String=when(ex){
        is IOException->"Chưa kết nối được ReMarket. Kiểm tra mạng rồi thử lại."
        is HttpException->{if(ex.code()==401)state.update{it.copy(expired=true)}
            runCatching{JsonParser.parseString(ex.response()?.errorBody()?.string()?:"{}").asJsonObject.get("detail")?.asString}.getOrNull()
                ?:when(ex.code()){400->"Kiểm tra thông tin đã nhập.";403->"Bạn không có quyền thao tác với vòng này.";404->"Vòng hoặc sản phẩm không còn khả dụng với tài khoản của bạn.";409->"Phương án vừa thay đổi. Tải lại để xem và xác nhận phiên bản mới.";429->"Bạn thao tác quá nhanh. Hãy thử lại sau.";else->"Không thực hiện được yêu cầu."}}
        else->"Không đọc được dữ liệu từ máy chủ."
    }
    private fun work(action:suspend()->Unit){if(state.value.busy)return;poll?.cancel();val session=epoch;job=viewModelScope.launch{
        state.update{it.copy(busy=true,error=null,notice=null)}
        try{action()}catch(ex:Exception){if(ex is CancellationException)throw ex;if(epoch==session)error(errorText(ex))}finally{if(epoch==session)state.update{it.copy(busy=false)}}}}
    fun find(){if(state.value.busy)return;history.clear();nav(SsScreen.FIND)}
    fun filter(change:(SsTim)->SsTim){if(!state.value.busy)state.update{it.copy(filter=change(it.filter),error=null)}}
    fun cap(text:String){state.update{it.copy(cap=text.filter(Char::isDigit).take(13),error=null)}}
    fun select(page:Int=1){if(state.value.busy)return;nav(SsScreen.SELECT);work{val rows=repo.mine(page).danhSach;state.update{it.copy(mine=rows)}}}
    fun choose(tin:TinDang){if(tin.traoDoi?.choPhepSmartSwap!=true||tin.trangThai!="DangHienThi")return error("Chọn tin Smart Swap đang hiển thị và còn khả dụng.")
        state.update{it.copy(selected=tin,filter=it.filter.copy(tinDangId=tin.id),cap=tin.traoDoi.soTienBuToiDa.toString())};back()}
    fun search(page:Int=1){val s=state.value;if(s.filter.tinDangId==null)return error("Chọn sản phẩm Smart Swap của bạn trước.")
        val cap=if(s.cap.isBlank())null else s.cap.toLongOrNull();if(s.cap.isNotBlank()&&(cap==null||cap !in 0..1_000_000_000_000L))return error("Nhập tiền bù tối đa hợp lệ.")
        if(s.busy)return;nav(SsScreen.MATCHES);state.update{it.copy(results=null)};work{val f=s.filter;val rows=repo.search(f.tinDangId,f.tuKhoaMuonNhan.trim().ifBlank{null},cap,f.tinhTrangToiThieu,f.tinhThanh.trim().ifBlank{null},f.verified,page);state.update{it.copy(results=rows)}}}
    fun preview(code:String){if(state.value.busy)return;previewCode=code;nav(SsScreen.PREVIEW);inviteKey=key();state.update{it.copy(preview=null,photos=emptyMap(),message="",checked=false)};work{loadPreview(code)}}
    private suspend fun loadPreview(code:String){val row=repo.preview(SsPreviewBody(code));state.update{it.copy(preview=row,checked=false)}
        val photos=coroutineScope{row.vong.chang.map{leg->async{val p=catalog.chiTiet(leg.sanPham.tinDangId,leg.nguoiGiaoId==user);leg.sanPham.sanPhamId to (p.anh.firstOrNull{it.laAnhBia}?:p.anh.firstOrNull())?.url}}.awaitAll().toMap()};state.update{it.copy(photos=photos)}}
    fun invitation(text:String=state.value.message,checked:Boolean=state.value.checked){inviteKey=key();state.update{it.copy(message=text.take(2000),checked=checked)}}
    fun invite(){val s=state.value;val p=s.preview?:return;if(!s.checked||s.message.trim().length<5)return error("Đọc phương án, đồng ý và nhập lời nhắn ít nhất 5 ký tự.")
        work{val row=repo.invite(SsMoiBody(inviteKey,p.vong.maDeXuat,p.bamPhuongAn,s.message.trim(),true));circleId=row.id;history.clear();history.addLast(SsScreen.LIST);state.update{it.copy(circle=row,screen=SsScreen.DETAIL,checked=false,notice="Đã gửi đề nghị cho hai thành viên. Chờ đủ 3 đồng ý.")}}}
    fun list(tab:String=state.value.tab,page:Int=1){if(state.value.busy)return;nav(SsScreen.LIST);state.update{it.copy(tab=tab,cycles=null)};work{val rows=repo.list(tab,page);state.update{it.copy(cycles=rows)}}}
    fun detail(id:String){if(state.value.busy)return;circleId=id;nav(SsScreen.DETAIL);state.update{it.copy(circle=null,delivery=null,checked=false,evidence=emptyList())};work{val row=repo.detail(id);state.update{it.copy(circle=row)}}}
    fun consent(){nav(SsScreen.CONSENT);state.update{it.copy(checked=false)}}
    fun checked(value:Boolean){state.update{it.copy(checked=value)}}
    fun respond(accept:Boolean,reason:String?=null){val s=state.value;val v=s.circle?:return;val p=v.phuongAnHienTai?:return
        if(accept&&!s.checked)return error("Đọc và xác nhận sản phẩm, tiền bù của phiên bản này.")
        if(!accept&&reason.orEmpty().trim().length<5)return error("Nhập lý do từ chối ít nhất 5 ký tự.")
        work{val row=repo.respond(v.id,SsPhanHoiBody(p.id,accept,accept,reason));state.update{it.copy(circle=row,checked=false,notice=if(accept)"Đã lưu đồng ý của bạn." else "Đề nghị đã dừng do từ chối.")}}}
    fun cancel(reason:String){val v=state.value.circle?:return;val p=v.phuongAnHienTai?:return;if(reason.trim().length<5)return error("Nhập lý do ít nhất 5 ký tự.")
        work{val row=repo.cancel(v.id,SsHuyBody(p.id,reason.trim()));state.update{it.copy(circle=row)}}}
    fun chat(){val v=state.value.circle?:return;if(state.value.busy)return;nav(SsScreen.CHAT);state.update{it.copy(chat=null)};work{val row=repo.chat(v.id);state.update{it.copy(chat=row)}}}
    fun draft(text:String){messageKey=key();state.update{it.copy(draft=text.take(4000))}}
    fun send(){val s=state.value;val c=s.chat?:return;if(s.draft.isBlank())return error("Nhập tin nhắn.");work{val row=messages.guiTin(c.id,GuiTinBody(messageKey,s.draft.trim()));state.update{it.copy(chat=merge(it.chat,row),draft="")};messageKey=key()}}
    fun sendImage(bytes:ByteArray){val c=state.value.chat?:return;if(bytes.isEmpty()||bytes.size>5*1024*1024)return error("Ảnh JPG/PNG tối đa 5 MB.");val requestKey=key();work{val row=messages.guiAnh(c.id,requestKey,bytes);state.update{it.copy(chat=merge(it.chat,row))}}}
    private fun merge(old:HoiThoai?,fresh:HoiThoai):HoiThoai=fresh.copy(tinNhan=((old?.takeIf{it.id==fresh.id}?.tinNhan?:emptyList())+fresh.tinNhan).distinctBy{it.id}.sortedBy{it.soThuTu})
    fun older(){val c=state.value.chat?:return;work{val row=messages.chat(c.id,c.tinNhan.firstOrNull()?.soThuTu);state.update{it.copy(chat=merge(it.chat,row))}}}
    fun compensation(){val v=state.value.circle?:return;val p=v.phuongAnHienTai?:return;if(!v.coTheDeXuat)return
        proposalKey=key();nav(SsScreen.COMPENSATION);state.update{it.copy(baseVersion=p.id,compensation=p.tienBu,proposalText="")}}
    fun money(payer:String,payee:String,amount:String){if(payer==payee)return;val s=state.value;val value=amount.toLongOrNull()?:0
        proposalKey=key();state.update{it.copy(compensation=s.compensation.filterNot{m->m.nguoiTraId==payer&&m.nguoiNhanId==payee}+if(value>0)listOf(SsTienBu(payer,payee,value))else emptyList())}}
    fun proposalText(text:String){proposalKey=key();state.update{it.copy(proposalText=text.take(2000))}}
    fun propose(){val s=state.value;val v=s.circle?:return;val p=s.baseVersion?:return;if(s.proposalText.trim().length<5)return error("Nhập lý do điều chỉnh ít nhất 5 ký tự.")
        if(s.compensation.size>2)return error("Phương án tối đa hai khoản bù. Xóa khoản cũ trước khi thêm khoản mới.")
        work{val row=repo.propose(v.id,SsDeXuatBody(proposalKey,p,s.compensation,s.proposalText.trim()));state.update{it.copy(circle=row,screen=SsScreen.CONSENT,checked=false,notice="Phiên bản mới cần cả ba xác nhận lại.")};if(history.lastOrNull()==SsScreen.CONSENT)history.removeLast()}}
    fun delivery(){val v=state.value.circle?:return;if(state.value.busy)return;nav(SsScreen.DELIVERY);state.update{it.copy(delivery=null,checkedItem=false,received=false,checkedMoney=false)};work{loadDelivery(v.id)}}
    private suspend fun loadDelivery(id:String){val d=repo.delivery(id);val v=repo.detail(id);state.update{it.copy(delivery=d,circle=v)}}
    fun contact(profile:HoSo){val d=state.value.delivery?:return;val mine=d.lienHe.firstOrNull{it.nguoiDungId==user};contactKey=key();nav(SsScreen.CONTACT)
        state.update{it.copy(baseVersion=d.phienBanThoaThuanId,contact=SsContactForm(mine?.tenLienHe?:profile.hoTen,mine?.soDienThoai?:profile.soDienThoai.orEmpty(),mine?.diaChiGui?:profile.diaChi.orEmpty(),mine?.diaChiNhan?:profile.diaChi.orEmpty()))}}
    fun contactForm(change:(SsContactForm)->SsContactForm){contactKey=key();state.update{it.copy(contact=change(it.contact))}}
    fun saveContact(){val s=state.value;val v=s.circle?:return;val version=s.baseVersion?:return;val f=s.contact
        if(f.name.trim().length<2||!Regex("^(\\+84|0)[0-9]{9}$").matches(f.phone.trim())||f.from.trim().length<10||f.to.trim().length<10||!f.confirmed)return error("Nhập tên, số điện thoại hợp lệ, địa chỉ từ 10 ký tự và xác nhận thông tin.")
        work{val d=repo.contact(v.id,SsContactBody(contactKey,version,f.name.trim(),f.phone.trim(),f.from.trim(),f.to.trim(),true,s.delivery?.lienHe?.firstOrNull{it.nguoiDungId==user}?.phienBan));state.update{it.copy(delivery=d,screen=SsScreen.DELIVERY)};if(history.lastOrNull()==SsScreen.DELIVERY)history.removeLast()}}
    fun book(){val v=state.value.circle?:return;val p=v.phuongAnHienTai?:return;work{repo.book(v.id,SsBookBody(p.id));loadDelivery(v.id)}}
    fun tracking(ship:SsShip,code:String){val v=state.value.circle?:return;if(!Regex("^[A-Za-z0-9][A-Za-z0-9_-]{3,39}$").matches(code.trim()))return error("Mã vận đơn gồm 4–40 chữ, số, dấu - hoặc _.")
        work{val d=repo.tracking(v.id,ship.id,SsTrackingBody(ship.phienBan,code.trim()));state.update{it.copy(delivery=d)}}}
    fun ship(ship:SsShip,next:String){val v=state.value.circle?:return;work{val d=repo.status(v.id,ship.id,SsStatusBody(ship.phienBan,next,next=="DaGui"));state.update{it.copy(delivery=d)}}}
    fun receipt(item:Boolean=state.value.checkedItem,received:Boolean=state.value.received,money:Boolean=state.value.checkedMoney){state.update{it.copy(checkedItem=item,received=received,checkedMoney=money)}}
    fun receive(ship:SsShip){val s=state.value;val v=s.circle?:return;val p=v.phuongAnHienTai?:return
        if(!s.checkedItem||!s.received||!s.checkedMoney)return error("Xác nhận đã kiểm tra, nhận đúng hàng và tiền bù của bạn nếu có.")
        work{repo.receive(v.id,ship.id,SsReceiveBody(p.id,true,true,true));loadDelivery(v.id);uploadEvidence(v.id);state.update{it.copy(checkedItem=false,received=false,checkedMoney=false,notice=if(it.delivery?.tienDo?.hoanTat==true)"Trao đổi thành công!" else "Đã nhận đúng hàng. Chờ các thành viên còn lại.")}}}
    fun addEvidence(bytes:ByteArray){val s=state.value;if(bytes.isEmpty()||bytes.size>5*1024*1024)return error("Ảnh JPG/PNG tối đa 5 MB.")
        if(s.evidence.size+s.delivery?.bangChung.orEmpty().count{it.nguoiTaiLenId==user}>=5)return error("Mỗi người tối đa 5 ảnh bằng chứng.")
        state.update{it.copy(evidence=it.evidence+AnhBangChungCho(key(),bytes))}}
    fun removeEvidence(id:String){state.update{it.copy(evidence=it.evidence.filterNot{p->p.id==id})}}
    private suspend fun uploadEvidence(id:String){for(photo in state.value.evidence){val d=repo.evidence(id,photo.key,photo.bytes);state.update{it.copy(delivery=d,evidence=it.evidence.filterNot{p->p.id==photo.id})}}}
    fun saveEvidence(){val v=state.value.circle?:return;work{uploadEvidence(v.id)}}
    fun issue(ship:SsShip){issueKey=key();nav(SsScreen.ISSUE);state.update{it.copy(issueShip=ship.id,issueKind=if(ship.trangThai=="ChoGui")"ChuaNhanHang" else "SaiHang",issueTitle="",issueText="")}}
    fun issueForm(kind:String=state.value.issueKind,title:String=state.value.issueTitle,text:String=state.value.issueText){issueKey=key();state.update{it.copy(issueKind=kind,issueTitle=title.take(200),issueText=text.take(4000))}}
    fun report(){val s=state.value;val v=s.circle?:return;val ship=s.issueShip?:return;if(s.issueTitle.trim().length<5||s.issueText.trim().length<10)return error("Tiêu đề ít nhất 5 ký tự, nội dung ít nhất 10 ký tự.")
        work{repo.issue(v.id,SsIssueBody(issueKey,ship,s.issueKind,s.issueTitle.trim(),s.issueText.trim()));loadDelivery(v.id);state.update{it.copy(screen=SsScreen.DELIVERY,notice="Đã báo sự cố. Giao dịch tạm dừng để xử lý.")};if(history.lastOrNull()==SsScreen.DELIVERY)history.removeLast()}}
    fun review(){val v=state.value.circle?:return;if(v.trangThai!="HoanTat")return;reviewKey=key();nav(SsScreen.REVIEW);state.update{it.copy(ratings=emptyMap(),reviews=emptyList())};work{val rows=repo.reviews(v.id);state.update{it.copy(reviews=rows)}}}
    fun rating(id:String,change:(SsRatingForm)->SsRatingForm){reviewKey=key();state.update{it.copy(ratings=it.ratings+(id to change(it.ratings[id]?:SsRatingForm())))}}
    fun rate(){val s=state.value;val v=s.circle?:return;val partners=v.thanhVien.filter{it.nguoiDungId!=user};val items=partners.map{val f=s.ratings[it.nguoiDungId]?:SsRatingForm();SsReviewItem(it.nguoiDungId,f.stars,f.text.trim())}
        if(items.any{it.soSao !in 1..5||it.noiDung.length<5})return error("Đánh giá cả hai thành viên: chọn số sao và nhận xét ít nhất 5 ký tự.")
        work{val rows=repo.rate(v.id,SsReviewBody(reviewKey,items));state.update{it.copy(reviews=rows,screen=SsScreen.REVIEW_DONE)}}}
    fun notifications(page:Int=1){if(state.value.busy)return;nav(SsScreen.NOTIFICATIONS);work{val rows=repo.notifications(page);state.update{it.copy(notifications=rows,notificationPage=page)}}}
    fun openNotification(n:SsThongBao){if(state.value.busy)return;circleId=n.vongId;nav(SsScreen.DETAIL);state.update{it.copy(circle=null,delivery=null,evidence=emptyList())};work{repo.read(n.id);val row=repo.detail(n.vongId);state.update{it.copy(circle=row)}}}
    fun reload(){val s=state.value;when(s.screen){
        SsScreen.SELECT->select(s.mine?.trang?:1);SsScreen.MATCHES->search(s.results?.trang?:1);SsScreen.PREVIEW->previewCode?.let{code->work{loadPreview(code)}}
        SsScreen.LIST->list(s.tab,s.cycles?.trang?:1);SsScreen.NOTIFICATIONS->notifications(s.notificationPage)
        SsScreen.FIND->Unit
        else->(s.circle?.id?:circleId)?.let{id->work{val row=repo.detail(id);state.update{it.copy(circle=row,checked=if(row.phuongAnHienTai?.id!=it.circle?.phuongAnHienTai?.id)false else it.checked)}
            if(s.screen==SsScreen.DELIVERY)loadDelivery(id);if(s.screen==SsScreen.CHAT){val c=repo.chat(id);state.update{it.copy(chat=merge(it.chat,c))}}
            if(s.screen==SsScreen.REVIEW){val rows=repo.reviews(id);state.update{it.copy(reviews=rows)}}}}
    }}
    fun poll(){val s=state.value;val v=s.circle?:return;if(s.busy||poll?.isActive==true||s.screen !in listOf(SsScreen.DETAIL,SsScreen.CONSENT,SsScreen.CHAT,SsScreen.DELIVERY))return;val session=epoch
        poll=viewModelScope.launch{try{val row=repo.detail(v.id);val d=if(s.screen==SsScreen.DELIVERY)repo.delivery(v.id)else null;val c=if(s.screen==SsScreen.CHAT)repo.chat(v.id)else null
            if(epoch==session&&state.value.screen==s.screen&&state.value.circle?.id==v.id&&!state.value.busy)state.update{it.copy(circle=row,delivery=d?:it.delivery,chat=if(c!=null)merge(it.chat,c)else it.chat,
                checked=if(row.phuongAnHienTai?.id!=it.circle?.phuongAnHienTai?.id)false else it.checked,pollError=false)}
        }catch(ex:Exception){if(ex is CancellationException)throw ex;if(epoch==session){if(ex is HttpException&&ex.code()==401)state.update{it.copy(expired=true)};state.update{it.copy(pollError=true)}}}}
    }
    companion object{fun factory(repo:SmartSwapRepository,catalog:TinDangRepository,messages:MuaBanRepository):ViewModelProvider.Factory=viewModelFactory{initializer{SmartSwapViewModel(repo,catalog,messages)}}}
}
