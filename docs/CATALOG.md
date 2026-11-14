# ReMarket — Sản phẩm và tin đăng

Tiếp tục trên module Android và solution Backend hiện có. Phần tài khoản/JWT/DataStore được giữ; catalog sử dụng cùng phiên xác thực và ImageLoader Coil. Không có danh sách sản phẩm hoặc thông tin người bán cố định trong mã Android.

## Màn hình và nguồn thiết kế

| Màn hình | Frame đã đọc trong Nhom4.fig | Thực hiện |
| --- | --- | --- |
| Trang chủ | 4:461 | Search, danh mục cuộn ngang, nổi bật, tin mới, điều hướng dưới |
| Tìm kiếm | 4:556 | Từ khóa, danh mục, giá, tình trạng, tỉnh/thành, sắp xếp, phân trang |
| Danh mục | 4:617 | Lưới thích ứng 2/3 cột, số tin tính từ SQL và bao gồm danh mục con |
| Danh sách | 4:684 | Card ảnh/giá/tình trạng/địa điểm; dữ liệu và số lượng từ API |
| Chi tiết | 4:755, 128:6, 128:67 | Pager ảnh, mô tả, giá, danh mục và người đăng thật |
| Đăng bán / chỉnh sửa | 114:2, 114:115 | Ảnh, ảnh bìa, form có validation, cuộn/IME; lưu nháp/thay đổi |
| Xem trước | 114:292 | Bản riêng tư đã lưu trên server, xác nhận gửi duyệt hoặc sửa tiếp |
| Gửi tin thành công | 114:252 | Chờ duyệt, chưa công khai; đi quản lý hoặc đăng tiếp |
| Tin của tôi | 114:332 | Tabs và số lượng SQL: đang bán, chờ duyệt, đã bán, nháp; bổ sung đã ẩn/từ chối |
| Quản lý tin | 114:192 | Sửa, ẩn, gửi duyệt lại, xóa; tải lại khi có thay đổi từ Admin |
| Xác nhận đã bán | 114:66 | Trang xác nhận tác động, xác nhận/hủy |

MCP tiếp tục báo hết quota Starter. Các màn dựa trên node/text/geometry trong bản `.fig` đã giải mã cùng [phân tích](../FIGMA_ANALYSIS.md); chưa có ảnh render Figma trực tuyến để tuyên bố khớp pixel tuyệt đối. Dùng Roboto đóng gói, primary #28734F, background #FCFDFB, lề 24, card radius20, input radius12, CTA50 và bottom bar67. System bars thật thay thanh giả của mockup. Nội dung dài cuộn, grid 2 cột trên điện thoại/3 cột khi đủ rộng; không cố định toàn app ở chiều rộng360.

Sáu icon danh mục lấy **nguyên hình học vector** từ `.fig` (không vẽ lại): [manifest](assets/catalog-icons.json), [script trích xuất](../tools/Export-FigmaCatalogIcons.cjs). Ảnh demo gốc trong `.fig`: `128:56` → `figma-ban.jpg` (1200×896), `128:119` → `figma-ke.jpg` (1200×896), `146:420` → `figma-iphone.png` (430×430), nằm trong `Backend/src/ReMarket.Api/wwwroot/demo`. Seeder chỉ nâng các placeholder nguyên bản theo ID fixture, không thay ảnh người dùng. Danh sách, giá và trạng thái vẫn do SQL quyết định; không ép tin iPhone đã bán hoặc kệ nháp thành công khai để khớp dữ liệu mockup.

## Chạy

```powershell
cd Backend
. .\scripts\Use-DotNet8.ps1
.\scripts\Initialize-Database.ps1 -SeedDemo
dotnet run --project src/ReMarket.Api
```

Migration thứ tư `20261108165930_TinDangMuaBan` thêm `NgayCapNhat`, `NgayXoa`, trường dự phòng `LuotXem` và hai index phục vụ truy vấn. Không xóa/tạo lại DB. Android dùng base URL hiện có `http://10.0.2.2:5080/`; thiết bị thật và HTTPS xem [Android README](../Android/README.md).

## API REST

