# Rà soát ReMarket — 13/11/2026

Đợt này tiếp tục source hiện có, không tạo lại project. Backend và Android Debug build thành công; các bộ HTTP/SQL và 20 ca Android đạt. **Chưa nghiệm thu toàn bộ ứng dụng**: còn các mục cài đặt chưa triển khai và các giới hạn kiểm chứng giao diện/thiết bị nêu bên dưới. Kết quả Android dựa trên `OK (20 tests)`, không lấy exit code ADB làm bằng chứng đạt.

## Build và kiểm thử đã chạy

| Hạng mục | Kết quả | Bằng chứng |
| --- | --- | --- |
| Backend solution, Release, .NET 8 | 0 lỗi, 0 cảnh báo | [Log build](qa/full-audit/backend-build.txt) |
| Android Debug + APK instrumentation + lint + JVM unit test | Build thành công; lint 0 lỗi, 22 cảnh báo | [Log build](qa/full-audit/android-build.txt), [lint](../Android/app/build/reports/lint-results-debug.html) |
| HTTP/SQL, SQL Server thật | 8/8 bộ đạt, 1.267 assertions | [Log toàn bộ](qa/full-audit/backend-all.txt) |
| Database mới, SQL deployment script áp hai lần, seed và ràng buộc | 20 assertions đạt, tự dọn DB test riêng | [Log database mới](qa/full-audit/fresh-database.txt) |
| Độ phủ REST API theo method + route template | 121/121 nhận request và có ít nhất một phản hồi 2xx; không quan sát 5xx trong lượt chạy này | [Ma trận](API_TEST_MATRIX.md), [JSON](qa/full-audit/api-coverage.json) |
| Android Emulator, API thật | **20/20 ca đạt**, chạy 351 giây, API 36 | [Log UI](qa/full-audit/android-e2e.txt) |
| Tài khoản demo | Đăng nhập thật 10/10 email trong README; logout từng phiên kiểm tra | [Kết quả không chứa mật khẩu/token](qa/full-audit/demo-logins.json) |

Tổng Backend/database: **1.287 assertions**, không phải 1.287 test case độc lập. Runner dùng TestServer với JWT, service và SQL Server thật, không dùng EF InMemory/SQLite/mock repository. Test Android gọi host HTTP thật. Độ phủ route không chứng minh mọi nhánh, race condition, kích thước dữ liệu hay đầu vào đã được kiểm tra. JVM unit test hiện có chỉ là smoke test mẫu; phần nghiệp vụ và đồ thị nằm trong runner Backend.

| Bộ Backend | Assertions đạt | Phạm vi chính |
| --- | ---: | --- |
| Tài khoản | 57 | Đăng ký, trùng liên hệ, hash mật khẩu, đăng nhập, access/refresh token, thu hồi phiên, hồ sơ, reset mật khẩu qua outbox Development, USER/ADMIN |
| Sản phẩm/tin đăng | 94 | Upload, CRUD, tìm/lọc/phân trang, preview, trạng thái tin, chủ sở hữu, validation, rowversion |
| Mua bán | 126 | Chat, thương lượng/phiếu, báo giá, tạo/hủy đơn, thanh toán/vận chuyển mô phỏng, nhận hàng, đánh giá, hỗ trợ, quyền sở hữu và giữ sản phẩm |
| Trao đổi 1–1 | 127 | Hai tài khoản, sản phẩm khả dụng, gửi/nhận, chấp nhận/từ chối, đề xuất lại, chat, hai xác nhận giao nhận, hoàn tất/đánh giá |
| Tìm Smart Swap | 289 | Chu trình đúng ba người/sản phẩm, hướng cạnh, nhu cầu, danh mục, tình trạng, giá, tiền bù, sắp xếp, không có kết quả, giữ hàng; 100 đồ thị sinh đối chiếu oracle độc lập |
| Đồng thuận Smart Swap | 171 | Mời hai người, ba xác nhận, từ chối/hết hạn, bản tiền bù mới, quyền thay đổi khoản, tổng trả/nhận, cạnh tranh giữ sản phẩm |
| Giao nhận Smart Swap | 256 | Ba JWT riêng, chat/thông báo, thông tin giao nhận, đúng ba vận đơn, mã và trạng thái, receipt đúng người, 0/3–3/3, sự cố, chứng cứ, đánh giá |
| Admin | 147 | Toàn bộ route quản trị, 401/403, thu hồi role tức thời, khóa/mở khóa, kiểm duyệt, danh mục, ba loại giao dịch, chứng cứ riêng tư, tranh chấp/xác minh, thống kê đối chiếu SQL |

