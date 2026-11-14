package com.ute.app_ban_do_cu.feature.exchange

import androidx.lifecycle.*
import androidx.lifecycle.viewmodel.initializer
import androidx.lifecycle.viewmodel.viewModelFactory
import com.google.gson.JsonParser
import com.ute.app_ban_do_cu.domain.model.*
import com.ute.app_ban_do_cu.domain.repository.*
import com.ute.app_ban_do_cu.feature.catalog.AnhChon
import kotlinx.coroutines.*
import kotlinx.coroutines.flow.*
import retrofit2.HttpException
import java.io.IOException
import java.time.*
import java.time.format.DateTimeFormatter
import java.time.format.ResolverStyle
import java.util.UUID

enum class ManTraoDoi{NONE,KHAM_PHA,TIN_TOI,FORM_TIN,TIN_XONG,CHI_TIET,CHON_MON,TAO_DE_NGHI,DE_NGHI,CHI_TIET_DE_NGHI,DE_XUAT_LAI,GIAO_NHAN,DANH_GIA,DANH_GIA_XONG}
data class FormTraoDoi(val id:String?=null,val phienBan:String?=null,val ten:String="",val danhMucId:String="",val tinhTrang:String="NhuMoi",
    val moTa:String="",val gia:String="",val muonNhan:String="",val motMot:Boolean=true,val smart:Boolean=true,val buTien:Boolean=true,val buToiDa:String="0",
    val ghiChu:String="",val tinhThanh:String="",val anh:List<AnhTin> = emptyList(),val anhCho:List<AnhChon> = emptyList(),val errors:Map<String,String> = emptyMap())
data class AnhBangChungCho(val id:String,val bytes:ByteArray,val key:String=UUID.randomUUID().toString())
data class TraoDoiState(val man:ManTraoDoi=ManTraoDoi.NONE,val busy:Boolean=false,val error:String?=null,val notice:String?=null,val expired:Boolean=false,
    val filter:BoLocTraoDoi=BoLocTraoDoi(),val results:TrangTin?=null,val categories:List<DanhMucBanHang> = emptyList(),val mine:TinCuaToi?=null,val mineStatus:String?=null,
    val detail:TinDang?=null,val own:Boolean=false,val form:FormTraoDoi=FormTraoDoi(),val target:TinDang?=null,val selected:TinDang?=null,val selectable:TrangTin?=null,val selectQuery:String="",
    val amount:String="0",val payer:String?=null,val message:String="",val location:String="",val date:String="",val time:String="",val terms:Boolean=false,
    val offers:TrangDeNghi?=null,val tab:String="gui",val offer:DeNghiTraoDoi?=null,val checked:Boolean=false,val received:Boolean=false,val evidence:List<AnhBangChungCho> = emptyList(),
    val stars:Int=0,val rating:String="",val criteria:List<TieuChiTraoDoi> = emptyList(),val tags:Set<String> = emptySet(),val pollError:Boolean=false)

