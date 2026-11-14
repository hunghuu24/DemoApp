# Kiểm thử Admin ReMarket —12/11/2026

Báo cáo này giữ kết quả lịch sử ngày 12/11. Xem [PROJECT_AUDIT.md](PROJECT_AUDIT.md) cho đợt rà soát ngày 13/11, các lỗi mới đã sửa, ma trận 121 route và giới hạn còn lại.

Thực hiện trên workspace hiện có, SQL Server2022 thật tại localhost. Các bộ HTTP/SQL tự tạo `ReMarketTest_<guid>`, chạy11 migrations và seed, rồi dọn đúng database/thư mục media do chính lần chạy tạo. Không dùng EF InMemory, SQLite hoặc mock repository.

| Kiểm tra Backend | Kết quả | Log |
| --- | --- | --- |
|Release build solution|0 lỗi,0 cảnh báo|[backend-build.txt](qa/admin/backend-build.txt)|
|Admin HTTP/SQL|135 bước đạt|[backend-checks.txt](qa/admin/backend-checks.txt)|
|Tài khoản/JWT/phân quyền|54 bước đạt|[auth-regression.txt](qa/admin/auth-regression.txt)|
|Catalog/tin đăng|94 bước đạt|[listings-regression.txt](qa/admin/listings-regression.txt)|
|Mua bán|119 bước đạt|[commerce-regression.txt](qa/admin/commerce-regression.txt)|
|Trao đổi1–1|125 bước đạt|[exchange-regression.txt](qa/admin/exchange-regression.txt)|
|Đồng thuận Smart Swap|170 bước đạt|[smart-swap-workflow-regression.txt](qa/admin/smart-swap-workflow-regression.txt)|
|Giao nhận/bằng chứng/đánh giá Smart Swap|255 bước đạt|[smart-delivery-regression.txt](qa/admin/smart-delivery-regression.txt)|
|SQL schema idempotent2 lần trên DB mới, seed và checks|20 đạt,0 lỗi|[fresh-database.txt](qa/admin/fresh-database.txt)|

Tổng **972** bước HTTP/SQL/ràng buộc trong đợt này. Mỗi số là số assertion/request được console runner ghi nhận, không phải972 test case độc lập.

## Bất biến được xác minh

- Ma trận21 route quản trị: chưa đăng nhập401, USER403, bao gồm ghi và file riêng tư. Role ADMIN bị thu hồi trong SQL làm JWT hiện hữu bị403 ngay.
- Khóa0/7/30 ngày; thời hạn khác400; không tự khóa; thu hồi phiên đang hoạt động; không đăng nhập khi khóa; mở khóa cần rà soát; phiên cũ vẫn401; worker giải phóng khóa có hạn đã hết.
- Thống kê người dùng khớp COUNT SQL; tổng cột biểu đồ khớp tổng mua bán/trao đổi; khoảng ngày đảo ngược400.
- Thêm danh mục cha/con, icon/thứ tự/hiển thị; API công khai trả icon đã lưu và ẩn danh mục tắt; chu trình400; rowversion cũ409.
- Tạo tin/upload thật, từ chối có lý do/lịch sử, duyệt rồi ẩn có lịch sử. Hồi quy catalog kiểm tra quyền sở hữu và chống sửa/xóa tin đang giữ món.
- Admin đọc đủ3 participant/3 sản phẩm/3 vận đơn và2 receipt người nhận. Media thuộc giao dịch200; USER403. Admin gọi API USER nhận hàng của người khác404; route Admin nhận hàng không tồn tại404.
- Khiếu nại được tiếp nhận rồi ghi quyết định; retry cùng key không nhân đôi, đổi nội dung cùng key409. Quyết định chờ thực hiện, không đóng ngay. Chưa xác nhận rà soát thực hiện400.
- Xác minh tranh chấp không thay số receipt. Tiếp tục trở về2/3 đang thực hiện; không hoàn tất. Hai khiếu nại đồng thời: đóng một vẫn tạm dừng; đóng hết mới khôi phục. Người nhận thứ ba tự xác nhận mới hoàn tất.
- Admin ghi nhận phản ánh chỉ giữa hai participant thật; người ngoài400. Retry không nhân đôi; audit chỉ rõ Admin ghi nhận. Phương án kết thúc đã xác minh hủy root, nhả đúng hold, không tạo receipt hoặc xác nhận tiền bù.
- Script SQL giữ48 bảng,100 FK,70 CHECK,22 trigger và11 migration. [Kiểm tra schema/demo](qa/admin/schema-and-demo.txt) xác nhận vòng demo gốc vẫn `DangBanGiao / DangThucHien /2` người nhận.

## Android

Build debug, APK kiểm thử và lint đạt: [android-build.txt](qa/admin/android-build.txt). Lint0 lỗi,22 cảnh báo: phiên bản thư viện, đường vector Figma dài, kích thước sơ đồ vector cũ, thứ tự Modifier của helper cũ và gợi ý dùng extension URI. Không tắt lint detector.

**Lượt cuối:8 kiểm thử đạt,0 lỗi,0 bỏ qua**,161.864 giây trên Android Emulator API36,360×800, density160: [emulator-final.txt](qa/admin/emulator-final.txt). Gồm2 luồng Admin,3 ca tài khoản,2 ca catalog và1 ca hỗ trợ quản trị. Lượt riêng2 ca Admin cũng đạt trước đó trong97.073 giây: [emulator-tests.txt](qa/admin/emulator-tests.txt). Kiểm tra giao diện bằng Compose instrumentation, đăng ký/JWT/SQL và media thật:

- Người dùng: tìm kiếm, chi tiết, khóa7 ngày, JWT đã khóa401, rà soát/mở khóa.
- Tin đăng: ảnh thật, từ chối, ba checkbox kiểm duyệt, duyệt và ẩn có lý do.
- Danh mục: tạo tên/icon/thứ tự, sửa tên/tắt hiển thị, đọc lại kết quả API.
- Báo cáo7 ngày, lọc giao dịch mua bán/1–1, danh sách khiếu nại, drawer, back và đăng xuất. USER gọi quản trị403.
- Vòng3 người độc lập: Admin xem3 thành viên/sản phẩm/vận đơn,2/3 người nhận và bằng chứng; tạo phản ánh, ghi quyết định, xác minh thực hiện. Vòng vẫn2/3 sau xử lý; Admin gọi nhận hàng404. Tài khoản thứ ba tự xác nhận mới3/3 và UI tải lại trạng thái hoàn tất.

Đã xem ảnh chụp thực tế để kiểm tra chữ tiếng Việt, xuống dòng, màu, icon, form cuộn và footer. [18 ảnh chụp](qa/admin/screenshots): [tổng quan](qa/admin/screenshots/dashboard.png), [menu](qa/admin/screenshots/admin-menu.png), [kiểm duyệt](qa/admin/screenshots/moderation-checklist.png), [Smart Swap2/3](qa/admin/screenshots/smart-swap-two-of-three.png), [quyết định](qa/admin/screenshots/dispute-decision.png), [sau xử lý vẫn2/3](qa/admin/screenshots/resolved-still-two-of-three.png), [người thứ ba hoàn tất](qa/admin/screenshots/completed-by-third-user.png).

Lần chạy đầu phát hiện fixture tải bằng chứng trước receipt; API trả409 đúng quy tắc. Đã sửa fixture upload sau khi người dùng tự xác nhận, build lại APK test và chạy lại đạt cả hai ca. Vòng lỗi chỉ chứa tài khoản/sản phẩm QA đã được kết thúc qua API quản trị, không tạo receipt. Lần cold boot Emulator xuất hiện ANR của System UI; đã đóng hộp thoại/chờ hệ thống ổn định, không thay mã ứng dụng để che lỗi môi trường.

Ca hồi quy catalog ban đầu chọn dropdown ngay khi bàn phím đang mở; đã bổ sung đóng bàn phím, kiểm tra danh mục API có trong form và chờ lựa chọn xuất hiện. Lượt8 ca cuối đã kiểm tra thành công đăng/sửa/ẩn/đã bán/xóa, tìm kiếm/lọc/phân trang/danh mục, đăng ký/đăng nhập/hồ sơ/đăng xuất/DataStore và hỗ trợ quản trị. Bộ ánh xạ icon dùng chung với catalog; lịch sử/thỏa thuận/thanh toán/vận chuyển Admin dùng nhãn tiếng Việt. [SHA256 APK cuối](qa/admin/apk-sha256.txt).

Source test: `QuanTriEndToEndTest` tạo tài khoản/tin/ảnh/vòng riêng bằng REST API. Hai luồng kiểm tra quản lý người dùng/khóa/kiểm duyệt/danh mục/báo cáo/menu/đăng xuất và Smart Swap2/3/bằng chứng/tạo tranh chấp/quyết định/xác minh. Không tiêu thụ vòng demo2/3 đã có.

Đã thu hồi riêng role ADMIN và mọi phiên của tài khoản QA do lần này đăng ký; xóa file thông tin đăng nhập tạm; dừng host API và Emulator do tác vụ khởi chạy. [Dọn môi trường](qa/admin/cleanup.txt). Truy vấn cuối xác nhận không còn hold hoạt động của các tài khoản `admin-ui-*`, còn vòng demo gốc vẫn2/3. Dữ liệu/audit của các fixture được giữ dưới dạng lịch sử, không xóa dây chuyền hồ sơ người dùng/giao dịch.

## Giới hạn đối chiếu

Đã đọc18 frame quản trị và xuất9 icon vector gốc từ file `.fig`. Hai lệnh Figma MCP lấy design context/screenshot bị chặn quota Starter; không có screenshot Figma mới để so pixel. Metadata layout/màu/font/nội dung nguồn được lưu trong [assets/admin](assets/admin). Không gọi đây là nghiệm thu pixel tuyệt đối. Ảnh/UI dùng dữ liệu thật nên tên, số lượng, độ dài và biểu đồ khác fixture Figma.

Video evidence có player Android dùng bearer và API range, nhưng chỉ ghi nhận là đã chạy nếu có fixture video hợp lệ trong kiểm tra riêng. CSDL seed cũ có thể tham chiếu tệp chưa tồn tại; API trả404 và UI hiển thị lỗi/thử lại, không tạo ảnh bằng chứng giả.

Thanh toán/vận chuyển tiếp tục mô phỏng. Xác minh quyết định là thao tác quản trị dựa trên chứng cứ ngoài ứng dụng; không chứng minh chuyển tiền thật hoặc thay xác nhận hàng của thành viên.