| Bộ Android instrumentation | Ca đạt | Phạm vi |
| --- | ---: | --- |
| `TaiKhoanEndToEndTest` | 3 | Đăng ký/đăng nhập thật, phân luồng role, phiên và đăng xuất, hồ sơ |
| `SanPhamEndToEndTest` | 2 | Đăng/sửa/ẩn/bán/xóa tin, preview, upload, tìm/lọc/phân trang và empty |
| `MuaBanEndToEndTest` | 4 | Chat/phiếu, checkout/đơn/thanh toán/vận chuyển mô phỏng, nhận hàng/đánh giá, hủy/hoàn tiền mô phỏng, hỗ trợ User/Admin |
| `TraoDoiEndToEndTest` | 3 | Hai người thương lượng/nhận hàng/đánh giá, lọc món khả dụng, từ chối/hủy, form đăng trao đổi |
| `SmartSwapEndToEndTest` | 3 | Ba tài khoản, tìm vòng/tiền bù/đồng thuận lại/chat/ba vận đơn/3 receipts/hai đánh giá; quay lại/bộ lọc/hủy; từ chối/thông báo/sự cố |
| `QuanTriEndToEndTest` | 2 | Người dùng/kiểm duyệt/danh mục/xung đột/thống kê; xem 2/3, chứng cứ, giữ tạm rồi xử lý tiếp, chỉ User thứ ba được hoàn tất |
| `KiemTraGiaoDienTest` | 3 | Các form tài khoản, 404/empty/validation/Back, form cuộn 320×640 và font 1,3 |

Đã chụp **118 ảnh** trong lượt 20/20 cuối cùng, tách khỏi ảnh lượt đầu. [Manifest SHA-256](qa/full-audit/screenshot-manifest.json) và gallery: [tài khoản](qa/full-audit/gallery-account.png), [catalog](qa/full-audit/gallery-catalog.png), [mua bán](qa/full-audit/gallery-commerce.png), [1–1](qa/full-audit/gallery-exchange.png), [Smart Swap](qa/full-audit/gallery-smart-swap.png), [Admin](qa/full-audit/gallery-admin.png). Ảnh đơn giữ kích thước gốc; gallery chỉ thu nhỏ để duyệt. Một số ảnh sản phẩm test là màu đặc được upload qua API, không phải sản phẩm hardcode trong ứng dụng.

## Lỗi và hành vi đã sửa

- Báo cáo Admin với ngày sát biên `DateTimeOffset` từng có thể gây overflow/500. API kiểm tra miền ngày và trả 400; đã thêm kiểm thử cả hai biên và cửa sổ so sánh.
- Phương án tranh chấp `GhiNhan` (giữ tạm dừng) của trao đổi từng có thể đóng khiếu nại nhưng để vòng treo vĩnh viễn. Sau xác minh phương án tạm thời, khiếu nại trở về `DangXuLy`; Admin có thể ra quyết định cuối. Quyết định cuối đã xác minh thay thế tác dụng giữ tạm của cùng khiếu nại; các khiếu nại khác vẫn được xét. Không sinh receipt hay xác nhận tiền thay người dùng. Backend đã kiểm tra giữ tạm → tiếp tục xử lý → quyết định tiếp tục → vòng còn 2/3.
- Form danh mục Android tải lại sau xung đột từng giữ dữ liệu/rowversion cũ. Nay tải lại cả form từ API; thông báo và nút xác minh tranh chấp cũng thể hiện đúng việc tiếp tục xử lý.
- Worker mở khóa có hạn thêm thứ tự ổn định trước `Take`, tránh chọn tùy ý khi xử lý theo lô.
- Bổ sung runner `--all`, đo route/status từ request thực và xuất ma trận API. Chỉ lưu template, method, status/count; không lưu token, header, body hay ID người dùng vào báo cáo độ phủ.
- Lượt UI đầu ghi `20 tests, 3 failures`: hai ca thất bại, cộng một lỗi cleanup. Nguyên nhân là nhập/bấm khi tab Admin còn tải, bước chờ hỗ trợ chưa xét Admin ViewModel và cleanup kỳ vọng 200 thay vì 201. Đã sửa test chờ đúng trạng thái và mã HTTP; không đổi logic ứng dụng để bỏ qua kiểm thử. [Log ban đầu](qa/full-audit/android-e2e-initial.txt) được giữ lại. Fixture bị bỏ dở đã hủy bằng quy trình tranh chấp Admin, không giả lập receipt.

