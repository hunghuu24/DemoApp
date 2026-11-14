# Quản trị ReMarket

Tiếp tục project Android/Backend hiện có. Migration11 `20261112143941_QuanTriDanhMucVaTranhChap` bổ sung biểu tượng danh mục và thông tin căn cứ/thông báo/hướng xử lý/idempotency cho quyết định. Giữ48 bảng và toàn bộ dữ liệu nghiệp vụ cũ.

## Thiết kế và điều hướng

Đã đọc cả18 frame `Admin ·` từ `Nhom4.fig`; metadata gốc nằm trong [assets/admin](assets/admin). Figma MCP `get_design_context` và `get_screenshot` bị giới hạn quota Starter trong phiên này. Không có ảnh screenshot từ Figma MCP để khẳng định đối chiếu pixel. Nội dung, kích thước, tọa độ, typography và màu được lấy từ file nguồn, không suy diễn các màn chưa đọc.

| Form nguồn | Node | Android |
| --- | --- | --- |
| Tổng quan / menu |205:116809 /205:116652|Dashboard SQL, drawer quản trị|
| Danh sách / chi tiết người dùng |205:116578 /205:116512|Tìm kiếm, trạng thái, phân trang, thông tin/hoạt động/lịch sử|
| Khóa / tài khoản khóa |205:116413 /205:116345|Dialog7/30 ngày/vĩnh viễn; mở khóa có rà soát|
| Sản phẩm / kiểm duyệt |205:116276 /205:116220|Danh sách API, ảnh Coil, checklist, duyệt|
| Từ chối / ẩn |205:116138 /205:116077|Lý do bắt buộc, lịch sử, thông báo cho chủ tin|
| Giao dịch / chi tiết |205:115976 /205:115767|Mua bán,1–1,3 bên; sản phẩm/thành viên/thỏa thuận/vận chuyển/chat; ghi nhận phản ánh|
| Khiếu nại / bằng chứng / quyết định |205:115705 /205:115645 /205:115596|Tiếp nhận, yêu cầu bổ sung, ảnh/video có kiểm tra quyền, quyết định, xác minh thực hiện|
| Danh mục / thêm sửa |205:115492 /205:115419|Tên, cha, icon, thứ tự, hiển thị|
| Thống kê |205:115300|7/30 ngày/tùy chọn, biểu đồ, phân bố danh mục, hiệu quả xử lý|

Các frame360×800 dùng Roboto cục bộ, nền `#FCFDFB`, xanh `#28734F`, chữ `#202E26`, phụ `#65746A`, nền thẻ `#EAF3EC`, viền `#DCE4DD`, lỗi `#AE413C`. Tái sử dụng `RmField`, `RmButton`, icon vector, `TdCard`, checkbox/switch, Coil/JWT hiện có. Lề ngang24dp; header56dp; nút48dp; thẻ bo20dp. Nội dung dài cuộn, footer hành động giữ phía dưới, có loading/rỗng/lỗi/thử lại/thành công. Dữ liệu thực quyết định độ dài và xuống dòng.

Đăng nhập ADMIN mở tổng quan. Menu có người dùng, sản phẩm, giao dịch, khiếu nại, danh mục, báo cáo và đăng xuất. Nút back và back hệ thống quay về form trước; drawer đóng trước khi rời form. Đăng xuất/xóa phiên hoặc mất role không giữ dữ liệu quản trị trong ViewModel.

## API

Tất cả route sau ở `/api/admin`, có `[Authorize(Roles="ADMIN")]`, kiểm tra role còn hiệu lực trong SQL ở middleware và service. Request DTO được kiểm tra độ dài, trạng thái, phiên bản rowversion. Các thao tác ghi giới hạn tốc độ; đọc thông tin quản trị và bằng chứng không cache công khai.

