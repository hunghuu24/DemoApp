# ReMarket — Thiết kế CSDL quan hệ

SQL Server / EF Core8.0.33. Đã có **48 bảng nghiệp vụ,100 khóa ngoại,70 CHECK constraint và22 trigger**, chưa tính `__EFMigrationsHistory`. Đã áp dụng11 migrations trên SQL Server2022; migration10 bổ sung giao nhận/ba vận đơn/sự cố Smart Swap, migration11 bổ sung icon danh mục và căn cứ/thông báo/hướng xử lý/idempotency quyết định quản trị. Đã kiểm tra script idempotent hai lần trên DB mới; dữ liệu cũ giữ nguyên.

Nguồn thiết kế: [FIGMA_ANALYSIS.md](../../FIGMA_ANALYSIS.md). Phiên bản này chuẩn bị dữ liệu cho các nhóm màn đã đọc; các chi tiết bổ sung như schema, constraint, reservation, version agreement là quyết định kỹ thuật, không phải schema được cung cấp bởi Figma.

## 1. Quy ước

- Tên bảng/cột/model tiếng Việt **không dấu**, PascalCase; schema `dbo`. Text tiếng Việt lưu `nvarchar`, collation `Vietnamese_100_CI_AS`. Enum mã không dấu lưu `varchar(40)` và CHECK tập giá trị, không lưu nhãn UI tùy ý.
- Mọi bảng có `Id uniqueidentifier` (PK, ứng dụng cấp GUID), `NgayTao datetimeoffset(0)` (UTC do ứng dụng cung cấp), `PhienBan rowversion` (SQL Server sinh, EF concurrency token). `PhienBan` khác `SoPhienBan` của thỏa thuận nghiệp vụ.
- Tiền VND là `bigint`/C# `long`, không dùng float/decimal có phần lẻ. Có CHECK cho giá trị và tổng tiền đơn.
- FK dùng **NO ACTION** để không xóa dây chuyền lịch sử; account/tin có trạng thái thay vì xóa vật lý tùy tiện. Không có cascade delete ngầm.
- Navigation property có trong model; quan hệ reference cấu hình rõ FK/principal key, kể cả nhiều reference cùng bảng. Các quan hệ 1–1 logic được bảo vệ bằng unique index trên FK.
- Optional unique (SĐT, mã tham chiếu, conversation chưa gắn giao dịch) dùng filtered unique index do SQL Server provider sinh; nhiều NULL được phép. Unique email tính trên `UPPER(LTRIM(RTRIM(Email)))` persisted computed column, ngăn trùng khác hoa/thường hoặc khoảng trắng hai đầu.

## 2. Bảng theo nhóm nghiệp vụ

| Nhóm | Bảng | Số cột riêng | FK |
| --- | --- | --- | --- |
| TaiKhoan | VaiTro | 2 | 0 |
| TaiKhoan | NguoiDung | 5 | 0 |
| TaiKhoan | NguoiDungVaiTro | 2 | 2 |
| TaiKhoan | HoSoCaNhan | 7 | 1 |
| TaiKhoan | PhienDangNhap | 5 | 1 |
| TaiKhoan | YeuCauDatLaiMatKhau | 4 | 1 |
| SanPham | DanhMuc | 6 | 1 |
| SanPham | SanPham | 6 | 2 |
| SanPham | AnhSanPham | 4 | 1 |
| SanPham | TinDang | 13 | 2 |
| SanPham | NhuCauTraoDoi | 6 | 2 |
| GiaoDich | GiaoDich | 4 | 0 |
| GiaoDich | ThanhVienGiaoDich | 3 | 2 |
| GiaoDich | GiuChoSanPham | 4 | 2 |
| DonHang | DonHang | 16 | 4 |
| DonHang | ChiTietDonHang | 8 | 2 |
| DonHang | ThanhToan | 10 | 1 |
| DonHang | PhieuThuongLuong | 8 | 4 |
| TraoDoi | DeNghiTraoDoi | 7 | 5 |
| TraoDoi | PhienBanThoaThuan | 7 | 2 |
| TraoDoi | DongYThoaThuan | 5 | 2 |
| TraoDoi | TienBu | 7 | 3 |
| SmartSwap | VongSmartSwap | 3 | 1 |
| SmartSwap | ThanhVienVong | 5 | 3 |
| SmartSwap | ChangTraoDoi | 4 | 3 |
| Chat | CuocTroChuyen | 5 | 3 |
| Chat | ThanhVienTroChuyen | 4 | 2 |
| Chat | TinNhan | 6 | 1 |
| Chat | TepTinNhan | 5 | 1 |
| Chat | ThongBao | 6 | 2 |
| VanChuyen | VanChuyenMoPhong | 4 | 1 |
| VanChuyen | LichSuVanChuyen | 5 | 1 |
| VanChuyen | XacNhanGiaoNhan | 7 | 5 |
| VanChuyen | BangChungGiaoNhan | 4 | 2 |
| DanhGia | DanhGia | 7 | 3 |
| DanhGia | TieuChiDanhGia | 2 | 0 |
| DanhGia | DanhGiaTieuChi | 2 | 2 |
| KhieuNai | KhieuNai | 8 | 3 |
| KhieuNai | BangChungKhieuNai | 5 | 2 |
| KhieuNai | PhanHoiKhieuNai | 3 | 2 |
| KhieuNai | QuyetDinhXuLy | 10 | 2 |
| HoTro | YeuCauHoTro | 8 | 2 |
| HoTro | PhanHoiHoTro | 3 | 2 |
| HoTro | LichSuHoTro | 5 | 2 |
| QuanTri | NhatKyKiemDuyet | 5 | 2 |
| QuanTri | KhoaTaiKhoan | 6 | 2 |
| QuanTri | NhatKyQuanTri | 5 | 1 |

## 3. Quan hệ và nguyên tắc mô hình hóa

| Quan hệ | Cardinality / ý nghĩa |
| --- | --- |
| NguoiDung ↔ VaiTro | N–N qua NguoiDungVaiTro, unique theo cặp |
| NguoiDung → HoSoCaNhan | 1–0..1, FK hồ sơ unique |
| DanhMuc → DanhMuc con | 1–N self-reference; CHECK không tự làm cha; chống vòng nhiều cấp thuộc service sau này |
| NguoiDung → SanPham → AnhSanPham/TinDang | 1–N; sản phẩm tách khỏi vòng đời tin; mỗi sản phẩm tối đa một tin pending/published |
| TinDang → NhuCauTraoDoi | 1–0..1, một mô tả nhu cầu/giới hạn bù và tùy chọn Smart Swap |
| GiaoDich → DonHang / DeNghiTraoDoi / VongSmartSwap | Các subtype có GiaoDichId unique; trigger kiểm tra đúng Loai; root không tự chứng minh đã có đủ chi tiết khi còn KhoiTao |
| GiaoDich ↔ NguoiDung | Qua ThanhVienGiaoDich; alternate key (GiaoDichId,NguoiDungId) để FK ràng đúng participant |
| DonHang → ChiTietDonHang/ThanhToan/VanChuyenMoPhong | Một đơn thuộc một người bán; dòng giữ snapshot tên/giá. Hàng cũ đơn chiếc có SoLuong=1; vận chuyển là 0..1/đơn |
| DeNghiTraoDoi → hai sản phẩm | FK composite (SanPhamId,NguoiGui/NhanId) khớp (Id,ChuSoHuuId), không đề nghị món thuộc người khác |
| GiaoDich → PhienBanThoaThuan → DongYThoaThuan/TienBu | 1–N phiên bản; unique số version; tối đa một bản DaChot. Consent và tiền bù chỉ dùng participant của đúng giao dịch |
| VongSmartSwap → ThanhVienVong → ChangTraoDoi | Vị trí 1..3, không trùng user/sản phẩm/vị trí; mỗi người giao một cạnh, nhận một cạnh; FK composite buộc món thuộc đúng người giao và cả hai thuộc cùng vòng |
| CuocTroChuyen ↔ NguoiDung → TinNhan | Membership riêng để chat trước khi có giao dịch; FK (CuocTroChuyenId,NguoiGuiId) chặn sender ngoài chat; client message key unique chống gửi lặp |
| GiaoDich → XacNhanGiaoNhan → BangChungGiaoNhan | Một xác nhận mỗi (giao dịch, người, loại Giao/Nhan); partner/version phải cùng giao dịch, trigger đối chiếu đúng món/cạnh |
| GiaoDich → DanhGia | Reviewer/reviewee cùng participant, không tự đánh giá; unique người đánh giá/được đánh giá/giao dịch; sao 1..5 |
| GiaoDich → KhieuNai → bằng chứng/phản hồi/quyết định | Hai người trong cùng giao dịch; quyết định khác với xác minh đã thực hiện; không tự hoàn tiền |
| YeuCauHoTro → DonHang tùy chọn | Mã HT khác mã KN; cho phép hỗ trợ tài khoản không gắn đơn |
| NguoiDung/TinDang → lịch sử quản trị | Người thực hiện có FK user; role/permission còn phải kiểm tra tại service/API |

