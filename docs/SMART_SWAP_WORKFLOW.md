# Lời mời và thương lượng Smart Swap 3 bên

Cập nhật 12/11/2026. Backend dùng [thuật toán tìm vòng đã có](SMART_SWAP_SEARCH.md), JWT/session/role USER và SQL Server hiện có. Thêm API xem phương án, gửi lời mời, phản hồi, thương lượng, lịch sử, chat nhóm và thông báo. Android giữ nguyên ở bước này.

## Luồng và trạng thái

1. `GET /api/smart-swaps/search` lấy vòng từ tin đang khả dụng.
2. `POST /api/smart-swaps/preview` với `maDeXuat` từ kết quả tìm. Server đọc lại đúng ba tin, chạy lại thuật toán, trả ba hồ sơ, chiều giao, giá trị, tiền bù và `bamPhuongAn`. Preview không tạo vòng/giao dịch/hold/chat.
3. Thành viên có đúng một món trong vòng gửi lời mời, kèm hash và `dongYPhuongAn:true`. Server khóa ba sản phẩm theo thứ tự, kiểm tra lại dữ liệu/nhu cầu/phiên bản/hash. Không tin giá hoặc danh sách khoản bù do client tự đặt trong lời mời.
4. Lưu vòng, ba thành viên/ba cạnh, phiên bản1, ba consent và chat nhóm. Consent người khởi tạo là DongY vì họ đã xác nhận rõ lúc gửi; hai người kia là ChoPhanHoi. Hai người được mời nhận thông báo lưu SQL.
5. Hai người còn lại đọc phương án, gửi phản hồi cho đúng ID phiên bản. Chỉ đủ3/3 mới chốt phiên bản, giữ cả ba món và bắt đầu giao dịch chính thức.
6. Một người từ chối hoặc phương án chờ quá24 giờ: hủy cả vòng; không tự quay về phương án cũ. Nếu đang thương lượng lại có hold, chỉ giải phóng hold của chính vòng này.

| Giai đoạn | VongSmartSwap | GiaoDich | Consent hiện tại | Hold |
| --- | --- | --- | --- | --- |
| Preview | Chưa tạo | Chưa tạo | Chưa có | Không có |
| Gửi lời mời | ChoDongY | ChoDongY | 1 đồng ý +2 chờ | Không có |
| Một người được mời đồng ý | ChoDongY | ChoDongY | 2 đồng ý +1 chờ | Không có |
| Đủ3/3 | SanSangBanGiao | DangThucHien | 3 đồng ý, bản DaChot | Đủ3 món |
| Phương án mới sau chốt | ChoDongY | ChoDongY | Cả3 về ChoPhanHoi | Giữ nguyên3 món |
| Phương án mới trước chốt | ChoDongY | ChoDongY | Cả3 về ChoPhanHoi | Chưa giữ |
| Từ chối/hết hạn/hủy | DaHuy | DaHuy | Giữ lịch sử | Giải phóng đúng hold của vòng |

Hồ sơ `GiaoDich` được tạo ở dạng **chờ đồng ý** khi gửi lời mời để liên kết participant, lịch sử và chat. Nó chỉ trở thành giao dịch chính thức `DangThucHien` khi cùng một phiên bản có đủ3 xác nhận. Không coi một dòng GiaoDich đang chờ là giao dịch đã bắt đầu.

`maKetThuc` phân biệt `TuChoi`, `HetHan`, `DaHuy`; `lyDoKetThuc` lưu lý do. Các enum gốc giữ tương thích dữ liệu trước đây. `ngayChapNhan` là thời điểm phiên bản hiện tại được đủ ba người chấp nhận; khi thương lượng lại, trường này về NULL, thời gian phản hồi cũ vẫn còn trong lịch sử consent.

## Tiền bù và quyền đề xuất

Phiên bản đầu lấy nguyên phương án cân đối giá trị do thuật toán tính. Ví dụ A cho món10 triệu/nhận12 triệu, B cho12/nhận8, C cho8/nhận10: A trả B2 triệu, C trả B2 triệu.

Thành viên có thể trao đổi bằng tin nhắn trong chat nhóm, rồi dùng API `proposals` gửi phương án có cấu trúc. Tin nhắn văn bản thông thường không sửa số tiền trong database.

