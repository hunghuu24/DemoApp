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
import com.ute.app_ban_do_cu.feature.auth.TaiKhoanViewModel
import com.ute.app_ban_do_cu.feature.smartswap.SmartSwapViewModel
import com.ute.app_ban_do_cu.feature.catalog.BanHangViewModel
import com.ute.app_ban_do_cu.feature.exchange.TraoDoiViewModel
import com.ute.app_ban_do_cu.feature.commerce.MuaBanViewModel
import kotlinx.coroutines.runBlocking
import okhttp3.*
import okhttp3.MediaType.Companion.toMediaType
import okhttp3.RequestBody.Companion.toRequestBody
import org.junit.*
import java.io.ByteArrayOutputStream
import java.io.File
import java.util.UUID
import java.util.concurrent.TimeUnit

/** Three independently registered JWT users. All listings, cycles, chat, shipments and reviews use real HTTP/SQL. */
class SmartSwapEndToEndTest {
    @get:Rule val compose=createEmptyComposeRule()
    private val app get()=ApplicationProvider.getApplicationContext<ReMarketApplication>()
    private val gson=Gson();private val http=OkHttpClient.Builder().callTimeout(45,TimeUnit.SECONDS).build()
    private var scenario:ActivityScenario<MainActivity>?=null;private lateinit var vm:SmartSwapViewModel;private lateinit var auth:TaiKhoanViewModel
    private lateinit var catalog:BanHangViewModel;private lateinit var exchange:TraoDoiViewModel;private lateinit var commerce:MuaBanViewModel
    private val suffix=UUID.randomUUID().toString().replace("-","").take(9);private val password="SmartUi!2026"
    private val emails=(0..2).map{"smart-ui-$it-$suffix@example.test"};private lateinit var users:List<PhienDto>;private lateinit var admin:String
    private val tins=mutableListOf<TinDang>();private val circles=mutableListOf<String>()
    private val png by lazy{val bitmap=Bitmap.createBitmap(180,120,Bitmap.Config.ARGB_8888).apply{eraseColor(Color.rgb(45,114,80))};ByteArrayOutputStream().use{bitmap.compress(Bitmap.CompressFormat.PNG,100,it);bitmap.recycle();it.toByteArray()}}
    private fun key()=UUID.randomUUID().toString()
    private fun request(method:String,path:String,body:Any?=null,token:String="",expected:Int=200):String{
        val b=Request.Builder().url(BuildConfig.API_BASE_URL+path.removePrefix("/"));if(token.isNotBlank())b.header("Authorization","Bearer $token")
        val payload=if(method=="GET")null else gson.toJson(body?:emptyMap<String,String>()).toRequestBody("application/json".toMediaType())
        http.newUiTestCall(b.method(method,payload).build()).execute().use{val text=it.body?.string().orEmpty();Assert.assertEquals("$method $path: $text",expected,it.code);return text}
    }
    private inline fun <reified T> get(path:String,actor:Int=0):T=gson.fromJson(request("GET",path,token=users[actor].accessToken),T::class.java)
    private inline fun <reified T> post(path:String,body:Any,actor:Int=0,expected:Int=200):T=gson.fromJson(request("POST",path,body,users[actor].accessToken,expected),T::class.java)
    private inline fun <reified T> put(path:String,body:Any,actor:Int=0):T=gson.fromJson(request("PUT",path,body,users[actor].accessToken),T::class.java)
    private fun idle(){compose.waitUntil(60_000){!vm.uiState.value.busy&&!auth.uiState.value.dangTai&&!catalog.uiState.value.busy&&!exchange.uiState.value.busy&&!commerce.uiState.value.busy};compose.waitForIdle()}
    private fun ready(tag:String){compose.waitUntil(60_000){compose.onAllNodesWithTag(tag).fetchSemanticsNodes().isNotEmpty()};idle()}
    private fun click(tag:String,scroll:Boolean=false){val n=compose.onNodeWithTag(tag);if(scroll)n.performScrollTo();n.performClick();compose.waitForIdle()}
    private fun input(tag:String,value:String,scroll:Boolean=true){val n=compose.onNodeWithTag(tag);if(scroll)n.performScrollTo();n.performTextReplacement(value)}
    private fun shot(name:String){closeSoftKeyboard();compose.waitForIdle();val bitmap=InstrumentationRegistry.getInstrumentation().uiAutomation.takeScreenshot();val f=File(app.getExternalFilesDir("qa-smart-swap"),"$name.png").apply{parentFile?.mkdirs()};f.outputStream().use{bitmap.compress(Bitmap.CompressFormat.PNG,100,it)};bitmap.recycle()}
    private fun start(actor:Int){scenario?.close();runBlocking{if(app.container.taiKhoanRepository.conPhien())app.container.taiKhoanRepository.dangXuat();app.container.taiKhoanRepository.dangNhap(DangNhapBody(emails[actor],password))}
        scenario=ActivityScenario.launch(MainActivity::class.java);scenario!!.onActivity{vm=ViewModelProvider(it)[SmartSwapViewModel::class.java];auth=ViewModelProvider(it)[TaiKhoanViewModel::class.java];catalog=ViewModelProvider(it)[BanHangViewModel::class.java];exchange=ViewModelProvider(it)[TraoDoiViewModel::class.java];commerce=ViewModelProvider(it)[MuaBanViewModel::class.java]};ready("user_home")}
    private fun open(id:String){click("settings_tab");click("smart_swaps_row",true);ready("ss_list");click("ss_cycle_$id",true);ready("ss_detail")}
    private fun agree(){click("ss_consent_open");ready("ss_consent");click("ss_response_consent",true);Assert.assertTrue(vm.uiState.value.checked);click("ss_accept");idle();Assert.assertNull(vm.uiState.value.error)}
    @Before fun setup(){runBlocking{app.container.phienStore.xoa()};val args=InstrumentationRegistry.getArguments()
        admin=gson.fromJson(request("POST","api/taikhoan/dangnhap",DangNhapBody(requireNotNull(args.getString("adminEmail")),requireNotNull(args.getString("adminPassword")))),PhienDto::class.java).accessToken
        emails.forEachIndexed{i,email->request("POST","api/taikhoan/dangky",DangKyBody("Swap ${listOf("Anh","Nam","Vy")[i]}",email,"07"+((suffix+i).hashCode().toLong().and(0x7fffffff)%100000000).toString().padStart(8,'0'),password,password),expected=201)}
        users=emails.map{gson.fromJson(request("POST","api/taikhoan/dangnhap",DangNhapBody(it,password)),PhienDto::class.java)}
        val category=get<Array<DanhMucBanHang>>("api/tindang/danhmuc").first{it.danhMucChaId==null}.id
        val titles=listOf("iPhone","Laptop","iPad");val values=listOf(10_000_000L,12_000_000L,8_000_000L)
        users.forEachIndexed{i,user->
            val f=LuuTinBody("${titles[i]} $suffix",category,values[i],"NhuMoi","Sản phẩm kiểm thử hoạt động tốt, đầy đủ phụ kiện.","Hà Nội","Hà Nội",null,DieuKienTraoDoi("Muốn nhận ${titles[(i+1)%3]} $suffix",true,true,true,2_000_000))
            var tin=post<TinDang>("api/tindang",f,i,201)
            val body=MultipartBody.Builder().setType(MultipartBody.FORM).addFormDataPart("phienBan",tin.phienBan).addFormDataPart("anh","qa.png",png.toRequestBody("image/png".toMediaType())).build()
            http.newUiTestCall(Request.Builder().url(BuildConfig.API_BASE_URL+"api/tindang/${tin.id}/anh").header("Authorization","Bearer ${user.accessToken}").post(body).build()).execute().use{Assert.assertEquals(200,it.code);tin=gson.fromJson(it.body!!.string(),TinDang::class.java)}
            tin=post("api/tindang/${tin.id}/gui-duyet",PhienBanTinBody(tin.phienBan),i)
            tins.add(gson.fromJson(request("POST","api/admin/tindang/${tin.id}/duyet",DuyetTinBody(tin.phienBan,true,null),admin),TinDang::class.java))
        }
    }
    @After fun cleanup(){
        circles.forEach{id->val v=get<SsVong>("api/smart-swaps/$id");if(v.coTheHuy)post<SsVong>("api/smart-swaps/$id/cancel",SsHuyBody(v.phuongAnHienTai!!.id,"Kết thúc dữ liệu kiểm thử giao diện."))}
        tins.forEachIndexed{i,tin->val row=get<TinDang>("api/tindang/${tin.id}/quanly",i);if(row.trangThai!="DangGiuCho")request("DELETE","api/tindang/${tin.id}",PhienBanTinBody(row.phienBan),users[i].accessToken,204)}
        if(scenario!=null)runBlocking{app.container.taiKhoanRepository.dangXuat()};runBlocking{app.container.phienStore.xoa()};scenario?.close()
    }
    private fun circle(official:Boolean=false):SsVong{
        val found=get<SsKetQua>("api/smart-swaps/search?tinDangId=${tins[0].id}").items.single();val p=post<SsPreview>("api/smart-swaps/preview",SsPreviewBody(found.maDeXuat))
        var v=post<SsVong>("api/smart-swaps/invitations",SsMoiBody(key(),found.maDeXuat,p.bamPhuongAn,"Cùng trao đổi ba sản phẩm và kiểm tra khi giao nhận.",true),expected=201);circles.add(v.id)
        if(official)(1..2).forEach{v=post("api/smart-swaps/${v.id}/responses",SsPhanHoiBody(v.phuongAnHienTai!!.id,true,true),it)};return v
    }
    private fun contact(){click("ss_contact_open",true);ready("ss_contact");input("ss_contact_phone","0901234567");input("ss_contact_from","123 Đường Gửi, Hà Nội");input("ss_contact_to","456 Đường Nhận, Hà Nội");closeSoftKeyboard();click("ss_contact_check",true);shot("delivery-contact");click("ss_contact_submit");ready("ss_delivery");Assert.assertNull(vm.uiState.value.error)}
    private fun outgoing(actor:Int){val ship=vm.uiState.value.delivery!!.vanChuyen.single{it.nguoiGuiId==users[actor].hoSo.id}
        input("ss_tracking_${ship.id}","UI-${suffix.uppercase()}-$actor");closeSoftKeyboard();click("ss_tracking_save_${ship.id}",true);idle()
        click("ss_ship_next_${ship.id}",true);click("ss_dispatch_confirm");idle();click("ss_ship_next_${ship.id}",true);idle();click("ss_ship_next_${ship.id}",true);idle();Assert.assertNull(vm.uiState.value.error)
    }
    private fun receive(){click("ss_check_item",true);click("ss_check_received",true);click("ss_check_money",true);click("ss_receive_submit");idle();Assert.assertNull(vm.uiState.value.error)}
    @Test fun baTaiKhoanTimThuongLuongChatGiaoNhanVaHaiDanhGia(){
        start(0);click("tab_1");ready("exchange_explore");click("smart_swap_entry",true);ready("ss_find");shot("find-empty-selection")
        click("ss_search_submit");compose.onNodeWithTag("ss_error").assertExists()
        click("ss_select_product",true);ready("ss_select");click("ss_product_${tins[0].id}",true);ready("ss_find")
        input("ss_wanted","Camera $suffix");closeSoftKeyboard();click("ss_search_submit");ready("ss_empty");shot("no-matching-cycle")
        click("ss_back");ready("ss_find");input("ss_wanted","Laptop $suffix");closeSoftKeyboard();shot("find-cycle");click("ss_search_submit");ready("ss_matches")
        Assert.assertEquals(1,vm.uiState.value.results!!.tongSo);Assert.assertEquals(3,vm.uiState.value.results!!.hoSo!!.size);shot("matching-cycles")
        click("ss_match_open_0",true);ready("ss_preview");shot("cycle-preview");Assert.assertEquals(0,get<SsTrangVong>("api/smart-swaps").tongSo)
        input("ss_invitation_message","Mình muốn cùng hai bạn trao đổi và kiểm tra sản phẩm khi nhận.");closeSoftKeyboard();click("ss_invitation_consent",true);click("ss_invite_submit");ready("ss_detail");val id=vm.uiState.value.circle!!.id;circles.add(id);shot("invitation-waiting")
        click("ss_chat_open");ready("ss_chat");input("ss_chat_message","Mình đề xuất điều chỉnh tiền bù trước khi chốt nhé.",false);closeSoftKeyboard();click("ss_chat_send");idle();shot("group-chat")
        click("ss_chat_adjust");ready("ss_compensation");click("ss_money_partner",true);click("ss_money_partner_${users[1].hoSo.id}")
        input("ss_money_amount","1500000");closeSoftKeyboard();click("ss_money_apply",true);input("ss_proposal_message","Mình xin bù Nam 1.500.000đ, khoản của Vy giữ nguyên.");closeSoftKeyboard();shot("compensation-sheet");click("ss_proposal_submit");ready("ss_consent")
        Assert.assertEquals(2,vm.uiState.value.circle!!.phuongAnHienTai!!.soPhienBan);Assert.assertEquals(0,vm.uiState.value.circle!!.phuongAnHienTai!!.dongY.count{it.quyetDinh=="DongY"})
        click("ss_response_consent",true);Assert.assertTrue(vm.uiState.value.checked);click("ss_accept");idle();Assert.assertNull(vm.uiState.value.error);Assert.assertEquals(1,vm.uiState.value.circle!!.phuongAnHienTai!!.dongY.count{it.quyetDinh=="DongY"})
        start(1);open(id);agree();shot("two-of-three-consents");Assert.assertEquals(2,vm.uiState.value.circle!!.phuongAnHienTai!!.dongY.count{it.quyetDinh=="DongY"})
        start(2);open(id);agree();Assert.assertEquals("SanSangBanGiao",vm.uiState.value.circle!!.trangThai);shot("three-consents")
        click("ss_consent_delivery");ready("ss_delivery");contact()
        start(1);open(id);click("ss_delivery_open");ready("ss_delivery");contact()
        start(0);open(id);click("ss_delivery_open");ready("ss_delivery");contact();click("ss_book_shipments");idle();Assert.assertEquals(3,vm.uiState.value.delivery!!.vanChuyen.size);outgoing(0);shot("shipping-tracking")
        start(1);open(id);click("ss_delivery_open");ready("ss_delivery");outgoing(1)
        start(2);open(id);click("ss_delivery_open");ready("ss_delivery");outgoing(2);receive();Assert.assertEquals("1/3",vm.uiState.value.delivery!!.tienDo.tienDo);shot("one-of-three-received")
        start(1);open(id);click("ss_delivery_open");ready("ss_delivery");receive();Assert.assertEquals("2/3",vm.uiState.value.delivery!!.tienDo.tienDo);Assert.assertFalse(vm.uiState.value.delivery!!.tienDo.hoanTat);compose.onNodeWithTag("ss_review_open").assertDoesNotExist();shot("two-of-three-not-complete")
        start(0);open(id);click("ss_delivery_open");ready("ss_delivery");click("ss_receive_submit");compose.onNodeWithTag("ss_error").assertExists();scenario!!.onActivity{vm.addEvidence(png)};receive()
        Assert.assertEquals("3/3",vm.uiState.value.delivery!!.tienDo.tienDo);Assert.assertTrue(vm.uiState.value.delivery!!.tienDo.hoanTat);Assert.assertEquals(1,vm.uiState.value.delivery!!.bangChung!!.size);shot("three-of-three-completed")
        click("ss_review_open");ready("ss_review");users.drop(1).forEach{u->click("ss_star_${u.hoSo.id}_5",true);input("ss_review_text_${u.hoSo.id}","Thành viên đúng hẹn, sản phẩm phù hợp và hỗ trợ kiểm tra.")};closeSoftKeyboard();shot("two-member-reviews");click("ss_review_submit");ready("ss_review_success");shot("reviews-persisted")
        Assert.assertEquals(2,get<Array<SsReview>>("api/smart-swaps/$id/reviews").size)
        click("ss_back");ready("ss_delivery");click("ss_back");ready("ss_detail");click("ss_back");ready("ss_list");click("ss_back");ready("settings")
    }
    @Test fun tuChoiThongBaoVaBaoSuCoTamDung(){
        val refused=circle();start(1);open(refused.id);click("ss_consent_open");ready("ss_consent");click("ss_reject_open");input("ss_action_reason","Mình chưa thể tham gia vòng trao đổi này.",false);closeSoftKeyboard();click("ss_action_confirm");idle();Assert.assertEquals("DaHuy",vm.uiState.value.circle!!.trangThai);shot("rejected-circle")
        start(0);click("settings_tab");click("smart_swaps_row",true);ready("ss_list");click("ss_list_notifications");ready("ss_notification_list");val n=vm.uiState.value.notifications!!.first{it.vongId==refused.id};click("ss_notification_${n.id}",true);ready("ss_detail");Assert.assertEquals("TuChoi",vm.uiState.value.circle!!.maKetThuc);shot("status-notification")
        val v=circle(true);(0..2).forEach{actor->put<SsGiaoNhan>("api/smart-swaps/${v.id}/delivery/contact",SsContactBody(key(),v.phuongAnHienTai!!.id,users[actor].hoSo.hoTen,"0901234567","123 Đường Gửi, Hà Nội","456 Đường Nhận, Hà Nội",true,null),actor)}
        var delivery=post<SsGiaoNhan>("api/smart-swaps/${v.id}/shipments",SsBookBody(v.phuongAnHienTai!!.id))
        delivery.vanChuyen.forEach{original->val actor=users.indexOfFirst{it.hoSo.id==original.nguoiGuiId};var ship=original
            delivery=put("api/smart-swaps/${v.id}/shipments/${ship.id}/tracking",SsTrackingBody(ship.phienBan,"ISSUE-${ship.id.take(8)}"),actor);ship=delivery.vanChuyen.first{it.id==ship.id}
            listOf("DaGui","DangVanChuyen","DaGiao").forEach{next->delivery=post("api/smart-swaps/${v.id}/shipments/${ship.id}/status",SsStatusBody(ship.phienBan,next,true),actor);ship=delivery.vanChuyen.first{it.id==ship.id}}
        }
        start(1);open(v.id);click("ss_delivery_open");ready("ss_delivery");val incoming=vm.uiState.value.delivery!!.vanChuyen.single{it.nguoiNhanId==users[1].hoSo.id};click("ss_issue_open_${incoming.id}",true);ready("ss_issue")
        input("ss_issue_title","Món nhận có vấn đề cần kiểm tra");input("ss_issue_text","Mình nhận sản phẩm khác so với mô tả, cần nhóm và quản trị hỗ trợ.");closeSoftKeyboard();shot("report-issue-form");click("ss_issue_submit");ready("ss_delivery");Assert.assertTrue(vm.uiState.value.delivery!!.tienDo.tamDung);compose.onNodeWithTag("ss_receive_submit").assertDoesNotExist();shot("paused-circle")
        click("ss_delivery_chat");ready("ss_chat");input("ss_chat_message","Mình đã báo sự cố, nhóm cùng kiểm tra giúp nhé.",false);closeSoftKeyboard();click("ss_chat_send");idle();Assert.assertNull(vm.uiState.value.error)
    }
    @Test fun boLocThatSoDoVaHuyDeNghiQuayLai(){
        start(0);click("tab_1");ready("exchange_explore");click("smart_swap_entry",true);ready("ss_find");click("ss_select_product",true);ready("ss_select");click("ss_product_${tins[0].id}",true);ready("ss_find")
        input("ss_search_cap","0");closeSoftKeyboard();click("ss_search_submit");ready("ss_empty");click("ss_back");ready("ss_find")
        input("ss_search_cap","2000000");closeSoftKeyboard();click("ss_verified",true);click("ss_search_submit");idle();compose.onNodeWithTag("ss_error").assertExists()
        click("ss_back");ready("ss_find");click("ss_verified",true);click("ss_search_condition",true);click("ss_search_condition_NhuMoi");click("ss_search_submit");ready("ss_matches");Assert.assertEquals(1,vm.uiState.value.results!!.tongSo)
        click("ss_match_open_0",true);ready("ss_preview");shot("diagram-direction-corrected");click("ss_back");ready("ss_matches");click("ss_back");ready("ss_find");click("ss_back");ready("exchange_explore")
        click("settings_tab");ready("settings");val v=circle();click("smart_swaps_row",true);ready("ss_list");click("ss_cycle_${v.id}",true);ready("ss_detail");click("ss_cancel_open")
        input("ss_action_reason","Mình chủ động hủy đề nghị trước khi bàn giao.",false);closeSoftKeyboard();click("ss_action_confirm");idle();Assert.assertEquals("DaHuy",vm.uiState.value.circle!!.trangThai);shot("cancelled-circle")
        click("ss_back");ready("ss_list");click("ss_back");ready("settings")
    }
}
