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
import com.ute.app_ban_do_cu.feature.catalog.*
import com.ute.app_ban_do_cu.feature.exchange.TraoDoiViewModel
import com.ute.app_ban_do_cu.feature.commerce.MuaBanViewModel
import com.ute.app_ban_do_cu.feature.smartswap.SmartSwapViewModel
import kotlinx.coroutines.runBlocking
import okhttp3.*
import okhttp3.MediaType.Companion.toMediaType
import okhttp3.RequestBody.Companion.toRequestBody
import org.junit.*
import java.io.ByteArrayOutputStream
import java.io.File
import java.util.UUID
import java.util.concurrent.TimeUnit

/** Real Compose + Retrofit/Coil + HTTP + SQL. Only the platform photo-picker result is supplied to its ViewModel callback. */
class SanPhamEndToEndTest {
    @get:Rule val compose = createEmptyComposeRule()
    private val app get() = ApplicationProvider.getApplicationContext<ReMarketApplication>()
    private val http = OkHttpClient.Builder().callTimeout(45, TimeUnit.SECONDS).build()
    private val gson = Gson()
    private var scenario: ActivityScenario<MainActivity>? = null
    private lateinit var vm: BanHangViewModel
    private lateinit var exchange:TraoDoiViewModel
    private lateinit var commerce:MuaBanViewModel
    private lateinit var smart:SmartSwapViewModel
    private lateinit var adminToken: String
    private lateinit var category: String
    private val createdIds = mutableListOf<String>()
    private val suffix = UUID.randomUUID().toString().replace("-", "").take(12)
    private val png by lazy {
        val bitmap = Bitmap.createBitmap(240, 160, Bitmap.Config.ARGB_8888).apply { eraseColor(Color.rgb(73, 113, 82)) }
        ByteArrayOutputStream().use { output -> bitmap.compress(Bitmap.CompressFormat.PNG, 100, output); bitmap.recycle(); output.toByteArray() }
    }
    private fun request(method: String, path: String, body: Any? = null, token: String? = app.container.phienStore.hienTai.get()?.accessToken, expected: Int = 200): String {
        val builder = Request.Builder().url(BuildConfig.API_BASE_URL + path.removePrefix("/"))
        if (token != null) builder.header("Authorization", "Bearer $token")
        val content = if (method in listOf("GET", "HEAD")) null else gson.toJson(body ?: emptyMap<String, String>()).toRequestBody("application/json".toMediaType())
        http.newUiTestCall(builder.method(method, content).build()).execute().use {
            Assert.assertEquals("$method $path", expected, it.code)
            return it.body?.string().orEmpty()
        }
    }
    private fun ready(tag: String) {
        compose.waitUntil(60_000) { compose.onAllNodesWithTag(tag).fetchSemanticsNodes().isNotEmpty() }
        compose.waitUntil(60_000) { !vm.uiState.value.busy&&!exchange.uiState.value.busy&&!commerce.uiState.value.busy&&!smart.uiState.value.busy }
        compose.waitForIdle()
    }
    private fun click(tag: String, scroll: Boolean = false) { val node = compose.onNodeWithTag(tag); if (scroll) node.performScrollTo(); node.performClick() }
    private fun input(tag: String, value: String) { compose.onNodeWithTag(tag).performScrollTo().performTextReplacement(value) }
    private fun shot(name: String) {
        closeSoftKeyboard(); compose.waitForIdle()
        if (name == "home") compose.waitUntil(60_000) { compose.onAllNodesWithTag("product_image_loaded", useUnmergedTree = true).fetchSemanticsNodes().isNotEmpty() }
        val bitmap = InstrumentationRegistry.getInstrumentation().uiAutomation.takeScreenshot()
        val file = File(app.getExternalFilesDir("qa"), "$name.png").apply { parentFile?.mkdirs() }
        file.outputStream().use { bitmap.compress(Bitmap.CompressFormat.PNG, 100, it) }; bitmap.recycle()
    }
    @Before fun setup() {
        runBlocking { app.container.phienStore.xoa() }
        val args = InstrumentationRegistry.getArguments()
        val adminEmail = requireNotNull(args.getString("adminEmail")) { "Provide a real operator-provisioned ADMIN" }
        val adminPassword = requireNotNull(args.getString("adminPassword"))
        adminToken = gson.fromJson(request("POST", "api/taikhoan/dangnhap", DangNhapBody(adminEmail, adminPassword), null), PhienDto::class.java).accessToken
        val email = "products-$suffix@example.test"
        val phone = "08" + ((suffix.hashCode().toLong() and 0x7fffffffL) % 100_000_000L).toString().padStart(8, '0')
        val password = "ProductsUi!2026"
        request("POST", "api/taikhoan/dangky", DangKyBody("Sản phẩm Android", email, phone, password, password), null, 201)
        scenario = ActivityScenario.launch(MainActivity::class.java)
        scenario!!.onActivity { vm = ViewModelProvider(it)[BanHangViewModel::class.java];exchange=ViewModelProvider(it)[TraoDoiViewModel::class.java];commerce=ViewModelProvider(it)[MuaBanViewModel::class.java];smart=ViewModelProvider(it)[SmartSwapViewModel::class.java] }
        ready("splash"); click("splash")
        compose.onNodeWithTag("login_account").performTextReplacement(email)
        compose.onNodeWithTag("login_password").performTextInput(password)
        closeSoftKeyboard(); click("login_submit", true); ready("user_home")
        category = gson.fromJson(request("GET", "api/tindang/danhmuc"), Array<DanhMucBanHang>::class.java).first { it.danhMucChaId == null }.id
    }
    @After fun cleanup() {
        // Only IDs created by this test are removed, through the normal owner-authorized API.
        createdIds.forEach { id ->
            val token = app.container.phienStore.hienTai.get()?.accessToken ?: return@forEach
            http.newUiTestCall(Request.Builder().url(BuildConfig.API_BASE_URL + "api/tindang/$id/quanly").header("Authorization", "Bearer $token").build()).execute().use { result ->
                if (result.code == 200) {
                    val tin = gson.fromJson(result.body!!.string(), TinDang::class.java)
                    request("DELETE", "api/tindang/$id", PhienBanTinBody(tin.phienBan), expected = 204)
                }
            }
        }
        app.container.phienStore.hienTai.get()?.accessToken?.let { token ->
            http.newUiTestCall(Request.Builder().url(BuildConfig.API_BASE_URL + "api/taikhoan/dangxuat").header("Authorization", "Bearer $token")
                .post(ByteArray(0).toRequestBody()).build()).execute().close()
        }
        runBlocking { app.container.phienStore.xoa() }; scenario?.close()
    }
    private fun approve(tin: TinDang) {
        request("POST", "api/admin/tindang/${tin.id}/duyet", DuyetTinBody(tin.phienBan, true, null), adminToken)
    }
    private fun attachPhoto() {
        // Native picker produces bytes through this same callback; no fake API/repository is used.
        scenario!!.onActivity { vm.themAnh(listOf(AnhChon("qa://$suffix", png))) }
    }
    @Test fun dangBanSuaAnDaBanVaXoa() {
        shot("home")
        click("tab_2"); ready("listing_form")
        click("listing_preview_submit")
        compose.onNodeWithTag("error_ten").assertExists()
        input("listing_name", "Ghế Android $suffix")
        closeSoftKeyboard()
        Assert.assertTrue("Selected API category must be present in the form",vm.uiState.value.categories.any{it.id==category})
        click("listing_category", true); ready("listing_category_$category"); click("listing_category_$category")
        input("listing_price", "450000")
        input("listing_description", "Ghế gỗ kiểm tra trên Android, còn sử dụng tốt và chắc chắn.")
        input("listing_city", "TP. Hồ Chí Minh")
        input("listing_location", "Quận 1, TP. Hồ Chí Minh")
        closeSoftKeyboard(); attachPhoto(); shot("listing-form")
        click("listing_preview_submit"); ready("listing_preview"); shot("listing-preview")
        click("listing_publish"); ready("listing_success"); shot("listing-success")
        var tin = vm.uiState.value.detail!!
        createdIds.add(tin.id)
        request("GET", "api/tindang/${tin.id}", expected = 404)
        approve(tin)
        click("success_my_listings"); ready("my_listings_screen"); shot("my-listings")
        click("listing_${tin.id}", true); ready("listing_manage"); shot("listing-manage")
        click("edit_listing"); ready("listing_form")
        input("listing_name", "Ghế đã chỉnh sửa $suffix"); closeSoftKeyboard()
        click("listing_preview_submit"); ready("listing_manage")
        compose.onNodeWithText("Ghế đã chỉnh sửa $suffix").assertExists()
        tin = vm.uiState.value.detail!!; Assert.assertEquals("ChoDuyet", tin.trangThai); approve(tin)
        click("reload_listing"); ready("listing_manage")
        click("hide_listing"); click("listing_action_confirm"); ready("listing_manage")
        Assert.assertEquals("DaAn", vm.uiState.value.detail!!.trangThai); shot("listing-hidden")
        request("GET", "api/tindang/${tin.id}", expected = 404)
        click("restore_listing"); ready("listing_manage"); approve(vm.uiState.value.detail!!)
        click("reload_listing"); ready("listing_manage")
        click("mark_sold"); ready("sold_confirmation"); shot("sold-confirmation")
        click("sold_confirm"); ready("listing_manage")
        Assert.assertEquals("DaBan", vm.uiState.value.detail!!.trangThai); shot("listing-sold")
        click("delete_listing"); click("listing_action_confirm"); ready("my_listings_screen")
        compose.onNodeWithTag("my_listings_empty").assertExists()
        request("GET", "api/tindang/${tin.id}/quanly", expected = 404)
    }
    private fun createPublished(index: Int): TinDang {
        var tin = gson.fromJson(request("POST", "api/tindang", LuuTinBody("AndroidGroup$suffix $index", category, index * 100000L,
            "NhuMoi", "Sản phẩm kiểm tra tìm kiếm và phân trang trên thiết bị Android.", "TP. Hồ Chí Minh", "Quận 3, TP. Hồ Chí Minh", null), expected = 201), TinDang::class.java)
        createdIds.add(tin.id)
        val content = MultipartBody.Builder().setType(MultipartBody.FORM).addFormDataPart("phienBan", tin.phienBan)
            .addFormDataPart("anh", "test.png", png.toRequestBody("image/png".toMediaType())).build()
        http.newUiTestCall(Request.Builder().url(BuildConfig.API_BASE_URL + "api/tindang/${tin.id}/anh").header("Authorization", "Bearer ${app.container.phienStore.hienTai.get()!!.accessToken}")
            .post(content).build()).execute().use { Assert.assertEquals(200, it.code); tin = gson.fromJson(it.body!!.string(), TinDang::class.java) }
        tin = gson.fromJson(request("POST", "api/tindang/${tin.id}/gui-duyet", PhienBanTinBody(tin.phienBan)), TinDang::class.java)
        approve(tin); return tin
    }
    @Test fun timKiemBoLocPhanTrangDanhMucVaDieuHuong() {
        val fixtures = (1..7).map(::createPublished)
        click("open_search", true); ready("catalog_results")
        input("catalog_query", "AndroidGroup$suffix"); closeSoftKeyboard(); click("catalog_search"); ready("catalog_results")
        Assert.assertEquals(7, vm.uiState.value.results!!.tongSo)
        click("catalog_sort"); click("sort_giatang"); ready("catalog_results")
        click("page_next", true); ready("catalog_results")
        Assert.assertEquals(2, vm.uiState.value.results!!.trang)
        compose.onNodeWithTag("listing_${fixtures.last().id}").assertExists(); shot("search-pagination")
        click("open_filters", true)
        input("filter_max", "200000"); closeSoftKeyboard(); click("filter_apply", true); ready("catalog_results")
        Assert.assertEquals(2, vm.uiState.value.results!!.tongSo); shot("search");shot("search-filter")
        click("listing_${fixtures.first().id}", true); ready("product_detail")
        compose.waitUntil(60_000) { compose.onAllNodesWithTag("product_image_loaded").fetchSemanticsNodes().isNotEmpty() }
        shot("product-detail")
        click("catalog_back"); ready("catalog_results")
        input("catalog_query", "no-such-$suffix"); closeSoftKeyboard(); click("catalog_search"); ready("catalog_results")
        compose.onNodeWithTag("catalog_empty").assertExists(); shot("search-empty")
        click("catalog_back"); ready("user_home")
        click("all_categories", true); ready("categories"); shot("categories")
        click("category_$category", true); ready("catalog_results"); shot("category-list")
        click("catalog_back"); ready("categories"); click("catalog_back"); ready("user_home")
        click("tab_1"); ready("exchange_explore")
        click("tab_3"); ready("inbox")
        click("settings_tab"); compose.onNodeWithTag("my_listings_row").performScrollTo().performClick(); ready("my_listings_screen")
        Assert.assertEquals(7, vm.uiState.value.mine!!.danhSach.tongSo)
    }
}