| Method / suffix | Chức năng |
| --- | --- |
|GET `bao-cao?tuNgay=&denNgay=`|Thống kê SQL theo múi giờ Việt Nam, tối đa366 ngày|
|GET `nguoi-dung?q=&trangThai=&trang=` / `nguoi-dung/{id}`|Danh sách12/trang, chi tiết, vai trò, hoạt động, khóa và audit|
|POST `nguoi-dung/{id}/khoa` / `mo-khoa`|Lý do + phiên bản; khóa0/7/30 ngày; mở khóa yêu cầu rà soát|
|GET/POST `danh-muc`, PUT `danh-muc/{id}`|Danh mục cha/con, icon, hiển thị, thứ tự|
|GET `san-pham?q=&trangThai=&trang=` / `san-pham/{id}`|Tin bán/trao đổi/hỗn hợp ở mọi trạng thái|
|POST `san-pham/{id}/duyet` / `an`|Duyệt/từ chối hoặc ẩn; không sửa nội dung thay chủ tin|
|GET `giao-dich?q=&loai=&trangThai=&trang=`|MuaBan / TraoDoiMotMot / SmartSwap|
|GET `giao-dich/{id}?truoc=`|Thành viên, sản phẩm, chiều giao, lịch sử tiền bù/đồng ý, vận đơn, receipt,100 tin chat; tải tin cũ bằng thứ tự|
|GET `giao-dich/{id}/media/{kind}/{file}`|Ảnh sản phẩm, receipt, khiếu nại hoặc chat; file phải thuộc giao dịch|
|POST `giao-dich/{id}/khieu-nai`|Admin ghi nhận phản ánh đã đối chiếu giữa hai participant, audit chỉ rõ người thực hiện|
|GET `khieu-nai?q=&trangThai=&trang=` / `khieu-nai/{id}`|Ưu tiên hạn phản hồi; bằng chứng, phản hồi, quyết định|
|POST `khieu-nai/{id}/tiep-nhan`|Tiếp nhận/yêu cầu bổ sung, thông báo các thành viên|
|POST `khieu-nai/{id}/quyet-dinh`|Căn cứ, kết luận, biện pháp, hướng xử lý và nội dung thông báo; key chống lặp|
|POST `khieu-nai/{id}/xac-minh`|Xác minh việc thực hiện quyết định, giữ nguyên receipt thành viên|

API quản trị tổng quan/kiểm duyệt/hỗ trợ từ các giai đoạn trước tiếp tục hoạt động và vẫn bắt buộc ADMIN.

Kỳ báo cáo lọc theo ngày tạo của người dùng, tin, giao dịch và khiếu nại; trạng thái là trạng thái hiện tại của những hồ sơ trong kỳ, không phải số lần chuyển trạng thái trong kỳ. Tổng người dùng và tin đang chờ duyệt là toàn hệ thống. Biểu đồ gom giao dịch thành nhóm7 ngày, tiền bù đọc phiên bản thỏa thuận thực tế. Cách tính này được hiển thị bằng nhãn “trong kỳ”; số liệu được truy vấn SQL, không lấy fixture Figma.

## Quy tắc

