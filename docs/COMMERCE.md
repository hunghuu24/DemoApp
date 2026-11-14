# ReMarket — Mua bán, thương lượng và hỗ trợ

Cập nhật 10/11/2026. Mở rộng project hiện có; tiếp tục dùng JWT/session, USER/ADMIN, SQL Server, catalog và kho ảnh đã có. Giá, sản phẩm, hội thoại, đơn hàng và trạng thái đều từ REST API. Không có tài khoản đăng nhập hoặc dữ liệu sản phẩm thay thế hệ thống thật trong app.

## Chạy và demo

```powershell
cd Backend
. .\scripts\Use-DotNet8.ps1
.\scripts\Initialize-Database.ps1 -SeedDemo
dotnet run --project src/ReMarket.Api
```

Migration 5–6 thêm bảng thương lượng/hỗ trợ và các cột theo dõi mô phỏng; không xóa dữ liệu cũ. Normal startup không tự migrate/seed. Giữ cấu hình JWT/SQL đã tạo ở giai đoạn tài khoản. Swagger: `http://localhost:5080/swagger`; Android Emulator: `http://10.0.2.2:5080/`.

1. Đăng ký hai tài khoản người mua/người bán bằng chức năng tài khoản hiện có.
2. Người bán đăng món đồ, upload ảnh, gửi duyệt; ADMIN duyệt qua màn quản trị/API hiện có. Đăng nhập bằng hai emulator hoặc đổi tài khoản trên cùng app.
3. Người mua mở chi tiết → **Liên hệ** → gửi tin/ảnh → **Trả giá**. Người bán vào Tin nhắn, chấp nhận hoặc từ chối phiếu.
4. Người mua chọn **Mua hàng** trong phiếu đã chấp nhận hoặc **Mua ngay** ở sản phẩm. Điền người nhận/điện thoại/địa chỉ; chọn phiếu hoặc không áp dụng; tạo đơn.
5. Trong chi tiết đơn, chọn **Thanh toán mô phỏng** và kết quả thành công/thất bại. Thất bại cho phép thử lại trong hạn giữ chỗ.
6. Người bán vào Cá nhân → Đơn hàng của tôi → **Đơn bán**. Thực hiện từng bước bàn giao, đang vận chuyển, đã giao; có nhánh thất bại và giao lại.
7. Người mua tải lại/theo dõi tự cập nhật, xác nhận nhận hàng sau khi đã giao, đánh giá riêng sản phẩm/người bán. Gửi hỗ trợ từ đơn hoặc Cá nhân → Yêu cầu hỗ trợ.
8. ADMIN vào **Quản lý yêu cầu hỗ trợ**, tiếp nhận/giải quyết và nhập phản hồi. Người gửi xem lịch sử và bổ sung thông tin trước khi đóng.

Để cấp ADMIN trong Development cho một tài khoản đã đăng ký, dùng lệnh operator `dotnet run --project src/ReMarket.Api -- --grant-admin EMAIL`. App không tự nâng quyền. Admin được cấp bằng lệnh này vẫn giữ USER; tài khoản chỉ có ADMIN không được gọi các API USER.

## Thiết kế đã đọc

