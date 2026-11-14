# Android ReMarket

Mở **thư mục `Android/`** trong Android Studio. Nguồn Android, wrapper và cấu hình trước đây ở root đã được chuyển nguyên vào đây rồi chuyển Activity sang Compose. Giữ applicationId `com.ute.app_ban_do_cu` để không đổi định danh ứng dụng.

## Nền tảng

- Kotlin built-in của AGP 9.3.3; Compose compiler plugin 2.2.10; Compose BOM 2025.08.01.
- SDK 37, minSdk 24; wrapper Gradle 9.5.0, daemon JDK 25 theo `gradle/gradle-daemon-jvm.properties`. Gradle có thể tải JDK theo cấu hình hiện có; `local.properties` chỉ trỏ SDK cục bộ và bị ignore.
- Activity Compose → ViewModel/StateFlow → domain repository → implementation → Retrofit/Gson → API. DTO không đi thẳng vào UI.
- DI thủ công trong `AppContainer`; chưa cần framework DI hoặc NavHost cho các màn nghiệp vụ chưa được viết.
- Màu, khoảng cách và component tài khoản lấy từ phân tích Figma; font Roboto được đóng gói trong app, hỗ trợ tiếng Việt và các weight cần dùng.

## Kết nối

Khởi động Backend tại cổng 5080, apply migration và seed trước. Workspace hiện đặt `REMARKET_API_BASE_URL=http://127.0.0.1:5080/` trong `gradle.properties`. Sau khi kết nối thiết bị qua USB hoặc Wireless debugging, chạy `../tools/Start-ReMarketLocal.ps1` từ thư mục này để khởi động Backend và cấu hình ADB reverse. Script cũng dùng được cho Emulator.

Nếu Emulator không dùng ADB reverse, ghi đè khi build bằng `-PREMARKET_API_BASE_URL=http://10.0.2.2:5080/`. Địa chỉ `10.0.2.2` không dùng cho điện thoại thật. Chạy lại script sau khi ADB ngắt/kết nối lại; Android Studio không tự khởi động Backend.

Với thiết bị thật qua USB:

```powershell
adb reverse tcp:5080 tcp:5080
.\gradlew.bat :app:assembleDebug -PREMARKET_API_BASE_URL=http://127.0.0.1:5080/
```

URL cấu hình bằng Gradle property `REMARKET_API_BASE_URL`, phải có dấu `/` cuối và không chứa credentials/query. Debug chỉ cho HTTP tại 10.0.2.2/localhost/127.0.0.1 qua network security config; không bật cleartext cho toàn bộ domain. Bản release cần base URL HTTPS của backend thật. Nếu dùng LAN, phải cấu hình binding, host/firewall và domain HTTP debug cụ thể hoặc dùng HTTPS; không hardcode địa chỉ SQL Server vào app.

## Build và phạm vi

```powershell
.\gradlew.bat :app:assembleDebug
.\gradlew.bat :app:lintDebug :app:testDebugUnitTest
```

APK: `app/build/outputs/apk/debug/app-debug.apk`. `feature/auth` và `feature/account` có Splash, đăng ký/đăng nhập, quên/đặt lại mật khẩu, hồ sơ/sửa ảnh, cài đặt và logout; role từ API quyết định Home/Admin. DataStore/Android Keystore giữ token mã hóa, không giữ mật khẩu; interceptor/refresh chỉ gọi backend đúng origin. Xem [ACCOUNTS.md](../docs/ACCOUNTS.md) về endpoint, SMTP và test emulator.

`feature/catalog` có ViewModel/StateFlow riêng cho Home, danh mục/search/list/detail, form ảnh, preview/success và quản lý tin. Bottom Navigation hoạt động cả năm mục, Tin nhắn mở `feature/commerce`. Dùng chung JWT client và Coil ImageLoader; danh sách/giá/trạng thái từ API. Form giữ dữ liệu khi lỗi, ảnh tải từng file và giữ phần chưa gửi để thử lại. Sáu icon danh mục trích nguyên hình học vector từ `.fig`. Xem [CATALOG.md](../docs/CATALOG.md).

`feature/commerce` triển khai inbox/chat/ảnh/phiếu giảm giá, checkout, đơn mua–bán/chi tiết/theo dõi, thanh toán và vận chuyển mô phỏng, nhận hàng, đánh giá và hỗ trợ. Cá nhân mở Đơn hàng/Yêu cầu hỗ trợ; ADMIN có hàng chờ hỗ trợ. Polling3 giây theo lifecycle cho chat/đơn/ticket, tải lại thủ công, phân trang và tin cũ; không chạy socket hoặc tự cập nhật trạng thái giả. Các API mutation dùng GUID gửi lại và rowversion; đổi tài khoản xóa state/hủy request cũ. Xem [COMMERCE.md](../docs/COMMERCE.md) và [kiểm thử](../docs/COMMERCE_VALIDATION.md).

`feature/exchange` nối tab Trao đổi và Cá nhân → Trao đổi của tôi: khám phá/bộ lọc, đăng/sửa/ẩn/xóa tin, chọn món của mình, gửi/nhận đề nghị, thương lượng tiền bù/lịch hẹn, đồng ý/từ chối/hủy, chat, giao nhận hai phía/ảnh bằng chứng và đánh giá. Dùng `TraoDoiViewModel`/StateFlow và `TraoDoiRepository`/Retrofit thật. Mở chi tiết/chọn món không tạo giao dịch; sản phẩm chỉ được giữ sau đồng ý cùng phiên bản. Ảnh Coil riêng tư dùng authenticated client; retry giữ key và dữ liệu form, cập nhật phiên bản khi tải lại. Xem [EXCHANGE.md](../docs/EXCHANGE.md) và [kiểm thử](../docs/EXCHANGE_VALIDATION.md).

`feature/smartswap` nối **Trao đổi → Tìm vòng trao đổi** và **Cá nhân → Smart Swap của tôi**: tìm vòng từ SQL, danh sách/chi tiết 3 người, sơ đồ chiều giao, lời mời, xác nhận phiên bản, chat nhóm/ảnh, bottom sheet tiền bù, thông báo, thông tin giao nhận, vận đơn mô phỏng, tiến độ nhận đúng 0/3–3/3, sự cố, ảnh bằng chứng và đánh giá hai thành viên. Dùng `SmartSwapViewModel`/StateFlow và `SmartSwapRepository`/Retrofit, cùng JWT/DataStore/Coil hiện có. Xem [hướng dẫn](../docs/SMART_SWAP_ANDROID.md) và [kiểm thử Emulator](../docs/SMART_SWAP_ANDROID_VALIDATION.md).

Sửa wrapper Windows về cách gọi `GradleWrapperMain` với classpath jar thực để tránh lỗi `-classpath` rỗng của wrapper ban đầu trong môi trường hiện tại.