`SanPham.ChuSoHuuId` là người sở hữu/đăng món tại thời điểm tạo nguồn tin, giữ ổn định để lịch sử còn tham chiếu. Không cập nhật trực tiếp trường này sau trao đổi; việc tái đăng món đã nhận cần nguồn/snapshot mới hoặc thêm mô hình chuyển quyền ở bước sau. `NhatKyQuanTri.DoiTuongId` cố ý là ID snapshot đa loại, không giả tạo một FK trỏ nhiều bảng; `NguoiThucHienId` vẫn có FK.

## 4. Ràng buộc đã thực thi

### Khóa, CHECK và index

- Giá không âm, giá bán dương cho tin mua bán/CaHai; tổng đơn = tiền hàng + phí; không tự mua/đổi; FK cùng chủ sở hữu/cùng vòng/cùng participant.
- Enum hợp lệ; hạn giữ chỗ/đề nghị sau ngày tạo; thời hạn khóa hợp lệ; trạng thái hoàn tất của root bắt buộc có ngày hoàn tất.
- Tối đa một ảnh bìa, không trùng thứ tự ảnh, một reservation chưa giải phóng/món. Reservation hết hạn vẫn cần service/job giải phóng rõ ràng, không tự biến mất khỏi unique index theo đồng hồ.
- Unique các mã, số version, người/vị trí/món trong vòng, review, receipt, client message key. Toàn bộ chi tiết index nằm trong configuration và SQL migration.

### Trigger nhiều bảng

- `TR_DonHang_LoaiGiaoDich`
- `TR_DeNghiTraoDoi_LoaiGiaoDich`
- `TR_VongSmartSwap_LoaiGiaoDich`
- `TR_ThanhVienGiaoDich_SoLuong`
- `TR_PhienBanThoaThuan_Chot`
- `TR_DongYThoaThuan_Khoa`
- `TR_TienBu_KhoaNoiDung`
- `TR_VongSmartSwap_DieuKien`
- `TR_ThanhVienVong_Khoa`
- `TR_ChangTraoDoi_Khoa`
- `TR_XacNhanGiaoNhan_HopLe`
- `TR_GiaoDich_HoanTat`
- `TR_DanhGia_DaHoanTat`
- `TR_BangChungGiaoNhan_GioiHan`
- `TR_AnhSanPham_GioiHan`
- `TR_TinDang_GioiHanAnh`
- `TR_KhieuNai_DongPhieu`

Các trigger xử lý tập `inserted/deleted`, không giả định chỉ một dòng. Những điều kiện chính:

1. Subtype khớp LoaiGiaoDich; participant tối đa 2 hoặc 3; không thay participant khi root đã bắt đầu.
2. Chốt thỏa thuận chỉ sau đủ 2/2 hoặc 3/3 đồng ý. Nội dung/tiền bù đã chốt không được sửa tại chỗ; xác nhận đã trả/nhận tiền bù được phép cập nhật riêng.
3. Mở bàn giao Smart Swap cần 3 thành viên, 3 cạnh và 3 consent của bản DaChot; hoàn tất vòng cần đủ 6 xác nhận. Hoàn tất root 1–1 cần hai lượt nhận đúng bản đã chốt.
4. Receipt đúng món, đối tác và chiều giao/nhận; không sửa receipt sau khi root hoàn tất. Đánh giá chỉ khi root hoàn tất.
5. Ảnh tin bán tối đa 8, tin trao đổi/CaHai tối đa 6; bằng chứng một receipt tối đa 5 ảnh JPG/PNG. Tin CaHai chọn 6 là quyết định bổ sung đã ghi ở kiến trúc.
6. Không đóng khiếu nại nếu chưa có quyết định được đánh dấu xác minh đã thực hiện. Quyết định không gọi payment/refund.

Các điều kiện aggregate được kiểm tra tại transition tương ứng, **không phải một hệ workflow đã hoàn chỉnh cho mọi cách sửa SQL tùy ý**. Ví dụ thay agreement trong giao dịch đang mở phải được service thực hiện nguyên tử và reset/khóa phase đúng; quyền Admin, eligibility của người bị khóa, độ hợp lệ mong muốn matching, hạn phản hồi và đồng bộ current state là công việc tiếp theo. Chỉ cấp DB write cho backend, không để client tự cập nhật mọi bảng/cột.

`UseSqlOutputClause(false)` được cấu hình để EF ghi được vào bảng có trigger với SQL Server. Script triển khai thêm SET options cần thiết cho computed column/filtered index; không thay column enum varchar bằng literal Unicode trong filtered predicate.

## 5. Migration, seed và kết nối

- [DbContext](../../Backend/src/ReMarket.Infrastructure/Persistence/ReMarketDbContext.cs), [Configurations](../../Backend/src/ReMarket.Infrastructure/Configurations), [Models](../../Backend/src/ReMarket.Domain/Models), [Migrations](../../Backend/src/ReMarket.Infrastructure/Persistence/Migrations).
- [SQL idempotent](remarket-schema.sql) là bản xuất từ migration, đã được áp hai lần trên một DB mới độc lập trước khi seed/check.
- [Dữ liệu demo](DU_LIEU_DEMO.md) phủ đủ 43 bảng; [hướng dẫn backend](../../Backend/README.md) có lệnh kết nối/migrate/seed.
- [schema-dictionary.json](schema-dictionary.json) là bản tra cứu metadata phục vụ tài liệu; source of truth khi phát triển là model/configuration và migration snapshot. Khi đổi schema phải cập nhật tài liệu tương ứng, không chỉ sửa JSON.

## 6. Từ điển cột và khóa ngoại đầy đủ

Mỗi bảng bên dưới còn có ba cột chung `Id`, `NgayTao`, `PhienBan` đã nêu ở mục 1. `Có` trong cột NULL nghĩa là được bỏ trống; cột tham chiếu FK ghép dùng các field đã liệt kê, không tạo thêm shadow field cho cùng quan hệ.

### VaiTro

| Cột | SQL Server | NULL | Ghi chú |
| --- | --- | --- | --- |
| Ma | nvarchar(40) | Không | — |
| Ten | nvarchar(100) | Không | — |

- Unique index: `(Ma)`.

### NguoiDung

| Cột | SQL Server | NULL | Ghi chú |
| --- | --- | --- | --- |
| Email | nvarchar(256) | Không | — |
| EmailChuanHoa | nvarchar(256) | Không | Computed persisted: UPPER(LTRIM(RTRIM([Email]))) |
| SoDienThoai | nvarchar(20) | Có | — |
| MatKhauBam | nvarchar(512) | Không | — |
| TrangThai | varchar(40) | Không | Enum TrangThaiNguoiDung |

- Unique index: `(EmailChuanHoa)`.
- Unique index: `(SoDienThoai)`.
- CHECK Email: `LEN(LTRIM(RTRIM([Email]))) > 3`.

### NguoiDungVaiTro

| Cột | SQL Server | NULL | Ghi chú |
| --- | --- | --- | --- |
| NguoiDungId | uniqueidentifier | Không | FK: NguoiDung |
| VaiTroId | uniqueidentifier | Không | FK: VaiTro |

- FK NguoiDung: `(NguoiDungId)` → `NguoiDung(Id)`, NO ACTION.
- FK VaiTro: `(VaiTroId)` → `VaiTro(Id)`, NO ACTION.
- Unique index: `(NguoiDungId, VaiTroId)`.

### HoSoCaNhan

| Cột | SQL Server | NULL | Ghi chú |
| --- | --- | --- | --- |
| NguoiDungId | uniqueidentifier | Không | FK: NguoiDung |
| HoTen | nvarchar(120) | Không | — |
| AnhDaiDienUrl | nvarchar(1000) | Có | — |
| DiaChi | nvarchar(500) | Có | — |
| TinhThanh | nvarchar(100) | Có | — |
| DaXacMinhEmail | bit | Không | — |
| DaXacMinhDienThoai | bit | Không | — |

- FK NguoiDung: `(NguoiDungId)` → `NguoiDung(Id)`, NO ACTION.
- Unique index: `(NguoiDungId)`.

### DanhMuc

| Cột | SQL Server | NULL | Ghi chú |
| --- | --- | --- | --- |
| Ten | nvarchar(120) | Không | — |
| Ma | nvarchar(60) | Không | — |
| DanhMucChaId | uniqueidentifier | Có | FK: DanhMucCha |
| ThuTu | int | Không | — |
| DangHienThi | bit | Không | — |
| BieuTuong | nvarchar(30) | Không | Mã icon được API cho phép, mặc định rỗng cho dữ liệu cũ |

- FK DanhMucCha: `(DanhMucChaId)` → `DanhMuc(Id)`, NO ACTION.
- Unique index: `(Ma)`.
- CHECK KhongTuThamChieu: `[DanhMucChaId] IS NULL OR [DanhMucChaId] <> [Id]`.
- CHECK ThuTu: `[ThuTu] >= 0`.

### SanPham

