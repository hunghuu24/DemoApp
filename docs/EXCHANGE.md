# Trao đổi sản phẩm 1–1 — ReMarket

Cập nhật 11/11/2026. Mở rộng project hiện có, giữ tài khoản/JWT, catalog, mua bán và SQL Server. Android dùng Compose/MVVM/StateFlow, Retrofit, Coil và phiên DataStore hiện có. Không tạo project khác, không có danh sách sản phẩm hay tài khoản đăng nhập giả trong app.

## Phạm vi và demo

Đã triển khai đăng/quản lý tin trao đổi, khám phá/tìm kiếm/bộ lọc, chi tiết, chọn món đã đăng của mình, gửi/nhận đề nghị, từ chối/hủy/chấp nhận, chat riêng, đề xuất lại tiền bù/lịch hẹn, xác nhận giao nhận và hai bên đánh giá. Phiếu thương lượng mua bán vẫn là chức năng riêng.

Các cờ **cho phép1–1**, **cho phép Smart Swap3 bên**, **cho phép bù tiền** và **mức bù tối đa** được lưu thật vào SQL và dùng khi lọc/kiểm tra điều kiện. Backend đã có [tìm vòng](SMART_SWAP_SEARCH.md), [đồng thuận/thương lượng](SMART_SWAP_WORKFLOW.md) và [giao nhận/hoàn tất/sự cố](SMART_SWAP_DELIVERY.md) Smart Swap. Bộ lọc “Cho phép3 bên” trong Android vẫn chỉ lọc tin; kết nối UI Smart Swap là bước tiếp theo.

```powershell
cd Backend
. .\scripts\Use-DotNet8.ps1
.\scripts\Initialize-Database.ps1 -SeedDemo
dotnet run --project src/ReMarket.Api
```

Migration thứ7 `20261111155701_TraoDoiMotMot` cần được áp dụng trước khi chạy. Normal startup không tự migrate/seed. Android Emulator dùng `http://10.0.2.2:5080/`. Build APK từ `Android/` bằng wrapper hiện có.

Thử bằng hai tài khoản USER thật:

1. Mỗi bên vào tab **Trao đổi → Trao đổi của tôi → Đăng tin trao đổi**. Upload ảnh, nhập món muốn nhận, giá trị tham khảo, điều kiện và tỉnh/thành phố. Có lưu nháp; tin gửi đăng chờ ADMIN duyệt theo workflow catalog hiện có.
2. ADMIN duyệt hai tin. Người A tìm/mở tin người B, chọn **Đề nghị trao đổi**, chọn đúng món đã đăng của A. Điền tiền bù0 hoặc mức hợp lệ, người trả, lời nhắn/lịch hẹn và đồng ý quy định. Chỉ nút **Gửi đề nghị** mới tạo bản ghi đề nghị/giao dịch chờ phản hồi.
3. B vào Cá nhân → **Trao đổi của tôi → Đã nhận**; xem đúng món/lời nhắn, chat hoặc đề xuất lại. Ví dụ A đề nghị bù300.000đ, B đề xuất400.000đ: phiên bản1 được giữ trong lịch sử, A cần đồng ý phiên bản2.
4. Khi hai bên đồng ý cùng phiên bản, cả hai món được giữ. A/B gặp để kiểm tra và giao nhận. Tiền bù do hai người trả/nhận trực tiếp, không có ngân hàng hoặc giữ tiền qua app.
5. Mỗi người xác nhận đã kiểm tra, giao món của mình, nhận món đối tác và đủ tiền bù nếu có. Có thể chọn tối đa5 ảnh bằng chứng. Sau xác nhận của A, màn vẫn chờ B; chỉ đủ cả hai mới hoàn tất và đánh dấu hai tin đã trao đổi.
6. A và B đều có thể đánh giá đối tác một lần sau hoàn tất, chọn số sao, các tag và nhận xét. Lịch sử/ảnh/thỏa thuận vẫn đọc được khi tin ngừng công khai.

Có thể dùng các tài khoản demo đã seed theo [DU_LIEU_DEMO.md](database/DU_LIEU_DEMO.md), tạo **hai tin mới** cho kịch bản; Sony/Kindle trong fixture cũ đã được lịch sử giao dịch tham chiếu. Không reset mật khẩu hoặc sửa trạng thái fixture cũ để làm chúng khả dụng lại. Bộ kiểm thử Android tự đăng ký cặp “Minh Anh demo”/“Hoàng Nam demo” với email ngẫu nhiên và đăng nhập thật, không dùng cơ chế đặc biệt trong app.

## Thiết kế Figma

