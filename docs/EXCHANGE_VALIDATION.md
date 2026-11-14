# Xác minh trao đổi 1–1 ReMarket — 11/11/2026

Project hiện có, ASP.NET Core 8/EF Core 8.0.33, SQL Server 2022 local và Android Compose/Retrofit/Coil. [Hướng dẫn demo, API và quy tắc](EXCHANGE.md). Không dùng mock repository, sản phẩm cố định hoặc cơ chế đăng nhập đặc biệt trong app.

## Backend và CSDL

- Debug và Release build thành công, Release **0 lỗi, 0 cảnh báo**.
- **125 kiểm tra trao đổi HTTP/SQL đạt** trên DB SQL Server mới ngẫu nhiên, real JWT: [log](qa/exchange/backend-checks.txt). Hai USER MinhAnhDemo/HoangNamDemo được đăng ký và đăng nhập qua API thật, có thêm người ngoài và ADMIN để kiểm tra quyền.
- Hồi quy **94 catalog**, **119 mua bán**, **54 tài khoản** đạt: [catalog](qa/exchange/catalog-regression.txt), [mua bán](qa/exchange/commerce-regression.txt), [tài khoản](qa/exchange/auth-regression.txt).
- **20 kiểm tra SQL nền tảng đạt**, gồm cả trigger Smart Swap cũ; script SQL idempotent áp dụng **hai lần** trên DB trống, seed/check thành công: [log](qa/exchange/fresh-database.txt). Tổng **412 kiểm tra/assertion Backend**, không phải412 bài UI.
- Migration7 `20261111155701_TraoDoiMotMot` đã áp dụng trên ReMarketDemo. **47 bảng, 92 FK, 64 CHECK, 17 trigger**, không tính migration history: [số liệu SQL](qa/exchange/schema-counts.txt). Không có model changes chưa tạo migration.
- Đã xuất lại [SQL triển khai](database/remarket-schema.sql), [từ điển47 entity](database/schema-dictionary.json) và [tài liệu CSDL](database/THIET_KE_CSDL.md). Migration giữ cột nullable cho đề nghị legacy, không reset dữ liệu cũ.

| Nhóm | Tình huống đã xác minh qua HTTP/SQL |
| --- | --- |
| Đăng tin | Trường bắt buộc, ảnh tối đa6, JPG/PNG decode/re-encode, draft/private, submit/ADMIN duyệt, cờ1–1=false được lưu, ít nhất một cơ chế trao đổi |
| Khám phá/chọn món | Query/filter/phân trang, nhu cầu muốn nhận, chỉ tin đúng owner đã đăng/còn khả dụng và bật1–1; loại draft/đang giữ; GET sản phẩm không thêm GiaoDich |
| Tạo đề nghị | Không tự đổi món của mình, không dùng tin của người khác, kiểm tra cả hai version, điều kiện tiền bù và cap người trả; key retry không thêm giao dịch, đổi payload cùng key409 |
| Quyền/lịch sử | Người ngoài không đọc đề nghị/chat/ảnh riêng/bằng chứng; snapshot còn đọc được khi tin ngừng công khai; creator không tự chấp nhận; stale version409 |
| Counter | Đóng bản cũ, giữ lịch sử tiền bù/lời nhắn, tạo consent mới; counter sau chấp nhận giữ hai món và khóa bàn giao đến khi đồng ý lại |
| Giữ món | Đồng ý đúng bản giữ hai món nguyên tử; rút khỏi khám phá; hai accept cạnh tranh chỉ một thành công; không sửa hoặc bán món đang giữ |
| Chat | Hội thoại riêng theo giao dịch, mở lại cùngID, tin thật được lưu và chỉ participant đọc được |
| Hủy/từ chối/hết hạn | Đúng quyền/phiên bản, nhả hold và công khai lại đúng tin; hết hạn chỉ xử lý root chờ đồng ý, bảo toàn tranh chấp legacy |
| Giao nhận | Actor xác nhận giao món của mình/nhận món đối tác, đúng phiên bản đã chốt; một phía chưa hoàn tất; kiểm tra tiền bù đúng vai trò; không counter/hủy sau giao nhận |
| Bằng chứng | Chỉ chủ receipt tải lên, giới hạn5 ảnh, retry key/hash không nhân ảnh; private route có kiểm tra participant |
| Hoàn tất/đánh giá | Hai bên nhận đủ và xác nhận trả/nhận tiền bù mới hoàn tất; hai tin DaTraoDoi, hold được giải phóng; hai đánh giá riêng, không trùng và không đánh giá sớm |

Regression catalog đã phát hiện cập nhật ảnh cạnh tranh không hoàn thành ổn định khi giữ shared read locks lâu. Catalog chuyển sang ReadCommitted, vẫn giữ UPDLOCK sản phẩm–tin và rowversion, cùng unique/FK/trigger. Sau sửa,94 catalog,119 mua bán,125 trao đổi đều đạt; không bỏ kiểm tra concurrency.

## Android và đối chiếu thiết kế

`:app:assembleDebug`, `:app:assembleDebugAndroidTest`, `:app:lintDebug`, `:app:testDebugUnitTest` thành công: [build](qa/exchange/android-build.txt), [build test cuối](qa/exchange/android-test-build.txt). Unit test hiện có 1 bài đạt. Lint **0 lỗi, 14 cảnh báo** gồm 7 gợi ý dependency và 7 vector dài đã có từ catalog/chat; không đổi dependency hoặc giản lược hình học Figma để che cảnh báo.

