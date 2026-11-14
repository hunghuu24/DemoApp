# Backend ReMarket

Solution .NET 8 gồm Domain → Application → Infrastructure và Api host. API tham chiếu Infrastructure để đăng ký DbContext. CSDL **SQL Server**, provider/design/tool EF Core **8.0.33**. Các lớp model/configuration có tên tiếng Việt không dấu; dữ liệu text dùng Unicode.

## Chuẩn bị và kết nối

```powershell
. .\scripts\Use-DotNet8.ps1
dotnet --version
dotnet tool restore
```

`global.json` chọn SDK 8.0.4xx với latestPatch. Cài SDK 8 nếu chưa có. Trong phiên khởi tạo đã cài SDK 8.0.427 cục bộ dưới `%LOCALAPPDATA%\ReMarket\dotnet8`; helper hỗ trợ vị trí này mà không sửa PATH hệ thống.

Default trong `src/ReMarket.Api/appsettings.json` và design-time factory:

```text
Server=localhost;Database=ReMarketDemo;Trusted_Connection=True;TrustServerCertificate=True
```

Đây là kết nối máy phát triển. Để dùng instance khác, thiết lập cùng biến môi trường cho CLI migration, seed, API và checks:

```powershell
$env:ConnectionStrings__ReMarket = 'Server=.\SQLEXPRESS;Database=ReMarketDemo;Trusted_Connection=True;TrustServerCertificate=True'
```

Không ghi SQL password vào repo. Với môi trường thật, cấu hình connection string riêng và chứng thư SQL phù hợp; `TrustServerCertificate=True` chỉ là cấu hình demo cục bộ. Biến môi trường giúp `ReMarketDbContextFactory` và API cùng dùng một đích; user-secrets chỉ được host API đọc, nên nếu dùng user-secrets vẫn cần cung cấp cấu hình tương ứng cho design-time CLI.

## Migration và dữ liệu mẫu

```powershell
.\scripts\Initialize-Database.ps1 -SeedDemo
```

Hoặc dùng lệnh riêng:

```powershell
dotnet ef database update --project src/ReMarket.Infrastructure --startup-project src/ReMarket.Api
dotnet run --project src/ReMarket.Api -- --seed-demo
dotnet run --project src/ReMarket.Api
```

Normal startup **không tự migrate/seed**. Tham số `--migrate` và `--seed-demo` chỉ hoạt động ở Development, phải gọi rõ ràng. Khi không dùng launch profile cần đặt `ASPNETCORE_ENVIRONMENT=Development` trước lệnh demo. Seeder dùng transaction + application lock và marker v1; chạy lại không thêm bản ghi hoặc đổi mật khẩu dữ liệu hiện hữu. Không seed nếu migration còn thiếu; không gọi `EnsureCreated` để thay migration.

Migrations đã có:

1. `KhoiTaoCoSoDuLieu`: 43 bảng, khóa/index/CHECK/collation/rowversion.
2. `ThemRangBuocNghiepVu`: 17 trigger theo tập bản ghi, có Down để gỡ trigger.
3. `TaiKhoanJwtVaKhoiPhuc`: phiên đăng nhập, mã khôi phục mật khẩu và role USER/ADMIN; giữ dữ liệu tài khoản cũ.
4. `TinDangMuaBan`: thời điểm cập nhật/xóa mềm, trường dự phòng lượt xem và index truy vấn tin.
5. `MuaBanThuongLuongVaHoTro`: phiếu thương lượng, lịch sử hỗ trợ, snapshot giảm giá, key gửi lại, mốc thanh toán/hủy và trạng thái giữ chỗ; giữ dữ liệu cũ.
6. `ThuTuLichSuMoPhong`: identity thứ tự lịch sử vận chuyển/hỗ trợ khi nhiều sự kiện cùng giây.
7. `TraoDoiMotMot`: điều kiện 1–1, liên kết đề nghị với đúng cặp tin–sản phẩm, key gửi lại và ảnh bằng chứng, trạng thái đã trao đổi, tiêu chí đánh giá tham chiếu; giữ lịch sử cũ.
8. `TieuChiTimSmartSwap`: từ khóa, tình trạng tối thiểu và khoảng giá muốn nhận nullable cùng CHECK SQL; tái sử dụng danh mục muốn nhận, giữ nguyên dữ liệu cũ.
9. `LoiMoiVaThuongLuongSmartSwap`: metadata lời mời/key/hash/creator, liên kết đúng tin–sản phẩm, key phiên bản tiền bù và tin chat cấu trúc; dữ liệu cũ nullable, giữ lịch sử.
10. `GiaoNhanSmartSwap`: thông tin đã xác nhận, nguồn vận chuyển đơn/chặng, snapshot/mã tự nhập/nhận đúng, sự cố và5 trigger giao nhận; giữ dữ liệu mua bán cũ.

