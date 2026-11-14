// Read-only audit of the83 source frames and their existing Compose implementations.
// node tools/Audit-FigmaScreens.cjs <summary.json>
const fs=require('fs'),path=require('path');
const source=JSON.parse(fs.readFileSync(process.argv[2],'utf8'));
const out=path.resolve(__dirname,'../docs/qa/full-audit');fs.mkdirSync(out,{recursive:true});
const mappings=new Map();
function add(group,file,entries){for(const [ids,state,shot] of entries)for(const id of ids.split(','))mappings.set(id,{group,file:`Android/app/src/main/java/com/ute/app_ban_do_cu/feature/${file}`,state,screenshot:`screenshots-final/${group}/${shot}.png`});}
add('account','auth/AuthScreens.kt',[
 ['2:2955','SplashScreen','splash'],['2:2983','DangNhapScreen','login'],['2:3019','DangKyScreen','register'],['2:3064','QuenMatKhauScreen','forgot-password'],['92:558','MatKhauScreen / đổi mật khẩu','change-password']]);
add('account','account/HoSoScreens.kt',[
 ['2:3096','HoSoScreen','profile'],['2:3139','SuaHoSoScreen','edit-profile'],['2:3179','CaiDatScreen','settings']]);
add('catalog','catalog/KhamPhaScreens.kt',[
 ['4:461','HomeSanPhamScreen','home'],['4:556','TimKiemScreen / từ khóa','search'],['4:617','DanhMucScreen','categories'],['4:684','TimKiemScreen / danh mục','search-filter']]);
add('catalog','catalog/TinDangScreens.kt',[
 ['4:755,128:6,128:67','ChiTietTinScreen / sản phẩm từ API','product-detail'],['114:2,114:115','FormTinScreen / tạo, sửa','listing-form'],['114:292','ChiTietTinScreen / preview','listing-preview'],['114:252','GuiThanhCongScreen','listing-success']]);
add('catalog','catalog/QuanLyTinScreens.kt',[
 ['114:66','XacNhanDaBanScreen','sold-confirmation'],['114:192','QuanLyChiTietScreen','listing-manage'],['114:332','TinCuaToiScreen','my-listings']]);
add('commerce','commerce/ChatScreens.kt',[
 ['85:2,128:139','ChatScreen / thương lượng','chat-discount-accepted'],['188:2','ChatScreen / phiếu được chấp nhận','chat-discount-accepted'],['90:16,90:78','HopThuScreen / đã đọc, chưa đọc','inbox-seller'],['128:174','TaoPhieuScreen','coupon-form']]);
add('commerce','commerce/DonHangScreens.kt',[
 ['44:191','CheckoutScreen / giá gốc','checkout-top'],['194:72','CheckoutScreen / áp dụng phiếu','checkout-discount'],['44:223','DonHangListScreen','orders-seller'],['44:248','DonHangDetailScreen','order-paid'],['77:65','DonHangDetailScreen / theo dõi','order-delivered'],['146:248','DanhGiaScreen','rating-form'],['146:351','DanhGiaThanhCongScreen','rating-success']]);
add('commerce','commerce/HoTroScreens.kt',[
 ['44:279','TaoHoTroScreen / từ đơn','support-form'],['128:216','TaoHoTroScreen / chọn đơn','support-form'],['44:303','ChiTietHoTroScreen','support-resolved']]);
add('exchange','exchange/TraoDoiExplore.kt',[
 ['205:118152','TraoDoiExplore','explore'],['205:118039','bộ lọc trao đổi','search-filter'],['205:117992','chi tiết sản phẩm trao đổi','product-detail'],['205:117942','chọn món khả dụng','selector-empty']]);
add('exchange','exchange/TraoDoiListingScreens.kt',[
 ['226:102','tin trao đổi của tôi','my-listings'],['226:213','form tin trao đổi','post-form-top']]);
add('exchange','exchange/TraoDoiOfferScreens.kt',[
 ['205:117877','tạo đề nghị','create-offer'],['205:117719','đề nghị đã gửi','offer-sent'],['205:117605','đề nghị đã nhận','offers-received'],['205:117522','đang thương lượng','counter-awaiting-response'],['205:117344','đề xuất lại','counter-form'],['205:117393','chi tiết đề nghị nhận','agreement-accepted'],['205:117447','giao dịch hoàn tất','completed'],['205:117122','xác nhận giao nhận','handover-form'],['205:117061','chờ đối tác','waiting-partner'],['205:116984','đánh giá trao đổi','review-form'],['205:116916','đánh giá thành công','review-success']]);
add('exchange','commerce/ChatScreens.kt',[['205:117265','ChatScreen / ngữ cảnh1–1','exchange-chat']]);
add('smart-swap','smartswap/SmartSwapDiscovery.kt',[
 ['205:115234','SsFind','find-cycle'],['205:115155','SsMatches','matching-cycles']]);
