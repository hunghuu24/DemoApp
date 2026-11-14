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
import com.ute.app_ban_do_cu.feature.catalog.BanHangViewModel
import com.ute.app_ban_do_cu.feature.commerce.MuaBanViewModel
import com.ute.app_ban_do_cu.feature.admin.QuanTriViewModel
import kotlinx.coroutines.runBlocking
import okhttp3.*
import okhttp3.MediaType.Companion.toMediaType
import okhttp3.RequestBody.Companion.toRequestBody
import org.junit.*
import java.io.ByteArrayOutputStream
import java.io.File
import java.util.UUID
import java.util.concurrent.TimeUnit

/** Compose -> ViewModel -> authenticated Retrofit/Coil -> real HTTP -> SQL Server.
 * Other participants use their real JWTs; only photo-picker bytes are supplied to its callback. */
class MuaBanEndToEndTest {
    @get:Rule val compose = createEmptyComposeRule()
    private val app get() = ApplicationProvider.getApplicationContext<ReMarketApplication>()
    private val gson = Gson()
    private val http = OkHttpClient.Builder().callTimeout(45,TimeUnit.SECONDS).build()
    private var scenario: ActivityScenario<MainActivity>? = null
    private lateinit var vm: MuaBanViewModel
    private lateinit var catalog: BanHangViewModel
    private lateinit var auth: TaiKhoanViewModel
    private lateinit var management: QuanTriViewModel
    private lateinit var admin: String
    private lateinit var adminEmail: String
    private lateinit var adminPassword: String
    private lateinit var buyer: String
    private lateinit var seller: String
    private lateinit var listing: TinDang
    private val suffix = UUID.randomUUID().toString().replace("-","").take(12)
    private val password = "CommerceUi!2026"
    private val buyerEmail = "commerce-buyer-$suffix@example.test"
    private val sellerEmail = "commerce-seller-$suffix@example.test"
    private val png by lazy {
        val bitmap=Bitmap.createBitmap(240,160,Bitmap.Config.ARGB_8888).apply{eraseColor(Color.rgb(54,120,87))}
        ByteArrayOutputStream().use{bitmap.compress(Bitmap.CompressFormat.PNG,100,it);bitmap.recycle();it.toByteArray()}
    }
    private fun key()=UUID.randomUUID().toString()
    private fun request(method:String,path:String,body:Any?=null,token:String?=buyer,expected:Int=200):String {
        val builder=Request.Builder().url(BuildConfig.API_BASE_URL+path.removePrefix("/"))
        if(!token.isNullOrBlank())builder.header("Authorization","Bearer $token")
        val content=if(method=="GET")null else gson.toJson(body?:emptyMap<String,String>()).toRequestBody("application/json".toMediaType())
        http.newUiTestCall(builder.method(method,content).build()).execute().use{
            val text=it.body?.string().orEmpty()
            Assert.assertEquals("$method $path: $text",expected,it.code);return text
        }
    }
    private inline fun <reified T> post(path:String,body:Any,token:String=buyer,expected:Int=200):T = gson.fromJson(request("POST",path,body,token,expected),T::class.java)
    private inline fun <reified T> get(path:String,token:String=buyer):T = gson.fromJson(request("GET",path,token=token),T::class.java)
    private fun idle(){compose.waitUntil(60_000){!vm.uiState.value.busy&&!catalog.uiState.value.busy&&!auth.uiState.value.dangTai&&!management.uiState.value.busy};compose.waitForIdle()}
    private fun ready(tag:String){compose.waitUntil(60_000){compose.onAllNodesWithTag(tag).fetchSemanticsNodes().isNotEmpty()};idle()}
    private fun click(tag:String,scroll:Boolean=false){val node=compose.onNodeWithTag(tag);if(scroll)node.performScrollTo();node.performClick();compose.waitForIdle()}
    private fun input(tag:String,text:String){compose.onNodeWithTag(tag).performScrollTo().performTextReplacement(text)}
    private fun shot(name:String){
        closeSoftKeyboard();compose.waitForIdle()
        val bitmap=InstrumentationRegistry.getInstrumentation().uiAutomation.takeScreenshot()
        val file=File(app.getExternalFilesDir("qa-commerce"),"$name.png").apply{parentFile?.mkdirs()}
        file.outputStream().use{bitmap.compress(Bitmap.CompressFormat.PNG,100,it)};bitmap.recycle()
    }
    private fun startAs(email:String,pass:String=password,adminUser:Boolean=false){
        scenario?.close()
        runBlocking{app.container.phienStore.xoa();app.container.taiKhoanRepository.dangNhap(DangNhapBody(email,pass))}
        scenario=ActivityScenario.launch(MainActivity::class.java)
        scenario!!.onActivity{
            vm=ViewModelProvider(it)[MuaBanViewModel::class.java]
            catalog=ViewModelProvider(it)[BanHangViewModel::class.java]
            auth=ViewModelProvider(it)[TaiKhoanViewModel::class.java]
            management=ViewModelProvider(it)[QuanTriViewModel::class.java]
        }
        ready(if(adminUser)"admin_home" else "user_home")
    }
    @Before fun setup(){
        runBlocking{app.container.phienStore.xoa()}
        val args=InstrumentationRegistry.getArguments()
        adminEmail=requireNotNull(args.getString("adminEmail"));adminPassword=requireNotNull(args.getString("adminPassword"))
        admin=post<PhienDto>("api/taikhoan/dangnhap",DangNhapBody(adminEmail,adminPassword),token="").accessToken
        listOf(buyerEmail,sellerEmail).forEachIndexed{index,email->
            val phone="07"+(((suffix+index).hashCode().toLong() and 0x7fffffffL)%100_000_000L).toString().padStart(8,'0')
            request("POST","api/taikhoan/dangky",DangKyBody(if(index==0)"Người mua Android" else "Người bán Android",email,phone,password,password),null,201)
        }
        buyer=post<PhienDto>("api/taikhoan/dangnhap",DangNhapBody(buyerEmail,password),token="").accessToken
        seller=post<PhienDto>("api/taikhoan/dangnhap",DangNhapBody(sellerEmail,password),token="").accessToken
        val category=get<Array<DanhMucBanHang>>("api/tindang/danhmuc").first{it.danhMucChaId==null}.id
        listing=post("api/tindang",LuuTinBody("Bàn mua bán $suffix",category,500000,"NhuMoi","Bàn kiểm thử luồng mua bán trên Android, còn sử dụng tốt.","TP. Hồ Chí Minh","Quận 1, TP. Hồ Chí Minh",null),seller,201)
        val image=MultipartBody.Builder().setType(MultipartBody.FORM).addFormDataPart("phienBan",listing.phienBan).addFormDataPart("anh","test.png",png.toRequestBody("image/png".toMediaType())).build()
        http.newUiTestCall(Request.Builder().url(BuildConfig.API_BASE_URL+"api/tindang/${listing.id}/anh").header("Authorization","Bearer $seller").post(image).build()).execute().use{
            Assert.assertEquals(200,it.code);listing=gson.fromJson(it.body!!.string(),TinDang::class.java)
        }
        listing=post("api/tindang/${listing.id}/gui-duyet",PhienBanTinBody(listing.phienBan),seller)
        post<TinDang>("api/admin/tindang/${listing.id}/duyet",DuyetTinBody(listing.phienBan,true,null),admin)
    }
    @After fun cleanup(){
        // Release only this fixture's reservations; completed history remains for audit.
        if(::buyer.isInitialized&&::listing.isInitialized){
            get<TrangDonMua>("api/donhang?vaiTro=mua").items.filter{it.sanPham.tinDangId==listing.id&&it.coTheHuy}.forEach{
                post<DonMua>("api/donhang/${it.id}/huy",HuyDonBody(it.phienBan,"Dọn đơn kiểm thử Android"))
            }
            val current=get<TinDang>("api/tindang/${listing.id}/quanly",seller)
            if(current.trangThai!="DangGiuCho")request("DELETE","api/tindang/${listing.id}",PhienBanTinBody(current.phienBan),seller,204)
        }
        if(scenario!=null)runBlocking{app.container.taiKhoanRepository.dangXuat()}
        runBlocking{app.container.phienStore.xoa()};scenario?.close()
    }
    private fun searchProduct(){
        click("open_search",true);ready("catalog_results");input("catalog_query",suffix);closeSoftKeyboard();click("catalog_search");idle()
        click("listing_${listing.id}",true);ready("product_detail")
    }
    private fun shipThroughApi(id:String){
        var o=get<DonMua>("api/donhang/$id",seller)
        listOf("DaLayHang","DangVanChuyen","DaGiao").forEach{state->o=post("api/donhang/$id/vanchuyen",ShipBody(o.phienBan,state),seller)}
    }
    @Test fun nguoiMuaThuongLuongMuaNhanDanhGiaVaHoTro(){
        startAs(buyerEmail);searchProduct();click("contact_seller");ready("trade_chat")
        compose.onNodeWithTag("chat_message").performTextReplacement("Món đồ còn không? Tôi muốn thương lượng giá.");closeSoftKeyboard();click("chat_send");idle()
        scenario!!.onActivity{vm.guiAnh(png)};idle()
        click("chat_offer");ready("coupon_form");shot("coupon-form")
        input("coupon_amount","50000");closeSoftKeyboard();click("coupon_submit");ready("trade_chat")
        var chat=vm.uiState.value.chat!!;val coupon=chat.phieu.single()
        chat=post("api/hoithoai/${chat.id}/phieu/${coupon.id}",TraLoiPhieuBody(coupon.phienBan,true),seller)
        click("commerce_reload");idle();shot("chat-discount-accepted")
        click("buy_coupon_${coupon.id}");ready("checkout");Assert.assertEquals(50000L,vm.uiState.value.quote!!.soTienGiam);shot("checkout-top")
        click("checkout_submit");compose.onNodeWithTag("error_diaChi").assertExists()
        input("checkout_address","123 Nguyễn Văn Cừ, Quận 5, TP. Hồ Chí Minh");closeSoftKeyboard()
        click("checkout_coupon",true);click("checkout_coupon_");idle();Assert.assertEquals(0L,vm.uiState.value.quote!!.soTienGiam)
        click("checkout_coupon",true);click("checkout_coupon_${coupon.id}");idle();shot("checkout-discount")
        click("checkout_submit");ready("order_detail");val order=vm.uiState.value.order!!;Assert.assertEquals(480000L,order.tongTien);shot("order-pending")
        click("pay_order");click("payment_failure");idle();Assert.assertEquals("ThatBai",vm.uiState.value.order!!.thanhToan.trangThai)
        click("pay_order");click("payment_success");idle();Assert.assertEquals("DaThanhToan",vm.uiState.value.order!!.trangThai);shot("order-paid")
        shipThroughApi(order.id);click("commerce_reload");idle();shot("order-delivered")
        click("receive_order");click("receive_confirm");idle();Assert.assertEquals("DaNhan",vm.uiState.value.order!!.trangThai)
        click("rate_order");ready("order_rating");click("product_star_4");click("seller_star_5")
        input("rating_text","Sản phẩm đúng mô tả, người bán phản hồi nhanh.");closeSoftKeyboard();shot("rating-form")
        click("rating_submit");ready("rating_success");shot("rating-success");click("rating_back_order");ready("order_detail")
        Assert.assertNotNull(vm.uiState.value.order!!.danhGia)
        click("order_support");ready("support_form");input("support_title","Cần hỗ trợ kiểm tra đơn")
        input("support_description","Tôi cần đội ngũ kiểm tra thông tin của đơn hàng đã nhận.");closeSoftKeyboard();shot("support-form")
        click("support_submit");ready("support_detail");val ticket=vm.uiState.value.support!!
        input("support_reply","Tôi bổ sung thông tin để đội ngũ kiểm tra.");closeSoftKeyboard();click("support_reply_submit",true);idle()
        val latest=get<HoTroMua>("api/admin/hotro/${ticket.id}",admin)
        post<HoTroMua>("api/admin/hotro/${ticket.id}/phanhoi",PhanHoiHoTroBody(latest.phienBan,"Đã kiểm tra, thông tin đơn hàng chính xác.","DaGiaiQuyet"),admin)
        click("commerce_reload");idle();Assert.assertEquals("DaGiaiQuyet",vm.uiState.value.support!!.trangThai);shot("support-resolved")
        click("commerce_back");ready("order_detail");click("commerce_back");ready("product_detail");click("catalog_back");ready("catalog_results");click("catalog_back");ready("user_home")
        click("settings_tab");click("orders_row",true);ready("orders");click("order_status_DaNhan",true);idle();click("order_${order.id}",true);ready("order_detail");shot("order-completed")
    }
    @Test fun nguoiBanChapNhanGiaVaMoPhongVanChuyen(){
        var chat=post<HoiThoai>("api/hoithoai",MoChatBody(listing.id));chat=post("api/hoithoai/${chat.id}/phieu",TaoPhieuBody(key(),30000))
        startAs(sellerEmail);click("tab_3");ready("trade_inbox");shot("inbox-seller");click("chat_${chat.id}",true);ready("trade_chat")
        click("accept_coupon_${chat.phieu.single().id}");idle();Assert.assertEquals("DaChapNhan",vm.uiState.value.chat!!.phieu.single().trangThai)
        val q=post<BaoGia>("api/donhang/baogia",BaoGiaBody(listing.id,chat.phieu.single().id))
        var o=post<DonMua>("api/donhang",TaoDonBody(key(),listing.id,q.phienBanTin,q.phieuThuongLuongId,"Người mua Android","0901234567","123 Nguyễn Văn Cừ, TP. Hồ Chí Minh"),expected=201)
        o=post("api/donhang/${o.id}/thanhtoan",ThanhToanBody(key(),o.phienBan,true))
        click("commerce_back");ready("trade_inbox");click("settings_tab");click("orders_row",true);ready("orders");click("orders_ban");idle();shot("orders-seller")
        click("order_${o.id}",true);ready("order_detail");click("order_contact",true);ready("trade_chat");click("commerce_back");ready("order_detail")
        listOf("DaLayHang","DangVanChuyen","GiaoThatBai","DangVanChuyen","DaGiao").forEach{state->click("ship_$state");idle();Assert.assertEquals(state,vm.uiState.value.order!!.vanChuyen!!.trangThai)}
        compose.onNodeWithTag("receive_order").assertDoesNotExist();shot("seller-shipped")
        val delivered=get<DonMua>("api/donhang/${o.id}")
        post<DonMua>("api/donhang/${o.id}/nhanhang",PhienBanTinBody(delivered.phienBan))
    }
    @Test fun quanTriTiepNhanVaGiaiQuyetHoTro(){
        val ticket=post<HoTroMua>("api/hotro",HoTroBody(key(),null,"TaiKhoanKhac","Hỗ trợ tài khoản $suffix","Tôi cần được kiểm tra thông tin hồ sơ cá nhân trên ứng dụng."),expected=201)
        startAs(adminEmail,adminPassword,true);click("admin_support",true);ready("support_list");shot("admin-support-list")
        click("support_${ticket.id}",true);ready("support_detail");input("support_reply","Đội ngũ đã tiếp nhận và đang kiểm tra.");closeSoftKeyboard();click("support_process",true);idle()
        Assert.assertEquals("DangXuLy",vm.uiState.value.support!!.trangThai);shot("admin-support-processing")
        input("support_reply","Đã kiểm tra và giải quyết thông tin hồ sơ.");closeSoftKeyboard();click("support_resolve",true);idle();Assert.assertEquals("DaGiaiQuyet",vm.uiState.value.support!!.trangThai)
        click("commerce_back");ready("support_list");click("commerce_back");ready("admin_home")
    }
    @Test fun muaGiaGocHuyDonHoanTienVaHoTroChonDon(){
        startAs(buyerEmail);click("settings_tab");click("orders_row",true);ready("orders")
        compose.onNodeWithTag("orders_empty").assertExists();shot("orders-empty")
        click("commerce_back");ready("user_home");searchProduct();click("buy_product");ready("checkout")
        Assert.assertEquals(0L,vm.uiState.value.quote!!.soTienGiam)
        val automation=InstrumentationRegistry.getInstrumentation().uiAutomation
        try{
            // Await wm completion and replace the Compose root before synchronizing Espresso.
            // Querying the old root while display reconfiguration is in flight can stall its idle loop.
            automation.executeShellCommand("wm size 800x1000").use{java.io.FileInputStream(it.fileDescriptor).use{stream->stream.readBytes()}}
            scenario!!.recreate();compose.waitUntil(15_000){compose.onNodeWithTag("checkout").fetchSemanticsNode().size.width>=790};shot("checkout-wide")
        }finally{
            automation.executeShellCommand("wm size 360x800").use{java.io.FileInputStream(it.fileDescriptor).use{stream->stream.readBytes()}}
            scenario!!.recreate();compose.waitForIdle()
        }
        input("checkout_name","Người nhận demo");input("checkout_phone","123");input("checkout_address","456 Nguyễn Trãi, TP. Hồ Chí Minh");closeSoftKeyboard();click("checkout_submit")
        compose.onNodeWithTag("error_phone").assertExists()
        input("checkout_phone","0901234567");closeSoftKeyboard();click("payment_method",true)
        compose.onNodeWithText("Mô phỏng · không trừ tiền thật").performClick();click("checkout_submit");ready("order_detail")
        click("cancel_order");compose.onNodeWithTag("cancel_reason").performTextReplacement("Tôi muốn thay đổi thông tin mua hàng");closeSoftKeyboard();click("cancel_confirm");idle()
        Assert.assertEquals("DaHuy",vm.uiState.value.order!!.trangThai);shot("order-cancelled-unpaid")
        click("commerce_back");ready("product_detail");click("buy_product");ready("checkout")
        input("checkout_address","789 Nguyễn Trãi, TP. Hồ Chí Minh");closeSoftKeyboard();click("checkout_submit");ready("order_detail")
        click("pay_order");click("payment_success");idle();click("cancel_order");compose.onNodeWithTag("cancel_reason").performTextReplacement("Tôi không cần món đồ này nữa");closeSoftKeyboard();click("cancel_confirm");idle()
        Assert.assertEquals("HoanTien",vm.uiState.value.order!!.thanhToan.trangThai);shot("order-refunded")
        val order=vm.uiState.value.order!!
        click("commerce_back");ready("product_detail");click("catalog_back");ready("catalog_results");click("catalog_back");ready("user_home")
        click("settings_tab");click("support_row",true);ready("support_list");click("new_support");ready("support_form")
        click("support_choose_order",true);ready("orders");click("order_${order.id}",true);ready("support_form")
        Assert.assertEquals(order.id,vm.uiState.value.supportOrderId)
        input("support_title","Kiểm tra khoản hoàn mô phỏng");input("support_description","Vui lòng xác nhận khoản hoàn mô phỏng của đơn hàng đã hủy.");closeSoftKeyboard();click("support_submit");ready("support_detail")
        Assert.assertEquals(order.id,vm.uiState.value.support!!.donHangId)
        val ticket=vm.uiState.value.support!!
        click("commerce_back");ready("support_list");compose.onNodeWithTag("support_${ticket.id}").assertExists()
    }
}