- Đã đọc ảnh/context form `226:213` qua Figma MCP trong phiên này. Các lượt đọc tiếp bị giới hạn Starter; các màn còn lại đối chiếu snapshot `Nhom4.fig` đã giải mã và [phân tích thiết kế](../FIGMA_ANALYSIS.md), không giả định nội dung chưa đọc.
- Các frame đối chiếu: Explore/filter `205:118152`/`205:118039`; tin của tôi `226:102`; chi tiết `205:117992`; chọn món `205:117942`; tạo đề nghị `205:117877`; gửi/nhận `205:117719`/`205:117605`; chi tiết/counter `205:117393`/`205:117344`; thương lượng/chat `205:117522`/`205:117265`; giao nhận/chờ `205:117122`/`205:117061`; hoàn tất/rating `205:117447`, `205:116984`, `205:116916`.
- Roboto được đóng gói trong app; dùng foundation sẵn có: xanh `#28734F`, nền `#FCFDFB`, chữ `#202E26`, phụ `#65746A`, nhạt `#EAF3EC`, viền form `#DCE4DE`. Nội dung padding ngang24dp, form/input48dp radius12dp; các section và form dài cuộn được. Viewport chính360×800; không cố định chiều cao theo các frame dài813–1872 của file.
- Sáu SVG MCP đã tải vào [assets/exchange](assets/exchange). `tools/Convert-FigmaExchangeSvg.cjs` chuyển5 asset back/camera/down/city-down/switch thành VectorDrawable giữ root22/28/18/18/40×24 và stroke; status bar dùng Android thật. Switch có state/semantics thật, ảnh món đồ luôn lấy API/Coil. Không dùng ảnh chụp Figma làm giao diện hoặc URL asset tạm lúc chạy.
- Tùy chọn1–1/3 bên bổ sung vào form theo yêu cầu người dùng, cùng kiểu switch của form. Tỉnh/thành phố nhập text, không hardcode danh sách địa giới có thể thay đổi. Lịch hẹn tùy chọn có validation ngày/giờ. Các trạng thái loading/empty/error, chờ duyệt và kiểm tra quyền là phần triển khai từ dữ liệu thật.
- Chat tái sử dụng module hiện có, mở hội thoại riêng cho từng đề nghị và hiển thị mã giao dịch. Đếm tiền bù, thời gian, xác minh và số món lấy từ DB; không sao chép giá/rating/số lượng/hạn phản hồi mẫu vào dữ liệu chạy.

## API

Tiền nguyên VND `long`; JSON camelCase, rowversion Base64; mọi route trao đổi yêu cầu JWT/session còn hiệu lực và USER. ADMIN duyệt qua route catalog riêng. Actor lấy từ JWT, không từ request. 400 đầu vào sai,401 mất phiên,403 sai người thao tác,404 ngoài quyền xem/không tồn tại,409 trạng thái/version/xung đột tồn kho.

