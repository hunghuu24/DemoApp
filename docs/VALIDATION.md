# Kết quả xác minh bộ khung ReMarket

> Đây là báo cáo giai đoạn nền tảng. Kết quả mới sau khi triển khai tài khoản, migration thứ ba và kiểm thử Android nằm trong [ACCOUNTS_VALIDATION.md](ACCOUNTS_VALIDATION.md).

Ngày kiểm tra: **07/11/2026**, Asia/Saigon. Kiểm tra trên Windows, SQL Server 2022 Developer (local), .NET SDK 8.0.427, EF Core 8.0.33 và Android SDK 37. Không suy từ build rằng toàn bộ chức năng Figma đã hoạt động.

## Build và schema

| Kiểm tra | Kết quả |
| --- | --- |
| Backend Debug build | Thành công, 0 warning / 0 error |
| Backend Release build, cả project DatabaseChecks | Thành công, 0 warning / 0 error |
| EF model so với migration snapshot | Không có thay đổi chưa migration |
| EF database update trên ReMarketDemo | Đã áp dụng đủ hai migration |
| Số đối tượng SQL thực | 43 bảng nghiệp vụ, 80 FK, 52 CHECK, 17 trigger |
| SQL idempotent trên DB mới tách biệt | Chạy hai lần thành công, seed và kiểm tra thành công; DB tạm đã được xóa |
| Seed demo | Có dữ liệu ở đủ 43 bảng; chạy lại không tăng số dòng ở bất kỳ bảng nào |
| Android assembleDebug | Thành công, có APK debug |
| Android testDebugUnitTest | Thành công; test template, không đại diện kiểm thử nghiệp vụ |
| Android lintDebug | Thành công, **0 lỗi / 6 cảnh báo phiên bản dependency**; không tạo baseline hoặc tắt kiểm tra để bỏ lỗi |

Các cảnh báo phiên bản Android liên quan AGP, Kotlin Compose plugin, Compose BOM, Activity Compose và hai artifact Lifecycle. Phiên bản đang pin đã build với nhau; chưa tự nâng toàn bộ toolchain sang bản mới chỉ để hết cảnh báo. Có thông báo đóng gói nguyên `libandroidx.graphics.path.so` khi không strip được; không chặn APK debug.

APK: [app-debug.apk](../Android/app/build/outputs/apk/debug/app-debug.apk), khoảng 11,47 MB. Báo cáo lint: [lint-results-debug.html](../Android/app/build/reports/lint-results-debug.html). Các file build/report bị ignore và có thể tạo lại bằng lệnh trong README.

## 20 kiểm tra dữ liệu trên SQL Server thật

Chạy bằng [ReMarket.DatabaseChecks](../Backend/tests/ReMarket.DatabaseChecks/Program.cs), đạt **20/20**, trên cả ReMarketDemo và DB mới sinh từ script SQL:

1. Model khớp migration snapshot.
2. Đủ dữ liệu trong 43 bảng.
3. Seeder lặp không thêm hoặc đổi số dòng.
4. Email chuẩn hóa không trùng hoa/thường/khoảng trắng.
5. Chặn tiền bù âm.
6. Chặn tự trao đổi với chính mình.
7. Chặn sender không thuộc hội thoại.
8. Chặn đánh giá sao ngoài 1–5.
9. Chặn hoàn tất Smart Swap khi mới 5/6.
10. Chặn thu hồi consent của bản đã chốt.
11. Chặn đổi số tiền bù tại chỗ sau chốt.
12. Chặn receipt sai món/cạnh trao đổi.
13. Chặn đánh giá khi giao dịch chưa hoàn tất.
14. Chặn hai reservation còn giữ cho cùng một món.
15. Chặn payee tiền bù không thuộc giao dịch.
16. Chặn đóng khiếu nại khi quyết định chưa được xác minh thực hiện.
17. Bàn giao cần bản hiện tại có đủ 3/3 consent.
18. Hoàn tất 1–1 cần đủ hai lượt nhận.
19. Thêm xác nhận thứ sáu đúng chiều, hoàn tất vòng/root và tạo được hai review cho hai đối tác.
20. Hai DbContext ghi cùng hồ sơ: rowversion chặn ghi đè từ bản cũ.

Các thao tác ghi của checks được rollback; SS-0301 trong ReMarketDemo vẫn ở 5/6 để dùng trình diễn. Những kiểm tra này tập trung vào các ràng buộc quan trọng, không khẳng định bao phủ mọi state transition/quyền hoặc cạnh tranh đồng thời ở service chưa viết.

## HTTP smoke check

Đã chạy host Development tại `http://localhost:5080`, xác minh:

- `/api/hethong/trangthai`: HTTP 200, `tenUngDung=ReMarket`, `coSoDuLieuSanSang=true`, JSON camelCase khớp DTO Android.
- `/health/live`: HTTP 200.
- `/health/ready`: HTTP 200.
- `/swagger/v1/swagger.json`: HTTP 200.
- `/demo/san-pham.png`: HTTP 200, `image/png`.

Host dùng để kiểm tra đã được dừng sau khi xác minh. Khi demo cần chạy lại theo README. Chưa chạy app trên thiết bị/emulator, chưa kiểm thử Android → host qua mạng thực và chưa có test UI đối chiếu 83 frame.

## Phạm vi chưa thực hiện

Không có CRUD hoàn chỉnh, JWT/login API, payment gateway, thuật toán Smart Swap, realtime chat, upload thật, cron vận chuyển/hết hạn hay giao diện User/Admin đầy đủ. Role, model, FK, trạng thái và dữ liệu demo chuẩn bị cho các bước đó; chưa thể dùng bộ khung này để tuyên bố sản phẩm đã hoàn thành nghiệp vụ.
