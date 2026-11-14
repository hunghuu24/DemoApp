# ReMarket — Phân tích thiết kế Figma

Ngày phân tích: **07/11/2026**, múi giờ Asia/Saigon. Phạm vi: đọc thiết kế và lập kế hoạch; chưa lập trình ứng dụng.

> Cập nhật triển khai Admin12/11/2026: đã đọc lại18 frame quản trị, xuất [metadata gốc](docs/assets/admin) và chín icon vector bằng các tool cục bộ. MCP design context/screenshot hiện báo hết quota Starter; không có ảnh MCP mới để kiểm chứng pixel. Ánh xạ form, API và kiểm thử hiện hành nằm trong [ADMIN.md](docs/ADMIN.md), tách khỏi kết quả phân tích lịch sử dưới đây.

## 1. Nguồn, phương pháp và độ phủ

- Figma: [Nhom4 — User · Khám phá trao đổi](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=205-118152), file key `A0eyikaueyfowTKl4NuM9F`.
- File cục bộ: [Nhom4.fig](Nhom4.fig), **8.606.329 byte**; metadata export `2026-11-07T14:30:13.568Z`, tương đương **21:30:13 ngày 07/11/2026** tại Việt Nam.
- SHA-256: `7C9D39E26CEC0CBA81E2E8C9877C49E8D3EC4FDA5E5353211E1A3581F7CC6BE7`.
- Đã dùng Figma MCP, skill `figma-use`, ở chế độ đọc. MCP trả danh sách page, toàn bộ 84 node cấp cao trên page chính, metadata của node được dẫn và chi tiết một phần màn hình; sau đó báo giới hạn lượt gọi của gói Starter. Không sửa file Figma.
- Tiếp tục giải mã bản `.fig` cục bộ: ZIP → `canvas.fig` (`fig-kiwi`, version 106) → schema Kiwi nhúng trong file → dữ liệu node. Script phân tích và thư viện chỉ nằm trong thư mục tạm hệ thống, không thêm dependency vào dự án Android. Cơ chế giải mã tham khảo [mã nguồn parser](https://github.com/sunyui/figma-parser/blob/main/scripts/decode_kiwi.cjs).
- Đọc được **5.203 node**, gồm toàn bộ nội dung chữ, màu, kích thước, bố cục và tương tác của **83 frame màn hình/trạng thái**. Đối chiếu **83/83** tên, ID và kích thước với MCP; đối chiếu thứ tự/nội dung text ở **14 frame khác nhau**, không có sai khác. Đây là đối chiếu mẫu giữa hai nguồn, không khẳng định mọi thuộc tính của bản trực tuyến đều giống snapshot cục bộ.
- `My Component` (`0:1`): **84 frame cấp cao**, gồm 83 frame rộng 360 và frame `Quay lại` (`77:117`, 22×22). `Use Component` (`1:2`) rỗng. File nhị phân còn có `Internal Only Canvas` (`0:2`, ẩn, 27 node con), không phải màn hình người dùng.
- Đã xem thumbnail nhúng 400×203 để kiểm tra tổng thể canvas; thumbnail không đủ đọc chi tiết từng màn. Chưa có bộ screenshot từng frame ở độ phân giải đầy đủ, nên chưa xác minh trực quan toàn bộ ảnh, hình icon, clipping và độ khớp pixel khi chạy Prototype. Những kết luận dưới đây dựa vào dữ liệu node thực đã đọc, không suy đoán màn hình bị thiếu.

### Quy ước bằng chứng

**Đã đọc**: có frame/node/text/interaction thực trong snapshot. **Đề xuất**: phương án cho giai đoạn triển khai, chưa có trong thiết kế. **Chưa đặc tả**: nguồn hiện có không đủ xác định hành vi. Tên frame được giữ nguyên kể cả khi sai; cột nội dung giải thích màn thực sự hiển thị. Frame trạng thái, modal và biến thể dữ liệu vẫn được liệt kê riêng, không đồng nghĩa cần 83 Activity hay 83 Fragment.

## 2. Toàn bộ màn hình User và Admin

Tổng **65 frame User**, trong đó **9 Smart Swap**, và **18 frame Admin**. Mọi dòng trong bảng đã đọc nội dung node; kích thước là số đo Figma, chưa phải kích thước dp của thiết bị.

### Tài khoản — 8 frame

| ID / Figma | Tên frame nguyên bản | Kích thước | Nội dung / trạng thái đã đọc |
| --- | --- | --- | --- |
| [2:2955](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=2-2955) | Splash · ReMarket | 360×800 | Splash thương hiệu, thông điệp sống xanh; điểm bắt đầu Flow 1. |
| [2:2983](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=2-2983) | Đăng nhập | 360×800 | Đăng nhập email/SĐT và mật khẩu; liên kết đăng ký, quên mật khẩu. |
| [2:3019](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=2-3019) | Đăng ký | 360×800 | Họ tên, email, SĐT, mật khẩu/xác nhận, điều khoản. |
| [2:3064](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=2-3064) | Quên mật khẩu | 360×800 | Nhập email nhận liên kết khôi phục, hiệu lực 15 phút. |
| [92:558](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=92-558) | Quên mật khẩu | 360×800 | Nội dung thực là Đổi mật khẩu: cũ, mới, xác nhận; tên/header còn Quên mật khẩu. |
| [2:3096](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=2-3096) | Thông tin cá nhân | 360×800 | Avatar, họ tên, email, SĐT, địa chỉ; CTA chỉnh sửa. |
| [2:3139](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=2-3139) | Chỉnh sửa thông tin cá nhân | 360×800 | Đổi ảnh và chỉnh bốn trường hồ sơ; lưu thay đổi. |
| [2:3179](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=2-3179) | Cài đặt tài khoản | 360×901 | Tiện ích đơn/trao đổi/hỗ trợ; bảo mật, quyền riêng tư, đăng xuất; main nav. |

### Khám phá mua bán — 7 frame

| ID / Figma | Tên frame nguyên bản | Kích thước | Nội dung / trạng thái đã đọc |
| --- | --- | --- | --- |
| [4:461](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=4-461) | Trang chủ | 360×800 | Home: search, chips danh mục, sản phẩm nổi bật/mới đăng, main nav. |
| [4:556](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=4-556) | Tìm kiếm sản phẩm | 360×800 | Kết quả ghế gỗ, chips lọc/sort; copy ghi 2 kết quả nhưng chỉ một card sản phẩm trong text. |
| [4:617](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=4-617) | Danh mục sản phẩm | 360×800 | Sáu danh mục: Nội thất, Điện tử, Thời trang, Gia dụng, Sách & học tập, Thể thao. |
| [4:684](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=4-684) | Danh sách sản phẩm | 360×800 | Danh sách Nội thất, 128 sản phẩm, lọc và sort, ba sản phẩm mẫu. |
| [4:755](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=4-755) | Chi tiết sản phẩm | 360×800 | iPhone 13: ảnh, giá 7,2 triệu, mô tả, người bán, liên hệ/mua ngay. |
| [128:6](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=128-6) | Chi tiết sản phẩm 1 | 360×800 | Biến thể Detail bàn trà 650.000đ; có nhãn CTA lặp trong cây. |
| [128:67](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=128-67) | Chi tiết sản phẩm 2 | 360×800 | Biến thể Detail kệ sách 350.000đ; khác giá 320.000đ ở list. |

### Đăng bán và quản lý tin — 7 frame

| ID / Figma | Tên frame nguyên bản | Kích thước | Nội dung / trạng thái đã đọc |
| --- | --- | --- | --- |
| [114:2](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=114-2) | Đăng bán sản phẩm | 360×800 | Form đăng bán, 2/8 ảnh, tên/danh mục/giá/tình trạng/mô tả/địa điểm. |
| [114:292](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=114-292) | Đăng tin · Xem trước | 360×800 | Preview tin riêng tư trước gửi duyệt; xác nhận hoặc quay lại sửa. |
| [114:252](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=114-252) | Đăng tin · Gửi tin thành công | 360×800 | Gửi tin thành công, Chờ duyệt, chưa hiển thị; quản lý hoặc đăng thêm. |
| [114:332](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=114-332) | Đăng tin · Quản lý tin | 360×800 | Tabs Đang bán/Chờ duyệt/Đã bán/Bản nháp; list ba tin, main nav. |
| [114:192](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=114-192) | Tin của tôi · Chi tiết quản lý | 360×800 | Chi tiết quản lý: ảnh, thông tin, lượt xem/lưu/chat; sửa, đã bán, ẩn, xóa. |
| [114:115](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=114-115) | Tin của tôi · Chỉnh sửa tin | 360×800 | Sửa tin đang hiển thị, ảnh bìa, thông tin, lưu/hủy. |
| [114:66](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=114-66) | Tin của tôi · Xác nhận đã bán | 360×800 | Trang xác nhận đã bán, giải thích tác động; xác nhận hoặc tiếp tục bán. |

### Chat giá, checkout, đơn, hỗ trợ và đánh giá — 16 frame

| ID / Figma | Tên frame nguyên bản | Kích thước | Nội dung / trạng thái đã đọc |
| --- | --- | --- | --- |
| [90:16](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=90-16) | Đơn hàng của tôi | 360×800 | Nội dung thực là Inbox: Tất cả/Chưa đọc, hai hội thoại; tên frame vẫn Đơn hàng của tôi. |
| [128:139](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=128-139) | Chat thương lượng giá | 360×800 | Chat giá ban đầu với Nguyễn Văn A, giá niêm yết 7,2 triệu; nút Trả giá. |
| [128:174](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=128-174) | Phiếu giảm giá | 360×800 | Form Phiếu giảm giá: nhập số tiền muốn giảm; Hoàn Tất mở trạng thái đề nghị. |
| [188:2](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=188-2) | Chat thương lượng giá | 360×800 | Chat nhìn từ phía người bán, đối tác Phạm Bá Khôi; phiếu -700k, Từ chối/Chấp nhận. |
| [85:2](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=85-2) | Chat thương lượng giá | 360×800 | Chat có phiếu -700k đã đồng ý; Từ chối/Mua hàng. |
| [194:72](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=194-72) | Xác nhận mua hàng | 360×800 | Checkout giá gốc: 7.200.000 + ship 30.000 = 7.230.000đ; thông tin giao hàng. |
| [44:191](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=44-191) | Xác nhận mua hàng | 360×800 | Checkout sau thương lượng: 6.500.000 + ship 30.000 = 6.530.000đ. |
| [44:223](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=44-223) | Đơn hàng của tôi | 360×800 | Orders: tabs Tất cả/Đang giao/Hoàn tất/Đã hủy; hai đơn mẫu. |
| [44:248](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=44-248) | Chi tiết đơn hàng | 360×800 | Detail đơn đang vận chuyển, timeline, xác nhận Đã nhận được hàng. |
| [77:65](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=77-65) | Chi tiết đơn hàng | 360×800 | Biến thể Detail có timeline đánh dấu nhận, CTA Đánh giá/Gửi hỗ trợ; header còn Đang vận chuyển. |
| [44:279](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=44-279) | Gửi yêu cầu hỗ trợ | 360×800 | Form hỗ trợ từ đơn, loại vấn đề, mô tả, nhắc bổ sung minh chứng. |
| [44:303](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=44-303) | Theo dõi yêu cầu hỗ trợ | 360×800 | Theo dõi HT-260018, trạng thái Đang xử lý và timeline gửi/kiểm tra/giải quyết. |
| [90:78](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=90-78) | Đơn hàng của tôi | 360×800 | Nội dung thực là Lịch sử yêu cầu hỗ trợ; một phiếu Chờ duyệt; tên frame vẫn Đơn hàng của tôi. |
| [128:216](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=128-216) | Gửi yêu cầu hỗ trợ mới | 360×800 | Form hỗ trợ mới, chưa điền đơn liên quan, loại vấn đề và mô tả. |
| [146:248](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=146-248) | Đánh giá | 360×800 | Form đánh giá, câu hỏi đánh giá sản phẩm/nhận xét; có nhóm layer text lặp. |
| [146:351](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=146-351) | Đánh giá thành công | 360×800 | Thông báo đánh giá thành công, Quay lại đơn hàng; có layer/header lặp. |

### Tin trao đổi và giao dịch 1–1 — 18 frame

| ID / Figma | Tên frame nguyên bản | Kích thước | Nội dung / trạng thái đã đọc |
| --- | --- | --- | --- |
| [205:118152](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=205-118152) | User · Khám phá trao đổi | 360×894 | Explore trao đổi: search, filter, tabs, CTA tìm vòng, card món muốn nhận, main nav. |
| [205:118039](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=205-118039) | User · Bộ lọc trao đổi đang mở | 360×800 | Explore với bottom sheet lọc đang mở: danh mục, vùng, khoảng giá, cho phép bù tiền. |
| [226:102](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=226-102) | User · Trao đổi của tôi | 360×894 | Quản lý TIN trao đổi của mình; hai tin hiển thị, một nháp; Sửa/Ẩn/Xóa; đăng tin mới. |
| [226:213](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=226-213) | User · Đăng tin trao đổi | 360×1872 | Form dài bốn section: món có, món muốn nhận, điều kiện bù, địa điểm; đăng/lưu nháp; 0/6 ảnh. |
| [205:117992](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=205-117992) | User · Chi tiết sản phẩm trao đổi | 360×813 | Detail Kindle, mô tả, món muốn nhận, chủ tin xác minh/rating; nhắn tin/đề nghị. |
| [205:117942](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=205-117942) | User · Chọn đồ của tôi | 360×800 | Chọn một trong ba tin của mình; Sony được chọn; chỉ chọn tin đủ điều kiện. |
| [205:117877](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=205-117877) | User · Tạo đề nghị | 360×813 | Tạo đề nghị TD-1024, Sony đổi Kindle, bù 300k, lời nhắn, hẹn và đồng ý quy định. |
| [205:117719](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=205-117719) | User · Đề nghị đã gửi | 360×1325 | Đề nghị ĐÃ GỬI, list lẫn Smart Swap 2/3 và 1–1; hạn 48h/24h, hủy/xem tiến độ. |
| [205:117605](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=205-117605) | User · Đề nghị đã nhận | 360×1203 | Đề nghị ĐÃ NHẬN từ góc nhìn Nam; một Smart Swap và hai 1–1. |
| [205:117393](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=205-117393) | User · Chi tiết đề nghị nhận được | 360×800 | Chi tiết đề nghị nhận TD-1024, Sony/Kindle và bù 300k; chat/từ chối/counter/accept. |
| [205:117344](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=205-117344) | User · Đề xuất lại | 360×800 | Phản đề nghị từ Nam: tăng bù 300k→400k, lý do/lời nhắn; phải được Minh Anh đồng ý. |
| [205:117522](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=205-117522) | User · Đang thương lượng | 360×800 | Tab Thương lượng: counter 400k, lịch sử, hạn phản hồi và mở chat. |
| [205:117265](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=205-117265) | User · Chat thương lượng trao đổi | 360×800 | Chat trao đổi 1–1 TD-1024 với Nam, lịch sử tin và composer. |
| [205:117122](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=205-117122) | User · Xác nhận giao nhận | 360×811 | Xác nhận đã kiểm tra/nhận Kindle và trả đủ 400k; bằng chứng tối đa 5 ảnh. |
| [205:117061](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=205-117061) | User · Chờ đối tác xác nhận | 360×811 | Đã xác nhận phía Minh Anh nhưng chờ Nam; giao dịch chưa hoàn tất; báo vấn đề/nhắc đối tác. |
| [205:117447](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=205-117447) | User · Trao đổi đã hoàn thành | 360×800 | Tab Hoàn thành; TD-1024, hai bên xác nhận; có thêm nội dung chờ và route Smart Swap lệch ngữ cảnh. |
| [205:116984](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=205-116984) | User · Hoàn tất và đánh giá trao đổi | 360×811 | Hoàn tất 1–1, hai tag đánh giá, nhận xét và gửi đánh giá Nam. |
| [205:116916](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=205-116916) | User · Đánh giá trao đổi thành công | 360×819 | Đã gửi đánh giá công khai, tóm tắt đồ/tiền bù; về Explore hoặc lịch sử trao đổi. |

### Smart Swap — 9 frame User

| ID / Figma | Tên frame nguyên bản | Kích thước | Nội dung / trạng thái đã đọc |
| --- | --- | --- | --- |
| [205:115234](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=205-115234) | Smart Swap · Tìm vòng trao đổi | 360×800 | Tìm vòng: món đang có, mong muốn iPad, bù tối đa 500k, khu vực, điều kiện, thành viên xác minh. |
| [205:115155](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=205-115155) | Smart Swap · Vòng phù hợp | 360×800 | Hai vòng phù hợp SS-0301/0302; người/món theo vòng, tiền bù 500k/300k. |
| [205:115087](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=205-115087) | Smart Swap · Chi tiết vòng | 360×800 | Chi tiết vòng phía gửi: sơ đồ A→C→B→A, ba món, tiền bù trực tiếp; gửi cho hai người. |
| [236:18](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=236-18) | Smart Swap · Chi tiết vòng | 360×800 | Biến thể chi tiết phía nhận, Từ chối/Tham gia. |
| [205:115021](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=205-115021) | Smart Swap · Xác nhận 3 bên | 360×800 | Xác nhận 3 bên, trạng thái 2/3; góc nhìn C Thảo Vy, bàn giao còn khóa; Xác nhận. |
| [205:114968](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=205-114968) | Smart Swap · Chat nhóm | 360×800 | Chat nhóm SS-0301, 3/3 tham gia; trao đổi mức bù, read receipt, Điều chỉnh. |
| [236:150](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=236-150) | Smart Swap · Điều chỉnh tiền bù | 360×800 | Chat nhóm với sheet điều chỉnh tiền bù 500k, chọn người nhận Nam/Vy, xác nhận. |
| [205:114915](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=205-114915) | Smart Swap · Bàn giao | 360×800 | Bàn giao 5/6; ba cặp giao/nhận, tiền bù, tối đa 5 ảnh; chờ nhận iPad. |
| [205:114854](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=205-114854) | Smart Swap · Hoàn tất và đánh giá | 360×800 | Hoàn tất 6/6, nhận/giao và tiền bù; hai form nhận xét Nam/Vy, gửi 2 đánh giá. |

### Admin — 18 frame

| ID / Figma | Tên frame nguyên bản | Kích thước | Nội dung / trạng thái đã đọc |
| --- | --- | --- | --- |
| [205:116809](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=205-116809) | Admin · Tổng quan | 360×800 | Dashboard KPI, biểu đồ giao dịch, việc cần làm; Flow 2. |
| [205:116652](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=205-116652) | Admin · Menu quản trị đang mở | 360×800 | Dashboard với drawer mở; role, bảy mục quản trị, logout, thông tin phiên. |
| [205:116578](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=205-116578) | Admin · Quản lý người dùng | 360×800 | Quản lý user: tìm kiếm, active/locked, list bốn tài khoản và chỉ số. |
| [205:116512](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=205-116512) | Admin · Chi tiết người dùng hoạt động | 360×800 | Detail user active, liên hệ, tin/giao dịch/rating/vi phạm, nhật ký; khóa tài khoản. |
| [205:116413](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=205-116413) | Admin · Modal khóa tài khoản | 360×800 | Detail active với dialog khóa: lý do, 7/30 ngày/vĩnh viễn, cảnh báo quyền hạn. |
| [205:116345](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=205-116345) | Admin · Chi tiết người dùng đã khóa | 360×800 | Detail user khóa 7 ngày, lý do/hết hạn/audit; xác nhận mở khóa. |
| [205:116276](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=205-116276) | Admin · Quản lý sản phẩm | 360×800 | Quản lý sản phẩm: Pending/Visible/Rejected/Hidden, ba tin và tìm kiếm. |
| [205:116220](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=205-116220) | Admin · Duyệt chi tiết tin | 360×800 | Duyệt SP-2086: thông tin, owner, checklist, Từ chối/Duyệt tin. |
| [205:116138](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=205-116138) | Admin · Modal từ chối tin | 360×800 | Frame duyệt với dialog từ chối, lý do và ghi chú người đăng. |
| [205:116077](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=205-116077) | Admin · Chi tiết tin đang hiển thị | 360×800 | Tin đang hiển thị SP-2048, lịch sử, lý do và xác nhận ẩn tin. |
| [205:115976](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=205-115976) | Admin · Quản lý giao dịch | 360×800 | Quản lý giao dịch: tabs mua bán/trao đổi, Smart Swap, bộ lọc, SS/TD và tranh chấp. |
| [205:115767](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=205-115767) | Admin · Chi tiết giao dịch trao đổi | 360×800 | Detail giao dịch có section 1–1, vòng 3 bên, lịch sử/bằng chứng, form tranh chấp; cuộn dọc. |
| [205:115705](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=205-115705) | Admin · Danh sách khiếu nại | 360×800 | List khiếu nại New/Processing/Resolved, priority và hạn phản hồi; ba phiếu. |
| [205:115645](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=205-115645) | Admin · Chi tiết tranh chấp và bằng chứng | 360×800 | Detail KN-031, bên gửi/bị phản ánh, mô tả, ảnh/video/chat và tiếp nhận/xử lý. |
| [205:115596](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=205-115596) | Admin · Xử lý tranh chấp | 360×800 | Quyết định tranh chấp, căn cứ, kết luận, biện pháp, thông báo; không tự hoàn tiền. |
| [205:115492](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=205-115492) | Admin · Quản lý danh mục | 360×800 | Quản lý năm danh mục gốc, số tin/danh mục con, thứ tự, ẩn/hiện, thêm. |
| [205:115419](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=205-115419) | Admin · Thêm và chỉnh sửa danh mục | 360×800 | Tên frame ghi thêm/chỉnh sửa, nội dung đang là Thêm danh mục; chọn cha/icon/hiển thị. |
| [205:115300](https://www.figma.com/design/A0eyikaueyfowTKl4NuM9F/Nhom4?node-id=205-115300) | Admin · Báo cáo thống kê | 360×800 | Thống kê khoảng ngày, biểu đồ mua bán/trao đổi, phân bố danh mục, hiệu quả duyệt/khiếu nại. |

## 3. Màu sắc và phong cách

Nền trắng ngà pha xanh, chữ tối thiên xanh, màu thương hiệu xanh lá; card trắng, đường viền mảnh, nút bo tròn. Có hai nhóm màu gần nhau giữa phần chuẩn hóa mới và nhóm chat/đơn hàng cũ; không được tự coi các mã gần nhau là một token.

| Vai trò quan sát | Mã đã đọc | Bằng chứng / cách dùng |
| --- | --- | --- |
| Primary | `#28734F` | Logo, nút Đăng nhập `2:3010`, CTA đăng bán `114:64` |
| Nền ứng dụng | `#FCFDFB` | Frame Trang chủ `4:461`, Khám phá `205:118152` |
| Surface | `#FFFFFF` | Card, input, drawer, dialog, bottom navigation |
| Chữ chính | `#202E26` | Tiêu đề và nội dung trong nhóm màn chuẩn hóa |
| Chữ phụ | `#65746A` | Placeholder, thông tin phụ |
| Viền / divider | `#DCE4DD` | Input, card, thanh điều hướng; có biến thể `#DCE4DE` |
| Nền xanh nhạt | `#EAF3EC` | Khối hỗ trợ, trạng thái, biểu trưng |
| Cảnh báo / đang chờ | `#9B5E32` trên `#FAEEE4` | Chờ duyệt; thông báo tác động xử lý |
| Nguy hiểm / tranh chấp | `#AE413C` | Ưu tiên cao, tranh chấp, thao tác nguy hiểm |
| Nhóm chat/đơn cũ | `#16845B`, `#17211C`, `#66736B`, `#E1E8E3`, `#E8F5EE` | CTA thanh toán, tổng tiền, chữ và viền ở nhóm `44:*` |
| Scrim modal/menu/lọc | `#12261E`, opacity khoảng **43,92%** | `205:116479`, `205:116759`, `205:118108` |
| Scrim chỉnh tiền bù | `#102219`, opacity **40%** | `236:192` |

Các vai trò trên là cách diễn giải từ vị trí sử dụng, không phải tên variable có sẵn. Chưa có cơ sở xác nhận một bộ semantic token màu hoàn chỉnh. Chưa đọc được định nghĩa shadow/elevation thực; không tự thêm thông số bóng đổ vào bản đặc tả.

## 4. Typography: Roboto

Trong 83 frame có **1.958 text node**: **1.924 Roboto**, **34 Inter Regular 12**. Cả 34 node Inter chỉ chứa khoảng trắng; toàn bộ text có nội dung hiển thị sử dụng Roboto. Các node rỗng vẫn có thể chiếm chiều cao trong auto-layout, ví dụ `205:118230` cao 15.

| Vai trò / mẫu thực | Size Figma | Style / weight | Line height đã đọc |
| --- | --- | --- | --- |
| Logo Splash `2:2967` | 32 | Bold / 700 | 100% |
| Headline Splash `2:2975` | 28 | Bold / 700 | 1,20 × size |
| Chào đăng nhập `2:2997` | 28 | Bold / 700 | 1,15 × size |
| Logo trang chủ `4:475` | 26 | Bold / 700 | 100% |
| KPI Admin | 25 | Bold / 700 | 1,38 × size |
| App bar `205:118161` | 20 | SemiBold / 600 | 1,20 × size, tức 24 |
| Heading section `4:483` | 17 | SemiBold / 600 | 1,35 × size |
| Heading trao đổi `205:118181` | 17 | SemiBold / 600 | 1,38 × size |
| Tên sản phẩm `205:118198` | 16 | SemiBold / 600 | 1,38 × size |
| Giá tham khảo `205:118199` | 17 | Bold / 700 | 1,38 × size |
| Nội dung / nhãn form | 12–15 | Regular / Medium / SemiBold | Phải lấy theo node; không dùng một line height cho tất cả |
| Metadata / chip / nhãn nhỏ | 10–12 | Regular / SemiBold | Có text 8–9 ở một số màn cũ |

Roboto có các style Light 300, Regular 400, Medium 500, SemiBold 600, Bold 700 trong nguồn. Letter spacing phần lớn bằng 0. Đây không phải một type scale đồng nhất: có nhiều size 8–32 và các hệ số line height khác nhau. **Đề xuất** tạo tài nguyên typography theo vai trò, giữ ngoại lệ có chủ đích và rà soát khả năng đọc của text 8–10sp trước khi triển khai. Không kéo giãn chữ để ép khớp kích thước Figma.

## 5. Layout và kích thước Android

### 5.1 Kích thước thực

- **71/83 frame là 360×800**; **12 frame cao hơn 800**. Tất cả màn Admin là 360×800.
- Ngoại lệ: 811 × 3; 813 × 2; 819 × 1; 894 × 2; 901 × 1; 1203 × 1; 1325 × 1; 1872 × 1. ID cụ thể có trong bảng màn hình.
- Với Trang chủ: status bar **28**, app bar **56**, nội dung **649**, bottom navigation **67** → tổng **800**. App bar bắt đầu y=28, nội dung y=84, navigation y=733.
- Lề nội dung chuẩn **24 mỗi bên** → chiều rộng dùng được **312**. Có ngoại lệ app bar đăng bán dùng lề **20** và form trao đổi dùng section padding dọc **20/24**.
- Frame link `205:118152` cao 894: status 28 + app bar 56 + vùng nội dung 728 + bottom nav 67 + text trắng 15 = 894. Không phải viewport Android 360×800.

### 5.2 Số đo component đại diện

| Thành phần | Số đo đã đọc | Node bằng chứng |
| --- | --- | --- |
| Search | 312×46, radius 12, padding ngang 14 | `205:118164` |
| Segmented control | 312×44, radius 12, padding 3; hai tab 153×38 | `205:118176` |
| Card trao đổi lớn | 312×272, padding 12, radius 20; ảnh 288×110 | `205:118192`, `205:118193` |
| Card compact | 312×88; ảnh 64×64, khoảng cách ảnh–text 10 | `205:118202`, `205:118204` |
| CTA đăng bán | 312×50, radius 100 | `114:64` |
| Bottom nav | 360×67, 5 mục ~70,4×44; icon 20×20 trong vùng chỉ báo 52×28 | `205:118209` và các node con |
| Action footer | Ví dụ 360×74, padding 12/24; nhóm mới có footer 72 hoặc 68 | `114:63`, `205:116475`, `236:184` |
| Dialog khóa tài khoản | 312×482 ở x=24, y=159; padding 20; radius 20 | `205:116480` |
| Drawer Admin | 286×772; bắt đầu dưới status bar; padding 20 | `205:116760` |
| Bottom sheet bộ lọc | 360×514, y=286; padding 20; hai góc trên radius 24 | `205:118109` |
| Bottom sheet tiền bù | 360×410, y=390; padding ngang 24; góc trên radius 24 | `236:193` |

Gap được dùng nhiều: **8, 10, 5, 4, 12**; còn có 2, 3, 6, 14, 16, 18, 24… Radius phổ biến: **100** cho pill, **12** cho ô/khối nhỏ, **20** cho card/dialog; có radius 9, 14 và ngoại lệ khác. Không thể coi đây là spacing scale chỉ gồm bội số 4.

### 5.3 Auto-layout, cuộn và giới hạn

Nhóm account/catalog/đăng tin và phần mới dùng nhiều frame auto-layout ngang/dọc. Nhiều màn chat/đơn hàng cũ đặt text/shape trực tiếp theo tọa độ. Tên layer “Nội dung cuộn” không tự chứng minh có cấu hình scroll. Snapshot chỉ có **10 node đặt `scrollDirection=VERTICAL`**, nằm trong `205:115767`, `205:115976`, `205:116984`, `205:117122`, `205:117265`, `205:117605`, `205:117719`, `205:117877`, `205:117992`, `205:118152`.

Không xác nhận được hành vi cố định header/footer khi chạy chỉ từ tên “Hành động cố định”. Form `226:213` có section cuối y=1656, cao 234, kết thúc ở **1890**, vượt frame 1872 **18 đơn vị**; cần ảnh export để xác định phần bị cắt trong trình bày thực.

**Đề xuất triển khai:** dùng viewport tham chiếu 360×800, dp cho hình học và sp cho chữ; đo lại với font Android thực. Nội dung dài phải cuộn trong vùng giữa app bar và footer/nav; giữ CTA truy cập được khi bàn phím mở. Dùng system insets thật cho status/navigation bar, không vẽ thêm status bar “9:41” của mockup. Không scale toàn bộ frame 1872 xuống 800. Điều chỉnh 12 frame ngoại lệ bằng bố cục cuộn là quyết định triển khai cần kiểm tra bằng ảnh đối chiếu.

## 6. Icon và hình ảnh

- Có **623 VECTOR node** trong cây 83 màn, không đồng nghĩa 623 icon khác nhau. Các tên lớp gợi hệ icon nét: `arrow-left`, `search`, `house`, `user-round`, `circle-plus`, `message-circle`, `sliders-horizontal`, `badge-check`, `menu`, `users`, `chart-column`, `flag`, `layout-grid`, `paperclip`, `send`… Tên tương đồng một thư viện không đủ xác nhận thư viện gốc hoặc giấy phép.
- Kích thước quan sát: icon nav 20×20, icon search 18×18, biểu tượng filter 16×16; frame back rời 22×22. Icon trang trí nhỏ và vùng nhận chạm là hai kích thước khác nhau.
- Nhóm cũ còn dùng ký tự `↻`, `● ▰`, `✓`, `○` thay icon/status. Không được tự coi glyph này là vector asset hoàn chỉnh.
- ZIP có **38 file ảnh** trong `images/`. Đã đọc được các image fill, nhưng chưa đối chiếu trực quan từng ảnh gốc với từng màn. Thumbnail chỉ chứng minh có ảnh sản phẩm/placeholder trên canvas.
- **Đề xuất:** lập manifest asset theo node ID rồi xuất SVG/vector drawable và ảnh đúng tỉ lệ ở giai đoạn dựng UI; xác minh stroke, màu, crop và bản quyền trước khi chọn icon thay thế. Chưa chốt một thư viện icon hoặc tạo asset mới trong giai đoạn này.

## 7. Phân nhóm chức năng theo thiết kế

| Nhóm | Phạm vi đã đọc | Quy tắc / giới hạn nguồn |
| --- | --- | --- |
| Tài khoản | Splash, đăng nhập/đăng ký, khôi phục mật khẩu, đổi mật khẩu, hồ sơ và cài đặt | Đăng ký ghi tối thiểu 8 ký tự gồm chữ/số; link khôi phục 15 phút. Có mục xác minh, chính sách, xóa tài khoản nhưng không có frame chi tiết tương ứng |
| Mua bán | Home, tìm kiếm, danh mục, danh sách, chi tiết; liên hệ, mua ngay | Sáu danh mục trong màn danh mục; bộ lọc và sort mới thể hiện giá trị mẫu, chưa có đủ trạng thái thao tác |
| Đăng tin bán | Form ảnh/tên/danh mục/giá/tình trạng/mô tả/địa điểm; preview, gửi duyệt, quản lý, sửa, đánh dấu đã bán | Tối đa **8 ảnh**; trạng thái Đang bán, Chờ duyệt, Đã bán, Bản nháp. Có Ẩn/Xóa nhưng thiếu màn kết quả/xác nhận |
| Đăng tin trao đổi | Quản lý tin riêng `226:102`, form 4 section `226:213`, lưu nháp | Tối đa **6 ảnh**, JPG/PNG; mô tả món muốn nhận; tiền bù tối đa bắt buộc khi bật bù tiền, chấp nhận nhập 0 |
| Đơn hàng | Checkout theo giá gốc/đã thương lượng, danh sách, timeline, xác nhận nhận hàng, đánh giá, hỗ trợ | Có lời hứa giữ tiền đến khi nhận hàng; chưa chỉ rõ nhà cung cấp thanh toán, lựa chọn phương thức hoặc quy trình hoàn tiền |
| Trao đổi 1–1 | Khám phá/lọc, chọn đồ, tạo đề nghị, đề nghị gửi/nhận, phản đề nghị, chat, nhận hàng, chờ đối tác, đánh giá | Chỉ chọn tin đang hiển thị và chưa có giao dịch mới; tiền bù trả trực tiếp; phản đề nghị cần chấp nhận lại; chỉ hoàn tất khi cả hai xác nhận |
| Smart Swap 3 bên | Tìm vòng, hai vòng gợi ý, chi tiết gửi/nhận, đồng ý tham gia, chat nhóm, chỉnh tiền bù, bàn giao, đánh giá hai người | 3 người / 3 món; **3/3 đồng ý** mới mở bàn giao; **6/6 giao và nhận** mới hoàn tất; một người từ chối thì dừng vòng; tiền bù trực tiếp |
| Chat | Inbox, chat giá, phiếu giảm, chat trao đổi 1–1, chat nhóm | Có text nhập, đính kèm/icon và read receipt ở một số màn; không có prototype gửi text thật, retry hoặc trạng thái mất mạng |
| Admin | Dashboard/drawer; user lock/unlock; moderation; giao dịch; tranh chấp; danh mục; thống kê | Mobile 360×800; khóa 7/30 ngày hoặc vĩnh viễn; lưu lịch sử; tranh chấp chỉ đóng khi xác minh thực hiện; không tự xử lý tiền bù qua app |

Đăng tin trao đổi (`226:102`) và lịch sử đề nghị trao đổi (`205:117719`…) cùng dùng nhãn “Trao đổi của tôi” nhưng là **hai vùng nghiệp vụ khác nhau**. Không gộp thành một màn chỉ vì trùng nhãn.

## 8. Danh sách component UI có thể tái sử dụng

**Hiện trạng:** không tìm thấy node COMPONENT, COMPONENT_SET, INSTANCE hoặc SYMBOL trong file đã giải mã. “My Component” là tên page, không chứng minh có component library. Có một collection variable `Dimensions` và variable số `Grabber Height=4` (hai mode), chưa phải hệ token ReMarket đầy đủ. Danh sách dưới đây là **đề xuất tách component từ các cấu trúc lặp đã đọc**; trạng thái chưa thiết kế phải bổ sung sau.

| Component đề xuất | Props / biến thể cần hỗ trợ | Bằng chứng |
| --- | --- | --- |
| AppScreen / AppBar | title, back/menu, trailing action, body cuộn, footer/nav tùy màn | `4:461`, `205:116809` |
| UserBottomNavigation | 5 item, selected; giữ back stack theo tab | `4:532`, `205:118209` |
| AdminDrawer / DrawerItem | avatar, role, selected, badge 7, logout | `205:116760` |
| Primary/Secondary/DangerButton | text, icon, full/half width; disabled/loading là trạng thái cần thiết kế thêm | `114:64`, `205:116480` |
| LabeledTextField / PasswordField / MoneyField | label, required, helper, suffix VND, multiline, show/hide password | `2:3019`, `226:213`, `236:193` |
| SearchField | query, placeholder, leading/trailing icon | `205:118164` |
| SelectField / OptionRow | selected value, chevron, list radio/check | `205:115419`, `226:213` |
| FilterChip / StatusBadge / SegmentedTabs | selected, label/count, màu theo nghĩa | `205:118176`, `114:332` |
| ProductCard / ProductRow | ảnh, tên, giá, tình trạng, địa điểm; grid/list/compact | `4:507`, `205:118192`, `205:118202` |
| ListingManagementCard | stats, trạng thái, sửa/ẩn/xóa/quản lý | `114:192`, `226:102` |
| ProductGallery / ImageUploader | cover, count, add/remove; giới hạn 8 bán / 6 trao đổi / 5 bằng chứng | `114:2`, `226:213`, `205:117122` |
| Avatar / IdentityRow / VerifiedBadge | ảnh hoặc initials, tên, role, rating, trạng thái xác minh | `2:3096`, `205:117992` |
| InfoBanner / NoticeCard | info/success/warning, icon, title, body | `114:252`, `205:115234` |
| KeyValueRow / PriceSummary | label/value, tổng, người trả/người nhận | `44:191`, `205:115087` |
| OrderCard / OrderTimeline | mã, món, tổng tiền, từng mốc/thời điểm | `44:223`, `44:248` |
| SupportTicketCard / EvidenceList | mã, priority, SLA hiển thị, ảnh/video/chat, lịch sử | `90:78`, `205:115645` |
| ExchangePairCard / CompensationRow | đồ gửi/nhận, owner, direction, amount | `205:117877`, `205:117344` |
| OfferCard / OfferTimeline | sent/received/counter/complete, hạn phản hồi | `205:117719`, `205:117522` |
| SwapCycleDiagram / SwapParticipantRow | 3 participant, A→C→B→A, item, acceptance | `205:115087`, `205:115021` |
| HandoverChecklist / EvidenceUploader | 2 bên hoặc 3 bên; từng xác nhận, bằng chứng | `205:117122`, `205:114915` |
| ConversationRow / ChatBubble / Composer | 1–1/nhóm, sender, time, read receipt, attachment | `90:16`, `205:114968` |
| NegotiationOfferCard | mức giảm, chấp nhận/từ chối/mua, vai trò người xem | `85:2`, `188:2` |
| RatingForm / SuccessFeedback | chọn sao/tag, nhận xét; 1 đối tác hoặc 2 đối tác | `146:248`, `205:114854` |
| ConfirmDialog / BottomSheet / Scrim | lý do, tùy chọn, cancel/confirm, close behavior | `205:116480`, `205:118109`, `236:193` |
| AdminUserCard / ModerationChecklist | thông tin, trạng thái, audit, actions | `205:116578`, `205:116220` |
| KPIStatCard / TransactionBarChart / CategoryDistribution | số liệu, kỳ thời gian, series, legend | `205:116809`, `205:115300` |

## 9. Điều hướng Prototype đã đọc

### 9.1 Cơ chế và điểm bắt đầu

- **Flow 1:** `2:2955` Splash. **Flow 2:** `205:116809` Admin Tổng quan. Không có đường đăng nhập Admin hay chuyển role nối từ User trong nguồn đã đọc.
- **231 interaction đang hoạt động**, mỗi interaction có 1 action: **230 ON_CLICK**, **1 DRAG**. Tất cả action là `INTERNAL_NODE / NAVIGATE`; không thấy action OVERLAY, BACK, CLOSE, CHANGE_TO, URL, SET_VARIABLE hoặc conditional action.
- Vì vậy các “modal”, “drawer” và “sheet” là **frame trạng thái riêng có lớp nền bên dưới**, được mở bằng NAVIGATE; chưa phải overlay tương tác native trong Prototype.
- 29 action ghi rõ `INSTANT_TRANSITION`; 201 action không lưu loại transition trong snapshot; 1 `SMART_ANIMATE`. Không tự gán duration cho những action thiếu thông số.
- DRAG ở node `225:90` trong Cài đặt → `205:117719`: MCP đọc được Smart Animate **0,3 giây, EASE_OUT**. Cùng node ON_CLICK lại đi `44:223` Đơn hàng: đây là bất nhất cần sửa.
- Không thấy AFTER_TIMEOUT: Splash hiện chuyển bằng click, không có căn cứ khẳng định tự chuyển sau vài giây.

### 9.2 Các chuỗi điều hướng chính đang nối

| Luồng | Chuỗi thực tế / nhánh chính |
| --- | --- |
| Tài khoản | Splash → Login → Home; Login ↔ Register; Login → Forgot → Login; Settings → Profile ↔ Edit; Settings → Change password → Login |
| Catalog | Home → Search; Home → Categories → List → Detail bàn; Home → 3 Detail theo card |
| Mua trực tiếp | Detail iPhone `4:755` → Checkout gốc `194:72` → Orders `44:223` |
| Thương lượng giá | Inbox `90:16` → Chat `128:139` → Phiếu giảm `128:174` → Chat chấp nhận `188:2` → Chat mua `85:2` → Checkout giảm `44:191` → Orders |
| Đơn / hỗ trợ / đánh giá | Orders → một trong hai Detail; `77:65` → Review → Review success; `77:65` → Support → Tracking. Settings → History `90:78` → New support `128:216` hoặc Tracking |
| Đăng bán | Main tab → Manage `114:332` → Form `114:2` → Preview `114:292` → Success `114:252` → Manage; Manage → Detail → Edit hoặc Confirm sold |
| Tin trao đổi | Explore `205:118152` → My exchange listings `226:102` → Form `226:213` → My listings. Nhánh này khác lịch sử đề nghị |
| Đề nghị 1–1 | Explore → Detail exchange → Pick mine → Create offer → Received detail `205:117393`; Counter → quay về Received detail; Accept → Sent `205:117719` → Confirm receipt `205:117122` → Review `205:116984` → Review success |
| Smart Swap | Explore → Find cycle → Matches → Cycle detail `205:115087` → Recipient detail `236:18` → Negotiating `205:117522` → Group chat `205:114968` → Compensation sheet `236:150` → Consent `205:115021` → Handover `205:114915` → Completion `205:114854` |
| Admin | Overview → Menu → Users / Products / Transactions / Complaints / Categories / Reports; Users → Active detail → Lock modal → Locked detail; Products → Review detail → Reject modal; Complaints → Evidence → Decision; Categories → Add form |

Các chuỗi trên mô tả dây nối hiện hữu, kể cả chỗ sai nghiệp vụ. Phụ lục A liệt kê đầy đủ **231 cạnh** với ID node nguồn/đích để kiểm tra từng hotspot.

## 10. Trạng thái nghiệp vụ được thể hiện và phần còn thiếu

| Vùng | Trạng thái / ràng buộc có bằng chứng | Thiếu hoặc chưa nối đủ |
| --- | --- | --- |
| Account | Form mặc định, hồ sơ mẫu, sửa, đổi/khôi phục mật khẩu | Validation, login sai, đang gửi, email đã gửi, link hết hạn, xác minh, xóa tài khoản |
| Catalog | Có kết quả, kết thúc kết quả, filter chip giá trị mẫu | Loading, rỗng thật sự, lỗi mạng, lọc/sort tương tác, sản phẩm bị gỡ |
| Tin bán | Chờ duyệt, Đang bán, Đã bán, Bản nháp trong tab/copy | Hầu hết tab chưa có frame riêng; trạng thái sau ẩn/xóa, upload lỗi |
| Order | Đã đặt/đã thanh toán/đang vận chuyển/đã nhận trong timeline; tabs hoàn tất/đã hủy | Lỗi/hủy thanh toán, webhook, refund, trường hợp người mua chưa xác nhận |
| Đề nghị 1–1 | Chờ phản hồi, counter 300k→400k, thương lượng, chờ đối tác, hai bên hoàn tất, đánh giá | Từ chối, hủy, hết hạn chỉ có nhãn/copy; route chat/handover chưa nhất quán |
| Smart Swap | Matches; 2/3→3/3; handover 5/6→6/6; 2 đánh giá | Không tìm thấy vòng, người rút lui/hết hạn, đổi khoản bù sau đồng ý, lỗi ảnh; nhiều CTA không có cạnh |
| Admin | Active/locked + confirm unlock; pending/reject modal/hide confirm; complaint new/processing/resolved trong tab | Kết quả duyệt/từ chối hợp lệ, refresh số liệu, quyền hạn khác, trạng thái edit danh mục riêng |

### Quy tắc 1–1 có thể rút từ text

Đề nghị mẫu `TD-1024`: Minh Anh gửi Sony 1.200.000đ để nhận Kindle 1.600.000đ; ban đầu bù 300.000đ, Nam đề xuất lại 400.000đ. Phản đề nghị chưa thay đổi thỏa thuận cho tới khi bên kia chấp nhận. Nhãn đề nghị ghi hạn 24 giờ. Tiền bù trả trực tiếp, không thanh toán trong ứng dụng. Form nhận hàng ghi tối đa 5 ảnh JPG/PNG và chỉ hoàn tất khi hai bên đã xác nhận; không được triển khai việc bấm một nút là tự hoàn tất cho cả hai.

### Quy tắc Smart Swap có thể rút từ text

Vòng `SS-0301`: **A Minh Anh giao laptop cho C Thảo Vy; C giao Canon cho B Hoàng Nam; B giao iPad cho A**. A trả B 500.000đ, C trả B 500.000đ, B nhận trực tiếp 1.000.000đ. Đây là khoản bù được đề xuất/thỏa thuận, không đủ căn cứ suy ra công thức tự động lấy chênh lệch định giá.

Cần đủ 3/3 đồng ý trước bàn giao; một người từ chối làm vòng dừng. Mỗi người có xác nhận giao và nhận riêng → 6/6. Màn bàn giao thể hiện 5/6, đang chờ A nhận iPad; hoàn tất thể hiện 6/6 và đánh giá B/C. Đề nghị gửi có copy hết hạn 48 giờ. Không thấy đặc tả thuật toán matching, xếp hạng, giữ chỗ món đồ hoặc chính sách tái xác nhận khi chỉnh tiền bù.

## 11. Bất nhất và câu hỏi cần giải quyết trước triển khai

| Mức | Bằng chứng | Vấn đề / quyết định còn thiếu |
| --- | --- | --- |
| P0 | `226:115` → `4294967295:4294967295` | Đích không tồn tại. Phải bỏ/sửa liên kết; không tạo màn hình tưởng tượng cho ID này |
| P0 | Settings node `225:90` | Click “Trao đổi của tôi” mở Orders, còn drag mở Sent offers. Cần xác định entry point lịch sử đề nghị |
| P0 | `205:117581` | “Mở chat trao đổi” của TD-1024 lại mở nhóm SS-0301; đề xuất route theo loại giao dịch |
| P0 | `240:279` ở `205:117447` | Card TD-1024 “Xem chi tiết” mở bàn giao Smart Swap; cần route 1–1 riêng |
| P0 | `205:117670` | Đề nghị nhận được đang Chờ phản hồi lại mở “Chờ Nam xác nhận” giao nhận, bỏ qua xem/chấp nhận đề nghị |
| P0 | `205:117262` → review; copy ở `205:117122`, `205:117061` | Prototype nhảy từ xác nhận cá nhân thẳng tới hoàn tất, chưa thể hiện chờ đồng bộ đối tác. Cần chặn theo dữ liệu hai bên |
| P0 | `205:116218` trong reject modal | “Xác nhận từ chối” SP-2086 lại mở chi tiết tin hiển thị SP-2048. Không được dùng đó làm kết quả reject |
| P0 | Checkout và tiền bù trao đổi | Hai cơ chế khác nhau: checkout nói giữ tiền, còn swap trả trực tiếp. Chưa có đặc tả tích hợp/custody tiền mua bán |
| P1 | Frame `90:16`, `90:78`, `92:558` | Tên layer không khớp nội dung: lần lượt Inbox, lịch sử hỗ trợ, đổi mật khẩu; không đặt route theo tên layer sai |
| P1 | Đăng ký `2:3026`, `2:3028` | Header và title lồng nhau có hotspot đi hai đích khác nhau; nút Đăng ký thực chưa có dây nối riêng |
| P1 | Detail iPhone `4:794` và CTA con | Hotspot cả hàng đi chat, “Mua ngay” con đi checkout; cần tách vùng nhận chạm |
| P1 | Quản lý tin `114:192` | Cả hàng Chỉnh sửa/Đã bán có hotspot đã bán, trong khi nút sửa con mở Edit; tránh action chồng lấn |
| P1 | `205:118147` | Cả hàng Xóa lọc/Áp dụng cùng dẫn Explore; không chứng minh Xóa lọc có reset dữ liệu |
| P1 | `205:115989` | Toàn bộ vùng nội dung Quản lý giao dịch trỏ một Detail chung; không có cạnh riêng cho từng mã |
| P1 | Admin `205:115767` | Cùng frame có section 1–1, 3 bên và bằng chứng; chưa có reaction cho control chuyển ngữ cảnh. Không khẳng định tabs thực sự đổi dữ liệu |
| P1 | Nút Duyệt tin, Sửa/Ẩn/Xóa tin trao đổi, một số nút gửi đánh giá/chat/báo cáo | Có nhãn nhưng không có interaction riêng; cần đặc tả kết quả, không coi là đã hoàn thành Prototype |
| P1 | TD-1024 / SS-0301 | Có lẫn góc nhìn Minh Anh/Nam/Vy; Sent hiển thị “Hoàn tất” nhưng còn hạn 24h và bù 300k; copy completed còn “chờ xác nhận”; cần chuẩn hóa theo role/state |
| P1 | Admin “Chờ 3/3 nhận” và User “6/6” | Chỉ tiêu 3 lượt nhận khác 6 lượt giao/nhận; cần lưu dữ liệu chi tiết rồi hiển thị đúng mẫu số |
| P1 | iPhone bị gắn “Nội thất”; kệ 320k ở list / 350k ở detail; order/card có mã hoặc tên máy lệch | Dữ liệu mẫu không nhất quán; không dùng làm ràng buộc nghiệp vụ |
| P1 | `146:248`, `146:351` | Có nhóm text/header lặp trong cây; cần ảnh export để xác định chồng lớp thực tế trước khi dựng |
| P1 | 12 frame cao hơn 800; text trắng cao 15; form `226:213` tràn 18 | Rà soát scroll, insets, footer và clipping trên viewport mục tiêu |
| P2 | Tab thứ hai `205:118039` ghi “Tìm kiếm”, các màn chính ghi “Trao đổi” | Thống nhất nhãn main navigation |

Không có bằng chứng về màn đăng nhập Admin riêng, notification center, ví tiền, OTP, dashboard desktop, bản đồ, chatbot AI hay màn lỗi/empty hoàn chỉnh. Những phần đó không được đưa vào danh sách màn đã có. Chưa thể xác nhận hình dạng mọi vector hoặc trạng thái prototype khi thao tác thực vì chưa có ảnh export đầy đủ.

## 12. Handoff cho kế hoạch triển khai

Xem [IMPLEMENTATION_PLAN.md](IMPLEMENTATION_PLAN.md) để biết cấu trúc Android/backend đề xuất, nhóm route, mô hình dữ liệu, mốc triển khai và tiêu chí nghiệm thu. Các sửa đổi hành vi được đề xuất ở đó phải được phân biệt với Prototype đang tồn tại. Mọi con số tiền, ngày, tên và KPI trong mockup là dữ liệu mẫu, không phải dữ liệu vận hành hiện tại.

## Phụ lục A — Toàn bộ liên kết Prototype

Nguồn: `prototypeInteractions` không bị xóa trong bản `.fig`. Mỗi dòng là một interaction/action; giữ cả hotspot lồng nhau, tự trỏ và liên kết lỗi. “Nguồn” là frame chứa hotspot; “Hotspot” ghi node ID và nhãn ngắn/layer name. Nhãn dài của cả vùng được biểu diễn bằng tên layer để bảng không lặp toàn bộ nội dung. Các ID ở bảng màn hình mở được trực tiếp trên Figma.

| Frame nguồn | Hotspot: ID · nhãn/layer | Trigger | Frame đích | Transition trong snapshot |
| --- | --- | --- | --- | --- |
| 4:461 | 4:480 · Tìm món đồ bạn cần | ON_CLICK | 4:556 | INSTANT_TRANSITION |
| 4:461 | 4:484 · Tất cả | ON_CLICK | 4:617 | INSTANT_TRANSITION |
| 4:461 | 4:507 · Iphone 13 128gb  /  7.200.000 đ  /  Như mới  /  Quận 1, TP. HCM | ON_CLICK | 4:755 | Không lưu |
| 4:461 | 4:514 · Bàn trà gỗ nhỏ  /  650.000 đ  /  Đã sử dụng · Tốt  /  Bình Thạnh, TP. HCM | ON_CLICK | 128:6 | Không lưu |
| 4:461 | 4:521 · Sản phẩm mới | ON_CLICK | 128:67 | Không lưu |
| 4:461 | 4:537 · Trao đổi | ON_CLICK | 205:118152 | Không lưu |
| 4:461 | 4:541 · Đăng bán | ON_CLICK | 114:332 | Không lưu |
| 4:461 | 4:545 · Tin nhắn | ON_CLICK | 90:16 | Không lưu |
| 4:461 | 4:549 · Cá nhân | ON_CLICK | 2:3179 | Không lưu |
| 4:556 | 4:567 · Quay lại | ON_CLICK | 4:461 | Không lưu |
| 4:617 | 4:628 · Quay lại | ON_CLICK | 4:461 | Không lưu |
| 4:617 | 4:636 · Bạn đang tìm gì? | ON_CLICK | 4:684 | INSTANT_TRANSITION |
| 4:617 | 4:643 · Nội thất | ON_CLICK | 4:684 | INSTANT_TRANSITION |
| 4:684 | 4:695 · Quay lại | ON_CLICK | 4:617 | Không lưu |
| 4:684 | 172:14 · Bàn trà gỗ nhỏ  /  650.000 đ  /  Đã sử dụng · Tốt  /  Bình Thạnh, TP. HCM | ON_CLICK | 128:6 | Không lưu |
| 4:755 | 4:766 · Quay lại | ON_CLICK | 4:461 | Không lưu |
| 4:755 | 4:794 · Liên hệ  /  Mua ngay | ON_CLICK | 128:139 | Không lưu |
| 4:755 | 4:797 · Mua ngay | ON_CLICK | 194:72 | Không lưu |
| 4:755 | 4:798 · Mua ngay | ON_CLICK | 194:72 | INSTANT_TRANSITION |
| 128:6 | 128:17 · Quay lại | ON_CLICK | 4:461 | Không lưu |
| 128:67 | 128:77 · Chi tiết sản phẩm | ON_CLICK | 4:461 | Không lưu |
| 128:67 | 128:78 · Quay lại | ON_CLICK | 4:461 | Không lưu |
| 2:3179 | 2:3193 · Đơn hàng của tôi  /  Xem và cập nhật hồ sơ của bạn | ON_CLICK | 44:223 | Không lưu |
| 2:3179 | 225:90 · Trao đổi của tôi  /  Xem và cập nhật hồ sơ của bạn | ON_CLICK | 44:223 | Không lưu |
| 2:3179 | 225:90 · Trao đổi của tôi  /  Xem và cập nhật hồ sơ của bạn | DRAG | 205:117719 | SMART_ANIMATE |
| 2:3179 | 2:3200 · Yêu cầu hỗ trợ  /  Bảo vệ tài khoản bằng mật khẩu mới | ON_CLICK | 90:78 | Không lưu |
| 2:3179 | 92:467 · Thông tin cá nhân | ON_CLICK | 2:3096 | INSTANT_TRANSITION |
| 2:3179 | 92:471 · Đổi mật khẩu  /  Bảo vệ tài khoản bằng mật khẩu mới | ON_CLICK | 92:558 | Không lưu |
| 2:3179 | 2:3237 · Đăng xuất | ON_CLICK | 2:2983 | INSTANT_TRANSITION |
| 2:3179 | 92:524 · Trang chủ | ON_CLICK | 4:461 | Không lưu |
| 2:3179 | 92:529 · Trao đổi | ON_CLICK | 205:118152 | Không lưu |
| 2:3179 | 92:534 · Đăng bán | ON_CLICK | 114:332 | Không lưu |
| 2:3179 | 92:539 · Tin nhắn | ON_CLICK | 90:16 | Không lưu |
| 2:3139 | 2:3477 · arrow-left | ON_CLICK | 2:3096 | Không lưu |
| 2:3139 | 2:3175 · Lưu thay đổi | ON_CLICK | 2:3096 | INSTANT_TRANSITION |
| 2:3096 | 2:3447 · arrow-left | ON_CLICK | 2:3179 | Không lưu |
| 2:3096 | 2:3135 · Chỉnh sửa | ON_CLICK | 2:3139 | INSTANT_TRANSITION |
| 2:3064 | 2:3429 · arrow-left | ON_CLICK | 2:2983 | Không lưu |
| 2:3064 | 2:3086 · Gửi liên kết khôi phục | ON_CLICK | 2:2983 | INSTANT_TRANSITION |
| 2:3064 | 2:3087 · Quay lại đăng nhập | ON_CLICK | 2:2983 | INSTANT_TRANSITION |
| 92:558 | 92:585 · Xác nhận | ON_CLICK | 2:2983 | INSTANT_TRANSITION |
| 2:3019 | 2:3026 · Đăng ký | ON_CLICK | 2:2983 | Không lưu |
| 2:3019 | 2:3028 · Đăng ký | ON_CLICK | 4:461 | INSTANT_TRANSITION |
| 2:3019 | 2:3060 · Đã có tài khoản? Đăng nhập | ON_CLICK | 2:2983 | INSTANT_TRANSITION |
| 2:2983 | 2:3009 · Quên mật khẩu? | ON_CLICK | 2:3064 | INSTANT_TRANSITION |
| 2:2983 | 2:3011 · Đăng nhập | ON_CLICK | 4:461 | INSTANT_TRANSITION |
| 2:2983 | 2:3012 · Chưa có tài khoản? Đăng ký | ON_CLICK | 2:3019 | INSTANT_TRANSITION |
| 2:2955 | 2:2955 · Splash · ReMarket | ON_CLICK | 2:2983 | Không lưu |
| 2:2955 | 2:2976 · Mua bán và trao đổi đồ cũ tại Việt Nam. Cùng nhau sống xanh hơn mỗi ngày. | ON_CLICK | 4:461 | INSTANT_TRANSITION |
| 85:2 | 128:135 · Mũi tên quay lại | ON_CLICK | 90:16 | Không lưu |
| 85:2 | 85:21 · Reject offer | ON_CLICK | 128:139 | Không lưu |
| 85:2 | 85:23 · Accept offer | ON_CLICK | 44:191 | INSTANT_TRANSITION |
| 85:2 | 85:24 · Mua hàng | ON_CLICK | 44:191 | INSTANT_TRANSITION |
| 128:139 | 128:144 · Mũi tên quay lại | ON_CLICK | 90:16 | Không lưu |
| 128:139 | 128:163 · Accept offer | ON_CLICK | 128:174 | Không lưu |
| 188:2 | 188:7 · Mũi tên quay lại | ON_CLICK | 90:16 | Không lưu |
| 188:2 | 190:66 · Accept offer | ON_CLICK | 85:2 | INSTANT_TRANSITION |
| 188:2 | 190:67 · Chấp nhận | ON_CLICK | 85:2 | INSTANT_TRANSITION |
| 44:191 | 44:222 · Thanh toán an toàn | ON_CLICK | 44:223 | INSTANT_TRANSITION |
| 44:191 | 88:2 · Mũi tên quay lại | ON_CLICK | 85:2 | Không lưu |
| 194:72 | 194:106 · Thanh toán an toàn | ON_CLICK | 44:223 | INSTANT_TRANSITION |
| 194:72 | 194:107 · Mũi tên quay lại | ON_CLICK | 4:755 | Không lưu |
| 44:223 | 44:235 · Product card · iPhone 13 · 128GB | ON_CLICK | 44:248 | Không lưu |
| 44:223 | 77:54 · Product card · iPhone 13 · 128GB | ON_CLICK | 77:65 | Không lưu |
| 44:223 | 88:4 · Mũi tên quay lại | ON_CLICK | 2:3179 | Không lưu |
| 90:16 | 90:27 · Product card · iPhone 13 · 128GB | ON_CLICK | 128:139 | Không lưu |
| 90:16 | 92:642 · Trang chủ | ON_CLICK | 4:461 | Không lưu |
| 90:16 | 92:647 · Trao đổi | ON_CLICK | 205:118152 | Không lưu |
| 90:16 | 92:652 · Đăng bán | ON_CLICK | 114:332 | Không lưu |
| 90:16 | 92:657 · Tin nhắn | ON_CLICK | 90:16 | Không lưu |
| 90:16 | 92:662 · Cá nhân | ON_CLICK | 2:3179 | Không lưu |
| 90:78 | 90:87 · Product card · iPhone 13 · 128GB | ON_CLICK | 44:303 | Không lưu |
| 90:78 | 90:97 · Mũi tên quay lại | ON_CLICK | 2:3179 | Không lưu |
| 90:78 | 91:105 · Button · Gửi yêu cầu mới | ON_CLICK | 128:216 | Không lưu |
| 90:78 | 91:106 · Gửi yêu cầu mới | ON_CLICK | 128:216 | INSTANT_TRANSITION |
| 44:248 | 44:278 · Đã nhận được hàng | ON_CLICK | 44:223 | INSTANT_TRANSITION |
| 44:248 | 88:8 · Mũi tên quay lại | ON_CLICK | 44:223 | Không lưu |
| 77:65 | 77:93 · Button · Đã nhận được hàng | ON_CLICK | 146:248 | Không lưu |
| 77:65 | 77:106 · Button · Đã nhận được hàng | ON_CLICK | 44:279 | Không lưu |
| 77:65 | 88:6 · Mũi tên quay lại | ON_CLICK | 44:223 | Không lưu |
| 44:279 | 44:284 · Gửi yêu cầu hỗ trợ | ON_CLICK | 44:303 | INSTANT_TRANSITION |
| 44:279 | 44:301 · Button · Gửi yêu cầu hỗ trợ | ON_CLICK | 44:303 | Không lưu |
| 44:279 | 89:10 · Mũi tên quay lại | ON_CLICK | 77:65 | Không lưu |
| 44:303 | 44:327 · Gửi yêu cầu mới | ON_CLICK | 90:78 | INSTANT_TRANSITION |
| 44:303 | 89:12 · Mũi tên quay lại | ON_CLICK | 44:223 | Không lưu |
| 114:2 | 114:13 · Quay lại | ON_CLICK | 114:332 | Không lưu |
| 114:2 | 114:64 · Đăng bán | ON_CLICK | 114:292 | Không lưu |
| 114:66 | 114:75 · Biểu tượng | ON_CLICK | 114:192 | Không lưu |
| 114:66 | 114:111 · Xác nhận đã bán | ON_CLICK | 114:332 | Không lưu |
| 114:66 | 114:113 · Hủy · Tiếp tục bán | ON_CLICK | 114:192 | Không lưu |
| 114:115 | 114:124 · Biểu tượng | ON_CLICK | 114:192 | Không lưu |
| 114:115 | 114:127 · Hủy | ON_CLICK | 114:192 | Không lưu |
| 114:115 | 114:190 · Lưu thay đổi | ON_CLICK | 114:192 | Không lưu |
| 114:192 | 114:201 · Biểu tượng | ON_CLICK | 114:332 | Không lưu |
| 114:192 | 114:242 · Chỉnh sửa  /  Đánh dấu đã bán | ON_CLICK | 114:66 | Không lưu |
| 114:192 | 114:243 · Chỉnh sửa | ON_CLICK | 114:115 | Không lưu |
| 114:252 | 114:288 · Về quản lý tin | ON_CLICK | 114:332 | Không lưu |
| 114:252 | 114:290 · Đăng thêm tin | ON_CLICK | 114:2 | Không lưu |
| 114:292 | 114:301 · Biểu tượng | ON_CLICK | 114:2 | Không lưu |
| 114:292 | 114:329 · Xác nhận đăng tin | ON_CLICK | 114:252 | Không lưu |
| 114:292 | 114:331 · Quay lại chỉnh sửa | ON_CLICK | 114:2 | Không lưu |
| 114:332 | 114:343 · Đăng tin mới | ON_CLICK | 114:2 | Không lưu |
| 114:332 | 114:378 · Quản lý › | ON_CLICK | 114:192 | Không lưu |
| 114:332 | 114:411 · Trang chủ | ON_CLICK | 4:461 | Không lưu |
| 114:332 | 114:416 · Trao đổi | ON_CLICK | 205:118152 | Không lưu |
| 114:332 | 114:426 · Tin nhắn | ON_CLICK | 90:16 | Không lưu |
| 114:332 | 114:431 · Cá nhân | ON_CLICK | 2:3179 | Không lưu |
| 128:174 | 128:208 · Hoàn Tất | ON_CLICK | 188:2 | INSTANT_TRANSITION |
| 128:174 | 128:209 · Mũi tên quay lại | ON_CLICK | 128:139 | Không lưu |
| 128:216 | 128:237 · Button · Gửi yêu cầu hỗ trợ | ON_CLICK | 44:303 | Không lưu |
| 128:216 | 128:239 · Mũi tên quay lại | ON_CLICK | 90:78 | Không lưu |
| 146:248 | 146:270 · Mũi tên quay lại | ON_CLICK | 90:78 | Không lưu |
| 146:248 | 146:333 · Button · Gửi yêu cầu hỗ trợ | ON_CLICK | 146:351 | Không lưu |
| 146:248 | 146:335 · Mũi tên quay lại | ON_CLICK | 77:65 | Không lưu |
| 146:351 | 146:362 · Button · Gửi yêu cầu hỗ trợ | ON_CLICK | 44:303 | Không lưu |
| 146:351 | 146:364 · Mũi tên quay lại | ON_CLICK | 90:78 | Không lưu |
| 146:351 | 146:387 · Button · Gửi yêu cầu hỗ trợ | ON_CLICK | 77:65 | Không lưu |
| 146:351 | 146:389 · Mũi tên quay lại | ON_CLICK | 146:248 | Không lưu |
| 205:114854 | 205:114862 · Biểu tượng | ON_CLICK | 205:114915 | Không lưu |
| 205:114915 | 205:114923 · Biểu tượng | ON_CLICK | 205:117447 | Không lưu |
| 205:114915 | 205:114965 · Xác nhận đã nhận iPad từ Nam | ON_CLICK | 205:114854 | Không lưu |
| 205:114968 | 236:246 · Điều chỉnh | ON_CLICK | 236:150 | Không lưu |
| 205:115021 | 205:115029 · Biểu tượng | ON_CLICK | 205:114968 | Không lưu |
| 205:115021 | 205:115085 · Xác nhận | ON_CLICK | 205:114915 | Không lưu |
| 205:115087 | 205:115095 · Biểu tượng | ON_CLICK | 205:115155 | Không lưu |
| 205:115087 | 205:115153 · Gửi đề nghị cho 2 thành viên | ON_CLICK | 236:18 | Không lưu |
| 236:18 | 236:26 · Biểu tượng | ON_CLICK | 205:117605 | Không lưu |
| 236:18 | 236:126 · Từ chối  /  Tham gia | ON_CLICK | 205:117605 | Không lưu |
| 236:18 | 236:129 · Tham gia | ON_CLICK | 205:117522 | Không lưu |
| 205:115155 | 205:115163 · Biểu tượng | ON_CLICK | 205:115234 | Không lưu |
| 205:115155 | 205:115193 · Xem chi tiết | ON_CLICK | 205:115087 | Không lưu |
| 205:115234 | 205:115242 · Biểu tượng | ON_CLICK | 205:118152 | Không lưu |
| 205:115234 | 205:115298 · Tìm vòng phù hợp | ON_CLICK | 205:115155 | Không lưu |
| 205:115300 | 205:115308 · menu | ON_CLICK | 205:116652 | Không lưu |
| 205:115419 | 205:115427 · Biểu tượng | ON_CLICK | 205:115492 | Không lưu |
| 205:115419 | 205:115487 · Hủy | ON_CLICK | 205:115492 | Không lưu |
| 205:115419 | 205:115489 · Lưu danh mục | ON_CLICK | 205:115492 | Không lưu |
| 205:115492 | 205:115500 · menu | ON_CLICK | 205:116652 | Không lưu |
| 205:115492 | 205:115592 · + Thêm danh mục | ON_CLICK | 205:115419 | Không lưu |
| 205:115596 | 205:115604 · Biểu tượng | ON_CLICK | 205:115645 | Không lưu |
| 205:115596 | 205:115641 · Xác nhận quyết định xử lý | ON_CLICK | 205:115705 | Không lưu |
| 205:115645 | 205:115653 · Biểu tượng | ON_CLICK | 205:115705 | Không lưu |
| 205:115645 | 205:115698 · Tiếp nhận | ON_CLICK | 205:115596 | Không lưu |
| 205:115645 | 205:115702 · Xử lý tranh chấp | ON_CLICK | 205:115596 | Không lưu |
| 205:115705 | 205:115713 · menu | ON_CLICK | 205:116652 | Không lưu |
| 205:115705 | 205:115730 · Thẻ thông tin | ON_CLICK | 205:115645 | Không lưu |
| 205:115767 | 205:115775 · Biểu tượng | ON_CLICK | 205:115976 | Không lưu |
| 205:115976 | 205:115984 · menu | ON_CLICK | 205:116652 | Không lưu |
| 205:115976 | 205:115989 · Nội dung · Cuộn dọc | ON_CLICK | 205:115767 | Không lưu |
| 205:116077 | 205:116085 · Biểu tượng | ON_CLICK | 205:116220 | Không lưu |
| 205:116077 | 205:116132 · Hủy | ON_CLICK | 205:116220 | Không lưu |
| 205:116077 | 205:116134 · Xác nhận ẩn tin | ON_CLICK | 205:116276 | Không lưu |
| 205:116138 | 205:116198 · x | ON_CLICK | 205:116220 | Không lưu |
| 205:116138 | 205:116216 · Hủy | ON_CLICK | 205:116220 | Không lưu |
| 205:116138 | 205:116218 · Xác nhận từ chối | ON_CLICK | 205:116077 | Không lưu |
| 205:116220 | 205:116228 · Biểu tượng | ON_CLICK | 205:116276 | Không lưu |
| 205:116220 | 205:116271 · Từ chối | ON_CLICK | 205:116138 | Không lưu |
| 205:116276 | 205:116284 · menu | ON_CLICK | 205:116652 | Không lưu |
| 205:116276 | 205:116317 · Kiểm duyệt tin › | ON_CLICK | 205:116220 | Không lưu |
| 205:116345 | 205:116353 · Biểu tượng | ON_CLICK | 205:116413 | Không lưu |
| 205:116345 | 205:116404 · Hủy | ON_CLICK | 205:116578 | Không lưu |
| 205:116345 | 205:116406 · Mở khóa | ON_CLICK | 205:116578 | Không lưu |
| 205:116413 | 205:116483 · x | ON_CLICK | 205:116512 | Không lưu |
| 205:116413 | 205:116508 · Hủy | ON_CLICK | 205:116512 | Không lưu |
| 205:116413 | 205:116510 · Xác nhận khóa | ON_CLICK | 205:116345 | Không lưu |
| 205:116512 | 205:116520 · Biểu tượng | ON_CLICK | 205:116578 | Không lưu |
| 205:116512 | 205:116574 · Khóa tài khoản | ON_CLICK | 205:116413 | Không lưu |
| 205:116578 | 205:116586 · menu | ON_CLICK | 205:116652 | Không lưu |
| 205:116578 | 205:116603 · Thẻ thông tin | ON_CLICK | 205:116512 | Không lưu |
| 205:116652 | 205:116773 · Tổng quan | ON_CLICK | 205:116809 | Không lưu |
| 205:116652 | 205:116777 · Người dùng | ON_CLICK | 205:116578 | Không lưu |
| 205:116652 | 205:116781 · Sản phẩm | ON_CLICK | 205:116276 | Không lưu |
| 205:116652 | 205:116785 · Giao dịch | ON_CLICK | 205:115976 | Không lưu |
| 205:116652 | 205:116789 · Khiếu nại  /  7 | ON_CLICK | 205:115705 | Không lưu |
| 205:116652 | 205:116794 · Danh mục | ON_CLICK | 205:115492 | Không lưu |
| 205:116652 | 205:116798 · Báo cáo | ON_CLICK | 205:115300 | Không lưu |
| 205:116809 | 205:116817 · menu | ON_CLICK | 205:116652 | Không lưu |
| 205:116916 | 205:116924 · Biểu tượng | ON_CLICK | 205:116984 | Không lưu |
| 205:116916 | 205:116958 · Về khám phá trao đổi | ON_CLICK | 205:118152 | Không lưu |
| 205:116916 | 205:116960 · Trao đổi của tôi | ON_CLICK | 205:117719 | Không lưu |
| 205:116984 | 205:116992 · Biểu tượng | ON_CLICK | 205:117122 | Không lưu |
| 205:116984 | 205:117058 · Gửi đánh giá | ON_CLICK | 205:116916 | Không lưu |
| 205:117061 | 205:117069 · Biểu tượng | ON_CLICK | 205:117605 | Không lưu |
| 205:117122 | 205:117130 · Biểu tượng | ON_CLICK | 205:117719 | Không lưu |
| 205:117122 | 205:117262 · Xác nhận đã nhận | ON_CLICK | 205:116984 | Không lưu |
| 205:117265 | 205:117273 · Biểu tượng | ON_CLICK | 205:117992 | Không lưu |
| 205:117344 | 205:117352 · Biểu tượng | ON_CLICK | 205:117393 | Không lưu |
| 205:117344 | 205:117390 · Gửi đề xuất mới | ON_CLICK | 205:117393 | Không lưu |
| 205:117393 | 205:117401 · Biểu tượng | ON_CLICK | 205:117719 | Không lưu |
| 205:117393 | 205:117442 · Đề xuất lại | ON_CLICK | 205:117344 | Không lưu |
| 205:117393 | 205:117444 · Chấp nhận đề nghị | ON_CLICK | 205:117719 | Không lưu |
| 205:117447 | 205:117459 · Đã gửi | ON_CLICK | 205:117719 | Không lưu |
| 205:117447 | 205:117461 · Đã nhận | ON_CLICK | 205:117605 | Không lưu |
| 205:117447 | 205:117463 · Thương lượng | ON_CLICK | 205:117522 | Không lưu |
| 205:117447 | 240:279 · Xem chi tiết | ON_CLICK | 205:114915 | Không lưu |
| 205:117522 | 205:117534 · Đã gửi | ON_CLICK | 205:117719 | Không lưu |
| 205:117522 | 205:117536 · Đã nhận | ON_CLICK | 205:117605 | Không lưu |
| 205:117522 | 205:117540 · Hoàn thành | ON_CLICK | 205:117447 | Không lưu |
| 205:117522 | 205:117581 · Mở chat trao đổi | ON_CLICK | 205:114968 | Không lưu |
| 205:117605 | 205:117618 · Đã gửi | ON_CLICK | 205:117719 | Không lưu |
| 205:117605 | 205:117622 · Thương lượng | ON_CLICK | 205:117522 | Không lưu |
| 205:117605 | 205:117632 · Thẻ Smart Swap 3 bên | ON_CLICK | 236:18 | Không lưu |
| 205:117605 | 205:117670 · Thẻ thông tin | ON_CLICK | 205:117061 | Không lưu |
| 205:117719 | 205:117727 · Biểu tượng | ON_CLICK | 2:3179 | Không lưu |
| 205:117719 | 205:117734 · Đã nhận | ON_CLICK | 205:117605 | Không lưu |
| 205:117719 | 205:117736 · Thương lượng | ON_CLICK | 205:117522 | Không lưu |
| 205:117719 | 205:117824 · Thẻ thông tin | ON_CLICK | 205:117122 | Không lưu |
| 205:117877 | 205:117885 · Biểu tượng | ON_CLICK | 205:117942 | Không lưu |
| 205:117877 | 205:117939 · Gửi đề nghị | ON_CLICK | 205:117393 | Không lưu |
| 205:117942 | 205:117950 · Biểu tượng | ON_CLICK | 205:117992 | Không lưu |
| 205:117942 | 205:117989 · Tiếp tục · 1 món đã chọn | ON_CLICK | 205:117877 | Không lưu |
| 205:117992 | 205:118000 · Biểu tượng | ON_CLICK | 205:118152 | Không lưu |
| 205:117992 | 205:118034 · Nhắn tin | ON_CLICK | 205:117265 | Không lưu |
| 205:117992 | 205:118036 · Đề nghị trao đổi | ON_CLICK | 205:117942 | Không lưu |
| 205:118039 | 205:118147 · Xóa lọc  /  Áp dụng · 24 món | ON_CLICK | 205:118152 | Không lưu |
| 205:118152 | 205:118172 · Bộ lọc | ON_CLICK | 205:118039 | Không lưu |
| 205:118152 | 205:118179 · Trao đổi của tôi | ON_CLICK | 226:102 | Không lưu |
| 205:118152 | 205:118188 · Tìm vòng trao đổi | ON_CLICK | 205:115234 | Không lưu |
| 205:118152 | 205:118192 · Thẻ thông tin | ON_CLICK | 205:117992 | Không lưu |
| 205:118152 | 205:118210 · Trang chủ | ON_CLICK | 4:461 | Không lưu |
| 205:118152 | 205:118218 · Đăng bán | ON_CLICK | 114:332 | Không lưu |
| 205:118152 | 205:118222 · Tin nhắn | ON_CLICK | 90:16 | Không lưu |
| 205:118152 | 205:118226 · Cá nhân | ON_CLICK | 2:3179 | Không lưu |
| 226:102 | 226:113 · Khám phá | ON_CLICK | 205:118152 | Không lưu |
| 226:102 | 226:115 · Trao đổi của tôi | ON_CLICK | 4294967295:4294967295 **ĐÍCH KHÔNG TỒN TẠI** | Không lưu |
| 226:102 | 226:315 · Đăng tin trao đổi | ON_CLICK | 226:213 | Không lưu |
| 226:213 | 226:221 · back | ON_CLICK | 226:102 | Không lưu |
| 226:213 | 226:307 · Đăng tin trao đổi | ON_CLICK | 226:102 | Không lưu |
| 226:213 | 226:309 · Lưu bản nháp | ON_CLICK | 226:102 | Không lưu |
| 236:150 | 236:199 · Đóng khung điều chỉnh | ON_CLICK | 205:114968 | Không lưu |
| 236:150 | 236:219 · Xác nhận | ON_CLICK | 205:115021 | Không lưu |
