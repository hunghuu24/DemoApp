package com.ute.app_ban_do_cu.feature.commerce

import androidx.activity.compose.BackHandler
import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.activity.result.PickVisualMediaRequest
import androidx.activity.result.contract.ActivityResultContracts
import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.text.BasicTextField
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
import com.ute.app_ban_do_cu.feature.catalog.DieuHuongSanPham
import kotlinx.coroutines.*
import java.io.ByteArrayOutputStream
import java.io.IOException

@Composable
fun MuaBanApp(vm:MuaBanViewModel,loader:ImageLoader,profile:HoSo,onTab:(Int)->Unit,onExit:()->Unit) {
    val s by vm.uiState.collectAsStateWithLifecycle()
    val lifecycle=LocalLifecycleOwner.current.lifecycle
    val context=LocalContext.current
    val scope=rememberCoroutineScope()
    val snack=remember{SnackbarHostState()}
    var dialog by remember{mutableStateOf<String?>(null)}
    var reason by remember{mutableStateOf("")}
    var reading by remember{mutableStateOf(false)}
    val picker=rememberLauncherForActivityResult(ActivityResultContracts.PickVisualMedia()){uri->if(uri!=null)scope.launch{
        reading=true
        try {
            val bytes=withContext(Dispatchers.IO){context.contentResolver.openInputStream(uri)?.use{input->
                val out=ByteArrayOutputStream();val buffer=ByteArray(16*1024)
                while(true){val n=input.read(buffer);if(n<0)break;if(out.size()+n>5*1024*1024)throw IOException("Ảnh tối đa 5 MB.");out.write(buffer,0,n)}
                out.toByteArray()
            }?:throw IOException("Không đọc được ảnh.")}
            vm.guiAnh(bytes)
        }catch(ex:IOException){vm.loi(ex.message?:"Không đọc được ảnh.")}catch(_:SecurityException){vm.loi("Chưa có quyền đọc ảnh đã chọn.")}
        finally{reading=false}
    }}
    LaunchedEffect(s.man,lifecycle){lifecycle.repeatOnLifecycle(Lifecycle.State.STARTED){while(isActive){delay(3000);vm.lamMoiAmTham()}}}
    LaunchedEffect(s.notice){s.notice?.let{snack.showSnackbar(it)}}
    val enabled=!s.busy&&!reading
    val back={if(enabled){vm.back();if(vm.uiState.value.man==ManMuaBan.NONE)onExit()}}
    BackHandler {back()}
    val title=when(s.man){ManMuaBan.HOP_THU->"Tin nhắn";ManMuaBan.CHAT->s.chat?.doiTac?.ifBlank{"Thương lượng giá"}?:"Thương lượng giá";
        ManMuaBan.PHIEU->"Phiếu giảm giá";ManMuaBan.CHECKOUT->"Xác nhận mua hàng";ManMuaBan.DON_HANG->"Đơn hàng của tôi";
        ManMuaBan.CHI_TIET_DON->"Chi tiết đơn hàng";ManMuaBan.DANH_GIA,ManMuaBan.DANH_GIA_XONG->"Đánh giá";
        ManMuaBan.HO_TRO->if(s.adminSupport)"Quản lý hỗ trợ" else "Lịch sử yêu cầu";ManMuaBan.TAO_HO_TRO->"Gửi yêu cầu hỗ trợ";
        ManMuaBan.CHI_TIET_HO_TRO->"Theo dõi yêu cầu hỗ trợ";else->"ReMarket"}
    MuaBanTheme {
        Scaffold(modifier=Modifier.imePadding(),containerColor=MuaBanColors.Background,
            topBar={Row(Modifier.fillMaxWidth().background(Color.White).statusBarsPadding().heightIn(min=60.dp).padding(horizontal=16.dp),verticalAlignment=Alignment.CenterVertically){
                IconButton(back,enabled=enabled,modifier=Modifier.size(48.dp).testTag("commerce_back")){
                    if(s.man==ManMuaBan.CHECKOUT)Icon(painterResource(R.drawable.ic_fg_checkout_back),"Quay lại",Modifier.size(14.1076.dp),tint=Color.Unspecified)
                    else RmIcon(R.drawable.ic_rm_arrow_left,description="Quay lại")
                }
                Text(title,Modifier.weight(1f).padding(start=12.dp),style=MaterialTheme.typography.titleLarge,maxLines=2)
                if(s.man in listOf(ManMuaBan.CHI_TIET_DON,ManMuaBan.CHAT,ManMuaBan.CHI_TIET_HO_TRO)) IconButton(vm::taiLai,enabled=enabled,modifier=Modifier.testTag("commerce_reload")){RmIcon(R.drawable.ic_rm_eye,description="Tải lại")}
            }},
            bottomBar={when(s.man){
                ManMuaBan.HOP_THU->DieuHuongSanPham(3,enabled,onTab)
                ManMuaBan.CHAT->if(s.chat!=null)Row(Modifier.fillMaxWidth().background(Color.White).navigationBarsPadding().padding(horizontal=16.dp,vertical=10.dp),verticalAlignment=Alignment.CenterVertically,horizontalArrangement=Arrangement.spacedBy(8.dp)){
                    IconButton({picker.launch(PickVisualMediaRequest(ActivityResultContracts.PickVisualMedia.ImageOnly))},enabled=enabled,modifier=Modifier.testTag("chat_photo")){
                        Icon(painterResource(R.drawable.ic_fg_commerce_paperclip),"Gửi ảnh",Modifier.size(20.dp),tint=MuaBanColors.Muted)
                    }
                    BasicTextField(s.tinNhan,vm::soanTin,Modifier.weight(1f).testTag("chat_message").background(ReMarketPalette.SurfaceMuted,RoundedCornerShape(24.dp))
                        .border(1.dp,ReMarketPalette.Border,RoundedCornerShape(24.dp)).heightIn(min=48.dp).padding(horizontal=16.dp,vertical=14.dp),
                        enabled=enabled,maxLines=4,textStyle=rmText(14).copy(color=MuaBanColors.Text),decorationBox={inner->Box{if(s.tinNhan.isEmpty())Text("Nhập tin nhắn...",style=rmText(14),color=MuaBanColors.Muted);inner()}})
                    IconButton(vm::guiTin,enabled=enabled&&s.tinNhan.isNotBlank(),modifier=Modifier.testTag("chat_send").background(if(s.tinNhan.isNotBlank())ReMarketPalette.Primary else ReMarketPalette.Soft,CircleShape)){
                        Icon(painterResource(R.drawable.ic_fg_commerce_send),"Gửi",Modifier.size(18.dp),tint=if(s.tinNhan.isNotBlank())Color.White else MuaBanColors.Green)
                    }
                }
                ManMuaBan.PHIEU->MbFooter{MbButton("Hoàn tất",vm::taoPhieu,Modifier.testTag("coupon_submit"),enabled)}
                ManMuaBan.CHECKOUT->MbFooter{MbButton("Tạo đơn mua hàng",vm::datHang,Modifier.testTag("checkout_submit"),enabled&&s.quote!=null)}
                ManMuaBan.CHI_TIET_DON->s.order?.let{o->MbFooter{
                    if(o.coTheThanhToan)MbButton("Thanh toán mô phỏng",{dialog="pay"},Modifier.testTag("pay_order"),enabled)
                    o.buocVanChuyen.forEach{next->MbButton(when(next){"DaLayHang"->"Bàn giao mô phỏng";"DangVanChuyen"->"Chuyển sang đang vận chuyển";"DaGiao"->"Mô phỏng đã giao hàng";else->"Mô phỏng giao thất bại"},{vm.chuyenShip(next)},Modifier.testTag("ship_$next"),enabled,outlined=next=="GiaoThatBai")}
                    if(o.coTheNhanHang)MbButton("Đã nhận được hàng",{dialog="receive"},Modifier.testTag("receive_order"),enabled)
                    Row(horizontalArrangement=Arrangement.spacedBy(10.dp)){
                        if(o.coTheDanhGia)MbButton("Đánh giá",vm::moDanhGia,Modifier.weight(1f).testTag("rate_order"),enabled)
                        MbButton("Gửi hỗ trợ",{vm.moHoTro(o)},Modifier.weight(1f).testTag("order_support"),enabled,outlined=true)
                    }
                    if(o.coTheHuy) TextButton({reason="";dialog="cancel"},enabled=enabled,modifier=Modifier.testTag("cancel_order")){Text("Hủy đơn")}
                }}
                ManMuaBan.DANH_GIA->MbFooter{MbButton("Gửi đánh giá",vm::guiDanhGia,Modifier.testTag("rating_submit"),enabled)}
                ManMuaBan.DANH_GIA_XONG->MbFooter{MbButton("Quay lại đơn hàng",vm::veDon,Modifier.testTag("rating_back_order"),enabled)}
                ManMuaBan.TAO_HO_TRO->MbFooter{MbButton("Gửi yêu cầu hỗ trợ",vm::guiHoTro,Modifier.testTag("support_submit"),enabled)}
                ManMuaBan.CHI_TIET_HO_TRO->if(!s.adminSupport)MbFooter{MbButton("Gửi yêu cầu mới",{vm.moHoTro()},enabled=enabled,outlined=true)}
                else->Unit
            }},snackbarHost={SnackbarHost(snack)}){padding->
            Column(Modifier.fillMaxSize().padding(padding).consumeWindowInsets(padding)){
                if(s.busy||reading)LinearProgressIndicator(Modifier.fillMaxWidth().testTag("commerce_loading"))
                if(s.error!=null)Row(Modifier.fillMaxWidth().background(ReMarketPalette.Warm).padding(horizontal=20.dp,vertical=8.dp),verticalAlignment=Alignment.CenterVertically){
                    Text(s.error!!,Modifier.weight(1f).testTag("commerce_error"),style=rmText(12),color=ReMarketPalette.Error)
                    TextButton(vm::taiLai,enabled=enabled){Text("Tải lại")}
                }
                if(s.pollError)Text("Chưa cập nhật được dữ liệu. Bạn có thể bấm tải lại.",Modifier.padding(horizontal=24.dp,vertical=6.dp),style=rmText(11),color=MuaBanColors.Muted)
                Box(Modifier.weight(1f)){when(s.man){
                    ManMuaBan.HOP_THU->HoiThoaiListScreen(s,vm,loader)
                    ManMuaBan.CHAT->ChatMuaScreen(s,vm,loader,profile)
                    ManMuaBan.PHIEU->PhieuScreen(s,vm,loader,profile)
                    ManMuaBan.CHECKOUT->CheckoutScreen(s,vm,loader)
                    ManMuaBan.DON_HANG->DonHangListScreen(s,vm,loader)
                    ManMuaBan.CHI_TIET_DON->DonHangDetailScreen(s,vm,loader)
                    ManMuaBan.DANH_GIA->DanhGiaScreen(s,vm,loader)
                    ManMuaBan.DANH_GIA_XONG->DanhGiaXongScreen()
                    ManMuaBan.HO_TRO->HoTroListScreen(s,vm)
                    ManMuaBan.TAO_HO_TRO->TaoHoTroScreen(s,vm)
                    ManMuaBan.CHI_TIET_HO_TRO->HoTroDetailScreen(s,vm)
                    else->Unit
                }}
            }
        }
        if(dialog=="pay")AlertDialog(onDismissRequest={dialog=null},title={Text("Thanh toán mô phỏng")},text={Text("Không trừ tiền thật. Chọn kết quả để kiểm tra trạng thái đơn trong ứng dụng.")},
            confirmButton={TextButton({dialog=null;vm.thanhToan(true)},Modifier.testTag("payment_success")){Text("Thành công")}},
            dismissButton={Column{TextButton({dialog=null;vm.thanhToan(false)},Modifier.testTag("payment_failure")){Text("Thất bại")};TextButton({dialog=null}){Text("Đóng")}}})
        if(dialog=="receive")AlertDialog(onDismissRequest={dialog=null},title={Text("Xác nhận đã nhận hàng?")},text={Text("Chỉ xác nhận khi bạn đã nhận và kiểm tra món đồ. Khoản giữ mô phỏng sẽ được giải ngân và đơn hoàn tất.")},
            confirmButton={TextButton({dialog=null;vm.nhanHang()},Modifier.testTag("receive_confirm")){Text("Xác nhận")}},dismissButton={TextButton({dialog=null}){Text("Hủy")}})
        if(dialog=="cancel")AlertDialog(onDismissRequest={dialog=null},title={Text("Hủy đơn hàng")},text={RmField("Lý do",reason,{reason=it.take(500)},"Nhập lý do hủy","cancel_reason",multiline=true)},
            confirmButton={TextButton({dialog=null;vm.huyDon(reason)},enabled=reason.trim().length>=5,modifier=Modifier.testTag("cancel_confirm")){Text("Xác nhận hủy")}},dismissButton={TextButton({dialog=null}){Text("Tiếp tục đơn")}})
    }
}
@Composable
private fun MbFooter(content:@Composable ColumnScope.()->Unit){RmActionBar(content=content)}
