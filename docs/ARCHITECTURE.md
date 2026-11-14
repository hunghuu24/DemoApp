# Kiến trúc khởi tạo ReMarket

Theo yêu cầu mới: Android Compose/MVVM/Retrofit, ASP.NET Core 8/EF Core và SQL Server thay thế phương án XML/Ktor/PostgreSQL trong tài liệu kế hoạch ban đầu. Phân tích màn hình và các bất nhất Prototype vẫn giữ giá trị; lựa chọn công nghệ cũ không còn áp dụng.

## Android

`MainActivity` dựng `ReMarketTheme` và `TaiKhoanApp`. `TaiKhoanViewModel` expose `StateFlow<TaiKhoanUiState>`; Compose collect theo lifecycle, không tự truy cập network. Model/command và repository interface nằm trong domain, không phụ thuộc Retrofit/OkHttp. Implementation thực hiện Retrofit và lưu token bằng DataStore/Android Keystore. Application giữ `AppContainer`, HttpClient/Retrofit/ImageLoader được tạo một lần, không tạo lại theo recomposition.

`feature/auth` và `feature/account` có luồng tài khoản thật, Home/Admin theo role server. `feature/bootstrap` giữ làm tài nguyên chẩn đoán cũ. Android không chứa SQL connection string hoặc tài khoản hardcode. Điều hướng không sao chép các dây nối sai trong Figma; backend kiểm tra JWT/session/role.

`feature/catalog/BanHangViewModel` giữ StateFlow cho khám phá/tin đăng và form, gọi `TinDangRepository` trong domain. Retrofit/Multipart ở implementation; Compose chỉ chuyển kết quả Photo Picker có giới hạn dung lượng vào ViewModel. `BanHangApp` được mở sau USER login; các trạng thái riêng được xóa khi đổi tài khoản/logout. CRUD dùng aggregate sản phẩm–tin đăng và rowversion từ API, không tự suy ra owner/role/trạng thái trong app.

`feature/commerce/MuaBanViewModel` giữ state chat/thương lượng, báo giá/đơn, đánh giá/hỗ trợ; domain `MuaBanRepository` nối Retrofit `MuaBanApi` và authenticated client hiện có. `MuaBanApp` nối chi tiết catalog, bottom bar, Cá nhân và Admin; polling foreground theo lifecycle, không tự sinh dữ liệu đơn/timeline. Reset state và cancel request khi đổi user; giữ GUID request khi thử lại và rowversion server cho các chuyển trạng thái.

`feature/exchange/TraoDoiViewModel` giữ luồng tin trao đổi, đề nghị/phiên bản/consent, giao nhận và đánh giá. Repository tái sử dụng catalog cho tin/ảnh và `/api/traodoi` cho nghiệp vụ. Chat mở module commerce theo hội thoại giao dịch riêng; đóng chat trở lại đề nghị. Polling chi tiết theo lifecycle, hủy job và xóa state khi đổi tài khoản; lỗi stale tải lại dữ liệu trước khi người dùng xác nhận lại.

## Backend

| Project | Trách nhiệm | Tham chiếu |
| --- | --- | --- |
| Domain | 48 entity, interface chung, enum trạng thái; không phụ thuộc EF/HTTP | Không có project khác |
| Application | DTO/hợp đồng tài khoản, catalog, mua bán, trao đổi, đồ thị/lời mời/phiên bản Smart Swap và hệ thống | Domain |
| Infrastructure | 48 entity configurations, DbContext/11 migrations/seed, tài khoản, catalog, mua bán, trao đổi, Smart Swap và quản trị | Application, EF SQL Server |
| Api | Composition root, JWT/role, rate limiting, controllers, Swagger, health | Infrastructure |
| DatabaseChecks | Kiểm tra SQL thật, HTTP/SQL tài khoản, tin đăng, mua bán, trao đổi và Smart Swap qua ASP.NET TestServer; oracle đồ thị; xuất từ điển EF | Infrastructure, Api |

Không có service/repository rỗng cho mọi bảng chỉ để tạo cảm giác đã có chức năng. Ở các bước tiếp theo, use case nhận request → kiểm tra quyền/quy tắc → transaction EF → phản hồi DTO; entity không được expose trực tiếp cho API ghi.