Sau khi sửa model/configuration ở bước tiếp theo:

```powershell
dotnet ef migrations add TenThayDoi --project src/ReMarket.Infrastructure --startup-project src/ReMarket.Api --output-dir Persistence/Migrations
dotnet ef migrations has-pending-model-changes --project src/ReMarket.Infrastructure --startup-project src/ReMarket.Api
.\scripts\Export-Schema.ps1
```

[remarket-schema.sql](../docs/database/remarket-schema.sql) là script idempotent xuất từ migrations. Export helper thêm các SET option cần cho computed column và filtered index; CREATE TRIGGER được bọc EXEC để dùng được trong script idempotent. Khi chạy SQL thủ công, dùng database riêng đã tạo, không sửa script để ghi vào DB đang chứa dữ liệu khác.

```powershell
sqlcmd -S localhost -E -C -b -d ReMarketDemo -i ..\docs\database\remarket-schema.sql
```

## API nền tảng

- `GET /health/live`: host đang chạy.
- `GET /health/ready`: 200 khi DB truy cập được và schema đã có đủ migrations; 503 nếu chưa sẵn sàng.
- `GET /api/hethong/trangthai`: DTO camelCase tương thích Retrofit Android.
- `/swagger` chỉ Development; `/demo/san-pham.png` là placeholder local dùng trong seed.

Đã có API tài khoản, catalog, [mua bán](../docs/COMMERCE.md), [trao đổi1–1](../docs/EXCHANGE.md), [tìm vòng](../docs/SMART_SWAP_SEARCH.md), [lời mời/thương lượng](../docs/SMART_SWAP_WORKFLOW.md) và [giao nhận Smart Swap](../docs/SMART_SWAP_DELIVERY.md). Ba vận đơn mô phỏng gắn3 cạnh; cập nhật vận chuyển không tự xác nhận người nhận. Đủ3 người nhận đúng mới hoàn tất; sự cố tạm dừng vòng và giữ hold. Có48 bảng,100 FK,70 CHECK,22 trigger. Không gọi gateway/ngân hàng hoặc giao vận thật. Chat dùng API/polling, chưa WebSocket.

## Xác minh

```powershell
dotnet build ReMarket.sln
dotnet run --project tests/ReMarket.DatabaseChecks
dotnet run --project tests/ReMarket.DatabaseChecks -- --auth
dotnet run --project tests/ReMarket.DatabaseChecks -- --listings
dotnet run --project tests/ReMarket.DatabaseChecks -- --commerce
dotnet run --project tests/ReMarket.DatabaseChecks -- --exchange
dotnet run --project tests/ReMarket.DatabaseChecks -- --smart-swaps
dotnet run --project tests/ReMarket.DatabaseChecks -- --smart-swap-workflow
dotnet run --project tests/ReMarket.DatabaseChecks -- --smart-delivery
dotnet run --project tests/ReMarket.DatabaseChecks -- --export-dictionary ../docs/database/schema-dictionary.json
.\scripts\Verify-FreshDatabase.ps1
```

DatabaseChecks nền tảng chỉ cho DB tên `ReMarketDemo` hoặc tiền tố `ReMarketTest_`, cần dữ liệu demo và không chạy song song với người đang sửa fixture; mutation của nhóm nền tảng được rollback. Các nhóm HTTP/SQL tài khoản, catalog, mua bán, trao đổi và Smart Swap dùng database test ngẫu nhiên riêng, dọn đúng DB/ảnh do chính lượt chạy tạo. Smart Swap còn đối chiếu đồ thị sinh ngẫu nhiên với bộ duyệt vét cạn độc lập. Seeder lặp là no-op. Verify-FreshDatabase chỉ dùng Windows Authentication, tạo tên DB ngẫu nhiên, áp SQL hai lần, seed/check, rồi xóa đúng DB tạm mà nó vừa tạo. `ReMarketDemo` không bị script này xóa.

Demo giao nhận riêng, thực hiện rõ ràng sau migration ở Development:

```powershell
dotnet run --project src/ReMarket.Api -- --seed-smart-delivery-demo
```

Tạo3 USER riêng và vòng2/3 thông qua các service thật; chạy lại giữ tiến độ, không reset. [Tài khoản/demo C xác nhận cuối](../docs/SMART_SWAP_DELIVERY.md). Đọc [thiết kế CSDL](../docs/database/THIET_KE_CSDL.md) trước khi thêm service. CHECK/FK/trigger bổ sung transaction, permission và idempotency.