## SQL Server, migrations và dữ liệu

Kiểm tra trên SQL Server 2022, `localhost`, Windows Authentication, database `ReMarketDemo`. Schema hiện có 48 bảng nghiệp vụ, 100 khóa ngoại, 70 CHECK constraints, 22 triggers và 11 migrations ([trạng thái SQL cuối](qa/full-audit/sql-final.json)). Model khớp snapshot trong kiểm thử database và lệnh EF CLI [has-pending-model-changes](qa/full-audit/model-snapshot.txt); fresh DB được tạo từ script idempotent, seed và chạy kiểm tra tính lặp/ràng buộc. Các bộ HTTP tự chạy migrations trên database tên `ReMarketTest_<guid>` riêng. Đợt này không đổi schema, không thêm migration rỗng.

Giữ dữ liệu demo cũ và tiến độ vòng giao nhận mẫu ở **2/3**, không sử dụng receipt thứ ba của demo để làm test. Tài khoản, sản phẩm, giao dịch UI được tạo qua API riêng cho từng ca. Các bản ghi lịch sử/nhật ký kiểm thử có thể còn trong database Development; không xóa dây chuyền dữ liệu người dùng. Seed lại không reset mật khẩu, trạng thái giao dịch hoặc sản phẩm đang giữ.

Kiểm tra media sau lượt UI cuối: **259 tham chiếu hợp lệ, 0 tệp thiếu/đường dẫn không hợp lệ** ([JSON](qa/full-audit/media-integrity-final.json)); đầu đợt là 208 trước khi các ca UI upload thêm. Đây là số tham chiếu tại thời điểm quét, không phải số sản phẩm. Ảnh upload mới được kiểm tra trong HTTP/UI. SQL có CHECK/FK/unique/trigger bảo vệ giao dịch ngay cả khi ghi ngoài API; các phép thử phá ràng buộc được rollback.

## Đối chiếu Android và Figma

Đã đọc metadata thật của **83 frame (65 User, 18 Admin)** từ `Nhom4.fig`: tên, kích thước, văn bản, màu, typography và cấu trúc con; ánh xạ từng frame tới Compose và trạng thái tương ứng trong [FIGMA_SCREEN_AUDIT.md](FIGMA_SCREEN_AUDIT.md). Cả 83 ánh xạ có ảnh Android đại diện từ lượt UI cuối. [Dữ liệu đối chiếu](qa/full-audit/figma-screen-audit.json) giữ thông tin gốc, không tự suy diễn màn hình chưa đọc. SHA-256 nguồn: `7C9D39E26CEC0CBA81E2E8C9877C49E8D3EC4FDA5E5353211E1A3581F7CC6BE7`.

Đã gọi Figma MCP design context và screenshot; cả hai bị giới hạn quota. Vì vậy, **chưa thể đối chiếu pixel từng frame với render Figma**, và ảnh Android trong bảng là bằng chứng trạng thái đại diện, không phải ảnh diff hoặc 83 ca nghiệm thu độc lập. Nhiều biến thể dữ liệu dùng cùng component được ánh xạ tới một ảnh. Không dựng ảnh Figma giả để thay nguồn.

