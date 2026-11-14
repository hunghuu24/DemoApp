# ReMarket — Kế hoạch triển khai đề xuất

> **Cập nhật theo yêu cầu ngày 07/11/2026:** kiến trúc được chốt lại thành Android Kotlin/Jetpack Compose/MVVM/Retrofit, ASP.NET Core 8 Web API/EF Core và SQL Server. Phần Kotlin/XML, Ktor và PostgreSQL bên dưới là kế hoạch lịch sử, đã được thay thế. Xem [README.md](README.md), [kiến trúc hiện tại](docs/ARCHITECTURE.md) và [thiết kế CSDL](docs/database/THIET_KE_CSDL.md). Nguồn Android đã chuyển vào `Android/`; backend nằm trong `Backend/`. Giai đoạn khởi tạo đã có mã nền tảng, migration và seed; chưa triển khai toàn bộ chức năng Figma.

> Đã triển khai tiếp các luồng [tài khoản](docs/ACCOUNTS.md) và [sản phẩm/tin đăng](docs/CATALOG.md) trên kiến trúc hiện tại. Tiến độ và bằng chứng kiểm thử nằm trong các tài liệu này; các mốc trong kế hoạch lịch sử bên dưới không phải toàn bộ chức năng đã hoàn thành.

> **Cập nhật 12/11/2026:** đã kết nối các form Android Smart Swap từ tìm vòng đến hai đánh giá, sau các luồng mua bán/trao đổi 1–1 và Backend Smart Swap. Xem [triển khai](docs/SMART_SWAP_ANDROID.md) và [kiểm thử thực tế](docs/SMART_SWAP_ANDROID_VALIDATION.md). Phần kế hoạch dưới đây được giữ như tài liệu lịch sử; tiến độ và quy tắc giao nhận hiện hành nằm trong các tài liệu liên kết.

Ngày lập: **07/11/2026**, Asia/Saigon. Tài liệu này là phương án triển khai dựa trên [FIGMA_ANALYSIS.md](FIGMA_ANALYSIS.md), chưa phải mã nguồn, API đã có hoặc quyết định công nghệ đã được nhóm chốt.

> **Cập nhật Admin12/11/2026:** Backend và Android đã triển khai quản lý người dùng/khóa phiên, duyệt/từ chối/ẩn tin, cây danh mục/icon/thứ tự, giao dịch mua bán/1–1/3 bên, bằng chứng và quyết định/xác minh tranh chấp, báo cáo SQL. Migration11 giữ48 bảng. Admin không xác nhận hàng/tiền thay thành viên; xử lý tranh chấp không làm vòng2/3 hoàn tất. Xem [ADMIN.md](docs/ADMIN.md) và [kết quả kiểm tra](docs/ADMIN_VALIDATION.md). Các mục kế hoạch lịch sử sau đây không thay thế trạng thái triển khai hiện hành.

## 1. Mục tiêu và hiện trạng workspace

Mục tiêu: triển khai ứng dụng Android ReMarket cho User và Admin theo các màn đã đọc, có backend quản lý dữ liệu và tính nhất quán giữa mua bán, trao đổi 1–1, Smart Swap. Thiết kế có **83 frame/trạng thái**, không yêu cầu tạo 83 màn kỹ thuật độc lập.

Workspace hiện tại là dự án Android native, không phải dự án web:

| Thành phần đã đọc | Hiện trạng |
| --- | --- |
| Module | `:app`; root project `App_Ban_Do_Cu` |
| Namespace / applicationId | `com.ute.app_ban_do_cu` |
| UI | `MainActivity.kt` kế thừa `AppCompatActivity`, `activity_main.xml` chỉ có Hello World |
| Layout / theme | XML Views; đã có AppCompat, Material và ConstraintLayout trong version catalog |
| SDK | `minSdk=24`, `targetSdk=37`, `compileSdk release(37)` trong file hiện có |
| Build | Gradle Kotlin DSL; catalog ghi AGP 9.3.3; Java compatibility 11 |
| Insets / bàn phím | Activity có edge-to-edge và xử lý systemBars; manifest `adjustResize` |
| Backend / dữ liệu | Chưa thấy backend, API contract, database hoặc lớp dữ liệu nghiệp vụ |
| Git | Thư mục hiện tại chưa là Git repository; không có diff/history để đối chiếu |

Các phiên bản trên là **giá trị trong file**, chưa được xác nhận build thành công hoặc tương thích với môi trường. Giai đoạn này không chạy Gradle, không thay dependency, không tạo screen/API/database, không đổi tên package và không sửa nguồn Android.

