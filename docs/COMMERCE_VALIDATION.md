# Xác minh mua bán ReMarket — 10/11/2026

Kiểm tra trên project hiện có, ASP.NET Core8/EF Core8.0.33 và SQL Server2022 local; Android Compose/Retrofit/Coil trên emulator QA riêng 360×800, density160. Không dùng mock repository hoặc tài khoản đặc biệt trong ứng dụng. [Hướng dẫn/API và quy tắc](COMMERCE.md).

## Backend và CSDL

- Debug và Release build thành công; Release **0 lỗi, 0 cảnh báo**.
- **119 kiểm tra mua bán HTTP/SQL qua ASP.NET TestServer đạt**, real JWT và SQL Server trên database mới ngẫu nhiên; tự xóa đúng DB/ảnh tạm sau chạy. [Log](qa/commerce/backend-checks.txt).
- Hồi quy **94 kiểm tra catalog** và **54 kiểm tra tài khoản** đạt trên model mới. **20 kiểm tra nền tảng SQL** đạt trong Verify-FreshDatabase; các mutation kiểm tra được rollback. Tổng287 assertion/check Backend trong các nhóm này, không phải287 test UI.
- Sáu migration đã áp dụng trên ReMarketDemo; **47 bảng, 90 FK, 62 CHECK, 17 trigger**, không tính bảng migration history. `has-pending-model-changes`: không có thay đổi chưa tạo migration.
- Script SQL idempotent được xuất lại từ6 migrations, áp dụng **hai lần** trên DB trống riêng, seed và chạy20 checks thành công. [Log DB mới](qa/commerce/fresh-database.txt).
- [Từ điển JSON](database/schema-dictionary.json) xuất từ EF design-time model:47 entity; [SQL triển khai](database/remarket-schema.sql) và [thiết kế CSDL](database/THIET_KE_CSDL.md) đã cập nhật.

Các nhóm mua bán được xác minh:

| Nhóm | Các tình huống |
| --- | --- |
| Xác thực/quyền | Anonymous401; chủ tin không mua chính món của mình; người ngoài không đọc đơn/địa chỉ/ảnh/chat/ticket; người bán không đọc ticket riêng của người mua; USER không giả ADMIN hoặc thao tác người bán/người mua |
| Chat | Mở lại dùng cùng hội thoại; nội dung thật và ảnh JPEG được lưu; chỉ thành viên truy cập; gửi lại cùng GUID không trùng, đổi nội dung cùng GUID bị409; text rỗng bị400; unread dùng sequence/watermark |
| Thương lượng | Khoản giảm không thể bằng/to hơn giá; người mua không tự chấp nhận; chưa chấp nhận không áp dụng; từ chối/gửi mới/chấp nhận; ràng buộc đúng người mua và món; thứ tự theo message sequence khi cùng giây |
| Báo giá/tạo đơn | Server tính giá gốc/giảm/phí/tổng; từ chối phương thức ngân hàng thật và điện thoại sai; snapshot/participant/payment/ship/hold có trong SQL; gửi lại không trùng đơn; thay nội dung cùng key409 |
| Tồn kho | Tin giữ chỗ bị rút khỏi công khai, ảnh riêng đơn vẫn đọc được; không xác nhận bán bên ngoài khi đang giữ; hai buyer đồng thời chỉ một201 và một409; hủy/hết hạn mở bán lại và giải phóng unique hold |
| Thanh toán/ship | Thất bại lưu và cho thử lại; gửi lại kết quả không nhân giao dịch; thành công lưu khoản giữ mô phỏng; người bán chuyển đúng bước; không bỏ bước; giao thất bại/giao lại có đủ lịch sử đúng thứ tự |
| Nhận/đánh giá | Không nhận trước thanh toán/đã giao, không hủy sau bàn giao; chỉ buyer nhận; nhận lại không trùng; giải ngân/sold/completed SQL đúng thứ tự trigger; sản phẩm/người bán có điểm riêng, chặn đánh giá sớm/trùng/tự đánh giá |
| Hỗ trợ | Gắn đơn phải là participant; tạo lại cùng key không trùng; owner bổ sung/ADMIN tiếp nhận/giải quyết; không sửa ticket đóng; lịch sử cùng giây có sequence; phản hồi trước migration vẫn đọc được, không dựng trạng thái lịch sử chưa có |
| Hủy/hết hạn | Hủy chưa trả, hủy đã trả trước bàn giao với refund mô phỏng; hết hạn pending được lưu hủy và nhả giữ chỗ, sản phẩm công khai lại |

