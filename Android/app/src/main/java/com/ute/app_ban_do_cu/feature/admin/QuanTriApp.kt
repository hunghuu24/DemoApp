package com.ute.app_ban_do_cu.feature.admin

import android.net.Uri
import android.widget.MediaController
import android.widget.VideoView
import androidx.activity.compose.BackHandler
import androidx.compose.foundation.*
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.draw.clip
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.platform.testTag
import androidx.compose.ui.res.painterResource
import androidx.compose.ui.semantics.Role
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.dp
import androidx.compose.ui.viewinterop.AndroidView
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import coil.ImageLoader
import com.ute.app_ban_do_cu.BuildConfig
import com.ute.app_ban_do_cu.ReMarketApplication
import com.ute.app_ban_do_cu.R
import com.ute.app_ban_do_cu.core.designsystem.*
import com.ute.app_ban_do_cu.domain.model.HoSo
import com.ute.app_ban_do_cu.feature.catalog.AnhSanPham
import com.ute.app_ban_do_cu.feature.catalog.LuaChon
import com.ute.app_ban_do_cu.feature.auth.Brand
import com.ute.app_ban_do_cu.feature.account.HoSoAvatar
import com.ute.app_ban_do_cu.feature.smartswap.SsCheck
import kotlinx.coroutines.launch