| Cột | SQL Server | NULL | Ghi chú |
| --- | --- | --- | --- |
| ChuSoHuuId | uniqueidentifier | Không | FK: ChuSoHuu |
| DanhMucId | uniqueidentifier | Không | FK: DanhMuc |
| Ten | nvarchar(200) | Không | — |
| MoTa | nvarchar(4000) | Không | — |
| TinhTrang | varchar(40) | Không | Enum TinhTrangSanPham |
| GiaTriThamKhao | bigint | Không | — |

- FK ChuSoHuu: `(ChuSoHuuId)` → `NguoiDung(Id)`, NO ACTION.
- FK DanhMuc: `(DanhMucId)` → `DanhMuc(Id)`, NO ACTION.
- Alternate key: `(Id, ChuSoHuuId)`.
- CHECK GiaTri: `[GiaTriThamKhao] >= 0`.

### AnhSanPham

| Cột | SQL Server | NULL | Ghi chú |
| --- | --- | --- | --- |
| SanPhamId | uniqueidentifier | Không | FK: SanPham |
| DuongDan | nvarchar(1000) | Không | — |
| ThuTu | int | Không | — |
| LaAnhBia | bit | Không | — |

- FK SanPham: `(SanPhamId)` → `SanPham(Id)`, NO ACTION.
- Unique index: `(SanPhamId, ThuTu)`.
- Filtered unique index: `(SanPhamId)`, WHERE `[LaAnhBia] = 1`.
- CHECK ThuTu: `[ThuTu] >= 0`.

### TinDang

| Cột | SQL Server | NULL | Ghi chú |
| --- | --- | --- | --- |
| SanPhamId | uniqueidentifier | Không | FK: SanPham |
| NguoiDangId | uniqueidentifier | Không | FK: SanPham |
| LoaiTin | varchar(40) | Không | Enum LoaiTinDang |
| GiaBan | bigint | Có | — |
| TrangThai | varchar(40) | Không | Enum TrangThaiTinDang |
| TinhThanh | nvarchar(100) | Không | — |
| DiaDiem | nvarchar(500) | Không | — |
| NgayDang | datetimeoffset(0) | Có | — |
| NguoiDuyetId | uniqueidentifier | Có | FK: NguoiDuyet |
| LyDoTuChoi | nvarchar(1000) | Có | — |
| NgayCapNhat | datetimeoffset(0) | Có | Thứ tự tin của tôi, cập nhật aggregate |
| NgayXoa | datetimeoffset(0) | Có | Xóa mềm, giữ FK/lịch sử |
| LuotXem | int | Không | Dự phòng analytics, mặc định0; chưa có bộ đếm lượt xem |

- FK SanPham: `(SanPhamId, NguoiDangId)` → `SanPham(Id, ChuSoHuuId)`, NO ACTION.
- FK NguoiDuyet: `(NguoiDuyetId)` → `NguoiDung(Id)`, NO ACTION.
- Alternate key: `(Id, SanPhamId)`.
- Filtered unique index: `(SanPhamId)`, WHERE `[TrangThai] IN ('ChoDuyet', 'DangHienThi')`.
- CHECK GiaBan: `([LoaiTin] = N'TraoDoi' AND ([GiaBan] IS NULL OR [GiaBan] >= 0)) OR ([LoaiTin] IN (N'MuaBan', N'CaHai') AND [GiaBan] IS NOT NULL AND [GiaBan] > 0)`.
- CHECK LuotXem: `[LuotXem] >= 0`.
- Index catalog: `(NgayXoa, TrangThai, LoaiTin, NgayDang)` và `(NguoiDangId, NgayXoa, TrangThai)`.
- Vòng đời/xóa mềm/rowversion trong service: [CATALOG.md](../CATALOG.md).

### NhuCauTraoDoi

| Cột | SQL Server | NULL | Ghi chú |
| --- | --- | --- | --- |
| TinDangId | uniqueidentifier | Không | FK: TinDang |
| MoTaMongMuon | nvarchar(2000) | Không | — |
| DanhMucMongMuonId | uniqueidentifier | Có | FK: DanhMucMongMuon |
| ChoPhepBuTien | bit | Không | — |
| SoTienBuToiDa | bigint | Không | — |
| ChoPhepSmartSwap | bit | Không | — |

- FK TinDang: `(TinDangId)` → `TinDang(Id)`, NO ACTION.
- FK DanhMucMongMuon: `(DanhMucMongMuonId)` → `DanhMuc(Id)`, NO ACTION.
- Unique index: `(TinDangId)`.
- CHECK TienBu: `[SoTienBuToiDa] >= 0 AND ([ChoPhepBuTien] = 1 OR [SoTienBuToiDa] = 0)`.

### GiaoDich

| Cột | SQL Server | NULL | Ghi chú |
| --- | --- | --- | --- |
| Ma | nvarchar(30) | Không | — |
| Loai | varchar(40) | Không | Enum LoaiGiaoDich |
| TrangThai | varchar(40) | Không | Enum TrangThaiGiaoDich |
| NgayHoanTat | datetimeoffset(0) | Có | — |

- Unique index: `(Ma)`.
- CHECK NgayHoanTat: `([TrangThai] = N'HoanTat' AND [NgayHoanTat] IS NOT NULL) OR ([TrangThai] <> N'HoanTat' AND [NgayHoanTat] IS NULL)`.

### ThanhVienGiaoDich

| Cột | SQL Server | NULL | Ghi chú |
| --- | --- | --- | --- |
| GiaoDichId | uniqueidentifier | Không | FK: GiaoDich |
| NguoiDungId | uniqueidentifier | Không | FK: NguoiDung |
| VaiTroHienThi | nvarchar(40) | Không | — |

- FK GiaoDich: `(GiaoDichId)` → `GiaoDich(Id)`, NO ACTION.
- FK NguoiDung: `(NguoiDungId)` → `NguoiDung(Id)`, NO ACTION.
- Alternate key: `(GiaoDichId, NguoiDungId)`.

### GiuChoSanPham

| Cột | SQL Server | NULL | Ghi chú |
| --- | --- | --- | --- |
| SanPhamId | uniqueidentifier | Không | FK: SanPham |
| GiaoDichId | uniqueidentifier | Không | FK: GiaoDich |
| NgayHetHan | datetimeoffset(0) | Không | — |
| DaGiaiPhong | bit | Không | — |

- FK SanPham: `(SanPhamId)` → `SanPham(Id)`, NO ACTION.
- FK GiaoDich: `(GiaoDichId)` → `GiaoDich(Id)`, NO ACTION.
- Filtered unique index: `(SanPhamId)`, WHERE `[DaGiaiPhong] = 0`.
- CHECK ThoiHan: `[NgayHetHan] > [NgayTao]`.

### DonHang

| Cột | SQL Server | NULL | Ghi chú |
| --- | --- | --- | --- |
| GiaoDichId | uniqueidentifier | Không | FK: GiaoDich, NguoiMua, NguoiBan |
| NguoiMuaId | uniqueidentifier | Không | FK: NguoiMua |
| NguoiBanId | uniqueidentifier | Không | FK: NguoiBan |
| TenNguoiNhan | nvarchar(120) | Không | — |
| SoDienThoaiNhan | nvarchar(20) | Không | — |
| DiaChiNhan | nvarchar(500) | Không | — |
| TienHang | bigint | Không | — |
| PhiVanChuyen | bigint | Không | — |
| TongTien | bigint | Không | — |
| TrangThai | varchar(40) | Không | Enum TrangThaiDonHang |

- FK GiaoDich: `(GiaoDichId)` → `GiaoDich(Id)`, NO ACTION.
- FK NguoiMua: `(GiaoDichId, NguoiMuaId)` → `ThanhVienGiaoDich(GiaoDichId, NguoiDungId)`, NO ACTION.
- FK NguoiBan: `(GiaoDichId, NguoiBanId)` → `ThanhVienGiaoDich(GiaoDichId, NguoiDungId)`, NO ACTION.
- Unique index: `(GiaoDichId)`.
- CHECK KhacNguoi: `[NguoiMuaId] <> [NguoiBanId]`.
- CHECK TongTien: `[TienHang] > 0 AND [PhiVanChuyen] >= 0 AND [TongTien] = [TienHang] + [PhiVanChuyen]`.

### ChiTietDonHang

| Cột | SQL Server | NULL | Ghi chú |
| --- | --- | --- | --- |
| DonHangId | uniqueidentifier | Không | FK: DonHang |
| TinDangId | uniqueidentifier | Không | FK: TinDang |
| SanPhamId | uniqueidentifier | Không | FK: TinDang |
| TenSanPham | nvarchar(200) | Không | — |
| DonGia | bigint | Không | — |
| SoLuong | int | Không | — |

- FK DonHang: `(DonHangId)` → `DonHang(Id)`, NO ACTION.
- FK TinDang: `(TinDangId, SanPhamId)` → `TinDang(Id, SanPhamId)`, NO ACTION.
- Unique index: `(DonHangId, SanPhamId)`.
- CHECK GiaSoLuong: `[DonGia] > 0 AND [SoLuong] = 1`.

### ThanhToan