Lượt chạy bổ sung đã phát hiện thứ tự phiếu không ổn định khi `NgayTao` cùng giây. API đã chuyển sang thứ tự `TinNhan.SoThuTu`; bộ119 checks sau sửa đạt. Event ship/support cũng dùng identity để giữ thứ tự cùng giây.

## Android

`assembleDebug`, `assembleDebugAndroidTest`, `testDebugUnitTest` và `lintDebug` thành công. [Log build](qa/commerce/android-build.txt). Lint **0 lỗi, 14 cảnh báo**:7 gợi ý phiên bản dependency và7 đường vector dài (5 catalog,2 icon chat trích từ Figma). Chưa đổi phiên bản nền tảng hoặc làm mất hình học vector để che cảnh báo.

Instrumentation gọi Retrofit/Coil/API thật và lưu SQL Server:

Lượt cuối trên APK hiện tại: **OK (9 tests)**, **182,176 giây**, không skip. [Log instrumentation](qa/commerce/instrumentation-result.txt). Đã kiểm tra ảnh render ở360×800 và form800×1000, trả emulator về360×800 sau test.

| Bài | Luồng |
| --- | --- |
| `nguoiMuaThuongLuongMuaNhanDanhGiaVaHoTro` | Tìm sản phẩm → chi tiết/liên hệ → tin/ảnh → đề nghị giảm → seller thật chấp nhận qua API → áp dụng/bỏ/áp lại phiếu → validation/checkout → pay thất bại/thành công → seller chuyển ship qua API → buyer nhận → hai điểm đánh giá → hỗ trợ/bổ sung → ADMIN giải quyết → xem lại đơn hoàn tất |
| `nguoiBanChapNhanGiaVaMoPhongVanChuyen` | Seller đăng nhập thật → Inbox/chat → chấp nhận phiếu → Đơn bán → liên hệ từ đơn → bàn giao/đang vận chuyển/thất bại/giao lại/đã giao; seller không có nút nhận thay buyer |
| `muaGiaGocHuyDonHoanTienVaHoTroChonDon` | Đơn rỗng → Mua ngay không phiếu → form rộng800×1000 → validation SĐT → dropdown mô phỏng → hủy pending → mua lại/trả/hủy/refund → hỗ trợ từ Cá nhân, chọn đơn liên quan và thấy ticket khi quay về lịch sử |
| `quanTriTiepNhanVaGiaiQuyetHoTro` | ADMIN thật → danh sách ticket → tiếp nhận và nhập phản hồi → giải quyết → quay lại dashboard |
| Hai bài `SanPhamEndToEndTest` | Hồi quy đăng/sửa/ẩn/đã bán/xóa, ảnh, duyệt, tìm kiếm/lọc/phân trang/danh mục và bottom navigation |
| Ba bài `TaiKhoanEndToEndTest` | Hồi quy đăng ký/đăng nhập/hồ sơ/logout, ADMIN, DataStore và khởi động lại |

Các tài khoản buyer/seller được đăng ký mới qua API; credential ADMIN được tạo/cấp quyền thật cho QA và truyền từ file tạm ngoài repository. Mật khẩu/JWT không đưa vào log/báo cáo. Chỉ callback kết quả Photo Picker được cung cấp PNG kiểm thử; upload/sanitization/ảnh riêng/Coil là API thật. Những ảnh màu xanh trong sản phẩm/tin nhắn là PNG kiểm thử, không phải dữ liệu cố định trong app.

