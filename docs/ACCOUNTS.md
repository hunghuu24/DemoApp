# ReMarket — Tài khoản, JWT và Android

> Tài liệu giai đoạn tài khoản. Catalog/Home đầy đủ và quản lý tin đã được triển khai tiếp, xem [CATALOG.md](CATALOG.md); các chức năng tài khoản bên dưới được giữ lại.

Triển khai tiếp trên solution/module hiện có, không tạo lại Backend hoặc Android. Các màn đã nối API thật: Splash, đăng ký, đăng nhập, quên/đặt lại mật khẩu, thông tin cá nhân, sửa hồ sơ/ảnh đại diện, cài đặt, đổi mật khẩu và đăng xuất. USER vào trang chủ; ADMIN vào tổng quan quản trị. Hai trang đích đọc số liệu từ SQL, chưa thay thế toàn bộ chức năng mua bán/Admin.

## Chạy dự án

```powershell
cd Backend
. .\scripts\Use-DotNet8.ps1
.\scripts\Initialize-Authentication.ps1
.\scripts\Initialize-Database.ps1
dotnet run --project src/ReMarket.Api
```

Khóa ký JWT phát sinh ngẫu nhiên và lưu bằng development user-secrets, không nằm trong appsettings/Git/Android và không in ra console. Script giữ nguyên khóa nếu đã có. Môi trường khác cung cấp `Jwt__SigningKey` (Base64 của ít nhất 32 byte ngẫu nhiên), issuer/audience đúng cấu hình. Không tạo khóa mặc định dùng chung hoặc tự đổi khóa mỗi lần khởi động.

Mở `Android/` trong Android Studio hoặc build `:app:assembleDebug`. Emulator dùng `http://10.0.2.2:5080/`. Không có username/password điền sẵn hoặc nhánh đăng nhập giả; tài khoản mới phải đăng ký và được xác minh bằng hash trong SQL.

## Vai trò và cấp quyền

Migration `TaiKhoanJwtVaKhoiPhuc` chuyển role cũ `NguoiDung/QuanTriVien` thành **USER/ADMIN**, giữ nguyên ID và quan hệ; DB mới cũng được tạo đủ hai role. Đăng ký chỉ gán USER. Payload có trường tự cấp role bị từ chối, không có HTTP API cho người dùng tự nâng quyền.

Để thử Admin mà không cần dùng tài khoản mẫu: đăng ký một tài khoản thật, sau đó operator có quyền DB chạy CLI **Development**:

```powershell
dotnet run --project src/ReMarket.Api -- --grant-admin email-da-dang-ky@example.test
```

Đăng nhập lại trên app để chuyển vào quản trị. Đây là lệnh operator ngoài HTTP, không phải tài khoản hoặc mật khẩu đặc biệt. Seed demo vẫn dùng các bản ghi đã băm trong SQL; không được dùng seed làm cơ chế xác thực thay thế.

## API

| Method / URL | Xác thực | Ý nghĩa |
| --- | --- | --- |
| POST `/api/taikhoan/dangky` | Public, rate limit | hoTen, email, soDienThoai, matKhau, xacNhanMatKhau, dongYDieuKhoan; trả 201 và hồ sơ, chưa tự đăng nhập |
| POST `/api/taikhoan/dangnhap` | Public, rate limit | taiKhoan (email/SĐT), matKhau; trả access/refresh và hồ sơ/vaiTro |
| POST `/api/taikhoan/lam-moi` | Refresh token trong body | Rotate refresh token + jti; refresh cũ/access cũ không dùng lại được |
| POST `/api/taikhoan/dangxuat` | Bearer | Thu hồi phiên thiết bị hiện tại, 204; không chỉ xóa token ở app |
| POST `/api/taikhoan/quenmatkhau` | Public, rate limit | Email; phản hồi 202 chung cho email tồn tại/không tồn tại, không trả token |
| POST `/api/taikhoan/datlaimatkhau` | Token một lần trong body | email, token, matKhauMoi, xacNhanMatKhau; 204, thu hồi mọi phiên của tài khoản |
| POST `/api/taikhoan/doimatkhau` | Bearer, rate limit | Kiểm tra mật khẩu cũ, đổi hash và thu hồi mọi phiên |
| GET `/api/taikhoan/toi` | Bearer | Hồ sơ của subject trong JWT, không nhận userId từ client |
| PUT `/api/taikhoan/toi` | Bearer | hoTen/email/SĐT/địa chỉ/tỉnh thành + phienBan Base64; chống ghi đè bằng rowversion, 409 khi cũ |
| POST `/api/taikhoan/anh-dai-dien` | Bearer | multipart field `anh`, JPG/PNG ≤2 MB; decode, giới hạn kích thước, crop 512×512, encode JPEG bỏ metadata |
| GET `/api/taikhoan/anh-dai-dien` | Bearer | Chỉ ảnh của tài khoản hiện tại, private/no-store, không trả đường dẫn file nội bộ |
| GET `/api/trangchu` | USER | Danh mục/tin đang hiển thị từ SQL |
| GET `/api/admin/tongquan` | ADMIN | Các tổng số từ SQL; USER nhận 403 |

