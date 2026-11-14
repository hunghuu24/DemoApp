# Kiểm thử Android Smart Swap — 12/11/2026

## Môi trường và dữ liệu

- Android Emulator `ReMarketQA`, Android 16/API 36, 360×800, mật độ 160; APK compile/target SDK 37 của project hiện có.
- ASP.NET Core 8 Release tại `10.0.2.2:5080` từ emulator, SQL Server thật/ReMarketDemo; JWT, Retrofit, DataStore và Coil thật.
- Mỗi ca Smart Swap đăng ký ba USER riêng, đăng ba tin/ảnh, duyệt bằng ADMIN kiểm thử được cấp tạm. Không thay thế API/repository bằng mock, không dùng vòng cố định trong ứng dụng.
- Ảnh bằng chứng trong kiểm thử được cung cấp qua callback ViewModel mà Photo Picker gọi; việc upload/lưu/đọc vẫn qua HTTP và SQL. Không coi đây là kiểm thử UI của trình chọn ảnh hệ thống.

## Smart Swap: 3/3 ca đạt

Log trực tiếp: [emulator-smart-swaps.txt](qa/smart-swap-android/emulator-smart-swaps.txt), `OK (3 tests)`, 206.207 giây.

| Ca | Đã kiểm tra |
|---|---|
| `boLocThatSoDoVaHuyDeNghiQuayLai` | Hạn mức bù 0 trả rỗng; bộ lọc xác minh không nhận tài khoản chưa xác minh; tình trạng nhận; xem sơ đồ; quay về kết quả/tìm vòng/khám phá; hủy đề nghị thật; Back về cài đặt |
| `baTaiKhoanTimThuongLuongChatGiaoNhanVaHaiDanhGia` | Chọn sản phẩm đang đăng; rỗng/có kết quả; ba tên thật; xem trước không tạo vòng; gửi đề nghị; chat; điều chỉnh khoản bù của A và giữ khoản C; phiên bản mới reset cả ba; A/B/C lần lượt đồng ý; ba địa chỉ; đúng ba vận đơn; nhập mã và đi từng trạng thái; nhận đúng 1/3, 2/3, 3/3; ảnh bằng chứng; hai đánh giá lưu và đọc lại qua REST; chuỗi Back |
| `tuChoiThongBaoVaBaoSuCoTamDung` | Người nhận từ chối và vòng dừng; thông báo mở đúng vòng; vòng chính thức khác có ba chặng đã giao; báo nhận sai hàng bằng UI; vòng tạm dừng và ẩn xác nhận nhận hàng; chat nhóm vẫn gửi được |

Ca chính kiểm tra **2/3 chưa hoàn tất, chưa có nút đánh giá**; chỉ người nhận thứ ba mới mở hoàn tất. Các lần đồng ý/nhận hàng là thao tác riêng của ba tài khoản, không xác nhận hộ.

Các trạng thái lỗi đã chạy: thiếu sản phẩm, không tìm thấy vòng, bộ lọc xác minh loại tin không đủ điều kiện, chưa tích đủ xác nhận nhận hàng. Loading được chờ theo StateFlow của các module. Chưa dùng kiểm thử mất mạng cưỡng bức để khẳng định khả năng phục hồi mọi tình huống mạng.

## Backend: 714 kiểm tra đạt

| Bộ kiểm tra HTTP/SQL | Số ca | Log |
|---|---:|---|
| Đồ thị/tìm vòng | 289 | [backend-search.txt](qa/smart-swap-android/backend-search.txt) |
| Lời mời/phiên bản/đồng thuận/khóa sản phẩm | 170 | [backend-workflow.txt](qa/smart-swap-android/backend-workflow.txt) |
| Giao nhận, bộ lọc form, ảnh bằng chứng và đánh giá | 255 | [backend-delivery-reviews.txt](qa/smart-swap-android/backend-delivery-reviews.txt) |

Bộ cuối gồm: không đánh giá khi đang chờ hoặc mới nhận 2/3; không đánh giá chính mình/người ngoài; yêu cầu sai không để lại đánh giá một phần; ba tài khoản gửi đúng sáu đánh giá; retry không nhân đôi/ghi đè; ảnh cần xác nhận nhận đúng, riêng tư cho ba thành viên; tranh chấp chặn hoàn tất; xác nhận cuối và báo sự cố đồng thời chỉ một thao tác thành công. Có kiểm tra EF model khớp migration hiện có, không thêm migration.

Backend Release build **0 lỗi, 0 cảnh báo**: [backend-build.txt](qa/smart-swap-android/backend-build.txt).

## Hồi quy và build Android

Ứng dụng và APK kiểm thử đã build thành công sau thay đổi giao diện: [android-apk-tested.txt](qa/smart-swap-android/android-apk-tested.txt).

Lint và kiểm thử đơn vị cuối **đạt**, `BUILD SUCCESSFUL`: [android-final-checks.txt](qa/smart-swap-android/android-final-checks.txt). Lint **0 lỗi, 16 cảnh báo**: 7 cảnh báo phiên bản thư viện, 7 đường vector gốc dài, 1 kích thước vector sơ đồ gốc 288×52 và 1 thứ tự tham số Modifier của helper. Không tắt detector lint để làm kiểm tra thành công.

APK ứng dụng có SHA-256 `C8EE0184B3C81ACF4622CE00830066BF33450295F9620FBE5401A502C6FED42C`; bản ứng dụng giữ nguyên giữa lượt Smart Swap đạt và lượt kiểm tra cuối. [APK Debug](../Android/app/build/outputs/apk/debug/app-debug.apk), [lint HTML](../Android/app/build/reports/lint-results-debug.html).

**15 ca giao diện khác nhau đạt qua các lượt chạy**, không coi là một lượt chạy duy nhất:

| Nhóm | Kết quả | Bằng chứng |
|---|---:|---|
| Smart Swap | 3/3 | `emulator-smart-swaps.txt`, 206.207 giây |
| Tài khoản | 3/3 | Các trạng thái kết thúc `0` trong `emulator-regression-before-wait-fix.txt` |
| Mua bán | 4/4 | Cùng log hồi quy |
| Trao đổi 1–1 | 3/3 | Cùng log hồi quy |
| Sản phẩm | 2/2 | [emulator-catalog-retest.txt](qa/smart-swap-android/emulator-catalog-retest.txt), `OK (2 tests)`, 60.66 giây |

Lượt hồi quy đầu có một ca sản phẩm bấm tab Tin nhắn khi module Trao đổi đang loading; đã cập nhật điều kiện chờ các ViewModel trong `SanPhamEndToEndTest`. Log ban đầu được giữ tại [emulator-regression-before-wait-fix.txt](qa/smart-swap-android/emulator-regression-before-wait-fix.txt), không che lần thất bại. Sau đó chạy lại cả nhóm sản phẩm và đạt. APK ứng dụng không thay đổi trong quá trình sửa đồng bộ kiểm thử.

Môi trường Emulator từng hiện hộp thoại “System UI isn’t responding”, làm Espresso không lấy được focus trước khi đăng nhập. Đã phục hồi System UI, kiểm tra `mCurrentFocus` trỏ vào MainActivity rồi mới chạy lại; log môi trường riêng ở `emulator-catalog-systemui-focus.txt`. Có một lượt native ART crash lúc vừa cold boot, lưu `emulator-cold-start-crash.txt`; các kết quả đạt ở trên là các lượt chạy sau khi hệ thống đã sẵn sàng. Không tính các lượt bị gián đoạn này là ca đạt.

## Đối chiếu hình ảnh

Đã xem ảnh chụp Emulator của các form tìm vòng, kết quả, chi tiết, sheet tiền bù, xác nhận ba bên, chat, địa chỉ, vận đơn, sự cố và hai đánh giá. Toàn bộ ảnh ở [qa/smart-swap-android/screenshots](qa/smart-swap-android/screenshots).

| Form/trạng thái | Ảnh |
|---|---|
| Tìm vòng | [find-cycle.png](qa/smart-swap-android/screenshots/find-cycle.png) |
| Kết quả thật | [matching-cycles.png](qa/smart-swap-android/screenshots/matching-cycles.png) |
| Ba món/chiều giao | [diagram-direction-corrected.png](qa/smart-swap-android/screenshots/diagram-direction-corrected.png) |
| Sheet tiền bù | [compensation-sheet.png](qa/smart-swap-android/screenshots/compensation-sheet.png) |
| Đồng ý ba bên | [three-consents.png](qa/smart-swap-android/screenshots/three-consents.png) |
| Chat nhóm | [group-chat.png](qa/smart-swap-android/screenshots/group-chat.png) |
| Thông tin giao nhận | [delivery-contact.png](qa/smart-swap-android/screenshots/delivery-contact.png) |
| Vận đơn/tiến độ | [shipping-tracking.png](qa/smart-swap-android/screenshots/shipping-tracking.png) |
| Sự cố | [paused-circle.png](qa/smart-swap-android/screenshots/paused-circle.png) |
| Hai đánh giá | [two-member-reviews.png](qa/smart-swap-android/screenshots/two-member-reviews.png) |
| Đã lưu hai đánh giá | [reviews-persisted.png](qa/smart-swap-android/screenshots/reviews-persisted.png) |

Trong QA đã sửa thông báo nổi che ô đồng ý bằng thông báo trong bố cục; sửa nhãn món ở cạnh quay về không chạm đường mũi tên, giữ vector gốc; ảnh sản phẩm bo góc 12dp. Các CTA cố định không che vùng cuộn/IME.

Nguồn Figma: MCP có code + ảnh gốc của `205:115234`; tám frame còn lại bị hạn mức Starter, đối chiếu bằng text/hình học đã đọc từ `Nhom4.fig`. Các JSON nguồn và SVG ở [assets/smart-swap](assets/smart-swap). Không khẳng định đã so sánh pixel với ảnh MCP của tám frame chưa được trả ảnh. Chi tiết khác biệt có chủ đích (vận đơn mô phỏng, tiến độ 3 người nhận thay nhãn 6 giao/nhận) nằm trong [SMART_SWAP_ANDROID.md](SMART_SWAP_ANDROID.md).

## Dữ liệu demo và giới hạn

Kiểm tra SQL đọc vòng demo `eb1cabbf-ec78-4ab9-99e4-d504b7db537b`: vòng `DangBanGiao`, giao dịch `DangThucHien`, nhận đúng **2/3**; không dùng ca demo này để bấm hoàn tất trong QA. Các fixture kiểm thử dùng tài khoản/sản phẩm mới; lịch sử đã hoàn tất hoặc tranh chấp được giữ theo khóa ngoại.

Đã thu hồi 1 quyền ADMIN tạm và 33 phiên đăng nhập của tài khoản Smart Swap/Admin do lượt kiểm thử tạo; xác minh lại ADMIN tạm có 0 quyền ADMIN/0 phiên hoạt động, demo cũ vẫn 2/3: [final-database-check.json](qa/smart-swap-android/final-database-check.json). Đã xóa file thông tin đăng nhập tạm và dừng API/Emulator QA. Để chạy demo tiếp, khởi động Backend bình thường theo README, không dùng host JWT tạm của kiểm thử.

Thanh toán/vận chuyển mô phỏng; chưa có nghiệp vụ quản trị khôi phục vòng sự cố. Chat dùng polling khi màn STARTED, chưa có WebSocket.