| Method / URL | Quyền | Chức năng |
| --- | --- | --- |
| GET `/api/tindang/trangchu` | Public | Danh mục, tin nổi bật và mới đăng từ SQL |
| GET `/api/tindang/danhmuc` | Public | Danh mục hiện hành, số tin trong cả nhánh |
| GET `/api/tindang` | Public | Tìm kiếm/lọc/sort/phân trang |
| GET `/api/tindang/{id}` | Public | Chỉ tin đang hiển thị, chủ hoạt động, danh mục khả dụng |
| GET `/api/tindang/cuatoi` | USER | Tin mua bán của subject JWT, thống kê tất cả trạng thái |
| GET `/api/tindang/{id}/quanly` | USER, chủ tin | Detail/preview riêng tư, phiên bản và quyền sửa hiện hành |
| POST `/api/tindang` | USER | Tạo sản phẩm + tin nháp trong một lần SaveChanges nguyên tử; trả201 |
| PUT `/api/tindang/{id}` | USER, chủ tin | Cập nhật cùng aggregate sản phẩm/tin, không nhận owner/status tùy ý |
| POST `/api/tindang/{id}/anh` | USER, chủ tin | Multipart `anh` + `phienBan` |
| DELETE `/api/tindang/{id}/anh/{anhId}` | USER, chủ tin | Body `phienBan`; xóa ảnh và gán lại ảnh bìa nếu cần |
| PUT `/api/tindang/{id}/anh/{anhId}/bia` | USER, chủ tin | Body `phienBan`; thay ảnh bìa |
| GET `/api/tindang/{id}/anh/{anhId}` | Theo khả năng xem tin | Ảnh công khai khi tin công khai; nháp/chờ duyệt/ẩn chỉ chủ hoặc ADMIN |
| POST `/api/tindang/{id}/gui-duyet` | USER, chủ tin | Nháp/từ chối/ẩn → Chờ duyệt; cần ảnh bìa và nội dung hợp lệ |
| POST `/api/tindang/{id}/an` | USER, chủ tin | Đang bán/chờ duyệt → Đã ẩn |
| POST `/api/tindang/{id}/da-ban` | USER, chủ tin | Đang bán/đã ẩn → Đã bán, không tạo đơn hàng giả |
| DELETE `/api/tindang/{id}` | USER, chủ tin | Body `phienBan`; xóa mềm, trả204 |
| GET `/api/admin/tindang` | ADMIN | Hàng chờ duyệt có phân trang |
| POST `/api/admin/tindang/{id}/duyet` | ADMIN | `phienBan`, `chapNhan`, `lyDo`; không tự duyệt tin của mình; ghi nhật ký |
| GET `/api/hopthu` | Đã đăng nhập | Chỉ conversation có membership của user, đọc tối đa50 |

Query tìm kiếm: `tuKhoa`, `danhMucId`, `giaTu`, `giaDen`, `tinhTrang` (`NhuMoi`, `DaSuDungTot`, `CoLoiNhe`), `tinhThanh`, `loaiTin` (`MuaBan`/`TraoDoi`), `sapXep` (`moinhat`, `giatang`, `giagiam`), `trang` (từ1), `kichThuocTrang` (1–50; Android dùng6). Kết quả có `items`, `tongSo`, `trang`, `kichThuocTrang`, `tongTrang`. Sort có ID phụ để ổn định các dòng trùng giá/ngày. Từ khóa tìm trong tên/mô tả, không phân biệt dấu/hoa thường; dùng EF parameter hóa. Danh mục cha ẩn kéo theo ẩn kết quả của nhánh con. Khoảng giá sai và query không hợp lệ trả400.

Body lưu: `ten` (3–200), `danhMucId`, `giaBan` (số nguyên VND 1–1.000 tỷ), `tinhTrang`, `moTa` (10–4000), `tinhThanh` (2–100), `diaDiem` (5–500), `phienBan` (khi sửa). Các chuỗi được trim và kiểm tra lại phía server. Nháp có thể chưa có ảnh; gửi duyệt cần ít nhất một ảnh bìa. Trường ngoài hợp đồng như `nguoiDangId`/`trangThai` bị từ chối bởi JSON binding hiện có.

## Vòng đời và đồng thời

- Tạo → **Bản nháp** riêng tư → tải ảnh → xem trước → gửi → **Chờ duyệt**. ADMIN chấp nhận → **Đang bán**, từ chối → **Từ chối** kèm lý do.
- Sửa nội dung/ảnh của tin đang bán tự chuyển lại **Chờ duyệt**, không giữ dấu duyệt cũ cho nội dung mới. Sửa tin chờ duyệt tiếp tục nằm trong hàng chờ.
- **Ẩn** dừng công khai ngay; gửi hiển thị lại phải qua duyệt. **Đã bán** biến mất khỏi list/detail công khai, còn trong lịch sử chủ tin và không sửa nội dung tiếp.
- **Xóa** đặt `NgayXoa`, ẩn khỏi tất cả list/preview/ảnh HTTP; giữ bản ghi sản phẩm/tin và FK lịch sử. Không xóa dây chuyền đơn/đề nghị.
- Lệnh ghi yêu cầu `phienBan` Base64 rowversion. Transaction có khóa hàng tin và kiểm tra lại phiên bản; yêu cầu cũ nhận409, không ghi đè. UI giữ form khi lỗi và có thao tác tải lại dữ liệu server.
- Sản phẩm giữ chỗ chưa giải phóng không được sửa/ẩn/xóa/đánh dấu bán ngoài giao dịch. Nội dung đã được đơn hàng/đề nghị/vòng tham chiếu không được sửa để bảo toàn lịch sử.
- API mới tạo/quản lý tin **MuaBan**. Tin hỗn hợp/trao đổi nền tảng vẫn đọc được, không bị tự chuyển sang luồng mua bán khi chỉnh sửa.