- Khóa tài khoản thu hồi mọi phiên đang có ngay trong transaction. JWT cũ bị401; mở khóa không hồi sinh phiên cũ. Không cho tự khóa hoặc khóa ADMIN khác. Worker rà khóa hết hạn mỗi30 giây; khóa vĩnh viễn không có ngày hết hạn.
- Danh mục có mã ổn định, không tự đổi tin lịch sử. Chặn cha không tồn tại, chính nó, chu trình và tên trùng cùng cha. Khóa ứng dụng SQL tuần tự hóa chỉnh cấu trúc cây. Icon đã lưu được trả về cả API danh mục công khai; nhánh có cha ẩn không vào khám phá.
- Khi gặp409 do phiên bản cũ, quay lại và mở bản ghi mới trước khi gửi lại; không ghi đè thay đổi của quản trị viên khác.
- Duyệt giữ kiểm tra ảnh/danh mục/quyền từ catalog; ẩn chỉ tin đang hiển thị và không giữ món. Ghi lịch sử, lý do và thông báo. Rowversion cũ trả409.
- Chi tiết giao dịch hoàn toàn đọc: cả ba participant, ba sản phẩm, ba chiều giao; từng phiên bản tiền bù/đồng ý; cả ba vận đơn và xác nhận thực của đúng người nhận. File được kiểm tra thuộc giao dịch, đường dẫn dưới kho media, loại ảnh/video hợp lệ; file thiếu trả404.
- Admin có thể ghi nhận phản ánh sau đối chiếu; không giả đăng nhập participant. Audit và phản hồi ghi người quản trị thực hiện. Trao đổi đang hoạt động chuyển tạm dừng. Giao dịch đã hoàn tất chỉ ghi hồ sơ phản ánh, không rút receipt.
- Khiếu nại: `Moi → DangXuLy → ChoThucHien → DaGiaiQuyet`. Quyết định không đóng ngay; phải xác minh đã thực hiện, ghi căn cứ và thông báo các bên. Với trao đổi, phương án `GhiNhan` giữ tạm dừng được xác minh sẽ trở lại `DangXuLy`, cho phép bổ sung và ra quyết định cuối; không đánh dấu đã giải quyết khi vẫn cần xử lý. Lịch sử phương án giữ tạm dừng không ngăn phương án cuối đã xác minh của cùng khiếu nại. Tiền hoàn/bù chỉ ghi nhận trực tiếp; không gọi ngân hàng.
- Với trao đổi tạm dừng, chỉ khôi phục khi tất cả khiếu nại đã đóng/từ chối, thỏa thuận vẫn chốt đủ và còn giữ đúng 2 hoặc 3 món theo loại giao dịch. Quyết định kết thúc đã xác minh nhả hold và ẩn tin; phương án giữ tạm dừng tiếp tục chờ xử lý. Quyết định kết thúc được ưu tiên. Quyết định cuối đã xác minh thay thế giữ tạm của cùng khiếu nại; giữ tạm của khiếu nại khác vẫn chặn tiếp tục. Mua bán chỉ ghi nhận biện pháp thủ công, không sửa thanh toán/đơn qua quyết định này.
- Thứ tự khóa giữ chung sản phẩm → tin → vòng → giao dịch → khiếu nại. Các bước lưu quyết định đã xác minh, đóng khiếu nại, đổi trạng thái vòng và root được tách lần SaveChanges trong **cùng transaction**, đáp ứng trigger SQL mà vẫn rollback nguyên tử khi lỗi.
- Admin **không có API nhận hàng**. Không ghi `XacNhanGiaoNhan`, `NgayNhanDungHang` hoặc xác nhận tiền bù thay participant. Khôi phục Smart Swap2/3 vẫn là2/3; chỉ người nhận thứ ba xác nhận đúng hàng mới hoàn tất.

## Chạy và kiểm tra

```powershell
cd Backend
. ./scripts/Use-DotNet8.ps1
./scripts/Initialize-Database.ps1
dotnet run --project tests/ReMarket.DatabaseChecks -c Release -- --admin
```

Đăng ký tài khoản thật rồi dùng lệnh Development hiện có `dotnet run --project src/ReMarket.Api -- --grant-admin <email>` để cấp role trong database. Không có mật khẩu quản trị cố định trong Android; không tạo tài khoản ADMIN tự động. Dùng hướng dẫn JWT/SQL trong [ACCOUNTS.md](ACCOUNTS.md).

Android instrumented test `QuanTriEndToEndTest` nhận `adminEmail` và `adminPassword` qua instrumentation arguments. Test tự đăng ký người dùng, đăng tin/upload/kiểm duyệt và xây vòng3 bên qua API thật. Test dùng vòng riêng, không hoàn tất vòng demo2/3 đã có. Thông tin đăng nhập QA chỉ lưu tạm cục bộ, không commit vào tài liệu hay source.

```powershell
cd Android
./gradlew.bat :app:assembleDebug :app:assembleDebugAndroidTest :app:lintDebug
adb install -r app/build/outputs/apk/debug/app-debug.apk
adb install -r app/build/outputs/apk/androidTest/debug/app-debug-androidTest.apk
# Cung cấp hai biến từ tài khoản Development đã cấp ADMIN, không lưu vào source:
adb shell am instrument -w -r `
  -e class com.ute.app_ban_do_cu.QuanTriEndToEndTest `
  -e adminEmail $adminEmail -e adminPassword $adminPassword `
  com.ute.app_ban_do_cu.test/androidx.test.runner.AndroidJUnitRunner
```

Xem [PROJECT_AUDIT.md](PROJECT_AUDIT.md) cho kết quả rà soát mới nhất và giới hạn kiểm chứng; [ADMIN_VALIDATION.md](ADMIN_VALIDATION.md) giữ kết quả lịch sử ngày 12/11.