- Figma MCP đọc được context và ảnh màn xác nhận mua `194:72` ngày 10/11/2026. Các lượt đọc tiếp bị giới hạn Starter; các màn còn lại đối chiếu cấu trúc/text/vector đã giải mã từ `Nhom4.fig`, không giả định nội dung online chưa đọc.
- Nguồn đối chiếu: chat `128:139`, `188:2`, `85:2`; phiếu `128:174`; checkout giảm giá `44:191`; danh sách đơn `44:223`; chi tiết `44:248`, `77:65`; đánh giá `146:248`, `146:351`; hỗ trợ `44:279`, `44:303`, `90:78`, `128:216`; hộp thư `90:16`. Xem thêm [phân tích gốc](../FIGMA_ANALYSIS.md).
- Compose dùng Roboto đã đóng gói, nền `#F7F9F7`, xanh `#16845B`, chữ `#17211C`/`#17352B`, phụ `#66736B`, viền `#E1E8E3`, nền nhạt `#E8F5EE`. Padding ngang 24dp tại viewport 360×800, toolbar 56dp, card radius14dp, CTA tối thiểu48dp radius12dp. Form dài và lịch sử cuộn; composer tránh bàn phím.
- Hai SVG back/chevron lấy trực tiếp từ MCP ở [assets/commerce](assets/commerce); paperclip/send được trích nguyên vector blob của file `.fig` bằng `tools/Export-FigmaCatalogIcons.cjs ... commerce`. Không dùng URL asset tạm lúc chạy.
- Phiếu trong thiết kế là **đề nghị giảm giá thương lượng**, không phải hệ thống phát hành mã khuyến mãi marketing. Phiếu cần người bán đồng ý; đúng người mua/món đồ/giá niêm yết và còn hạn mới dùng được.
- Thanh toán, giữ tiền, hoàn/giải ngân và vận chuyển luôn ghi rõ **mô phỏng**, tránh diễn đạt như có ngân hàng thật. Timeline hiển thị sự kiện DB, không tự tô hoàn tất theo hình Figma. Nút thao tác nghiệp vụ lấy từ quyền/trạng thái API.
- Các bước cần bổ sung để kiểm tra nghiệp vụ (kết quả thanh toán mô phỏng, thao tác vận chuyển của người bán, hàng chờ hỗ trợ Admin, validation/loading/error) là quyết định triển khai, không khẳng định có toàn bộ trong Prototype.

## REST API

Tất cả endpoint sau yêu cầu JWT còn hiệu lực và role USER; `/api/admin/hotro` yêu cầu ADMIN. JSON camelCase, tiền nguyên VND `long`; version SQL rowversion Base64. Lỗi dùng ProblemDetails; 400 sai đầu vào, 401 mất phiên, 403 sai vai trò/thao tác, 404 không tồn tại hoặc ngoài quyền xem, 409 xung đột/trạng thái/version. Giới hạn rate mặc định nhóm mua bán 60 yêu cầu/phút, không thay đổi giới hạn đăng nhập/khôi phục.

| Method / URL | Nghiệp vụ |
| --- | --- |
| POST `/api/hoithoai` | `{tinDangId}` mở/tái sử dụng chat người mua–tin đăng |
| GET `/api/hoithoai?chuaDoc=false&trang=1` | Hộp thư thành viên, 20 hội thoại/trang, số tin chưa đọc |
| GET `/api/hoithoai/{id}?truoc=SO_THU_TU` | 50 tin gần nhất hoặc tin cũ; cập nhật mốc đã đọc khi mở trang mới nhất |
| POST `/api/hoithoai/{id}/tinnhan` | `{maPhiaClient,noiDung}` gửi văn bản |
| POST `/api/hoithoai/{id}/anh` | Multipart `maPhiaClient`, `anh`, tối đa5MB; ảnh giải mã và encode JPEG |
| GET `/api/hoithoai/{id}/anh/{fileId}` | Ảnh chat riêng tư theo thành viên |
| GET `/api/hoithoai/{id}/sanpham/anh` | Ảnh sản phẩm cho hội thoại, kể cả tin ngừng công khai |
| POST `/api/hoithoai/{id}/phieu` | Người mua gửi `{maPhiaClient,soTienGiam}` |
| POST `/api/hoithoai/{id}/phieu/{couponId}` | `{phienBan,chapNhan}`; người bán chấp nhận/từ chối, người mua hủy phiếu của mình |
| POST `/api/donhang/baogia` | `{tinDangId,phieuThuongLuongId?}` tính giá + phí + phiếu khả dụng |
| POST `/api/donhang` | Tạo đơn201: `{maYeuCau,tinDangId,phienBanTin,phieuThuongLuongId?,tenNguoiNhan,soDienThoai,diaChi,phuongThuc:"MoPhong"}` |
| GET `/api/donhang?vaiTro=mua&trangThai=DaNhan&trang=1` | Đơn mua/bán, lọc trạng thái, 10 đơn/trang |
| GET `/api/donhang/{id}` | Snapshot giá, trạng thái, người nhận, thanh toán, vận chuyển, lịch sử và cờ thao tác |
| GET `/api/donhang/{id}/anh` | Ảnh riêng tư của đơn theo người mua/người bán |
| POST `/api/donhang/{id}/hoithoai` | Mở chat đúng người mua/người bán của đơn, kể cả sau giữ chỗ/bán |
| POST `/api/donhang/{id}/thanhtoan` | Người mua gửi `{maYeuCau,phienBan,thanhCong}`; lưu kết quả mô phỏng |
| POST `/api/donhang/{id}/huy` | `{phienBan,lyDo}` trước bàn giao, giải phóng món, hoàn mô phỏng nếu đã trả |
| POST `/api/donhang/{id}/vanchuyen` | Người bán gửi `{phienBan,trangThai}` theo transition cho phép |
| POST `/api/donhang/{id}/nhanhang` | Người mua gửi `{phienBan}` khi vận chuyển đã giao |
| POST `/api/donhang/{id}/danhgia` | `{maYeuCau,soSaoNguoiBan,soSaoSanPham,noiDung}` sau hoàn tất |
| POST `/api/hotro` | Tạo201: `{maYeuCau,donHangId?,loai,tieuDe,noiDung}` |
| GET `/api/hotro?trang=1`, GET `/api/hotro/{id}` | Người gửi xem yêu cầu của mình, 10/trang |
| POST `/api/hotro/{id}/phanhoi` | `{phienBan,noiDung,trangThai:null}` bổ sung thông tin trước đóng |
| GET `/api/admin/hotro?trang=1`, GET `/api/admin/hotro/{id}` | ADMIN xem yêu cầu để xử lý |
| POST `/api/admin/hotro/{id}/phanhoi` | `{phienBan,noiDung,trangThai:"DangXuLy"|"DaGiaiQuyet"}` |