| Cột | SQL Server | NULL | Ghi chú |
| --- | --- | --- | --- |
| DonHangId | uniqueidentifier | Không | FK: DonHang |
| SoTien | bigint | Không | — |
| TrangThai | varchar(40) | Không | Enum TrangThaiThanhToan |
| LaMoPhong | bit | Không | — |
| MaThamChieu | nvarchar(100) | Có | — |

- FK DonHang: `(DonHangId)` → `DonHang(Id)`, NO ACTION.
- Unique index: `(MaThamChieu)`.
- CHECK SoTien: `[SoTien] > 0`.

### DeNghiTraoDoi

| Cột | SQL Server | NULL | Ghi chú |
| --- | --- | --- | --- |
| GiaoDichId | uniqueidentifier | Không | FK: GiaoDich, NguoiGui, NguoiNhan |
| NguoiGuiId | uniqueidentifier | Không | FK: NguoiGui, SanPhamGui |
| NguoiNhanId | uniqueidentifier | Không | FK: NguoiNhan, SanPhamNhan |
| SanPhamGuiId | uniqueidentifier | Không | FK: SanPhamGui |
| SanPhamNhanId | uniqueidentifier | Không | FK: SanPhamNhan |
| TrangThai | varchar(40) | Không | Enum TrangThaiDeNghi |
| NgayHetHan | datetimeoffset(0) | Không | — |

- FK GiaoDich: `(GiaoDichId)` → `GiaoDich(Id)`, NO ACTION.
- FK NguoiGui: `(GiaoDichId, NguoiGuiId)` → `ThanhVienGiaoDich(GiaoDichId, NguoiDungId)`, NO ACTION.
- FK NguoiNhan: `(GiaoDichId, NguoiNhanId)` → `ThanhVienGiaoDich(GiaoDichId, NguoiDungId)`, NO ACTION.
- FK SanPhamGui: `(SanPhamGuiId, NguoiGuiId)` → `SanPham(Id, ChuSoHuuId)`, NO ACTION.
- FK SanPhamNhan: `(SanPhamNhanId, NguoiNhanId)` → `SanPham(Id, ChuSoHuuId)`, NO ACTION.
- Unique index: `(GiaoDichId)`.
- CHECK KhacNguoiVaMon: `[NguoiGuiId] <> [NguoiNhanId] AND [SanPhamGuiId] <> [SanPhamNhanId]`.
- CHECK ThoiHan: `[NgayHetHan] > [NgayTao]`.

### PhienBanThoaThuan

| Cột | SQL Server | NULL | Ghi chú |
| --- | --- | --- | --- |
| GiaoDichId | uniqueidentifier | Không | FK: GiaoDich, NguoiTao |
| SoPhienBan | int | Không | — |
| NguoiTaoId | uniqueidentifier | Không | FK: NguoiTao |
| NoiDung | nvarchar(2000) | Không | — |
| DiaDiemHen | nvarchar(500) | Có | — |
| ThoiGianHen | datetimeoffset(0) | Có | — |
| TrangThai | varchar(40) | Không | Enum TrangThaiThoaThuan |

- FK GiaoDich: `(GiaoDichId)` → `GiaoDich(Id)`, NO ACTION.
- FK NguoiTao: `(GiaoDichId, NguoiTaoId)` → `ThanhVienGiaoDich(GiaoDichId, NguoiDungId)`, NO ACTION.
- Alternate key: `(Id, GiaoDichId)`.
- Unique index: `(GiaoDichId, SoPhienBan)`.
- Filtered unique index: `(GiaoDichId)`, WHERE `[TrangThai] = 'DaChot'`.
- CHECK PhienBan: `[SoPhienBan] > 0`.

### DongYThoaThuan

| Cột | SQL Server | NULL | Ghi chú |
| --- | --- | --- | --- |
| PhienBanThoaThuanId | uniqueidentifier | Không | FK: PhienBanThoaThuan |
| GiaoDichId | uniqueidentifier | Không | FK: PhienBanThoaThuan, ThanhVien |
| NguoiDungId | uniqueidentifier | Không | FK: ThanhVien |
| QuyetDinh | varchar(40) | Không | Enum QuyetDinhDongY |
| NgayPhanHoi | datetimeoffset(0) | Có | — |

- FK PhienBanThoaThuan: `(PhienBanThoaThuanId, GiaoDichId)` → `PhienBanThoaThuan(Id, GiaoDichId)`, NO ACTION.
- FK ThanhVien: `(GiaoDichId, NguoiDungId)` → `ThanhVienGiaoDich(GiaoDichId, NguoiDungId)`, NO ACTION.
- Unique index: `(PhienBanThoaThuanId, NguoiDungId)`.
- CHECK NgayPhanHoi: `([QuyetDinh] = N'ChoPhanHoi' AND [NgayPhanHoi] IS NULL) OR ([QuyetDinh] <> N'ChoPhanHoi' AND [NgayPhanHoi] IS NOT NULL)`.

### TienBu

| Cột | SQL Server | NULL | Ghi chú |
| --- | --- | --- | --- |
| PhienBanThoaThuanId | uniqueidentifier | Không | FK: PhienBanThoaThuan |
| GiaoDichId | uniqueidentifier | Không | FK: PhienBanThoaThuan, NguoiTra, NguoiNhan |
| NguoiTraId | uniqueidentifier | Không | FK: NguoiTra |
| NguoiNhanId | uniqueidentifier | Không | FK: NguoiNhan |
| SoTien | bigint | Không | — |
| NguoiTraDaXacNhan | bit | Không | — |
| NguoiNhanDaXacNhan | bit | Không | — |

- FK PhienBanThoaThuan: `(PhienBanThoaThuanId, GiaoDichId)` → `PhienBanThoaThuan(Id, GiaoDichId)`, NO ACTION.
- FK NguoiTra: `(GiaoDichId, NguoiTraId)` → `ThanhVienGiaoDich(GiaoDichId, NguoiDungId)`, NO ACTION.
- FK NguoiNhan: `(GiaoDichId, NguoiNhanId)` → `ThanhVienGiaoDich(GiaoDichId, NguoiDungId)`, NO ACTION.
- Unique index: `(PhienBanThoaThuanId, NguoiTraId, NguoiNhanId)`.
- CHECK SoTienVaNguoi: `[SoTien] > 0 AND [NguoiTraId] <> [NguoiNhanId]`.

### VongSmartSwap

| Cột | SQL Server | NULL | Ghi chú |
| --- | --- | --- | --- |
| GiaoDichId | uniqueidentifier | Không | FK: GiaoDich |
| TrangThai | varchar(40) | Không | Enum TrangThaiVong |
| NgayHetHan | datetimeoffset(0) | Không | — |

- FK GiaoDich: `(GiaoDichId)` → `GiaoDich(Id)`, NO ACTION.
- Alternate key: `(Id, GiaoDichId)`.
- Unique index: `(GiaoDichId)`.
- CHECK ThoiHan: `[NgayHetHan] > [NgayTao]`.

### ThanhVienVong

| Cột | SQL Server | NULL | Ghi chú |
| --- | --- | --- | --- |
| VongSmartSwapId | uniqueidentifier | Không | FK: VongSmartSwap |
| GiaoDichId | uniqueidentifier | Không | FK: VongSmartSwap, ThanhVien |
| NguoiDungId | uniqueidentifier | Không | FK: ThanhVien, SanPham |
| SanPhamId | uniqueidentifier | Không | FK: SanPham |
| ViTri | int | Không | — |

- FK VongSmartSwap: `(VongSmartSwapId, GiaoDichId)` → `VongSmartSwap(Id, GiaoDichId)`, NO ACTION.
- FK ThanhVien: `(GiaoDichId, NguoiDungId)` → `ThanhVienGiaoDich(GiaoDichId, NguoiDungId)`, NO ACTION.
- FK SanPham: `(SanPhamId, NguoiDungId)` → `SanPham(Id, ChuSoHuuId)`, NO ACTION.
- Alternate key: `(Id, VongSmartSwapId)`.
- Alternate key: `(Id, VongSmartSwapId, SanPhamId)`.
- Unique index: `(VongSmartSwapId, NguoiDungId)`.
- Unique index: `(VongSmartSwapId, SanPhamId)`.
- Unique index: `(VongSmartSwapId, ViTri)`.
- CHECK ViTri: `[ViTri] BETWEEN 1 AND 3`.

### ChangTraoDoi

| Cột | SQL Server | NULL | Ghi chú |
| --- | --- | --- | --- |
| VongSmartSwapId | uniqueidentifier | Không | FK: VongSmartSwap, ThanhVienGiao, ThanhVienNhan |
| ThanhVienGiaoId | uniqueidentifier | Không | FK: ThanhVienGiao |
| ThanhVienNhanId | uniqueidentifier | Không | FK: ThanhVienNhan |
| SanPhamId | uniqueidentifier | Không | FK: ThanhVienGiao |

