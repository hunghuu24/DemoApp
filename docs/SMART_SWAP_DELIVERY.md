# Giao nhận Smart Swap 3 bên

Cập nhật 12/11/2026. Backend tiếp nối [lời mời/đồng thuận/tiền bù](SMART_SWAP_WORKFLOW.md). Chat nhóm3 người, tin nhắn và thông báo SQL đã có được tái sử dụng; bổ sung thông tin giao nhận, ba vận đơn mô phỏng, tiến độ nhận đúng hàng, hoàn tất và báo sự cố. Android giữ nguyên ở giai đoạn API này.

## Luồng thực hiện

1. Vòng đã có3/3 consent cùng bản DaChot, root DangThucHien,3 sản phẩm đang giữ.
2. Mỗi người tự xác nhận tên liên hệ, điện thoại, địa chỉ gửi và địa chỉ nhận cho đúng bản thỏa thuận. Người khởi tạo không nhập thay hai người còn lại. Thiếu một người thì không tạo vận đơn.
3. Một participant gọi tạo vận chuyển. Trong một transaction, tạo đủ3 dòng VanChuyenMoPhong liên kết3 ChangTraoDoi của cùng vòng và chụp thông tin liên hệ. Không tạo đơn mua bán giả để gắn vận chuyển.
4. Người gửi của từng chặng nhập mã vận đơn và chuyển trạng thái theo thứ tự: **ChoGui → DaGui → DangVanChuyen → DaGiao**. Đây là thao tác mô phỏng do người gửi thực hiện, không có hãng vận chuyển thật.
5. Khi chuyển DaGui, người gửi phải xác nhận rõ đã gửi. Server ghi receipt DaGiao của chính người gửi; không ghi receipt người nhận.
6. Khi vận chuyển báo DaGiao, đúng người nhận của chặng tự kiểm tra hàng rồi gọi nhận đúng hàng. Nhãn vận chuyển DaGiao không được tính thành xác nhận của người nhận.
7. Tiến độ0/3 →1/3 →2/3 vẫn chưa hoàn tất. Chỉ người nhận thứ ba xác nhận đúng hàng, đủ tiền bù đã chốt nếu có và không có sự cố mới hoàn tất vòng/root, đánh dấu các tin DaTraoDoi và nhả3 hold.

| Trạng thái | Vòng | Root | Tiến độ |
| --- | --- | --- | --- |
| Đủ đồng thuận, chờ thông tin | SanSangBanGiao | DangThucHien | 0/3 |
| Tạo đủ ba vận đơn | DangBanGiao | DangThucHien | 0/3 |
| Ba vận chuyển đều DaGiao nhưng chưa kiểm tra | DangBanGiao | DangThucHien | 0/3 |
| Một/hai người nhận đúng hàng | DangBanGiao | DangThucHien | 1/3 hoặc2/3 |
| Đủ ba người nhận đúng hàng | HoanTat | HoanTat | 3/3 |
| Báo sự cố đang mở | TamDung | TranhChap | Giữ nguyên tiến độ trước sự cố |

Không có worker tự nâng trạng thái vận chuyển, tự nhận thay hoặc tự hoàn tất. Mốc ngày giao dự kiến chỉ là metadata mô phỏng. Mã nội bộ `maMoPhong` do hệ thống sinh phân biệt với `maVanDon` do người gửi nhập; mã người gửi nhập là duy nhất,4–40 ký tự chữ/số/`-`/`_`, chuẩn hóa chữ hoa và không đổi sau DaGui.

## API

JWT/session USER bắt buộc. Mỗi `{id}` là ID vòng đã gửi; `{shipmentId}` phải thuộc chính vòng đó.

| Method/route | Nội dung |
| --- | --- |
| GET `/api/smart-swaps/{id}/delivery` | Ba thông tin xác nhận, ba chặng, lịch sử vận chuyển, sự cố, tiến độ và quyền thao tác |
| PUT `/api/smart-swaps/{id}/delivery/contact` | Chính actor xác nhận/cập nhật thông tin trước khi đóng vận đơn |
| POST `/api/smart-swaps/{id}/shipments` | Tạo đủ3 vận đơn; retry trả cùng bộ3 dòng |
| PUT `/api/smart-swaps/{id}/shipments/{shipmentId}/tracking` | Người gửi nhập/cập nhật mã trước khi gửi |
| POST `/api/smart-swaps/{id}/shipments/{shipmentId}/status` | Người gửi cập nhật mô phỏng, không bỏ bước/lùi bước |
| POST `/api/smart-swaps/{id}/shipments/{shipmentId}/received` | Chỉ người nhận được chỉ định xác nhận kiểm tra đúng hàng |
| POST `/api/smart-swaps/{id}/issues` | Người nhận báo SaiHang hoặc ChuaNhanHang và tạm dừng cả vòng |

