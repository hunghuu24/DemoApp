# ReMarket — Kết quả xác minh sản phẩm và tin đăng

Ngày hoàn tất kiểm tra: **09/11/2026**, Asia/Saigon. Giữ solution/module đang có; không tạo lại dự án hoặc reset ReMarketDemo. Môi trường: Windows, .NET SDK8.0.427, EF8.0.33, SQL Server2022; Android Emulator API36, 360×800/density160, app compile/target SDK37.

## Build và migration

| Kiểm tra | Kết quả thực tế |
| --- | --- |
| Backend Release build | Thành công, 0 warning / 0 error |
| Backend Debug build khi export SQL | Thành công |
| Migration trên ReMarketDemo | Đủ bốn migration; migration mới `20261108165930_TinDangMuaBan` |
| Schema SQL thực | 45 bảng nghiệp vụ, 82 FK, 55 CHECK, 17 trigger |
| Model so với EF snapshot | Khớp, được kiểm tra trong DatabaseChecks |
| SQL idempotent từ cả bốn migration | Chạy hai lần trên một DB mới riêng, seed và 20 kiểm tra SQL thành công; đã dọn DB tạm |
| Android assembleDebug / assembleDebugAndroidTest | Thành công; app APK khoảng14,9MB |
| Android testDebugUnitTest | Thành công, 1 test template; không dùng làm bằng chứng nghiệp vụ |
| Android lintDebug | **0 lỗi / 12 cảnh báo**: 7 phiên bản dependency/toolchain; 5 đường vector dài từ icon Figma gốc |

Giữ hình học vector gốc thay vì rút gọn hình dạng để hết cảnh báo. Không thêm lint baseline hoặc tắt rule. APK: [app-debug.apk](../Android/app/build/outputs/apk/debug/app-debug.apk). Báo cáo lint: [HTML](../Android/app/build/reports/lint-results-debug.html).

## HTTP và SQL Server thật

**94 kiểm tra tin đăng đạt**, chạy `DatabaseChecks --listings` ở Release trên DB SQL Server tạm mới. Không fake repository/SQL, không bỏ qua JWT/password verification. Các nhóm kiểm tra:

- Đăng ký/đăng nhập các tài khoản khác nhau; API chủ tin dựa vào subject JWT. Người khác không đọc preview, sửa, upload hoặc xóa tin; USER không duyệt, ADMIN không tự duyệt tin của mình.
- Validation giá, mô tả, tình trạng và danh mục bị ẩn; danh mục cha ẩn kéo theo nhánh con.
- Tạo đồng thời sản phẩm + nháp; nháp/chờ duyệt không vào search/detail công khai. Yêu cầu gửi chưa có ảnh bìa bị từ chối.
- Upload multipart thật, giả MIME/nội dung bị từ chối; ảnh encode JPEG, đúng tỷ lệ, tên GUID không dùng filename `../../` của client. Ảnh nháp chỉ chủ/Admin xem. Bearer ảnh không hợp lệ trả401 để client có thể refresh.
- Thay bìa qua unique SQL index, đúng một bìa; xóa bìa tự gán lại; giới hạn8 ảnh và từ chối ảnh thứ9. Upload phiên bản cũ bị từ chối và không để lại file mới.
- Hai lệnh sửa đồng thời cùng rowversion: một thành công, một409; không ghi đè nhau.
- Gửi duyệt, Admin từ chối có lý do, gửi lại và duyệt thật; công khai ảnh/tin sau duyệt. Chỉnh tin đang bán rút lại về chờ duyệt.
- Tìm kiếm không dấu, giá/tình trạng/khu vực, nhánh danh mục; sort giá tăng/giảm; hai trang trên nhiều bản ghi, tổng số đúng, không trùng dòng giữa trang, trang vượt cuối rỗng; query sai trả400.
- Home, số lượng danh mục và tabs Tin của tôi lấy từ SQL; không lẫn chủ tin.
- Ẩn/gửi duyệt lại/đã bán/xóa mềm tác động ngay tới khả năng xem tin/ảnh. Giữ bản ghi quan hệ sau xóa; chặn sửa/bán món đang giữ chỗ và sửa nội dung tin đã bán.

Kiểm tra hồi quy: **54 kiểm tra tài khoản đạt** và **20 kiểm tra CSDL nền tảng đạt**. Kiểm tra nghiệp vụ rollback các thay đổi thử; bộ kiểm tra tin đăng/tài khoản tạo và dọn đúng DB `ReMarketTest_GUID` cùng thư mục file riêng, không xóa DB demo.

## Android end-to-end

Lần chạy cuối trên APK hiện tại: **OK (5 tests)**, không skip, **56,022 giây**. [Log instrumentation](qa/catalog/instrumentation-result.txt).