| Method / route | Ý nghĩa |
| --- | --- |
| POST `/api/tindang` | Tạo nháp với body catalog hiện có, thêm object `traoDoi`; `giaBan` trong command dùng cho giá trị tham khảo, entity tin loại TraoDoi lưu GiaBan=NULL |
| PUT `/api/tindang/{id}` | Sửa nội dung/điều kiện khi chưa được giao dịch tham chiếu; version bắt buộc, tin hiển thị sửa phải duyệt lại |
| POST `/api/tindang/{id}/anh`, PUT ảnh bìa, DELETE ảnh | Dùng kho ảnh thật, tối đa6 ảnh cho tin trao đổi, mỗi ảnh5MB |
| POST `/api/tindang/{id}/gui-duyet`, `/an`; DELETE tin | Gửi duyệt/ẩn/xóa mềm; không được sửa/ẩn/xóa khi đang giữ chỗ |
| GET `/api/tindang?loaiTin=TraoDoi&...` | Tìm kiếm theo tên/mô tả/món muốn nhận, danh mục, tình trạng, tỉnh, khoảng giá trị tham khảo, cờ bù/1–1/3 bên, sắp xếp và phân trang |
| GET `/api/tindang/{id}` | Chi tiết công khai gồm `traoDoi`; không tạo đề nghị/GiaoDich |
| GET `/api/tindang/cuatoi?loaiTin=TraoDoi&trangThai=...&trang=1` | Tin của tôi, gồm nháp/chờ duyệt/hiển thị/ẩn/giữ/đã trao đổi |
| GET `/api/traodoi/moncuatoi?tuKhoa=...&trang=1` | Chọn món của actor đã đăng, còn khả dụng, cho phép1–1; SQL lọc trước khi phân trang10 |
| POST `/api/traodoi` | Gửi đề nghị201, tạo GiaoDich.ChoDongY/2participant/phiên bản1; chưa giữ món |
| GET `/api/traodoi?tab=gui|nhan|thuongluong|hoantat|tatca&trang=1` | Danh sách đúng participant,10/trang |
| GET `/api/traodoi/{id}` | Hai món, điều kiện hiện tại/lịch sử, consent, giao nhận, bằng chứng, rating và cờ thao tác |
| POST `/api/traodoi/{id}/phanhoi` | `{phienBan,dongY,lyDo?}`; đối tác của người tạo phiên bản hiện tại chấp nhận/từ chối |
| POST `/api/traodoi/{id}/dexuatlai` | `{phienBan,soTienBu,nguoiTraId?,loiNhan,diaDiemHen?,thoiGianHen?}`; tạo phiên bản tiếp theo |
| POST `/api/traodoi/{id}/huy` | `{phienBan,lyDo}` trước giao nhận; giải phóng cả hai giữ chỗ nếu có |
| POST `/api/traodoi/{id}/hoithoai` | Mở/tái sử dụng chat riêng, dùng chung các API `/api/hoithoai/{chatId}/...` để gửi text/ảnh và đọc tin |
| GET `/api/traodoi/{id}/anh/{sanPhamId}` | Ảnh riêng của đúng một trong hai món, chỉ participant |
| POST `/api/traodoi/{id}/giaonhan` | `{phienBan,daKiemTra,daGiaoMonCuaToi,daNhanDu,daXacNhanTienBu}`; tất cả phải true |
| POST `/api/traodoi/{id}/bangchung` | Multipart `anh`,`maYeuCau`; gắn vào xác nhận nhận của chính actor, tối đa5, khóa gửi lại |
| GET `/api/traodoi/{id}/bangchung/{fileId}` | Chỉ participant xem bằng chứng của giao dịch |
| GET `/api/traodoi/tieuchi` | Tag rating từ SQL |
| POST `/api/traodoi/{id}/danhgia` | `{maYeuCau,soSao,noiDung,tieuChiIds}` sau hoàn tất; một rating/người/đối tác |

Object `traoDoi` khi tạo/sửa tin:

```json
{
  "moTaMongMuon": "Muốn nhận máy đọc sách có thể kiểm tra trực tiếp.",
  "choPhepMotMot": true,
  "choPhepSmartSwap": true,
  "choPhepBuTien": true,
  "soTienBuToiDa": 600000,
  "ghiChu": "Ưu tiên gặp trực tiếp, có đủ phụ kiện."
}
```

Gửi đề nghị dùng `maYeuCau` GUID, `tinGuiId`, `tinNhanId`, `phienBanTinGui`, `phienBanTinNhan`, `soTienBu`, `nguoiTraId?`, `loiNhan`, `diaDiemHen?`, `thoiGianHen?` ISO DateTimeOffset và `dongYQuyDinh:true`. Chỉ dùng ID tin đã có, không truyền món tự gõ hoặc tự đặt owner/giá/participant.

## Nghiệp vụ, khóa và lịch sử

