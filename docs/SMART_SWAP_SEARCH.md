# Tìm vòng Smart Swap 3 bên

Cập nhật12/11/2026. Tài liệu này mô tả Backend tìm kiếm trên project ReMarket hiện có. Search không tạo vòng/giao dịch/consent, không giữ sản phẩm và không gửi lời mời. Bước tiếp theo đã có [API lời mời/đồng thuận/thương lượng](SMART_SWAP_WORKFLOW.md), dùng lại thuật toán này để kiểm tra đúng3 tin trong transaction. Android giữ nguyên.

## Nguồn dữ liệu và đồ thị

`TimSmartSwapService` truy vấn SQL Server bằng EF Core, đọc `NhuCauTraoDoi` → `TinDang` → `SanPham`. Không truy vấn vòng mẫu trong `VongSmartSwap` để làm kết quả, không có bảng tên sản phẩm iPhone/Laptop/iPad hoặc danh sách người A/B/C trong thuật toán.

Ứng viên phải có đủ các điều kiện:

- Tin `DangHienThi`, chưa xóa mềm, loại `TraoDoi` hoặc `CaHai`.
- `ChoPhepSmartSwap=true`; không bắt buộc bật trao đổi 1–1.
- Chủ tin đúng chủ sản phẩm, tài khoản `HoatDong`.
- Danh mục sản phẩm và danh mục muốn nhận còn hiển thị, kể cả mọi danh mục cha. Nhánh có cha bị ẩn/mất hoặc chu trình danh mục không được coi là công khai.
- Giá trị tham khảo trong khoảng1–1.000.000.000.000 VND.
- Không có `GiuChoSanPham` chưa giải phóng. Mốc hết hạn trên một hold không tự chứng minh sản phẩm đã được giải phóng; dùng quy tắc tồn kho hiện có.

Mỗi đỉnh là một **tin–sản phẩm–chủ sở hữu–nhu cầu**. Một người có thể có nhiều ứng viên, nhưng trong từng vòng chỉ được chọn một tin/sản phẩm của người đó.

Cạnh có hướng `u → v` nghĩa là **u giao sản phẩm cho v**, sản phẩm u đáp ứng nhu cầu v. Ví dụ:

| Người | Có | Muốn nhận | Giao cho | Nhận từ |
| --- | --- | --- | --- | --- |
| A | iPhone | Laptop | C | B |
| B | Laptop | iPad | A | C |
| C | iPad | iPhone | B | A |

Đường giao đồ là **A → C → B → A**. Không dùng chiều “người muốn nhận → người đang có” để hiển thị giao đồ.

`BoTimVongSmartSwap` dựng danh sách kề có hướng, duyệt hai cạnh `a→b`, `b→c` và tra cạnh đóng `c→a`. Chỉ nhận vòng đơn đúng3 chủ sở hữu và3 sản phẩm khác nhau. Không nhận self-loop, vòng2 hoặc vòng4 người. Chỉ duyệt khi a là tin nhỏ nhất theo thứ tự GUID trong bộ ba: ba cách xoay cùng một vòng chỉ xuất hiện một lần. Hai chiều giao ngược nhau được giữ riêng nếu đều đáp ứng nhu cầu, vì món mỗi người nhận khác nhau.

`maDeXuat` là chuỗi3 ID tin theo chiều giao đã chuẩn hóa, ổn định khi đổi thứ tự đầu vào. Nó không phải ID giao dịch và không chứng minh giá trị/điều kiện vẫn còn nguyên ở lần tìm tiếp theo.

## Nhu cầu, tình trạng và khoảng giá

Migration8 `20261112061936_TieuChiTimSmartSwap` thêm4 cột nullable vào `NhuCauTraoDoi`: `TuKhoaMongMuon`, `TinhTrangToiThieu`, `GiaTriMongMuonTu`, `GiaTriMongMuonDen`. Tái sử dụng FK `DanhMucMongMuonId` hiện có; không ghi đè nhu cầu cũ.

Thiết lập qua API tạo/sửa tin hiện có, trong object `traoDoi`:

```json
{
  "moTaMongMuon": "Muốn nhận Laptop để làm việc",
  "choPhepMotMot": true,
  "choPhepSmartSwap": true,
  "choPhepBuTien": true,
  "soTienBuToiDa": 2000000,
  "ghiChu": "Kiểm tra trực tiếp trước khi giao nhận",
  "tieuChiSmartSwap": {
    "danhMucMongMuonId": null,
    "tuKhoaMongMuon": "laptop",
    "tinhTrangToiThieu": "DaSuDungTot",
    "giaTriMongMuonTu": 8000000,
    "giaTriMongMuonDen": 15000000
  }
}
```

Đây là phần `traoDoi` của body `POST /api/tindang` hoặc `PUT /api/tindang/{id}`, dùng cùng tên/ảnh/danh mục/giá trị/phiên bản và quy trình duyệt trước đây. `danhMucMongMuonId` nhận GUID thật từ danh mục của hệ thống. Các trường mới cũng có trong DTO quản lý/chi tiết tin. Omit hoặc gửi `tieuChiSmartSwap:null` giữ các tiêu chí cũ khi sửa bằng client trước đây; gửi object `{}` xóa các tiêu chí tùy chọn và quay về đối sánh mô tả cũ. Sửa tin đã công khai vẫn phải duyệt lại, rowversion/quyền và kiểm tra nguồn lịch sử vẫn được áp dụng.

Các điều kiện nhận sản phẩm được kiểm tra đồng thời:

1. Nếu có danh mục muốn nhận: sản phẩm phải thuộc danh mục đó hoặc danh mục con.
2. Nếu có từ khóa rõ ràng: khớp đủ từng từ trong ít nhất một phương án, trên **tên sản phẩm và tên nhánh danh mục**. Dấu `|` phân tách phương án OR; trong mỗi phương án là AND, ví dụ `laptop|may doc sach`. Không dò phần mô tả dài của sản phẩm vì câu như “không phải iPhone” có thể gây khớp sai.
3. Nếu chỉ có danh mục cấu trúc, dùng danh mục làm nhu cầu chính. Nếu chưa có cả danh mục lẫn từ khóa cấu trúc, dùng `MoTaMongMuon` cũ: chuẩn hóa chữ thường/bỏ dấu/đ→d/tách chữ–số, bỏ một số từ dẫn như “tôi muốn nhận/đổi lấy”, rồi yêu cầu **đủ các từ còn lại**, theo ranh giới từ. “Muốn nhận Laptop” nhận Laptop; “iPhone” không tự khớp “iPhones”.
4. Tin cũ chỉ có câu chung chung, phủ định hoặc lựa chọn tự nhiên chưa có ngữ pháp rõ không được đoán thành nhu cầu tích cực. Cần bổ sung danh mục/từ khóa cấu trúc. Không dùng AI ngữ nghĩa, từ đồng nghĩa hoặc bộ luật theo tên sản phẩm; “máy tính xách tay” và “laptop” cần danh mục chung hoặc phương án từ khóa rõ ràng.
5. Tình trạng món giao phải đạt tối thiểu theo thứ tự **CoLoiNhe < DaSuDungTot < NhuMoi**. NULL không áp thêm điều kiện tối thiểu.
6. Giá trị món nhận nằm trong `[GiaTriMongMuonTu, GiaTriMongMuonDen]`, bao gồm hai đầu; mỗi đầu NULL là không giới hạn phía đó. Đây là khoảng giá trị **món muốn nhận**, không phải giá trị món đem đổi hoặc tổng tiền bù.

API và CHECK SQL chặn khoảng giá đảo ngược/âm/vượt1.000 tỷ, mã tình trạng không hợp lệ; FK và service kiểm tra danh mục. Từ khóa tối đa200 ký tự; trường mô tả cũ giữ nguyên giới hạn.

## Tiền bù theo toàn vòng

Không gán tiền bù độc lập trên mỗi cạnh trước khi đóng vòng. Với mỗi người:

```text
chênh lệch = giá trị món nhận - giá trị món cho
tiền cần bù = max(chênh lệch, 0)
tiền được nhận = max(-chênh lệch, 0)
```