## Quy tắc và lưu dữ liệu

- Mỗi món đồ cũ là một đơn vị; đơn hiện tại mua một món, số lượng1. Không tự sinh số lượng tồn kho nhiều chiếc. `GiuChoSanPham` unique filtered bảo vệ một giữ chỗ đang hoạt động trên mỗi sản phẩm; `DangGiuCho` rút tin khỏi catalog.
- Tạo đơn khóa sản phẩm trước, tiếp theo tin (`UPDLOCK/ROWLOCK`) trong transaction; catalog cũng dùng cùng thứ tự khóa. Kiểm tra giá/loại tin, người đăng khác người mua, tài khoản và danh mục đang hoạt động, version và giữ chỗ trước khi lưu. Snapshot giá gốc/giảm/tổng nằm trong đơn và dòng đơn, không tin số tiền client gửi.
- Giữ chỗ đơn chưa trả hết hạn sau15 phút. Background worker mỗi30 giây giải phóng; đọc đơn/danh sách/báo giá cũng xử lý hết hạn liên quan. Worker chỉ hủy đơn chưa thanh toán, không tự giao hàng hoặc hoàn tất đơn đã trả. Có thể cấu hình `Commerce:EnableExpirationWorker=false` cho test host riêng.
- Phí mô phỏng `Commerce:ShippingFee`, mặc định30000 VND; lưu vào đơn lúc tạo. Sau khi trả thành công khoản mô phỏng được giữ đến khi nhận; lúc đó lưu giải ngân, xác nhận nhận và đánh dấu món `DaBan` trước khi đóng giao dịch để thỏa trigger SQL.
- Đơn: `ChoThanhToan → DaThanhToan → DangGiao → DaNhan`; có nhánh `DaHuy` trước bàn giao. Thất bại thanh toán lưu `ThanhToan.ThatBai`, đơn vẫn `ChoThanhToan`; hủy đơn đã trả trước bàn giao lưu `HoanTien` và ngày hoàn. Không gọi ngân hàng, ví, đơn vị giao vận hoặc tự tạo giao dịch tiền thật.
- Ship: `ChoLayHang → DaLayHang → DangVanChuyen → DaGiao`. Có `GiaoThatBai → DangVanChuyen` để giao lại; không cho bỏ bước. Người mua không được giả thao tác người bán; người bán không được nhận hàng thay người mua. `LichSuVanChuyen.SoThuTu` identity giữ đúng thứ tự khi nhiều sự kiện cùng giây.
- Phiếu24 giờ; `0 < soTienGiam < giaNiemYet`. Một phiếu được chấp nhận mỗi hội thoại; chấp nhận phiếu mới đóng phiếu cũ. Phiếu gắn đơn chuyển `DaSuDung` và unique FK bảo vệ không dùng lại, kể cả đơn sau đó hủy. Có thể thương lượng phiếu mới khi món được mở bán lại. Giá thay đổi làm phiếu cũ không hợp lệ.
- `maYeuCau`/`maPhiaClient` do client sinh GUID. Tạo đơn dùng unique người mua–key và hash nội dung để tránh trùng do gửi lại; nội dung khác cùng key trả409. Tin nhắn, đề nghị, đánh giá và hỗ trợ có quy tắc gửi lại. Thanh toán lưu key/kết quả lần cuối; client giữ key khi lỗi mạng. Rowversion ngăn ghi đè dữ liệu vừa đổi; app có tải lại.
- Hội thoại và ảnh chỉ thành viên đọc; gửi tin yêu cầu membership. Số thứ tự tin và watermark thành viên bảo vệ số chưa đọc khi gửi trong cùng giây. Android tự cập nhật mỗi3 giây khi màn ở foreground; có tải lại và tin cũ/phân trang. Chưa dùng WebSocket/FCM hoặc thông báo hệ điều hành nền.
- Chỉ hai bên đơn xem địa chỉ/ảnh/lịch sử của đơn; ADMIN không mặc nhiên được đọc đơn qua route USER. Hỗ trợ thuộc **người gửi**, người bán cùng đơn không xem ticket riêng của người mua. ADMIN xử lý qua route riêng; yêu cầu đóng không sửa tiếp. `LichSuHoTro` lưu actor, nội dung, thời điểm và thứ tự.
- Ticket cũ vẫn hiển thị nội dung `PhanHoiHoTro` trước migration, gắn nhãn “Phản hồi”; không tự suy ra thời điểm tiếp nhận/giải quyết trước đây khi DB không có lịch sử. Phản hồi mới và event history dùng cùng thời điểm, không hiển thị trùng.
- Đánh giá một lần bởi người mua sau giao dịch hoàn tất; sản phẩm và người bán có số sao riêng1–5. Giữ `DanhGia.SoSao` cũ cho điểm người bán; cột mới `SoSaoSanPham` nullable giữ tương thích dữ liệu cũ.
- FK NO ACTION, rowversion, CHECK và filtered unique bổ sung qua EF migrations; ảnh ghi tên GUID sau kiểm tra nội dung, phục vụ qua route có quyền. Không xóa lịch sử theo việc ẩn/xóa mềm tin. Dữ liệu seed cũ giữ nguyên, dữ liệu demo thương lượng/đơn mới được tạo qua API/UI bằng tài khoản thật.

## Tổ chức mã

Backend: `Application/MuaBan/HopDongMuaBan.cs`; `Infrastructure/Commerce/MuaBanService.cs`, `ThuongLuongService.cs`, `MuaBanDocService.cs`, `KhoaTonKho.cs`; `Api/Controllers/MuaBanController.cs`; `Api/Authentication/HetHanDonHangWorker.cs`. Dùng DbContext/SQL transaction của project hiện tại.

Android: `feature/commerce/MuaBanViewModel` StateFlow, màn chat/checkout/đơn/đánh giá/hỗ trợ, `MuaBanComponents`, Retrofit `MuaBanApi`, `MuaBanRepository`/implementation trong domain/data. Dùng chung authenticated client, refresh, ImageLoader và DataStore. Đổi tài khoản xóa state giao dịch và hủy request cũ. Bottom Navigation, Cá nhân và Admin nối vào cùng điều hướng; form giữ nội dung khi lỗi, có loading/empty/error/thành công.

Kiểm tra tự động và ảnh thiết bị: [COMMERCE_VALIDATION.md](COMMERCE_VALIDATION.md).