Đổi email/SĐT làm mất cờ xác minh tương ứng; không hiển thị “đã xác minh” cho tài khoản mới. Email chuẩn hóa phục vụ unique/login; SĐT Việt Nam được chuẩn hóa về dạng 0xxxxxxxxx, chấp nhận đầu vào +84. Mật khẩu 8–128 ký tự gồm chữ/số theo Figma, PBKDF2 Identity V3 với 210.000 vòng, salt ngẫu nhiên; hash cũ được nâng khi đăng nhập đúng.

## Token và phiên

- Access JWT HS256 15 phút, có sub/sid/jti/role; kiểm tra signature, thuật toán, issuer, audience, expiry (clock skew 30 giây).
- Refresh token ngẫu nhiên 32 byte, hạn tối đa 7 ngày cho phiên; SQL chỉ giữ SHA-256 của refresh token. Không gửi refresh trong URL/cookie hoặc log.
- Middleware kiểm tra session chưa thu hồi/hết hạn, jti hiện hành và user hoạt động ở mỗi request. Role lấy lại từ SQL để việc bỏ quyền/khóa user có hiệu lực ngay cả khi JWT chưa hết hạn.
- Logout thu hồi phiên hiện tại. Đổi/đặt lại mật khẩu thu hồi mọi phiên. Refresh đồng thời dùng rowversion; token đã thay thế không thể replay.
- Giao dịch đăng nhập/đặt lại mật khẩu khóa hàng user theo thứ tự để tránh tạo phiên bằng mật khẩu cũ sau khi reset đã hoàn tất.

## Khôi phục mật khẩu và email

Mã khôi phục ngẫu nhiên, SQL chỉ lưu hash, hết hạn 15 phút, dùng một lần. Token/email phải khớp; dùng lại, sửa token hoặc quá hạn đều bị từ chối. Sau reset, mật khẩu cũ và các token cũ bị vô hiệu.

**Development chưa cấu hình SMTP:** thư được lưu trong `Backend/.local/mail/*.json`, ngoài wwwroot và bị ignore. Mở trường `LienKet` để chạy deep link trên thiết bị test. Không có endpoint HTTP đọc outbox và API quên mật khẩu không trả link/token. Giữ các file này riêng tư vì có token một lần; không đưa vào Git hoặc báo cáo.

**Gửi thư thật:** cấu hình `Smtp:Host`, `Port`, `EnableSsl`, `Username`, `Password`, `From` bằng env/user-secrets. Không lưu credential trong repo. Production còn yêu cầu `Recovery:BaseUrl` là HTTPS; cần domain và verified App Links tương ứng trong Android. Khi chưa có cấu hình, không tuyên bố đã gửi email tới hộp thư thật. Phiên này kiểm tra end-to-end bằng outbox Development, chưa gửi SMTP ra ngoài.

Deep link Development: `remarket://tai-khoan/dat-lai-mat-khau?email=...&token=...`. Android chỉ nhận đúng scheme/host/path, giữ token trong ViewModel (không ghi DataStore/SavedState hoặc log). Màn đặt lại mật khẩu là bổ sung cần thiết cho liên kết khôi phục; Figma có form đổi mật khẩu nhưng chưa có trạng thái nhập password từ email.

## Android và Figma

