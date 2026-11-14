# Dữ liệu demo ReMarket

Seeder: [DuLieuMau.cs](../../Backend/src/ReMarket.Infrastructure/Seeding/DuLieuMau.cs). Fixture v1 dùng GUID xác định từ key và mốc UTC cố định 29/10/2026; đây là dữ liệu trình diễn theo Figma. Worker mua bán chỉ giải phóng đơn **chưa thanh toán có HanThanhToan** đã hết hạn. Worker trao đổi1–1 chỉ hủy đề nghị hết hạn có root **ChoDongY**, không sửa tranh chấp, vòng Smart Swap hoặc đơn cũ đang vận chuyển.

## Tài khoản

Mật khẩu chung lúc **seed lần đầu**: `ReMarketDemo!2026`. Có thể đặt `REMARKET_DEMO_PASSWORD` trước lần seed đầu. DB lưu hash có salt qua ASP.NET Core PasswordHasher, không lưu mật khẩu rõ. API đăng nhập hiện đã xác minh hash thật trong SQL; Android không điền sẵn hoặc nhận diện đặc biệt các tài khoản demo. Nên đăng ký tài khoản riêng để kiểm tra; cấp quyền theo [hướng dẫn tài khoản](../ACCOUNTS.md). Migration thứ ba chuẩn hóa mã role thành USER/ADMIN.

| Email demo | Hồ sơ | Role / trạng thái |
| --- | --- | --- |
| linh@remarket.example.test | Linh Nguyễn | Người dùng + Quản trị viên, hoạt động |
| anh@remarket.example.test | Nguyễn Minh Anh | Người dùng, hoạt động |
| nam@remarket.example.test | Nguyễn Hoàng Nam | Người dùng, hoạt động |
| vy@remarket.example.test | Thảo Vy | Người dùng, hoạt động |
| bao@remarket.example.test | Quốc Bảo | Người dùng, khóa 7 ngày theo fixture |
| huy@remarket.example.test | Trần Gia Huy | Người dùng, hoạt động |
| ha@remarket.example.test | Lê Thu Hà | Người dùng, hoạt động |

Tên miền `.test`, số điện thoại và ảnh đều là dữ liệu demo. Bàn trà, kệ sách và iPhone dùng ảnh gốc trích từ `.fig` qua các đường dẫn `/demo/figma-*`; các fixture khác vẫn dùng `/demo/san-pham.png`. Không thay ảnh người dùng đã upload. Xem nguồn node cụ thể trong [CATALOG.md](../CATALOG.md).

## Kịch bản

| Mã / nhóm | Dữ liệu sẵn có |
| --- | --- |
| Catalog/tin | 7 danh mục (6 gốc, 1 máy đọc sách); 11 sản phẩm/tin/ảnh; 8 nhu cầu trao đổi; có tin nháp, pending, visible, sold, hidden |
| RM260018 | Anh mua iPhone của Vy, 6.500.000 + 30.000 = 6.530.000đ; payment mô phỏng, shipping có 3 mốc và đang vận chuyển |
| TD-1024 | Anh đổi Sony lấy Kindle của Nam; bản cũ bù 300k, bản mới đã đồng ý 400k; hai lượt nhận, hoàn tất, một đánh giá và tiêu chí |
| SS-0301 | Anh/Dell → Vy; Vy/Canon → Nam; Nam/iPad → Anh. A trả B 500k, C trả B 500k trực tiếp; 3/3 đồng ý, 5/6 giao/nhận; chờ Anh nhận iPad |
| TD-1018 / KN-031 | Hà/Logitech đổi Huy/JBL và bù 200k; đang tranh chấp, có bằng chứng/phản hồi/quyết định; chờ xác minh thực hiện |
| Chat | 3 conversation mua bán/1–1/nhóm; 7 membership, 3 tin nhắn, 1 attachment, read cursor |
| Hỗ trợ | HT-260018 liên quan đơn iPhone, trạng thái đang xử lý, có phản hồi Admin |
| Admin | Role, khóa tài khoản, moderation log, audit seed; thông báo nhắc Anh nhận iPad |

Đã kiểm tra tất cả 43 bảng nền tảng có dữ liệu. `PhienDangNhap`/`YeuCauDatLaiMatKhau` được tạo bởi đăng nhập/khôi phục thật, không seed token giả. Hai bảng mới `PhieuThuongLuong`/`LichSuHoTro` được tạo qua luồng API/UI mua bán/hỗ trợ. Khi cần số lượng hàng chính xác sau thao tác demo, truy vấn DB; không dùng các KPI 1.284/248/96 của mockup làm dữ liệu giả đã vận hành.

