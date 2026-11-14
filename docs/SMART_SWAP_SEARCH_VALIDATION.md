# Xác minh tìm Smart Swap — 12/11/2026

Backend của project ReMarket hiện có, .NET8/EF Core8.0.33 và SQL Server2022 thật. [Thuật toán, tiêu chí và hợp đồng API](SMART_SWAP_SEARCH.md). Không sửa/build Android ở giai đoạn Backend này.

## Kết quả

| Nhóm | Kết quả / chứng cứ |
| --- | --- |
| Smart Swap thuần + HTTP/SQL | **289 kiểm tra đạt**, bao gồm100 đồ thị sinh ngẫu nhiên đối chiếu oracle độc lập: [log](qa/smart-swaps/backend-checks.txt) |
| Hồi quy trao đổi1–1 | **125 đạt**: [log](qa/smart-swaps/exchange-regression.txt) |
| Hồi quy catalog | **94 đạt**: [log](qa/smart-swaps/catalog-regression.txt) |
| Hồi quy mua bán | **119 đạt**: [log](qa/smart-swaps/commerce-regression.txt) |
| Hồi quy tài khoản | **54 đạt**: [log](qa/smart-swaps/auth-regression.txt) |
| SQL nền tảng trên DB mới | **20 đạt**, mutation rollback: [log](qa/smart-swaps/fresh-database.txt) |
| Release build | **0 lỗi,0 cảnh báo**: [log](qa/smart-swaps/release-build.txt) |
| Model/migration | Không có model changes chưa tạo migration: [log](qa/smart-swaps/model-check.txt) |

Tổng **701 kiểm tra/assertion Backend**, trong đó289 mới cho Smart Swap; không phải701 test UI hoặc một báo cáo hiệu năng tải đồng thời. Các nhóm HTTP dùng ASP.NET TestServer, middleware/controller/DI/JWT thật và SQL Server provider thật, không dùng EF InMemory/SQLite/mock repository.

## Đồ thị và nghiệp vụ

| Nhóm | Đã kiểm tra |
| --- | --- |
| Đúng3 người | Ví dụ iPhone–Laptop–iPad trả đúng chiều giao A→C→B→A; vòng xoay không trùng; đổi thứ tự đầu vào giữ mã; loại self-loop, vòng2, vòng4, chỉ2 chủ sở hữu và trùng sản phẩm |
| Dữ liệu bất kỳ | Ví dụ nhạc cụ/kính thiên văn/sách/đèn;100 đồ thị ngẫu nhiên3–9 đỉnh, danh mục/owner/sản phẩm ngẫu nhiên, đối chiếu tập vòng chính xác với ba vòng lặp vét cạn độc lập |
| Đồ thị dày | **60 đỉnh,3.540 cạnh,68.440 chu trình có hướng**; đếm đủ mọi vòng trong ngân sách và chỉ giữ một phần tử khi yêu cầu trang size1 |
| Nhu cầu | Danh mục cha/con; từ khóa AND và phương án OR rõ ràng; Unicode tiếng Việt/case/đ→d/chữ–số; ranh giới từ; nhu cầu thiếu, chung chung hoặc phủ định không tạo cạnh giả |
| Tình trạng/giá | Thứ bậc tình trạng, mức tối thiểu bao gồm biên; khoảng giá món nhận bao gồm biên, loại dưới/trên khoảng; giá tối đa1.000 tỷ không tràn số |
| Tiền bù | Hai người trả/một người nhận; một người trả/hai người nhận; cap tổng trả; vượt cap1 VND bị loại; người nhận phải bật bù; giá bằng nhau không cần tiền; mỗi người và toàn vòng bảo toàn giá trị |
| Xếp hạng | Điểm cao trước, ngưỡng điểm trước phân trang, trang liên tiếp không lặp, anchor ở bất kỳ vị trí, trang ngoài phạm vi trả rỗng giữ tổng |
| Ngân sách | Vượt số tin/cạnh/lượt duyệt báo lỗi rõ ràng, không trả200 kèm kết quả một phần; cancellation đã hủy được tôn trọng kể cả tập rỗng |

## REST API và SQL

Fixture HTTP được tạo trong DB mới `ReMarketTest_<GUID>`, chưa có bất kỳ dòng `VongSmartSwap`. Ba USER và ADMIN được đăng ký/đăng nhập qua API thật. Tin của ba USER có ảnh upload thật và được ADMIN duyệt thật; ba chủ sở hữu dùng cùng loại “chỉ Smart Swap, không1–1” để xác minh không phụ thuộc cờ1–1.

