# Xác minh giao nhận Smart Swap — 12/11/2026

[Hợp đồng API/demo](SMART_SWAP_DELIVERY.md). Project ReMarket hiện có, .NET8/EF Core8.0.33 và SQL Server2022 thật. API kiểm tra qua ASP.NET TestServer với middleware/controller/JWT thật; bổ sung smoke Kestrel trên ReMarketDemo bằng ba tài khoản demo riêng. Không mock repository hoặc dùng EF InMemory/SQLite.

## Kết quả

| Nhóm | Lượt cuối / chứng cứ |
| --- | --- |
| Giao nhận/ba vận đơn/receipt/sự cố/demo | **222 kiểm tra đạt**: [log](qa/smart-swap-delivery/backend-checks.txt) |
| Hồi quy đồng thuận Smart Swap | **170 đạt**: [log](qa/smart-swap-delivery/workflow-regression.txt) |
| Hồi quy tìm vòng | **289 đạt**, gồm100 đồ thị ngẫu nhiên/oracle: [log](qa/smart-swap-delivery/search-regression.txt) |
| Hồi quy trao đổi1–1 | **125 đạt**: [log](qa/smart-swap-delivery/exchange-regression.txt) |
| Hồi quy catalog | **94 đạt**: [log](qa/smart-swap-delivery/catalog-regression.txt) |
| Hồi quy mua bán | **119 đạt**: [log](qa/smart-swap-delivery/commerce-regression.txt) |
| Hồi quy tài khoản | **54 đạt**: [log](qa/smart-swap-delivery/auth-regression.txt) |
| SQL nền tảng/fresh database | **20 đạt**: [log](qa/smart-swap-delivery/fresh-database.txt) |
| Release build | **0 lỗi,0 cảnh báo**: [log](qa/smart-swap-delivery/release-build.txt) |
| Model/migration | Không có model changes chưa tạo migration: [log](qa/smart-swap-delivery/model-check.txt) |

Tổng **1.093 kiểm tra/assertion Backend**. Không phải1.093 test UI hoặc benchmark tải production. Android không sửa/build ở giai đoạn Backend này.

## Luồng ba tài khoản

Fixture tạo DB SQL ngẫu nhiên, đăng ký/đăng nhập A/B/C qua API thật, thêm người ngoài và ADMIN duyệt. Các sản phẩm có ảnh upload thật, nhu cầu/tiền bù được thuật toán tìm từ SQL rồi đi qua3/3 đồng thuận. Ba sender và receiver lấy từ các cạnh thật, không truyền người xác nhận từ client.

| Nhóm | Đã kiểm tra |
| --- | --- |
| Điều kiện bắt đầu | Chưa đủ3 consent không xác nhận contact hoặc tạo vận đơn được; thiếu một contact không tạo bộ vận đơn dở |
| Contact | Điện thoại sai/thiếu xác nhận bị400; chuẩn hóa+84; retry không nhân dòng; cùng key đổi payload409; sau booking địa chỉ snapshot bất biến |
| Quyền riêng tư | Anonymous401, người ngoài404; contact người khác và địa chỉ của chặng không liên quan bị redact |
| Ba vận đơn | Đúng3 dòng ChoGui theo3 cạnh/cùng vòng; tạo lại không nhân dòng; không tạo DonHang mua bán để chứa chặng |
| Mã/trạng thái | Chỉ sender nhập mã403 nếu người khác; mã trùng kể cả khác hoa thường409; chưa có mã/thiếu xác nhận gửi400; bỏ bước409; đi đủ ChoGui→DaGui→DangVanChuyen→DaGiao, lịch sử lưu SQL |
| Không xác nhận thay | Ba nhãn vận chuyển DaGiao vẫn **0/3** receipt nhận; chỉ có3 receipt gửi do chính sender xác nhận |
| Người nhận | Sender không xác nhận thay được403; đúng receiver nhưng chưa DaGiao409; kiểm tra/nhận đúng=false400; không tự xác nhận tiền bù của actor |
| Tiến độ | A xác nhận→1/3; B→**2/3 vẫn DangThucHien, giữ đủ3 hold**; C→3/3 mới HoanTat ở cả vòng/root, nhả3 hold, tin DaTraoDoi |
| Idempotency | Nhận lặp ở1/3 hoặc sau3/3 không nhân receipt; cuối vòng đúng6 receipt gửi/nhận, không thêm xác nhận từ sự kiện giao vận |
| SQL hoàn tất | UPDATE vòng HoanTat khi2/3 bị trigger từ chối/rollback |
| Chat/thông báo | Tin nhóm gửi bằng API thật sau giao nhận; trạng thái hoàn tất có thông báo cho tài khoản C; chat vẫn dùng được khi tạm dừng |
| Sai hàng/chưa nhận | Mỗi loại được báo bởi receiver đúng chặng, tạo KhieuNai và TamDung/TranhChap nguyên tử; người khác403; key lặp không nhân sự cố, đổi payload409 |
| Tạm dừng | Không nhận đúng/cập nhật shipping tiếp được409; giữ3 hold; SQL không thể hoàn tất vòng có sự cố |
| Race | Xác nhận cuối và báo sự cố song song: đúng một200 và một409; cuối cùng hoặc hoàn tất hoặc tạm dừng, không cả hai |

