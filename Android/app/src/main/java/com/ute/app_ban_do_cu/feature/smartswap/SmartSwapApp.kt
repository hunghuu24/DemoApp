package com.ute.app_ban_do_cu.feature.smartswap

import androidx.activity.compose.BackHandler
import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.activity.result.PickVisualMediaRequest
import androidx.activity.result.contract.ActivityResultContracts
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.platform.testTag
import androidx.compose.ui.res.painterResource
import androidx.compose.ui.unit.dp
import androidx.lifecycle.Lifecycle
import androidx.lifecycle.compose.*
import androidx.lifecycle.repeatOnLifecycle
import coil.ImageLoader
import com.ute.app_ban_do_cu.R
import com.ute.app_ban_do_cu.core.designsystem.*
import com.ute.app_ban_do_cu.domain.model.HoSo
import com.ute.app_ban_do_cu.feature.catalog.DieuHuongSanPham
import com.ute.app_ban_do_cu.feature.exchange.TdNote
import kotlinx.coroutines.*
import java.io.ByteArrayOutputStream
import java.io.IOException

@OptIn(ExperimentalMaterial3Api::class)
@Composable fun SmartSwapApp(vm:SmartSwapViewModel,loader:ImageLoader,profile:HoSo,onTab:(Int)->Unit,onExit:()->Unit){
    val s by vm.uiState.collectAsStateWithLifecycle();val lifecycle=LocalLifecycleOwner.current.lifecycle
    var dialog by remember{mutableStateOf<String?>(null)};var reason by remember{mutableStateOf("")}
    val context=LocalContext.current;val scope=rememberCoroutineScope();var reading by remember{mutableStateOf(false)}
    val picker=rememberLauncherForActivityResult(ActivityResultContracts.PickVisualMedia()){uri->if(uri!=null)scope.launch{
        reading=true
        try{val bytes=withContext(Dispatchers.IO){context.contentResolver.openInputStream(uri)?.use{input->val out=ByteArrayOutputStream();val buffer=ByteArray(16384)
            while(true){val n=input.read(buffer);if(n<0)break;if(out.size()+n>5*1024*1024)throw IOException("Ảnh tối đa 5 MB.");out.write(buffer,0,n)};out.toByteArray()}?:throw IOException("Không đọc được ảnh.")};if(s.screen==SsScreen.DELIVERY)vm.addEvidence(bytes)else vm.sendImage(bytes)}
        catch(ex:IOException){vm.error(ex.message?:"Không đọc được ảnh.")}catch(_:SecurityException){vm.error("Không có quyền đọc ảnh đã chọn.")}finally{reading=false}
    }}
    val enabled=!s.busy&&!reading;val back={if(enabled){vm.back();if(vm.uiState.value.screen==SsScreen.NONE)onExit()}}
    BackHandler{back()}
    LaunchedEffect(s.screen,lifecycle){lifecycle.repeatOnLifecycle(Lifecycle.State.STARTED){while(isActive){vm.poll();delay(3000)}}}
    val title=when(s.screen){SsScreen.FIND->"Tìm vòng trao đổi";SsScreen.SELECT->"Chọn đồ của tôi";SsScreen.MATCHES->"Vòng phù hợp";SsScreen.PREVIEW,SsScreen.DETAIL->"Chi tiết vòng";SsScreen.LIST->"Smart Swap của tôi";SsScreen.CONSENT->"Xác nhận 3 bên";SsScreen.CHAT->"Nhóm ${s.circle?.ma.orEmpty()}";SsScreen.COMPENSATION->"Điều chỉnh tiền bù";SsScreen.DELIVERY->"Bàn giao 3 bên";SsScreen.CONTACT->"Thông tin giao nhận";SsScreen.ISSUE->"Báo cáo vấn đề";SsScreen.REVIEW->"Hoàn tất và đánh giá";SsScreen.REVIEW_DONE->"Đã gửi đánh giá";SsScreen.NOTIFICATIONS->"Thông báo Smart Swap";else->"Smart Swap"}
    Scaffold(containerColor=ReMarketPalette.Background,modifier=Modifier.imePadding(),topBar={
        Row(Modifier.fillMaxWidth().statusBarsPadding().heightIn(min=60.dp).padding(horizontal=16.dp),verticalAlignment=Alignment.CenterVertically,horizontalArrangement=Arrangement.spacedBy(8.dp)){
            IconButton(back,enabled=enabled,modifier=Modifier.size(48.dp).testTag("ss_back")){Icon(painterResource(R.drawable.ic_fg_smart_back),"Quay lại",Modifier.size(22.1728.dp,21.8.dp),tint=Color.Unspecified)}
            Text(title,Modifier.weight(1f),style=MaterialTheme.typography.titleLarge,maxLines=2)
            if(s.screen !in listOf(SsScreen.FIND,SsScreen.SELECT,SsScreen.COMPENSATION,SsScreen.CONTACT,SsScreen.ISSUE,SsScreen.REVIEW,SsScreen.REVIEW_DONE))TextButton(vm::reload,enabled=enabled,contentPadding=PaddingValues(0.dp),modifier=Modifier.testTag("ss_reload")){Text("Tải lại",style=rmText(11))}
        }
    },bottomBar={
        if(s.screen==SsScreen.LIST)DieuHuongSanPham(1,enabled,onTab)
        else if(s.screen !in listOf(SsScreen.SELECT,SsScreen.MATCHES,SsScreen.NOTIFICATIONS,SsScreen.COMPENSATION,SsScreen.NONE))RmActionBar{
            when(s.screen){
                SsScreen.FIND->SsButton("Tìm vòng phù hợp","ss_search_submit",enabled){vm.search()}
                SsScreen.PREVIEW->if(s.preview?.coTheKhoiTao==true)SsButton("Gửi đề nghị cho 2 thành viên","ss_invite_submit",enabled){vm.invite()}
                SsScreen.DETAIL->{Row(horizontalArrangement=Arrangement.spacedBy(8.dp)){
                    SsButton("Chat nhóm","ss_chat_open",enabled&&s.circle?.hoiThoaiId!=null,true,Modifier.weight(1f)){vm.chat()}
                    SsButton("Xác nhận 3 bên","ss_consent_open",enabled&&s.circle?.phuongAnHienTai!=null,true,Modifier.weight(1f)){vm.consent()}}
                    val v=s.circle
                    if(v!=null&&v.trangThai in listOf("SanSangBanGiao","DangBanGiao","TamDung","HoanTat"))SsButton(if(v.trangThai=="HoanTat")"Hoàn tất và đánh giá" else "Theo dõi bàn giao","ss_delivery_open",enabled){if(v.trangThai=="HoanTat")vm.review()else vm.delivery()}
                    else if(v?.coTheHuy==true)TextButton({reason="";dialog="cancel"},enabled=enabled,modifier=Modifier.testTag("ss_cancel_open")){Text("Hủy đề nghị")}
                }
                SsScreen.CONSENT->{val v=s.circle
                    if(v?.coThePhanHoi==true)Row(horizontalArrangement=Arrangement.spacedBy(8.dp)){
                        SsButton("Từ chối","ss_reject_open",enabled,true,Modifier.weight(1f)){reason="";dialog="reject"}
                        SsButton("Xác nhận","ss_accept",enabled,modifier=Modifier.weight(1f)){vm.respond(true)}}
                    if(v!=null&&v.trangThai in listOf("SanSangBanGiao","DangBanGiao","HoanTat","TamDung"))SsButton("Theo dõi bàn giao","ss_consent_delivery",enabled){vm.delivery()}
                    SsButton("Chat và thương lượng","ss_consent_chat",enabled&&v?.hoiThoaiId!=null,true){vm.chat()}
                }
                SsScreen.CHAT->Row(verticalAlignment=Alignment.CenterVertically,horizontalArrangement=Arrangement.spacedBy(8.dp)){
                    IconButton({picker.launch(PickVisualMediaRequest(ActivityResultContracts.PickVisualMedia.ImageOnly))},enabled=enabled&&s.chat!=null,modifier=Modifier.size(48.dp).testTag("ss_chat_image")){Icon(painterResource(R.drawable.ic_fg_commerce_paperclip),"Gửi ảnh nhóm",Modifier.size(20.dp),tint=ReMarketPalette.Muted)}
                    Box(Modifier.weight(1f)){RmField("",s.draft,vm::draft,"Nhập tin nhắn...","ss_chat_message",enabled=enabled,multiline=true,maxLines=4,cornerRadius=24.dp)}
                    IconButton(vm::send,enabled=enabled&&s.chat!=null,modifier=Modifier.size(48.dp).testTag("ss_chat_send")){Icon(painterResource(R.drawable.ic_fg_commerce_send),"Gửi tin nhắn",Modifier.size(20.dp),tint=ReMarketPalette.Primary)}
                }
                SsScreen.DELIVERY->{val d=s.delivery;val incoming=d?.vanChuyen?.firstOrNull{it.nguoiNhanId==profile.id}
                    if(d?.coTheTaoVanChuyen==true)SsButton("Tạo 3 đơn vận chuyển mô phỏng","ss_book_shipments",enabled){vm.book()}
                    if(incoming?.coTheNhanDungHang==true)SsButton("Xác nhận đã nhận đúng hàng","ss_receive_submit",enabled){vm.receive(incoming)}
                    if(d?.tienDo?.hoanTat==true)SsButton("Đánh giá hai thành viên","ss_review_open",enabled){vm.review()}
                    if(incoming?.ngayNhanDungHang!=null&&s.evidence.isNotEmpty())SsButton("Lưu ảnh bằng chứng","ss_evidence_submit",enabled){vm.saveEvidence()}
                    TextButton(vm::chat,enabled=enabled&&s.circle?.hoiThoaiId!=null,modifier=Modifier.testTag("ss_delivery_chat")){Text("Chat nhóm")}
                }
                SsScreen.CONTACT->SsButton("Xác nhận thông tin","ss_contact_submit",enabled){vm.saveContact()}
                SsScreen.ISSUE->SsButton("Gửi báo cáo và tạm dừng vòng","ss_issue_submit",enabled){vm.report()}
                SsScreen.REVIEW->if(s.reviews.isEmpty())SsButton("Gửi 2 đánh giá","ss_review_submit",enabled&&s.circle?.trangThai=="HoanTat"){vm.rate()}else SsButton("Vòng trao đổi của tôi","ss_review_history",enabled,true){vm.list("hoantat")}
                SsScreen.REVIEW_DONE->SsButton("Về vòng trao đổi của tôi","ss_review_done_history",enabled){vm.list("hoantat")}
                else->Unit
            }
        }
    }){padding->Column(Modifier.fillMaxSize().padding(padding).consumeWindowInsets(padding)){
        if(s.busy||reading)LinearProgressIndicator(Modifier.fillMaxWidth().testTag("ss_loading"))
        s.notice?.let{Text(it,Modifier.fillMaxWidth().background(ReMarketPalette.Soft).padding(horizontal=24.dp,vertical=8.dp).testTag("ss_notice"),style=rmText(12),color=ReMarketPalette.Primary)}
        s.error?.let{message->Row(Modifier.fillMaxWidth().background(ReMarketPalette.Warm).padding(horizontal=24.dp,vertical=6.dp),verticalAlignment=Alignment.CenterVertically){Text(message,Modifier.weight(1f).testTag("ss_error"),style=rmText(12),color=ReMarketPalette.Error);TextButton(vm::reload,enabled=enabled){Text("Tải lại")}}}
        if(s.pollError)Text("Chưa cập nhật được dữ liệu. Hãy kiểm tra kết nối và tải lại.",Modifier.padding(horizontal=24.dp),style=rmText(11),color=ReMarketPalette.Muted)
        key(s.screen){Box(Modifier.weight(1f)){when(s.screen){
            SsScreen.FIND->SsFind(s,vm,loader);SsScreen.SELECT->SsSelect(s,vm,loader);SsScreen.MATCHES->SsMatches(s,vm,profile.id);SsScreen.PREVIEW->SsPreviewScreen(s,vm,loader,profile.id)
            SsScreen.LIST->SsCycleList(s,vm);SsScreen.DETAIL,SsScreen.CONSENT->SsCircleScreen(s,vm,loader,profile.id,s.screen==SsScreen.CONSENT)
            SsScreen.COMPENSATION,SsScreen.CHAT->SsChatScreen(s,vm,loader,profile.id);SsScreen.DELIVERY->SsDeliveryScreen(s,vm,profile,loader){picker.launch(PickVisualMediaRequest(ActivityResultContracts.PickVisualMedia.ImageOnly))}
            SsScreen.CONTACT->SsContactScreen(s,vm);SsScreen.ISSUE->SsIssueScreen(s,vm);SsScreen.REVIEW->SsReviewScreen(s,vm,loader,profile.id)
            SsScreen.REVIEW_DONE->SsContent("ss_review_success"){TdNote("Cảm ơn bạn đã đánh giá!","Đã lưu ${s.reviews.size} đánh giá cho hai thành viên vào giao dịch ${s.circle?.ma.orEmpty()}.")}
            SsScreen.NOTIFICATIONS->SsNotifications(s,vm);else->Unit
        }}}
    }}
    if(s.screen==SsScreen.COMPENSATION)ModalBottomSheet(onDismissRequest={if(enabled)vm.back()},sheetState=rememberModalBottomSheetState(skipPartiallyExpanded=true),containerColor=ReMarketPalette.Background){
        Column(Modifier.fillMaxWidth().heightIn(max=650.dp).imePadding()){
            Box(Modifier.weight(1f)){SsCompensation(s,vm,profile.id)}
            s.error?.let{Text(it,Modifier.padding(horizontal=24.dp).testTag("ss_compensation_error"),style=rmText(12),color=ReMarketPalette.Error)}
            Box(Modifier.padding(horizontal=24.dp,vertical=12.dp)){SsButton("Gửi phương án mới","ss_proposal_submit",enabled&&s.circle?.phuongAnHienTai?.id==s.baseVersion){vm.propose()}}
        }
    }
    if(dialog!=null)AlertDialog(onDismissRequest={if(enabled)dialog=null},title={Text(if(dialog=="reject")"Từ chối tham gia" else "Hủy đề nghị")},text={Column(verticalArrangement=Arrangement.spacedBy(12.dp)){
        Text("Vòng sẽ dừng, các thành viên sẽ được thông báo.",style=rmText(13));RmField("Lý do",reason,{reason=it.take(500)},"Ít nhất 5 ký tự","ss_action_reason",enabled=enabled,multiline=true)
    }},confirmButton={TextButton({if(reason.trim().length<5)vm.error("Nhập lý do ít nhất 5 ký tự.")else{if(dialog=="reject")vm.respond(false,reason.trim())else vm.cancel(reason.trim());dialog=null}},enabled=enabled,modifier=Modifier.testTag("ss_action_confirm")){Text("Xác nhận")}},dismissButton={TextButton({dialog=null},enabled=enabled){Text("Quay lại")}})
}
