# Smart Swap trên Android ReMarket

Tiếp tục project Kotlin/Compose hiện có; không tạo ứng dụng mới. Tất cả dữ liệu chạy qua Retrofit, JWT/DataStore và ASP.NET Core/SQL Server. Không có vòng, thành viên, sản phẩm, tin nhắn hay đánh giá giả trong mã ứng dụng.

## Mở tính năng

- **Trao đổi → Tìm vòng trao đổi**: chọn sản phẩm của mình đã đăng, đang hiển thị, bật Smart Swap; tìm và xem kết quả.
- **Cá nhân → Smart Swap của tôi**: lời mời đã gửi/nhận, chờ đồng ý, đang giao, hoàn tất, sự cố, đã dừng.
- Thông báo Smart Swap mở đúng vòng và đánh dấu đã đọc bằng API.

## Đối chiếu thiết kế

| Form | Figma | Triển khai |
|---|---|---|
| Tìm vòng | `205:115234` | Sản phẩm API, nhu cầu đã đăng, từ khóa bổ sung, hạn mức bù, khu vực, tình trạng, xác minh liên hệ |
| Vòng phù hợp | `205:115155` | Ba người, ba món, chiều giao/nhận, điểm phù hợp, các khoản bù, phân trang |
| Chi tiết gửi/nhận | `205:115087`, `236:18` | Sơ đồ A → C → B → A; ảnh và tên thật; gửi lời mời, tham gia/từ chối |
| Xác nhận | `205:115021` | Trạng thái từng thành viên, phiên bản hiện tại, đọc phương án trước khi đồng ý |
| Chat nhóm | `205:114968` | Tên người gửi, tin nhắn/ảnh thật, lịch sử, tin hệ thống gắn phiên bản |
| Điều chỉnh tiền bù | `236:150` | Bottom sheet trên chat, chọn thành viên/hướng bù/số tiền; giữ nguyên khoản giữa hai người khác |
| Bàn giao | `205:114915` | Thông tin giao nhận, ba vận đơn mô phỏng, xác nhận nhận đúng, ảnh bằng chứng, báo sự cố |
| Hoàn tất/đánh giá | `205:114854` | Hai form sao/nhận xét cho hai thành viên còn lại, lưu đồng thời vào SQL |

MCP trả code và ảnh màn tìm vòng trong phiên `5a4f7d38-c0e9-4325-9e94-d361adfde130`; tám frame còn lại gặp hạn mức Starter. Đối chiếu phần còn lại bằng nội dung và hình học gốc đã giải mã từ `Nhom4.fig`, lưu tại `docs/assets/smart-swap/*.json`. Không coi các màn này là đã được lấy ảnh render qua MCP.

Giữ nền `#FCFDFB`, xanh `#28734F`, chữ `#202E26`, chữ phụ `#65746A`, nền nhạt `#EAF3EC`, viền `#DCE4DD`; Roboto có sẵn, tiêu đề 20sp, nội dung 11–14sp, padding ngang 24dp, card bo 20dp, input bo 12dp, nút chính tròn cao 48dp. Dùng thanh trạng thái Android thật, không vẽ giờ/pin cố định. SVG back/down/check lấy từ MCP; vector chiều vòng `205:115119` lấy từ `.fig`, không vẽ lại đường mũi tên. Ảnh sản phẩm luôn lấy API/Coil.

Tái sử dụng `RmButton`, `RmField`, `TdCard`, `TdCheck`, `TdNote`, `LuaChon`, `AnhSanPham`, `PhanTrang`, `DieuHuongSanPham` và hệ thống font/màu hiện có. Các biểu mẫu dài cuộn được, có loading, lỗi có thể tải lại, dữ liệu rỗng và kết quả thành công.

Figma dùng giao nhận trực tiếp và nhãn 6/6 giao/nhận. Giao diện hiện tại bổ sung các form địa chỉ/vận đơn theo nghiệp vụ Backend đã triển khai; chỉ số chính là **0/3 → 1/3 → 2/3 → 3/3 người nhận đúng hàng**. Mỗi người tự xác nhận gửi và nhận; không chuyển nhãn vận chuyển “Đã giao” thành xác nhận của người nhận.

## Cấu trúc

```
domain/model/SmartSwapModels.kt
domain/repository/SmartSwapRepository.kt
data/remote/SmartSwapApi.kt
data/repository/SmartSwapRepositoryImpl.kt
feature/smartswap/
  SmartSwapViewModel.kt
  SmartSwapApp.kt
  SmartSwapComponents.kt
  SmartSwapDiscovery.kt
  SmartSwapCircle.kt
  SmartSwapChat.kt
  SmartSwapDeliveryScreens.kt
```