- FK VongSmartSwap: `(VongSmartSwapId)` → `VongSmartSwap(Id)`, NO ACTION.
- FK ThanhVienGiao: `(ThanhVienGiaoId, VongSmartSwapId, SanPhamId)` → `ThanhVienVong(Id, VongSmartSwapId, SanPhamId)`, NO ACTION.
- FK ThanhVienNhan: `(ThanhVienNhanId, VongSmartSwapId)` → `ThanhVienVong(Id, VongSmartSwapId)`, NO ACTION.
- Unique index: `(VongSmartSwapId, ThanhVienGiaoId)`.
- Unique index: `(VongSmartSwapId, ThanhVienNhanId)`.
- CHECK KhacThanhVien: `[ThanhVienGiaoId] <> [ThanhVienNhanId]`.

### CuocTroChuyen

| Cột | SQL Server | NULL | Ghi chú |
| --- | --- | --- | --- |
| GiaoDichId | uniqueidentifier | Có | FK: GiaoDich |
| TinDangId | uniqueidentifier | Có | FK: TinDang |
| Loai | varchar(40) | Không | Enum LoaiTroChuyen |
| TieuDe | nvarchar(200) | Không | — |

- FK GiaoDich: `(GiaoDichId)` → `GiaoDich(Id)`, NO ACTION.
- FK TinDang: `(TinDangId)` → `TinDang(Id)`, NO ACTION.
- Unique index: `(GiaoDichId)`.

### ThanhVienTroChuyen

| Cột | SQL Server | NULL | Ghi chú |
| --- | --- | --- | --- |
| CuocTroChuyenId | uniqueidentifier | Không | FK: CuocTroChuyen |
| NguoiDungId | uniqueidentifier | Không | FK: NguoiDung |
| LanDocCuoi | datetimeoffset(0) | Có | — |

- FK CuocTroChuyen: `(CuocTroChuyenId)` → `CuocTroChuyen(Id)`, NO ACTION.
- FK NguoiDung: `(NguoiDungId)` → `NguoiDung(Id)`, NO ACTION.
- Alternate key: `(CuocTroChuyenId, NguoiDungId)`.

### TinNhan

| Cột | SQL Server | NULL | Ghi chú |
| --- | --- | --- | --- |
| CuocTroChuyenId | uniqueidentifier | Không | FK: NguoiGui |
| NguoiGuiId | uniqueidentifier | Không | FK: NguoiGui |
| MaPhiaClient | uniqueidentifier | Không | — |
| Loai | varchar(40) | Không | Enum LoaiTinNhan |
| NoiDung | nvarchar(4000) | Có | — |

- FK NguoiGui: `(CuocTroChuyenId, NguoiGuiId)` → `ThanhVienTroChuyen(CuocTroChuyenId, NguoiDungId)`, NO ACTION.
- Unique index: `(CuocTroChuyenId, NguoiGuiId, MaPhiaClient)`.
- Index: `(CuocTroChuyenId, NgayTao)`.
- CHECK VanBan: `[Loai] <> N'VanBan' OR ([NoiDung] IS NOT NULL AND LEN(LTRIM(RTRIM([NoiDung]))) > 0)`.

### TepTinNhan

| Cột | SQL Server | NULL | Ghi chú |
| --- | --- | --- | --- |
| TinNhanId | uniqueidentifier | Không | FK: TinNhan |
| DuongDan | nvarchar(1000) | Không | — |
| TenTep | nvarchar(200) | Không | — |
| LoaiMime | nvarchar(100) | Không | — |
| DungLuong | bigint | Không | — |

- FK TinNhan: `(TinNhanId)` → `TinNhan(Id)`, NO ACTION.
- CHECK DungLuong: `[DungLuong] > 0`.

### ThongBao

| Cột | SQL Server | NULL | Ghi chú |
| --- | --- | --- | --- |
| NguoiDungId | uniqueidentifier | Không | FK: NguoiDung |
| GiaoDichId | uniqueidentifier | Có | FK: GiaoDich |
| Loai | varchar(40) | Không | Enum LoaiThongBao |
| TieuDe | nvarchar(200) | Không | — |
| NoiDung | nvarchar(2000) | Không | — |
| DaDoc | bit | Không | — |

- FK NguoiDung: `(NguoiDungId)` → `NguoiDung(Id)`, NO ACTION.
- FK GiaoDich: `(GiaoDichId)` → `GiaoDich(Id)`, NO ACTION.
- Index: `(NguoiDungId, DaDoc, NgayTao)`.

### VanChuyenMoPhong

| Cột | SQL Server | NULL | Ghi chú |
| --- | --- | --- | --- |
| DonHangId | uniqueidentifier | Không | FK: DonHang |
| MaVanDon | nvarchar(40) | Không | — |
| TrangThai | varchar(40) | Không | Enum TrangThaiVanChuyen |
| NgayGiaoDuKien | datetimeoffset(0) | Không | — |

- FK DonHang: `(DonHangId)` → `DonHang(Id)`, NO ACTION.
- Unique index: `(DonHangId)`.
- Unique index: `(MaVanDon)`.

### LichSuVanChuyen

| Cột | SQL Server | NULL | Ghi chú |
| --- | --- | --- | --- |
| VanChuyenMoPhongId | uniqueidentifier | Không | FK: VanChuyenMoPhong |
| TrangThai | varchar(40) | Không | Enum TrangThaiVanChuyen |
| MoTa | nvarchar(1000) | Không | — |
| ThoiDiem | datetimeoffset(0) | Không | — |

- FK VanChuyenMoPhong: `(VanChuyenMoPhongId)` → `VanChuyenMoPhong(Id)`, NO ACTION.
- Index: `(VanChuyenMoPhongId, ThoiDiem)`.

### XacNhanGiaoNhan

| Cột | SQL Server | NULL | Ghi chú |
| --- | --- | --- | --- |
| GiaoDichId | uniqueidentifier | Không | FK: GiaoDich, NguoiXacNhan, NguoiDoiTac, PhienBanThoaThuan |
| NguoiXacNhanId | uniqueidentifier | Không | FK: NguoiXacNhan |
| NguoiDoiTacId | uniqueidentifier | Không | FK: NguoiDoiTac |
| SanPhamId | uniqueidentifier | Không | FK: SanPham |
| PhienBanThoaThuanId | uniqueidentifier | Có | FK: PhienBanThoaThuan |
| Loai | varchar(40) | Không | Enum LoaiXacNhan |
| NgayXacNhan | datetimeoffset(0) | Không | — |

- FK GiaoDich: `(GiaoDichId)` → `GiaoDich(Id)`, NO ACTION.
- FK NguoiXacNhan: `(GiaoDichId, NguoiXacNhanId)` → `ThanhVienGiaoDich(GiaoDichId, NguoiDungId)`, NO ACTION.
- FK NguoiDoiTac: `(GiaoDichId, NguoiDoiTacId)` → `ThanhVienGiaoDich(GiaoDichId, NguoiDungId)`, NO ACTION.
- FK SanPham: `(SanPhamId)` → `SanPham(Id)`, NO ACTION.
- FK PhienBanThoaThuan: `(PhienBanThoaThuanId, GiaoDichId)` → `PhienBanThoaThuan(Id, GiaoDichId)`, NO ACTION.
- Unique index: `(GiaoDichId, NguoiXacNhanId, Loai)`.
- CHECK KhacNguoi: `[NguoiXacNhanId] <> [NguoiDoiTacId]`.

### BangChungGiaoNhan

| Cột | SQL Server | NULL | Ghi chú |
| --- | --- | --- | --- |
| XacNhanGiaoNhanId | uniqueidentifier | Không | FK: XacNhanGiaoNhan |
| NguoiTaiLenId | uniqueidentifier | Không | FK: NguoiTaiLen |
| DuongDan | nvarchar(1000) | Không | — |
| LoaiMime | nvarchar(100) | Không | — |

- FK XacNhanGiaoNhan: `(XacNhanGiaoNhanId)` → `XacNhanGiaoNhan(Id)`, NO ACTION.
- FK NguoiTaiLen: `(NguoiTaiLenId)` → `NguoiDung(Id)`, NO ACTION.

### DanhGia

| Cột | SQL Server | NULL | Ghi chú |
| --- | --- | --- | --- |
| GiaoDichId | uniqueidentifier | Không | FK: GiaoDich, NguoiDanhGia, NguoiDuocDanhGia |
| NguoiDanhGiaId | uniqueidentifier | Không | FK: NguoiDanhGia |
| NguoiDuocDanhGiaId | uniqueidentifier | Không | FK: NguoiDuocDanhGia |
| SoSao | int | Không | — |
| NoiDung | nvarchar(2000) | Không | — |

- FK GiaoDich: `(GiaoDichId)` → `GiaoDich(Id)`, NO ACTION.
- FK NguoiDanhGia: `(GiaoDichId, NguoiDanhGiaId)` → `ThanhVienGiaoDich(GiaoDichId, NguoiDungId)`, NO ACTION.
- FK NguoiDuocDanhGia: `(GiaoDichId, NguoiDuocDanhGiaId)` → `ThanhVienGiaoDich(GiaoDichId, NguoiDungId)`, NO ACTION.
- Unique index: `(GiaoDichId, NguoiDanhGiaId, NguoiDuocDanhGiaId)`.
- CHECK SoSaoVaNguoi: `[SoSao] BETWEEN 1 AND 5 AND [NguoiDanhGiaId] <> [NguoiDuocDanhGiaId]`.

