# Ma trận REST API

121 routes; 121 routes đã nhận request kiểm thử; 121 có phản hồi2xx. Đây là độ phủ route, không phải độ phủ mọi nhánh/đầu vào.

| Method | Route | HTTP statuses đã quan sát |
| --- | --- | --- |
|POST|/api/donhang/baogia|200, 400, 401, 403, 409|
|POST|/api/donhang|201, 400, 409|
|GET|/api/donhang|200|
|GET|/api/donhang/{id}|200, 404|
|POST|/api/donhang/{id}/hoithoai|200|
|POST|/api/donhang/{id}/thanhtoan|200, 403, 409|
|POST|/api/donhang/{id}/huy|200, 409|
|POST|/api/donhang/{id}/vanchuyen|200, 403, 409|
|POST|/api/donhang/{id}/nhanhang|200, 403, 409|
|POST|/api/donhang/{id}/danhgia|200, 403, 409|
|GET|/api/donhang/{id}/anh|200, 404|
|GET|/api/admin/tindang|200|
|POST|/api/admin/tindang/{id}/duyet|200, 403|
|GET|/api/hoithoai|200|
|POST|/api/hoithoai|200|
|GET|/api/hoithoai/{id}|200, 404|
|POST|/api/hoithoai/{id}/tinnhan|200, 400, 404, 409|
|POST|/api/hoithoai/{id}/phieu|200, 400, 403|
|POST|/api/hoithoai/{id}/phieu/{phieuId}|200, 403|
|POST|/api/hoithoai/{id}/anh|200|
|GET|/api/hoithoai/{id}/anh/{tepId}|200, 401, 404|
|GET|/api/hoithoai/{id}/sanpham/anh|200|
|GET|/api/hopthu|200|
|POST|/api/hotro|201, 404|
|GET|/api/hotro|200|
|GET|/api/hotro/{id}|200, 404|
|POST|/api/hotro/{id}/phanhoi|200, 403, 409|
|GET|/api/admin/bao-cao|200, 400, 401, 403|
|GET|/api/admin/nguoi-dung|200, 401, 403|
|GET|/api/admin/nguoi-dung/{id}|200, 401, 403|
|POST|/api/admin/nguoi-dung/{id}/khoa|200, 400, 401, 403|
|POST|/api/admin/nguoi-dung/{id}/mo-khoa|200, 400, 401, 403|
|GET|/api/admin/danh-muc|200, 401, 403|
|POST|/api/admin/danh-muc|201, 401, 403|
|PUT|/api/admin/danh-muc/{id}|200, 400, 401, 403, 409|
|GET|/api/admin/san-pham|200, 401, 403|
|GET|/api/admin/san-pham/{id}|200, 401, 403|
|POST|/api/admin/san-pham/{id}/duyet|200, 401, 403|
|POST|/api/admin/san-pham/{id}/an|200, 401, 403|
|GET|/api/admin/giao-dich|200, 401, 403|
|GET|/api/admin/giao-dich/{id}|200, 401, 403|
|GET|/api/admin/giao-dich/{id}/media/{kind}/{file}|200, 401, 403|
|GET|/api/admin/khieu-nai|200, 401, 403|
|POST|/api/admin/giao-dich/{id}/khieu-nai|201, 400, 401, 403|
|GET|/api/admin/khieu-nai/{id}|200, 401, 403|
|POST|/api/admin/khieu-nai/{id}/tiep-nhan|200, 401, 403|
|POST|/api/admin/khieu-nai/{id}/quyet-dinh|200, 401, 403, 409|
|POST|/api/admin/khieu-nai/{id}/xac-minh|200, 400, 401, 403|
|GET|/api/admin/hotro|200|
|GET|/api/admin/hotro/{id}|200|
|POST|/api/admin/hotro/{id}/phanhoi|200|
|GET|/health/live|200|
|GET|/health/ready|200|
|GET|/api/hethong/trangthai|200|
|GET|/api/smart-swaps/search|200, 400, 401, 404, 422|
|POST|/api/taikhoan/dangky|201, 400, 409|
|POST|/api/taikhoan/dangnhap|200, 401|
|POST|/api/taikhoan/lam-moi|200, 401|
|POST|/api/taikhoan/dangxuat|204|
|POST|/api/taikhoan/quenmatkhau|202|
|POST|/api/taikhoan/datlaimatkhau|204, 400|
|POST|/api/taikhoan/doimatkhau|204, 400|
|GET|/api/taikhoan/toi|200, 401|
|PUT|/api/taikhoan/toi|200, 409|
|POST|/api/taikhoan/anh-dai-dien|200, 400|
|GET|/api/taikhoan/anh-dai-dien|200|
|GET|/api/tindang|200, 400|
|POST|/api/tindang|201, 400, 401|
|GET|/api/tindang/danhmuc|200|
|GET|/api/tindang/trangchu|200|
|GET|/api/tindang/{id}|200, 404|
|PUT|/api/tindang/{id}|200, 404, 409|
|DELETE|/api/tindang/{id}|204, 404|
|GET|/api/tindang/cuatoi|200|
|GET|/api/tindang/{id}/quanly|200, 404|
|POST|/api/tindang/{id}/gui-duyet|200, 400|
|POST|/api/tindang/{id}/an|200|
|POST|/api/tindang/{id}/da-ban|200, 409|
|POST|/api/tindang/{id}/anh|200, 400, 404, 409|
|DELETE|/api/tindang/{id}/anh/{anhId}|200|
|GET|/api/tindang/{id}/anh/{anhId}|200, 401, 404|
|PUT|/api/tindang/{id}/anh/{anhId}/bia|200|
|GET|/api/trangchu|200|
|GET|/api/admin/tongquan|200, 403|
|GET|/api/traodoi/moncuatoi|200|
|GET|/api/traodoi/tieuchi|200|
|GET|/api/traodoi|200|
|POST|/api/traodoi|201, 400, 403, 409|
|GET|/api/traodoi/{id}|200, 404|
|POST|/api/traodoi/{id}/phanhoi|200, 403, 409|
|POST|/api/traodoi/{id}/dexuatlai|200, 409|
|POST|/api/traodoi/{id}/huy|200, 409|
|POST|/api/traodoi/{id}/giaonhan|200, 409|
|POST|/api/traodoi/{id}/danhgia|200, 409|
|POST|/api/traodoi/{id}/hoithoai|200|
|GET|/api/traodoi/{id}/anh/{sanPhamId}|200, 404|
|POST|/api/traodoi/{id}/bangchung|200, 400, 409|
|GET|/api/traodoi/{id}/bangchung/{fileId}|200, 404|
|POST|/api/smart-swaps/{id}/evidence|200, 409|
|GET|/api/smart-swaps/{id}/evidence/{file}|200, 404|
|GET|/api/smart-swaps/{id}/reviews|200, 401, 404|
|POST|/api/smart-swaps/{id}/reviews|200, 400, 404, 409|
|POST|/api/smart-swaps/preview|200, 400|
|POST|/api/smart-swaps/invitations|201, 400, 403, 409|
|GET|/api/smart-swaps|200, 401|
|GET|/api/smart-swaps/{id}|200, 404|
|POST|/api/smart-swaps/{id}/responses|200, 400, 404, 409|
|POST|/api/smart-swaps/{id}/proposals|200, 400, 403, 404, 409|
|GET|/api/smart-swaps/{id}/versions|200, 404|
|POST|/api/smart-swaps/{id}/cancel|200, 409|
|GET|/api/smart-swaps/{id}/chat|200, 404|
|GET|/api/smart-swaps/{id}/products/{productId}/image|200, 404|
|GET|/api/smart-swaps/notifications|200|
|POST|/api/smart-swaps/notifications/{id}/read|204, 404|
|GET|/api/smart-swaps/{id}/delivery|200, 401, 404|
|PUT|/api/smart-swaps/{id}/delivery/contact|200, 400, 409|
|POST|/api/smart-swaps/{id}/shipments|200, 409|
|PUT|/api/smart-swaps/{id}/shipments/{shipmentId}/tracking|200, 403, 409|
|POST|/api/smart-swaps/{id}/shipments/{shipmentId}/status|200, 400, 409|
|POST|/api/smart-swaps/{id}/shipments/{shipmentId}/received|200, 400, 403, 404, 409|
|POST|/api/smart-swaps/{id}/issues|200, 403, 409|
