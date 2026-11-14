# ReMarket — Xác minh chức năng tài khoản

> Báo cáo giai đoạn tài khoản ngày 07/11. Kết quả kiểm tra tiếp theo với catalog và migration thứ tư nằm ở [CATALOG_VALIDATION.md](CATALOG_VALIDATION.md).

Ngày kiểm tra: 07/11/2026 (Asia/Saigon). Tiếp tục trên solution/module hiện có. Backend dùng .NET SDK 8.0.427, EF Core 8.0.33, SQL Server 2022; Android Emulator API 36, viewport 360×800, ứng dụng compile/target SDK 37. Các phép thử gọi API và SQL thật.

## Backend và cơ sở dữ liệu

| Kiểm tra | Kết quả |
| --- | --- |
| Build solution Debug và Release | Thành công, 0 warning / 0 error |
| HTTP/SQL tài khoản `--auth`, Release | **54 passed**, không skip; DB mới tách biệt, áp migration thật |
| Kiểm tra CSDL nghiệp vụ hiện có | **20 passed / 0 failed**, model khớp migration snapshot |
| SQL idempotent xuất lại từ đủ 3 migration | Áp hai lần trên DB mới, seed và 20 kiểm tra thành công; dọn DB tạm |
| ReMarketDemo sau migration | **45 bảng, 82 FK, 54 CHECK, 17 trigger**; đủ 3 migration |
| NuGet audit API, kể cả transitive | Không phát hiện dependency có lỗ hổng đã biết từ nguồn NuGet cấu hình tại lúc kiểm tra |

54 kiểm tra xác minh: password yếu/không đúng; đăng ký thật và hash PBKDF2 có salt; email trùng sau chuẩn hóa; payload tự thêm ADMIN; hồ sơ lấy từ subject JWT; chỉnh sửa SQL và xung đột rowversion; ảnh đại diện được decode/encode và truy cập có xác thực; từ chối JWT sửa chữ ký, sai issuer/audience, hết hạn; refresh rotation/replay; mã reset sai/quá hạn/dùng lại; reset và đổi mật khẩu thu hồi phiên; logout từ chối access/refresh cũ, giữ phiên thiết bị khác; USER nhận 403 ở API ADMIN; role SQL bị gỡ hoặc tài khoản bị khóa có hiệu lực ngay.

HTTP test host dùng pipeline ASP.NET/authorization/model binding thật, chỉ thay khóa ký, DB test, thư mục file/outbox và giới hạn tần suất dành cho test. Không fake tài khoản/repository/password verification. Các mutation kiểm tra nghiệp vụ được rollback; DB tài khoản tạm được xóa sau test.

## Android kết nối API

| Kiểm tra | Kết quả |
| --- | --- |
| assembleDebug + assembleDebugAndroidTest | Thành công; APK app và test được tạo |
| testDebugUnitTest | Thành công; 1 test template, không dùng làm bằng chứng xác thực nghiệp vụ |
| lintDebug | 0 lỗi; 7 cảnh báo phiên bản dependency/toolchain, không thêm baseline hoặc tắt rule |
| UI đăng ký → đăng nhập USER → hồ sơ → sửa tên/địa chỉ → logout | Passed; token trước logout nhận 401 từ API, DataStore đã xóa |
| UI đăng nhập ADMIN → trang tổng quan → cài đặt → logout | Passed; ADMIN được đăng ký thật rồi cấp role qua operator CLI, không skip |
| UI đăng nhập → tạo lại Activity → khôi phục phiên | Passed |
| Buộc dừng tiến trình rồi mở lại app | `LaunchState: COLD`; DataStore/Keystore khôi phục, gọi `/toi`, vào Home với dữ liệu SQL |
| Deep link đặt lại mật khẩu từ Development outbox | Mở đúng màn, nhập/xác nhận mật khẩu mới, quay về login, đăng nhập ADMIN bằng mật khẩu mới thành công |

Ba bài instrumentation trả **OK (3 tests)** trong 17,006 giây: [log kết quả](qa/accounts/instrumentation-result.txt). USER được tạo mới bằng UI/API; ADMIN dùng credential ngẫu nhiên được truyền khi chạy test. Không lưu password/JWT/link khôi phục trong báo cáo hoặc APK. Các tài khoản QA tạo trong ReMarketDemo vẫn được giữ để không xóa dữ liệu demo đang dùng.

Emulator QA chạy độc lập bằng AVD tạm. Lần chạy đầu gặp hộp thoại SystemUI ANR do áp lực RAM của máy; sau giải phóng build daemon và khởi động lại SystemUI, chạy đủ ba test thành công. Không thay dữ liệu AVD hiện có của người dùng. Emulator và API phục vụ kiểm tra đã được dừng sau khi xong.

## Ảnh đối chiếu và giới hạn

Ảnh app chạy thực tế ở 360×800: [Splash](qa/accounts/splash.png), [đăng nhập](qa/accounts/login.png), [đăng ký](qa/accounts/register.png), [quên mật khẩu](qa/accounts/forgot-password.png), [đặt lại mật khẩu](qa/accounts/reset-password.png), [hồ sơ](qa/accounts/profile.png), [chỉnh sửa](qa/accounts/edit-profile.png), [cài đặt](qa/accounts/settings.png), [cài đặt cuộn](qa/accounts/settings-scrolled.png), [Home](qa/accounts/home.png), [Admin](qa/accounts/admin.png).

MCP Figma hết quota Starter; giao diện dựa trên thông tin node/text/geometry/color từ `.fig` đã giải mã, dùng asset Splash gốc và Roboto đóng gói. Chưa có screenshot Figma trực tuyến mới để đo sai lệch pixel. Icon Lucide chuyển thành vector theo tên layer.

Khôi phục mật khẩu đã kiểm tra bằng outbox Development và liên kết vào Android; **chưa gửi email SMTP thật**. Production cần credential SMTP, HTTPS API, domain/App Links cho Recovery:BaseUrl và cấu hình khóa ký riêng. Các mục Settings ngoài phạm vi tài khoản chưa kích hoạt; Home/Admin hiện là trang đích đọc dữ liệu thật, chưa triển khai toàn bộ nghiệp vụ giao dịch/quản trị.