- Compose/MVVM/StateFlow/Retrofit trên module app hiện có; domain model/repository contract không phụ thuộc Retrofit.
- DataStore lưu token **đã mã hóa AES-GCM bằng Android Keystore**, nằm trong noBackupFilesDir; không lưu mật khẩu. Kiểm tra phiên thật qua `/toi` trước khi vào trang bảo vệ sau khởi động.
- OkHttp chỉ gửi bearer tới đúng origin của backend. Refresh dùng client/dispatcher riêng và mutex, retry một lần. DataStore kiểm tra refresh hiện hành để phản hồi trễ không khôi phục phiên đã logout hoặc ghi đè đăng nhập mới.
- Sau logout/thu hồi phiên, state chuyển về đăng nhập, xóa hồ sơ/role và không cho Back mở lại trang đã đăng nhập. Nếu không gửi được logout tới server do mạng, app báo lỗi và giữ phiên để thử lại.
- Màu `#28734F`, nền `#FCFDFB`, text `#202E26/#65746A`; Roboto đóng gói, app bar 56dp, lề 24dp, input 48dp/radius12, CTA 50dp/pill, card radius20. System bars thật thay status bar vẽ sẵn.
- Ảnh Splash lấy nguyên từ image hash của node `2:2971` trong Nhom4.fig, không dùng screenshot nguyên màn. Icon nét Lucide cục bộ theo tên layer, Roboto và giấy phép trong `docs/assets`. Avatar là dữ liệu người dùng thật, không dùng chân dung Nguyễn Minh Anh của mockup cho mọi tài khoản.
- MCP vẫn hết quota Starter trong phiên này, nên dựng từ snapshot `.fig` đã giải mã và thông số phân tích; không khẳng định khớp pixel tuyệt đối với bản trực tuyến chưa chụp lại.
- Các hàng Settings ngoài phạm vi (đơn/trao đổi/hỗ trợ, xác minh liên hệ, privacy/delete) chưa kích hoạt; không có nút giả báo thành công. Chỉ trang đích Home/Admin và các chức năng tài khoản được triển khai ở bước này.

## Kiểm tra có thể chạy lại

```powershell
cd Backend
. .\scripts\Use-DotNet8.ps1
dotnet run --project tests/ReMarket.DatabaseChecks -- --auth
dotnet run --project tests/ReMarket.DatabaseChecks
```

`--auth` tạo SQL DB test ngẫu nhiên, áp mọi migration, gọi HTTP qua ASP.NET pipeline và dọn đúng DB/file test khi kết thúc. Không thay seed demo, không dùng fake repository hoặc bỏ qua password verification. Kiểm tra registration/login/logout, chữ ký/issuer/audience/hạn token, refresh/replay, profile/concurrency/avatar, reset và quyền USER/ADMIN.

Kiểm tra Android dùng `TaiKhoanEndToEndTest` trên emulator với API đang chạy. Tài khoản USER được đăng ký mới qua UI/API; test ADMIN nhận thông tin tài khoản đã được operator cấp quyền qua instrumentation args, không nằm trong mã app. Các tài khoản test tạo mới được giữ trong ReMarketDemo; không tự xóa tài khoản đang sử dụng.

Sau khi cài APK app và APK androidTest lên emulator, chạy bằng PowerShell (hai biến `$adminEmail`/`$adminPassword` lấy từ tài khoản test đã đăng ký/cấp quyền, không ghi giá trị vào repo):

```powershell
cd Android
.\gradlew.bat :app:assembleDebug :app:assembleDebugAndroidTest
adb install -r app/build/outputs/apk/debug/app-debug.apk
adb install -r app/build/outputs/apk/androidTest/debug/app-debug-androidTest.apk
adb shell am instrument -w -r -e class com.ute.app_ban_do_cu.TaiKhoanEndToEndTest -e adminEmail $adminEmail -e adminPassword $adminPassword com.ute.app_ban_do_cu.test/androidx.test.runner.AndroidJUnitRunner
```

Không log/chia sẻ lệnh đã mở rộng credential. Nếu thiếu args, bài kiểm tra ADMIN bị skip; phải cung cấp để xác minh đủ cả hai vai trò. Kết quả và ảnh thực tế: [ACCOUNTS_VALIDATION.md](ACCOUNTS_VALIDATION.md).