## 2. Quyết định kiến trúc đề xuất

| Hạng mục | Đề xuất | Lý do / giới hạn |
| --- | --- | --- |
| Android | Tiếp tục Kotlin + XML Views; Single Activity, Fragment Navigation, ViewBinding | Phù hợp scaffold hiện tại; chưa có lý do thiết kế buộc phải đổi toolkit |
| UI state | ViewModel cung cấp UI state; Fragment hiển thị và gửi intent; state nghiệp vụ qua repository | Tách vòng đời UI và dữ liệu, giữ form/filter khi xoay màn hoặc đổi tab |
| Domain | Use case cho submit tin, accept/counter, consent/handover, moderation | Chỉ tách use case nơi có quy tắc phức tạp; tránh thêm tầng rỗng cho mọi thao tác đọc |
| Backend | Kotlin/Ktor dạng modular monolith; REST cho lệnh/truy vấn, WebSocket cho cập nhật chat/state | Dùng cùng ngôn ngữ với Android; giữ giao dịch liên quan trong một backend trước khi cần mở rộng |
| Database | PostgreSQL, migration có phiên bản | Mua/đổi cùng một món và đồng ý nhiều bên cần ràng buộc cùng transaction |
| Media | Object storage qua giao diện trừu tượng; lưu metadata/quyền truy cập trong DB | Ảnh sản phẩm và bằng chứng có vòng đời/quyền xem khác nhau; chưa chốt nhà cung cấp |
| Android network/cache | HTTP client + serialization; cache có chọn lọc; lưu nháp cục bộ | Chọn thư viện/phiên bản tương thích ở mốc nền tảng; chưa cài đặt trong giai đoạn phân tích |
| Admin | Khu vực role Admin trong cùng Android app, graph riêng | 18 frame đều là mobile 360×800; không có cơ sở yêu cầu dashboard web riêng |