Lượt cuối trên APK hiện tại: **OK (12 tests)**, **136,286 giây**, không skip: [log đầy đủ](qa/exchange/instrumentation-result.txt). Gồm 3 bài trao đổi dưới đây, 4 bài mua bán, 2 bài tin đăng và 3 bài tài khoản; tất cả dùng API/SQL thật. [SHA-256 APK](qa/exchange/apk-sha256.txt). Chỉ kiểm tra lại rộng hơn sau khi phát hiện/sửa lỗi đồng bộ test, không thay nghiệp vụ để bỏ qua assertion.

Viewport QA360×800, density160, AVD riêng; giữ dữ liệu/AVD của người dùng. Roboto đóng gói, foundation và bố cục lấy từ thiết kế đã đọc. Form `226:213` đọc trực tiếp bằng Figma MCP; các frame khác dùng snapshot `.fig` đã giải mã vì MCP hết lượt Starter. Không tuyên bố đã đọc lại mọi frame trực tuyến hoặc đối chiếu pixel toàn bộ ứng dụng.

Bài mua bán cũ đổi display800×1000 từng kẹt Espresso khi truy vấn root Compose trong lúc cấu hình display đang thay đổi. Đã dừng hai lượt bị kẹt, sửa helper chờ lệnh `wm` hoàn thành và recreate Activity trước khi đồng bộ Compose, giữ nguyên toàn bộ assertion chức năng/chiều rộng. Bài riêng đạt21,121 giây [log](qa/exchange/resize-regression.txt); sau đó cả12 bài đạt trong cùng lượt cuối. [Ảnh800×1000](qa/exchange/checkout-wide-regression.png); emulator đã trả về360×800. Hai tin QA bỏ dở được xóa mềm qua API đúng chủ sở hữu, không reset dữ liệu khác.

Ba kịch bản mới gọi API thật qua Retrofit, thay tài khoản bằng đăng nhập JWT/DataStore thật:

| Bài instrumentation | Luồng |
| --- | --- |
| `haiTaiKhoanThuongLuongNhanDuVaDanhGia` | A xem chi tiết/chat hỏi tình trạng → chọn món đã đăng → đề nghị bù300k/lịch hẹn → B nhận và counter400k → A đồng ý → A xác nhận/ảnh bằng chứng, còn chờ B → B nhận đủ → cả hai đánh giá; kiểm tra receipt/tiền bù/review cuối qua API |
| `dangTinSuaAnXoaVaHaiCoCheTraoDoi` | Validation form dài/ảnh/cơ chế → draft/private → sửa ghi chú/gửi duyệt → ADMIN duyệt API → tin của tôi/chi tiết → ẩn/xóa; kiểm tra cờ1–1 và3 bên từ response thật |
| `boLocChonMonHopLeTuChoiVaHuy` | Search/filter giá/reset → selector rỗng với tin chỉ3 bên → tạo món1–1 hợp lệ → gửi và B từ chối → gửi mới/chấp nhận/hủy trước bàn giao → hai tin công khai lại |

Ảnh kiểm chứng ở [qa/exchange/screenshots](qa/exchange/screenshots):

| Form/tìm kiếm | Đề nghị/thương lượng | Giao nhận/hoàn tất |
| --- | --- | --- |
| [Đăng tin đầu form](qa/exchange/screenshots/post-form-top.png) | [Tạo đề nghị](qa/exchange/screenshots/create-offer.png) | [Xác nhận](qa/exchange/screenshots/handover-form.png) |
| [Điều kiện và địa điểm](qa/exchange/screenshots/post-form-bottom.png) | [Đề nghị nhận](qa/exchange/screenshots/offers-received.png) | [Chờ đối tác](qa/exchange/screenshots/waiting-partner.png) |
| [Khám phá](qa/exchange/screenshots/explore.png) | [Counter tiền bù](qa/exchange/screenshots/counter-form.png) | [Hoàn tất](qa/exchange/screenshots/completed.png) |
| [Chi tiết](qa/exchange/screenshots/product-detail.png) | [Cùng đồng ý](qa/exchange/screenshots/agreement-accepted.png) | [Đã đánh giá](qa/exchange/screenshots/review-success.png) |
| [Bộ lọc](qa/exchange/screenshots/search-filter.png) | [Chat riêng](qa/exchange/screenshots/exchange-chat.png) | [Form đánh giá](qa/exchange/screenshots/review-form.png) |

Callback kết quả Photo Picker nhận PNG kiểm thử; upload, validation/sanitization, ảnh riêng, Coil, repository và HTTP đều thật. Những ô ảnh xanh trong fixture là ảnh test, không có sản phẩm hardcode trong runtime. Credential ADMIN QA truyền từ file tạm ngoài repository, không ghi mật khẩu/JWT vào log. Chỉ fixture do chính test tạo được hủy/xóa mềm khi có thể; lịch sử hoàn tất giữ nguyên.

Sau kiểm tra đã thu hồi mọi phiên và gỡ role ADMIN của đúng tài khoản quản trị tạm. Xác minh SQL còn0 role ADMIN/0 phiên chưa thu hồi, token vừa cấp bị HTTP401 sau thu hồi; xóa file credential ngoài repository. Health ready thành công trước khi dừng đúng API/emulator do tác vụ này tạo. Giữ CSDL ReMarketDemo, dữ liệu và lịch sử trao đổi hoàn tất; không reset AVD hoặc tài khoản của người dùng.

## Phạm vi còn lại

Cờ cho phép Smart Swap3 bên được lưu/lọc, chưa triển khai tạo vòng/matching ba bên. Tiền bù thanh toán trực tiếp giữa hai người, app chỉ lưu xác nhận; không có cổng ngân hàng. Chat dùng polling foreground; hỗ trợ giao nhận mở ticket hiện có, chưa phải toàn bộ workflow quản trị tranh chấp trao đổi. Retry bằng chứng giữ ảnh/key, không làm giả trạng thái đã nhận hoặc hoàn tất.