add('smart-swap','smartswap/SmartSwapCircle.kt',[
 ['205:115087,236:18','chi tiết/preview vòng','cycle-preview'],['205:115021','đồng thuận phiên bản','three-consents'],['236:150','sheet tiền bù','compensation-sheet']]);
add('smart-swap','smartswap/SmartSwapChat.kt',[['205:114968','chat nhóm ba thành viên','group-chat']]);
add('smart-swap','smartswap/SmartSwapDeliveryScreens.kt',[
 ['205:114915','bàn giao và tiến độ','two-of-three-not-complete'],['205:114854','hoàn tất và hai đánh giá','two-member-reviews']]);
add('admin','admin/QuanTriDashboard.kt',[['205:116809','QtDashboard','dashboard'],['205:115300','QtReportScreen','statistics']]);
add('admin','admin/QuanTriApp.kt',[
 ['205:116652','drawer quản trị','admin-menu'],['205:116413','dialog khóa','lock-dialog'],['205:116138','dialog từ chối','reject-dialog']]);
add('admin','admin/QuanTriManagement.kt',[
 ['205:116578','QtUsers','users'],['205:116512','QtUserScreen / hoạt động','user-detail'],['205:116345','QtUserScreen / khóa','user-locked'],['205:116276','QtListings','listings'],['205:116220','QtListingScreen / kiểm duyệt','moderation-checklist'],['205:116077','QtListingScreen / đã duyệt','approved-listing'],['205:115492','QtCategories','categories'],['205:115419','QtCategoryFormScreen','category-form']]);
add('admin','admin/QuanTriTransactions.kt',[
 ['205:115976','QtTrades','trades'],['205:115767','QtTradeScreen / mua bán,1–1,3 bên','smart-swap-two-of-three'],['205:115705','QtComplaints','complaints'],['205:115645','QtComplaintScreen / QtEvidenceScreen','evidence-detail'],['205:115596','QtDecisionScreen / xác minh riêng','dispute-decision']]);
if(source.screens.length!==83||mappings.size!==83)throw Error(`Expected83 source frames and mappings: ${source.screens.length}/${mappings.size}`);
const rows=source.screens.map(s=>{
 const m=mappings.get(s.id);if(!m)throw Error('Missing mapping '+s.id);
 if(!fs.existsSync(path.resolve(__dirname,'..',m.file)))throw Error('Missing implementation '+m.file);
 return {id:s.id,name:s.name,width:s.width,height:s.height,...m,
   originalText:s.texts.filter(t=>t.text.trim()).map(t=>t.text),
   palette:s.colors.map(x=>x.v.hex),typography:s.typography,
   layout:s.children.map(c=>({name:c.name,size:c.size,y:c.position?.m12,scroll:c.scroll})),
   sourceRead:true,implementationExists:true,androidScreenshotExists:fs.existsSync(path.join(out,m.screenshot)),figmaRenderAvailable:false};
});
fs.writeFileSync(path.join(out,'figma-screen-audit.json'),JSON.stringify({source:'Nhom4.fig / decoded node metadata',frameCount:83,figmaMcp:'Quota exhausted for design-context and screenshot; no pixel-diff claim',frames:rows},null,2)+'\n');
const md=['# Đối chiếu83 frame Figma','',
 'Nguồn là snapshot `.fig` thực, không phải ảnh thiết kế tự dựng. Đã đọc tên, kích thước, văn bản, palette và typography của từng frame, ánh xạ đến code Compose hiện có. Ảnh Android dùng dữ liệu API thật nên khác fixture về tên, giá và số lượng.',
 '', 'Figma MCP hết quota cho cả design-context và screenshot. Chưa có render Figma toàn bộ để xác nhận pixel/icon/clipping tuyệt đối; cột ảnh chỉ chứng minh có ảnh Android, không tự động đồng nghĩa giao diện khớp. Xem nhận xét và giới hạn trong PROJECT_AUDIT.md.', '',
 '| Node | Frame | Kích thước | Code/state tương ứng | Ảnh Android |', '| --- | --- | --- | --- | --- |',
 ...rows.map(r=>`|${r.id}|${r.name.replaceAll('|','/')}|${r.width}×${r.height}|[${r.state}](${r.file.replace('Android/','../Android/')})|${r.androidScreenshotExists?'[Ảnh](qa/full-audit/'+r.screenshot+')':'Chưa chụp trạng thái này'}|`)];
fs.writeFileSync(path.resolve(__dirname,'../docs/FIGMA_SCREEN_AUDIT.md'),md.join('\n')+'\n');
console.log(`Read/mapped ${rows.length} source frames; ${rows.filter(r=>r.androidScreenshotExists).length} currently have corresponding Android screenshots.`);