- Mỗi khoản là cặp người trả–người nhận và số tiền dương. Chỉ ba participant được tham gia, không tự trả cho mình hoặc trùng cặp.
- Chỉ cho phép phương án đã gộp số dư, tối đa2 khoản cho ba người; không có người vừa trả vừa nhận hoặc vòng tiền trung gian.
- Tất cả người có trả/nhận tiền phải cho phép bù. Tổng trả của từng người phải trong hạn mức của tin của người đó; mức0 vẫn cho phép nhận nếu bật cho phép bù.
- Người đề xuất chỉ được đổi các cặp có liên quan đến chính mình. Các cặp giữa hai thành viên còn lại phải giữ nguyên so với bản cơ sở; thêm/xóa/đổi những cặp đó bị403. Người nhận tiền có thể đề xuất sửa khoản mình nhận, nhưng vẫn cần người trả và người thứ ba chấp nhận.
- Khoản bù được biểu diễn bằng một khoản trả từ participant này tới participant kia, nên mỗi khoản trả có đúng một khoản nhận tương ứng. Server tự tính tổng trả/nhận từ các dòng, kiểm tra cân bằng; client không truyền tổng để thay kết quả tính.
- Phương án mới chỉ là đề nghị. Không tự đổi bản cũ, không tự gán consent cho người khác; **kể cả người đề xuất cũng cần xác nhận lại phiên bản mới** bằng API phản hồi.

Mỗi đề xuất đóng bản trước thành `DaThayThe`, thêm `PhienBanThoaThuan` mới, các dòng TienBu mới và ba consent mới ChoPhanHoi. Lịch sử tiền/lời nhắn/votes không bị ghi đè. Phiên bản mới có24 giờ phản hồi. Nếu bản trước đã chốt, giữ ba sản phẩm trong khi chờ, chuyển root về ChoDongY và khóa bàn giao. Khi đủ3/3 mới chốt lại; từ chối/hết hạn kết thúc cả vòng và nhả hold.

Sau thương lượng, các giá trị cá nhân có thể khác phương án bù mặc định theo giá tham khảo: đây là thay đổi do ba bên cùng chấp nhận. Không phát sinh tiền đã thanh toán, ngân hàng hoặc số dư ví. Khi đã có bất kỳ giao nhận hoặc xác nhận tiền thực tế nào, API chặn thương lượng/hủy; không hồi tố khoản tiền của giao dịch đã bàn giao.

## API

Tất cả route sau nằm dưới `/api/smart-swaps`, cần JWT của USER. ADMIN không có quyền đọc vòng riêng chỉ nhờ role; người gọi phải là participant với các route vòng đã gửi.

| Method/route | Nội dung |
| --- | --- |
| GET `/search` | Tìm vòng; hợp đồng trước đây giữ nguyên |
| POST `/preview` | Xem phương án từ dữ liệu hiện tại, không ghi dữ liệu |
| POST `/invitations` | Gửi lời mời,201 và Location vòng mới; retry cùng key trả cùng ID |
| GET `/` | Danh sách vòng của mình,10/trang; query `tab=tatca/gui/nhan/chodongy/chinhthuc/dahuy`, `trang>=1` |
| GET `/{id}` | Ba thành viên/hồ sơ/món cho–nhận/nhu cầu, ba cạnh, bản hiện tại/lịch sử, trạng thái và action flags |
| POST `/{id}/responses` | Đồng ý/từ chối đúng phiên bản hiện tại |
| POST `/{id}/proposals` | Phương án tiền bù mới liên kết vào chat nhóm |
| GET `/{id}/versions` | Toàn bộ lịch sử phương án và consent |
| POST `/{id}/cancel` | Thành viên hủy trước bàn giao, cần lý do và đúng bản hiện tại |
| GET `/{id}/chat` | Hội thoại nhóm hiện có, không tạo lại khi xem |
| GET `/{id}/products/{productId}/image` | Ảnh riêng của đúng sản phẩm trong vòng, kể cả lúc đã giữ/hết công khai |
| GET `/notifications` | Thông báo Smart Swap của mình,20/trang; `chuaDoc`, `trang` |
| POST `/notifications/{id}/read` | Đánh dấu đọc đúng thông báo của mình,204; retry idempotent |