## Ảnh và lưu trữ

Upload tối đa8 ảnh/tin mua bán, mỗi ảnh5MB; JPG/PNG phải decode thành công, cạnh tối đa6000 và tối đa16 megapixel. Server sửa hướng EXIF, giữ tỷ lệ, thu cạnh dài về tối đa1600, encode JPEG và bỏ metadata. Không tin tên file/MIME do client gửi; tên lưu là GUID. Thay bìa giải quyết filtered unique index trong transaction. Upload thất bại/phiên bản cũ dọn file mới; xóa ảnh thành công dọn file tương ứng sau commit.

File người dùng ở `Backend/.local/products`, ngoài wwwroot/Git. Có thể cấu hình `Media:ProductFolder` qua env `Media__ProductFolder`; sao lưu thư mục này cùng DB khi chuyển môi trường. Endpoint ảnh kiểm tra trạng thái tin/chủ/quyền ADMIN và trả `private, no-store`/`nosniff`. Chỉ bốn file fixture demo được phép ánh xạ vào wwwroot; không nhận đường dẫn tùy ý. Xóa mềm giữ file để bảo toàn lịch sử; ảnh của tin đã xóa không truy cập qua endpoint. Chưa có job thu gom file mồ côi sau khi tiến trình bị dừng đột ngột.

Android đọc file Photo Picker có giới hạn dung lượng, gửi từng ảnh và giữ lại các ảnh chưa gửi khi lỗi. Retry không tải lại những ảnh vừa thành công. Đổi bìa/xóa ảnh sử dụng phiên bản mới nhất trả từ server. Coil hiển thị loading, lỗi ảnh và chạm tải lại.

## Điều hướng và phạm vi

Bottom Navigation: Trang chủ tải lại catalog; Trao đổi đọc danh sách `loaiTin=TraoDoi`; Đăng bán mở form; Tin nhắn đọc inbox riêng; Cá nhân mở cài đặt cũ. Tin của tôi đi từ Home hoặc Cài đặt. Có Back giữa danh mục → list → detail, form → preview → success và quản lý → xác nhận đã bán. Đăng xuất xóa cả state catalog để không giữ dữ liệu riêng của tài khoản trước.

Bổ sung trang duyệt tin tối thiểu trên Android ADMIN để demo được vòng gửi–duyệt–hiển thị bằng API thật. Luồng chat realtime, tạo đơn mua hàng và giao dịch trao đổi chưa thuộc bước này; nút Liên hệ/Mua ngay thông báo phạm vi hiện tại, không tạo thành công giả. UI không hiển thị lượt lưu/lượt xem giả; `LuotXem` mới là trường dự phòng, chưa có analytics. Đổi mật khẩu, khôi phục, hồ sơ và logout tiếp tục theo [ACCOUNTS.md](ACCOUNTS.md).

## Kiểm thử

```powershell
cd Backend
. .\scripts\Use-DotNet8.ps1
dotnet run --project tests/ReMarket.DatabaseChecks -- --listings
dotnet run --project tests/ReMarket.DatabaseChecks -- --auth
dotnet run --project tests/ReMarket.DatabaseChecks
```

Kiểm tra tin đăng tạo DB SQL Server tạm `ReMarketTest_GUID`, áp migration thật, đăng ký/đăng nhập thật, gọi HTTP pipeline và dọn DB/file riêng của lần chạy. Instrumentation `SanPhamEndToEndTest` nhận credential ADMIN đã cấp quyền như hướng dẫn tài khoản; dùng cùng APK/SQL/API. Callback ảnh được đưa byte PNG kiểm thử vào ViewModel để không phụ thuộc giao diện Photo Picker của từng OS; không fake Retrofit/repository hoặc dữ liệu trả từ server. Test tạo và xóa mềm đúng ID tin của chính nó qua API; tài khoản QA vẫn giữ trong DB demo.

Kết quả, ảnh thực tế và các giới hạn xác minh: [CATALOG_VALIDATION.md](CATALOG_VALIDATION.md).