`GET /api/smart-swaps/{id}` cũng có `tienDoGiaoNhan` gồm số đã nhận đúng/tổng3/chuỗi0/3…3/3/cờ tạm dừng/hoàn tất. Danh sách vòng hỗ trợ thêm `tab=tamdung`, `tab=hoantat`; tab chinhthuc gồm cả vòng đang giao nhận.

Body xác nhận thông tin (thay placeholder bằng ID/key thật):

```json
{
  "maYeuCau": "<GUID mới>",
  "phienBanThoaThuanId": "<ID bản hiện tại đã chốt>",
  "tenLienHe": "Người liên hệ demo",
  "soDienThoai": "0901234567",
  "diaChiGui": "123 Đường Gửi, Hà Nội",
  "diaChiNhan": "456 Đường Nhận, Hà Nội",
  "xacNhanThongTin": true,
  "phienBan": null
}
```

Điện thoại Việt Nam được chuẩn hóa; tên tối thiểu2 ký tự, địa chỉ tối thiểu10. Lần đầu phienBan NULL; khi sửa dùng rowversion thông tin của chính mình từ GET delivery. Retry giữ cùng maYeuCau/nội dung, không nhân bản; đổi nội dung cùng key409. Một phiên bản thương lượng mới làm xác nhận thông tin bản cũ hết hiệu lực; cả ba cần chốt/kiểm tra thông tin lại. Địa chỉ được đóng vào vận đơn khi tạo, sau đó không sửa được.

Tạo vận chuyển:

```json
{ "phienBanThoaThuanId": "<ID bản đã chốt>" }
```

Nhập mã:

```json
{ "phienBan": "<rowversion vận đơn>", "maVanDon": "DEMO-SS-0001" }
```

Gửi/cập nhật vận chuyển:

```json
{ "phienBan": "<rowversion mới nhất>", "trangThai": "DaGui", "xacNhanDaGui": true }
```

Hai bước sau dùng trangThai DangVanChuyen rồi DaGiao. Lặp lại đúng trạng thái không thêm lịch sử/receipt. State/range/version sai409/400, người không phải sender403. Source của vận đơn, bản thỏa thuận và địa chỉ snapshot không được đổi.

Nhận hàng:

```json
{
  "phienBanThoaThuanId": "<ID bản hiện tại>",
  "daKiemTraHang": true,
  "nhanDungHang": true,
  "daXacNhanTienBuCuaToi": true
}
```

Actor lấy từ JWT, người nhận lấy từ cạnh đã lưu; không có trường người xác nhận trong body. Sender, thành viên thứ ba hoặc ADMIN ngoài vòng không được nhận thay. Chỉ nhận sau nhãn DaGiao. Hai cờ kiểm tra/đúng hàng phải true; sai hàng/chưa nhận dùng API issues, không biến thành receipt thành công.

Nếu actor có khoản bù trong bản đã chốt, cần tự xác nhận đã trả/nhận đủ phần mình. Chỉ cập nhật cờ trả/nhận thuộc actor đó; không xác nhận khoản người khác và không gọi ngân hàng/ví. Vòng không có khoản bù không yêu cầu cờ này. Xác nhận lặp cùng chặng/bản kể cả sau hoàn tất trả tiến độ hiện tại, không nhân receipt/thông báo.

## Sự cố và quyền riêng tư

```json
{
  "maYeuCau": "<GUID mới>",
  "vanChuyenId": "<vận đơn mình là người nhận>",
  "loaiSuCo": "SaiHang",
  "tieuDe": "Sản phẩm nhận không đúng mô tả",
  "noiDung": "Tôi nhận một sản phẩm khác với sản phẩm đã chốt trong vòng."
}
```

Hai loại: SaiHang sau khi có xác nhận gửi; ChuaNhanHang có thể báo khi chờ gửi hoặc đang vận chuyển/đã báo giao nhưng thực tế chưa nhận. Chỉ người nhận được chỉ định được báo chặng mình; không báo chặng của hai người khác. Ghi KhieuNai gắn đúng vận đơn, người báo, người gửi và giao dịch; key/hash chống gửi lặp. Từ thời điểm báo, cùng transaction đặt Vong TamDung/root TranhChap, thông báo cả3 và ghi sự kiện chat.

Tạm dừng chặn nhập mã, cập nhật vận chuyển, nhận đúng hàng, thương lượng/hủy và hoàn tất. Giữ nguyên receipt đã có và3 hold để xử lý tranh chấp; chat nhóm/thông báo vẫn dùng được. Không tự bỏ qua hoặc tự đóng sự cố. Quy trình quản trị giải quyết/khôi phục tranh chấp và hỗ trợ sau khi người dùng đã xác nhận đúng/đã hoàn tất là bước riêng, chưa thêm endpoint mở lại ở giai đoạn này.

