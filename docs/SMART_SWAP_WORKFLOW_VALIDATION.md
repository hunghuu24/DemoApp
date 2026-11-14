# Xác minh nghiệp vụ Smart Swap — 12/11/2026

[Hợp đồng API và quy tắc nghiệp vụ](SMART_SWAP_WORKFLOW.md). Kiểm tra trên project hiện có, .NET8/EF Core8.0.33, ASP.NET TestServer với middleware/controller/DI/JWT thật và SQL Server thật. Không dùng repository mock, EF InMemory hoặc vòng đăng sẵn để thay thuật toán. Android giữ nguyên.

## Kết quả lượt cuối

| Nhóm | Kết quả / log |
| --- | --- |
| Lời mời/phiên bản/chat/tồn kho Smart Swap | **170 kiểm tra đạt**: [log](qa/smart-swap-workflow/backend-checks.txt) |
| Hồi quy thuật toán Smart Swap | **289 đạt**, gồm100 đồ thị ngẫu nhiên/oracle và đồ thị60 đỉnh: [log](qa/smart-swap-workflow/search-regression.txt) |
| Hồi quy trao đổi1–1 | **125 đạt**: [log](qa/smart-swap-workflow/exchange-regression.txt) |
| Hồi quy catalog | **94 đạt**: [log](qa/smart-swap-workflow/catalog-regression.txt) |
| Hồi quy mua bán | **119 đạt**: [log](qa/smart-swap-workflow/commerce-regression.txt) |
| Hồi quy tài khoản | **54 đạt**: [log](qa/smart-swap-workflow/auth-regression.txt) |
| SQL nền tảng/fresh database | **20 đạt**: [log](qa/smart-swap-workflow/fresh-database.txt) |
| Release build | **0 lỗi,0 cảnh báo**: [log](qa/smart-swap-workflow/release-build.txt) |
| Model khớp migration | Không có thay đổi model chưa tạo migration: [log](qa/smart-swap-workflow/model-check.txt) |

Tổng **871 kiểm tra/assertion Backend**; không phải871 bài UI hoặc một benchmark tải production. Lượt trước với701 kiểm tra là báo cáo giai đoạn tìm kiếm; tài liệu này thêm và xác minh nghiệp vụ trên cùng project.

## Luồng ba tài khoản thật

Fixture tạo SQL database ngẫu nhiên riêng, đăng ký và đăng nhập USER A/B/C bằng API JWT thật. Thêm USER ngoài vòng và ADMIN để kiểm tra quyền/duyệt. Ba tin có ảnh upload thật và được ADMIN duyệt; không sửa tài khoản hay vòng demo cũ.

| Nhóm | Tình huống đã đạt |
| --- | --- |
| Chọn/preview | Tìm vòng bằng thuật toán hiện có; preview có3 hồ sơ,3 cạnh và phương án bù4 triệu; không tạo GiaoDich; mã sai400, hash giả/stale409, người không có món không khởi tạo403 |
| Gửi lời mời | Creator xác nhận rõ trước khi gửi; hai request retry đồng thời tạo đúng1 vòng/1 nhóm/1 phiên bản; payload đổi cùng key409; vòng trùng đang chờ409 |
| Thông báo | Hai người được mời có thông báo lưu SQL trỏ đúng vòng; đọc đúng chủ sở hữu204, người khác404; retry đọc idempotent |
| Đồng thuận | Bản1 chỉ creator đồng ý, hai người chờ;2/3 vẫn root ChoDongY và0 hold; đồng ý thiếu cờ đã đọc400; đủ3/3 mới DaChot/SanSangBanGiao/DangThucHien và3 hold |
| SQL guards | SQL trực tiếp không thể chốt phiên bản hoặc chuyển vòng ready trước3/3; các mutation sai được rollback |
| Quyền riêng tư | Người ngoài không đọc/vote/propose vòng, chat, lịch sử hoặc ảnh riêng; ảnh của món không thuộc vòng404; ADMIN không tự được thêm vào participant |
| Chat nhóm | Một nhóm có3 membership; cả3 gửi tin thật qua endpoint chung; mỗi phiên bản có một tin HeThong gắn đúng ID phiên bản; retry không nhân tin cấu trúc |
| Tiền bù | Không sửa cặp giữa hai người khác403; tổng trả vượt cap400; bản mới3,5 triệu có tổng trả=nhận; cả người trả và nhận đều có thể đề xuất khoản liên quan tới mình |
| Phiên bản | Counter sau chốt giữ3 món, chuyển root/vòng về chờ,0/3 consent mới kể cả proposer; bản cũ giữ số tiền và3 consent trong lịch sử; stale vote/base409; cùng proposal key không nhân version, đổi payload409 |
| Chốt lại | Hai consent mới vẫn không bàn giao; consent mới thứ ba kích hoạt lại, không thêm hold trùng; cả3 thành viên lần lượt đề xuất được bản mới |
| Từ chối | Một người từ chối bản mới hủy toàn vòng và giải phóng3 hold; vòng bị từ chối không tiếp tục; tin công khai lại |
| Hết hạn | Mutation vào bản đã hết hạn409 đồng thời lưu trạng thái HetHan; worker hủy counter chờ và nhả3 món |
| Bảo toàn trạng thái | Tranh chấp không bị GET/worker/hủy ghi đè; legacy thiếu creator mới chỉ đọc và worker bỏ qua |
| Sản phẩm bị rút | Món bị ẩn sau lời mời làm xác nhận cuối409 và0 hold; hủy lời mời không tự hiện lại món đã bị chủ tin ẩn |
| Sau bàn giao | Fixture có một receipt hợp lệ theo bản đã chốt: counter và hủy đều409; không hồi tố phương án đã bàn giao |

