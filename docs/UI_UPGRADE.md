# Nâng cấp UI/UX ReMarket — 13/11/2026

Nâng cấp trực tiếp 28 file UI, giữ Roboto, màu xanh ReMarket, các màn hình và điều hướng hiện có. Không đổi Backend, REST API, EF migrations hoặc schema; 7 ViewModel không thay đổi. Giao diện tiếp tục lấy dữ liệu từ API thật.

| Nhóm | File UI đã sửa/thêm |
| --- | --- |
|core/designsystem|[ReMarketTheme.kt](../Android/app/src/main/java/com/ute/app_ban_do_cu/core/designsystem/ReMarketTheme.kt), [TaiKhoanComponents.kt](../Android/app/src/main/java/com/ute/app_ban_do_cu/core/designsystem/TaiKhoanComponents.kt), [MarketplaceComponents.kt](../Android/app/src/main/java/com/ute/app_ban_do_cu/core/designsystem/MarketplaceComponents.kt)|
|feature/account|[HoSoScreens.kt](../Android/app/src/main/java/com/ute/app_ban_do_cu/feature/account/HoSoScreens.kt)|
|feature/admin|[QuanTriApp.kt](../Android/app/src/main/java/com/ute/app_ban_do_cu/feature/admin/QuanTriApp.kt), [QuanTriComponents.kt](../Android/app/src/main/java/com/ute/app_ban_do_cu/feature/admin/QuanTriComponents.kt), [QuanTriDashboard.kt](../Android/app/src/main/java/com/ute/app_ban_do_cu/feature/admin/QuanTriDashboard.kt), [QuanTriTransactions.kt](../Android/app/src/main/java/com/ute/app_ban_do_cu/feature/admin/QuanTriTransactions.kt)|
|feature/auth|[TaiKhoanApp.kt](../Android/app/src/main/java/com/ute/app_ban_do_cu/feature/auth/TaiKhoanApp.kt)|
|feature/catalog|[BanHangApp.kt](../Android/app/src/main/java/com/ute/app_ban_do_cu/feature/catalog/BanHangApp.kt), [BanHangComponents.kt](../Android/app/src/main/java/com/ute/app_ban_do_cu/feature/catalog/BanHangComponents.kt), [KhamPhaScreens.kt](../Android/app/src/main/java/com/ute/app_ban_do_cu/feature/catalog/KhamPhaScreens.kt), [QuanLyTinScreens.kt](../Android/app/src/main/java/com/ute/app_ban_do_cu/feature/catalog/QuanLyTinScreens.kt), [TinDangScreens.kt](../Android/app/src/main/java/com/ute/app_ban_do_cu/feature/catalog/TinDangScreens.kt)|
|feature/commerce|[ChatScreens.kt](../Android/app/src/main/java/com/ute/app_ban_do_cu/feature/commerce/ChatScreens.kt), [DonHangScreens.kt](../Android/app/src/main/java/com/ute/app_ban_do_cu/feature/commerce/DonHangScreens.kt), [MuaBanApp.kt](../Android/app/src/main/java/com/ute/app_ban_do_cu/feature/commerce/MuaBanApp.kt), [MuaBanComponents.kt](../Android/app/src/main/java/com/ute/app_ban_do_cu/feature/commerce/MuaBanComponents.kt)|
|feature/exchange|[TraoDoiApp.kt](../Android/app/src/main/java/com/ute/app_ban_do_cu/feature/exchange/TraoDoiApp.kt), [TraoDoiComponents.kt](../Android/app/src/main/java/com/ute/app_ban_do_cu/feature/exchange/TraoDoiComponents.kt), [TraoDoiExplore.kt](../Android/app/src/main/java/com/ute/app_ban_do_cu/feature/exchange/TraoDoiExplore.kt), [TraoDoiOfferScreens.kt](../Android/app/src/main/java/com/ute/app_ban_do_cu/feature/exchange/TraoDoiOfferScreens.kt)|
|feature/smartswap|[SmartSwapApp.kt](../Android/app/src/main/java/com/ute/app_ban_do_cu/feature/smartswap/SmartSwapApp.kt), [SmartSwapChat.kt](../Android/app/src/main/java/com/ute/app_ban_do_cu/feature/smartswap/SmartSwapChat.kt), [SmartSwapCircle.kt](../Android/app/src/main/java/com/ute/app_ban_do_cu/feature/smartswap/SmartSwapCircle.kt), [SmartSwapComponents.kt](../Android/app/src/main/java/com/ute/app_ban_do_cu/feature/smartswap/SmartSwapComponents.kt), [SmartSwapDeliveryScreens.kt](../Android/app/src/main/java/com/ute/app_ban_do_cu/feature/smartswap/SmartSwapDeliveryScreens.kt), [SmartSwapDiscovery.kt](../Android/app/src/main/java/com/ute/app_ban_do_cu/feature/smartswap/SmartSwapDiscovery.kt)|

