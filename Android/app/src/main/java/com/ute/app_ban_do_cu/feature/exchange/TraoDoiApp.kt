package com.ute.app_ban_do_cu.feature.exchange

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
import androidx.lifecycle.compose.LocalLifecycleOwner
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import androidx.lifecycle.repeatOnLifecycle
import coil.ImageLoader
import com.ute.app_ban_do_cu.R
import com.ute.app_ban_do_cu.core.designsystem.*
import com.ute.app_ban_do_cu.domain.model.HoSo
import com.ute.app_ban_do_cu.feature.catalog.AnhChon
import com.ute.app_ban_do_cu.feature.catalog.DieuHuongSanPham
import kotlinx.coroutines.*
import java.io.ByteArrayOutputStream
import java.io.IOException

@Composable fun TraoDoiApp(vm:TraoDoiViewModel,loader:ImageLoader,profile:HoSo,onTab:(Int)->Unit,onExit:()->Unit,
    onChat:(String)->Unit,onContact:(String)->Unit,onSupport:(String)->Unit,onSmart:()->Unit){
    val s by vm.uiState.collectAsStateWithLifecycle();val context=LocalContext.current;val scope=rememberCoroutineScope();val lifecycle=LocalLifecycleOwner.current.lifecycle
    val snack=remember{SnackbarHostState()};var reading by remember{mutableStateOf(false)};var dialog by remember{mutableStateOf<String?>(null)};var reason by remember{mutableStateOf("")}
    val picker=rememberLauncherForActivityResult(ActivityResultContracts.PickMultipleVisualMedia(if(s.man==ManTraoDoi.GIAO_NHAN)5 else 6)){uris->if(uris.isNotEmpty())scope.launch{
        reading=true
        try{
            val photos=withContext(Dispatchers.IO){uris.map{uri->val bytes=context.contentResolver.openInputStream(uri)?.use{input->
                val output=ByteArrayOutputStream();val buffer=ByteArray(16384);while(true){val n=input.read(buffer);if(n<0)break;if(output.size()+n>5*1024*1024)throw IOException("Mỗi ảnh tối đa 5 MB.");output.write(buffer,0,n)};output.toByteArray()
            }?:throw IOException("Không đọc được ảnh đã chọn.");AnhChon(uri.toString(),bytes)}}
            if(s.man==ManTraoDoi.GIAO_NHAN)vm.themBangChung(photos)else vm.themAnh(photos)
        }catch(ex:IOException){vm.loi(ex.message?:"Không đọc được ảnh.")}catch(_:SecurityException){vm.loi("Không có quyền đọc ảnh đã chọn.")}finally{reading=false}
    }}
    val enabled=!s.busy&&!reading
    val back={if(enabled){vm.back();if(vm.uiState.value.man==ManTraoDoi.NONE)onExit()}}
    BackHandler{back()}
    LaunchedEffect(s.notice){s.notice?.let{snack.showSnackbar(it)}}
    LaunchedEffect(s.man,lifecycle){lifecycle.repeatOnLifecycle(Lifecycle.State.STARTED){while(isActive){vm.capNhat();delay(3000)}}}
    val title=when(s.man){ManTraoDoi.KHAM_PHA,ManTraoDoi.TIN_TOI->"Khám phá trao đổi";ManTraoDoi.FORM_TIN->if(s.form.id==null)"Đăng tin trao đổi" else "Sửa tin trao đổi";
        ManTraoDoi.TIN_XONG->"Đăng tin trao đổi";ManTraoDoi.CHI_TIET->"Chi tiết trao đổi";ManTraoDoi.CHON_MON->"Chọn đồ của tôi";ManTraoDoi.TAO_DE_NGHI->"Tạo đề nghị trao đổi";
        ManTraoDoi.DE_NGHI->"Trao đổi của tôi";ManTraoDoi.CHI_TIET_DE_NGHI->"Đề nghị trao đổi";ManTraoDoi.DE_XUAT_LAI->"Đề xuất lại";ManTraoDoi.GIAO_NHAN->"Xác nhận giao nhận";
        ManTraoDoi.DANH_GIA->"Đánh giá trao đổi";ManTraoDoi.DANH_GIA_XONG->"Đã gửi đánh giá";else->"Trao đổi"}
    TraoDoiTheme{Scaffold(modifier=Modifier.imePadding(),containerColor=ReMarketPalette.Background,
        topBar={Row(Modifier.fillMaxWidth().statusBarsPadding().heightIn(min=60.dp).padding(horizontal=16.dp),verticalAlignment=Alignment.CenterVertically,horizontalArrangement=Arrangement.spacedBy(8.dp)){
            IconButton(back,enabled=enabled,modifier=Modifier.size(48.dp).testTag("exchange_back")){Icon(painterResource(R.drawable.ic_fg_exchange_back),"Quay lại",Modifier.size(22.dp),tint=Color.Unspecified)}
            Text(title,Modifier.weight(1f),style=MaterialTheme.typography.titleLarge,maxLines=2)
            if(s.man in listOf(ManTraoDoi.CHI_TIET_DE_NGHI,ManTraoDoi.DE_NGHI,ManTraoDoi.CHI_TIET))TextButton(vm::taiLai,enabled=enabled,modifier=Modifier.testTag("exchange_reload"),contentPadding=PaddingValues(0.dp)){Text("Tải lại",style=rmText(11))}
        }},
        bottomBar={when(s.man){
            ManTraoDoi.KHAM_PHA,ManTraoDoi.TIN_TOI,ManTraoDoi.DE_NGHI->DieuHuongSanPham(1,enabled,onTab)
            ManTraoDoi.CHI_TIET->s.detail?.let{d->TdFooter{
                if(s.own){
                    if(d.coTheChinhSua)TdButton("Chỉnh sửa",vm::suaTin,Modifier.testTag("exchange_edit_listing"),enabled,outlined=true)
                    if(d.trangThai in listOf("BanNhap","TuChoi","DaAn"))TdButton("Gửi duyệt",{vm.chuyenTin("gui-duyet")},Modifier.testTag("exchange_submit_review"),enabled)
                    Row(horizontalArrangement=Arrangement.spacedBy(12.dp)){
                        if(d.trangThai in listOf("DangHienThi","ChoDuyet"))TextButton({dialog="hide"},enabled=enabled,modifier=Modifier.testTag("exchange_hide_listing")){Text("Ẩn tin")}
                        if(d.trangThai!="DangGiuCho")TextButton({dialog="delete"},enabled=enabled,modifier=Modifier.testTag("exchange_delete_listing")){Text("Xóa tin")}
                    }
                }else if(d.nguoiDangId==profile.id)TdButton("Quản lý tin của bạn",{vm.chiTiet(d.id,true)},enabled=enabled)
                else Row(horizontalArrangement=Arrangement.spacedBy(10.dp)){
                    TdButton("Nhắn tin",{onContact(d.id)},Modifier.weight(1f).testTag("exchange_contact"),enabled,true)
                    TdButton("Đề nghị trao đổi",vm::chonMon,Modifier.weight(1f).testTag("exchange_propose"),enabled&&d.traoDoi?.choPhepMotMot==true)
                }
            }}
            ManTraoDoi.CHON_MON->TdFooter{TdButton("Tiếp tục · ${if(s.selected!=null)1 else 0} món đã chọn",vm::tiepTuc,Modifier.testTag("exchange_select_continue"),enabled&&s.selected!=null)}
            ManTraoDoi.TAO_DE_NGHI->TdFooter{TdButton("Gửi đề nghị",vm::guiDeNghi,Modifier.testTag("exchange_send_offer"),enabled)}
            ManTraoDoi.DE_XUAT_LAI->TdFooter{TdButton("Gửi đề xuất mới",vm::guiDoi,Modifier.testTag("exchange_counter_submit"),enabled)}
            ManTraoDoi.CHI_TIET_DE_NGHI->s.offer?.let{o->TdFooter{
                Row(horizontalArrangement=Arrangement.spacedBy(8.dp)){
                    TdButton("Nhắn đối tác",{vm.chat(onChat)},Modifier.weight(1f).testTag("exchange_chat"),enabled,true)
                    if(o.coTheDeXuatLai)TdButton("Đề xuất lại",vm::moDoi,Modifier.weight(1f).testTag("exchange_counter_open"),enabled,true)
                }
                if(o.coTheDongY)TdButton("Chấp nhận đề nghị",{vm.phanHoi(true)},Modifier.testTag("exchange_accept"),enabled)
                if(o.coTheGiaoNhan)TdButton("Xác nhận giao nhận",vm::moGiaoNhan,Modifier.testTag("exchange_handover_open"),enabled)
                if(o.coTheDanhGia)TdButton("Đánh giá đối tác",vm::moDanhGia,Modifier.testTag("exchange_rate_open"),enabled)
                Row(Modifier.fillMaxWidth(),horizontalArrangement=Arrangement.SpaceBetween){
                    if(o.coTheTuChoi)TextButton({reason="";dialog="reject"},enabled=enabled,modifier=Modifier.testTag("exchange_reject")){Text("Từ chối")}
                    if(o.coTheHuy)TextButton({reason="";dialog="cancel"},enabled=enabled,modifier=Modifier.testTag("exchange_cancel")){Text("Hủy đề nghị")}
                    if(o.xacNhan.any{it.daNhan})TextButton({onSupport(o.ma)},enabled=enabled,modifier=Modifier.testTag("exchange_support")){Text("Báo vấn đề giao nhận")}
                }
            }}
            ManTraoDoi.GIAO_NHAN->TdFooter{TdButton(if(s.offer?.daXacNhanCuaToi==true&&s.evidence.isNotEmpty())"Tiếp tục tải bằng chứng" else "Xác nhận đã nhận",vm::xacNhan,Modifier.testTag("exchange_confirm_received"),enabled&&(s.offer?.coTheGiaoNhan==true||s.offer?.daXacNhanCuaToi==true))}
            ManTraoDoi.DANH_GIA->TdFooter{TdButton("Gửi đánh giá",vm::guiDanhGia,Modifier.testTag("exchange_rating_submit"),enabled)}
            ManTraoDoi.DANH_GIA_XONG->TdFooter{TdButton("Về khám phá trao đổi",vm::khamPha,Modifier.testTag("exchange_rating_explore"),enabled);TdButton("Lịch sử trao đổi",{vm.deNghi("hoantat")},Modifier.testTag("exchange_rating_history"),enabled,true)}
            else->Unit
        }},snackbarHost={SnackbarHost(snack)}){padding->Column(Modifier.fillMaxSize().padding(padding).consumeWindowInsets(padding)){
            if(s.busy||reading)LinearProgressIndicator(Modifier.fillMaxWidth().testTag("exchange_loading"))
            s.error?.let{text->Row(Modifier.fillMaxWidth().background(ReMarketPalette.Warm).padding(horizontal=20.dp,vertical=6.dp),verticalAlignment=Alignment.CenterVertically){Text(text,Modifier.weight(1f).testTag("exchange_error"),style=rmText(12),color=ReMarketPalette.Error);TextButton(vm::taiLai,enabled=enabled){Text("Thử lại")}}}
            if(s.pollError)Text("Chưa cập nhật được dữ liệu. Bạn có thể tải lại.",Modifier.padding(horizontal=24.dp),style=rmText(11),color=ReMarketPalette.Muted)
            Box(Modifier.weight(1f)){when(s.man){
                ManTraoDoi.KHAM_PHA->TraoDoiExplore(s,vm,loader,onSmart);ManTraoDoi.TIN_TOI->TraoDoiMyListings(s,vm,loader,profile.tinhThanh.orEmpty())
                ManTraoDoi.FORM_TIN->FormTraoDoiScreen(s,vm,loader){picker.launch(PickVisualMediaRequest(ActivityResultContracts.PickVisualMedia.ImageOnly))}
                ManTraoDoi.TIN_XONG->TinTraoDoiXong(s,vm);ManTraoDoi.CHI_TIET->ChiTietTraoDoiScreen(s,loader,profile)
                ManTraoDoi.CHON_MON->ChonMonScreen(s,vm,loader);ManTraoDoi.TAO_DE_NGHI->DeNghiFormScreen(s,vm,loader,false)
                ManTraoDoi.DE_NGHI->DeNghiListScreen(s,vm,loader);ManTraoDoi.CHI_TIET_DE_NGHI->DeNghiDetailScreen(s,loader)
                ManTraoDoi.DE_XUAT_LAI->DeNghiFormScreen(s,vm,loader,true)
                ManTraoDoi.GIAO_NHAN->GiaoNhanTraoDoiScreen(s,vm,loader){picker.launch(PickVisualMediaRequest(ActivityResultContracts.PickVisualMedia.ImageOnly))}
                ManTraoDoi.DANH_GIA->DanhGiaTraoDoiScreen(s,vm,loader);ManTraoDoi.DANH_GIA_XONG->DanhGiaTraoDoiXong(s,loader);else->Unit
            }}
        }}
        if(dialog!=null)AlertDialog(onDismissRequest={dialog=null},title={Text(when(dialog){"hide"->"Ẩn tin trao đổi?";"delete"->"Xóa tin trao đổi?";"reject"->"Từ chối đề nghị?";else->"Hủy đề nghị?"})},
            text={if(dialog in listOf("cancel","reject"))RmField("Lý do",reason,{reason=it.take(500)},"Nhập lý do","exchange_action_reason",multiline=true)else Text("Tin sẽ ngừng hiển thị. Lịch sử giao dịch liên quan vẫn được giữ.")},
            confirmButton={TextButton({val action=dialog;dialog=null;when(action){"hide"->vm.chuyenTin("an");"delete"->vm.chuyenTin("xoa");"reject"->vm.phanHoi(false,reason);"cancel"->vm.huy(reason)}},
                enabled=dialog !in listOf("cancel","reject")||reason.trim().length>=5,modifier=Modifier.testTag("exchange_action_confirm")){Text("Xác nhận")}},dismissButton={TextButton({dialog=null}){Text("Quay lại")}})
    }
}
@Composable private fun TdFooter(content:@Composable ColumnScope.()->Unit){RmActionBar(content=content)}