Receipt trong tình huống bảo vệ sau bàn giao được fixture SQL tạo để kiểm tra guard, không phải API giao nhận mới. Scope hiện tại dừng ở lời mời/đồng thuận/thương lượng và chuyển chính thức.

## Cạnh tranh và nguyên tử

Ba cuộc chạy đồng thời dùng HTTP thật vào cùng TestServer/SQL Server:

1. Hai vòng khác nhau dùng chung một món, mỗi vòng đã có2/3: hai xác nhận cuối song song chỉ một200, một409. Vòng thắng có đúng3 hold; vòng thua0 hold. Sau hủy vòng thắng, vòng thua thử lại được và giữ đủ3 món, chứng minh không lưu dở consent/hold của lần thất bại.
2. Xác nhận cuối Smart Swap cùng lúc tạo đơn mua bán món chung: chỉ Smart Swap chính thức hoặc đơn được giữ món. Bên thua409; cleanup theo đúng bên đã thành công.
3. Xác nhận cuối Smart Swap cùng lúc chấp nhận trao đổi1–1 món chung: chỉ một giao dịch được giữ món; bên thua409.

Truy vấn cuối cùng xác minh không sản phẩm nào có nhiều hơn một hold chưa giải phóng. Các service dùng chung khóa sản phẩm theo thứ tự rồi khóa tin; unique filtered hold là lớp bảo vệ SQL. Những cuộc chạy này xác minh hành vi cạnh tranh của các tình huống trên, không phải cam kết mọi workload hoặc latency production.

## Migration và dữ liệu

Migration9 **20261112070453_LoiMoiVaThuongLuongSmartSwap** đã áp dụng trên ReMarketDemo: [log](qa/smart-swap-workflow/migration.txt). Số liệu SQL thực tế: **47 bảng,95 FK,67 CHECK,17 trigger,9 migrations**, không tính bảng migration history: [query](qa/smart-swap-workflow/schema-counts.txt).

[SQL idempotent](database/remarket-schema.sql) xuất lại từ9 migrations, áp **hai lần** trên DB trống độc lập, seed/chạy20 kiểm tra thành công. [Từ điển EF47 entity](database/schema-dictionary.json) và [thiết kế CSDL](database/THIET_KE_CSDL.md) đã cập nhật. Các cột creator/key/tin nguồn/link tin nhắn nullable cho lịch sử cũ; không seed vòng mới hoặc tự nâng dữ liệu cũ thành lời mời mới.

Mỗi nhóm HTTP tạo và dọn đúng DB/ảnh tạm của chính lượt chạy; mutation SQL nền tảng rollback. Không reset ReMarketDemo, sửa tài khoản người dùng, tạo credential bypass hoặc ghi JWT/mật khẩu vào log. Không thay đổi code/APK/AVD Android và không triển khai server công khai.

## Giới hạn còn lại

Thông báo được lưu SQL và truy xuất qua API; chưa có push thiết bị hoặc WebSocket. Phương án tiền bù là thỏa thuận, chưa chuyển tiền qua ngân hàng/ví. Giao nhận6 xác nhận, hoàn tất/đánh giá vòng3 bên và UI Android là giai đoạn tiếp theo; API hiện tại chỉ cho phép bắt đầu bước đó khi đúng bản có đủ3 consent.