| Bài kiểm tra | Phần đã xác minh |
| --- | --- |
| `dangBanSuaAnDaBanVaXoa` | Login thật → form validation → tạo/upload → preview → gửi thành công/chờ duyệt → Admin duyệt qua API → Tin của tôi → sửa → duyệt lại → ẩn → gửi lại → đã bán → xóa; đối chiếu HTTP/SQL sau các bước |
| `timKiemBoLocPhanTrangDanhMucVaDieuHuong` | Tạo7 tin kiểm thử qua API thật → search → sort giá → trang2 → lọc giá còn2 → detail/Coil giải mã ảnh → empty → danh mục/list → các mục Bottom Navigation → Tin của tôi |
| `aDangKyDangNhapHoSoDangXuat` | Hồi quy đăng ký, USER Home, hồ sơ/sửa, logout; token cũ bị server trả401 |
| `bAdminVaoTrangQuanTri` | Hồi quy ADMIN vào trang quản trị/cài đặt và logout, credential operator cấp thật |
| `zLuuPhienDataStoreVaKhoiDongLai` | Hồi quy lưu phiên và tạo lại Activity, xác thực lại trước Home |

Ảnh trong test được đưa vào callback ViewModel tương ứng kết quả picker; phần upload/Coil/Retrofit/API/SQL vẫn chạy thật. Đã kiểm tra riêng **Photo Picker hệ thống**: mở thư viện, chọn ảnh JPG, Done, form hiển thị thumbnail và `1 / 8 ảnh`/Ảnh bìa: [ảnh bằng chứng](qa/catalog/native-photo-picker.png).

Đã kiểm tra thủ công **lỗi mạng và thử lại**: dừng đúng API dùng cho QA khi app đang ở Home, bấm tải lại thấy lỗi kết nối; khởi động API và bấm Thử lại trở về dữ liệu/ảnh: [lỗi](qa/catalog/network-error.png), [khôi phục](qa/catalog/network-recovered.png).

Đã kiểm tra viewport **800×1000** trên cùng emulator rồi trả lại360×800: Home không tràn ngang, danh mục chuyển thành3 cột và giữ nội dung thật: [Home rộng](qa/catalog/home-wide.png), [danh mục rộng](qa/catalog/categories-wide.png). Form dài/IME/cuộn được thao tác trong instrumentation. Chưa chạy trên thiết bị vật lý hoặc mọi mức font scale/Android API được hỗ trợ.

Đã mở trang kiểm duyệt trên Android bằng tài khoản ADMIN thật; hàng chờ lấy tin/mô tả/người đăng từ SQL và quay lại dashboard được: [ảnh trang duyệt](qa/catalog/admin-review.png). Không đổi trạng thái tin fixture của người dùng khác trong kiểm tra giao diện này; các transition duyệt/từ chối được kiểm tra qua API ở trên.

Test dùng credential ngẫu nhiên, không nhúng trong APK và không in JWT/password/link khôi phục ra log. Các tin do bài test mới tạo được xóa mềm qua API bằng đúng chủ tin và ID vừa tạo; không xóa dữ liệu người dùng khác. Tài khoản QA và bản ghi/file đã xóa mềm vẫn được giữ theo chính sách lịch sử hiện tại. API QA nâng giới hạn tần suất chỉ trong tiến trình kiểm thử để chạy nhiều bước liên tục; appsettings mặc định giữ nguyên.

Emulator QA và API phục vụ kiểm tra đã được dừng; file credential tạm đã xóa. AVD riêng của QA, không thay thế hoặc reset AVD cá nhân hiện có.

## Ảnh ứng dụng và đối chiếu thiết kế

Ảnh 360×800 của app chạy thực tế:

- [Home](qa/catalog/home.png), [danh mục](qa/catalog/categories.png), [danh sách](qa/catalog/category-list.png).
- [Tìm kiếm/phân trang](qa/catalog/search-pagination.png), [bộ lọc](qa/catalog/search-filter.png), [empty](qa/catalog/search-empty.png), [chi tiết](qa/catalog/product-detail.png).
- [Form](qa/catalog/listing-form.png), [preview](qa/catalog/listing-preview.png), [gửi thành công](qa/catalog/listing-success.png), [tin của tôi](qa/catalog/my-listings.png).
- [Quản lý](qa/catalog/listing-manage.png), [ẩn](qa/catalog/listing-hidden.png), [xác nhận đã bán](qa/catalog/sold-confirmation.png), [đã bán](qa/catalog/listing-sold.png).

Các mảng màu xanh trong ảnh sản phẩm của instrumentation là PNG kiểm thử upload, không phải dữ liệu sản phẩm cố định trong app. Home dùng ảnh bàn trà gốc từ `.fig` do SQL fixture cung cấp. Sáu icon danh mục được kiểm tra bằng ảnh render ở cả điện thoại và màn rộng. Toàn bộ asset nằm cục bộ, không tham chiếu URL tạm Figma.

MCP Figma vẫn hết quota Starter. Đối chiếu thiết kế dựa vào `.fig` đã giải mã và tài liệu node/geometry có sẵn; chưa tuyên bố đo khớp pixel với render trực tuyến. Tạo đơn/checkout, chat realtime và matching trao đổi chưa thuộc phạm vi giai đoạn này. Các CTA tương ứng chỉ thông báo phần chưa triển khai, không tạo kết quả thành công giả. Xem [CATALOG.md](CATALOG.md) về API, vòng đời, bảo vệ lịch sử và cách chạy.
