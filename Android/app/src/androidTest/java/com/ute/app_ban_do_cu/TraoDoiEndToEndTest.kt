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
import com.ute.app_ban_do_cu.feature.catalog.*
import com.ute.app_ban_do_cu.feature.commerce.MuaBanViewModel
import com.ute.app_ban_do_cu.feature.exchange.TraoDoiViewModel
import kotlinx.coroutines.runBlocking
import okhttp3.*
import okhttp3.MediaType.Companion.toMediaType
import okhttp3.RequestBody.Companion.toRequestBody
import org.junit.*
import java.io.ByteArrayOutputStream
import java.io.File
import java.util.UUID
import java.util.concurrent.TimeUnit

/** Two registered demo users exchange real uploaded listings through Compose/Retrofit/SQL.
 * Native picker result bytes are supplied to the same callback; repositories and HTTP are real. */
class TraoDoiEndToEndTest{
    @get:Rule val compose=createEmptyComposeRule()
    private val app get()=ApplicationProvider.getApplicationContext<ReMarketApplication>()
    private val gson=Gson();private val http=OkHttpClient.Builder().callTimeout(45,TimeUnit.SECONDS).build()
    private var scenario:ActivityScenario<MainActivity>?=null
    private lateinit var vm:TraoDoiViewModel;private lateinit var auth:TaiKhoanViewModel;private lateinit var catalog:BanHangViewModel;private lateinit var chat:MuaBanViewModel
    private lateinit var a:PhienDto;private lateinit var b:PhienDto;private lateinit var admin:String;private lateinit var category:String
    private val suffix=UUID.randomUUID().toString().replace("-","").take(10)
    private val emailA="exchange-demo-a-$suffix@example.test";private val emailB="exchange-demo-b-$suffix@example.test";private val password="ExchangeUi!2026"
    private val meetingDate=java.time.LocalDate.now(java.time.ZoneId.of("Asia/Ho_Chi_Minh")).plusDays(7).format(java.time.format.DateTimeFormatter.ofPattern("dd/MM/uuuu"))
    private val listings=mutableListOf<Pair<String,String>>();private val offers=mutableListOf<String>()
    private val png by lazy{val bitmap=Bitmap.createBitmap(240,160,Bitmap.Config.ARGB_8888).apply{eraseColor(Color.rgb(53,116,83))};ByteArrayOutputStream().use{bitmap.compress(Bitmap.CompressFormat.PNG,100,it);bitmap.recycle();it.toByteArray()}}
    private fun key()=UUID.randomUUID().toString()
    private fun request(method:String,path:String,body:Any?=null,token:String?=a.accessToken,expected:Int=200):String{
        val builder=Request.Builder().url(BuildConfig.API_BASE_URL+path.removePrefix("/"));if(!token.isNullOrBlank())builder.header("Authorization","Bearer $token")
        val payload=if(method=="GET")null else gson.toJson(body?:emptyMap<String,String>()).toRequestBody("application/json".toMediaType())
        http.newUiTestCall(builder.method(method,payload).build()).execute().use{val text=it.body?.string().orEmpty();Assert.assertEquals("$method $path: $text",expected,it.code);return text}
    }
    private inline fun <reified T> get(path:String,token:String=a.accessToken):T=gson.fromJson(request("GET",path,token=token),T::class.java)
    private inline fun <reified T> post(path:String,body:Any,token:String=a.accessToken,expected:Int=200):T=gson.fromJson(request("POST",path,body,token,expected),T::class.java)
    private fun idle(){compose.waitUntil(60_000){!vm.uiState.value.busy&&!auth.uiState.value.dangTai&&!catalog.uiState.value.busy&&!chat.uiState.value.busy};compose.waitForIdle()}
    private fun ready(tag:String){compose.waitUntil(60_000){compose.onAllNodesWithTag(tag).fetchSemanticsNodes().isNotEmpty()};idle()}
    private fun click(tag:String,scroll:Boolean=false){val node=compose.onNodeWithTag(tag);if(scroll)node.performScrollTo();node.performClick();compose.waitForIdle()}
    private fun input(tag:String,value:String){val node=compose.onNodeWithTag(tag);if(tag!="chat_message"&&tag!="exchange_action_reason")node.performScrollTo();node.performTextReplacement(value)}
    private fun shot(name:String){closeSoftKeyboard();compose.waitForIdle();val bmp=InstrumentationRegistry.getInstrumentation().uiAutomation.takeScreenshot();val file=File(app.getExternalFilesDir("qa-exchange"),"$name.png").apply{parentFile?.mkdirs()};file.outputStream().use{bmp.compress(Bitmap.CompressFormat.PNG,100,it)};bmp.recycle()}
    private fun startAs(second:Boolean=false){
        scenario?.close();runBlocking{if(app.container.taiKhoanRepository.conPhien())app.container.taiKhoanRepository.dangXuat();app.container.taiKhoanRepository.dangNhap(DangNhapBody(if(second)emailB else emailA,password))}
        scenario=ActivityScenario.launch(MainActivity::class.java);scenario!!.onActivity{vm=ViewModelProvider(it)[TraoDoiViewModel::class.java];auth=ViewModelProvider(it)[TaiKhoanViewModel::class.java];catalog=ViewModelProvider(it)[BanHangViewModel::class.java];chat=ViewModelProvider(it)[MuaBanViewModel::class.java]};ready("user_home")
    }
    @Before fun setup(){
        runBlocking{app.container.phienStore.xoa()};val args=InstrumentationRegistry.getArguments()
        admin=post<PhienDto>("api/taikhoan/dangnhap",DangNhapBody(requireNotNull(args.getString("adminEmail")),requireNotNull(args.getString("adminPassword"))),token="").accessToken
        listOf(emailA,emailB).forEachIndexed{index,email->val phone="06"+(((suffix+index).hashCode().toLong() and 0x7fffffffL)%100000000L).toString().padStart(8,'0');request("POST","api/taikhoan/dangky",DangKyBody(if(index==0)"Minh Anh demo" else "Hoàng Nam demo",email,phone,password,password),null,201)}
        a=post("api/taikhoan/dangnhap",DangNhapBody(emailA,password),token="");b=post("api/taikhoan/dangnhap",DangNhapBody(emailB,password),token="")
        category=get<Array<DanhMucBanHang>>("api/tindang/danhmuc").first{it.danhMucChaId==null}.id
    }
    @After fun cleanup(){
        // Only fixtures created by this test; preserve finalized records and FK history.
        offers.forEach{id->val o=get<DeNghiTraoDoi>("api/traodoi/$id");if(o.coTheHuy)post<DeNghiTraoDoi>("api/traodoi/$id/huy",HuyDeNghiBody(o.phienBan,"Kết thúc đề nghị kiểm thử"))}
        listings.forEach{(id,token)->val tin=get<TinDang>("api/tindang/$id/quanly",token);if(tin.trangThai!="DangGiuCho")request("DELETE","api/tindang/$id",PhienBanTinBody(tin.phienBan),token,204)}
        if(scenario!=null)runBlocking{app.container.taiKhoanRepository.dangXuat()};runBlocking{app.container.phienStore.xoa()};scenario?.close()
    }
    private fun published(second:Boolean,title:String,value:Long=1200000,mot:Boolean=true):TinDang{
        val token=if(second)b.accessToken else a.accessToken
        var tin=post<TinDang>("api/tindang",LuuTinBody("$title $suffix",category,value,"NhuMoi","Sản phẩm demo còn hoạt động tốt, có đầy đủ phụ kiện.","TP. Hồ Chí Minh","TP. Hồ Chí Minh",null,DieuKienTraoDoi("Muốn nhận đồ điện tử phù hợp hoặc sách máy đọc.",mot,true,true,1000000)),token,201);listings.add(tin.id to token)
        val body=MultipartBody.Builder().setType(MultipartBody.FORM).addFormDataPart("phienBan",tin.phienBan).addFormDataPart("anh","photo.png",png.toRequestBody("image/png".toMediaType())).build()
        http.newUiTestCall(Request.Builder().url(BuildConfig.API_BASE_URL+"api/tindang/${tin.id}/anh").header("Authorization","Bearer $token").post(body).build()).execute().use{Assert.assertEquals(200,it.code);tin=gson.fromJson(it.body!!.string(),TinDang::class.java)}
        tin=post("api/tindang/${tin.id}/gui-duyet",PhienBanTinBody(tin.phienBan),token)
        return post("api/admin/tindang/${tin.id}/duyet",DuyetTinBody(tin.phienBan,true,null),admin)
    }
    private fun openOffers(tab:String="gui"){click("settings_tab");click("exchanges_row",true);ready("exchange_offers_list");click("exchange_tab_$tab");idle()}
    private fun exploreTarget(tin:TinDang){click("tab_1");ready("exchange_explore");input("exchange_query",suffix);closeSoftKeyboard();click("exchange_search");idle();click("exchange_listing_${tin.id}",true);ready("exchange_detail")}
    private fun selectAndOffer(tin:TinDang,amount:String="0",appointment:Boolean=false){
        click("exchange_propose");ready("exchange_select");click("exchange_select_${tin.id}",true);click("exchange_select_continue");ready("exchange_offer_form")
        input("exchange_offer_amount",amount);input("exchange_offer_message","Mình muốn đổi món đồ của mình và sẽ kiểm tra trực tiếp khi gặp.")
        if(appointment){input("exchange_offer_date",meetingDate);input("exchange_offer_time","10:30")}
        closeSoftKeyboard();click("exchange_offer_terms",true);shot("create-offer");click("exchange_send_offer");ready("exchange_offer_detail");offers.add(vm.uiState.value.offer!!.id)
    }
    @Test fun haiTaiKhoanThuongLuongNhanDuVaDanhGia(){
        val own=published(false,"Sony demo");val target=published(true,"Kindle demo",1600000)
        startAs();exploreTarget(target);shot("product-detail");click("exchange_contact");ready("trade_chat")
        input("chat_message","Mình muốn hỏi thêm tình trạng của món đồ nhé.");closeSoftKeyboard();click("chat_send");idle();click("commerce_back");ready("exchange_detail")
        selectAndOffer(own,"300000",true);val id=vm.uiState.value.offer!!.id;Assert.assertEquals("ChoDongY",vm.uiState.value.offer!!.trangThaiGiaoDich);shot("offer-sent")
        click("exchange_chat");ready("trade_chat");input("chat_message","Mình đã gửi đề nghị bù 300.000đ.");closeSoftKeyboard();click("chat_send");idle();shot("exchange-chat");click("commerce_back");ready("exchange_offer_detail")
        startAs(true);openOffers("nhan");shot("offers-received");click("exchange_offer_$id",true);ready("exchange_offer_detail");click("exchange_counter_open");ready("exchange_counter")
        Assert.assertEquals(meetingDate,vm.uiState.value.date);Assert.assertEquals("10:30",vm.uiState.value.time)
        input("exchange_offer_amount","400000");input("exchange_offer_message","Kindle còn rất tốt, mình mong bạn bù 400.000đ nhé.");closeSoftKeyboard();shot("counter-form");click("exchange_counter_submit");ready("exchange_offer_detail")
        Assert.assertEquals(400000L,vm.uiState.value.offer!!.thoaThuan!!.soTienBu);compose.onNodeWithTag("exchange_accept").assertDoesNotExist()
        startAs();openOffers("thuongluong");click("exchange_offer_$id",true);ready("exchange_offer_detail");shot("counter-awaiting-response");click("exchange_accept");idle()
        Assert.assertEquals("DaChot",vm.uiState.value.offer!!.thoaThuan!!.trangThai);shot("agreement-accepted")
        click("exchange_handover_open");ready("exchange_handover");click("exchange_confirm_received");compose.onNodeWithTag("exchange_error").assertExists()
        click("exchange_check_item",true);click("exchange_check_received",true);scenario!!.onActivity{vm.themBangChung(listOf(AnhChon("qa-proof://$suffix",png)))};shot("handover-form")
        click("exchange_confirm_received");ready("exchange_offer_detail");Assert.assertEquals("DangThucHien",vm.uiState.value.offer!!.trangThaiGiaoDich);Assert.assertEquals(1,vm.uiState.value.offer!!.bangChung.size);compose.onNodeWithTag("exchange_rate_open").assertDoesNotExist();shot("waiting-partner")
        startAs(true);openOffers("nhan");click("exchange_offer_$id",true);ready("exchange_offer_detail");click("exchange_handover_open");ready("exchange_handover");click("exchange_check_item",true);click("exchange_check_received",true);click("exchange_confirm_received");ready("exchange_offer_detail")
        Assert.assertEquals("HoanTat",vm.uiState.value.offer!!.trangThaiGiaoDich);shot("completed")
        click("exchange_rate_open");ready("exchange_rating");click("exchange_star_5");click("exchange_tag_DUNG_MO_TA",true);click("exchange_tag_DUNG_HEN",true);input("exchange_rating_text","Minh Anh đúng hẹn, tai nghe đúng mô tả và giao tiếp thân thiện.");closeSoftKeyboard();shot("review-form");click("exchange_rating_submit");ready("exchange_rating_success");shot("review-success");click("exchange_rating_history");ready("exchange_offers_list")
        startAs();openOffers("hoantat");click("exchange_offer_$id",true);ready("exchange_offer_detail");click("exchange_rate_open");ready("exchange_rating");click("exchange_star_4");input("exchange_rating_text","Nam chuẩn bị đầy đủ phụ kiện, máy đọc sách hoạt động tốt.");closeSoftKeyboard();click("exchange_rating_submit");ready("exchange_rating_success")
        Assert.assertEquals(2,vm.uiState.value.offer!!.danhGia.size);val final=get<DeNghiTraoDoi>("api/traodoi/$id");Assert.assertTrue(final.xacNhan.all{it.daNhan});Assert.assertTrue(final.thoaThuan!!.nguoiTraDaXacNhan&&final.thoaThuan!!.nguoiNhanDaXacNhan)
    }
    @Test fun dangTinSuaAnXoaVaHaiCoCheTraoDoi(){
        startAs();click("tab_1");ready("exchange_explore");shot("explore");click("exchange_my_listings",true);ready("exchange_mine");click("exchange_new_listing");ready("exchange_listing_form");shot("post-form-top")
        click("exchange_publish",true);compose.onNodeWithTag("error_ten").assertExists()
        input("exchange_name","Tai nghe demo $suffix");click("exchange_category",true);click("exchange_category_$category");input("exchange_description","Tai nghe demo còn sử dụng tốt, đủ phụ kiện và có hộp.")
        input("exchange_value","1200000");input("exchange_wanted","Muốn đổi máy đọc sách Kindle hoặc máy đọc tương đương.");input("exchange_max_compensation","600000");input("exchange_city","TP. Hồ Chí Minh");closeSoftKeyboard()
        click("exchange_allow_one",true);click("exchange_allow_smart",true);click("exchange_publish",true);compose.onNodeWithTag("error_hinhThuc").assertExists()
        click("exchange_allow_one",true);click("exchange_allow_smart",true);scenario!!.onActivity{vm.themAnh(listOf(AnhChon("qa-photo://$suffix",png)))}
        shot("post-form-bottom");click("exchange_save_draft",true);ready("exchange_detail");val id=vm.uiState.value.detail!!.id;listings.add(id to a.accessToken);Assert.assertEquals("BanNhap",vm.uiState.value.detail!!.trangThai);request("GET","api/tindang/$id",expected=404)
        click("exchange_edit_listing");ready("exchange_listing_form");input("exchange_note","Ưu tiên trao đổi trực tiếp và kiểm tra sản phẩm trước.");closeSoftKeyboard();click("exchange_publish",true);ready("exchange_listing_success");shot("listing-pending")
        var tin=vm.uiState.value.detail!!;post<TinDang>("api/admin/tindang/$id/duyet",DuyetTinBody(tin.phienBan,true,null),admin)
        click("exchange_success_mine");ready("exchange_mine");shot("my-listings");click("exchange_mine_$id",true);ready("exchange_detail");Assert.assertTrue(vm.uiState.value.detail!!.traoDoi!!.choPhepMotMot&&vm.uiState.value.detail!!.traoDoi!!.choPhepSmartSwap)
        click("exchange_hide_listing");click("exchange_action_confirm");idle();Assert.assertEquals("DaAn",vm.uiState.value.detail!!.trangThai);request("GET","api/tindang/$id",expected=404)
        click("exchange_delete_listing");click("exchange_action_confirm");ready("exchange_mine");request("GET","api/tindang/$id/quanly",expected=404);listings.removeAll{it.first==id}
    }
    @Test fun boLocChonMonHopLeTuChoiVaHuy(){
        val unavailable=published(false,"Chỉ ba bên",500000,false);val target=published(true,"Món muốn nhận",1600000)
        startAs();click("tab_1");ready("exchange_explore");input("exchange_query",suffix);closeSoftKeyboard();click("exchange_search");idle();Assert.assertEquals(2,vm.uiState.value.results!!.tongSo)
        click("exchange_filters");input("exchange_filter_max","1000000");closeSoftKeyboard();click("exchange_filter_apply",true);idle();Assert.assertEquals(1,vm.uiState.value.results!!.tongSo);shot("search-filter")
        click("exchange_filters");click("exchange_filter_clear",true);idle();click("exchange_listing_${target.id}",true);ready("exchange_detail");click("exchange_propose");ready("exchange_select");compose.onNodeWithTag("exchange_select_empty").assertExists();shot("selector-empty")
        val own=published(false,"Món có thể đổi");click("exchange_back");ready("exchange_detail");selectAndOffer(own);val id=vm.uiState.value.offer!!.id
        startAs(true);openOffers("nhan");click("exchange_offer_$id",true);ready("exchange_offer_detail");click("exchange_reject");input("exchange_action_reason","Mình chưa phù hợp món đồ này");closeSoftKeyboard();click("exchange_action_confirm");idle();Assert.assertEquals("TuChoi",vm.uiState.value.offer!!.trangThai)
        val req=TaoDeNghiBody(key(),own.id,target.id,own.phienBan,target.phienBan,0,a.hoSo.id,"Mình muốn gửi lại đề nghị không có bù tiền.","TP. Hồ Chí Minh",null,true)
        val next=post<DeNghiTraoDoi>("api/traodoi",req,expected=201);offers.add(next.id);val accepted=post<DeNghiTraoDoi>("api/traodoi/${next.id}/phanhoi",PhanHoiDeNghiBody(next.phienBan,true),b.accessToken)
        startAs();openOffers();click("exchange_offer_${accepted.id}",true);ready("exchange_offer_detail");click("exchange_cancel");input("exchange_action_reason","Hủy trước khi hai bên gặp và bàn giao");closeSoftKeyboard();click("exchange_action_confirm");idle();Assert.assertEquals("DaHuy",vm.uiState.value.offer!!.trangThai);shot("offer-cancelled")
        request("GET","api/tindang/${own.id}");request("GET","api/tindang/${target.id}")
    }
}
