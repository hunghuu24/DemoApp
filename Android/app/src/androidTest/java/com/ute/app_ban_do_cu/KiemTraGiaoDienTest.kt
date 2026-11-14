package com.ute.app_ban_do_cu

import android.graphics.Bitmap
import androidx.compose.ui.test.*
import androidx.compose.ui.test.junit4.createEmptyComposeRule
import androidx.lifecycle.ViewModelProvider
import androidx.core.view.ViewCompat
import androidx.core.view.WindowInsetsCompat
import androidx.test.core.app.ActivityScenario
import androidx.test.core.app.ApplicationProvider
import androidx.test.espresso.Espresso
import androidx.test.espresso.Espresso.closeSoftKeyboard
import androidx.test.platform.app.InstrumentationRegistry
import com.ute.app_ban_do_cu.domain.model.*
import com.ute.app_ban_do_cu.feature.auth.TaiKhoanViewModel
import com.ute.app_ban_do_cu.feature.catalog.BanHangViewModel
import com.ute.app_ban_do_cu.feature.exchange.TraoDoiViewModel
import com.ute.app_ban_do_cu.feature.commerce.MuaBanViewModel
import com.ute.app_ban_do_cu.feature.smartswap.SmartSwapViewModel
import com.ute.app_ban_do_cu.feature.admin.QuanTriViewModel
import kotlinx.coroutines.runBlocking
import org.junit.*
import java.io.File
import java.io.FileInputStream
import java.util.UUID

/** Additional real-API visual/state checks; no substitute repository or fake server. */
class KiemTraGiaoDienTest {
    @get:Rule val compose=createEmptyComposeRule()
    private val app get()=ApplicationProvider.getApplicationContext<ReMarketApplication>()
    private var scenario:ActivityScenario<MainActivity>?=null
    private lateinit var auth:TaiKhoanViewModel;private lateinit var catalog:BanHangViewModel
    private lateinit var exchange:TraoDoiViewModel;private lateinit var commerce:MuaBanViewModel
    private lateinit var smart:SmartSwapViewModel;private lateinit var admin:QuanTriViewModel
    private val suffix=UUID.randomUUID().toString().replace("-","").take(10)
    private val email="visual-$suffix@example.test";private val password="VisualApi!2026"
    private fun idle(){compose.waitUntil(60_000){!auth.uiState.value.dangTai&&!catalog.uiState.value.busy&&!exchange.uiState.value.busy&&!commerce.uiState.value.busy&&!smart.uiState.value.busy&&!admin.uiState.value.busy};compose.waitForIdle()}
    private fun ready(tag:String){compose.waitUntil(60_000){compose.onAllNodesWithTag(tag).fetchSemanticsNodes().isNotEmpty()};idle()}
    private fun click(tag:String,scroll:Boolean=false){val n=compose.onNodeWithTag(tag);if(scroll)n.performScrollTo();n.performClick();compose.waitForIdle()}
    private fun input(tag:String,value:String){compose.onNodeWithTag(tag).performScrollTo().performTextReplacement(value)}
    private fun shot(name:String,hideKeyboard:Boolean=true){if(hideKeyboard)closeSoftKeyboard();compose.waitForIdle();val b=InstrumentationRegistry.getInstrumentation().uiAutomation.takeScreenshot();File(app.getExternalFilesDir("qa-full-audit"),"$name.png").apply{parentFile?.mkdirs()}.outputStream().use{b.compress(Bitmap.CompressFormat.PNG,100,it)};b.recycle()}
    private fun launch(){scenario=ActivityScenario.launch(MainActivity::class.java);scenario!!.onActivity{
        auth=ViewModelProvider(it)[TaiKhoanViewModel::class.java];catalog=ViewModelProvider(it)[BanHangViewModel::class.java];exchange=ViewModelProvider(it)[TraoDoiViewModel::class.java];commerce=ViewModelProvider(it)[MuaBanViewModel::class.java];smart=ViewModelProvider(it)[SmartSwapViewModel::class.java];admin=ViewModelProvider(it)[QuanTriViewModel::class.java]
    }}
    private fun register(){runBlocking{UiTestPacing.auth();app.container.taiKhoanRepository.dangKy(DangKyBody("Hồ sơ kiểm tra $suffix",email,"07"+(suffix.hashCode().toLong().and(0x7fffffff)%100000000).toString().padStart(8,'0'),password,password));UiTestPacing.auth();app.container.taiKhoanRepository.dangNhap(DangNhapBody(email,password))}}
    private fun shell(command:String):String=InstrumentationRegistry.getInstrumentation().uiAutomation.executeShellCommand(command).use{FileInputStream(it.fileDescriptor).use{stream->stream.readBytes().toString(Charsets.UTF_8).trim()}}
    @Before fun before(){runBlocking{app.container.phienStore.xoa()}}
    @After fun after(){runBlocking{if(app.container.taiKhoanRepository.conPhien())app.container.taiKhoanRepository.dangXuat();app.container.phienStore.xoa()};scenario?.close()}