- GET khám phá/chi tiết/chọn món không tạo bản ghi GiaoDich. Tin còn khả dụng phải đang hiển thị, chưa xóa, loại TraoDoi/CaHai, chủ tài khoản hoạt động, danh mục hiển thị, bật1–1 và không giữ cho giao dịch khác. Kiểm tra lại tại thời điểm gửi/chấp nhận, không tin trạng thái client.
- Gửi đề nghị: GiaoDich=`ChoDongY`, DeNghi=`ChoPhanHoi`; phiên bản1=`DeXuat`; creator đã đồng ý, đối tác=`ChoPhanHoi`. Giao dịch chưa bắt đầu, không khóa hai món chỉ vì xem hoặc mới gửi đề nghị.
- Người nhận phiên bản hiện tại đồng ý → đủ2/2 consent → `DaChot` → giữ cả hai món → GiaoDich=`DangThucHien`, đề nghị=`DaChapNhan`, tin=`DangGiuCho`. Unique hold/món và khóa sản phẩm bảo vệ hai giao dịch tranh cùng một món.
- Khóa tất cả sản phẩm theo thứ tự GUID ổn định trước tất cả tin, rồi khóa đề nghị. Mua bán/catalog dùng cùng quy tắc sản phẩm trước tin; catalog dùng ReadCommitted với UPDLOCK/rowversion thay vì giữ shared mapping locks của Serializable. Vẫn bảo vệ ảnh bìa, số ảnh, version và lịch sử bằng transaction/unique/FK/trigger.
- Đề xuất lại đóng phiên bản cũ thành `DaThayThe`, tạo phiên bản mới và consent mới. Khoản bù/lời nhắn/lịch hẹn cũ giữ nguyên trong lịch sử. Kể cả đã đồng ý, trước giao nhận vẫn có thể đề xuất lại; khi đó GiaoDich về `ChoDongY`, giữ hai món và khóa bàn giao đến khi cùng đồng ý phiên bản mới. Sau bất kỳ xác nhận giao nhận nào không được counter/hủy.
- Tiền bù>=0; nếu>0 thì cả hai tin phải cho phép bù, người trả phải là một participant và không vượt mức tối đa mà **tin của người trả** cho phép. Người nhận có thể đặt mức tự bù0 nhưng vẫn đồng ý nhận khoản bù. Không tính chênh lệch giá thành khoản nợ tự động; số tiền phải được đối tác chấp nhận.
- Khi actor xác nhận giao nhận, lưu2 dòng XacNhanGiaoNhan: đã giao món của mình và đã nhận món của đối tác, gắn đúng phiên bản DaChot. Chỉ cập nhật cờ trả/nhận tiền thuộc vai trò của actor. Có một người nhận chưa đủ để hoàn tất.
- Đủ hai người nhận và hai cờ tiền bù (nếu có) → GiaoDich=`HoanTat`, lưu ngày hoàn tất, hai tin=`DaTraoDoi`, giải phóng hold. Giữ chủ sở hữu bản ghi nguồn để bảo toàn FK lịch sử; đăng lại món nhận được cần tạo sản phẩm/tin nguồn mới.
- Đề nghị chờ có hạn24 giờ; worker30 giây và đọc đề nghị/danh sách xử lý hết hạn, hủy giao dịch chờ và nhả giữ chỗ khi đang counter. Không tự hủy giao dịch đã đồng ý đang chờ gặp, đang giao nhận, hoàn tất hoặc đang tranh chấp. Thời điểm48 giờ trên hold của giao dịch đã đồng ý là metadata, không tự cho phép bán món khi quá giờ.
- Tạo đề nghị dùng unique người gửi/key và hash payload. Gửi lại cùng nội dung không nhân GiaoDich; đổi payload cùng key409. Counter/accept/cancel kiểm tra rowversion. Xác nhận đã nhận gửi lại không nhân receipt. Evidence có key/hash và unique trên receipt/key. Rating có key và unique participant/đối tác.
- Chat và ảnh riêng đều kiểm tra membership/participant; không dùng role ADMIN để bỏ qua quyền sở hữu trao đổi. File gốc không dùng tên/path client, ảnh decode/re-encode JPEG, route riêng `private,no-store`. Đổi user hủy request và xóa state Android.
- Bằng chứng upload sau khi receipt được lưu; Android giữ hàng đợi ảnh và key khi lỗi, cho tiếp tục tải, không làm giả trạng thái giao dịch. Có thể gắn ảnh sau khi cả hai đã hoàn tất, vẫn giới hạn/quyền như cũ.
- Sau hoàn tất mỗi bên đánh giá một lần1–5 sao, các tiêu chí DB và nhận xét. “Báo vấn đề giao nhận” mở chức năng hỗ trợ hiện có với mã trao đổi trong tiêu đề; xử lý tranh chấp/biện pháp quản trị đầy đủ là nhóm chức năng riêng.

## CSDL và mã nguồn

Vẫn47 bảng; migration7 thêm `NhuCauTraoDoi.ChoPhepMotMot`/`GhiChu`, hai FK tin gửi–sản phẩm và tin nhận–sản phẩm trên DeNghiTraoDoi, key/hash tạo đề nghị, lý do kết thúc và key/hash evidence. Dữ liệu cũ được giữ, FK mới nullable cho legacy. Thêm trạng thái tin `DaTraoDoi`; bổ sung hai tag tham chiếu nếu chưa tồn tại, không ghi đè tag cũ. [Schema/từ điển](database/THIET_KE_CSDL.md).

Backend: `Application/TraoDoi`, `Infrastructure/Exchange/TraoDoiService`, `TraoDoiActions`, `TraoDoiRead`, `TraoDoiEvidence`, controller/worker mới; tái sử dụng catalog/kho ảnh/chat/khóa tồn kho. Android: `feature/exchange`, `TraoDoiRepository` và `TraoDoiApi`, nối vào MainActivity/TaiKhoanApp, tab Trao đổi và Cá nhân. [Kết quả kiểm thử](EXCHANGE_VALIDATION.md).
