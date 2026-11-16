# Kết nối local ReMarket — 16/11/2026

Đã gắn bước chuẩn bị API vào Debug build/Run, thêm tiến trình nền tự nối lại điện thoại đã cấp phép qua mDNS, phục hồi cổng 5080 và khởi động lại Backend. URL Android tiếp tục là `http://127.0.0.1:5080/`; SQL, schema, dữ liệu, JWT signing key và nghiệp vụ được giữ nguyên.

Cách chạy: điện thoại cùng Wi-Fi với PC, bật Wireless debugging và ghép đôi trong Android Studio; chọn điện thoại rồi Run. Nếu chỉ mở APK đã cài, nhấp đúp [Start-ReMarket.cmd](../Start-ReMarket.cmd) trên PC. [Hướng dẫn đầy đủ](../README.md).

**18/18 kiểm tra kết nối đạt trên điện thoại V2452A thật**, gồm:

- Debug build khi Backend/worker đang tắt tự chuẩn bị lại API; JWT trước lúc dừng vẫn dùng được sau khi khởi động lại.
- Lượt build dùng configuration cache/APK up-to-date vẫn chạy bước chuẩn bị.
- Ba lần mất forwarding tự khôi phục trong 2,73 / 3,89 / 2,93 giây.
- Ngắt rồi kết nối lại ADB không dây tự phục hồi API trong 4,02 giây.
- Dừng Backend do helper tạo: tự khởi động lại và SQL sẵn sàng sau 10,34 giây.
- Điện thoại gọi API trạng thái/SQL và trang chủ thật; đăng nhập Admin, truy cập báo cáo SQL và thu hồi JWT kiểm thử đều đạt.

[Kết quả từng kiểm tra](qa/local-connectivity/result.json) · [Log kiểm thử](qa/local-connectivity/recovery-tests.txt) · [Build khởi động mới](qa/local-connectivity/android-cold-start-build.txt) · [Build tiếp theo](qa/local-connectivity/android-warm-start-build.txt).

Kiểm tra bổ sung: PowerShell parse thành công; .NET 8.0.427 được tìm thấy ngay cả khi `dotnet` không có trên PATH; dịch vụ MSSQLSERVER hiện là Automatic. Không cài lại APK hay thao tác xác nhận giao dịch trên điện thoại.

Các lượt thử đầu đã phát hiện và xử lý việc pipe tiến trình nền giữ Gradle chờ, ADB không tự nối lại transport đã ngắt, và cách đọc/lưu mảng JSON trong Windows PowerShell 5.1. Log thất bại ban đầu được giữ cạnh log cuối.

**Chưa khởi động lại Windows thực tế.** Đã mô phỏng khởi động mới bằng cách dừng Backend và worker rồi chạy Debug build, cùng kiểm tra ngắt/nối lại transport của điện thoại thật. Wireless debugging, ghép đôi và mạng Wi-Fi cần hoạt động; phần mềm không tự bật Wi-Fi hoặc cấp phép ghép đôi thay người dùng.