### TieuChiDanhGia

| Cột | SQL Server | NULL | Ghi chú |
| --- | --- | --- | --- |
| Ma | nvarchar(40) | Không | — |
| Ten | nvarchar(150) | Không | — |

- Unique index: `(Ma)`.

### DanhGiaTieuChi

| Cột | SQL Server | NULL | Ghi chú |
| --- | --- | --- | --- |
| DanhGiaId | uniqueidentifier | Không | FK: DanhGia |
| TieuChiDanhGiaId | uniqueidentifier | Không | FK: TieuChiDanhGia |

- FK DanhGia: `(DanhGiaId)` → `DanhGia(Id)`, NO ACTION.
- FK TieuChiDanhGia: `(TieuChiDanhGiaId)` → `TieuChiDanhGia(Id)`, NO ACTION.
- Unique index: `(DanhGiaId, TieuChiDanhGiaId)`.

### KhieuNai

| Cột | SQL Server | NULL | Ghi chú |
| --- | --- | --- | --- |
| Ma | nvarchar(30) | Không | — |
| GiaoDichId | uniqueidentifier | Không | FK: GiaoDich, NguoiGui, NguoiBiKhieuNai |
| NguoiGuiId | uniqueidentifier | Không | FK: NguoiGui |
| NguoiBiKhieuNaiId | uniqueidentifier | Không | FK: NguoiBiKhieuNai |
| TieuDe | nvarchar(200) | Không | — |
| NoiDung | nvarchar(4000) | Không | — |
| TrangThai | varchar(40) | Không | Enum TrangThaiKhieuNai |
| HanPhanHoi | datetimeoffset(0) | Có | — |

- FK GiaoDich: `(GiaoDichId)` → `GiaoDich(Id)`, NO ACTION.
- FK NguoiGui: `(GiaoDichId, NguoiGuiId)` → `ThanhVienGiaoDich(GiaoDichId, NguoiDungId)`, NO ACTION.
- FK NguoiBiKhieuNai: `(GiaoDichId, NguoiBiKhieuNaiId)` → `ThanhVienGiaoDich(GiaoDichId, NguoiDungId)`, NO ACTION.
- Unique index: `(Ma)`.
- CHECK KhacNguoi: `[NguoiGuiId] <> [NguoiBiKhieuNaiId]`.

### BangChungKhieuNai

| Cột | SQL Server | NULL | Ghi chú |
| --- | --- | --- | --- |
| KhieuNaiId | uniqueidentifier | Không | FK: KhieuNai |
| NguoiTaiLenId | uniqueidentifier | Không | FK: NguoiTaiLen |
| DuongDan | nvarchar(1000) | Không | — |
| LoaiMime | nvarchar(100) | Không | — |
| MoTa | nvarchar(1000) | Không | — |

- FK KhieuNai: `(KhieuNaiId)` → `KhieuNai(Id)`, NO ACTION.
- FK NguoiTaiLen: `(NguoiTaiLenId)` → `NguoiDung(Id)`, NO ACTION.

### PhanHoiKhieuNai

| Cột | SQL Server | NULL | Ghi chú |
| --- | --- | --- | --- |
| KhieuNaiId | uniqueidentifier | Không | FK: KhieuNai |
| NguoiGuiId | uniqueidentifier | Không | FK: NguoiGui |
| NoiDung | nvarchar(2000) | Không | — |

- FK KhieuNai: `(KhieuNaiId)` → `KhieuNai(Id)`, NO ACTION.
- FK NguoiGui: `(NguoiGuiId)` → `NguoiDung(Id)`, NO ACTION.

### QuyetDinhXuLy

| Cột | SQL Server | NULL | Ghi chú |
| --- | --- | --- | --- |
| KhieuNaiId | uniqueidentifier | Không | FK: KhieuNai |
| QuanTriVienId | uniqueidentifier | Không | FK: QuanTriVien |
| KetLuan | nvarchar(2000) | Không | — |
| BienPhap | nvarchar(2000) | Không | — |
| DaXacMinhThucHien | bit | Không | — |
| MaYeuCau | uniqueidentifier | Có | Key retry cho quyết định quản trị |
| BamYeuCau | nvarchar(64) | Có | SHA256 nội dung và người thực hiện |
| CanCu | nvarchar(2000) | Không | Căn cứ đối chiếu |
| ThongBao | nvarchar(2000) | Không | Nội dung gửi các bên |
| HuongXuLy | nvarchar(20) | Không | TiepTuc / KetThuc / GhiNhan; mặc định GhiNhan cho dữ liệu cũ |

- FK KhieuNai: `(KhieuNaiId)` → `KhieuNai(Id)`, NO ACTION.
- FK QuanTriVien: `(QuanTriVienId)` → `NguoiDung(Id)`, NO ACTION.
- Unique filtered index: `(KhieuNaiId, MaYeuCau)` khi MaYeuCau khác NULL.

### YeuCauHoTro

| Cột | SQL Server | NULL | Ghi chú |
| --- | --- | --- | --- |
| Ma | nvarchar(30) | Không | — |
| NguoiGuiId | uniqueidentifier | Không | FK: NguoiGui |
| DonHangId | uniqueidentifier | Có | FK: DonHang |
| Loai | varchar(40) | Không | Enum LoaiYeuCauHoTro |
| TieuDe | nvarchar(200) | Không | — |
| NoiDung | nvarchar(4000) | Không | — |
| TrangThai | varchar(40) | Không | Enum TrangThaiHoTro |

- FK NguoiGui: `(NguoiGuiId)` → `NguoiDung(Id)`, NO ACTION.
- FK DonHang: `(DonHangId)` → `DonHang(Id)`, NO ACTION.
- Unique index: `(Ma)`.

### PhanHoiHoTro

| Cột | SQL Server | NULL | Ghi chú |
| --- | --- | --- | --- |
| YeuCauHoTroId | uniqueidentifier | Không | FK: YeuCauHoTro |
| NguoiGuiId | uniqueidentifier | Không | FK: NguoiGui |
| NoiDung | nvarchar(2000) | Không | — |

- FK YeuCauHoTro: `(YeuCauHoTroId)` → `YeuCauHoTro(Id)`, NO ACTION.
- FK NguoiGui: `(NguoiGuiId)` → `NguoiDung(Id)`, NO ACTION.

### NhatKyKiemDuyet

| Cột | SQL Server | NULL | Ghi chú |
| --- | --- | --- | --- |
| TinDangId | uniqueidentifier | Không | FK: TinDang |
| QuanTriVienId | uniqueidentifier | Không | FK: QuanTriVien |
| TrangThaiTruoc | varchar(40) | Không | Enum TrangThaiTinDang |
| TrangThaiSau | varchar(40) | Không | Enum TrangThaiTinDang |
| LyDo | nvarchar(1000) | Không | — |

- FK TinDang: `(TinDangId)` → `TinDang(Id)`, NO ACTION.
- FK QuanTriVien: `(QuanTriVienId)` → `NguoiDung(Id)`, NO ACTION.

### KhoaTaiKhoan

| Cột | SQL Server | NULL | Ghi chú |
| --- | --- | --- | --- |
| NguoiDungId | uniqueidentifier | Không | FK: NguoiDung |
| QuanTriVienId | uniqueidentifier | Không | FK: QuanTriVien |
| LyDo | nvarchar(1000) | Không | — |
| NgayBatDau | datetimeoffset(0) | Không | — |
| NgayKetThuc | datetimeoffset(0) | Có | — |
| NgayMoKhoa | datetimeoffset(0) | Có | — |

- FK NguoiDung: `(NguoiDungId)` → `NguoiDung(Id)`, NO ACTION.
- FK QuanTriVien: `(QuanTriVienId)` → `NguoiDung(Id)`, NO ACTION.
- CHECK ThoiHan: `([NgayKetThuc] IS NULL OR [NgayKetThuc] > [NgayBatDau]) AND ([NgayMoKhoa] IS NULL OR [NgayMoKhoa] >= [NgayBatDau])`.
- CHECK KhongTuKhoa: `[NguoiDungId] <> [QuanTriVienId]`.

### NhatKyQuanTri

| Cột | SQL Server | NULL | Ghi chú |
| --- | --- | --- | --- |
| NguoiThucHienId | uniqueidentifier | Không | FK: NguoiThucHien |
| HanhDong | nvarchar(100) | Không | — |
| LoaiDoiTuong | nvarchar(100) | Không | — |
| DoiTuongId | uniqueidentifier | Có | — |
| ChiTietJson | nvarchar(4000) | Không | — |

- FK NguoiThucHien: `(NguoiThucHienId)` → `NguoiDung(Id)`, NO ACTION.
- Index: `(LoaiDoiTuong, DoiTuongId, NgayTao)`.
- CHECK Json: `ISJSON([ChiTietJson]) = 1`.


### PhienDangNhap

Ngoài `Id`, `NgayTao`, `PhienBan` chung:

| Cột | SQL Server | NULL | Ý nghĩa |
| --- | --- | --- | --- |
| NguoiDungId | uniqueidentifier | Không | FK NguoiDung(Id), NO ACTION |
| MaTruyCap | varchar(32) | Không | jti của access token hiện hành |
| BamRefreshToken | varchar(64) | Không | SHA-256; unique, không lưu refresh token gốc |
| NgayHetHan | datetimeoffset(0) | Không | Thời hạn refresh |
| NgayThuHoi | datetimeoffset(0) | Có | Logout hoặc đổi/reset mật khẩu |

CHECK `NgayHetHan > NgayTao`; index `(NguoiDungId, NgayThuHoi)`. Refresh thay hash/jti với rowversion; JWT middleware đối chiếu phiên còn hiệu lực và role hiện hành trong SQL.

### YeuCauDatLaiMatKhau

Ngoài ba cột chung:

| Cột | SQL Server | NULL | Ý nghĩa |
| --- | --- | --- | --- |
| NguoiDungId | uniqueidentifier | Không | FK NguoiDung(Id), NO ACTION |
| BamToken | varchar(64) | Không | SHA-256; unique, không lưu mã gốc |
| NgayHetHan | datetimeoffset(0) | Không | 15 phút kể từ khi cấp |
| NgaySuDung | datetimeoffset(0) | Có | Mã đã dùng hoặc bị hủy |

CHECK `NgayHetHan > NgayTao`; index `(NguoiDungId, NgayHetHan)`. Reset mật khẩu dùng transaction, mã một lần, thu hồi mọi phiên và mọi mã còn lại. Migration `TaiKhoanJwtVaKhoiPhuc` chuyển mã vai trò cũ thành `USER`/`ADMIN` và bảo đảm tồn tại hai vai trò trên DB trống. Xem [tài khoản](../ACCOUNTS.md) về hashing và khôi phục.

## 7. Tập giá trị enum

| Enum | Giá trị |
| --- | --- |
| TrangThaiNguoiDung | HoatDong, DaKhoa, DaXoa |
| TinhTrangSanPham | NhuMoi, DaSuDungTot, CoLoiNhe |
| LoaiTinDang | MuaBan, TraoDoi, CaHai |
| TrangThaiTinDang | BanNhap, ChoDuyet, DangHienThi, TuChoi, DaAn, DaBan, DangGiuCho |
| LoaiGiaoDich | MuaBan, TraoDoiMotMot, SmartSwap |
| TrangThaiGiaoDich | KhoiTao, ChoDongY, DangThucHien, HoanTat, DaHuy, TranhChap |
| TrangThaiDonHang | ChoThanhToan, DaThanhToan, DangGiao, DaNhan, DaHuy |
| TrangThaiThanhToan | ChoXuLy, ThanhCong, ThatBai, HoanTien |
| TrangThaiDeNghi | ChoPhanHoi, DeXuatLai, DaChapNhan, TuChoi, DaHuy, HetHan |
| TrangThaiThoaThuan | DeXuat, DaChot, DaThayThe |
| QuyetDinhDongY | ChoPhanHoi, DongY, TuChoi |
| TrangThaiVong | ChoDongY, SanSangBanGiao, DangBanGiao, HoanTat, DaHuy |
| LoaiTroChuyen | TrucTiep, NhomSmartSwap |
| LoaiTinNhan | VanBan, HinhAnh, HeThong, DeNghiGia |
| LoaiThongBao | HeThong, TinDang, DonHang, TraoDoi, TinNhan, KhieuNai |
| TrangThaiVanChuyen | ChoLayHang, DaLayHang, DangVanChuyen, DaGiao, GiaoThatBai |
| LoaiXacNhan | DaGiao, DaNhan |
| TrangThaiKhieuNai | Moi, DangXuLy, ChoThucHien, DaGiaiQuyet, TuChoi |
| LoaiYeuCauHoTro | DonHangVanChuyen, ThanhToanHoanTien, TaiKhoanKhac |
| TrangThaiHoTro | Moi, DangXuLy, DaGiaiQuyet |

## 8. Bổ sung mua bán ngày 10/11/2026

Migration `20261110141612_MuaBanThuongLuongVaHoTro` và `20261110141855_ThuTuLichSuMoPhong`; model/configuration và từ điển JSON đã cập nhật. [Quy tắc/API mua bán](../COMMERCE.md) là nguồn đặc tả workflow hiện tại. Các cột mới nullable ở dữ liệu cũ giữ tương thích; không tự dựng phiên đăng nhập, phiếu hoặc lịch sử giả.

| Bảng | Cột bổ sung / ràng buộc |
| --- | --- |
| PhieuThuongLuong (mới) | `CuocTroChuyenId`, `TinNhanId`, `NguoiMuaId`, `NguoiBanId`, `GiaNiemYet bigint`, `SoTienGiam bigint`, `TrangThai varchar(40)`, `NgayHetHan datetimeoffset(0)`; thêm3 cột chung |
| LichSuHoTro (mới) | `YeuCauHoTroId`, `NguoiThucHienId`, `TrangThai varchar(40)`, `NoiDung nvarchar(4000)`, `SoThuTu bigint IDENTITY`; thêm3 cột chung |
| CuocTroChuyen | `NguoiMuaId uniqueidentifier NULL` FK NguoiDung; unique filtered `(TinDangId,NguoiMuaId)` khi cả hai khácNULL; hội thoại nhóm cũ giữNULL |
| TinNhan | `SoThuTu bigint IDENTITY`, index hội thoại/thứ tự; alternate key `(Id,CuocTroChuyenId)` cho phiếu gắn đúng tin/hội thoại |
| ThanhVienTroChuyen | `SoThuTuDaDoc bigint NOT NULL DEFAULT0`, watermark tin chưa đọc |
| DonHang | `PhieuThuongLuongId Guid?`, `MaYeuCau Guid?`, `BamYeuCau varchar(64)?`, `HanThanhToan`, `NgayHuy`, `LyDoHuy nvarchar(500)?`; unique phiếu khácNULL, unique người mua/key khácNULL, index trạng thái/hạn và người bán/thời gian |
| ChiTietDonHang | `GiaNiemYet bigint?`, `SoTienGiam bigint DEFAULT0`; CHECK tiền giảm không âm và giá snapshot bằng giá gốc trừ giảm khi có giá gốc |
| ThanhToan | `MaYeuCauCuoi Guid?`, `KetQuaCuoi bit?`, `NgayThanhToan`, `NgayGiaiNgan`, `NgayHoanTien` (datetimeoffset(0) nullable); `LaMoPhong` giữ nguyên |
| LichSuVanChuyen | `SoThuTu bigint IDENTITY`, sắp sự kiện cùng giây theo thứ tự ghi |
| DanhGia | `SoSaoSanPham int?` CHECK NULL hoặc1–5, `MaYeuCau Guid?`; `SoSao` cũ là điểm người bán |
| YeuCauHoTro | `MaYeuCau Guid?`, unique filtered người gửi/key khácNULL |
| TinDang | Enum/CHECK thêm `DangGiuCho`; không thay mã trạng thái cũ |

Phiếu có FK hội thoại, FK composite tin nhắn–hội thoại và hai FK composite participant của chat. CHECK người mua khác người bán, `GiaNiemYet>0`, `0<SoTienGiam<GiaNiemYet`, hạn sau ngày tạo và tập trạng thái cho phép. Unique filtered hội thoại với `TrangThai='DaChapNhan'` bảo vệ một phiếu đã chấp nhận. Lịch sử hỗ trợ có hai FK tới ticket/người thực hiện và CHECK trạng thái. Tất cả FK NO ACTION; thêm8FK và7CHECK so với giai đoạn catalog.

Từ điển được xuất từ EF design-time model, không còn duy trì cột thủ công:

```powershell
cd Backend
. .\scripts\Use-DotNet8.ps1
dotnet run --project tests/ReMarket.DatabaseChecks -- --export-dictionary ../docs/database/schema-dictionary.json
```

## 9. Bổ sung trao đổi 1–1 ngày 11/11/2026

Migration `20261111155701_TraoDoiMotMot`; [workflow/API](../EXCHANGE.md) và [từ điển EF mới nhất](schema-dictionary.json) mô tả đầy đủ. Không thêm bảng khác; tái sử dụng mô hình giao dịch/phiên bản/consent/tiền bù/receipt/review hiện có.