222 kiểm tra bao gồm cả thao tác fixture/HTTP/SQL và assertions nghiệp vụ. Không dùng “hàng đã giao” từ tracking làm bằng chứng nhận đúng. Tiền bù chỉ cập nhật cờ thuộc actor sau lời xác nhận rõ của người đó; không có giao dịch ngân hàng thật.

## Demo và smoke trên ReMarketDemo

`DemoGiaoNhanSmartSwap` tạo fixture qua services tài khoản/catalog/thuật toán/đồng thuận/giao nhận; mật khẩu được băm thật. Giai đoạn fixture này chủ động tạo hai receipt mẫu A/B, để C còn chờ. Normal startup và workers không làm hành động đó thay người dùng.

- Test trên DB tạm: seed lần đầu để2/3; seed lần hai giữ cùng ID/không nhân vòng. C đăng nhập JWT thật, xác nhận chặng của mình mới3/3. Seed lại sau hoàn tất không reset về2/3.
- Trên ReMarketDemo đã chạy CLI hai lần, cùng ID **eb1cabbf-ec78-4ab9-99e4-d504b7db537b**: [log seed](qa/smart-swap-delivery/demo-seed.txt).
- Kestrel QA riêng health ready200; lần lượt đăng nhập ba USER `swap-ship-a/b/c@remarket.example.test`, GET delivery xác minh3 vận đơn, đúng một incoming leg cho mỗi user, trạng thái vẫn2/3. Tài khoản C chưa được nhận thay: [log smoke](qa/smart-swap-delivery/live-demo.txt).
- Smoke chỉ đọc tiến độ và logout cả3 phiên; không tiêu thụ xác nhận C cuối trong database demo. Server QA riêng đã dừng, không dừng server/AVD của người dùng.

Các tài khoản demo không có nhánh xác thực đặc biệt. Không ghi password/JWT vào log; mật khẩu mặc định/config lúc seed được mô tả trong hướng dẫn demo. Ảnh fixture màu xanh được upload/sanitize qua kho ảnh thật, không có danh sách hàng giả trong runtime Android.

## Migration và dữ liệu cũ

Migration10 **20261112080514_GiaoNhanSmartSwap** đã áp dụng trên ReMarketDemo: [log](qa/smart-swap-delivery/migration.txt). Query thực tế xác minh **48 bảng,100 FK,70 CHECK,22 trigger,10 migrations** (không tính history) và demo3 vận đơn/2 receiver đã nhận: [số liệu SQL](qa/smart-swap-delivery/schema-and-demo.txt).

[SQL idempotent](database/remarket-schema.sql) xuất từ10 migrations, áp **hai lần** trên một DB trống độc lập rồi seed/chạy20 checks thành công. [Từ điển48 entity](database/schema-dictionary.json) và [tài liệu CSDL](database/THIET_KE_CSDL.md) đã cập nhật. Vận chuyển mua bán cũ tiếp tục dùng DonHangId; regression119 đạt sau khi nguồn này trở thành nullable có CHECK đúng một nguồn.

Các DB/ảnh tạm của test được dọn đúng tên/path đã sinh, không reset ReMarketDemo. Demo mới dùng tài khoản/sản phẩm/marker riêng, không sửa vòng legacy5/6 hoặc mật khẩu tài khoản cũ. Hành vi rollback migration được bảo vệ khi đã có vận đơn mới để giữ lịch sử.

## Phạm vi tiếp theo

API hiện tại lưu báo cáo và tạm dừng, không tự xử lý/khôi phục tranh chấp. Quản trị giải quyết sự cố, hỗ trợ sau xác nhận/hoàn tất và đánh giá vòng3 bên là nghiệp vụ tiếp theo. UI Android Smart Swap, push thiết bị và WebSocket chưa bổ sung. Không tích hợp vận chuyển hoặc ngân hàng thật.