    @Test fun taiKhoanCacFormTheoFigma(){
        launch();ready("splash");shot("splash");click("splash");ready("login");shot("login")
        click("forgot_link");ready("forgot");shot("forgot-password");input("forgot_email","not-an-email");closeSoftKeyboard();click("forgot_submit",true);idle();Assert.assertNotNull(auth.uiState.value.loi)
        Espresso.pressBack();ready("login");click("register_link",true);ready("register");shot("register")
        input("register_name","Hồ sơ kiểm tra $suffix");input("register_email",email);input("register_phone","07"+(suffix.hashCode().toLong().and(0x7fffffff)%100000000).toString().padStart(8,'0'));input("register_password",password);input("register_confirm",password);closeSoftKeyboard();click("register_submit",true);ready("login")
        input("login_account",email);input("login_password",password);closeSoftKeyboard();click("login_submit",true);ready("user_home")
        click("settings_tab");ready("settings");shot("settings");click("profile_row",true);ready("profile");shot("profile");click("profile_edit",true);ready("edit_profile");shot("edit-profile")
        Espresso.pressBack();ready("profile");Espresso.pressBack();ready("settings");click("change_password_row",true);ready("change_password");shot("change-password")
    }
    @Test fun loiApi404ValidationVaQuayLai(){
        register();launch();ready("user_home");scenario!!.onActivity{catalog.chiTiet(UUID.randomUUID().toString())};ready("catalog_error");Assert.assertNotNull(catalog.uiState.value.error);shot("api404-error")
        Espresso.pressBack();ready("user_home");click("tab_2");ready("listing_form");click("listing_preview_submit");compose.onNodeWithTag("error_ten").assertExists();shot("listing-validation")
        Espresso.pressBack();ready("user_home");click("all_products",true);ready("catalog_results");input("catalog_query","no-match-$suffix");closeSoftKeyboard()
        // Use the same real search command as the keyboard's search action.
        scenario!!.onActivity{catalog.tim()};idle();Assert.assertEquals(0,catalog.uiState.value.results!!.tongSo);shot("catalog-empty")
    }
    @Test fun formCuonManNhoVaFontLon(){
        register();launch();ready("user_home");click("tab_2");ready("listing_form")
        try{
            shell("wm size 320x640");shell("settings put system font_scale 1.3");scenario!!.recreate()
            compose.waitUntil(30_000){compose.onNodeWithTag("listing_form").fetchSemanticsNode().size.width<=325};idle()
            input("listing_name","Món đồ hiển thị trên màn hình nhỏ");closeSoftKeyboard();shot("listing-320-font130")
            compose.onNodeWithTag("listing_preview_submit").assertIsDisplayed().performClick();compose.onNodeWithTag("error_danhMuc").performScrollTo().assertExists()
        }finally{shell("settings put system font_scale 1.0");shell("wm size 360x800");scenario!!.recreate();compose.waitForIdle()}
    }
    @Test fun banPhimKhongCheTruongNhapVaNutXemTruoc(){
        register();launch();ready("user_home");click("tab_2");ready("listing_form")
        val previous=shell("settings get secure show_ime_with_hard_keyboard")
        try{
            shell("settings put secure show_ime_with_hard_keyboard 1")
            compose.onNodeWithTag("listing_name").performScrollTo().performClick()
            compose.waitUntil(15_000){var shown=false;scenario!!.onActivity{shown=ViewCompat.getRootWindowInsets(it.window.decorView)?.isVisible(WindowInsetsCompat.Type.ime())==true};shown}
            compose.waitForIdle();compose.onNodeWithTag("listing_name").assertIsDisplayed();compose.onNodeWithTag("listing_preview_submit").assertIsDisplayed();shot("listing-keyboard-name",false)
            input("listing_description","Mô tả dài để kiểm tra trường nhập và bàn phím trên form đăng tin.")
            compose.onNodeWithTag("listing_description").assertIsDisplayed();compose.onNodeWithTag("listing_preview_submit").assertIsDisplayed();shot("listing-keyboard-description",false)
        }finally{closeSoftKeyboard();if(previous=="null")shell("settings delete secure show_ime_with_hard_keyboard")else shell("settings put secure show_ime_with_hard_keyboard ${if(previous=="1")"1" else "0"}")}
    }
    @Test fun soDoSmartSwapManNhoVaFontLon(){
        val args=InstrumentationRegistry.getArguments()
        val demoPassword=args.getString("adminPassword")?:error("Supply demo credentials for the real-API visual check")
        UiTestPacing.auth();runBlocking{app.container.taiKhoanRepository.dangNhap(DangNhapBody(args.getString("swapEmail")?:"swap-ship-a@remarket.example.test",demoPassword))}
        launch();ready("user_home");click("settings_tab");ready("settings");click("smart_swaps_row",true);ready("ss_list")
        val circle=smart.uiState.value.cycles!!.items.first();click("ss_cycle_${circle.id}",true);ready("ss_detail")
        try{
            shell("wm size 320x640");shell("settings put system font_scale 1.3");scenario!!.recreate();ready("ss_detail")
            compose.waitUntil(30_000){compose.onRoot().fetchSemanticsNode().size.width<=325}
            compose.onNodeWithTag("ss_diagram").performScrollTo();compose.waitForIdle()
            val width=compose.onRoot().fetchSemanticsNode().size.width
            listOf("A","C","B").forEach{role->val node=compose.onNodeWithText(role);node.assertIsDisplayed();val bounds=node.fetchSemanticsNode().boundsInRoot;Assert.assertTrue(bounds.left>=0&&bounds.right<=width)}
            shot("smart-swap-320-font130")
        }finally{shell("settings put system font_scale 1.0");shell("wm size 360x800");scenario!!.recreate();compose.waitForIdle()}
        ready("ss_detail");click("ss_chat_open");ready("ss_chat")
        val longDraft=(1..30).joinToString("\n"){"Dòng trao đổi $it"}
        compose.onNodeWithTag("ss_chat_message").performTextReplacement(longDraft);closeSoftKeyboard();compose.waitForIdle()
        Assert.assertEquals(longDraft,smart.uiState.value.draft)
        Assert.assertTrue(compose.onNodeWithTag("ss_chat_message").fetchSemanticsNode().size.height<=160)
        listOf("DaGiao","DaGui","DangVanChuyen").forEach{compose.onAllNodesWithText(it,substring=true).assertCountEquals(0)}
        compose.onNodeWithTag("ss_chat").assertIsDisplayed();shot("smart-swap-long-draft")
    }
}