| Bảng | Bổ sung / quy tắc |
| --- | --- |
| NhuCauTraoDoi | `ChoPhepMotMot bit NOT NULL DEFAULT1`, `GhiChu nvarchar(1000) NULL`; CHECK ít nhất một trong MotMot/SmartSwap được bật |
| DeNghiTraoDoi | `TinGuiId`, `TinNhanId uniqueidentifier NULL`; FK composite về `(TinDang.Id,SanPhamId)` bảo vệ cặp tin–sản phẩm; CHECK hai tin cùng NULL (legacy) hoặc cùng có và khác nhau |
| DeNghiTraoDoi | `MaYeuCau uniqueidentifier NULL`, `BamYeuCau varchar(64) NULL`, `LyDoKetThuc nvarchar(500) NULL`; unique filtered `(NguoiGuiId,MaYeuCau)`, index trạng thái/hạn và người nhận/ngày |
| BangChungGiaoNhan | `MaYeuCau uniqueidentifier NULL`, `BamNoiDung varchar(64) NULL`; unique filtered `(XacNhanGiaoNhanId,MaYeuCau)` giúp retry upload không nhân đôi ảnh |
| TinDang | Enum/CHECK thêm `DaTraoDoi`; chỉ kết thúc sau hai lượt nhận đúng phiên bản đã chốt và đủ xác nhận tiền bù |
| TieuChiDanhGia | Migration thêm mã tham chiếu `DUNG_MO_TA`/`DUNG_HEN` nếu chưa có; seeder dùng bản ghi hiện hữu, không chèn trùng mã |

`TinNhanId` trong **DeNghiTraoDoi** là tin đăng nhận trao đổi, không phải khóa tới bảng TinNhan của chat. Các cột nullable giữ dữ liệu cũ và FK NO ACTION. Bảng giao dịch cũ đang tranh chấp được bảo toàn, worker chỉ xử lý giao dịch chờ đồng ý đã hết hạn. Thu hồi tin đã tham gia đề nghị không xóa nguồn lịch sử; sau hoàn tất ảnh riêng tư vẫn chỉ dành cho hai participant.

Mỗi người xác nhận giao món của mình và nhận món đối tác. Hai receipt đã nhận, đúng phiên bản hiện tại, kèm hai xác nhận trả/nhận tiền bù mới chuyển root `HoanTat`; một phía xác nhận giữ nguyên trạng thái đang thực hiện. Counter sau chấp nhận giữ hai món, đóng bản cũ và tạo consent mới; counter/hủy bị chặn khi đã bàn giao. Hoàn tất không đổi `SanPham.ChuSoHuuId` nguồn, hai tin chuyển `DaTraoDoi` và giải phóng giữ chỗ.

## 10. Tiêu chí tìm Smart Swap ngày12/11/2026

Migration `20261112061936_TieuChiTimSmartSwap` chỉ mở rộng bảng `NhuCauTraoDoi`. Bốn cột mới nullable để giữ tin cũ; tái sử dụng `DanhMucMongMuonId`/FK hiện có. Không seed vòng mới, không thêm bảng kết quả tìm kiếm. [Thuật toán/API](../SMART_SWAP_SEARCH.md).

| Cột | SQL / ý nghĩa |
| --- | --- |
| TuKhoaMongMuon | nvarchar(200) NULL; các từ khóa tích cực, `|` là OR giữa các phương án |
| TinhTrangToiThieu | nvarchar(40) NULL; enum CoLoiNhe/DaSuDungTot/NhuMoi, so thứ tự bằng quy tắc rõ ràng |
| GiaTriMongMuonTu | bigint NULL; giá trị tối thiểu của món muốn nhận |
| GiaTriMongMuonDen | bigint NULL; giá trị tối đa của món muốn nhận |

CHECK `CK_NhuCauTraoDoi_GiaMongMuon` giới hạn các đầu giá từ0 tới1.000.000.000.000 và từ≤đến nếu cả hai có giá trị. CHECK `CK_NhuCauTraoDoi_TinhTrang` giới hạn mã enum hoặc NULL. Không thêm/xóa FK, vẫn92 FK. API tạo/sửa tin nhận object `traoDoi.tieuChiSmartSwap`; omit/null giữ tiêu chí cũ, `{}` xóa các ràng buộc tùy chọn. Tất cả thay đổi vẫn nằm trong transaction và rowversion tin đăng hiện có.

API search chỉ đọc các bảng nguồn công khai/khả dụng; không INSERT vào `GiaoDich`, `VongSmartSwap`, `ThanhVienVong`, `ChangTraoDoi`, `TienBu` hoặc `GiuChoSanPham`. Kết quả là đề xuất có thể hết khả dụng ngay sau khi đọc; thao tác tạo vòng tương lai cần kiểm tra lại nguyên tử.

## 11. Lời mời và phương án Smart Swap ngày12/11/2026

Migration `20261112070453_LoiMoiVaThuongLuongSmartSwap`, [đặc tả nghiệp vụ/API](../SMART_SWAP_WORKFLOW.md). Tái sử dụng các bảng hiện có, vẫn47 entity.

| Bảng | Cột/ràng buộc mới |
| --- | --- |
| VongSmartSwap | `NguoiKhoiTaoId Guid?`; FK composite `(GiaoDichId,NguoiKhoiTaoId)` về participant |
| VongSmartSwap | `MaYeuCau Guid?`, `BamYeuCau varchar(64)?`, `MaDeXuat varchar(98)?`, `MaKetThuc varchar(40)?`, `LyDoKetThuc nvarchar(500)?`, `NgayChapNhan datetimeoffset(0)?` |
| VongSmartSwap | Unique actor/key khi khácNULL; unique MaDeXuat khi đang ChoDongY; index trạng thái/hạn; CHECK mã kết thúc chỉ TuChoi/HetHan/DaHuy và vòng phải DaHuy |
| ThanhVienVong | `TinDangId Guid?`; FK composite `(TinDangId,SanPhamId)` về `(TinDang.Id,SanPhamId)` |
| PhienBanThoaThuan | `MaYeuCau Guid?`, `BamYeuCau varchar(64)?`; unique filtered `(GiaoDichId,NguoiTaoId,MaYeuCau)` |
| TinNhan | `PhienBanThoaThuanId Guid?` FK về phiên bản; unique filtered để một bản có tối đa một tin cấu trúc |

Các cột nullable bảo toàn seed/hồ sơ cũ. Không tự gán creator hoặc đổi trạng thái vòng legacy; API vòng thiếu metadata mới chỉ đọc, worker bỏ qua. Tất cả3 FK mới NO ACTION; không cascade xóa lịch sử. Triggers3/3/6/6 và khóa nội dung bản đã chốt giữ nguyên. Tiền bù lưu từng người trả–người nhận dương, cân bằng theo các cặp nội bộ; cap/tổng trả/quyền đề xuất/đồng thuận do service kiểm tra dưới transaction.

Lời mời tạo root ChoDongY để gắn participant/chat/phiên bản; chỉ chuyển root DangThucHien khi đủ3/3 và giữ đủ3 món. Counter append bản mới và ba consent mới, không UPDATE số tiền bản cũ. Từ chối/hết hạn ghi MaKetThuc, nhả đúng hold đang thuộc giao dịch; không xóa phiên bản, consent hoặc chat.

## 12. Giao nhận Smart Swap ngày12/11/2026

Migration `20261112080514_GiaoNhanSmartSwap`. [Nghiệp vụ/API/demo](../SMART_SWAP_DELIVERY.md), [từ điển EF mới nhất](schema-dictionary.json).

| Bảng | Bổ sung |
| --- | --- |
| ThongTinGiaoNhanSmartSwap (mới) | Một dòng/ThanhVienVong; FK về thành viên và bản thỏa thuận, tên/điện thoại/địa chỉ gửi–nhận, thời điểm xác nhận, key/hash/rowversion |
| VanChuyenMoPhong | DonHangId nullable; ChangTraoDoiId/PhienBanThoaThuanId nullable FK; CHECK đúng một nguồn, unique filtered theo nguồn; MaVanDonTuNhap nullable unique; NgayNhanDungHang và6 trường contact snapshot |
| VanChuyenMoPhong/LichSuVanChuyen | Enum/CHECK thêm ChoGui, DaGui; mã/trạng thái mô phỏng được lưu SQL |
| KhieuNai | VanChuyenMoPhongId FK, LoaiSuCo SaiHang/ChuaNhanHang, key/hash; unique actor/key; CHECK cặp nguồn/loại sự cố |
| VongSmartSwap | Enum/CHECK thêm TamDung; root liên quan dùng TranhChap |

Năm trigger mới: `TR_SmartSwap_ThongTin`, `TR_SmartSwap_VanChuyen`, `TR_SmartSwap_Receipt`, `TR_SmartSwap_HoanTat`, `TR_SmartSwap_SuCo`. Chặn sai nguồn/vòng/bản, thay địa chỉ snapshot, sửa/xóa receipt trong protocol mới, nhận trước trạng thái giao/nhận sai chặng, hoàn tất chưa đủ3 hoặc có sự cố, báo sai nguồn/người nhận. Chỉ áp quy tắc vận đơn mới khi có nguồn chặng tương ứng; demo lịch sử không tự chuyển đổi.

Giữ guard6/6 của schema trước:3 sender xác nhận gửi +3 receiver xác nhận nhận đúng. Riêng tiến độ API đếm3 DaNhan; DaGiao vận chuyển không tăng tiến độ. Hoàn tất mới nhả3 hold và chuyển tin DaTraoDoi. Báo sự cố tạo KhieuNai và chuyển TamDung/TranhChap trong cùng transaction, không nhả hold. Toàn bộ5 FK mới NO ACTION; lịch sử không tự mất theo vòng hoặc tài khoản. Down migration có guard khi đã tồn tại vận đơn Smart Swap mới.