- Font Roboto được dùng chung. Nhóm mới dùng xanh `#28734F`, nền `#FCFDFB`, chữ `#202E26`; nhóm mua bán trong file gốc dùng `#16845B`, `#F7F9F7`, `#17211C`. Giữ hai palette theo nguồn, không coi khác nhau giữa các nhóm là lỗi.
- Rà layout 360×800, header, lề, nút, card, bottom navigation và form cuộn dựa trên số đo nguồn cùng component đang có. Hình/tên/giá/số lượng đến từ API nên khác fixture Figma. Thanh trạng thái dùng Android thật, không chép giờ cố định của thiết kế.
- UI bổ sung kiểm tra lỗi 404 thật, kết quả tìm rỗng thật, validation form và Back; viewport 320×640 với font 1,3, footer và lỗi trường vẫn truy cập được khi cuộn. Các suite nghiệp vụ còn kiểm tra trạng thái chờ, từ chối, 2/3, hoàn tất, sự cố, hết phiên và xung đột.
- Admin xem giao nhận ở chế độ đúng quyền; không triển khai nút nhận hàng thay người dùng dù prototype có thể biểu diễn thao tác đó.

## Các phần còn thiếu hoặc chưa kiểm chứng

| Mục | Trạng thái thực tế / việc cần tiếp theo |
| --- | --- |
| Xác minh email/SĐT tại Cài đặt | Dòng giao diện đang disabled; chưa có luồng xác minh liên hệ hoàn chỉnh. Câu hướng dẫn trên form hồ sơ chưa đồng nghĩa tính năng đã hoạt động. |
| Chính sách bảo mật và xóa tài khoản | Hai dòng Cài đặt đang disabled; chưa có màn chính sách/quy trình xóa tài khoản và xử lý dữ liệu liên quan. |
| Figma render đầy đủ | Cần render/export Figma có thể truy cập để kiểm tra icon, khoảng cách, ngắt dòng và từng biến thể bằng ảnh gốc; chưa xác nhận pixel fidelity. |
| Email production | Đã kiểm thử reset password bằng outbox Development; chưa gửi SMTP thật hoặc nghiệm thu reset deep link trên điện thoại thật. |
| Bằng chứng video | Upload chứng cứ hiện nhận JPG/PNG. Admin có nhánh đọc media MP4/range và player, nhưng chưa có luồng upload/phát video được kiểm chứng từ đầu đến cuối; không coi video đã hoàn tất. |
| Thiết bị thật và mạng khác | Đã viết cấu hình USB/LAN trong README; chưa chạy trên điện thoại thật, chưa kiểm tra đầy đủ mất mạng giữa giao dịch hay mọi kích thước/font/IME. |
| Release/production | Chưa build ký/phát hành Android production, triển khai HTTPS/SMTP, tải lớn hay audit bảo mật độc lập. Debug build và tests local không chứng nhận production. |
| Lint Android | Còn 22 warnings: phiên bản dependency mới, đường vector dài/kích thước vector, thứ tự Modifier và gợi ý KTX. Không có lint error; chưa tối ưu vector hoặc nâng dependency hàng loạt vì cần giữ asset và tương thích. |

Chat dùng REST và cơ chế tải lại/polling đang có; chưa phải push/WebSocket thời gian thực. Ngân hàng và hãng vận chuyển thật nằm ngoài phạm vi theo yêu cầu: các trạng thái mô phỏng đã lưu SQL và kiểm thử, không gọi giao dịch tài chính ngoài hệ thống.

## Chạy lại và tài liệu vận hành

[README.md](../README.md) có yêu cầu môi trường, cấu hình SQL, migration/seed, Backend/Swagger, Emulator/USB/LAN, API base URL, 10 tài khoản demo và kịch bản ba người Smart Swap. Không commit tài khoản QA tạm hoặc JWT. Khi tái chạy trên database có lịch sử demo, hãy tạo sản phẩm/vòng mới thay vì mở khóa thủ công hàng đã giao dịch.

Kết quả ngày 12/11 trong các báo cáo tính năng cũ là lịch sử; ma trận và log `qa/full-audit` là đợt rà soát này. Các giới hạn trên vẫn còn, nên không ghi “hoàn thành toàn bộ” hoặc “tất cả màn hình khớp Figma”.

Sau kiểm thử đã thu hồi role ADMIN tạm và mọi phiên của đúng tài khoản QA ([kết quả](qa/full-audit/qa-revocation.json)), xóa tệp credential tạm, dừng host/Emulator do lần kiểm tra khởi chạy. Giữ log, APK và ảnh bằng chứng trong workspace; không thay đổi quyền của Admin demo hoặc dữ liệu giao nhận mẫu.
