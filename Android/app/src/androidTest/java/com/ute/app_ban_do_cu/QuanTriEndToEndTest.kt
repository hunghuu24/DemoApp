package com.ute.app_ban_do_cu

import android.graphics.Bitmap
import android.graphics.Color
import androidx.compose.ui.test.*
import androidx.compose.ui.test.junit4.createEmptyComposeRule
import androidx.lifecycle.ViewModelProvider
import androidx.test.core.app.ActivityScenario
import androidx.test.core.app.ApplicationProvider
import androidx.test.espresso.Espresso.closeSoftKeyboard
import androidx.test.platform.app.InstrumentationRegistry
import com.google.gson.Gson
import com.ute.app_ban_do_cu.domain.model.*
import com.ute.app_ban_do_cu.feature.admin.*
import com.ute.app_ban_do_cu.feature.auth.TaiKhoanViewModel
import kotlinx.coroutines.runBlocking
import okhttp3.*
import okhttp3.MediaType.Companion.toMediaType
import okhttp3.RequestBody.Companion.toRequestBody
import org.junit.*
import java.io.ByteArrayOutputStream
import java.io.File
import java.util.UUID
import java.util.concurrent.TimeUnit

/** Real JWT/SQL fixtures, never role flags or fake repositories. */
class QuanTriEndToEndTest {
    @get:Rule val compose=createEmptyComposeRule()
    private val app get()=ApplicationProvider.getApplicationContext<ReMarketApplication>()
    private val gson=Gson();private val http=OkHttpClient.Builder().callTimeout(45,TimeUnit.SECONDS).build()
    private var scenario:ActivityScenario<MainActivity>?=null;private lateinit var vm:QuanTriViewModel;private lateinit var auth:TaiKhoanViewModel
    private val suffix=UUID.randomUUID().toString().replace("-","").take(10);private val password="AdminUi!2026"
    private lateinit var admin:PhienDto;private lateinit var email:String;private lateinit var adminPassword:String
    private val users=mutableListOf<PhienDto>();private val emails=mutableListOf<String>();private val listings=mutableListOf<Pair<TinDang,Int>>()
    private val cats=mutableListOf<String>()
    private val circles=mutableListOf<String>()
    private val png by lazy{val b=Bitmap.createBitmap(160,100,Bitmap.Config.ARGB_8888).apply{eraseColor(Color.rgb(40,115,79))};ByteArrayOutputStream().use{b.compress(Bitmap.CompressFormat.PNG,100,it);b.recycle();it.toByteArray()}}
    private fun key()=UUID.randomUUID().toString()
    private fun request(method:String,path:String,body:Any?=null,token:String=admin.accessToken,expected:Int=200):String {
        val b=Request.Builder().url(BuildConfig.API_BASE_URL+path.removePrefix("/"));if(token.isNotBlank())b.header("Authorization","Bearer $token")
        val payload=if(method=="GET")null else gson.toJson(body?:emptyMap<String,String>()).toRequestBody("application/json".toMediaType())
        http.newUiTestCall(b.method(method,payload).build()).execute().use{val text=it.body?.string().orEmpty();Assert.assertEquals("$method $path: $text",expected,it.code);return text}
    }
    private inline fun <reified T> get(path:String,token:String=admin.accessToken):T=gson.fromJson(request("GET",path,token=token),T::class.java)
    private inline fun <reified T> post(path:String,body:Any,token:String=admin.accessToken,expected:Int=200):T=gson.fromJson(request("POST",path,body,token,expected),T::class.java)
    private inline fun <reified T> put(path:String,body:Any,token:String=admin.accessToken):T=gson.fromJson(request("PUT",path,body,token),T::class.java)
    private fun upload(path:String,token:String,fields:Map<String,String>,name:String="anh"):String{val b=MultipartBody.Builder().setType(MultipartBody.FORM);fields.forEach{(k,v)->b.addFormDataPart(k,v)};b.addFormDataPart(name,"qa.png",png.toRequestBody("image/png".toMediaType()))
        http.newUiTestCall(Request.Builder().url(BuildConfig.API_BASE_URL+path).header("Authorization","Bearer $token").post(b.build()).build()).execute().use{val text=it.body?.string().orEmpty();Assert.assertEquals(text,200,it.code);return text}}
    private fun idle(){compose.waitUntil(60_000){!vm.uiState.value.busy&&!auth.uiState.value.dangTai};compose.waitForIdle()}
    private fun ready(tag:String){compose.waitUntil(60_000){compose.onAllNodesWithTag(tag).fetchSemanticsNodes().isNotEmpty()};idle()}
    private fun click(tag:String,scroll:Boolean=false){idle();val n=compose.onNodeWithTag(tag);if(scroll)n.performScrollTo();n.performClick();idle()}
    private fun input(tag:String,value:String){idle();compose.onNodeWithTag(tag).performScrollTo().performTextReplacement(value)}
    private fun shot(name:String){closeSoftKeyboard();compose.waitForIdle();val b=InstrumentationRegistry.getInstrumentation().uiAutomation.takeScreenshot();File(app.getExternalFilesDir("qa-admin"),"$name.png").apply{parentFile?.mkdirs()}.outputStream().use{b.compress(Bitmap.CompressFormat.PNG,100,it)};b.recycle()}
    private fun home(){if(vm.uiState.value.screen!=QtScreen.HOME)menu(QtScreen.HOME);ready("admin_home")}
    private fun menu(screen:QtScreen){repeat(12){if(compose.onAllNodesWithTag("qt_back").fetchSemanticsNodes().isNotEmpty())click("qt_back")};click("admin_menu");compose.onNodeWithTag("qt_menu_${screen.name}").performScrollTo().performClick();idle()}
    private fun start(){runBlocking{app.container.phienStore.xoa();app.container.taiKhoanRepository.dangNhap(DangNhapBody(email,adminPassword))};scenario=ActivityScenario.launch(MainActivity::class.java);scenario!!.onActivity{vm=ViewModelProvider(it)[QuanTriViewModel::class.java];auth=ViewModelProvider(it)[TaiKhoanViewModel::class.java]};ready("admin_home")}
    private fun account(label:String):Int{val i=users.size;val mail="admin-ui-$suffix-$i@example.test";request("POST","api/taikhoan/dangky",DangKyBody(label,mail,"07"+((suffix+i).hashCode().toLong().and(0x7fffffff)%100000000).toString().padStart(8,'0'),password,password),token="",expected=201);emails.add(mail);users.add(post("api/taikhoan/dangnhap",DangNhapBody(mail,password),token=""));return i}
    @Before fun setup(){val args=InstrumentationRegistry.getArguments();email=requireNotNull(args.getString("adminEmail"));adminPassword=requireNotNull(args.getString("adminPassword"));admin=post("api/taikhoan/dangnhap",DangNhapBody(email,adminPassword),token="");account("Thành viên $suffix")}
    private fun listing(actor:Int=0,swap:DieuKienTraoDoi?=null,title:String="Tin quản trị $suffix",value:Long=500000):TinDang{
        val token=users[actor].accessToken;val category=get<Array<DanhMucBanHang>>("api/tindang/danhmuc",token).first{it.danhMucChaId==null}.id
        var t=post<TinDang>("api/tindang",LuuTinBody(title,category,value,"NhuMoi","Sản phẩm demo kiểm thử quản trị, còn tốt và có ảnh thực tế.","Hà Nội","Hà Nội",null,swap),token,201)
        t=gson.fromJson(upload("api/tindang/${t.id}/anh",token,mapOf("phienBan" to t.phienBan)),TinDang::class.java)
        t=post("api/tindang/${t.id}/gui-duyet",PhienBanTinBody(t.phienBan),token);listings.add(t to actor);return t
    }
    @After fun cleanup(){
        users.forEach{u->val row=get<QtUserDetail>("api/admin/nguoi-dung/${u.hoSo.id}");if(row.coTheMoKhoa)post<QtUserDetail>("api/admin/nguoi-dung/${u.hoSo.id}/mo-khoa",QtUnlockBody(row.nguoiDung.phienBan,"Khôi phục tài khoản kiểm thử quản trị",true))}
        circles.forEach{id->val v=get<SsVong>("api/smart-swaps/$id",users[0].accessToken);if(v.trangThai !in listOf("HoanTat","DaHuy")){
            // Only this test's own three registered participants and products can be cleaned up.
            Assert.assertTrue(v.thanhVien.all{m->users.any{it.hoSo.id==m.nguoiDungId}&&listings.any{it.first.sanPhamId==m.sanPhamCho.sanPhamId}})
            var k=post<QtComplaintDetail>("api/admin/giao-dich/${v.giaoDichId}/khieu-nai",QtCreateComplaintBody(key(),users[0].hoSo.id,users[1].hoSo.id,"Dọn vòng kiểm thử quản trị","Kết thúc vòng vận chuyển mô phỏng do chính kịch bản kiểm thử tạo.",true),expected=201)
            k=post("api/admin/khieu-nai/${k.khieuNai.id}/quyet-dinh",QtDecisionBody(key(),k.khieuNai.phienBan,"Đã đối chiếu danh sách tài khoản và sản phẩm riêng của test.","Kết thúc vòng kiểm thử","Nhả giữ chỗ cho ba sản phẩm demo trong kịch bản.","Kịch bản kết thúc, đây là dữ liệu mô phỏng riêng được dọn sau kiểm thử.","KetThuc",true))
            post<QtComplaintDetail>("api/admin/khieu-nai/${k.khieuNai.id}/xac-minh",QtVerifyBody(k.khieuNai.phienBan,k.quyetDinh.first().id,true,"Đã kiểm tra chỉ có dữ liệu mô phỏng do chính test tạo và không có hàng thật."))
        }}
        listings.forEach{(t,actor)->val login=post<PhienDto>("api/taikhoan/dangnhap",DangNhapBody(emails[actor],password),token="");val row=get<TinDang>("api/tindang/${t.id}/quanly",login.accessToken);if(row.trangThai!="DangGiuCho")request("DELETE","api/tindang/${t.id}",PhienBanTinBody(row.phienBan),login.accessToken,204)}
        val all=get<Array<QtCategory>>("api/admin/danh-muc");cats.forEach{id->all.firstOrNull{it.id==id}?.let{c->put<QtCategory>("api/admin/danh-muc/$id",QtCategoryBody(c.ten,c.danhMucChaId,c.thuTu,false,c.bieuTuong,c.phienBan))}}
        runBlocking{if(app.container.taiKhoanRepository.conPhien())app.container.taiKhoanRepository.dangXuat();app.container.phienStore.xoa()};scenario?.close()
    }
    @Test fun nguoiDungKiemDuyetDanhMucVaThongKe(){
        val pending=listing();start();shot("dashboard")
        menu(QtScreen.USERS);ready("qt_users");shot("users");input("qt_query",emails[0]);closeSoftKeyboard();click("qt_search",true);idle();click("qt_user_${users[0].hoSo.id}",true);ready("qt_user_detail");shot("user-detail")
        click("qt_lock_open");input("qt_action_reason","Đăng tin trùng lặp cần rà soát");closeSoftKeyboard();shot("lock-dialog");click("qt_action_confirm");idle();Assert.assertEquals("DaKhoa",vm.uiState.value.user!!.nguoiDung.trangThai);shot("user-locked")
        request("GET","api/taikhoan/toi",token=users[0].accessToken,expected=401)
        click("qt_unlock_open");input("qt_action_reason","Đã rà soát, đủ điều kiện mở lại");closeSoftKeyboard();click("qt_unlock_check",true);click("qt_action_confirm");idle();Assert.assertEquals("HoatDong",vm.uiState.value.user!!.nguoiDung.trangThai);shot("user-unlocked")
        menu(QtScreen.LISTINGS);shot("listings");input("qt_query",suffix);closeSoftKeyboard();click("qt_search",true);idle();click("qt_listing_${pending.id}",true);ready("qt_listing_detail");shot("pending-listing")
        click("qt_reject_open");shot("reject-dialog");input("qt_action_reason","Cần bổ sung thông tin và ảnh chi tiết");closeSoftKeyboard();click("qt_action_confirm");idle();Assert.assertEquals("TuChoi",vm.uiState.value.listing!!.tin.trangThai)
        users[0]=post("api/taikhoan/dangnhap",DangNhapBody(emails[0],password),token="");val next=listing(title="Tin duyệt mới $suffix")
        menu(QtScreen.LISTINGS);shot("listings");input("qt_query",suffix);closeSoftKeyboard();click("qt_search",true);idle();click("qt_listing_${next.id}",true);ready("qt_listing_detail");(0..2).forEach{click("qt_moderation_check_$it",true)};shot("moderation-checklist");click("qt_approve");idle();Assert.assertEquals("DangHienThi",vm.uiState.value.listing!!.tin.trangThai);shot("approved-listing")
        click("qt_hide_open");input("qt_action_reason","Ẩn tin trong buổi kiểm thử quản trị");closeSoftKeyboard();click("qt_action_confirm");idle();Assert.assertEquals("DaAn",vm.uiState.value.listing!!.tin.trangThai)
        menu(QtScreen.CATEGORIES);click("qt_category_new");ready("qt_category_form");input("qt_category_name","Danh mục $suffix");click("qt_category_icon",true);click("qt_category_icon_book-open");input("qt_category_order","99");closeSoftKeyboard();shot("category-form");click("qt_category_save");ready("qt_categories")
        val c=vm.uiState.value.categories!!.single{it.ten=="Danh mục $suffix"};cats.add(c.id);Assert.assertEquals("book-open",c.bieuTuong)
        input("qt_query",suffix);closeSoftKeyboard();click("qt_category_${c.id}",true);ready("qt_category_form")
        val edited=put<QtCategory>("api/admin/danh-muc/${c.id}",QtCategoryBody("Cập nhật cạnh tranh $suffix",c.danhMucChaId,c.thuTu,true,c.bieuTuong,c.phienBan))
        click("qt_category_save");idle();Assert.assertNotNull(vm.uiState.value.error);click("qt_reload");idle()
        Assert.assertEquals(edited.phienBan,vm.uiState.value.category.phienBan);Assert.assertEquals(edited.ten,vm.uiState.value.category.ten)
        input("qt_category_name","Sách sửa $suffix");click("qt_category_visible",true);click("qt_category_save");ready("qt_categories");Assert.assertFalse(vm.uiState.value.categories!!.single{it.id==c.id}.dangHienThi);shot("categories")
        menu(QtScreen.REPORT);ready("qt_report");compose.onNodeWithText("7 ngày").performClick();idle();Assert.assertEquals(7,vm.uiState.value.report!!.bieuDo.size);shot("statistics")
        menu(QtScreen.TRADES);ready("qt_trades");shot("trades");click("qt_tab_MuaBan",true);idle();Assert.assertTrue(vm.uiState.value.trades!!.items.all{it.loai=="MuaBan"});click("qt_tab_TraoDoiMotMot",true);idle();Assert.assertTrue(vm.uiState.value.trades!!.items.all{it.loai=="TraoDoiMotMot"})
        menu(QtScreen.COMPLAINTS);ready("qt_complaints");shot("complaints");home();click("admin_menu");shot("admin-menu");click("qt_logout",true);ready("login");compose.onNodeWithTag("admin_home").assertDoesNotExist()
        request("GET","api/admin/bao-cao",token=users[0].accessToken,expected=403)
    }
    @Test fun smartSwapChiDocHaiTrenBaBangChungVaXuLyTranhChap(){
        account("Thành viên B $suffix");account("Thành viên C $suffix");val titles=listOf("iPhone","Laptop","iPad");val values=listOf(10000000L,12000000L,8000000L)
        val tins=(0..2).map{i->val t=listing(i,DieuKienTraoDoi("Muốn nhận ${titles[(i+1)%3]} $suffix",true,true,true,2000000),"${titles[i]} $suffix",values[i]);post<QtListing>("api/admin/san-pham/${t.id}/duyet",DuyetTinBody(t.phienBan,true,null)).tin}
        val found=get<SsKetQua>("api/smart-swaps/search?tinDangId=${tins[0].id}",users[0].accessToken).items.single();val p=post<SsPreview>("api/smart-swaps/preview",SsPreviewBody(found.maDeXuat),users[0].accessToken)
        var v=post<SsVong>("api/smart-swaps/invitations",SsMoiBody(key(),found.maDeXuat,p.bamPhuongAn,"Cùng đối chiếu giao nhận và tiền bù trong buổi kiểm thử.",true),users[0].accessToken,201);circles.add(v.id)
        (1..2).forEach{v=post("api/smart-swaps/${v.id}/responses",SsPhanHoiBody(v.phuongAnHienTai!!.id,true,true),users[it].accessToken)}
        (0..2).forEach{i->put<SsGiaoNhan>("api/smart-swaps/${v.id}/delivery/contact",SsContactBody(key(),v.phuongAnHienTai!!.id,users[i].hoSo.hoTen,"0901234567","123 Đường gửi, Hà Nội","456 Đường nhận, Hà Nội",true,null),users[i].accessToken)}
        var delivery=post<SsGiaoNhan>("api/smart-swaps/${v.id}/shipments",SsBookBody(v.phuongAnHienTai!!.id),users[0].accessToken)
        delivery.vanChuyen.forEach{original->val i=users.indexOfFirst{it.hoSo.id==original.nguoiGuiId};var ship=original
            delivery=put("api/smart-swaps/${v.id}/shipments/${ship.id}/tracking",SsTrackingBody(ship.phienBan,"ADMIN-${suffix.uppercase()}-$i"),users[i].accessToken);ship=delivery.vanChuyen.single{it.id==ship.id}
            listOf("DaGui","DangVanChuyen","DaGiao").forEach{next->delivery=post("api/smart-swaps/${v.id}/shipments/${ship.id}/status",SsStatusBody(ship.phienBan,next,true),users[i].accessToken);ship=delivery.vanChuyen.single{it.id==ship.id}}
        }
        (0..1).forEach{i->delivery=get("api/smart-swaps/${v.id}/delivery",users[i].accessToken);val incoming=delivery.vanChuyen.single{it.nguoiNhanId==users[i].hoSo.id}
            delivery=post("api/smart-swaps/${v.id}/shipments/${incoming.id}/received",SsReceiveBody(delivery.phienBanThoaThuanId!!,true,true,true),users[i].accessToken)
            if(i==0)upload("api/smart-swaps/${v.id}/evidence",users[i].accessToken,mapOf("maYeuCau" to key()),"anh")}
        start();menu(QtScreen.TRADES);click("qt_tab_SmartSwap",true);input("qt_query",v.ma);closeSoftKeyboard();click("qt_search",true);idle();click("qt_trade_${v.giaoDichId}",true);ready("qt_trade_detail");Assert.assertEquals(2,vm.uiState.value.trade!!.giaoDich.soNguoiNhan);Assert.assertEquals(3,vm.uiState.value.trade!!.thanhVienCount());shot("smart-swap-two-of-three")
        val e=vm.uiState.value.trade!!.bangChung.first();click("qt_evidence_${e.id}",true);ready("qt_evidence_detail");shot("evidence-detail");click("qt_back");ready("qt_trade_detail")
        click("qt_complaint_create",true);ready("qt_complaint_create_form");click("qt_complaint_reporter",true);click("qt_complaint_reporter_${users[2].hoSo.id}");click("qt_complaint_accused",true);click("qt_complaint_accused_${users[0].hoSo.id}");input("qt_complaint_title","Đối chiếu thông tin sản phẩm");input("qt_complaint_text","Quản trị đã đối chiếu phản ánh các bên về phụ kiện bàn giao.");closeSoftKeyboard();click("qt_complaint_create_check",true);shot("create-dispute");click("qt_complaint_create_submit");ready("qt_complaint_detail");val issue=vm.uiState.value.complaint!!.khieuNai.id
        Assert.assertTrue(get<SsGiaoNhan>("api/smart-swaps/${v.id}/delivery",users[2].accessToken).tienDo.tamDung)
        click("qt_decision_open");ready("qt_decision");input("qt_decision_evidence","Cần đối chiếu thêm bằng chứng và lịch sử trao đổi của ba bên.");input("qt_decision_conclusion","Tiếp tục giữ tạm dừng để bổ sung");input("qt_decision_action","Các bên bổ sung ảnh và thông tin phụ kiện.");input("qt_decision_message","Nhóm tiếp tục tạm dừng, chưa xác nhận kết quả cuối cùng.");closeSoftKeyboard();click("qt_decision_direction",true);click("qt_decision_direction_GhiNhan");click("qt_decision_notify",true);click("qt_decision_submit");ready("qt_complaint_detail")
        click("qt_verify_open");ready("qt_verify");input("qt_verify_text","Đã kiểm tra việc yêu cầu bổ sung, vẫn cần quyết định cuối.");closeSoftKeyboard();click("qt_verify_check",true);click("qt_verify_submit");ready("qt_complaint_detail")
        Assert.assertEquals("DangXuLy",vm.uiState.value.complaint!!.khieuNai.trangThai)
        Assert.assertTrue(get<SsGiaoNhan>("api/smart-swaps/${v.id}/delivery",users[2].accessToken).tienDo.tamDung);shot("dispute-still-open")
        click("qt_decision_open");ready("qt_decision");input("qt_decision_evidence","Đã rà soát tin đăng, chat và bằng chứng từ cả ba.");input("qt_decision_conclusion","Các bên thống nhất tiếp tục giao nhận");input("qt_decision_action","Bổ sung đúng phụ kiện để người nhận tự kiểm tra.");input("qt_decision_message","Đã thống nhất phương án, các thành viên thực hiện và xác nhận đúng phần mình.");closeSoftKeyboard();click("qt_decision_notify",true);shot("dispute-decision");click("qt_decision_submit");ready("qt_complaint_detail");Assert.assertEquals("ChoThucHien",vm.uiState.value.complaint!!.khieuNai.trangThai)
        click("qt_verify_open");ready("qt_verify");input("qt_verify_text","Đã đối chiếu bằng chứng bổ sung phụ kiện với cả ba thành viên.");closeSoftKeyboard();click("qt_verify_check",true);shot("verify-execution");click("qt_verify_submit");ready("qt_complaint_detail");Assert.assertEquals("DaGiaiQuyet",vm.uiState.value.complaint!!.khieuNai.trangThai)
        delivery=get("api/smart-swaps/${v.id}/delivery",users[2].accessToken);Assert.assertEquals(2,delivery.tienDo.soNguoiNhanDung);Assert.assertFalse(delivery.tienDo.hoanTat);Assert.assertFalse(delivery.tienDo.tamDung)
        click("qt_complaint_trade",true);ready("qt_trade_detail");shot("resolved-still-two-of-three");Assert.assertEquals(2,vm.uiState.value.trade!!.giaoDich.soNguoiNhan)
        val incoming=delivery.vanChuyen.single{it.nguoiNhanId==users[2].hoSo.id};request("POST","api/smart-swaps/${v.id}/shipments/${incoming.id}/received",SsReceiveBody(delivery.phienBanThoaThuanId!!,true,true,true),expected=404)
        delivery=post("api/smart-swaps/${v.id}/shipments/${incoming.id}/received",SsReceiveBody(delivery.phienBanThoaThuanId!!,true,true,true),users[2].accessToken);Assert.assertTrue(delivery.tienDo.hoanTat);click("qt_reload");idle();Assert.assertEquals(3,vm.uiState.value.trade!!.giaoDich.soNguoiNhan);shot("completed-by-third-user")
        Assert.assertEquals("DaGiaiQuyet",get<QtComplaintDetail>("api/admin/khieu-nai/$issue").khieuNai.trangThai)
    }
    private fun QtTradeDetail.thanhVienCount()=giaoDich.thanhVien.size
}