Chat nhóm được tạo một lần trong transaction lời mời. Có3 membership thật; dùng các endpoint chat đã có: `GET /api/hoithoai`, `GET /api/hoithoai/{chatId}`, `POST /api/hoithoai/{chatId}/tinnhan` với `maPhiaClient`, `noiDung`; ảnh/tin cũ/unread dùng cơ chế hiện có. Mỗi bản phương án có đúng một tin `HeThong` gắn `phienBanThoaThuanId`; trường mới trong DTO tin nhắn là nullable và tương thích client cũ. UI tương lai có thể dùng ID đó mở đúng lịch sử phiên bản. Không cần tự phân tích số tiền trong nội dung chat.

Ví dụ body (thay các placeholder ID/hash bằng giá trị thực từ API):

```json
{
  "maDeXuat": "<maDeXuat từ search>"
}
```

Lời mời:

```json
{
  "maYeuCau": "<GUID mới của client>",
  "maDeXuat": "<maDeXuat từ preview>",
  "bamPhuongAn": "<hash64 ký tự từ preview>",
  "loiNhan": "Mời hai bạn xem ba món và phương án bù tiền.",
  "dongYPhuongAn": true
}
```

Phản hồi:

```json
{
  "phienBanThoaThuanId": "<ID bản hiện tại>",
  "dongY": true,
  "daDocPhuongAn": true,
  "lyDo": null
}
```

Từ chối gửi `dongY:false` và lý do ít nhất5 ký tự. `daDocPhuongAn:true` bắt buộc cho đồng ý. Actor luôn lấy từ JWT, không lấy người dùng trong body. ID phiên bản khác hiện tại bị409; xác nhận lại cùng bản đã đồng ý không nhân consent hoặc thông báo.

Đề xuất mới, ví dụ A giảm phần mình trả cho B còn1,5 triệu, giữ C→B2 triệu:

```json
{
  "maYeuCau": "<GUID mới của client>",
  "phienBanCoSoId": "<ID bản hiện tại>",
  "tienBu": [
    { "nguoiTraId": "<A>", "nguoiNhanId": "<B>", "soTien": 1500000 },
    { "nguoiTraId": "<C>", "nguoiNhanId": "<B>", "soTien": 2000000 }
  ],
  "loiNhan": "Mình đề xuất giảm phần bù của mình, mời cả nhóm xem lại."
}
```

Danh sách là **toàn bộ phương án mới**, gồm cả các cặp cần giữ nguyên. Muốn xóa khoản được phép sửa thì bỏ cặp khỏi danh sách; không truyền số tiền0. Không thay đổi số tiền thì bị400, không tạo phiên bản rỗng chỉ để reset đồng ý. Khi bị lỗi mạng, retry giữ nguyên key, bản cơ sở và nội dung; cùng key đổi payload409. Cùng key/nội dung nhưng thứ tự khoản khác nhau vẫn cùng một phương án.

Cancel gửi `phienBanThoaThuanId`, `lyDo`. Vòng đã hủy trả trạng thái cuối mà không nhân thông báo. Các lỗi chính:401 JWT/session,404 tài nguyên riêng hoặc ảnh không thuộc vòng,403 sửa khoản giữa hai người khác/khởi tạo khi không có món,400 dữ liệu sai,409 stale/thiếu điều kiện/hết hạn/tranh chấp/đã bàn giao. Các mutation dùng rate limiter commerce hiện có; preview dùng limiter tìm Smart Swap.

## Transaction, cạnh tranh và hết hạn

Mọi mutation vòng khóa **tất cả sản phẩm theo GUID → tất cả tin theo GUID → vòng → chat nhóm**. Dùng chung `KhoaTonKho` với catalog, đơn hàng và trao đổi1–1. Không giữ một tin rồi mới khóa sản phẩm khác; tránh đảo thứ tự khóa khi hai vòng giao nhau.

Ở phiếu đồng ý cuối cùng, server kiểm tra lại đúng ba tin, nhu cầu, owner hoạt động, loại/cờ, trạng thái và hold, rồi kiểm tra cap phương án hiện tại. Trong cùng transaction:

1. Lưu consent thứ ba.
2. Chốt đúng phiên bản đủ3 consent.
3. Giữ đủ3 món và chuyển tin DangGiuCho, vòng SanSangBanGiao.
4. Chuyển root DangThucHien, ghi thông báo và sự kiện chat; commit toàn bộ.