- Chuẩn hóa card, button linh hoạt chiều cao, field focus/lỗi, badge, modal và thanh điều hướng Material 3.
- Trang chủ/danh sách: ảnh lớn, tên hai dòng, giá nổi bật, thông tin vị trí/tình trạng; tìm kiếm có xóa từ khóa.
- Form đăng tin chia nhóm, preview/quản lý tin rõ trạng thái; khắc phục padding bàn phím bị cộng lặp.
- Checkout/đơn hàng: tổng tiền, thẻ đơn và timeline; trao đổi 1–1 có cặp sản phẩm và bảng tiền bù.
- Smart Swap: sơ đồ co giãn, ba người/ba món, xác nhận và tiến độ nhận hàng; lịch sử vận chuyển có thể mở/đóng.
- Chat dùng bubble chung; ô soạn nhóm hiển thị tối đa bốn dòng và giữ toàn bộ nội dung. Thông báo hệ thống hiển thị trạng thái bằng tiếng Việt. Tài khoản/Admin đồng bộ card, tìm kiếm và trạng thái.

## Kiểm tra

- Các nhóm đều qua build; bản cuối: `assembleDebug`, `assembleDebugAndroidTest`, `lintDebug`, `testDebugUnitTest` **thành công** ([log](qa/ui-upgrade/12-final-validation-build.txt)). JVM unit test hiện có là smoke test mẫu.
- **22/22 ca UI hồi quy đạt**, dùng API/SQL thật và giới hạn API mặc định ([log](qa/ui-upgrade/android-e2e.txt)). Lượt đầu gặp 429 do tạo fixture dồn; đã thêm pacing vào client test, giữ nguyên giới hạn Backend ([log ban đầu](qa/ui-upgrade/android-e2e-rate-limit.txt)).
- Sau chỉnh cuối: **2/2 ca hiển thị bổ sung đạt**, gồm bàn phím đang mở, 320×640/font 130% và bản nháp chat 30 dòng không che cuộc trò chuyện ([log](qa/ui-upgrade/final-responsive-check.txt)).
- Lint: **0 lỗi, 24 cảnh báo**, chủ yếu dependency/vector và asset Figma cũ được giữ lại nhưng không còn dùng. Không nâng dependency hàng loạt.
- [121 ảnh hồi quy](qa/ui-upgrade/screenshot-manifest.json); [ảnh kiểm tra bổ sung cuối](qa/ui-upgrade/targeted-screenshots/). Ảnh màu đặc là fixture được upload qua API, không phải dữ liệu thay thế API trong ứng dụng.
- Vòng demo gốc vẫn **2/3**, 11 migrations giữ nguyên ([kiểm tra](qa/ui-upgrade/demo-progress-check.txt)). Không hoàn tất thay người dùng.

[Gallery trang chủ/catalog](qa/ui-upgrade/gallery-catalog.png) · [mua bán](qa/ui-upgrade/gallery-commerce.png) · [1–1](qa/ui-upgrade/gallery-exchange.png) · [Smart Swap](qa/ui-upgrade/gallery-smart-swap.png) · [tài khoản](qa/ui-upgrade/gallery-account.png) · [Admin](qa/ui-upgrade/gallery-admin.png). Gallery hồi quy chụp trước chỉnh nhỏ cuối cho tên rút gọn/icon, ô soạn chat và nhãn trạng thái hệ thống; ảnh bổ sung ghi nhận bản cuối.

Các mục Cài đặt vốn chưa triển khai vẫn được giữ disabled; đợt này không bổ sung nghiệp vụ mới. **Chưa xác minh bản UI mới trên máy thật**: hệ thống phê duyệt tự động từ chối bước cài/kiểm tra với thông báo `blocked by policy`. [APK Debug](../Android/app/build/outputs/apk/debug/app-debug.apk) đã build sẵn; Backend demo 5080 được giữ hoạt động.

Tham khảo cách nhóm nội dung và phân cấp thao tác: [Material 3](https://developer.android.com/design/ui/mobile/guides/components/material-overview), [Chợ Tốt](https://www.chotot.com/), [Carousell](https://www.carousell.sg/buy-and-sell-with-buy-button/l/).

Cập nhật 14/11/2026: bảy mục chính của Admin luôn dùng nút mở menu để chuyển trực tiếp giữa các mục; trang chi tiết/form vẫn giữ nút quay lại. Đã build APK/test APK và lint thành công ([log](qa/admin-navigation/build.txt)); ca UI Admin qua API thật đạt, bao gồm chuyển mục và các luồng chi tiết ([log](qa/admin-navigation/android-e2e.txt), [ảnh Người dùng](qa/admin-navigation/users.png)).

Tài khoản Admin: bỏ nút ở Tổng quan, mở từ cụm avatar/tên trong menu. Thêm [TaiKhoanQuanTriScreen.kt](../Android/app/src/main/java/com/ute/app_ban_do_cu/feature/admin/TaiKhoanQuanTriScreen.kt) riêng, gồm hồ sơ, mật khẩu và đăng xuất; không đưa các liên kết mua bán của USER vào trang này. Đã kiểm tra quay lại đúng mục quản trị, khôi phục vai trò khi khởi động Activity và thu hồi JWT khi đăng xuất; **2/2 ca USER/Admin đạt** ([log](qa/admin-account/android-e2e.txt), [ảnh](qa/admin-account/account.png), [build/lint](qa/admin-account/build.txt)). Kiểm tra mật khẩu Admin chỉ gồm điều hướng/validation trống, giữ nguyên mật khẩu và hồ sơ hiện tại. Lượt Emulator đầu bị System UI ANR; lượt chạy lại sau khôi phục đã đạt.