## Quyết định dữ liệu

- Giao dịch chung (`GiaoDich`) có chi tiết mua bán/1–1/3 bên riêng và danh sách participant chuẩn. Chat, review, complaint và tiền bù dùng FK về cùng participant.
- `SanPham` là snapshot nguồn của món đồ do người đăng sở hữu; `TinDang` là vòng đời xuất bản. FK chủ sở hữu giữ nguyên nguồn lịch sử. Khi tái đăng món đã nhận, cần tạo bản ghi nguồn mới/thông tin liên kết lịch sử; không đổi `ChuSoHuuId` của bản ghi đang được lịch sử giao dịch tham chiếu.
- Phiên bản thỏa thuận và consent được lưu riêng. `PhienBan` byte[] trên mỗi entity là SQL rowversion phục vụ concurrency, khác với số phiên bản nghiệp vụ `SoPhienBan`.
- Smart Swap có ba thành viên, ba cạnh giao và từng xác nhận giao/nhận, không lưu một boolean completed cho cả nhóm. Tiền bù là các khoản từ người trả tới người nhận, ghi nhận trả trực tiếp.
- FK delete là NO ACTION; lịch sử không tự mất theo user/tin. Xóa tài khoản, retention, anonymization và state transition API còn phải được đặc tả/triển khai.
- Tin hỗn hợp `CaHai` chọn giới hạn ảnh bảo thủ 6 để đáp ứng yêu cầu trao đổi. Đây là quyết định khởi tạo bổ sung, vì Figma chỉ nêu bán 8/trao đổi 6 riêng biệt.

## Ranh giới còn lại

Xác thực JWT/session, USER/ADMIN, tài khoản bị khóa, hồ sơ và upload avatar có API cùng kiểm thử SQL/HTTP/Android; xem [ACCOUNTS.md](ACCOUNTS.md). Quản trị đã có API và Android, gồm xử lý/khôi phục tranh chấp, quản lý người dùng/tin/danh mục, giao dịch và báo cáo SQL; xem [ADMIN.md](ADMIN.md). Phần còn lại gồm realtime và xác minh liên hệ. Trigger bảo vệ dữ liệu không thay toàn bộ workflow service.

Catalog/tin đăng cũng đã có API và Android; xem [CATALOG.md](CATALOG.md). Tin tạo nháp riêng tư, duyệt mới hiển thị, chỉnh nội dung tin đã duyệt phải duyệt lại. Xóa mềm giữ FK lịch sử. Luồng này từ chối sửa nguồn sản phẩm đã được giao dịch tham chiếu và chặn các thao tác với sản phẩm chưa giải phóng giữ chỗ.

Mua bán có khóa giữ một món dùng chung catalog, tạo đơn và snapshot giá nguyên tử; worker giải phóng đơn chưa trả hết hạn, thanh toán/hoàn/giải ngân mô phỏng, lịch sử vận chuyển/xác nhận nhận hàng, đánh giá và ticket USER/ADMIN. Xem [COMMERCE.md](COMMERCE.md). Không có chuyển tiền hay giao vận thật; bù tiền trao đổi không tự chuyển thành payment qua ứng dụng.

Trao đổi 1–1 khóa các sản phẩm rồi tin theo thứ tự ổn định, sau đó khóa đề nghị; chỉ khi cả hai consent cùng phiên bản mới giữ hai món. Counter đóng phiên bản trước, tạo bản mới/reset consent nguyên tử và chỉ cho phép trước bàn giao. Counter sau chấp nhận giữ nguyên tồn kho; hủy/từ chối/hết hạn giải phóng đúng giao dịch. Hai phía nhận đủ và xác nhận tiền bù mới hoàn tất, tin chuyển `DaTraoDoi`; review và bằng chứng giữ FK lịch sử. GET sản phẩm không tạo giao dịch. Catalog dùng ReadCommitted cùng khóa tồn kho và rowversion; cập nhật cạnh tranh không dùng read lock kéo dài rồi nâng cấp. Xem [EXCHANGE.md](EXCHANGE.md).

