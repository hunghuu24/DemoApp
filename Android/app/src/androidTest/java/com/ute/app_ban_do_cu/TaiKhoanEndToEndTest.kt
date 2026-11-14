package com.ute.app_ban_do_cu

import android.graphics.Bitmap
import androidx.compose.ui.test.*
import androidx.compose.ui.test.junit4.createEmptyComposeRule
import androidx.lifecycle.ViewModelProvider
import androidx.test.core.app.ActivityScenario
import androidx.test.core.app.ApplicationProvider
import androidx.test.espresso.Espresso.closeSoftKeyboard
import androidx.test.platform.app.InstrumentationRegistry
import com.google.gson.Gson
import com.ute.app_ban_do_cu.feature.auth.TaiKhoanViewModel
import kotlinx.coroutines.runBlocking
import okhttp3.MediaType.Companion.toMediaType
import okhttp3.OkHttpClient
import okhttp3.Request
import okhttp3.RequestBody.Companion.toRequestBody
import org.junit.*
import org.junit.runners.MethodSorters
import java.util.UUID
import java.io.File
import java.util.concurrent.TimeUnit

/** Uses the actual development API + SQL Server; never substitutes fake account data. */
@FixMethodOrder(MethodSorters.NAME_ASCENDING)
class TaiKhoanEndToEndTest {
    @get:Rule val compose = createEmptyComposeRule()
    private var scenario: ActivityScenario<MainActivity>? = null
    private val app get() = ApplicationProvider.getApplicationContext<ReMarketApplication>()
    private val http = OkHttpClient.Builder().callTimeout(30, TimeUnit.SECONDS).build()
    @Before fun setup() {
        runBlocking { app.container.phienStore.xoa() }
        scenario = ActivityScenario.launch(MainActivity::class.java)
        waitTag("splash")
    }
    @After fun teardown() { scenario?.close() }
    private fun waitTag(tag: String) = compose.waitUntil(60_000) {
        compose.onAllNodesWithTag(tag).fetchSemanticsNodes().isNotEmpty()
    }
    private fun ready(tag: String) = compose.waitUntil(60_000) {
        compose.onAllNodes(hasTestTag(tag) and isEnabled()).fetchSemanticsNodes().isNotEmpty()
    }
    private fun shot(name: String) {
        closeSoftKeyboard(); compose.waitForIdle()
        val bitmap = InstrumentationRegistry.getInstrumentation().uiAutomation.takeScreenshot()
        File(app.getExternalFilesDir("qa-admin-account"), "$name.png").apply { parentFile?.mkdirs() }
            .outputStream().use { bitmap.compress(Bitmap.CompressFormat.PNG, 100, it) }
        bitmap.recycle()
    }
    private fun adminContext() {
        listOf("user_home", "settings_tab", "my_listings_row", "orders_row", "exchanges_row", "smart_swaps_row", "support_row").forEach {
            compose.onNodeWithTag(it).assertDoesNotExist()
        }
        scenario!!.onActivity {
            val auth = ViewModelProvider(it)[TaiKhoanViewModel::class.java]
            Assert.assertTrue("Admin session must be retained", auth.uiState.value.hoSo?.vaiTro?.contains("ADMIN") == true)
        }
    }
    private fun openAdminAccount() {
        ready("admin_menu"); compose.onNodeWithTag("admin_menu").performClick()
        ready("qt_account_open"); compose.onNodeWithTag("qt_account_open").performScrollTo().performClick()
        waitTag("admin_account"); ready("profile_row"); adminContext()
    }
    private fun login(email: String, password: String) {
        ready("splash"); compose.onNodeWithTag("splash").performClick()
        waitTag("login")
        compose.onNodeWithTag("login_account").performTextReplacement(email)
        compose.onNodeWithTag("login_password").performTextInput(password)
        closeSoftKeyboard()
        compose.onNodeWithTag("login_submit").performScrollTo().performClick()
    }
    private fun createAccount(): Pair<String, String> {
        val random = UUID.randomUUID().toString().replace("-", "")
        val email = "android-$random@example.test"
        val phone = "09" + ((random.hashCode().toLong() and 0x7fffffffL) % 100_000_000).toString().padStart(8, '0')
        val password = "AndroidAccount!2026"
        val json = Gson().toJson(mapOf("hoTen" to "Android QA", "email" to email, "soDienThoai" to phone,
            "matKhau" to password, "xacNhanMatKhau" to password, "dongYDieuKhoan" to true))
        http.newUiTestCall(Request.Builder().url(BuildConfig.API_BASE_URL + "api/taikhoan/dangky")
            .post(json.toRequestBody("application/json".toMediaType())).build()).execute().use {
            Assert.assertEquals("Live account registration failed", 201, it.code)
        }
        return email to password
    }
    @Test fun aDangKyDangNhapHoSoDangXuat() {
        ready("splash"); compose.onNodeWithTag("splash").performClick()
        compose.onNodeWithTag("register_link").performScrollTo().performClick()
        waitTag("register")
        val random = UUID.randomUUID().toString().replace("-", "")
        val email = "ui-$random@example.test"
        val phone = "08" + ((random.hashCode().toLong() and 0x7fffffffL) % 100_000_000).toString().padStart(8, '0')
        val password = "AndroidAccount!2026"
        listOf("register_name" to "Kiểm tra Android", "register_email" to email, "register_phone" to phone,
            "register_password" to password, "register_confirm" to password).forEach { (tag, value) ->
            compose.onNodeWithTag(tag).performScrollTo().performTextInput(value)
        }
        closeSoftKeyboard()
        compose.onNodeWithTag("register_submit").performScrollTo().performClick()
        waitTag("login")
        compose.onNodeWithTag("login_account").assertTextEquals(email)
        compose.onNodeWithTag("login_password").performTextInput(password)
        closeSoftKeyboard()
        compose.onNodeWithTag("login_submit").performScrollTo().performClick()
        waitTag("user_home"); ready("settings_tab")
        val access = app.container.phienStore.hienTai.get()!!.accessToken
        compose.onNodeWithTag("settings_tab").performClick()
        compose.onNodeWithTag("profile_row").performScrollTo().performClick()
        waitTag("profile")
        compose.onNodeWithTag("profile_edit").performScrollTo().performClick()
        waitTag("edit_profile")
        compose.onNodeWithTag("edit_name").performScrollTo().performTextReplacement("Tên đã sửa trên Android")
        compose.onNodeWithTag("edit_address").performScrollTo().performTextReplacement("Quận 3, TP. Hồ Chí Minh")
        closeSoftKeyboard()
        compose.onNodeWithTag("edit_submit").performScrollTo().performClick()
        waitTag("profile")
        compose.onNodeWithText("Tên đã sửa trên Android").assertExists()
        compose.onNodeWithTag("toolbar_back").performClick()
        waitTag("settings")
        compose.onNodeWithTag("logout_submit").performScrollTo().performClick()
        waitTag("login")
        Assert.assertNull(app.container.phienStore.hienTai.get())
        http.newUiTestCall(Request.Builder().url(BuildConfig.API_BASE_URL + "api/taikhoan/toi")
            .header("Authorization", "Bearer $access").build()).execute().use {
            Assert.assertEquals("Logged-out JWT must fail on server", 401, it.code)
        }
        scenario?.close(); scenario = ActivityScenario.launch(MainActivity::class.java)
        waitTag("splash")
    }
    @Test fun bAdminVaoTrangQuanTri() {
        val args = InstrumentationRegistry.getArguments()
        val email = args.getString("adminEmail")
        val password = args.getString("adminPassword")
        Assume.assumeTrue("Provide operator-provisioned test ADMIN credentials", email != null && password != null)
        login(email!!, password!!)
        waitTag("admin_home"); ready("admin_menu")
        compose.onNodeWithTag("admin_settings").assertDoesNotExist()
        compose.onNodeWithTag("admin_menu").performClick(); ready("qt_account_open"); shot("drawer-account-entry")
        compose.onNodeWithTag("qt_account_open").performScrollTo().performClick()
        waitTag("admin_account"); ready("profile_row"); adminContext(); shot("account")
        compose.onNodeWithTag("profile_row").performScrollTo().performClick()
        waitTag("profile"); adminContext()
        compose.onNodeWithTag("profile_edit").performScrollTo().performClick()
        waitTag("edit_profile"); adminContext(); shot("edit-profile")
        compose.onNodeWithTag("toolbar_back").performClick(); waitTag("profile")
        compose.onNodeWithTag("toolbar_back").performClick(); waitTag("admin_account"); adminContext()
        compose.onNodeWithTag("change_password_row").performScrollTo().performClick()
        waitTag("change_old"); adminContext(); shot("change-password")
        compose.onNodeWithTag("change_submit").performScrollTo().performClick()
        compose.onNodeWithText("Vui lòng nhập mật khẩu hiện tại.").assertExists()
        compose.onNodeWithTag("toolbar_back").performClick(); waitTag("admin_account"); adminContext()
        compose.onNodeWithTag("admin_account_back").performScrollTo().performClick(); waitTag("admin_home")
        ready("admin_menu"); compose.onNodeWithTag("admin_menu").performClick()
        compose.onNodeWithTag("qt_menu_USERS").performScrollTo().performClick(); waitTag("qt_users")
        openAdminAccount()
        compose.onNodeWithTag("toolbar_back").performClick(); waitTag("qt_users"); adminContext()
        openAdminAccount()
        androidx.test.espresso.Espresso.pressBack(); waitTag("qt_users"); adminContext()
        scenario?.close(); scenario = ActivityScenario.launch(MainActivity::class.java)
        waitTag("admin_home"); adminContext(); openAdminAccount()
        val access = app.container.phienStore.hienTai.get()!!.accessToken
        compose.onNodeWithTag("logout_submit").performScrollTo().performClick()
        waitTag("login")
        Assert.assertNull(app.container.phienStore.hienTai.get())
        http.newUiTestCall(Request.Builder().url(BuildConfig.API_BASE_URL + "api/taikhoan/toi")
            .header("Authorization", "Bearer $access").build()).execute().use {
            Assert.assertEquals("Logged-out Admin JWT must fail on server", 401, it.code)
        }
    }
    @Test fun zLuuPhienDataStoreVaKhoiDongLai() {
        val (email, password) = createAccount()
        login(email, password)
        waitTag("user_home"); ready("settings_tab")
        Assert.assertNotNull(app.container.phienStore.hienTai.get())
        scenario?.close(); scenario = ActivityScenario.launch(MainActivity::class.java)
        waitTag("user_home"); ready("settings_tab")
        // Keep the session for an additional adb force-stop / cold-start verification.
    }
}
