# ReMarket

Ứng dụng mua bán đồ cũ, trao đổi 1–1 và Smart Swap 3 bên từ [Nhom4.fig](Nhom4.fig). Dự án hiện có được giữ nguyên: Android Kotlin/Jetpack Compose/MVVM/StateFlow/Retrofit/Coil/DataStore mã hóa; Backend ASP.NET Core 8/EF Core 8/JWT; SQL Server. Thanh toán và vận chuyển là mô phỏng có trạng thái trong database.

Kết quả kiểm tra, lỗi đã sửa và chức năng còn thiếu nằm trong [PROJECT_AUDIT.md](docs/PROJECT_AUDIT.md). Build thành công không đồng nghĩa đã nghiệm thu toàn bộ thiết kế hoặc triển khai production.

Đợt nâng cấp giao diện: xem [UI_UPGRADE.md](docs/UI_UPGRADE.md) cho 28 file UI đã cải thiện, kết quả 22 ca hồi quy, kiểm tra bàn phím/màn hình nhỏ và giới hạn kiểm chứng trên điện thoại thật.

Đợt rà soát 13/11/2026: Backend/Android Debug build đạt; 1.287 assertions HTTP/SQL/database và 20 ca Android đạt; 121/121 route có phản hồi thành công. Còn ba mục Cài đặt chưa hoạt động và chưa đối chiếu pixel đầy đủ với render Figma. Xem [ma trận API](docs/API_TEST_MATRIX.md) và [83 frame thiết kế](docs/FIGMA_SCREEN_AUDIT.md).

## Yêu cầu môi trường

| Thành phần | Cấu hình dự án |
| --- | --- |
| Backend | .NET SDK 8.0.4xx; EF CLI 8.0.33 theo local tool manifest |
| Database | SQL Server 2022 đã dùng để kiểm tra; Windows hoặc SQL Authentication; `sqlcmd` cho script kiểm tra database mới |
| Android | Android Studio, Android SDK 37, platform-tools/ADB; thiết bị tối thiểu API 24 |
| Build Android | Wrapper Gradle 9.5.0, AGP 9.3.3, daemon JDK 25 theo `Android/gradle/gradle-daemon-jvm.properties` |
| Emulator kiểm tra | API 36, 360×800, density 160; có thử viewport lớn/nhỏ trong các ca riêng |
| Tài nguyên | Nên có RAM 16 GB trở lên; chạy build và Emulator lần lượt khi máy thiếu RAM |

Cần mạng để tải NuGet/Maven/SDK/JDK lần đầu. Không cần .NET cho Android. Mở `Android/` trong Android Studio, `Backend/ReMarket.sln` trong IDE .NET; không tạo project mới.

## Cấu hình SQL Server, migration và Backend

Trong PowerShell tại root:

```powershell
cd Backend
. ./scripts/Use-DotNet8.ps1
$env:ASPNETCORE_ENVIRONMENT='Development'
$env:ConnectionStrings__ReMarket='Server=localhost;Database=ReMarketDemo;Trusted_Connection=True;TrustServerCertificate=True'
./scripts/Initialize-Authentication.ps1
./scripts/Initialize-Database.ps1 -SeedDemo
dotnet build ReMarket.sln -c Release
dotnet run --project src/ReMarket.Api --no-launch-profile -- --urls http://127.0.0.1:5080
```

`Use-DotNet8.ps1` chọn SDK 8 đã cài; máy phát triển hiện tại có bản tại `%LOCALAPPDATA%\ReMarket\dotnet8`. JWT signing key được tạo trong .NET user-secrets, không lưu trong Android. Có thể cấp key bằng biến môi trường `Jwt__SigningKey` cho host. Tài khoản SQL chạy migration cần quyền tạo/cập nhật schema; người chạy HTTP/SQL tests cần quyền tạo/xóa database test riêng.

SQL Express: đổi server thành `localhost\SQLEXPRESS`. SQL Authentication: dùng `Server=...;Database=ReMarketDemo;User Id=...;Password=...;TrustServerCertificate=True` trong biến môi trường hoặc user-secrets. `TrustServerCertificate=True` phục vụ SQL local. Không commit mật khẩu SQL vào appsettings hoặc đưa chuỗi SQL vào Android.

Các lệnh riêng, tiếp tục tại `Backend/`:

```powershell
dotnet tool restore
dotnet ef database update --project src/ReMarket.Infrastructure --startup-project src/ReMarket.Api
dotnet run --project src/ReMarket.Api --no-launch-profile -- --seed-demo
dotnet run --project src/ReMarket.Api --no-launch-profile -- --seed-smart-delivery-demo
# Tài khoản cần đăng ký thật trước khi cấp ADMIN:
dotnet run --project src/ReMarket.Api --no-launch-profile -- --grant-admin 'email-da-dang-ky@example.test'
```