Tìm Smart Swap đặt bộ dựng/duyệt đồ thị thuần trong Application, không phụ thuộc EF hoặc vòng đã lưu. Infrastructure truy vấn tập tin/nhu cầu công khai khả dụng, lọc danh mục/owner/hold, rồi đưa projection vào bộ tìm. Cạnh theo chiều giao món; vòng đúng3 chủ sở hữu/sản phẩm, loại xoay trùng, cân đối tiền bù theo net từng người và cap tổng trả. Điểm/xếp hạng/phân trang có thứ tự ổn định. API tìm kiếm chỉ đọc; giới hạn tài nguyên được báo422 rõ ràng thay vì trả thiếu kết quả, cancellation và rate limit riêng. Xem [SMART_SWAP_SEARCH.md](SMART_SWAP_SEARCH.md).

Lời mời Smart Swap tái kiểm tra đúng3 tin bằng thuật toán trong transaction, tạo root ChoDongY/ba participant/ba cạnh/chat nhóm và bản tiền bù. Xác nhận cuối cùng giữ3 món và chốt đủ3/3 nguyên tử. Counter chỉ thay khoản có liên quan người đề xuất, append phiên bản và reset cả3 consent; giữ hold nếu đã chấp nhận, khóa bàn giao đến đồng thuận mới. Từ chối/hết hạn/hủy nhả đúng hold; worker bảo toàn tranh chấp/legacy. Dùng chung thứ tự khóa sản phẩm → tin → vòng → chat với mua bán/1–1; key/hash/unique bảo vệ retry. Tin chat nullable FK tới phiên bản cho phép mở lịch sử có cấu trúc. Xem [SMART_SWAP_WORKFLOW.md](SMART_SWAP_WORKFLOW.md).

Giao nhận đóng thông tin từng thành viên vào3 vận đơn mô phỏng theo cạnh. Mở rộng bảng vận chuyển hiện có với đúng một nguồn DonHang hoặc ChangTraoDoi, tái sử dụng lịch sử. Sender tự xác nhận gửi; Receiver được chỉ định tự xác nhận kiểm tra đúng hàng và tiền bù của mình. Ba nhãn DaGiao không sinh receipt người nhận. Mốc2/3 giữ root đang thực hiện;3/3 mới hoàn tất/giải phóng hold. KhieuNai theo vận đơn chuyển root TranhChap/vòng TamDung, khóa mọi tiến độ tiếp theo; không tự xử lý sự cố. Chat tiếp tục hoạt động. Demo seed riêng qua service giữ một vòng2/3. Xem [SMART_SWAP_DELIVERY.md](SMART_SWAP_DELIVERY.md).

Android Smart Swap dùng ViewModel/StateFlow và Repository/Retrofit trong `feature/smartswap`, dùng lại JWT/DataStore/Coil và design system hiện có. Tìm/xem trước không tạo vòng, ba người xác nhận theo phiên bản, sheet tiền bù giữ mã cơ sở chống ghi đè, cập nhật chi tiết/chat/bàn giao khi STARTED. Nhận đủ3/3 mới mở hai form đánh giá; Backend lưu hai đánh giá nguyên tử với đúng hai đối tác. Bằng chứng gắn xác nhận của chính người tải lên. Dùng lại bảng/khóa ngoại hiện có, không thêm migration. Xem [SMART_SWAP_ANDROID.md](SMART_SWAP_ANDROID.md) và [kiểm thử Emulator](SMART_SWAP_ANDROID_VALIDATION.md).

## Nguồn kỹ thuật

- [Android built-in Kotlin](https://developer.android.com/build/migrate-to-built-in-kotlin) và [Compose compiler](https://kotlinlang.org/docs/compose-compiler-migration-guide.html): cách cấu hình plugin Android/Compose.
- [EF Core SQL Server provider](https://learn.microsoft.com/en-us/ef/core/providers/sql-server/): provider dùng cho DbContext.
- [EF Core migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/): schema được quản lý bằng migrations, không bằng EnsureCreated.

Nguồn yêu cầu nghiệp vụ là [FIGMA_ANALYSIS.md](../FIGMA_ANALYSIS.md); các lựa chọn kỹ thuật ở tài liệu này là quyết định cho bộ khung mới.