class TraoDoiViewModel(private val repo:TraoDoiRepository,private val catalog:TinDangRepository):ViewModel(){
    private val state=MutableStateFlow(TraoDoiState());val uiState=state.asStateFlow()
    private val history=ArrayDeque<ManTraoDoi>();private var user:String?=null;private var epoch=0;private var job:Job?=null;private var poll:Job?=null
    private var listingId:String?=null;private var offerId:String?=null;private var creationKey=key();private var ratingKey=key()
    private fun key()=UUID.randomUUID().toString()
    fun phienMoi(id:String?){if(user==id)return;epoch++;job?.cancel();poll?.cancel();user=id;history.clear();listingId=null;offerId=null;state.value=TraoDoiState()}
    private fun nav(man:ManTraoDoi){if(state.value.man!=man)history.addLast(state.value.man);state.update{it.copy(man=man,error=null,notice=null,pollError=false)}}
    fun dong(){if(!state.value.busy){history.clear();poll?.cancel();state.update{it.copy(man=ManTraoDoi.NONE,error=null,notice=null)}}}
    fun back(){if(state.value.busy)return;poll?.cancel();val man=history.removeLastOrNull()?:ManTraoDoi.NONE;state.update{it.copy(man=man,error=null,notice=null)};if(man in listOf(ManTraoDoi.TIN_TOI,ManTraoDoi.DE_NGHI,ManTraoDoi.KHAM_PHA))taiLai()}
    fun loi(text:String){state.update{it.copy(error=text)}}
    private fun work(action:suspend()->Unit){
        if(state.value.busy)return;val session=epoch
        job=viewModelScope.launch{state.update{it.copy(busy=true,error=null,notice=null)}
            try{action()}catch(ex:Exception){if(ex is CancellationException)throw ex;if(epoch==session){
                val text=when(ex){is IOException->"Chưa kết nối được ReMarket. Kiểm tra mạng rồi thử lại.";is HttpException->{if(ex.code()==401)state.update{it.copy(expired=true)}
                    val detail=try{JsonParser.parseString(ex.response()?.errorBody()?.string()?:"{}").asJsonObject.get("detail")?.asString}catch(_:Exception){null}
                    detail?:when(ex.code()){404->"Tin hoặc đề nghị không còn khả dụng.";403->"Bạn không có quyền thao tác.";409->"Dữ liệu vừa thay đổi. Hãy tải lại.";429->"Bạn thao tác quá nhanh. Hãy thử lại sau.";else->"Không thực hiện được yêu cầu."}}
                    else->"Không đọc được dữ liệu từ máy chủ."};state.update{it.copy(error=text)}
            }}finally{if(epoch==session)state.update{it.copy(busy=false)}}}
    }
    private suspend fun categories(){if(state.value.categories.isEmpty()){val rows=catalog.danhMuc();state.update{it.copy(categories=rows)}}}
    fun khamPha(){if(state.value.busy)return;history.clear();nav(ManTraoDoi.KHAM_PHA);work{categories();val rows=repo.tim(state.value.filter,1);state.update{it.copy(results=rows)}}}
    fun query(text:String){state.update{it.copy(filter=it.filter.copy(tuKhoa=text.take(100)))}}
    fun tim(filter:BoLocTraoDoi=state.value.filter,page:Int=1){if(state.value.busy)return;state.update{it.copy(filter=filter)};work{val rows=repo.tim(filter,page);state.update{it.copy(results=rows)}}}
    fun tinToi(status:String?=null,page:Int=1){if(state.value.busy)return;nav(ManTraoDoi.TIN_TOI);state.update{it.copy(mineStatus=status)};work{val rows=repo.cuaToi(status,page);state.update{it.copy(mine=rows)}}}
    fun chiTiet(id:String,own:Boolean=false){if(state.value.busy)return;listingId=id;nav(ManTraoDoi.CHI_TIET);state.update{it.copy(detail=null,own=own)};work{val row=catalog.chiTiet(id,own);state.update{it.copy(detail=row)}}}
    fun taoTin(city:String=""){if(state.value.busy)return;nav(ManTraoDoi.FORM_TIN);state.update{it.copy(form=FormTraoDoi(tinhThanh=city),detail=null,own=true)};work{categories()}}
    fun suaTin(){val d=state.value.detail?:return;val n=d.traoDoi?:return;if(state.value.busy)return;nav(ManTraoDoi.FORM_TIN)
        state.update{it.copy(form=FormTraoDoi(d.id,d.phienBan,d.ten,d.danhMucId,d.tinhTrang,d.moTa,d.giaTriThamKhao.toString(),n.moTaMongMuon,n.choPhepMotMot,n.choPhepSmartSwap,n.choPhepBuTien,n.soTienBuToiDa.toString(),n.ghiChu.orEmpty(),d.tinhThanh,d.anh))};work{categories()}}
    fun form(change:(FormTraoDoi)->FormTraoDoi){if(!state.value.busy)state.update{it.copy(form=change(it.form).copy(errors=emptyMap()),error=null)}}
    fun themAnh(photos:List<AnhChon>){val f=state.value.form;if(photos.any{it.bytes.isEmpty()||it.bytes.size>5*1024*1024})return loi("Mỗi ảnh JPG/PNG tối đa 5 MB.")
        if(f.anh.size+f.anhCho.size+photos.size>6)return loi("Tin trao đổi tối đa 6 ảnh.");form{it.copy(anhCho=(it.anhCho+photos).distinctBy{p->p.uri})}}
    fun boAnh(uri:String){form{it.copy(anhCho=it.anhCho.filterNot{p->p.uri==uri})}}
    fun xoaAnh(photo:AnhTin){val f=state.value.form;val id=f.id?:return;work{val d=catalog.xoaAnh(id,photo.id,f.phienBan?:"");state.update{it.copy(form=it.form.copy(anh=d.anh,phienBan=d.phienBan))}}}
    fun bia(photo:AnhTin){val f=state.value.form;val id=f.id?:return;work{val d=catalog.anhBia(id,photo.id,f.phienBan?:"");state.update{it.copy(form=it.form.copy(anh=d.anh,phienBan=d.phienBan))}}}
    private fun errors(f:FormTraoDoi,publish:Boolean)=buildMap{
        if(f.ten.trim().length<3)put("ten","Tên cần ít nhất 3 ký tự.");if(f.danhMucId.isBlank())put("danhMuc","Chọn danh mục.")
        if(f.moTa.trim().length<10)put("moTa","Mô tả cần ít nhất 10 ký tự.");if(f.muonNhan.trim().length<10)put("muonNhan","Mô tả món muốn nhận ít nhất 10 ký tự.")
        if((f.gia.toLongOrNull()?:0) !in 1..1_000_000_000_000L)put("gia","Nhập giá trị tham khảo hợp lệ.")
        if(!f.motMot&&!f.smart)put("hinhThuc","Chọn ít nhất một hình thức trao đổi.")
        if(f.buTien&&(f.buToiDa.toLongOrNull()?:-1) !in 0..1_000_000_000_000L)put("buToiDa","Nhập mức bù tối đa, nhập 0 nếu không thể bù.")
        if(f.tinhThanh.trim().length<2)put("tinhThanh","Nhập tỉnh/thành phố.")
        if(publish&&f.anh.isEmpty()&&f.anhCho.isEmpty())put("anh","Chọn ít nhất một ảnh.")
    }
    fun luu(publish:Boolean){val f=state.value.form;val errors=errors(f,publish);if(errors.isNotEmpty()){state.update{it.copy(form=f.copy(errors=errors))};return loi("Kiểm tra các thông tin bắt buộc.")}
        work{
            var d=catalog.luu(f.id,LuuTinBody(f.ten.trim(),f.danhMucId,f.gia.toLong(),f.tinhTrang,f.moTa.trim(),f.tinhThanh.trim(),f.tinhThanh.trim(),f.phienBan,
                DieuKienTraoDoi(f.muonNhan.trim(),f.motMot,f.smart,f.buTien,if(f.buTien)f.buToiDa.toLong()else 0,f.ghiChu.trim().ifBlank{null})))
            listingId=d.id;state.update{it.copy(detail=d,form=it.form.copy(id=d.id,phienBan=d.phienBan,anh=d.anh))}
            for(photo in f.anhCho){d=catalog.themAnh(d.id,d.phienBan,photo.bytes);state.update{it.copy(detail=d,form=it.form.copy(phienBan=d.phienBan,anh=d.anh,anhCho=it.form.anhCho.filterNot{p->p.uri==photo.uri}))}}
            if(publish)d=catalog.chuyen(d.id,d.phienBan,"gui-duyet")
            state.update{it.copy(detail=d,own=true,man=if(publish)ManTraoDoi.TIN_XONG else ManTraoDoi.CHI_TIET,notice=if(publish)"Đã gửi duyệt tin trao đổi." else "Đã lưu bản nháp.")}
        }
    }
    fun chuyenTin(action:String){val d=state.value.detail?:return;work{if(action=="xoa"){catalog.xoa(d.id,d.phienBan);val rows=repo.cuaToi(null,1);history.clear();state.update{it.copy(man=ManTraoDoi.TIN_TOI,mine=rows,detail=null)}}else{val row=catalog.chuyen(d.id,d.phienBan,action);state.update{it.copy(detail=row)}}}}
    fun chonMon(){val target=state.value.detail?:return;if(state.value.busy)return;nav(ManTraoDoi.CHON_MON);state.update{it.copy(target=target,selected=null,selectQuery="",selectable=null)};work{val rows=repo.monChon("",1);state.update{it.copy(selectable=rows)}}}
    fun timMon(query:String=state.value.selectQuery,page:Int=1){state.update{it.copy(selectQuery=query.take(100))};work{val rows=repo.monChon(query,page);state.update{it.copy(selectable=rows)}}}
    fun queryMon(query:String){state.update{it.copy(selectQuery=query.take(100))}}
    fun chon(tin:TinDang){state.update{it.copy(selected=tin)}}
    fun tiepTuc(){if(state.value.selected==null)return loi("Chọn một món của bạn.");creationKey=key();nav(ManTraoDoi.TAO_DE_NGHI);state.update{it.copy(amount="0",payer=user,message="",location=it.target?.tinhThanh.orEmpty(),date="",time="",terms=false)}}
    fun deNghiForm(amount:String=state.value.amount,payer:String?=state.value.payer,message:String=state.value.message,location:String=state.value.location,date:String=state.value.date,time:String=state.value.time,terms:Boolean=state.value.terms){
        creationKey=key();state.update{it.copy(amount=amount.filter(Char::isDigit).take(13),payer=payer,message=message.take(2000),location=location.take(500),date=date.take(10),time=time.take(5),terms=terms)}}
    private fun appointment(s:TraoDoiState):String?{
        if(s.date.isBlank()&&s.time.isBlank())return null
        val fmt=DateTimeFormatter.ofPattern("dd/MM/uuuu HH:mm").withResolverStyle(ResolverStyle.STRICT)
        val date=LocalDateTime.parse(s.date.trim()+" "+s.time.trim(),fmt).atZone(ZoneId.of("Asia/Ho_Chi_Minh"))
        require(date.toInstant().isAfter(Instant.now())){"Thời gian hẹn cần ở tương lai."};return date.toOffsetDateTime().toString()
    }
    fun guiDeNghi(){val s=state.value;val gui=s.selected?:return;val nhan=s.target?:return
        if(!s.terms||s.message.trim().length<5)return loi("Nhập lời nhắn và đồng ý quy định.")
        val amount=s.amount.toLongOrNull()?:return loi("Nhập tiền bù, hoặc 0 nếu không bù.")
        val appointment=try{appointment(s)}catch(_:Exception){return loi("Ngày hẹn theo dd/MM/yyyy, giờ HH:mm và ở tương lai.")}
        work{val o=repo.tao(TaoDeNghiBody(creationKey,gui.id,nhan.id,gui.phienBan,nhan.phienBan,amount,s.payer,s.message.trim(),s.location.trim().ifBlank{null},appointment,true));offerId=o.id;history.clear();history.addLast(ManTraoDoi.DE_NGHI);state.update{it.copy(offer=o,man=ManTraoDoi.CHI_TIET_DE_NGHI,notice="Đã gửi đề nghị. Chờ đối tác phản hồi.")}}
    }
    fun deNghi(tab:String="gui",page:Int=1){if(state.value.busy)return;nav(ManTraoDoi.DE_NGHI);state.update{it.copy(tab=tab)};work{val rows=repo.deNghi(tab,page);state.update{it.copy(offers=rows)}}}
    fun docDeNghi(id:String){if(state.value.busy)return;offerId=id;nav(ManTraoDoi.CHI_TIET_DE_NGHI);state.update{it.copy(offer=null)};work{val o=repo.doc(id);state.update{it.copy(offer=o)}}}
    fun phanHoi(accept:Boolean,reason:String?=null){val o=state.value.offer?:return;work{val row=repo.phanHoi(o.id,PhanHoiDeNghiBody(o.phienBan,accept,reason));state.update{it.copy(offer=row)}}}
    fun huy(reason:String){val o=state.value.offer?:return;if(reason.trim().length<5)return loi("Lý do cần ít nhất 5 ký tự.");work{val row=repo.huy(o.id,HuyDeNghiBody(o.phienBan,reason.trim()));state.update{it.copy(offer=row)}}}
    fun moDoi(){val p=state.value.offer?.thoaThuan?:return
        val hen=runCatching{p.thoiGianHen?.let{OffsetDateTime.parse(it).atZoneSameInstant(ZoneId.of("Asia/Ho_Chi_Minh"))}}.getOrNull()
        nav(ManTraoDoi.DE_XUAT_LAI);state.update{it.copy(amount=p.soTienBu.toString(),payer=p.nguoiTraId?:it.offer?.nguoiGuiId,message="",location=p.diaDiemHen.orEmpty(),
            date=hen?.format(DateTimeFormatter.ofPattern("dd/MM/uuuu")).orEmpty(),time=hen?.format(DateTimeFormatter.ofPattern("HH:mm")).orEmpty())}}
    fun guiDoi(){val s=state.value;val o=s.offer?:return;val amount=s.amount.toLongOrNull()?:return loi("Nhập mức bù hợp lệ.");if(s.message.trim().length<5)return loi("Nhập lời nhắn ít nhất 5 ký tự.")
        val appointment=try{appointment(s)}catch(_:Exception){return loi("Ngày/giờ hẹn không hợp lệ.")}
        work{val row=repo.doi(o.id,DoiDeNghiBody(o.phienBan,amount,s.payer,s.message.trim(),s.location.trim().ifBlank{null},appointment));state.update{it.copy(offer=row,man=ManTraoDoi.CHI_TIET_DE_NGHI)};if(history.lastOrNull()==ManTraoDoi.CHI_TIET_DE_NGHI)history.removeLast()}}
    fun chat(open:(String)->Unit){val o=state.value.offer?:return;work{val chat=repo.chat(o.id);open(chat.id)}}
    fun moGiaoNhan(){nav(ManTraoDoi.GIAO_NHAN);state.update{it.copy(checked=false,received=false,evidence=emptyList())}}
    fun kiemTra(checked:Boolean=state.value.checked,received:Boolean=state.value.received){state.update{it.copy(checked=checked,received=received)}}
    fun themBangChung(photos:List<AnhChon>){val s=state.value;val uploaded=s.offer?.bangChung?.count{it.nguoiTaiLenId==user}?:0
        if(uploaded+s.evidence.size+photos.size>5)return loi("Mỗi lần nhận tối đa 5 ảnh bằng chứng.")
        if(photos.any{it.bytes.isEmpty()||it.bytes.size>5*1024*1024})return loi("Ảnh JPG/PNG tối đa 5 MB.")
        state.update{it.copy(evidence=(it.evidence+photos.map{p->AnhBangChungCho(p.uri,p.bytes)}).distinctBy{p->p.id})}}
    fun boBangChung(id:String){state.update{it.copy(evidence=it.evidence.filterNot{p->p.id==id})}}
    fun xacNhan(){val s=state.value;val o=s.offer?:return;if(!s.checked||!s.received)return loi("Xác nhận đã kiểm tra, giao/nhận đồ và đủ tiền bù nếu có.")
        work{var row=if(o.daXacNhanCuaToi)o else repo.nhan(o.id,GiaoNhanTraoDoiBody(o.phienBan,true,true,true,true));state.update{it.copy(offer=row)}
            for(photo in s.evidence){row=repo.bangChung(o.id,photo.key,photo.bytes);state.update{it.copy(offer=row,evidence=it.evidence.filterNot{p->p.id==photo.id})}}
            state.update{it.copy(man=ManTraoDoi.CHI_TIET_DE_NGHI,notice=if(row.trangThaiGiaoDich=="HoanTat")"Trao đổi đã hoàn tất." else "Đã xác nhận phía bạn. Chờ đối tác xác nhận.")};if(history.lastOrNull()==ManTraoDoi.CHI_TIET_DE_NGHI)history.removeLast()
        }}
    fun moDanhGia(){ratingKey=key();nav(ManTraoDoi.DANH_GIA);state.update{it.copy(stars=0,rating="",tags=emptySet())};work{val rows=repo.tieuChi();state.update{it.copy(criteria=rows)}}}
    fun danhGiaForm(stars:Int=state.value.stars,text:String=state.value.rating){ratingKey=key();state.update{it.copy(stars=stars,rating=text.take(2000))}}
    fun tag(id:String){ratingKey=key();state.update{it.copy(tags=if(id in it.tags)it.tags-id else it.tags+id)}}
    fun guiDanhGia(){val s=state.value;val o=s.offer?:return;if(s.stars !in 1..5||s.rating.trim().length<5)return loi("Chọn số sao và nhận xét ít nhất 5 ký tự.")
        work{val row=repo.danhGia(o.id,DanhGiaTraoDoiBody(ratingKey,s.stars,s.rating.trim(),s.tags.toList()));state.update{it.copy(offer=row,man=ManTraoDoi.DANH_GIA_XONG)}}}
    fun taiLai(){val s=state.value;when(s.man){
        ManTraoDoi.KHAM_PHA->tim(s.filter,s.results?.trang?:1);ManTraoDoi.TIN_TOI->tinToi(s.mineStatus,s.mine?.danhSach?.trang?:1)
        ManTraoDoi.CHI_TIET->listingId?.let{id->work{val d=catalog.chiTiet(id,s.own);state.update{it.copy(detail=d)}}}
        ManTraoDoi.CHON_MON->timMon();ManTraoDoi.DE_NGHI->deNghi(s.tab,s.offers?.trang?:1)
        ManTraoDoi.CHI_TIET_DE_NGHI->offerId?.let{id->work{val o=repo.doc(id);state.update{it.copy(offer=o)}}}
        ManTraoDoi.TAO_DE_NGHI->guiDeNghi()
        ManTraoDoi.DE_XUAT_LAI,ManTraoDoi.GIAO_NHAN->s.offer?.let{o->work{val row=repo.doc(o.id);val changed=row.thoaThuan?.id!=o.thoaThuan?.id;state.update{it.copy(offer=row,checked=if(changed)false else it.checked,received=if(changed)false else it.received)}}}
        ManTraoDoi.DANH_GIA->if(s.criteria.isEmpty())work{val rows=repo.tieuChi();state.update{it.copy(criteria=rows)}}else guiDanhGia()
        ManTraoDoi.FORM_TIN->if(s.categories.isEmpty())work{categories()}else luu(false)
        else->Unit
    }}
    fun capNhat(){val s=state.value;val o=s.offer?:return;if(s.man!=ManTraoDoi.CHI_TIET_DE_NGHI||s.busy||poll?.isActive==true)return;val session=epoch
        poll=viewModelScope.launch{try{val row=repo.doc(o.id);if(epoch==session&&state.value.man==s.man&&state.value.offer?.id==o.id&&!state.value.busy)state.update{it.copy(offer=row,pollError=false)}}catch(ex:Exception){if(ex is CancellationException)throw ex;if(epoch==session){if(ex is HttpException&&ex.code()==401)state.update{it.copy(expired=true)};state.update{it.copy(pollError=true)}}}}
    }
    companion object{fun factory(repo:TraoDoiRepository,catalog:TinDangRepository):ViewModelProvider.Factory=viewModelFactory{initializer{TraoDoiViewModel(repo,catalog)}}}
}