`AppContainer` dùng lại authenticated OkHttp, refresh token và Coil. `MainActivity` tạo ViewModel qua factory, `TaiKhoanApp` điều phối với tài khoản/mua bán/trao đổi cũ. Khi tài khoản thay đổi, hủy công việc và xóa toàn bộ trạng thái Smart Swap trong bộ nhớ. Back của hệ thống và toolbar dùng cùng lịch sử; đóng Smart Swap quay về màn gọi nó.

Đọc trạng thái khi màn ở STARTED, làm mới mỗi 3 giây cho chi tiết/xác nhận/chat/bàn giao. Tạm ngưng khi ghi dữ liệu hoặc ứng dụng ra nền. Phiên bản đồng ý thay đổi thì bỏ chọn xác nhận cũ; bản nháp tiền bù giữ mã phiên bản cơ sở để Backend phát hiện xung đột. API kiểm tra quyền và khóa sản phẩm, không dựa riêng vào việc ẩn nút trên Android.

## API sử dụng và bổ sung

Các API workflow/bàn giao đã có được mô tả trong `SMART_SWAP_WORKFLOW.md` và `SMART_SWAP_DELIVERY.md`.

- `GET /api/tindang/cuatoi?loaiTin=TraoDoi&trangThai=DangHienThi`: sản phẩm để chọn, có phân trang.
- `GET /api/smart-swaps/search`: thêm `tuKhoaMuonNhan`, `buToiDa`, `tinhTrangToiThieu`, `chiThanhVienDaXacMinh`; trả thêm `hoSo` của các thành viên trên trang kết quả. Ba bộ lọc món nhận/tiền bù yêu cầu `tinDangId`, áp dụng trước đếm và phân trang.
- Bộ lọc chỉ thu hẹp những vòng phù hợp nhu cầu đã đăng, không tự sửa tin hoặc tạo giao dịch. Xác minh yêu cầu cả email và điện thoại đã xác minh trong hồ sơ SQL; không gắn huy hiệu giả cho tài khoản chưa xác minh.
- `GET /api/smart-swaps/{id}/reviews`: đánh giá đã gửi của chính người gọi.
- `POST /api/smart-swaps/{id}/reviews`: `{maYeuCau, danhGia:[{nguoiDuocDanhGiaId, soSao, noiDung}, ...]}`; đúng hai đối tác, sao 1–5, nhận xét ít nhất 5 ký tự. Chỉ sau hoàn tất cả vòng/root; transaction, retry cùng nội dung không nhân đôi, không ghi đè đánh giá cũ.
- `POST /api/smart-swaps/{id}/evidence`: multipart `maYeuCau`, `anh`; JPG/PNG tối đa 5 MB, tối đa 5 ảnh/người. Gắn vào xác nhận nhận đúng của người gọi, không thay thế việc xác nhận. Có thể chọn ảnh trước; tải sau khi xác nhận và giữ ảnh chưa tải khi lỗi để thử lại.
- `GET /api/smart-swaps/{id}/evidence/{file}`: ảnh riêng tư cho ba thành viên.

Sử dụng bảng `DanhGia`, `BangChungGiaoNhan`, khóa ngoại và ràng buộc hiện có; **không thay đổi EF model, không cần migration mới**.

## Chạy và kiểm tra

Backend cần SQL Server/ReMarketDemo đã migrate và cấu hình JWT theo `Backend/README.md`. Android Debug mặc định `http://10.0.2.2:5080/`; không chứa tài khoản đăng nhập hoặc dữ liệu demo trong runtime.

```powershell
cd Android
./gradlew.bat --no-daemon --max-workers=2 :app:assembleDebug :app:assembleDebugAndroidTest :app:lintDebug :app:testDebugUnitTest
```

`SmartSwapEndToEndTest` tạo ba tài khoản riêng, đăng tin/ảnh và duyệt qua REST bằng tài khoản ADMIN được cấp trong môi trường kiểm thử. Chạy trên emulator với hai tham số instrumentation `adminEmail`, `adminPassword`; không ghi thông tin đó vào repository. Các tiền điều kiện ở ca từ chối/sự cố được tạo bằng API thật. Luồng chính tìm/gửi/thương lượng/chat/xác nhận/giao nhận/đánh giá thao tác qua Compose. Kết quả thực tế và ảnh chụp nằm trong `SMART_SWAP_ANDROID_VALIDATION.md`.

Các giới hạn giữ nguyên Backend: thanh toán/vận chuyển mô phỏng; xử lý tiếp một vòng sự cố cần nghiệp vụ quản trị riêng, chưa có API khôi phục; UI hiển thị tạm dừng và vẫn cho chat.