Hiện có 48 bảng nghiệp vụ và 11 migrations. Host bình thường không tự migration/seed. Các lệnh seed/cấp Admin chỉ chạy trong Development; đăng nhập lại sau khi cấp role. Seed có tính lặp, không reset mật khẩu, đơn hay tiến độ cũ. Không xóa database hoặc tái kích hoạt sản phẩm đang giao dịch để “làm mới” demo.

- Swagger: `http://127.0.0.1:5080/swagger`; dùng access token từ `POST /api/taikhoan/dangnhap` trong **Authorize**.
- `/health/live`: host; `/health/ready`: SQL/schema, trả 200 khi sẵn sàng và 503 nếu chưa sẵn sàng.
- `/api/hethong/trangthai`: thông tin kết nối. USER không gọi được API Admin.

## Khởi động demo local nhanh

Workspace hiện cấu hình API `http://127.0.0.1:5080/` trong `Android/gradle.properties`, dùng được trên điện thoại thật hoặc Emulator qua ADB reverse. Sau khi kết nối một thiết bị trong Android Studio, chạy PowerShell tại root:

```powershell
./tools/Start-ReMarketLocal.ps1
# Nếu có nhiều thiết bị, chọn đúng serial từ adb devices:
./tools/Start-ReMarketLocal.ps1 -DeviceSerial 'SERIAL'
```

Script build/khởi động Backend Development ở nền khi chưa chạy, kiểm tra SQL sẵn sàng rồi nối cổng `5080` của thiết bị về máy tính. Không migration/seed hoặc reset dữ liệu. Sau đó bấm Run trong Android Studio. SQL phải đã cấu hình và chạy migrations theo phần trên. Log/PID host nằm trong `Backend/.local/runtime/`. Có thể dùng `-BackendOnly` khi chỉ cần API.

Chạy lại lệnh sau khi khởi động máy hoặc khi ADB bị ngắt/kết nối lại (USB hoặc Wireless debugging). Android Studio không tự khởi động Backend. Thông báo “Chưa kết nối được ReMarket” cần kiểm tra host và cổng ADB trước; đổi tài khoản không giải quyết lỗi mạng.

## Chạy Android Emulator

Trong Android Studio, chọn **File → Open → `D:\Project\App_Ban_Do_Cu\Android`** rồi chờ Gradle Sync hoàn tất. Đây là thư mục chứa `settings.gradle.kts` và `gradlew.bat`. Nếu thấy lỗi `Directory '...App_Ban_Do_Cu' does not contain a Gradle build`, IDE đang liên kết nhầm thư mục gốc: mở lại thư mục `Android` như trên. Không chạy `gradle init` để tạo project khác. Workspace gốc cũng đã sửa `.idea/gradle.xml` để liên kết tới `Android`; nếu vẫn mở workspace gốc, đóng/mở lại và Sync để IDE nạp cấu hình đã sửa.

Tạo AVD bằng Device Manager, khởi động thiết bị rồi chạy lệnh demo local trên. Nếu muốn Emulator truy cập trực tiếp máy tính không qua ADB reverse, build với `-PREMARKET_API_BASE_URL=http://10.0.2.2:5080/`; đây là giá trị fallback trong build script khi không có Gradle property.

```powershell
cd Android
./gradlew.bat :app:assembleDebug
adb devices
adb install -r app/build/outputs/apk/debug/app-debug.apk
adb shell am start -n com.ute.app_ban_do_cu/.MainActivity
```

Có thể dùng Run trong Android Studio. APK: [app-debug.apk](Android/app/build/outputs/apk/debug/app-debug.apk). Khi có nhiều thiết bị, thêm `adb -s SERIAL ...`.

## Điện thoại thật và địa chỉ API

Qua USB, bật USB debugging, xác nhận thiết bị bằng `adb devices`, rồi:

```powershell
adb reverse tcp:5080 tcp:5080
cd Android
./gradlew.bat :app:assembleDebug '-PREMARKET_API_BASE_URL=http://127.0.0.1:5080/'
adb install -r app/build/outputs/apk/debug/app-debug.apk
```

`REMARKET_API_BASE_URL` là Gradle property, có dấu `/` cuối, không chứa query/credentials. Hiện `Android/gradle.properties` đặt `http://127.0.0.1:5080/`; tham số `-P` khi build có thể ghi đè. Đổi URL cần build/cài lại APK vì giá trị nằm trong `BuildConfig.API_BASE_URL`. Emulator có thể dùng cùng URL qua ADB reverse hoặc ghi đè bằng `http://10.0.2.2:5080/`.