Hướng UI/data layer, repository và Single Activity/Fragment phù hợp [khuyến nghị Android cho Views](https://developer.android.com/topic/architecture/views/recommendations-views). Ktor có cơ chế [WebSocket server](https://ktor.io/docs/server-websockets.html) và [authentication](https://ktor.io/docs/server-auth.html); việc chọn Ktor cho ReMarket là đề xuất của tài liệu, không phải thông tin từ Figma.

Không chốt phiên bản thư viện mới bằng suy đoán. Trước khi thêm dependency cần kiểm tra tài liệu chính thức, toolchain và build scaffold. Không yêu cầu chuyển Compose, Flutter hay React Native trong kế hoạch này.

## 3. Cấu trúc dự án đề xuất

Đây là cây dự kiến, **chưa tạo các thư mục/mã nguồn dưới đây**. Ban đầu giữ một module Android, chia package theo chức năng; chỉ tách Gradle module khi có nhu cầu build/quyền sở hữu rõ ràng.

```text
App_Ban_Do_Cu/
├── FIGMA_ANALYSIS.md
├── IMPLEMENTATION_PLAN.md
├── Nhom4.fig
├── app/
│   └── src/main/
│       ├── java/com/ute/app_ban_do_cu/
│       │   ├── MainActivity.kt
│       │   ├── core/
│       │   │   ├── designsystem/       # component UI, typography, shape, icon mapping
│       │   │   ├── navigation/         # graph, route arguments, quyền và entry point
│       │   │   ├── network/            # HTTP, auth/session, WebSocket transport
│       │   │   ├── storage/            # draft/cache/session storage abstraction
│       │   │   └── common/             # format VND/ngày, result/error dùng chung
│       │   ├── domain/
│       │   │   ├── model/              # model nghiệp vụ, trạng thái, money, participant
│       │   │   ├── repository/         # interface theo dữ liệu/nghiệp vụ
│       │   │   └── usecase/            # các lệnh cần quy tắc liên bước
│       │   ├── data/
│       │   │   ├── remote/             # DTO và API adapter
│       │   │   ├── local/              # draft/cache entity
│       │   │   ├── mapper/
│       │   │   └── repository/         # implementation, hợp nhất local/remote
│       │   └── feature/
│       │       ├── auth/              # splash/login/register/reset/change password
│       │       ├── account/           # profile/settings
│       │       ├── catalog/           # home/search/category/product detail
│       │       ├── listing/           # create/preview/manage/edit/sold
│       │       ├── order/             # checkout/list/detail
│       │       ├── exchange/          # explore/my listings/offers/counter/handover
│       │       ├── smartswap/         # find/matches/cycle/consent/handover
│       │       ├── chat/              # inbox/1–1/group/negotiation
│       │       ├── support/           # history/form/tracking
│       │       ├── review/            # rating/success
│       │       └── admin/             # overview/users/moderation/transactions/
│       │                              # complaints/categories/reports
│       └── res/
│           ├── layout/                # fragment_*, item_*, view_*, dialog_*, sheet_*
│           ├── navigation/            # auth/user/admin và các graph con
│           ├── drawable/              # asset đã xuất/đối chiếu, shape selector
│           ├── font/                  # Roboto nếu đóng gói font được xác minh
│           └── values/                # colors, dimens, styles, strings
├── backend/                           # dự án server độc lập, đề xuất
│   └── src/main/kotlin/remarket/
│       ├── bootstrap/
│       ├── shared/                    # auth, DB transaction, money, errors, event
│       └── modules/
│           ├── accounts/
│           ├── categories/
│           ├── listings/
│           ├── orders/
│           ├── exchanges/
│           ├── smartswap/
│           ├── conversations/
│           ├── support/
│           ├── disputes/
│           ├── reviews/
│           ├── moderation/
│           ├── reports/
│           └── media/
│               # mỗi module: routes → service/domain → repository
├── contracts/                         # API/schema/event contract, đề xuất
└── docs/design/                       # asset manifest, frame exports, decisions
```

Không tạo một backend cho mỗi loại giao dịch. Listings, reservation và agreement dùng chung dữ liệu nhất quán; hành vi khác nhau nằm trong service/domain tương ứng. Mã xác thực, model UI và DTO API không được dùng thay cho nhau chỉ vì có trường trùng tên.

## 4. Điều hướng ứng dụng đề xuất và ánh xạ frame

Đây là nhóm route triển khai, **không phải các dây nối nguyên bản**. ID trong bảng tham chiếu danh mục đủ 83 frame ở tài liệu phân tích.

| Nhóm route / màn kỹ thuật | Frame nguồn | Cách tái sử dụng |
| --- | --- | --- |
| Auth graph | `2:2955`, `2:2983`, `2:3019`, `2:3064`, `92:558` | Tách reset password và change password dù tên frame trùng |
| Account graph | `2:3179`, `2:3096`, `2:3139` | Settings, Profile, Edit; chưa tạo OTP/privacy/delete screens chưa có thiết kế |
| Home/Search/Categories/List | `4:461`, `4:556`, `4:617`, `4:684` | Truyền query/category/filter qua state, không hardcode danh sách mẫu |
| ProductDetail | `4:755`, `128:6`, `128:67` | Một layout theo productId; tách onContact và onBuy |
| Listing form/preview/success | `114:2`, `114:115`, `114:292`, `114:252` | Form create/edit dùng mode, draftId/listingId; preview dùng dữ liệu nháp |
| Listing management | `114:332`, `114:192`, `114:66` | Tabs/trạng thái dữ liệu; confirm sold là action có xác nhận |
| Checkout | `44:191`, `194:72` | Một màn theo listingId và giá/đề nghị đã được server chấp nhận |
| Order list/detail | `44:223`, `44:248`, `77:65` | Một Detail theo orderId; CTA do trạng thái/quyền quyết định |
| Inbox/Chat/price offer | `90:16`, `128:139`, `188:2`, `85:2`, `128:174` | conversationId và participantRole; card trả giá là typed message, không nhân ba màn chat |
| Support | `90:78`, `128:216`, `44:279`, `44:303` | Form có orderId tùy chọn; ticketId cho detail/tracking |
| Purchase review | `146:248`, `146:351` | Form và kết quả, xác minh đơn đã đủ điều kiện đánh giá |
| Exchange explore/listings/form | `205:118152`, `205:118039`, `226:102`, `226:213` | Sheet filter là overlay UI; My listings khác offers history |
| Exchange detail/pick/create | `205:117992`, `205:117942`, `205:117877` | listingId, targetListingId, draftOffer; giữ form qua bước chọn đồ |
| Offer history | `205:117719`, `205:117605`, `205:117522`, `205:117447` | Một container tabs Sent/Received/Negotiating/Completed, hỗ trợ loại 1–1/3 bên |
| Offer detail/counter/chat | `205:117393`, `205:117344`, `205:117265` | offerId, agreementVersion, participantRole; chat 1–1 đúng transaction |
| 1–1 handover/review | `205:117122`, `205:117061`, `205:116984`, `205:116916` | Một handover có trạng thái waiting; review và receipt riêng |
| Smart Swap find/matches | `205:115234`, `205:115155` | matching requestId, điều kiện và kết quả phía server |
| Smart Swap detail/consent | `205:115087`, `236:18`, `205:115021` | Một cycle detail theo vai trò/phase; consent theo version |
| Smart Swap chat/sheet | `205:114968`, `236:150` | Tái sử dụng Chat, thêm compensation sheet; không làm chat giả bằng ảnh |
| Smart Swap handover/review | `205:114915`, `205:114854` | 6 xác nhận và 2 đối tượng đánh giá; có khóa trạng thái phía server |
| Admin overview/menu | `205:116809`, `205:116652` | Cùng màn, DrawerLayout/state drawer thay hai destination |
| Admin users | `205:116578`, `205:116512`, `205:116413`, `205:116345` | List/detail, lock dialog, unlock confirm; dùng cùng userId |
| Admin moderation | `205:116276`, `205:116220`, `205:116138`, `205:116077` | List/detail theo status; reject dialog/hide confirmation; giữ listingId |
| Admin transactions | `205:115976`, `205:115767` | Loại giao dịch quyết định section; không click toàn vùng để mở cùng mã |
| Admin disputes | `205:115705`, `205:115645`, `205:115596` | complaintId, evidence, decision; quyết định khác trạng thái đã giải quyết |
| Admin categories/reports | `205:115492`, `205:115419`, `205:115300` | Add form có thể dùng edit mode sau khi có đặc tả; reports theo range |

Main navigation giữ năm mục **Trang chủ / Trao đổi / Đăng bán / Tin nhắn / Cá nhân**. Tab Đăng bán hiện dẫn Quản lý tin, không tự chuyển thẳng form. Explore có lối vào quản lý tin trao đổi; mục tài khoản có lối vào lịch sử đề nghị trao đổi. Hai điểm này cần nhãn phân biệt khi sửa Prototype.

Với đề xuất này, Back đóng sheet/dialog trước; sau đó về màn trước trong flow. Giữ query, selected tab và draft khi đổi tab hoặc quay lại. Đó là hành vi đề xuất cần nghiệm thu, không phải hành vi đã được Prototype chứng minh. Auth/role được xác minh ở server; việc ẩn menu Admin không thay thế phân quyền.

## 5. Design system và tài sản giao diện

1. Chốt bảng ánh xạ frame → route → trạng thái, sửa các P0 trong phân tích trước khi nối navigation.
2. Xuất frame PNG đầy đủ và asset theo node ID; đối chiếu crop ảnh, vector, dialog, icon, text trùng/clipping. Lập manifest tên file, node nguồn, kích thước, loại, quyền sử dụng. Chưa có bộ ảnh này ở giai đoạn hiện tại.
3. Tạo token theo giá trị đã đọc: primary `#28734F`, background `#FCFDFB`, text `#202E26/#65746A`, surface trắng, outline `#DCE4DD`, warning/danger. Giữ các màu legacy trong mapping riêng cho đến khi nhóm thống nhất có chuẩn hóa hay giữ nguyên.
4. Định nghĩa Roboto theo vai trò/weight/line height; kiểm tra hiển thị tiếng Việt, dấu, chữ xuống dòng và font scaling. Loại khoảng trắng Inter khỏi layout sau khi đã kiểm tra ý nghĩa khoảng cách.
5. Tạo primitives trước: app bar, button, field, chip, badge, tabs, card, avatar, banner, dialog/sheet. Sau đó tạo cụm Order/Offer/Swap/Handover/Chat/Admin trong danh sách component của tài liệu phân tích.
6. Bổ sung đặc tả loading/error/empty/disabled/focus/validation cho các component cần thiết. Đây là phần thiết kế bổ sung, không tự nhận đã có variant trong Figma.

Viewport đối chiếu chính: **360×800**. Dùng bố cục cuộn cho các frame cao hơn, tránh vẽ thanh trạng thái giả. Tách vùng nhận chạm đủ dùng khỏi kích thước icon; phải kiểm thử thao tác khi bàn phím mở, system bars thay đổi và nội dung tiếng Việt dài hơn mẫu.

## 6. Mô hình dữ liệu miền đề xuất

Các entity sau là thiết kế kỹ thuật sơ bộ, cần chốt schema ở giai đoạn API. Không có database schema trong Figma.

| Nhóm | Entity / quan hệ đề xuất | Quy tắc cần giữ |
| --- | --- | --- |
| Account | User, Profile, Session, ContactVerification, AccountRestriction | Role và tình trạng khóa là dữ liệu server; hồ sơ và phiên xác thực tách nhau |
| Catalog | Category tự tham chiếu parentId, Listing, ListingMedia, ExchangePreference | Chủ tin, visibility/moderation/trading mode tách riêng; không dùng một status cho mọi ý nghĩa |
| Tin / nháp | ListingDraft hoặc draft state; Published listing revision | 8 ảnh tin bán, 6 ảnh trao đổi theo thiết kế; quy tắc cho tin hỗn hợp cần chốt |
| Reservation | ListingReservation liên kết listing và giao dịch đang giữ | Không cùng lúc chốt mua/đổi món đã bị giữ trong giao dịch khác |
| Purchase | Order, OrderItem, ShippingAddressSnapshot, PriceAgreement, PaymentRecord, OrderEvent | Giá do server tính; lưu snapshot tên/giá/địa chỉ lúc đặt; không lấy tổng do client gửi làm chuẩn |
| 1–1 | ExchangeOffer, OfferRevision, Agreement, Meeting, ReceiptConfirmation | Người gửi/nhận, hai món, từng phiên bản khoản bù; acceptance gắn version |
| 3 bên | SwapCycle, SwapParticipant, SwapLeg, CompensationTransfer, Consent, HandoverConfirmation | Ba người khác nhau, ba món khác nhau; cạnh giao đủ chu trình; 3 consent và 6 handover flags |
| Chat | Conversation, ConversationMember, Message, MessageReceipt, Attachment | Conversation loại purchase/exchange/swap; membership quyết định quyền xem; message trả giá có payload riêng |
| Hỗ trợ | SupportTicket, SupportMessage/Event, Evidence | Mã HT; orderId có thể rỗng theo form; khác khiếu nại tranh chấp KN |
| Tranh chấp | Dispute, DisputeParty, DisputeEvidence, Decision, ComplianceEvent | Liên quan order/exchange/swap; lưu đối tượng/món/khoản bù và căn cứ quyết định |
| Đánh giá | Review, ReviewTag, ReviewMedia | Reviewer phải là participant đủ điều kiện; 1–1 đánh giá đối tác, swap có hai đối tác |
| Admin | ModerationAction, RestrictionAction, AuditEvent | Actor, đối tượng, reason, thời hạn, before/after và thời điểm; giữ lịch sử giao dịch khi khóa/ẩn |
| Thống kê | Truy vấn tổng hợp/read model | KPI và series lấy từ dữ liệu thật, không hardcode 1.284/248/96 trong mockup |

Tiền VND lưu bằng số nguyên, kèm currency; không dùng floating point. Thời điểm lưu thống nhất UTC, hiển thị theo múi giờ được chọn; các ngày tháng trong Figma là fixture. Media có owner, entity liên quan, MIME, kích thước và quyền truy cập; không dùng URL công khai cho mọi bằng chứng mặc định.

## 7. Chuyển trạng thái và tính nhất quán

### 7.1 Tin đăng và mua bán

Tách vòng đời tin: draft → pending moderation → published hoặc rejected; các thao tác sold/hidden lưu lịch sử. Không mặc định sửa một tin đã duyệt có cần duyệt lại hay không vì Figma chưa nêu rõ. Khi người bán đánh dấu đã bán, chỉ đổi trạng thái sau khi server xác nhận và kiểm tra giao dịch liên quan.

Checkout có hai đường giá mẫu: giá gốc và giá đã thương lượng. Server phải xác minh listing còn khả dụng, giá thỏa thuận còn hiệu lực, phí giao hàng và quyền dùng mức giá trước khi tạo đơn. Chưa có quy tắc thời hạn phiếu giảm hoặc thuật toán phí ship ngoài số mẫu 30.000đ.

**Điểm cần chốt trước làm thanh toán:** nhà cung cấp, phương thức, thời điểm giữ/giải ngân, xác nhận giao hàng, hủy/hoàn và đối soát. Thiết kế nói giữ tiền nhưng chưa mô tả cơ chế. Có thể chuẩn bị contract và môi trường thử nghiệm, nhưng chưa coi một nút chuyển màn là thanh toán thành công.

### 7.2 Trao đổi 1–1

Chuỗi domain đề xuất: pending response → counter pending nếu có sửa → accepted/meeting → awaiting receipts → completed → reviewed. Rejected/cancelled/expired/disputed là nhánh kỹ thuật cần có quy tắc và UI bổ sung; một số nhánh mới chỉ được nhắc trong copy.

- Chỉ gửi món thuộc sở hữu người tạo, đang hiển thị và đủ điều kiện giao dịch.
- Mỗi counter tạo revision mới; không ghi đè thỏa thuận đã được bên kia chấp nhận.
- Sau xác nhận cá nhân, nếu đối tác chưa nhận đủ thì hiển thị màn chờ `205:117061`; không nhảy thẳng review.
- Server tổng hợp xác nhận cả hai trước completed; việc trả/nhận tiền bù trực tiếp được lưu lời xác nhận và bằng chứng, không kích hoạt payment API.
- Hạn 24h có trong mockup, nhưng cách gia hạn khi counter và thời điểm tự hết hạn cần chốt. Không hardcode một chính sách chưa được nêu.

### 7.3 Smart Swap

Matching đề xuất tìm chu trình đúng **3 bên** trên quan hệ món đang có/món muốn nhận, lọc khu vực, tình trạng, xác minh và giới hạn bù. Figma không mô tả thuật toán hay xếp hạng: kết quả mẫu SS-0301/0302 không đủ chứng minh công thức matching.

Các điều kiện domain cần kiểm tra độc lập với UI:

1. Ba user và ba listing khác nhau; mỗi người giao một món và nhận một món, đủ chu trình A→C→B→A theo mẫu. Không dùng arrow hiển thị thay dữ liệu quan hệ.
2. Khoản bù là tập chuyển tiền `(người trả, người nhận, số tiền)`; A→B 500k và C→B 500k không được biến thành tổng bù của A là 1 triệu. Không tự tính lại mức bù chỉ bằng giá tham khảo.
3. Consent tham chiếu agreement version. Đề xuất mọi sửa món/khoản bù sau đồng ý phải tạo version mới và yêu cầu đồng ý lại; cần nhóm xác nhận chính sách này vì Figma chỉ có sheet sửa tiền bù, chưa có trạng thái reset consent.
4. Bàn giao bị khóa khi dưới 3/3 consent. Một thành viên từ chối thì dừng vòng theo copy; quy tắc thu hồi giữ chỗ/hủy/hết hạn cần đặc tả thêm.
5. Lưu sáu xác nhận giao/nhận riêng, có actor và thời điểm; không dùng một boolean completed cho cả vòng. Tổng 3 lượt nhận trên Admin là một phép tổng hợp khác 6 lượt giao/nhận.
6. Xác nhận tiền bù trực tiếp tách khỏi sáu xác nhận món đồ. Đến completed chỉ khi các ràng buộc thỏa thuận đã chốt được thỏa mãn; nguồn chưa nói rõ tiền bù có là gate riêng ngoài 6/6 hay chỉ là lời xác nhận đi kèm.
7. Tạo quyền gửi hai review cho mỗi participant sau khi giao dịch hoàn tất; chống gửi lặp theo người đánh giá/người được đánh giá/giao dịch.

### 7.4 Đồng thời, retry và realtime

Các lệnh tạo/chấp nhận giao dịch, đồng ý thỏa thuận, xác nhận giao nhận và moderation cần idempotency và kiểm tra version. Reservation của các listing trong cùng giao dịch phải thay đổi nguyên tử để hai người không chốt cùng món. Có thể dùng transaction và row lock theo thứ tự ID cố định; PostgreSQL mô tả cơ chế này trong [tài liệu khóa hàng](https://www.postgresql.org/docs/17/explicit-locking.html). Đây là phương án kỹ thuật, không phải hành vi thấy trong Prototype.

WebSocket phát sự kiện sau commit; reconnect tải lại snapshot và các event còn thiếu theo cursor. Retry gửi chat dùng clientMessageId để không tạo tin trùng. API và realtime cùng đọc một nguồn trạng thái server; không tăng 2/3→3/3 hoặc 5/6→6/6 chỉ vì client vừa bấm nút.

## 8. Hợp đồng API và sự kiện đề xuất

Đường dẫn sau là bản phác thảo contract, chưa tồn tại trong workspace. Mỗi lệnh cần response state/version và lỗi có nghĩa; phân trang/filter của danh sách phải nhất quán.

| Nhóm | API dự kiến / sự kiện | Đối chiếu UI |
| --- | --- | --- |
| Auth/Profile | `/auth/register`, `/auth/login`, `/auth/recovery`, `/auth/password`, `/me` | Auth/account, không tự thêm OTP screen |
| Catalog/Listings | `/categories`, `/listings`, `/listings/{id}`, `/me/listings`, draft/submit/hide/mark-sold actions | Home/search/detail và hai loại form đăng tin |
| Media | Upload session/complete, attach media theo entity | Ảnh tin, chat và bằng chứng; giới hạn theo use case |
| Price negotiation/Orders | `/price-offers`, accept/reject; `/orders`, `/orders/{id}`, confirm-receipt | Chat giá, checkout và timeline |
| Exchange | `/exchange-offers`, counter/accept/reject/cancel, `/exchanges/{id}/receipts` | Sent/Received/Negotiating và handover |
| Smart Swap | `/swap-matches`, `/swap-cycles`, agreement revision, consent/reject, handover confirmations | Find/matches/detail/3 consent/6 confirmations |
| Chat | `/conversations`, messages/history, receipts; WebSocket `message.created`, `receipt.updated` | Inbox, chat 1–1/nhóm, read receipts |
| State events | `offer.updated`, `agreement.revised`, `consent.updated`, `handover.updated` | Cập nhật giữa các máy đang ở cùng giao dịch |
| Support/Disputes | `/support-tickets`, `/disputes`, evidence và history | HT tracking khác KN/Admin decision |
| Reviews | `/transactions/{id}/reviews` với recipientId | Đánh giá mua bán/1–1/2 đối tác swap |
| Admin | `/admin/users`, `/admin/listings`, `/admin/transactions`, `/admin/disputes`, `/admin/categories`, `/admin/reports` | Tất cả truy cập kiểm tra role và audit mutation |

Khóa tài khoản theo copy chặn đăng tin, nhắn tin và tạo giao dịch mới; giữ giao dịch hiện có để xử lý. Cần quyết định quyền xem/xác nhận giao dịch đang mở, khiếu nại và hỗ trợ trong thời gian khóa. Admin quyết định hoàn trả hai món/tiền bù là lệnh ghi nhận và theo dõi thực hiện, không gọi tự động hoàn tiền bù qua app.

## 9. Các mốc triển khai và điều kiện nghiệm thu

| Mốc | Công việc | Phụ thuộc / hoàn thành khi |
| --- | --- | --- |
| 0 — Chốt thiết kế | Sửa liên kết P0, role/góc nhìn, tên màn sai; xuất PNG; xác định scroll cho 12 frame; chốt thanh toán và consent version | Có bảng quyết định rõ phần đã chốt/phần deferred; không lấp chỗ thiếu bằng màn suy đoán |
| 1 — Nền tảng | Kiểm tra build hiện tại, thiết lập navigation/state, token/component, backend skeleton, DB migration/API contract | App/backend build được; màn tham chiếu 360×800 và auth/role boundary được chứng minh |
| 2 — Account + Catalog | Auth, profile, settings; home/search/category/detail | Dữ liệu API thật hoặc fixture có ghi rõ; back stack và form validation đúng |
| 3 — Đăng tin + moderation | Form bán/trao đổi, nháp, preview, submit, management/edit/sold; Admin duyệt/từ chối/ẩn | Tin chưa duyệt không xuất hiện công khai; upload/giới hạn ảnh và reason/audit đúng |
| 4 — Chat giá + Orders | Inbox, chat negotiation, giá thỏa thuận, checkout, order timeline | Không tạo order trùng khi retry; giá server; tích hợp thanh toán chỉ sau chốt contract |
| 5 — Trao đổi 1–1 | Explore/filter, chọn đồ, offer/counter/accept, chat, meeting/receipts/review | Counter không tự áp dụng; hai bên cùng xác nhận mới completed; route 1–1 đúng |
| 6 — Smart Swap | Matching, cycle, tiền bù, consent, chat nhóm, 6 confirmations, 2 review targets | Không bàn giao dưới 3/3; không hoàn tất dưới 6/6; chính sách đổi thỏa thuận được áp dụng |
| 7 — Admin còn lại + Support | User lock/unlock, transactions, KN/evidence/decision, categories, reports; HT và review flow | Kiểm tra quyền server, audit, dữ liệu báo cáo khớp giao dịch; quyết định không tự đóng tranh chấp |
| 8 — Nghiệm thu | Đối chiếu frame PNG, luồng đầy đủ, error/retry/reconnect, thiết bị và accessibility | Ma trận frame/state/role đã kiểm tra, không còn P0 và không có chức năng giả báo thành công |

Support hoặc review có thể làm cùng mốc nghiệp vụ liên quan. Các mốc là thứ tự phụ thuộc, không phải ước lượng ngày công. Chưa có đủ đặc tả backend, quy mô nhóm hoặc tích hợp thanh toán để đưa lịch chính xác.

## 10. Kế hoạch kiểm thử cho giai đoạn lập trình

Các kiểm thử dưới đây **chưa được viết hoặc chạy**. Chọn kiểm thử bảo vệ quy tắc, không tạo test chỉ lặp lại implementation.

- **Domain/API:** quyền sở hữu, tin đủ điều kiện; hai giao dịch tranh cùng listing; retry cùng idempotency key; accept revision cũ bị từ chối; khoản bù đúng payer/payee; số tiền nguyên và giới hạn.
- **State 1–1:** counter chưa được accept không đổi agreement; người A nhận hàng không tự xác nhận B; chỉ khi đủ hai phía mới cho review.
- **State 3 bên:** dưới 3/3 không mở bàn giao; 5/6 chưa completed; xác nhận lặp không tăng count; thay đổi version xử lý consent theo chính sách đã chốt; đúng hai đối tượng review.
- **Admin:** user thường gọi API Admin bị từ chối; reject đúng listingId; khóa/ẩn vẫn giữ lịch sử; quyết định tranh chấp chưa đồng nghĩa đã thực hiện hoàn trả.
- **Chat/realtime:** reconnect không mất tin, gửi lại không trùng, quyền membership, thứ tự/read receipt, event sau commit.
- **Luồng UI:** register/login/profile; bán→preview→pending→moderation; chat giá→checkout→order→support/review; 1–1 đầy đủ; swap đầy đủ; lock/reject/dispute.
- **Visual:** đối chiếu các nhóm frame đại diện rồi từng trạng thái: 360×800, 12 frame dài, dialog/drawer/sheet, chat có bàn phím, màn Admin dài; Roboto tiếng Việt, ảnh crop, line wrapping, font scaling và insets.
- **Trạng thái bổ sung:** loading, rỗng, lỗi mạng, session hết hạn, validation, media lỗi, permission bị từ chối. Chỉ nghiệm thu UI sau khi có thiết kế hoặc quyết định bổ sung được ghi nhận.

## 11. Những quyết định đang mở

| Chủ đề | Nội dung cần chốt | Ảnh hưởng |
| --- | --- | --- |
| Prototype / role | Sửa route lệch 1–1/3 bên; phân biệt quản lý tin với lịch sử đề nghị; entry Admin | Navigation và authorization acceptance |
| Style | Giữ legacy màu/size hay chuẩn hóa; xử lý text trắng, text/header lặp | Bộ token và ảnh đối chiếu |
| Ảnh | Giới hạn dung lượng, aspect/crop, video bằng chứng, quyền xem; tin hỗn hợp tối đa 6 hay 8 | Upload contract, storage và form |
| Order/payment | Phương thức, provider, giữ/giải ngân, ship, hủy/hoàn | Chặn làm thanh toán production cho tới khi đủ contract |
| Offer/reservation | Hủy/hết hạn/counter gia hạn; khi nào giữ món, thời gian giữ và giải phóng | Chống double booking và nhánh UI còn thiếu |
| Swap matching | Cách mô tả mong muốn, ranking, tiền bù đề xuất và giới hạn từng participant | Matching API; không suy công thức từ fixture |
| Agreement changes | Sửa bù sau 3/3 có reset đồng ý không; có cho sửa sau hẹn/bàn giao không | Versioning, consent và chat actions |
| Completion | Tiền bù xác nhận có là gate riêng; xử lý một người im lặng hoặc tranh chấp | Hoàn tất, timeout, hỗ trợ/Admin |
| Account restriction | Quyền xử lý giao dịch cũ và hỗ trợ khi bị khóa | Permission matrix phía server |
| Screens chưa có | Verify contact, privacy/delete, notification, empty/error/cancel states | Chỉ bổ sung thiết kế sau; không liệt kê là màn đã đọc |

Các phần độc lập như đối chiếu asset, chốt component và chuẩn bị contract có thể tiến hành trước khi mọi câu hỏi được trả lời. Phần phụ thuộc vào quyết định mở phải giữ trạng thái chưa chốt; không làm tính năng production dựa vào giả định ngầm.

## 12. Kết quả của giai đoạn phân tích này

Đã tạo hai tài liệu trong workspace: danh mục đủ 83 frame, phân tích style/layout/component, các trạng thái và toàn bộ 231 liên kết Prototype; kế hoạch kiến trúc Android/backend, mô hình dữ liệu, route và mốc triển khai. Chỉ hai tài liệu Markdown được thêm cho ứng dụng; không dựng UI, API, schema DB hay thay đổi cấu hình build. Chưa có kiểm chứng chạy Prototype đầy đủ hoặc build/test ứng dụng; đó là công việc của các mốc sau.