Người có chênh lệch khác0 phải cho phép bù tiền, kể cả khi họ nhận tiền. Tổng tiền người đó phải trả không được vượt `SoTienBuToiDa` của **tin của chính họ**. Người nhận tiền có mức tự bù0 vẫn được nhận tiền nếu đã bật cho phép bù. Người có chênh lệch0 không cần tham gia chuyển tiền.

Tổng chênh lệch của vòng bằng0. Ghép người cần trả với người cần nhận theo thứ tự GUID ổn định, trả trực tiếp khoản nhỏ hơn của hai số dư, tiếp tục tới khi đủ. Mỗi người bảo toàn giá trị sau tiền bù, tổng trả bằng tổng nhận, không có người ngoài hoặc nguồn tiền từ ứng dụng.

Ví dụ A có10 triệu muốn món12 triệu; B có12 triệu muốn món8 triệu; C có8 triệu muốn món10 triệu: A trả B2 triệu, C trả B2 triệu. Nếu một người trả cho hai người khác, kiểm tra **tổng** khoản phải trả theo cap, không tách mỗi khoản rồi lách cap. Tiền bù là phương án theo giá trị tham khảo do người đăng cung cấp, chưa phải khoản đã đồng ý hoặc thanh toán.

## Điểm và xếp hạng

Mỗi cạnh hợp lệ được tính điểm theo góc nhìn người nhận:

```text
60 điểm: nhu cầu/danh mục/từ khóa, tình trạng tối thiểu và khoảng giá đều đạt
+ 25 × min(giá trị món cho của người nhận, giá trị món nhận) / max(hai giá trị)
+ 10 × chất lượng món nhận: NhuMoi=1; DaSuDungTot=0,75; CoLoiNhe=0,4
+ 5 nếu người giao và nhận ở cùng tỉnh/thành phố sau chuẩn hóa
```

Điểm vòng là trung bình3 cạnh, làm tròn2 số thập phân. Xếp giảm dần theo điểm; nếu bằng điểm, ưu tiên tổng tiền bù thấp hơn rồi `maDeXuat` theo ordinal để phân trang ổn định. Lọc ngưỡng điểm và xếp hạng **trước** phân trang. Các lý do phù hợp có trong từng cạnh; không dùng số lượng tin hay KPI giả để tính điểm.

## REST API

```http
GET /api/smart-swaps/search?trang=1&kichThuocTrang=20&diemToiThieu=80
Authorization: Bearer <JWT của USER>
```

| Query | Mặc định / ý nghĩa |
| --- | --- |
| `tinDangId` | NULL: toàn bộ vòng. Nếu có, vòng phải chứa đúng tin công khai còn khả dụng này ở bất kỳ vị trí nào; có thể là tin của người khác |
| `tinhThanh` | NULL: không lọc địa điểm. Nếu có, cả3 tin phải nằm trong phần địa điểm khớp chuỗi, bỏ dấu/hoa thường tại SQL |
| `diemToiThieu` | 0; khoảng0–100 |
| `trang` | 1; khoảng1–1000 |
| `kichThuocTrang` | 20; khoảng1–50 |

HTTP200 trả `items`, `tongSo`, `trang`, `kichThuocTrang`, `tongTrang`, `soTinKhaDung`, `soCanh`, `thoiDiemTim`, `thongBao`. Mỗi vòng gồm:

- `maDeXuat`, `diemPhuHop`, `tongTienBu`.
- `chang`: đúng3 cạnh theo chiều giao; người giao/nhận, món giao, giá trị/tình trạng/tỉnh thành/rowversion tin, điểm và lý do.
- `thanhVien`:3 người, tin cho/tin nhận, giá trị cho/nhận, tiền cần bù/được nhận và cap.
- `tienBu`: các khoản trả trực tiếp đề xuất, chưa lưu `TienBu` nghiệp vụ.

Không thấy vòng trả HTTP200, ví dụ trên SQL không có tin đủ điều kiện:

```json
{
  "items": [],
  "tongSo": 0,
  "trang": 1,
  "kichThuocTrang": 20,
  "tongTrang": 0,
  "soTinKhaDung": 0,
  "soCanh": 0,
  "thoiDiemTim": "2026-11-12T00:00:00Z",
  "thongBao": "Chưa tìm thấy vòng Smart Swap 3 bên phù hợp với nhu cầu và hạn mức tiền bù."
}
```

Thời điểm trên chỉ minh họa; response thật dùng TimeProvider UTC. Nếu có ứng viên nhưng không khép vòng, `soTinKhaDung`/`soCanh` có thể khác0 trong cùng dạng kết quả rỗng. Trang vượt tổng trang giữ `tongSo` và có thông báo riêng. JWT thiếu/hết hạn401; không có role USER403; query không hợp lệ400; anchor không còn khả dụng trong bộ lọc404. Header `Cache-Control: private, no-store`.

Tìm kiếm là ảnh chụp trạng thái có thể thay đổi ngay sau khi đọc. API này không đặt chỗ. Giai đoạn tạo/lời mời/đồng ý Smart Swap sau này phải đọc lại cả nhu cầu, trạng thái, version và tồn kho trong transaction; không dùng kết quả tìm kiếm làm bằng chứng còn hàng.

## Giới hạn xử lý

Dựng cạnh có chi phí O(n² × chi phí đối sánh), tìm vòng O(tổng số đường đi2 cạnh được duyệt); trường hợp đồ thị dày là O(n³). Lưu danh sách kề O(n+e). Chỉ giữ ứng viên kết quả tốt nhất tới `trang × kichThuocTrang` thay vì giữ mọi vòng; `tongSo` vẫn được đếm đầy đủ trước khi trả trang. Kiểm tra CancellationToken khi dựng/duyệt.

Để tránh một request chiếm tài nguyên không giới hạn, cấu hình tùy chọn:

```json
{
  "SmartSwapSearch": {
    "SoTinToiDa": 1500,
    "SoCanhToiDa": 250000,
    "SoLuotDuyetToiDa": 2000000
  }
}
```

Đây là mặc định hiện có trong options; có thể cung cấp bằng environment `SmartSwapSearch__SoTinToiDa` v.v. Host xác minh giới hạn cấu hình khi khởi động. Truy vấn lấy tối đa `SoTinToiDa+1` để **phát hiện vượt giới hạn**, không âm thầm bỏ tin cũ/topN rồi kết luận không có vòng. Vượt ngân sách tin/cạnh/lượt duyệt trả **422**, yêu cầu thu hẹp địa điểm; không trả kết quả một phần với200. `tinDangId` lọc vòng sau khi dựng đồ thị, không tự thu hẹp tập tin SQL. Rate limit mặc định10 request/phút/IP cho route này, quá mức429.

## Chạy và kiểm tra

```powershell
cd Backend
. .\scripts\Use-DotNet8.ps1
.\scripts\Initialize-Database.ps1
dotnet run --project src/ReMarket.Api
# Terminal khác, cũng chọn SDK8:
dotnet run --project tests/ReMarket.DatabaseChecks -- --smart-swaps
```

Migration được áp rõ ràng trước startup; không tự tạo/seed vòng khi chạy API. Kiểm thử tạo DB SQL Server ngẫu nhiên riêng, dùng JWT thật và đăng/ảnh/duyệt thật, tự dọn đúng DB/ảnh do lượt test tạo. [Kết quả kiểm thử](SMART_SWAP_SEARCH_VALIDATION.md), [thiết kế CSDL](database/THIET_KE_CSDL.md).

Mã nguồn: [bộ tìm đồ thị](../Backend/src/ReMarket.Application/SmartSwap/BoTimVongSmartSwap.cs), [hợp đồng](../Backend/src/ReMarket.Application/SmartSwap/HopDongSmartSwap.cs), [truy vấn SQL](../Backend/src/ReMarket.Infrastructure/SmartSwap/TimSmartSwapService.cs), [controller](../Backend/src/ReMarket.Api/Controllers/SmartSwapsController.cs), [kiểm thử](../Backend/tests/ReMarket.DatabaseChecks/KiemTraSmartSwap.cs).