Demo mua bán đầy đủ dùng hai tài khoản mới, tin được duyệt, phiếu được người bán chấp nhận, đơn/thanh toán/ship/nhận/đánh giá/hỗ trợ tạo qua API thật theo [COMMERCE.md](../COMMERCE.md). Tests HTTP dùng DB tạm riêng; instrumentation dùng tài khoản/tin QA mới trên ReMarketDemo, giữ lịch sử đơn đã hoàn tất, chỉ hủy/xóa mềm đúng fixture của test. Không thay nội dung hoặc password của tài khoản demo cũ.

Demo trao đổi1–1 theo [EXCHANGE.md](../EXCHANGE.md): đăng nhập hai USER, mỗi người tạo một tin mới được duyệt rồi gửi đề nghị, counter tiền bù, đồng ý, nhận hàng hai phía và đánh giá. Sony/Kindle nguồn TD-1024 đã gắn lịch sử nên không sửa/reset để trao đổi lại. Test `--exchange` dùng SQL tạm; Android đăng ký cặp Minh Anh demo/Hoàng Nam demo mới với email ngẫu nhiên và JWT thật. Cả cờ1–1/SmartSwap, giới hạn bù và ảnh được lưu thật; không có tài khoản bypass trong ứng dụng. Tiêu chí `DUNG_MO_TA`/`DUNG_HEN` được migration7 thêm nếu thiếu và seeder dùng lại mã hiện hữu.

Tìm Smart Swap theo [SMART_SWAP_SEARCH.md](../SMART_SWAP_SEARCH.md) không cần vòng seed SS-0301 hoặc tin mẫu cố định. `--smart-swaps` tạo DB tạm, đăng ký/đăng/duyệt3 tin bằng API thật rồi tìm vòng từ dữ liệu đó, đồng thời so100 đồ thị ngẫu nhiên với oracle độc lập. Migration8 giữ các trường nhu cầu mới NULL ở tin cũ. Muốn demo bằng ReMarketDemo, tạo3 tin mới đáp ứng nhau, bật Smart Swap và cấu hình danh mục/từ khóa/tình trạng/khoảng giá/tiền bù phù hợp; không tái kích hoạt sản phẩm đã giao dịch chỉ để hiện vòng. Không thêm vòng đăng sẵn để thay thuật toán.

Lời mời/đồng thuận/tiền bù theo [SMART_SWAP_WORKFLOW.md](../SMART_SWAP_WORKFLOW.md) dùng3 USER có3 tin mới. `--smart-swap-workflow` kiểm tra trên SQL tạm và nhóm chat thật, không ghi đè demo cũ. Migration9 giữ creator/key/tin nguồn của vòng legacy NULL; worker mới không hủy SS-0301 vì mốc thời gian prototype cũ. Worker chỉ hủy lời mời mới đang chờ và root ChoDongY, không đổi vòng chính thức đang bàn giao hoặc tranh chấp.

Demo giao nhận mới: `dotnet run --project src/ReMarket.Api -- --seed-smart-delivery-demo`, chỉ Development sau migration10. Tạo3 USER `swap-ship-a/b/c@remarket.example.test`, dùng PasswordHasher và thuật toán/service thật,3 tin/3 vận đơn, đủ đồng thuận và dừng ở2/3 nhận đúng. A/B đã có fixture xác nhận; C cần tự đăng nhập/xác nhận cuối. Seed operator tạo fixture rõ ràng, normal startup/worker không xác nhận thay người dùng. Password theo cấu hình seed lần đầu ở trên; rerun giữ tiến độ, không reset hoặc ghi đè mật khẩu đã đổi. [API/hướng dẫn demo](../SMART_SWAP_DELIVERY.md). Kết quả trên ReMarketDemo: vòng `eb1cabbf-ec78-4ab9-99e4-d504b7db537b`; ID trên database khác lấy từ CLI/danh sách tài khoản.

## Tính lặp và bảo toàn dữ liệu

Seed diễn ra trong một transaction, lấy `sp_getapplock` và ghi marker `KhoiTaoDuLieuMauV1` cuối cùng. Lần chạy sau không thêm bản ghi hoặc ghi đè tài khoản/đơn/nghiệp vụ đã sửa; chỉ nâng ba placeholder ảnh fixture nếu ID và đường dẫn cũ còn nguyên, sau đó bỏ qua. Chưa có tính năng reset/reseed phá dữ liệu. Nếu fixture key bị chèn thủ công trước mà không có marker, seeder sẽ thất bại và rollback thay vì âm thầm hợp nhất dữ liệu không rõ nguồn.

DatabaseChecks chạy thao tác âm tính và hoàn tất 6/6 trong transaction rollback, giữ SS-0301 của demo ở 5/6. Verify-FreshDatabase tạo/xóa một DB test ngẫu nhiên riêng; không reset ReMarketDemo.