Với LAN, Backend phải bind địa chỉ LAN/`0.0.0.0`; cấu hình `AllowedHosts` chứa IP/hostname máy; mở cổng firewall và cho điện thoại cùng mạng. Debug chỉ cho HTTP tại localhost/127.0.0.1/10.0.2.2: cần thêm đúng host LAN vào `Android/app/src/debug/res/xml/network_security_config.xml` hoặc dùng HTTPS. Release dùng HTTPS và signing riêng. Không dùng địa chỉ SQL Server làm API URL.

## Tài khoản demo

Mật khẩu **khi seed lần đầu**: `ReMarketDemo!2026`, hoặc giá trị `REMARKET_DEMO_PASSWORD` đã đặt trước lần seed đó. Seed lại giữ mật khẩu đã đổi; database lưu hash có salt. Android không tự điền hoặc nhận diện đặc biệt tài khoản demo.

| Email | Hồ sơ / vai trò |
| --- | --- |
|`linh@remarket.example.test`|Linh Nguyễn, USER + ADMIN|
|`anh@remarket.example.test`|Nguyễn Minh Anh, USER|
|`nam@remarket.example.test`|Nguyễn Hoàng Nam, USER|
|`vy@remarket.example.test`|Thảo Vy, USER|
|`bao@remarket.example.test`|Quốc Bảo, USER; fixture khóa có hạn, có thể đã mở theo thời gian|
|`huy@remarket.example.test`|Trần Gia Huy, USER|
|`ha@remarket.example.test`|Lê Thu Hà, USER|
|`swap-ship-a@remarket.example.test`|A của demo giao nhận Smart Swap|
|`swap-ship-b@remarket.example.test`|B của demo giao nhận Smart Swap|
|`swap-ship-c@remarket.example.test`|C của demo giao nhận Smart Swap|

Ba tài khoản `swap-ship-*` được tạo bởi `--seed-smart-delivery-demo`. Tài khoản email ngẫu nhiên/`*-ui-*` là fixture kiểm thử, không phải tài khoản demo cố định. Xem [dữ liệu demo](docs/database/DU_LIEU_DEMO.md) và kết quả kiểm tra thực tế trong [báo cáo](docs/PROJECT_AUDIT.md).

## Demo Smart Swap bằng ba người dùng

Dùng ba thiết bị/Emulator hoặc đăng xuất và đổi tài khoản lần lượt. Admin ở phiên riêng để duyệt tin. Anh/Nam/Vy có thể tạo tin mới; không sửa sản phẩm đã gắn giao dịch lịch sử để tái dùng.

1. Mỗi USER đăng tin mới có ảnh, cùng “Hà Nội”, bật Smart Swap và bù tối đa 2.000.000 đ. A có **iPhone demo 13** giá 10 triệu, muốn **Laptop demo 13**; B có **Laptop demo 13** giá 12 triệu, muốn **iPad demo 13**; C có **iPad demo 13** giá 8 triệu, muốn **iPhone demo 13**. Chọn tình trạng/danh mục/khoảng giá phù hợp với món muốn nhận.
2. Admin duyệt ba tin. Sản phẩm phải còn hiển thị và chưa bị giữ/bán/trao đổi.
3. A vào **Trao đổi → Tìm vòng trao đổi**, chọn tin iPhone và tìm. Thuật toán đọc SQL để tìm chiều giao **A → C → B → A**. Tìm/xem trước chưa tạo giao dịch.
4. Xem sản phẩm/thành viên/phương án bù. Ví dụ trên: A trả B 2 triệu, C trả B 2 triệu; tổng trả bằng tổng nhận. Gửi đề nghị để lưu vòng và thông báo.
5. B/C vào **Cá nhân → Smart Swap của tôi**, xác nhận đúng phiên bản. Chỉ 2/3 đồng ý chưa mở bàn giao; một người từ chối thì không tiếp tục vòng.
6. Thử đề xuất tiền bù trong chat nhóm. Bản mới lưu lịch sử, yêu cầu **cả ba đồng ý lại**. Không sửa khoản chỉ thuộc hai người còn lại. Backend kiểm tra giới hạn từng người và cân bằng tiền.
7. Đủ 3/3 đồng ý, mỗi người xác nhận thông tin giao/nhận. Tạo vận chuyển sinh đúng **ba đơn** theo ba cạnh. Người gửi nhập mã `DEMO-A/B/C`, cập nhật **Chờ gửi → Đã gửi → Đang vận chuyển → Đã giao**. Không gọi ngân hàng/hãng vận chuyển thật.
8. Người nhận tự kiểm tra, xác nhận đúng món/đã nhận và tiền bù phần mình. Ba vận đơn “Đã giao” không tự sinh receipt. Tiến độ 0/3, 1/3, 2/3, 3/3; **2/3 chưa hoàn tất**.
9. Sau nhận đúng có thể tải bằng chứng. Đủ 3/3 mới mở hoàn tất/đánh giá hai đối tác. Admin không xác nhận hàng thay thành viên.
10. Demo sự cố bằng vòng mới: đúng người nhận báo sai/chưa nhận hàng; vòng tạm dừng, chat vẫn hoạt động. Admin xem chứng cứ, ghi phương án và xác minh thực hiện. Giữ tạm dừng vẫn mở khiếu nại để xử lý tiếp; tiếp tục chỉ khôi phục khi đủ điều kiện. Không tạo receipt qua xử lý tranh chấp.