Trigger SQL bảo vệ điều kiện chốt/ready; unique filtered hold trên SanPhamId là lớp bảo vệ cuối. Một món không được giữ cho hai vòng, hoặc đồng thời cho Smart Swap và đơn hàng/trao đổi1–1. Giao dịch thất bại rollback toàn bộ consent/hold/trạng thái của lần thử; có thể thử lại sau khi món được giải phóng.

Tạo lời mời có transaction application lock theo actor/key, unique(actor,key), hash payload và unique vòng đang chờ theo mã chiều giao. Retry đồng thời không tạo hai vòng; lời mời trùng vòng đang chờ bị409. Đề xuất mới dùng unique(transaction,actor,key), hash và kiểm tra bản cơ sở dưới khóa vòng. Hai phản hồi của hai người khác nhau có thể cùng xác nhận một ID phiên bản; không dùng rowversion của toàn vòng để làm vô hiệu phản hồi độc lập của người thứ hai.

Worker `HetHanSmartSwapWorker` kiểm tra30 giây/lượt,20 vòng/lô. GET vòng/danh sách và mutation cũng kiểm tra hết hạn. Worker chỉ xử lý vòng mới có creator, đang ChoDongY với root ChoDongY, chưa bàn giao/xác nhận tiền. Không tự hủy vòng chính thức đang chờ gặp, đang tranh chấp, đã hoàn tất hoặc vòng prototype cũ chưa có metadata lời mời. Có thể tắt worker bằng `SmartSwap__EnableExpirationWorker=false` cho kiểm thử; mặc định bật.

## CSDL và chạy demo

Migration9 `20261112070453_LoiMoiVaThuongLuongSmartSwap`:

- VongSmartSwap: creator FK về participant, key/hash lời mời, mã vòng đề xuất, mã/lý do kết thúc, thời điểm chấp nhận, index thời hạn và unique vòng chờ.
- ThanhVienVong: FK composite tin–sản phẩm để gắn đúng nguồn tin.
- PhienBanThoaThuan: key/hash đề xuất, unique theo giao dịch/actor/key.
- TinNhan: FK nullable tới phiên bản, unique có điều kiện để mỗi bản có một tin cấu trúc.
- Dữ liệu cũ giữ nullable; vòng không có creator mới chỉ xem và không bị worker mới hủy.

```powershell
cd Backend
. .\scripts\Use-DotNet8.ps1
.\scripts\Initialize-Database.ps1
dotnet run --project src/ReMarket.Api
# Kiểm thử SQL/HTTP riêng, không cần các vòng đăng sẵn:
dotnet run --project tests/ReMarket.DatabaseChecks -- --smart-swap-workflow
```

Demo bằng ba tài khoản USER mới, mỗi người có một tin được duyệt đáp ứng nhau. Một người tìm/preview/gửi mời; hai người còn lại vào notifications/vòng, xem nhóm và xác nhận. Sau bản1 đủ3/3, đề xuất giảm khoản liên quan tới mình; kiểm tra bản1 giữ lịch sử, bản2 có0/3 và vẫn giữ3 món. Cả ba xác nhận bản2 hoặc một người từ chối để kiểm tra kết thúc/nhả giữ chỗ. Không tái kích hoạt sản phẩm đã giao dịch trong fixture cũ để demo.

Cập nhật: [API giao nhận/3 vận đơn/hoàn tất](SMART_SWAP_DELIVERY.md) đã triển khai. `CoTheBanGiao` phản ánh đã đủ đồng thuận để xác nhận thông tin và bắt đầu giao nhận; chính người gửi/người nhận mới ghi các receipt theo thao tác rõ ràng. Review vòng3 bên và UI Android còn ở bước tiếp theo.

[Báo cáo kiểm thử](SMART_SWAP_WORKFLOW_VALIDATION.md). Mã chính: [DTO](../Backend/src/ReMarket.Application/SmartSwap/HopDongNghiepVuSmartSwap.cs), [service](../Backend/src/ReMarket.Infrastructure/SmartSwap/VongSmartSwapService.cs), [actions](../Backend/src/ReMarket.Infrastructure/SmartSwap/VongSmartSwapActions.cs), [read model](../Backend/src/ReMarket.Infrastructure/SmartSwap/VongSmartSwapRead.cs), [controller](../Backend/src/ReMarket.Api/Controllers/VongSmartSwapController.cs).