GET delivery: actor chỉ thấy đầy đủ thông tin liên hệ do chính mình xác nhận; contact của người khác chỉ có tên/trạng thái. Địa chỉ/điện thoại snapshot của một vận đơn chỉ hiện cho sender/receiver chặng đó; thành viên thứ ba thấy tiến độ/mã/trạng thái nhưng không có địa chỉ riêng không cần thiết. Người ngoài vòng404. Tin trạng thái trong chat/thông báo không chép địa chỉ/điện thoại.

## Transaction và CSDL

Tái sử dụng thứ tự khóa **sản phẩm → tin → vòng → chat** từ workflow trước. Tạo3 vận đơn, trạng thái, receipt/tiền bù/hoàn tất và báo sự cố đều nguyên tử. Race xác nhận cuối với báo sự cố được tuần tự hóa: một bên thành công, bên còn lại409; không có vòng vừa hoàn tất vừa tạm dừng.

Migration10 `20261112080514_GiaoNhanSmartSwap`:

- Thêm ThongTinGiaoNhanSmartSwap, một dòng/ThanhVienVong, bản thỏa thuận, địa chỉ đã xác nhận và key/hash/rowversion.
- VanChuyenMoPhong.DonHangId trở thành nullable; thêm ChangTraoDoiId/PhienBanThoaThuanId, tracking tự nhập, snapshot contact và NgayNhanDungHang. CHECK bắt buộc đúng một nguồn: đơn mua bán hoặc chặng Smart Swap. Unique theo nguồn giữ mỗi chặng đúng1 vận đơn. Luồng mua bán cũ giữ nguyên.
- Tái sử dụng LichSuVanChuyen, bổ sung enum ChoGui/DaGui.
- KhieuNai thêm nguồn vận đơn/loại sự cố/key/hash; vòng có trạng thái TamDung.
- Năm trigger mới bảo vệ thông tin đúng vòng/bản, nguồn vận đơn, receipt đúng trạng thái/người nhận, hoàn tất đủ3 và quan hệ báo cáo sự cố. Triggers6/6 trước đây tiếp tục bảo vệ3 lần gửi +3 lần nhận; tiến độ UI/API chỉ đếm3 receipt DaNhan.

Sau xác nhận cuối, transaction lưu receipt thứ ba/tiền bù của actor, đánh dấu3 tin DaTraoDoi, nhả hold, chuyển vòng HoanTat rồi root HoanTat/lưu NgayHoanTat. Mới2/3 không chạy các bước kết thúc này. Rollback migration có guard khi đã có vận đơn Smart Swap để không tự xóa lịch sử mới.

## Demo có sẵn

```powershell
cd Backend
. .\scripts\Use-DotNet8.ps1
.\scripts\Initialize-Database.ps1
dotnet run --project src/ReMarket.Api -- --seed-smart-delivery-demo
dotnet run --project src/ReMarket.Api
```

Seed chỉ chạy rõ ràng ở Development, dùng thuật toán thật để tìm vòng trong ba tin demo và các service thật để đi qua đồng thuận/giao nhận. Không ghi đè fixture cũ; chạy lại giữ ID/tiến độ, không reset mật khẩu, vận đơn hoặc vòng đã hoàn tất.

| Tài khoản USER | Demo |
| --- | --- |
| swap-ship-a@remarket.example.test | Có một chặng gửi và một chặng nhận; đã nhận đúng |
| swap-ship-b@remarket.example.test | Có một chặng gửi và một chặng nhận; đã nhận đúng |
| swap-ship-c@remarket.example.test | Chặng nhận đã báo DaGiao, chưa được người này xác nhận |

Mật khẩu lúc seed lần đầu mặc định `ReMarketDemo!2026`, hoặc giá trị `REMARKET_DEMO_PASSWORD` lúc seed. Đăng nhập thật qua API/PasswordHasher/JWT, không có nhánh bypass trong ứng dụng. Ảnh xanh là placeholder fixture demo, không phải ảnh sản phẩm của người dùng.

Vòng đã tạo trên ReMarketDemo trong phiên này: **eb1cabbf-ec78-4ab9-99e4-d504b7db537b**,3 vận đơn, tiến độ **2/3**, root DangThucHien. Đăng nhập C, GET delivery, chọn vận đơn có nguoiNhanId bằng ID hồ sơ C và POST received với bản hiện tại/các xác nhận. Lúc đó mới3/3 HoanTat. ID có thể khác trên database mới; CLI in ID thật và GET danh sách vòng của tài khoản demo tìm được vòng đó.

```powershell
dotnet run --project tests/ReMarket.DatabaseChecks -- --smart-delivery
```

[Báo cáo kiểm thử](SMART_SWAP_DELIVERY_VALIDATION.md), [CSDL](database/THIET_KE_CSDL.md), [API controller](../Backend/src/ReMarket.Api/Controllers/VongSmartSwapController.cs), [service giao nhận](../Backend/src/ReMarket.Infrastructure/SmartSwap/SmartSwapDelivery.cs).