Demo nhanh: `--seed-smart-delivery-demo` tạo vòng riêng ở 2/3; A/B có xác nhận fixture, C chưa xác nhận. Đăng nhập C xem và tự hoàn tất nếu muốn; seed lại không reset tiến độ. `SS-0301` là vòng legacy, không dùng thay kịch bản vận đơn mới. Tests tự tạo vòng riêng để giữ nguyên demo gốc.

## Kiểm thử

```powershell
cd Backend
. ./scripts/Use-DotNet8.ps1
dotnet build ReMarket.sln -c Release
dotnet run --project tests/ReMarket.DatabaseChecks -c Release -- --all --coverage-file ../docs/qa/full-audit/api-coverage.json
./scripts/Verify-FreshDatabase.ps1
```

`--all` chạy auth, listings, commerce, exchange, smart-swaps, smart-swap-workflow, smart-delivery và admin; có thể chạy riêng bằng từng cờ. Suite HTTP tạo/xóa đúng DB `ReMarketTest_<guid>` và media của nó. Graph được so với oracle trên 100 đồ thị sinh. Script fresh-DB áp SQL idempotent hai lần, seed/check và tự dọn; không reset ReMarketDemo.

```powershell
cd Android
./gradlew.bat :app:assembleDebug :app:assembleDebugAndroidTest :app:lintDebug :app:testDebugUnitTest
adb install -r app/build/outputs/apk/debug/app-debug.apk
adb install -r app/build/outputs/apk/androidTest/debug/app-debug-androidTest.apk
# Tài khoản QA đã đăng ký/cấp ADMIN, Backend và Emulator đang chạy:
$classes=@('QuanTriEndToEndTest','TaiKhoanEndToEndTest','SanPhamEndToEndTest',
  'MuaBanEndToEndTest','TraoDoiEndToEndTest','SmartSwapEndToEndTest','KiemTraGiaoDienTest') |
  ForEach-Object { 'com.ute.app_ban_do_cu.'+$_ }
adb shell am instrument -w -r -e class ($classes -join ',') `
  -e adminEmail $adminEmail -e adminPassword $adminPassword `
  com.ute.app_ban_do_cu.test/androidx.test.runner.AndroidJUnitRunner
```

Tests dùng API/SQL thật, tự tạo tài khoản/tin/giao dịch riêng. Ảnh picker được đưa qua callback trong test; không coi đó là kiểm tra gallery trên điện thoại thật. Chỉ kết quả `OK (...)` mới xác nhận JUnit đạt, không dựa vào exit code `adb` đơn lẻ.

## Cấu trúc và tài liệu

`Android/.../core,data,domain,feature` gồm auth/account/catalog/commerce/exchange/smartswap/admin. `Backend/src/` gồm Domain/Application/Infrastructure/Api; `Backend/tests/ReMarket.DatabaseChecks` chứa runner SQL/HTTP. `docs/qa/full-audit` lưu log, độ phủ API và bằng chứng lần rà soát.

[Kiến trúc](docs/ARCHITECTURE.md) · [CSDL](docs/database/THIET_KE_CSDL.md) · [Figma](FIGMA_ANALYSIS.md) · [Tài khoản](docs/ACCOUNTS.md) · [Catalog](docs/CATALOG.md) · [Mua bán](docs/COMMERCE.md) · [Trao đổi 1–1](docs/EXCHANGE.md) · [Tìm vòng](docs/SMART_SWAP_SEARCH.md) · [Đồng thuận](docs/SMART_SWAP_WORKFLOW.md) · [Giao nhận](docs/SMART_SWAP_DELIVERY.md) · [Android Smart Swap](docs/SMART_SWAP_ANDROID.md) · [Admin](docs/ADMIN.md).