Các lượt emulator khởi động lạnh có hộp thoại System UI ANR che app; đã xử lý môi trường/onboarding, tắt autofill của AVD QA và chạy lại trên cùng APK. Không thay app để bỏ qua lỗi quyền/đăng nhập. Không reset AVD hoặc dữ liệu người dùng khác. Lượt cuối9 bài không lỗi. Sau QA đã thu hồi phiên và role ADMIN của đúng tài khoản quản trị tạm, xóa file credential ngoài repo và dừng đúng API/emulator do tác vụ này khởi chạy; giữ lịch sử giao dịch trong database.

Ảnh thực tế từ21 lần chụp trong bộ test:

- [Chat đã chốt giá](qa/commerce/chat-discount-accepted.png), [form phiếu](qa/commerce/coupon-form.png), [inbox người bán](qa/commerce/inbox-seller.png).
- [Checkout đầu form](qa/commerce/checkout-top.png), [giảm giá/tổng tiền](qa/commerce/checkout-discount.png), [viewport800×1000](qa/commerce/checkout-wide.png).
- [Đơn chờ trả](qa/commerce/order-pending.png), [đã trả](qa/commerce/order-paid.png), [đã giao/chờ nhận](qa/commerce/order-delivered.png), [hoàn tất](qa/commerce/order-completed.png), [đơn bán](qa/commerce/orders-seller.png), [đơn rỗng](qa/commerce/orders-empty.png), [seller giao mô phỏng](qa/commerce/seller-shipped.png).
- [Hủy pending](qa/commerce/order-cancelled-unpaid.png), [hủy/refund](qa/commerce/order-refunded.png), [đánh giá](qa/commerce/rating-form.png), [đánh giá thành công](qa/commerce/rating-success.png).
- [Gửi hỗ trợ](qa/commerce/support-form.png), [đã giải quyết](qa/commerce/support-resolved.png), [hàng chờ Admin](qa/commerce/admin-support-list.png), [Admin đang xử lý](qa/commerce/admin-support-processing.png).

## Bằng chứng thiết kế và phạm vi

Đã đọc context/ảnh checkout `194:72` qua Figma MCP; các đọc tiếp chạm giới hạn Starter. Những frame khác đối chiếu dữ liệu text/layout/vector từ file `.fig` đã giải mã, không khẳng định kiểm tra pixel toàn bộ file online. Roboto, màu, card/form/bottom bar và icon dựa trên nguồn đã đọc; phần mô phỏng/validation/hủy là bổ sung nghiệp vụ có ghi rõ. [Chi tiết nguồn](COMMERCE.md).

Thanh toán/vận chuyển không gọi dịch vụ ngoài; mọi mốc hiển thị từ SQL và thao tác mô phỏng thật. Chưa kiểm tra thiết bị vật lý, mọi font scale/API Android, tải lớn/soak test hoặc WebSocket/FCM. Không có chuyển tiền ngân hàng/đơn vận chuyển thật để kiểm tra.

## Chạy lại

```powershell
cd Backend
. .\scripts\Use-DotNet8.ps1
dotnet run --project tests/ReMarket.DatabaseChecks -- --commerce
dotnet run --project tests/ReMarket.DatabaseChecks -- --listings
dotnet run --project tests/ReMarket.DatabaseChecks -- --auth
.\scripts\Verify-FreshDatabase.ps1
```

Build app/test APK trong `Android/`. Khởi động API với database demo và đủ migration; emulator QA truy cập `10.0.2.2:5080`. Chạy AndroidJUnitRunner với ba class `MuaBanEndToEndTest,SanPhamEndToEndTest,TaiKhoanEndToEndTest` trong package `com.ute.app_ban_do_cu`, truyền `adminEmail`/`adminPassword` từ credential operator cấp. Không ghi credential vào file dự án. Khi chạy cả bộ nhanh, host QA riêng có thể đặt `RateLimits__PermitLimit=1000`; host mặc định vẫn giữ giới hạn thật.