@OptIn(ExperimentalMaterial3Api::class)
@Composable fun QuanTriApp(vm:QuanTriViewModel,profile:HoSo,loader:ImageLoader,settings:()->Unit,support:()->Unit,logout:()->Unit){
    val s by vm.uiState.collectAsStateWithLifecycle();val drawer=rememberDrawerState(DrawerValue.Closed);val scope=rememberCoroutineScope()
    var dialog by remember{mutableStateOf<String?>(null)};var reason by remember{mutableStateOf("")};var days by remember{mutableStateOf("7")};var reviewed by remember{mutableStateOf(false)}
    BackHandler(drawer.isOpen){scope.launch{drawer.close()}}
    BackHandler(!drawer.isOpen&&s.screen!=QtScreen.HOME){vm.back()}
    val title=when(s.screen){QtScreen.HOME->"Tổng quan";QtScreen.USERS->"Người dùng";QtScreen.USER->"Chi tiết người dùng";QtScreen.LISTINGS->"Quản lý sản phẩm";QtScreen.LISTING->"Kiểm duyệt tin";QtScreen.CATEGORIES->"Danh mục";QtScreen.CATEGORY_FORM->if(s.category.id==null)"Thêm danh mục"else "Chỉnh sửa danh mục";QtScreen.TRADES->"Quản lý giao dịch";QtScreen.TRADE->"Chi tiết giao dịch";QtScreen.COMPLAINTS->"Khiếu nại";QtScreen.COMPLAINT->"Chi tiết tranh chấp";QtScreen.CREATE_COMPLAINT->"Tạo xử lý tranh chấp";QtScreen.DECISION->"Xử lý tranh chấp";QtScreen.VERIFY->"Xác minh quyết định";QtScreen.REPORT->"Báo cáo thống kê";QtScreen.EVIDENCE->"Chi tiết bằng chứng"}
    val menu=listOf(Triple(QtScreen.HOME,"Tổng quan",R.drawable.ic_fg_admin_layout_dashboard),Triple(QtScreen.USERS,"Người dùng",R.drawable.ic_fg_admin_users),Triple(QtScreen.LISTINGS,"Sản phẩm",R.drawable.ic_fg_admin_package),Triple(QtScreen.TRADES,"Giao dịch",R.drawable.ic_fg_admin_arrow_left_right),Triple(QtScreen.COMPLAINTS,"Khiếu nại",R.drawable.ic_fg_admin_flag),Triple(QtScreen.CATEGORIES,"Danh mục",R.drawable.ic_fg_admin_layout_grid),Triple(QtScreen.REPORT,"Báo cáo",R.drawable.ic_fg_admin_chart_column))
    val isSection=menu.any{it.first==s.screen}
    ModalNavigationDrawer(drawerState=drawer,gesturesEnabled=!s.busy,drawerContent={ModalDrawerSheet(Modifier.width(286.dp),drawerContainerColor=ReMarketPalette.Background){Column(Modifier.fillMaxHeight().statusBarsPadding().verticalScroll(rememberScrollState()).padding(horizontal=20.dp,vertical=16.dp),verticalArrangement=Arrangement.spacedBy(10.dp)){
        Brand();Spacer(Modifier.height(8.dp))
        Row(Modifier.fillMaxWidth().testTag("qt_account_open").clip(RoundedCornerShape(16.dp))
            .clickable(enabled=!s.busy,role=Role.Button,onClickLabel="Mở tài khoản quản trị"){scope.launch{drawer.close();settings()}}.padding(vertical=12.dp),
            horizontalArrangement=Arrangement.spacedBy(10.dp),verticalAlignment=Alignment.CenterVertically){
            HoSoAvatar(profile,loader,44.dp)
            Column(Modifier.weight(1f),verticalArrangement=Arrangement.spacedBy(4.dp)){
                Text(profile.hoTen,style=rmText(15,FontWeight.SemiBold),maxLines=2,overflow=TextOverflow.Ellipsis)
                Text("Quản trị viên · ADMIN",style=rmText(11),color=ReMarketPalette.Muted)
            }
            RmIcon(R.drawable.ic_rm_chevron_right,size=16.dp,description="Tài khoản quản trị")
        }
        HorizontalDivider(color=ReMarketPalette.Border)
        menu.forEach{(screen,label,icon)->NavigationDrawerItem(label={Text(label,style=rmText(14))},selected=s.screen==screen,onClick={if(!s.busy){scope.launch{drawer.close()};vm.section(screen)}},icon={RmIcon(icon,size=20.dp)},modifier=Modifier.testTag("qt_menu_${screen.name}"))}
        HorizontalDivider(color=ReMarketPalette.Border);TextButton({scope.launch{drawer.close()};logout()},enabled=!s.busy,modifier=Modifier.testTag("qt_logout")){RmIcon(R.drawable.ic_fg_admin_log_out);Spacer(Modifier.width(10.dp));Text("Đăng xuất")}
        Text("ReMarket Admin · Phiên bảo mật",style=rmText(11),color=ReMarketPalette.Muted)
    }}}){Scaffold(containerColor=ReMarketPalette.Background,modifier=Modifier.imePadding(),topBar={Row(Modifier.fillMaxWidth().statusBarsPadding().heightIn(min=60.dp).padding(horizontal=16.dp),verticalAlignment=Alignment.CenterVertically,horizontalArrangement=Arrangement.spacedBy(8.dp)){
        IconButton({if(isSection)scope.launch{drawer.open()}else vm.back()},enabled=!s.busy,modifier=Modifier.size(48.dp).testTag(if(isSection)"admin_menu"else"qt_back")){RmIcon(if(isSection)R.drawable.ic_fg_admin_menu else R.drawable.ic_fg_smart_back,size=22.dp,description=if(isSection)"Menu quản trị"else "Quay lại")}
        Text(title,Modifier.weight(1f),style=rmText(20,FontWeight.SemiBold),maxLines=2,overflow=TextOverflow.Ellipsis)
        IconButton(vm::reload,enabled=!s.busy,modifier=Modifier.size(48.dp).testTag("qt_reload")){RmIcon(R.drawable.ic_rm_recycle,size=18.dp,description="Tải lại")}
    }},bottomBar={val show=when(s.screen){QtScreen.USER->s.user?.let{it.coTheKhoa||it.coTheMoKhoa}==true;QtScreen.LISTING->s.listing?.let{it.coTheDuyet||it.coTheAn}==true;QtScreen.CATEGORIES,QtScreen.CATEGORY_FORM,QtScreen.CREATE_COMPLAINT,QtScreen.DECISION,QtScreen.VERIFY->true;QtScreen.COMPLAINT->s.complaint?.khieuNai?.trangThai in listOf("Moi","DangXuLy","ChoThucHien");else->false}
        if(show)RmActionBar{
            when(s.screen){
                QtScreen.USER->{if(s.user?.coTheKhoa==true)QtButton("Khóa tài khoản","qt_lock_open",enabled=!s.busy){reason="";days="7";dialog="lock"};if(s.user?.coTheMoKhoa==true)QtButton("Mở khóa tài khoản","qt_unlock_open",enabled=!s.busy){reason="";reviewed=false;dialog="unlock"}}
                QtScreen.LISTING->{if(s.listing?.coTheDuyet==true)Row(horizontalArrangement=Arrangement.spacedBy(8.dp)){QtButton("Từ chối","qt_reject_open",Modifier.weight(1f),!s.busy,true){reason="";dialog="reject"};QtButton("Duyệt tin","qt_approve",Modifier.weight(1f),!s.busy){vm.review(true)}};if(s.listing?.coTheAn==true)QtButton("Xác nhận ẩn tin","qt_hide_open",enabled=!s.busy){reason="";dialog="hide"}}
                QtScreen.CATEGORIES->QtButton("+ Thêm danh mục","qt_category_new",enabled=!s.busy){vm.category()}
                QtScreen.CATEGORY_FORM->QtButton("Lưu danh mục","qt_category_save",enabled=!s.busy){vm.saveCategory()}
                QtScreen.COMPLAINT->{val status=s.complaint?.khieuNai?.trangThai
                    if(status in listOf("Moi","DangXuLy"))Row(horizontalArrangement=Arrangement.spacedBy(8.dp)){QtButton(if(status=="Moi")"Tiếp nhận"else "Yêu cầu bổ sung","qt_complaint_accept",Modifier.weight(1f),!s.busy,true){reason="";dialog="accept"};if(status=="DangXuLy")QtButton("Xử lý tranh chấp","qt_decision_open",Modifier.weight(1f),!s.busy){vm.decision()}}
                    if(status=="ChoThucHien")QtButton("Xác minh thực hiện quyết định","qt_verify_open",enabled=!s.busy){vm.verify()}
                }
                QtScreen.CREATE_COMPLAINT->QtButton("Ghi nhận và thông báo","qt_complaint_create_submit",enabled=!s.busy){vm.saveComplaint()}
                QtScreen.DECISION->QtButton("Xác nhận quyết định xử lý","qt_decision_submit",enabled=!s.busy){vm.decide()}
                QtScreen.VERIFY->QtButton(if(s.complaint?.khieuNai?.loaiGiaoDich!="MuaBan"&&s.complaint?.quyetDinh?.firstOrNull{!it.daXacMinhThucHien}?.huongXuLy=="GhiNhan")"Xác minh và tiếp tục xử lý"else "Xác minh và đóng khiếu nại","qt_verify_submit",enabled=!s.busy){vm.confirmVerification()}
                else->Unit
            }
        }
    }){padding->Column(Modifier.fillMaxSize().padding(padding).consumeWindowInsets(padding)){
        if(s.busy)LinearProgressIndicator(Modifier.fillMaxWidth().testTag("qt_loading"))
        s.notice?.let{Text(it,Modifier.fillMaxWidth().background(ReMarketPalette.Soft).padding(horizontal=24.dp,vertical=8.dp).testTag("qt_notice"),style=rmText(12),color=ReMarketPalette.Primary)}
        s.error?.let{Row(Modifier.fillMaxWidth().background(ReMarketPalette.Warm).padding(horizontal=24.dp,vertical=8.dp),verticalAlignment=Alignment.CenterVertically){Text(it,Modifier.weight(1f).testTag("qt_error"),style=rmText(12),color=ReMarketPalette.Error);TextButton(vm::reload,enabled=!s.busy){Text("Tải lại")}}}
        key(s.screen){Box(Modifier.weight(1f)){when(s.screen){QtScreen.HOME->QtDashboard(s,vm,profile,support);QtScreen.USERS->QtUsers(s,vm);QtScreen.USER->QtUserScreen(s);QtScreen.LISTINGS->QtListings(s,vm,loader);QtScreen.LISTING->QtListingScreen(s,vm,loader);QtScreen.CATEGORIES->QtCategories(s,vm);QtScreen.CATEGORY_FORM->QtCategoryFormScreen(s,vm);QtScreen.TRADES->QtTrades(s,vm);QtScreen.TRADE->QtTradeScreen(s,vm,loader);QtScreen.COMPLAINTS->QtComplaints(s,vm);QtScreen.COMPLAINT->QtComplaintScreen(s,vm,loader);QtScreen.CREATE_COMPLAINT->QtCreateComplaintScreen(s,vm);QtScreen.DECISION->QtDecisionScreen(s,vm);QtScreen.VERIFY->QtVerifyScreen(s,vm);QtScreen.REPORT->QtReportScreen(s,vm);QtScreen.EVIDENCE->QtEvidenceScreen(s,loader)}}}
    }}}
    if(dialog!=null)AlertDialog(onDismissRequest={if(!s.busy)dialog=null},title={Text(when(dialog){"lock"->"Khóa tài khoản?";"unlock"->"Xác nhận mở khóa";"reject"->"Từ chối tin đăng";"hide"->"Xác nhận ẩn tin";else->"Tiếp nhận / yêu cầu bổ sung"})},text={Column(Modifier.verticalScroll(rememberScrollState()),verticalArrangement=Arrangement.spacedBy(12.dp)){
        RmField(if(dialog=="accept")"Nội dung gửi các bên"else "Lý do",reason,{reason=it.take(if(dialog=="accept")4000 else 500)},"Nêu rõ nội dung","qt_action_reason",enabled=!s.busy,multiline=true,minimumHeight=90)
        if(dialog=="lock"){LuaChon("Thời hạn khóa",days,listOf("7" to "7 ngày","30" to "30 ngày","0" to "Vĩnh viễn"),{days=it},tag="qt_lock_days");Text("Người dùng không thể đăng tin, nhắn tin hoặc tạo giao dịch mới. Giao dịch và lịch sử vẫn được lưu.",style=rmText(12))}
        if(dialog=="unlock")SsCheck("Đã rà soát và đủ điều kiện mở khóa",reviewed,!s.busy,"qt_unlock_check"){reviewed=it}
        if(dialog=="hide")Text("Tin sẽ không xuất hiện trong khám phá; lý do được gửi cho người đăng và dữ liệu giao dịch được giữ.",style=rmText(12))
        s.error?.let{Text(it,style=rmText(12),color=ReMarketPalette.Error)}
    }},confirmButton={TextButton({if(reason.trim().length<5)vm.error("Nội dung cần ít nhất 5 ký tự.")else if(dialog=="unlock"&&!reviewed)vm.error("Cần xác nhận đã rà soát.")else{when(dialog){"lock"->vm.lock(reason,days.toInt());"unlock"->vm.unlock(reason,reviewed);"reject"->vm.review(false,reason);"hide"->vm.hide(reason);else->vm.accept(reason)};dialog=null}},enabled=!s.busy,modifier=Modifier.testTag("qt_action_confirm")){Text("Xác nhận")}},dismissButton={TextButton({dialog=null},enabled=!s.busy){Text("Hủy")}})
}
@Composable private fun QtEvidenceScreen(s:QuanTriState,loader:ImageLoader){val e=s.evidence?:return;val context=LocalContext.current;var error by remember(e.id){mutableStateOf(false)};var player by remember{mutableStateOf<VideoView?>(null)}
    DisposableEffect(e.id){onDispose{player?.stopPlayback()}}
    QtBody("qt_evidence_detail"){Text(e.moTa.ifBlank{"Bằng chứng giao dịch"},style=rmText(16,FontWeight.SemiBold))
        if(e.loaiMime.startsWith("image/"))AnhSanPham(e.url,loader,Modifier.fillMaxWidth().height(400.dp),e.moTa)
        else if(e.loaiMime=="video/mp4"){
            if(error)Text("Chưa phát được video. Kiểm tra kết nối hoặc thời hạn phiên đăng nhập rồi mở lại bằng chứng.",style=rmText(13),color=ReMarketPalette.Error)
            AndroidView(factory={ctx->VideoView(ctx).apply{player=this;val token=(ctx.applicationContext as ReMarketApplication).container.phienStore.hienTai.get()?.accessToken
                setVideoURI(Uri.parse(BuildConfig.API_BASE_URL+e.url.removePrefix("/")),mapOf("Authorization" to "Bearer $token"));setMediaController(MediaController(ctx).also{it.setAnchorView(this)});setOnErrorListener{_,_,_->error=true;true};setOnPreparedListener{start()}}},modifier=Modifier.fillMaxWidth().height(340.dp))
        }else RmNotice("Loại tệp này chưa hỗ trợ xem trực tiếp trong ứng dụng.")
        Text("Tệp được lấy từ máy chủ trong phạm vi giao dịch. Việc xem không thay đổi xác nhận của thành viên.",style=rmText(12),color=ReMarketPalette.Muted)
    }
}