`GET /api/smart-swaps/search` tìm đúng vòng từ các tin đó, có phương án tiền bù4 triệu và snapshot/rowversion tin. Một người có thêm tin hợp lệ tạo hai lựa chọn vòng riêng, mỗi vòng vẫn đúng3 người/mỗi người một món. HTTP pagination được kiểm tra trên cả hai kết quả.

Các trường hợp SQL loại khỏi kết quả được thay lần lượt trên đúng fixture rồi khôi phục: cờ Smart Swap tắt; từng trạng thái khác `DangHienThi`; xóa mềm; chỉ bán; tài khoản khóa; danh mục cha bị ẩn; sai danh mục/từ khóa mong muốn; tình trạng quá thấp; giá nhận ngoài khoảng; cap quá thấp; bên nhận không chấp nhận bù; hold chưa giải phóng kể cả mốc thời gian đã qua. `CaHai` và hold đã giải phóng được xét lại đúng quy tắc.

Khi không tìm được vòng, HTTP200 có `items:[]`, `tongSo:0` và `thongBao` không rỗng. Anonymous401; query sai400, kể cả ngưỡng điểm100,01 hoặc−0,01; anchor không khả dụng404; vượt candidate budget422. Ngưỡng điểm dùng validation decimal, không làm tròn về số nguyên trước khi kiểm tra biên. Response `private,no-store`. Không có dữ liệu phiên đăng nhập/điện thoại/email/địa chỉ riêng trong DTO kết quả tìm vòng.

Đếm các bảng trước/sau tìm kiếm xác minh không thêm `GiaoDich`, `VongSmartSwap`, `ThanhVienVong`, `ChangTraoDoi`, `TienBu`, `GiuChoSanPham`, `ThongBao` hoặc `DeNghiTraoDoi`; rowversion tin không đổi. Sau toàn bộ kiểm thử vẫn không có dòng vòng Smart Swap đã lưu. Một giao dịch/hold riêng được fixture tạo chủ động để kiểm tra loại sản phẩm đang giữ, không được API search tạo.

Tiêu chí mới được kiểm tra qua API tạo/ảnh/gửi duyệt và DTO đọc lại. Client cũ sửa tin mà omit/null object tiêu chí vẫn giữ dữ liệu mới; `{}` xóa tiêu chí tùy chọn. API chặn khoảng giá đảo ngược/âm, mã tình trạng và danh mục không hợp lệ. Hai CHECK SQL mới cũng từ chối mutation sai trực tiếp, rồi rollback.

## Migration và bảo toàn workspace

Migration **20261112061936_TieuChiTimSmartSwap** đã áp dụng trên ReMarketDemo: [log](qa/smart-swaps/migration.txt). Query SQL xác minh **47 bảng,92 FK,66 CHECK,17 trigger,8 migrations**, không tính bảng history: [số liệu](qa/smart-swaps/schema-counts.txt).

[Script SQL idempotent](database/remarket-schema.sql) được xuất lại từ8 migrations, áp **hai lần** trên DB trống độc lập rồi seed/chạy20 checks thành công. [Từ điển JSON](database/schema-dictionary.json) xuất lại từ EF47 entity. Các cột mới nullable; không seed vòng mới, không ghi đè nhu cầu hoặc reset dữ liệu demo cũ.

Các DB và thư mục ảnh tạm do mỗi lượt kiểm thử tạo đã được dọn đúng tên/path do lượt đó sinh; không xóa ReMarketDemo hoặc ảnh/tài khoản của người dùng. Không tạo credential đặc biệt hay gửi token/mật khẩu vào báo cáo. Không thay đổi code, APK, AVD hoặc state Android ở bước này.

## Giới hạn đã xác định

Đối sánh dùng danh mục/từ khóa xác định và thứ bậc tình trạng, không phải hiểu ngôn ngữ tự nhiên bằng AI. Nhu cầu tự do phức tạp cần tiêu chí cấu trúc; xem quy tắc trong tài liệu API. Giới hạn tài nguyên có lỗi422 riêng và rate limit10 request/phút/IP; không có benchmark tải đồng thời hoặc cam kết latency production từ các kiểm tra này. Tìm kiếm không đặt chỗ; workflow mời/đồng ý/giao nhận3 bên và kết nối Android là giai đoạn khác.
