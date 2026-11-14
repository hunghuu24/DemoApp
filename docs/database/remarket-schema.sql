-- Required for persisted computed columns and filtered indexes, including sqlcmd sessions.
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET ARITHABORT ON;
SET NUMERIC_ROUNDABORT OFF;
GO
IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE TABLE [dbo].[DanhMuc] (
        [Id] uniqueidentifier NOT NULL,
        [NgayTao] datetimeoffset(0) NOT NULL,
        [PhienBan] rowversion NOT NULL,
        [Ten] nvarchar(120) NOT NULL,
        [Ma] nvarchar(60) NOT NULL,
        [DanhMucChaId] uniqueidentifier NULL,
        [ThuTu] int NOT NULL,
        [DangHienThi] bit NOT NULL,
        CONSTRAINT [PK_DanhMuc] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_DanhMuc_KhongTuThamChieu] CHECK ([DanhMucChaId] IS NULL OR [DanhMucChaId] <> [Id]),
        CONSTRAINT [CK_DanhMuc_ThuTu] CHECK ([ThuTu] >= 0),
        CONSTRAINT [FK_DanhMuc_DanhMuc_DanhMucChaId] FOREIGN KEY ([DanhMucChaId]) REFERENCES [dbo].[DanhMuc] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE TABLE [dbo].[GiaoDich] (
        [Id] uniqueidentifier NOT NULL,
        [NgayTao] datetimeoffset(0) NOT NULL,
        [PhienBan] rowversion NOT NULL,
        [Ma] nvarchar(30) NOT NULL,
        [Loai] varchar(40) NOT NULL,
        [TrangThai] varchar(40) NOT NULL,
        [NgayHoanTat] datetimeoffset(0) NULL,
        CONSTRAINT [PK_GiaoDich] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_GiaoDich_Loai] CHECK ([Loai] IN (N'MuaBan', N'TraoDoiMotMot', N'SmartSwap')),
        CONSTRAINT [CK_GiaoDich_NgayHoanTat] CHECK (([TrangThai] = N'HoanTat' AND [NgayHoanTat] IS NOT NULL) OR ([TrangThai] <> N'HoanTat' AND [NgayHoanTat] IS NULL)),
        CONSTRAINT [CK_GiaoDich_TrangThai] CHECK ([TrangThai] IN (N'KhoiTao', N'ChoDongY', N'DangThucHien', N'HoanTat', N'DaHuy', N'TranhChap'))
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE TABLE [dbo].[NguoiDung] (
        [Id] uniqueidentifier NOT NULL,
        [NgayTao] datetimeoffset(0) NOT NULL,
        [PhienBan] rowversion NOT NULL,
        [Email] nvarchar(256) NOT NULL,
        [EmailChuanHoa] AS UPPER(LTRIM(RTRIM([Email]))) PERSISTED,
        [SoDienThoai] nvarchar(20) NULL,
        [MatKhauBam] nvarchar(512) NOT NULL,
        [TrangThai] varchar(40) NOT NULL,
        CONSTRAINT [PK_NguoiDung] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_NguoiDung_Email] CHECK (LEN(LTRIM(RTRIM([Email]))) > 3),
        CONSTRAINT [CK_NguoiDung_TrangThai] CHECK ([TrangThai] IN (N'HoatDong', N'DaKhoa', N'DaXoa'))
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE TABLE [dbo].[TieuChiDanhGia] (
        [Id] uniqueidentifier NOT NULL,
        [NgayTao] datetimeoffset(0) NOT NULL,
        [PhienBan] rowversion NOT NULL,
        [Ma] nvarchar(40) NOT NULL,
        [Ten] nvarchar(150) NOT NULL,
        CONSTRAINT [PK_TieuChiDanhGia] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE TABLE [dbo].[VaiTro] (
        [Id] uniqueidentifier NOT NULL,
        [NgayTao] datetimeoffset(0) NOT NULL,
        [PhienBan] rowversion NOT NULL,
        [Ma] nvarchar(40) NOT NULL,
        [Ten] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_VaiTro] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE TABLE [dbo].[VongSmartSwap] (
        [Id] uniqueidentifier NOT NULL,
        [NgayTao] datetimeoffset(0) NOT NULL,
        [PhienBan] rowversion NOT NULL,
        [GiaoDichId] uniqueidentifier NOT NULL,
        [TrangThai] varchar(40) NOT NULL,
        [NgayHetHan] datetimeoffset(0) NOT NULL,
        CONSTRAINT [PK_VongSmartSwap] PRIMARY KEY ([Id]),
        CONSTRAINT [AK_VongSmartSwap_Id_GiaoDichId] UNIQUE ([Id], [GiaoDichId]),
        CONSTRAINT [CK_VongSmartSwap_ThoiHan] CHECK ([NgayHetHan] > [NgayTao]),
        CONSTRAINT [CK_VongSmartSwap_TrangThai] CHECK ([TrangThai] IN (N'ChoDongY', N'SanSangBanGiao', N'DangBanGiao', N'HoanTat', N'DaHuy')),
        CONSTRAINT [FK_VongSmartSwap_GiaoDich_GiaoDichId] FOREIGN KEY ([GiaoDichId]) REFERENCES [dbo].[GiaoDich] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE TABLE [dbo].[HoSoCaNhan] (
        [Id] uniqueidentifier NOT NULL,
        [NgayTao] datetimeoffset(0) NOT NULL,
        [PhienBan] rowversion NOT NULL,
        [NguoiDungId] uniqueidentifier NOT NULL,
        [HoTen] nvarchar(120) NOT NULL,
        [AnhDaiDienUrl] nvarchar(1000) NULL,
        [DiaChi] nvarchar(500) NULL,
        [TinhThanh] nvarchar(100) NULL,
        [DaXacMinhEmail] bit NOT NULL,
        [DaXacMinhDienThoai] bit NOT NULL,
        CONSTRAINT [PK_HoSoCaNhan] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_HoSoCaNhan_NguoiDung_NguoiDungId] FOREIGN KEY ([NguoiDungId]) REFERENCES [dbo].[NguoiDung] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE TABLE [dbo].[KhoaTaiKhoan] (
        [Id] uniqueidentifier NOT NULL,
        [NgayTao] datetimeoffset(0) NOT NULL,
        [PhienBan] rowversion NOT NULL,
        [NguoiDungId] uniqueidentifier NOT NULL,
        [QuanTriVienId] uniqueidentifier NOT NULL,
        [LyDo] nvarchar(1000) NOT NULL,
        [NgayBatDau] datetimeoffset(0) NOT NULL,
        [NgayKetThuc] datetimeoffset(0) NULL,
        [NgayMoKhoa] datetimeoffset(0) NULL,
        CONSTRAINT [PK_KhoaTaiKhoan] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_KhoaTaiKhoan_KhongTuKhoa] CHECK ([NguoiDungId] <> [QuanTriVienId]),
        CONSTRAINT [CK_KhoaTaiKhoan_ThoiHan] CHECK (([NgayKetThuc] IS NULL OR [NgayKetThuc] > [NgayBatDau]) AND ([NgayMoKhoa] IS NULL OR [NgayMoKhoa] >= [NgayBatDau])),
        CONSTRAINT [FK_KhoaTaiKhoan_NguoiDung_NguoiDungId] FOREIGN KEY ([NguoiDungId]) REFERENCES [dbo].[NguoiDung] ([Id]),
        CONSTRAINT [FK_KhoaTaiKhoan_NguoiDung_QuanTriVienId] FOREIGN KEY ([QuanTriVienId]) REFERENCES [dbo].[NguoiDung] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE TABLE [dbo].[NhatKyQuanTri] (
        [Id] uniqueidentifier NOT NULL,
        [NgayTao] datetimeoffset(0) NOT NULL,
        [PhienBan] rowversion NOT NULL,
        [NguoiThucHienId] uniqueidentifier NOT NULL,
        [HanhDong] nvarchar(100) NOT NULL,
        [LoaiDoiTuong] nvarchar(100) NOT NULL,
        [DoiTuongId] uniqueidentifier NULL,
        [ChiTietJson] nvarchar(4000) NOT NULL,
        CONSTRAINT [PK_NhatKyQuanTri] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_NhatKyQuanTri_Json] CHECK (ISJSON([ChiTietJson]) = 1),
        CONSTRAINT [FK_NhatKyQuanTri_NguoiDung_NguoiThucHienId] FOREIGN KEY ([NguoiThucHienId]) REFERENCES [dbo].[NguoiDung] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE TABLE [dbo].[SanPham] (
        [Id] uniqueidentifier NOT NULL,
        [NgayTao] datetimeoffset(0) NOT NULL,
        [PhienBan] rowversion NOT NULL,
        [ChuSoHuuId] uniqueidentifier NOT NULL,
        [DanhMucId] uniqueidentifier NOT NULL,
        [Ten] nvarchar(200) NOT NULL,
        [MoTa] nvarchar(4000) NOT NULL,
        [TinhTrang] varchar(40) NOT NULL,
        [GiaTriThamKhao] bigint NOT NULL,
        CONSTRAINT [PK_SanPham] PRIMARY KEY ([Id]),
        CONSTRAINT [AK_SanPham_Id_ChuSoHuuId] UNIQUE ([Id], [ChuSoHuuId]),
        CONSTRAINT [CK_SanPham_GiaTri] CHECK ([GiaTriThamKhao] >= 0),
        CONSTRAINT [CK_SanPham_TinhTrang] CHECK ([TinhTrang] IN (N'NhuMoi', N'DaSuDungTot', N'CoLoiNhe')),
        CONSTRAINT [FK_SanPham_DanhMuc_DanhMucId] FOREIGN KEY ([DanhMucId]) REFERENCES [dbo].[DanhMuc] ([Id]),
        CONSTRAINT [FK_SanPham_NguoiDung_ChuSoHuuId] FOREIGN KEY ([ChuSoHuuId]) REFERENCES [dbo].[NguoiDung] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE TABLE [dbo].[ThanhVienGiaoDich] (
        [Id] uniqueidentifier NOT NULL,
        [NgayTao] datetimeoffset(0) NOT NULL,
        [PhienBan] rowversion NOT NULL,
        [GiaoDichId] uniqueidentifier NOT NULL,
        [NguoiDungId] uniqueidentifier NOT NULL,
        [VaiTroHienThi] nvarchar(40) NOT NULL,
        CONSTRAINT [PK_ThanhVienGiaoDich] PRIMARY KEY ([Id]),
        CONSTRAINT [AK_ThanhVienGiaoDich_GiaoDichId_NguoiDungId] UNIQUE ([GiaoDichId], [NguoiDungId]),
        CONSTRAINT [FK_ThanhVienGiaoDich_GiaoDich_GiaoDichId] FOREIGN KEY ([GiaoDichId]) REFERENCES [dbo].[GiaoDich] ([Id]),
        CONSTRAINT [FK_ThanhVienGiaoDich_NguoiDung_NguoiDungId] FOREIGN KEY ([NguoiDungId]) REFERENCES [dbo].[NguoiDung] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE TABLE [dbo].[ThongBao] (
        [Id] uniqueidentifier NOT NULL,
        [NgayTao] datetimeoffset(0) NOT NULL,
        [PhienBan] rowversion NOT NULL,
        [NguoiDungId] uniqueidentifier NOT NULL,
        [GiaoDichId] uniqueidentifier NULL,
        [Loai] varchar(40) NOT NULL,
        [TieuDe] nvarchar(200) NOT NULL,
        [NoiDung] nvarchar(2000) NOT NULL,
        [DaDoc] bit NOT NULL,
        CONSTRAINT [PK_ThongBao] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_ThongBao_Loai] CHECK ([Loai] IN (N'HeThong', N'TinDang', N'DonHang', N'TraoDoi', N'TinNhan', N'KhieuNai')),
        CONSTRAINT [FK_ThongBao_GiaoDich_GiaoDichId] FOREIGN KEY ([GiaoDichId]) REFERENCES [dbo].[GiaoDich] ([Id]),
        CONSTRAINT [FK_ThongBao_NguoiDung_NguoiDungId] FOREIGN KEY ([NguoiDungId]) REFERENCES [dbo].[NguoiDung] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE TABLE [dbo].[NguoiDungVaiTro] (
        [Id] uniqueidentifier NOT NULL,
        [NgayTao] datetimeoffset(0) NOT NULL,
        [PhienBan] rowversion NOT NULL,
        [NguoiDungId] uniqueidentifier NOT NULL,
        [VaiTroId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_NguoiDungVaiTro] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_NguoiDungVaiTro_NguoiDung_NguoiDungId] FOREIGN KEY ([NguoiDungId]) REFERENCES [dbo].[NguoiDung] ([Id]),
        CONSTRAINT [FK_NguoiDungVaiTro_VaiTro_VaiTroId] FOREIGN KEY ([VaiTroId]) REFERENCES [dbo].[VaiTro] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE TABLE [dbo].[AnhSanPham] (
        [Id] uniqueidentifier NOT NULL,
        [NgayTao] datetimeoffset(0) NOT NULL,
        [PhienBan] rowversion NOT NULL,
        [SanPhamId] uniqueidentifier NOT NULL,
        [DuongDan] nvarchar(1000) NOT NULL,
        [ThuTu] int NOT NULL,
        [LaAnhBia] bit NOT NULL,
        CONSTRAINT [PK_AnhSanPham] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_AnhSanPham_ThuTu] CHECK ([ThuTu] >= 0),
        CONSTRAINT [FK_AnhSanPham_SanPham_SanPhamId] FOREIGN KEY ([SanPhamId]) REFERENCES [dbo].[SanPham] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE TABLE [dbo].[GiuChoSanPham] (
        [Id] uniqueidentifier NOT NULL,
        [NgayTao] datetimeoffset(0) NOT NULL,
        [PhienBan] rowversion NOT NULL,
        [SanPhamId] uniqueidentifier NOT NULL,
        [GiaoDichId] uniqueidentifier NOT NULL,
        [NgayHetHan] datetimeoffset(0) NOT NULL,
        [DaGiaiPhong] bit NOT NULL,
        CONSTRAINT [PK_GiuChoSanPham] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_GiuChoSanPham_ThoiHan] CHECK ([NgayHetHan] > [NgayTao]),
        CONSTRAINT [FK_GiuChoSanPham_GiaoDich_GiaoDichId] FOREIGN KEY ([GiaoDichId]) REFERENCES [dbo].[GiaoDich] ([Id]),
        CONSTRAINT [FK_GiuChoSanPham_SanPham_SanPhamId] FOREIGN KEY ([SanPhamId]) REFERENCES [dbo].[SanPham] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE TABLE [dbo].[TinDang] (
        [Id] uniqueidentifier NOT NULL,
        [NgayTao] datetimeoffset(0) NOT NULL,
        [PhienBan] rowversion NOT NULL,
        [SanPhamId] uniqueidentifier NOT NULL,
        [NguoiDangId] uniqueidentifier NOT NULL,
        [LoaiTin] varchar(40) NOT NULL,
        [GiaBan] bigint NULL,
        [TrangThai] varchar(40) NOT NULL,
        [TinhThanh] nvarchar(100) NOT NULL,
        [DiaDiem] nvarchar(500) NOT NULL,
        [NgayDang] datetimeoffset(0) NULL,
        [NguoiDuyetId] uniqueidentifier NULL,
        [LyDoTuChoi] nvarchar(1000) NULL,
        CONSTRAINT [PK_TinDang] PRIMARY KEY ([Id]),
        CONSTRAINT [AK_TinDang_Id_SanPhamId] UNIQUE ([Id], [SanPhamId]),
        CONSTRAINT [CK_TinDang_GiaBan] CHECK (([LoaiTin] = N'TraoDoi' AND ([GiaBan] IS NULL OR [GiaBan] >= 0)) OR ([LoaiTin] IN (N'MuaBan', N'CaHai') AND [GiaBan] IS NOT NULL AND [GiaBan] > 0)),
        CONSTRAINT [CK_TinDang_LoaiTin] CHECK ([LoaiTin] IN (N'MuaBan', N'TraoDoi', N'CaHai')),
        CONSTRAINT [CK_TinDang_TrangThai] CHECK ([TrangThai] IN (N'BanNhap', N'ChoDuyet', N'DangHienThi', N'TuChoi', N'DaAn', N'DaBan')),
        CONSTRAINT [FK_TinDang_NguoiDung_NguoiDuyetId] FOREIGN KEY ([NguoiDuyetId]) REFERENCES [dbo].[NguoiDung] ([Id]),
        CONSTRAINT [FK_TinDang_SanPham_SanPhamId_NguoiDangId] FOREIGN KEY ([SanPhamId], [NguoiDangId]) REFERENCES [dbo].[SanPham] ([Id], [ChuSoHuuId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE TABLE [dbo].[DanhGia] (
        [Id] uniqueidentifier NOT NULL,
        [NgayTao] datetimeoffset(0) NOT NULL,
        [PhienBan] rowversion NOT NULL,
        [GiaoDichId] uniqueidentifier NOT NULL,
        [NguoiDanhGiaId] uniqueidentifier NOT NULL,
        [NguoiDuocDanhGiaId] uniqueidentifier NOT NULL,
        [SoSao] int NOT NULL,
        [NoiDung] nvarchar(2000) NOT NULL,
        CONSTRAINT [PK_DanhGia] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_DanhGia_SoSaoVaNguoi] CHECK ([SoSao] BETWEEN 1 AND 5 AND [NguoiDanhGiaId] <> [NguoiDuocDanhGiaId]),
        CONSTRAINT [FK_DanhGia_GiaoDich_GiaoDichId] FOREIGN KEY ([GiaoDichId]) REFERENCES [dbo].[GiaoDich] ([Id]),
        CONSTRAINT [FK_DanhGia_ThanhVienGiaoDich_GiaoDichId_NguoiDanhGiaId] FOREIGN KEY ([GiaoDichId], [NguoiDanhGiaId]) REFERENCES [dbo].[ThanhVienGiaoDich] ([GiaoDichId], [NguoiDungId]),
        CONSTRAINT [FK_DanhGia_ThanhVienGiaoDich_GiaoDichId_NguoiDuocDanhGiaId] FOREIGN KEY ([GiaoDichId], [NguoiDuocDanhGiaId]) REFERENCES [dbo].[ThanhVienGiaoDich] ([GiaoDichId], [NguoiDungId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE TABLE [dbo].[DeNghiTraoDoi] (
        [Id] uniqueidentifier NOT NULL,
        [NgayTao] datetimeoffset(0) NOT NULL,
        [PhienBan] rowversion NOT NULL,
        [GiaoDichId] uniqueidentifier NOT NULL,
        [NguoiGuiId] uniqueidentifier NOT NULL,
        [NguoiNhanId] uniqueidentifier NOT NULL,
        [SanPhamGuiId] uniqueidentifier NOT NULL,
        [SanPhamNhanId] uniqueidentifier NOT NULL,
        [TrangThai] varchar(40) NOT NULL,
        [NgayHetHan] datetimeoffset(0) NOT NULL,
        CONSTRAINT [PK_DeNghiTraoDoi] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_DeNghiTraoDoi_KhacNguoiVaMon] CHECK ([NguoiGuiId] <> [NguoiNhanId] AND [SanPhamGuiId] <> [SanPhamNhanId]),
        CONSTRAINT [CK_DeNghiTraoDoi_ThoiHan] CHECK ([NgayHetHan] > [NgayTao]),
        CONSTRAINT [CK_DeNghiTraoDoi_TrangThai] CHECK ([TrangThai] IN (N'ChoPhanHoi', N'DeXuatLai', N'DaChapNhan', N'TuChoi', N'DaHuy', N'HetHan')),
        CONSTRAINT [FK_DeNghiTraoDoi_GiaoDich_GiaoDichId] FOREIGN KEY ([GiaoDichId]) REFERENCES [dbo].[GiaoDich] ([Id]),
        CONSTRAINT [FK_DeNghiTraoDoi_SanPham_SanPhamGuiId_NguoiGuiId] FOREIGN KEY ([SanPhamGuiId], [NguoiGuiId]) REFERENCES [dbo].[SanPham] ([Id], [ChuSoHuuId]),
        CONSTRAINT [FK_DeNghiTraoDoi_SanPham_SanPhamNhanId_NguoiNhanId] FOREIGN KEY ([SanPhamNhanId], [NguoiNhanId]) REFERENCES [dbo].[SanPham] ([Id], [ChuSoHuuId]),
        CONSTRAINT [FK_DeNghiTraoDoi_ThanhVienGiaoDich_GiaoDichId_NguoiGuiId] FOREIGN KEY ([GiaoDichId], [NguoiGuiId]) REFERENCES [dbo].[ThanhVienGiaoDich] ([GiaoDichId], [NguoiDungId]),
        CONSTRAINT [FK_DeNghiTraoDoi_ThanhVienGiaoDich_GiaoDichId_NguoiNhanId] FOREIGN KEY ([GiaoDichId], [NguoiNhanId]) REFERENCES [dbo].[ThanhVienGiaoDich] ([GiaoDichId], [NguoiDungId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE TABLE [dbo].[DonHang] (
        [Id] uniqueidentifier NOT NULL,
        [NgayTao] datetimeoffset(0) NOT NULL,
        [PhienBan] rowversion NOT NULL,
        [GiaoDichId] uniqueidentifier NOT NULL,
        [NguoiMuaId] uniqueidentifier NOT NULL,
        [NguoiBanId] uniqueidentifier NOT NULL,
        [TenNguoiNhan] nvarchar(120) NOT NULL,
        [SoDienThoaiNhan] nvarchar(20) NOT NULL,
        [DiaChiNhan] nvarchar(500) NOT NULL,
        [TienHang] bigint NOT NULL,
        [PhiVanChuyen] bigint NOT NULL,
        [TongTien] bigint NOT NULL,
        [TrangThai] varchar(40) NOT NULL,
        CONSTRAINT [PK_DonHang] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_DonHang_KhacNguoi] CHECK ([NguoiMuaId] <> [NguoiBanId]),
        CONSTRAINT [CK_DonHang_TongTien] CHECK ([TienHang] > 0 AND [PhiVanChuyen] >= 0 AND [TongTien] = [TienHang] + [PhiVanChuyen]),
        CONSTRAINT [CK_DonHang_TrangThai] CHECK ([TrangThai] IN (N'ChoThanhToan', N'DaThanhToan', N'DangGiao', N'DaNhan', N'DaHuy')),
        CONSTRAINT [FK_DonHang_GiaoDich_GiaoDichId] FOREIGN KEY ([GiaoDichId]) REFERENCES [dbo].[GiaoDich] ([Id]),
        CONSTRAINT [FK_DonHang_ThanhVienGiaoDich_GiaoDichId_NguoiBanId] FOREIGN KEY ([GiaoDichId], [NguoiBanId]) REFERENCES [dbo].[ThanhVienGiaoDich] ([GiaoDichId], [NguoiDungId]),
        CONSTRAINT [FK_DonHang_ThanhVienGiaoDich_GiaoDichId_NguoiMuaId] FOREIGN KEY ([GiaoDichId], [NguoiMuaId]) REFERENCES [dbo].[ThanhVienGiaoDich] ([GiaoDichId], [NguoiDungId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE TABLE [dbo].[KhieuNai] (
        [Id] uniqueidentifier NOT NULL,
        [NgayTao] datetimeoffset(0) NOT NULL,
        [PhienBan] rowversion NOT NULL,
        [Ma] nvarchar(30) NOT NULL,
        [GiaoDichId] uniqueidentifier NOT NULL,
        [NguoiGuiId] uniqueidentifier NOT NULL,
        [NguoiBiKhieuNaiId] uniqueidentifier NOT NULL,
        [TieuDe] nvarchar(200) NOT NULL,
        [NoiDung] nvarchar(4000) NOT NULL,
        [TrangThai] varchar(40) NOT NULL,
        [HanPhanHoi] datetimeoffset(0) NULL,
        CONSTRAINT [PK_KhieuNai] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_KhieuNai_KhacNguoi] CHECK ([NguoiGuiId] <> [NguoiBiKhieuNaiId]),
        CONSTRAINT [CK_KhieuNai_TrangThai] CHECK ([TrangThai] IN (N'Moi', N'DangXuLy', N'ChoThucHien', N'DaGiaiQuyet', N'TuChoi')),
        CONSTRAINT [FK_KhieuNai_GiaoDich_GiaoDichId] FOREIGN KEY ([GiaoDichId]) REFERENCES [dbo].[GiaoDich] ([Id]),
        CONSTRAINT [FK_KhieuNai_ThanhVienGiaoDich_GiaoDichId_NguoiBiKhieuNaiId] FOREIGN KEY ([GiaoDichId], [NguoiBiKhieuNaiId]) REFERENCES [dbo].[ThanhVienGiaoDich] ([GiaoDichId], [NguoiDungId]),
        CONSTRAINT [FK_KhieuNai_ThanhVienGiaoDich_GiaoDichId_NguoiGuiId] FOREIGN KEY ([GiaoDichId], [NguoiGuiId]) REFERENCES [dbo].[ThanhVienGiaoDich] ([GiaoDichId], [NguoiDungId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE TABLE [dbo].[PhienBanThoaThuan] (
        [Id] uniqueidentifier NOT NULL,
        [NgayTao] datetimeoffset(0) NOT NULL,
        [PhienBan] rowversion NOT NULL,
        [GiaoDichId] uniqueidentifier NOT NULL,
        [SoPhienBan] int NOT NULL,
        [NguoiTaoId] uniqueidentifier NOT NULL,
        [NoiDung] nvarchar(2000) NOT NULL,
        [DiaDiemHen] nvarchar(500) NULL,
        [ThoiGianHen] datetimeoffset(0) NULL,
        [TrangThai] varchar(40) NOT NULL,
        CONSTRAINT [PK_PhienBanThoaThuan] PRIMARY KEY ([Id]),
        CONSTRAINT [AK_PhienBanThoaThuan_Id_GiaoDichId] UNIQUE ([Id], [GiaoDichId]),
        CONSTRAINT [CK_PhienBanThoaThuan_PhienBan] CHECK ([SoPhienBan] > 0),
        CONSTRAINT [CK_PhienBanThoaThuan_TrangThai] CHECK ([TrangThai] IN (N'DeXuat', N'DaChot', N'DaThayThe')),
        CONSTRAINT [FK_PhienBanThoaThuan_GiaoDich_GiaoDichId] FOREIGN KEY ([GiaoDichId]) REFERENCES [dbo].[GiaoDich] ([Id]),
        CONSTRAINT [FK_PhienBanThoaThuan_ThanhVienGiaoDich_GiaoDichId_NguoiTaoId] FOREIGN KEY ([GiaoDichId], [NguoiTaoId]) REFERENCES [dbo].[ThanhVienGiaoDich] ([GiaoDichId], [NguoiDungId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE TABLE [dbo].[ThanhVienVong] (
        [Id] uniqueidentifier NOT NULL,
        [NgayTao] datetimeoffset(0) NOT NULL,
        [PhienBan] rowversion NOT NULL,
        [VongSmartSwapId] uniqueidentifier NOT NULL,
        [GiaoDichId] uniqueidentifier NOT NULL,
        [NguoiDungId] uniqueidentifier NOT NULL,
        [SanPhamId] uniqueidentifier NOT NULL,
        [ViTri] int NOT NULL,
        CONSTRAINT [PK_ThanhVienVong] PRIMARY KEY ([Id]),
        CONSTRAINT [AK_ThanhVienVong_Id_VongSmartSwapId] UNIQUE ([Id], [VongSmartSwapId]),
        CONSTRAINT [AK_ThanhVienVong_Id_VongSmartSwapId_SanPhamId] UNIQUE ([Id], [VongSmartSwapId], [SanPhamId]),
        CONSTRAINT [CK_ThanhVienVong_ViTri] CHECK ([ViTri] BETWEEN 1 AND 3),
        CONSTRAINT [FK_ThanhVienVong_SanPham_SanPhamId_NguoiDungId] FOREIGN KEY ([SanPhamId], [NguoiDungId]) REFERENCES [dbo].[SanPham] ([Id], [ChuSoHuuId]),
        CONSTRAINT [FK_ThanhVienVong_ThanhVienGiaoDich_GiaoDichId_NguoiDungId] FOREIGN KEY ([GiaoDichId], [NguoiDungId]) REFERENCES [dbo].[ThanhVienGiaoDich] ([GiaoDichId], [NguoiDungId]),
        CONSTRAINT [FK_ThanhVienVong_VongSmartSwap_VongSmartSwapId_GiaoDichId] FOREIGN KEY ([VongSmartSwapId], [GiaoDichId]) REFERENCES [dbo].[VongSmartSwap] ([Id], [GiaoDichId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE TABLE [dbo].[CuocTroChuyen] (
        [Id] uniqueidentifier NOT NULL,
        [NgayTao] datetimeoffset(0) NOT NULL,
        [PhienBan] rowversion NOT NULL,
        [GiaoDichId] uniqueidentifier NULL,
        [TinDangId] uniqueidentifier NULL,
        [Loai] varchar(40) NOT NULL,
        [TieuDe] nvarchar(200) NOT NULL,
        CONSTRAINT [PK_CuocTroChuyen] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_CuocTroChuyen_Loai] CHECK ([Loai] IN (N'TrucTiep', N'NhomSmartSwap')),
        CONSTRAINT [FK_CuocTroChuyen_GiaoDich_GiaoDichId] FOREIGN KEY ([GiaoDichId]) REFERENCES [dbo].[GiaoDich] ([Id]),
        CONSTRAINT [FK_CuocTroChuyen_TinDang_TinDangId] FOREIGN KEY ([TinDangId]) REFERENCES [dbo].[TinDang] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE TABLE [dbo].[NhatKyKiemDuyet] (
        [Id] uniqueidentifier NOT NULL,
        [NgayTao] datetimeoffset(0) NOT NULL,
        [PhienBan] rowversion NOT NULL,
        [TinDangId] uniqueidentifier NOT NULL,
        [QuanTriVienId] uniqueidentifier NOT NULL,
        [TrangThaiTruoc] varchar(40) NOT NULL,
        [TrangThaiSau] varchar(40) NOT NULL,
        [LyDo] nvarchar(1000) NOT NULL,
        CONSTRAINT [PK_NhatKyKiemDuyet] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_NhatKyKiemDuyet_TrangThaiSau] CHECK ([TrangThaiSau] IN (N'BanNhap', N'ChoDuyet', N'DangHienThi', N'TuChoi', N'DaAn', N'DaBan')),
        CONSTRAINT [CK_NhatKyKiemDuyet_TrangThaiTruoc] CHECK ([TrangThaiTruoc] IN (N'BanNhap', N'ChoDuyet', N'DangHienThi', N'TuChoi', N'DaAn', N'DaBan')),
        CONSTRAINT [FK_NhatKyKiemDuyet_NguoiDung_QuanTriVienId] FOREIGN KEY ([QuanTriVienId]) REFERENCES [dbo].[NguoiDung] ([Id]),
        CONSTRAINT [FK_NhatKyKiemDuyet_TinDang_TinDangId] FOREIGN KEY ([TinDangId]) REFERENCES [dbo].[TinDang] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE TABLE [dbo].[NhuCauTraoDoi] (
        [Id] uniqueidentifier NOT NULL,
        [NgayTao] datetimeoffset(0) NOT NULL,
        [PhienBan] rowversion NOT NULL,
        [TinDangId] uniqueidentifier NOT NULL,
        [MoTaMongMuon] nvarchar(2000) NOT NULL,
        [DanhMucMongMuonId] uniqueidentifier NULL,
        [ChoPhepBuTien] bit NOT NULL,
        [SoTienBuToiDa] bigint NOT NULL,
        [ChoPhepSmartSwap] bit NOT NULL,
        CONSTRAINT [PK_NhuCauTraoDoi] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_NhuCauTraoDoi_TienBu] CHECK ([SoTienBuToiDa] >= 0 AND ([ChoPhepBuTien] = 1 OR [SoTienBuToiDa] = 0)),
        CONSTRAINT [FK_NhuCauTraoDoi_DanhMuc_DanhMucMongMuonId] FOREIGN KEY ([DanhMucMongMuonId]) REFERENCES [dbo].[DanhMuc] ([Id]),
        CONSTRAINT [FK_NhuCauTraoDoi_TinDang_TinDangId] FOREIGN KEY ([TinDangId]) REFERENCES [dbo].[TinDang] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE TABLE [dbo].[DanhGiaTieuChi] (
        [Id] uniqueidentifier NOT NULL,
        [NgayTao] datetimeoffset(0) NOT NULL,
        [PhienBan] rowversion NOT NULL,
        [DanhGiaId] uniqueidentifier NOT NULL,
        [TieuChiDanhGiaId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_DanhGiaTieuChi] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DanhGiaTieuChi_DanhGia_DanhGiaId] FOREIGN KEY ([DanhGiaId]) REFERENCES [dbo].[DanhGia] ([Id]),
        CONSTRAINT [FK_DanhGiaTieuChi_TieuChiDanhGia_TieuChiDanhGiaId] FOREIGN KEY ([TieuChiDanhGiaId]) REFERENCES [dbo].[TieuChiDanhGia] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE TABLE [dbo].[ChiTietDonHang] (
        [Id] uniqueidentifier NOT NULL,
        [NgayTao] datetimeoffset(0) NOT NULL,
        [PhienBan] rowversion NOT NULL,
        [DonHangId] uniqueidentifier NOT NULL,
        [TinDangId] uniqueidentifier NOT NULL,
        [SanPhamId] uniqueidentifier NOT NULL,
        [TenSanPham] nvarchar(200) NOT NULL,
        [DonGia] bigint NOT NULL,
        [SoLuong] int NOT NULL,
        CONSTRAINT [PK_ChiTietDonHang] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_ChiTietDonHang_GiaSoLuong] CHECK ([DonGia] > 0 AND [SoLuong] = 1),
        CONSTRAINT [FK_ChiTietDonHang_DonHang_DonHangId] FOREIGN KEY ([DonHangId]) REFERENCES [dbo].[DonHang] ([Id]),
        CONSTRAINT [FK_ChiTietDonHang_TinDang_TinDangId_SanPhamId] FOREIGN KEY ([TinDangId], [SanPhamId]) REFERENCES [dbo].[TinDang] ([Id], [SanPhamId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE TABLE [dbo].[ThanhToan] (
        [Id] uniqueidentifier NOT NULL,
        [NgayTao] datetimeoffset(0) NOT NULL,
        [PhienBan] rowversion NOT NULL,
        [DonHangId] uniqueidentifier NOT NULL,
        [SoTien] bigint NOT NULL,
        [TrangThai] varchar(40) NOT NULL,
        [LaMoPhong] bit NOT NULL,
        [MaThamChieu] nvarchar(100) NULL,
        CONSTRAINT [PK_ThanhToan] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_ThanhToan_SoTien] CHECK ([SoTien] > 0),
        CONSTRAINT [CK_ThanhToan_TrangThai] CHECK ([TrangThai] IN (N'ChoXuLy', N'ThanhCong', N'ThatBai', N'HoanTien')),
        CONSTRAINT [FK_ThanhToan_DonHang_DonHangId] FOREIGN KEY ([DonHangId]) REFERENCES [dbo].[DonHang] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE TABLE [dbo].[VanChuyenMoPhong] (
        [Id] uniqueidentifier NOT NULL,
        [NgayTao] datetimeoffset(0) NOT NULL,
        [PhienBan] rowversion NOT NULL,
        [DonHangId] uniqueidentifier NOT NULL,
        [MaVanDon] nvarchar(40) NOT NULL,
        [TrangThai] varchar(40) NOT NULL,
        [NgayGiaoDuKien] datetimeoffset(0) NOT NULL,
        CONSTRAINT [PK_VanChuyenMoPhong] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_VanChuyenMoPhong_TrangThai] CHECK ([TrangThai] IN (N'ChoLayHang', N'DaLayHang', N'DangVanChuyen', N'DaGiao', N'GiaoThatBai')),
        CONSTRAINT [FK_VanChuyenMoPhong_DonHang_DonHangId] FOREIGN KEY ([DonHangId]) REFERENCES [dbo].[DonHang] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE TABLE [dbo].[YeuCauHoTro] (
        [Id] uniqueidentifier NOT NULL,
        [NgayTao] datetimeoffset(0) NOT NULL,
        [PhienBan] rowversion NOT NULL,
        [Ma] nvarchar(30) NOT NULL,
        [NguoiGuiId] uniqueidentifier NOT NULL,
        [DonHangId] uniqueidentifier NULL,
        [Loai] varchar(40) NOT NULL,
        [TieuDe] nvarchar(200) NOT NULL,
        [NoiDung] nvarchar(4000) NOT NULL,
        [TrangThai] varchar(40) NOT NULL,
        CONSTRAINT [PK_YeuCauHoTro] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_YeuCauHoTro_Loai] CHECK ([Loai] IN (N'DonHangVanChuyen', N'ThanhToanHoanTien', N'TaiKhoanKhac')),
        CONSTRAINT [CK_YeuCauHoTro_TrangThai] CHECK ([TrangThai] IN (N'Moi', N'DangXuLy', N'DaGiaiQuyet')),
        CONSTRAINT [FK_YeuCauHoTro_DonHang_DonHangId] FOREIGN KEY ([DonHangId]) REFERENCES [dbo].[DonHang] ([Id]),
        CONSTRAINT [FK_YeuCauHoTro_NguoiDung_NguoiGuiId] FOREIGN KEY ([NguoiGuiId]) REFERENCES [dbo].[NguoiDung] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE TABLE [dbo].[BangChungKhieuNai] (
        [Id] uniqueidentifier NOT NULL,
        [NgayTao] datetimeoffset(0) NOT NULL,
        [PhienBan] rowversion NOT NULL,
        [KhieuNaiId] uniqueidentifier NOT NULL,
        [NguoiTaiLenId] uniqueidentifier NOT NULL,
        [DuongDan] nvarchar(1000) NOT NULL,
        [LoaiMime] nvarchar(100) NOT NULL,
        [MoTa] nvarchar(1000) NOT NULL,
        CONSTRAINT [PK_BangChungKhieuNai] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_BangChungKhieuNai_KhieuNai_KhieuNaiId] FOREIGN KEY ([KhieuNaiId]) REFERENCES [dbo].[KhieuNai] ([Id]),
        CONSTRAINT [FK_BangChungKhieuNai_NguoiDung_NguoiTaiLenId] FOREIGN KEY ([NguoiTaiLenId]) REFERENCES [dbo].[NguoiDung] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE TABLE [dbo].[PhanHoiKhieuNai] (
        [Id] uniqueidentifier NOT NULL,
        [NgayTao] datetimeoffset(0) NOT NULL,
        [PhienBan] rowversion NOT NULL,
        [KhieuNaiId] uniqueidentifier NOT NULL,
        [NguoiGuiId] uniqueidentifier NOT NULL,
        [NoiDung] nvarchar(2000) NOT NULL,
        CONSTRAINT [PK_PhanHoiKhieuNai] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PhanHoiKhieuNai_KhieuNai_KhieuNaiId] FOREIGN KEY ([KhieuNaiId]) REFERENCES [dbo].[KhieuNai] ([Id]),
        CONSTRAINT [FK_PhanHoiKhieuNai_NguoiDung_NguoiGuiId] FOREIGN KEY ([NguoiGuiId]) REFERENCES [dbo].[NguoiDung] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE TABLE [dbo].[QuyetDinhXuLy] (
        [Id] uniqueidentifier NOT NULL,
        [NgayTao] datetimeoffset(0) NOT NULL,
        [PhienBan] rowversion NOT NULL,
        [KhieuNaiId] uniqueidentifier NOT NULL,
        [QuanTriVienId] uniqueidentifier NOT NULL,
        [KetLuan] nvarchar(2000) NOT NULL,
        [BienPhap] nvarchar(2000) NOT NULL,
        [DaXacMinhThucHien] bit NOT NULL,
        CONSTRAINT [PK_QuyetDinhXuLy] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_QuyetDinhXuLy_KhieuNai_KhieuNaiId] FOREIGN KEY ([KhieuNaiId]) REFERENCES [dbo].[KhieuNai] ([Id]),
        CONSTRAINT [FK_QuyetDinhXuLy_NguoiDung_QuanTriVienId] FOREIGN KEY ([QuanTriVienId]) REFERENCES [dbo].[NguoiDung] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE TABLE [dbo].[DongYThoaThuan] (
        [Id] uniqueidentifier NOT NULL,
        [NgayTao] datetimeoffset(0) NOT NULL,
        [PhienBan] rowversion NOT NULL,
        [PhienBanThoaThuanId] uniqueidentifier NOT NULL,
        [GiaoDichId] uniqueidentifier NOT NULL,
        [NguoiDungId] uniqueidentifier NOT NULL,
        [QuyetDinh] varchar(40) NOT NULL,
        [NgayPhanHoi] datetimeoffset(0) NULL,
        CONSTRAINT [PK_DongYThoaThuan] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_DongYThoaThuan_NgayPhanHoi] CHECK (([QuyetDinh] = N'ChoPhanHoi' AND [NgayPhanHoi] IS NULL) OR ([QuyetDinh] <> N'ChoPhanHoi' AND [NgayPhanHoi] IS NOT NULL)),
        CONSTRAINT [CK_DongYThoaThuan_QuyetDinh] CHECK ([QuyetDinh] IN (N'ChoPhanHoi', N'DongY', N'TuChoi')),
        CONSTRAINT [FK_DongYThoaThuan_PhienBanThoaThuan_PhienBanThoaThuanId_GiaoDichId] FOREIGN KEY ([PhienBanThoaThuanId], [GiaoDichId]) REFERENCES [dbo].[PhienBanThoaThuan] ([Id], [GiaoDichId]),
        CONSTRAINT [FK_DongYThoaThuan_ThanhVienGiaoDich_GiaoDichId_NguoiDungId] FOREIGN KEY ([GiaoDichId], [NguoiDungId]) REFERENCES [dbo].[ThanhVienGiaoDich] ([GiaoDichId], [NguoiDungId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE TABLE [dbo].[TienBu] (
        [Id] uniqueidentifier NOT NULL,
        [NgayTao] datetimeoffset(0) NOT NULL,
        [PhienBan] rowversion NOT NULL,
        [PhienBanThoaThuanId] uniqueidentifier NOT NULL,
        [GiaoDichId] uniqueidentifier NOT NULL,
        [NguoiTraId] uniqueidentifier NOT NULL,
        [NguoiNhanId] uniqueidentifier NOT NULL,
        [SoTien] bigint NOT NULL,
        [NguoiTraDaXacNhan] bit NOT NULL,
        [NguoiNhanDaXacNhan] bit NOT NULL,
        CONSTRAINT [PK_TienBu] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_TienBu_SoTienVaNguoi] CHECK ([SoTien] > 0 AND [NguoiTraId] <> [NguoiNhanId]),
        CONSTRAINT [FK_TienBu_PhienBanThoaThuan_PhienBanThoaThuanId_GiaoDichId] FOREIGN KEY ([PhienBanThoaThuanId], [GiaoDichId]) REFERENCES [dbo].[PhienBanThoaThuan] ([Id], [GiaoDichId]),
        CONSTRAINT [FK_TienBu_ThanhVienGiaoDich_GiaoDichId_NguoiNhanId] FOREIGN KEY ([GiaoDichId], [NguoiNhanId]) REFERENCES [dbo].[ThanhVienGiaoDich] ([GiaoDichId], [NguoiDungId]),
        CONSTRAINT [FK_TienBu_ThanhVienGiaoDich_GiaoDichId_NguoiTraId] FOREIGN KEY ([GiaoDichId], [NguoiTraId]) REFERENCES [dbo].[ThanhVienGiaoDich] ([GiaoDichId], [NguoiDungId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE TABLE [dbo].[XacNhanGiaoNhan] (
        [Id] uniqueidentifier NOT NULL,
        [NgayTao] datetimeoffset(0) NOT NULL,
        [PhienBan] rowversion NOT NULL,
        [GiaoDichId] uniqueidentifier NOT NULL,
        [NguoiXacNhanId] uniqueidentifier NOT NULL,
        [NguoiDoiTacId] uniqueidentifier NOT NULL,
        [SanPhamId] uniqueidentifier NOT NULL,
        [PhienBanThoaThuanId] uniqueidentifier NULL,
        [Loai] varchar(40) NOT NULL,
        [NgayXacNhan] datetimeoffset(0) NOT NULL,
        CONSTRAINT [PK_XacNhanGiaoNhan] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_XacNhanGiaoNhan_KhacNguoi] CHECK ([NguoiXacNhanId] <> [NguoiDoiTacId]),
        CONSTRAINT [CK_XacNhanGiaoNhan_Loai] CHECK ([Loai] IN (N'DaGiao', N'DaNhan')),
        CONSTRAINT [FK_XacNhanGiaoNhan_GiaoDich_GiaoDichId] FOREIGN KEY ([GiaoDichId]) REFERENCES [dbo].[GiaoDich] ([Id]),
        CONSTRAINT [FK_XacNhanGiaoNhan_PhienBanThoaThuan_PhienBanThoaThuanId_GiaoDichId] FOREIGN KEY ([PhienBanThoaThuanId], [GiaoDichId]) REFERENCES [dbo].[PhienBanThoaThuan] ([Id], [GiaoDichId]),
        CONSTRAINT [FK_XacNhanGiaoNhan_SanPham_SanPhamId] FOREIGN KEY ([SanPhamId]) REFERENCES [dbo].[SanPham] ([Id]),
        CONSTRAINT [FK_XacNhanGiaoNhan_ThanhVienGiaoDich_GiaoDichId_NguoiDoiTacId] FOREIGN KEY ([GiaoDichId], [NguoiDoiTacId]) REFERENCES [dbo].[ThanhVienGiaoDich] ([GiaoDichId], [NguoiDungId]),
        CONSTRAINT [FK_XacNhanGiaoNhan_ThanhVienGiaoDich_GiaoDichId_NguoiXacNhanId] FOREIGN KEY ([GiaoDichId], [NguoiXacNhanId]) REFERENCES [dbo].[ThanhVienGiaoDich] ([GiaoDichId], [NguoiDungId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE TABLE [dbo].[ChangTraoDoi] (
        [Id] uniqueidentifier NOT NULL,
        [NgayTao] datetimeoffset(0) NOT NULL,
        [PhienBan] rowversion NOT NULL,
        [VongSmartSwapId] uniqueidentifier NOT NULL,
        [ThanhVienGiaoId] uniqueidentifier NOT NULL,
        [ThanhVienNhanId] uniqueidentifier NOT NULL,
        [SanPhamId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_ChangTraoDoi] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_ChangTraoDoi_KhacThanhVien] CHECK ([ThanhVienGiaoId] <> [ThanhVienNhanId]),
        CONSTRAINT [FK_ChangTraoDoi_ThanhVienVong_ThanhVienGiaoId_VongSmartSwapId_SanPhamId] FOREIGN KEY ([ThanhVienGiaoId], [VongSmartSwapId], [SanPhamId]) REFERENCES [dbo].[ThanhVienVong] ([Id], [VongSmartSwapId], [SanPhamId]),
        CONSTRAINT [FK_ChangTraoDoi_ThanhVienVong_ThanhVienNhanId_VongSmartSwapId] FOREIGN KEY ([ThanhVienNhanId], [VongSmartSwapId]) REFERENCES [dbo].[ThanhVienVong] ([Id], [VongSmartSwapId]),
        CONSTRAINT [FK_ChangTraoDoi_VongSmartSwap_VongSmartSwapId] FOREIGN KEY ([VongSmartSwapId]) REFERENCES [dbo].[VongSmartSwap] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE TABLE [dbo].[ThanhVienTroChuyen] (
        [Id] uniqueidentifier NOT NULL,
        [NgayTao] datetimeoffset(0) NOT NULL,
        [PhienBan] rowversion NOT NULL,
        [CuocTroChuyenId] uniqueidentifier NOT NULL,
        [NguoiDungId] uniqueidentifier NOT NULL,
        [LanDocCuoi] datetimeoffset(0) NULL,
        CONSTRAINT [PK_ThanhVienTroChuyen] PRIMARY KEY ([Id]),
        CONSTRAINT [AK_ThanhVienTroChuyen_CuocTroChuyenId_NguoiDungId] UNIQUE ([CuocTroChuyenId], [NguoiDungId]),
        CONSTRAINT [FK_ThanhVienTroChuyen_CuocTroChuyen_CuocTroChuyenId] FOREIGN KEY ([CuocTroChuyenId]) REFERENCES [dbo].[CuocTroChuyen] ([Id]),
        CONSTRAINT [FK_ThanhVienTroChuyen_NguoiDung_NguoiDungId] FOREIGN KEY ([NguoiDungId]) REFERENCES [dbo].[NguoiDung] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE TABLE [dbo].[LichSuVanChuyen] (
        [Id] uniqueidentifier NOT NULL,
        [NgayTao] datetimeoffset(0) NOT NULL,
        [PhienBan] rowversion NOT NULL,
        [VanChuyenMoPhongId] uniqueidentifier NOT NULL,
        [TrangThai] varchar(40) NOT NULL,
        [MoTa] nvarchar(1000) NOT NULL,
        [ThoiDiem] datetimeoffset(0) NOT NULL,
        CONSTRAINT [PK_LichSuVanChuyen] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_LichSuVanChuyen_TrangThai] CHECK ([TrangThai] IN (N'ChoLayHang', N'DaLayHang', N'DangVanChuyen', N'DaGiao', N'GiaoThatBai')),
        CONSTRAINT [FK_LichSuVanChuyen_VanChuyenMoPhong_VanChuyenMoPhongId] FOREIGN KEY ([VanChuyenMoPhongId]) REFERENCES [dbo].[VanChuyenMoPhong] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE TABLE [dbo].[PhanHoiHoTro] (
        [Id] uniqueidentifier NOT NULL,
        [NgayTao] datetimeoffset(0) NOT NULL,
        [PhienBan] rowversion NOT NULL,
        [YeuCauHoTroId] uniqueidentifier NOT NULL,
        [NguoiGuiId] uniqueidentifier NOT NULL,
        [NoiDung] nvarchar(2000) NOT NULL,
        CONSTRAINT [PK_PhanHoiHoTro] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PhanHoiHoTro_NguoiDung_NguoiGuiId] FOREIGN KEY ([NguoiGuiId]) REFERENCES [dbo].[NguoiDung] ([Id]),
        CONSTRAINT [FK_PhanHoiHoTro_YeuCauHoTro_YeuCauHoTroId] FOREIGN KEY ([YeuCauHoTroId]) REFERENCES [dbo].[YeuCauHoTro] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE TABLE [dbo].[BangChungGiaoNhan] (
        [Id] uniqueidentifier NOT NULL,
        [NgayTao] datetimeoffset(0) NOT NULL,
        [PhienBan] rowversion NOT NULL,
        [XacNhanGiaoNhanId] uniqueidentifier NOT NULL,
        [NguoiTaiLenId] uniqueidentifier NOT NULL,
        [DuongDan] nvarchar(1000) NOT NULL,
        [LoaiMime] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_BangChungGiaoNhan] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_BangChungGiaoNhan_NguoiDung_NguoiTaiLenId] FOREIGN KEY ([NguoiTaiLenId]) REFERENCES [dbo].[NguoiDung] ([Id]),
        CONSTRAINT [FK_BangChungGiaoNhan_XacNhanGiaoNhan_XacNhanGiaoNhanId] FOREIGN KEY ([XacNhanGiaoNhanId]) REFERENCES [dbo].[XacNhanGiaoNhan] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE TABLE [dbo].[TinNhan] (
        [Id] uniqueidentifier NOT NULL,
        [NgayTao] datetimeoffset(0) NOT NULL,
        [PhienBan] rowversion NOT NULL,
        [CuocTroChuyenId] uniqueidentifier NOT NULL,
        [NguoiGuiId] uniqueidentifier NOT NULL,
        [MaPhiaClient] uniqueidentifier NOT NULL,
        [Loai] varchar(40) NOT NULL,
        [NoiDung] nvarchar(4000) NULL,
        CONSTRAINT [PK_TinNhan] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_TinNhan_Loai] CHECK ([Loai] IN (N'VanBan', N'HinhAnh', N'HeThong', N'DeNghiGia')),
        CONSTRAINT [CK_TinNhan_VanBan] CHECK ([Loai] <> N'VanBan' OR ([NoiDung] IS NOT NULL AND LEN(LTRIM(RTRIM([NoiDung]))) > 0)),
        CONSTRAINT [FK_TinNhan_ThanhVienTroChuyen_CuocTroChuyenId_NguoiGuiId] FOREIGN KEY ([CuocTroChuyenId], [NguoiGuiId]) REFERENCES [dbo].[ThanhVienTroChuyen] ([CuocTroChuyenId], [NguoiDungId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE TABLE [dbo].[TepTinNhan] (
        [Id] uniqueidentifier NOT NULL,
        [NgayTao] datetimeoffset(0) NOT NULL,
        [PhienBan] rowversion NOT NULL,
        [TinNhanId] uniqueidentifier NOT NULL,
        [DuongDan] nvarchar(1000) NOT NULL,
        [TenTep] nvarchar(200) NOT NULL,
        [LoaiMime] nvarchar(100) NOT NULL,
        [DungLuong] bigint NOT NULL,
        CONSTRAINT [PK_TepTinNhan] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_TepTinNhan_DungLuong] CHECK ([DungLuong] > 0),
        CONSTRAINT [FK_TepTinNhan_TinNhan_TinNhanId] FOREIGN KEY ([TinNhanId]) REFERENCES [dbo].[TinNhan] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_AnhSanPham_SanPhamId] ON [dbo].[AnhSanPham] ([SanPhamId]) WHERE [LaAnhBia] = 1');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE UNIQUE INDEX [IX_AnhSanPham_SanPhamId_ThuTu] ON [dbo].[AnhSanPham] ([SanPhamId], [ThuTu]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE INDEX [IX_BangChungGiaoNhan_NguoiTaiLenId] ON [dbo].[BangChungGiaoNhan] ([NguoiTaiLenId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE INDEX [IX_BangChungGiaoNhan_XacNhanGiaoNhanId] ON [dbo].[BangChungGiaoNhan] ([XacNhanGiaoNhanId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE INDEX [IX_BangChungKhieuNai_KhieuNaiId] ON [dbo].[BangChungKhieuNai] ([KhieuNaiId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE INDEX [IX_BangChungKhieuNai_NguoiTaiLenId] ON [dbo].[BangChungKhieuNai] ([NguoiTaiLenId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE INDEX [IX_ChangTraoDoi_ThanhVienGiaoId_VongSmartSwapId_SanPhamId] ON [dbo].[ChangTraoDoi] ([ThanhVienGiaoId], [VongSmartSwapId], [SanPhamId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE INDEX [IX_ChangTraoDoi_ThanhVienNhanId_VongSmartSwapId] ON [dbo].[ChangTraoDoi] ([ThanhVienNhanId], [VongSmartSwapId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ChangTraoDoi_VongSmartSwapId_ThanhVienGiaoId] ON [dbo].[ChangTraoDoi] ([VongSmartSwapId], [ThanhVienGiaoId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ChangTraoDoi_VongSmartSwapId_ThanhVienNhanId] ON [dbo].[ChangTraoDoi] ([VongSmartSwapId], [ThanhVienNhanId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ChiTietDonHang_DonHangId_SanPhamId] ON [dbo].[ChiTietDonHang] ([DonHangId], [SanPhamId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE INDEX [IX_ChiTietDonHang_TinDangId_SanPhamId] ON [dbo].[ChiTietDonHang] ([TinDangId], [SanPhamId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_CuocTroChuyen_GiaoDichId] ON [dbo].[CuocTroChuyen] ([GiaoDichId]) WHERE [GiaoDichId] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE INDEX [IX_CuocTroChuyen_TinDangId] ON [dbo].[CuocTroChuyen] ([TinDangId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE UNIQUE INDEX [IX_DanhGia_GiaoDichId_NguoiDanhGiaId_NguoiDuocDanhGiaId] ON [dbo].[DanhGia] ([GiaoDichId], [NguoiDanhGiaId], [NguoiDuocDanhGiaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE INDEX [IX_DanhGia_GiaoDichId_NguoiDuocDanhGiaId] ON [dbo].[DanhGia] ([GiaoDichId], [NguoiDuocDanhGiaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE UNIQUE INDEX [IX_DanhGiaTieuChi_DanhGiaId_TieuChiDanhGiaId] ON [dbo].[DanhGiaTieuChi] ([DanhGiaId], [TieuChiDanhGiaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE INDEX [IX_DanhGiaTieuChi_TieuChiDanhGiaId] ON [dbo].[DanhGiaTieuChi] ([TieuChiDanhGiaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE INDEX [IX_DanhMuc_DanhMucChaId] ON [dbo].[DanhMuc] ([DanhMucChaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE UNIQUE INDEX [IX_DanhMuc_Ma] ON [dbo].[DanhMuc] ([Ma]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE UNIQUE INDEX [IX_DeNghiTraoDoi_GiaoDichId] ON [dbo].[DeNghiTraoDoi] ([GiaoDichId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE INDEX [IX_DeNghiTraoDoi_GiaoDichId_NguoiGuiId] ON [dbo].[DeNghiTraoDoi] ([GiaoDichId], [NguoiGuiId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE INDEX [IX_DeNghiTraoDoi_GiaoDichId_NguoiNhanId] ON [dbo].[DeNghiTraoDoi] ([GiaoDichId], [NguoiNhanId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE INDEX [IX_DeNghiTraoDoi_SanPhamGuiId_NguoiGuiId] ON [dbo].[DeNghiTraoDoi] ([SanPhamGuiId], [NguoiGuiId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE INDEX [IX_DeNghiTraoDoi_SanPhamNhanId_NguoiNhanId] ON [dbo].[DeNghiTraoDoi] ([SanPhamNhanId], [NguoiNhanId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE INDEX [IX_DongYThoaThuan_GiaoDichId_NguoiDungId] ON [dbo].[DongYThoaThuan] ([GiaoDichId], [NguoiDungId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE INDEX [IX_DongYThoaThuan_PhienBanThoaThuanId_GiaoDichId] ON [dbo].[DongYThoaThuan] ([PhienBanThoaThuanId], [GiaoDichId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE UNIQUE INDEX [IX_DongYThoaThuan_PhienBanThoaThuanId_NguoiDungId] ON [dbo].[DongYThoaThuan] ([PhienBanThoaThuanId], [NguoiDungId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE UNIQUE INDEX [IX_DonHang_GiaoDichId] ON [dbo].[DonHang] ([GiaoDichId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE INDEX [IX_DonHang_GiaoDichId_NguoiBanId] ON [dbo].[DonHang] ([GiaoDichId], [NguoiBanId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE INDEX [IX_DonHang_GiaoDichId_NguoiMuaId] ON [dbo].[DonHang] ([GiaoDichId], [NguoiMuaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE UNIQUE INDEX [IX_GiaoDich_Ma] ON [dbo].[GiaoDich] ([Ma]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE INDEX [IX_GiuChoSanPham_GiaoDichId] ON [dbo].[GiuChoSanPham] ([GiaoDichId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_GiuChoSanPham_SanPhamId] ON [dbo].[GiuChoSanPham] ([SanPhamId]) WHERE [DaGiaiPhong] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE UNIQUE INDEX [IX_HoSoCaNhan_NguoiDungId] ON [dbo].[HoSoCaNhan] ([NguoiDungId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE INDEX [IX_KhieuNai_GiaoDichId_NguoiBiKhieuNaiId] ON [dbo].[KhieuNai] ([GiaoDichId], [NguoiBiKhieuNaiId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE INDEX [IX_KhieuNai_GiaoDichId_NguoiGuiId] ON [dbo].[KhieuNai] ([GiaoDichId], [NguoiGuiId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE UNIQUE INDEX [IX_KhieuNai_Ma] ON [dbo].[KhieuNai] ([Ma]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE INDEX [IX_KhoaTaiKhoan_NguoiDungId] ON [dbo].[KhoaTaiKhoan] ([NguoiDungId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE INDEX [IX_KhoaTaiKhoan_QuanTriVienId] ON [dbo].[KhoaTaiKhoan] ([QuanTriVienId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE INDEX [IX_LichSuVanChuyen_VanChuyenMoPhongId_ThoiDiem] ON [dbo].[LichSuVanChuyen] ([VanChuyenMoPhongId], [ThoiDiem]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE UNIQUE INDEX [IX_NguoiDung_EmailChuanHoa] ON [dbo].[NguoiDung] ([EmailChuanHoa]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_NguoiDung_SoDienThoai] ON [dbo].[NguoiDung] ([SoDienThoai]) WHERE [SoDienThoai] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE UNIQUE INDEX [IX_NguoiDungVaiTro_NguoiDungId_VaiTroId] ON [dbo].[NguoiDungVaiTro] ([NguoiDungId], [VaiTroId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE INDEX [IX_NguoiDungVaiTro_VaiTroId] ON [dbo].[NguoiDungVaiTro] ([VaiTroId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE INDEX [IX_NhatKyKiemDuyet_QuanTriVienId] ON [dbo].[NhatKyKiemDuyet] ([QuanTriVienId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE INDEX [IX_NhatKyKiemDuyet_TinDangId] ON [dbo].[NhatKyKiemDuyet] ([TinDangId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE INDEX [IX_NhatKyQuanTri_LoaiDoiTuong_DoiTuongId_NgayTao] ON [dbo].[NhatKyQuanTri] ([LoaiDoiTuong], [DoiTuongId], [NgayTao]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE INDEX [IX_NhatKyQuanTri_NguoiThucHienId] ON [dbo].[NhatKyQuanTri] ([NguoiThucHienId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE INDEX [IX_NhuCauTraoDoi_DanhMucMongMuonId] ON [dbo].[NhuCauTraoDoi] ([DanhMucMongMuonId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE UNIQUE INDEX [IX_NhuCauTraoDoi_TinDangId] ON [dbo].[NhuCauTraoDoi] ([TinDangId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE INDEX [IX_PhanHoiHoTro_NguoiGuiId] ON [dbo].[PhanHoiHoTro] ([NguoiGuiId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE INDEX [IX_PhanHoiHoTro_YeuCauHoTroId] ON [dbo].[PhanHoiHoTro] ([YeuCauHoTroId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE INDEX [IX_PhanHoiKhieuNai_KhieuNaiId] ON [dbo].[PhanHoiKhieuNai] ([KhieuNaiId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE INDEX [IX_PhanHoiKhieuNai_NguoiGuiId] ON [dbo].[PhanHoiKhieuNai] ([NguoiGuiId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_PhienBanThoaThuan_GiaoDichId] ON [dbo].[PhienBanThoaThuan] ([GiaoDichId]) WHERE [TrangThai] = ''DaChot''');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE INDEX [IX_PhienBanThoaThuan_GiaoDichId_NguoiTaoId] ON [dbo].[PhienBanThoaThuan] ([GiaoDichId], [NguoiTaoId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PhienBanThoaThuan_GiaoDichId_SoPhienBan] ON [dbo].[PhienBanThoaThuan] ([GiaoDichId], [SoPhienBan]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE INDEX [IX_QuyetDinhXuLy_KhieuNaiId] ON [dbo].[QuyetDinhXuLy] ([KhieuNaiId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE INDEX [IX_QuyetDinhXuLy_QuanTriVienId] ON [dbo].[QuyetDinhXuLy] ([QuanTriVienId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE INDEX [IX_SanPham_ChuSoHuuId] ON [dbo].[SanPham] ([ChuSoHuuId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE INDEX [IX_SanPham_DanhMucId] ON [dbo].[SanPham] ([DanhMucId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE INDEX [IX_TepTinNhan_TinNhanId] ON [dbo].[TepTinNhan] ([TinNhanId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE INDEX [IX_ThanhToan_DonHangId] ON [dbo].[ThanhToan] ([DonHangId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_ThanhToan_MaThamChieu] ON [dbo].[ThanhToan] ([MaThamChieu]) WHERE [MaThamChieu] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE INDEX [IX_ThanhVienGiaoDich_NguoiDungId] ON [dbo].[ThanhVienGiaoDich] ([NguoiDungId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE INDEX [IX_ThanhVienTroChuyen_NguoiDungId] ON [dbo].[ThanhVienTroChuyen] ([NguoiDungId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE INDEX [IX_ThanhVienVong_GiaoDichId_NguoiDungId] ON [dbo].[ThanhVienVong] ([GiaoDichId], [NguoiDungId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE INDEX [IX_ThanhVienVong_SanPhamId_NguoiDungId] ON [dbo].[ThanhVienVong] ([SanPhamId], [NguoiDungId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE INDEX [IX_ThanhVienVong_VongSmartSwapId_GiaoDichId] ON [dbo].[ThanhVienVong] ([VongSmartSwapId], [GiaoDichId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ThanhVienVong_VongSmartSwapId_NguoiDungId] ON [dbo].[ThanhVienVong] ([VongSmartSwapId], [NguoiDungId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ThanhVienVong_VongSmartSwapId_SanPhamId] ON [dbo].[ThanhVienVong] ([VongSmartSwapId], [SanPhamId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ThanhVienVong_VongSmartSwapId_ViTri] ON [dbo].[ThanhVienVong] ([VongSmartSwapId], [ViTri]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE INDEX [IX_ThongBao_GiaoDichId] ON [dbo].[ThongBao] ([GiaoDichId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE INDEX [IX_ThongBao_NguoiDungId_DaDoc_NgayTao] ON [dbo].[ThongBao] ([NguoiDungId], [DaDoc], [NgayTao]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE INDEX [IX_TienBu_GiaoDichId_NguoiNhanId] ON [dbo].[TienBu] ([GiaoDichId], [NguoiNhanId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE INDEX [IX_TienBu_GiaoDichId_NguoiTraId] ON [dbo].[TienBu] ([GiaoDichId], [NguoiTraId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE INDEX [IX_TienBu_PhienBanThoaThuanId_GiaoDichId] ON [dbo].[TienBu] ([PhienBanThoaThuanId], [GiaoDichId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE UNIQUE INDEX [IX_TienBu_PhienBanThoaThuanId_NguoiTraId_NguoiNhanId] ON [dbo].[TienBu] ([PhienBanThoaThuanId], [NguoiTraId], [NguoiNhanId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE UNIQUE INDEX [IX_TieuChiDanhGia_Ma] ON [dbo].[TieuChiDanhGia] ([Ma]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE INDEX [IX_TinDang_NguoiDuyetId] ON [dbo].[TinDang] ([NguoiDuyetId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_TinDang_SanPhamId] ON [dbo].[TinDang] ([SanPhamId]) WHERE [TrangThai] IN (''ChoDuyet'', ''DangHienThi'')');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE INDEX [IX_TinDang_SanPhamId_NguoiDangId] ON [dbo].[TinDang] ([SanPhamId], [NguoiDangId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE INDEX [IX_TinNhan_CuocTroChuyenId_NgayTao] ON [dbo].[TinNhan] ([CuocTroChuyenId], [NgayTao]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE UNIQUE INDEX [IX_TinNhan_CuocTroChuyenId_NguoiGuiId_MaPhiaClient] ON [dbo].[TinNhan] ([CuocTroChuyenId], [NguoiGuiId], [MaPhiaClient]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE UNIQUE INDEX [IX_VaiTro_Ma] ON [dbo].[VaiTro] ([Ma]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE UNIQUE INDEX [IX_VanChuyenMoPhong_DonHangId] ON [dbo].[VanChuyenMoPhong] ([DonHangId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE UNIQUE INDEX [IX_VanChuyenMoPhong_MaVanDon] ON [dbo].[VanChuyenMoPhong] ([MaVanDon]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE UNIQUE INDEX [IX_VongSmartSwap_GiaoDichId] ON [dbo].[VongSmartSwap] ([GiaoDichId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE INDEX [IX_XacNhanGiaoNhan_GiaoDichId_NguoiDoiTacId] ON [dbo].[XacNhanGiaoNhan] ([GiaoDichId], [NguoiDoiTacId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE UNIQUE INDEX [IX_XacNhanGiaoNhan_GiaoDichId_NguoiXacNhanId_Loai] ON [dbo].[XacNhanGiaoNhan] ([GiaoDichId], [NguoiXacNhanId], [Loai]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE INDEX [IX_XacNhanGiaoNhan_PhienBanThoaThuanId_GiaoDichId] ON [dbo].[XacNhanGiaoNhan] ([PhienBanThoaThuanId], [GiaoDichId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE INDEX [IX_XacNhanGiaoNhan_SanPhamId] ON [dbo].[XacNhanGiaoNhan] ([SanPhamId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE INDEX [IX_YeuCauHoTro_DonHangId] ON [dbo].[YeuCauHoTro] ([DonHangId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE UNIQUE INDEX [IX_YeuCauHoTro_Ma] ON [dbo].[YeuCauHoTro] ([Ma]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    CREATE INDEX [IX_YeuCauHoTro_NguoiGuiId] ON [dbo].[YeuCauHoTro] ([NguoiGuiId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151401_KhoiTaoCoSoDuLieu'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261107151401_KhoiTaoCoSoDuLieu', N'8.0.33');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151408_ThemRangBuocNghiepVu'
)
BEGIN
    EXEC(N'CREATE OR ALTER TRIGGER [dbo].[TR_DonHang_LoaiGiaoDich] ON [dbo].[DonHang] AFTER INSERT, UPDATE
    AS
    BEGIN
        SET NOCOUNT ON;
        IF EXISTS (SELECT 1 FROM inserted i JOIN dbo.GiaoDich g ON g.Id=i.GiaoDichId WHERE g.Loai<>N''MuaBan'')
            THROW 51001, N''Loai giao dich khong phu hop voi bang chi tiet.'', 1;
    END;');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151408_ThemRangBuocNghiepVu'
)
BEGIN
    EXEC(N'CREATE OR ALTER TRIGGER [dbo].[TR_DeNghiTraoDoi_LoaiGiaoDich] ON [dbo].[DeNghiTraoDoi] AFTER INSERT, UPDATE
    AS
    BEGIN
        SET NOCOUNT ON;
        IF EXISTS (SELECT 1 FROM inserted i JOIN dbo.GiaoDich g ON g.Id=i.GiaoDichId WHERE g.Loai<>N''TraoDoiMotMot'')
            THROW 51001, N''Loai giao dich khong phu hop voi bang chi tiet.'', 1;
    END;');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151408_ThemRangBuocNghiepVu'
)
BEGIN
    EXEC(N'CREATE OR ALTER TRIGGER [dbo].[TR_VongSmartSwap_LoaiGiaoDich] ON [dbo].[VongSmartSwap] AFTER INSERT, UPDATE
    AS
    BEGIN
        SET NOCOUNT ON;
        IF EXISTS (SELECT 1 FROM inserted i JOIN dbo.GiaoDich g ON g.Id=i.GiaoDichId WHERE g.Loai<>N''SmartSwap'')
            THROW 51001, N''Loai giao dich khong phu hop voi bang chi tiet.'', 1;
    END;');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151408_ThemRangBuocNghiepVu'
)
BEGIN
    EXEC(N'CREATE OR ALTER TRIGGER [dbo].[TR_ThanhVienGiaoDich_SoLuong] ON [dbo].[ThanhVienGiaoDich] AFTER INSERT, UPDATE, DELETE
    AS
    BEGIN
        SET NOCOUNT ON;
        IF EXISTS (SELECT 1 FROM dbo.GiaoDich g WHERE g.Id IN (SELECT GiaoDichId FROM inserted UNION SELECT GiaoDichId FROM deleted) AND (SELECT COUNT(*) FROM dbo.ThanhVienGiaoDich t WHERE t.GiaoDichId=g.Id)>CASE WHEN g.Loai=N''SmartSwap'' THEN 3 ELSE 2 END)
            THROW 51001, N''Vuot so thanh vien cua giao dich.'', 1;
        IF EXISTS (SELECT 1 FROM dbo.GiaoDich g WHERE g.Id IN (SELECT GiaoDichId FROM inserted UNION SELECT GiaoDichId FROM deleted) AND g.TrangThai NOT IN (N''KhoiTao'',N''ChoDongY''))
            THROW 51001, N''Khong sua thanh vien khi giao dich da bat dau.'', 1;
    END;');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151408_ThemRangBuocNghiepVu'
)
BEGIN
    EXEC(N'CREATE OR ALTER TRIGGER [dbo].[TR_PhienBanThoaThuan_Chot] ON [dbo].[PhienBanThoaThuan] AFTER INSERT, UPDATE
    AS
    BEGIN
        SET NOCOUNT ON;
        IF EXISTS (SELECT 1 FROM inserted i JOIN deleted d ON d.Id=i.Id WHERE d.TrangThai=N''DaChot'' AND (i.NoiDung<>d.NoiDung OR i.NguoiTaoId<>d.NguoiTaoId OR i.SoPhienBan<>d.SoPhienBan OR i.GiaoDichId<>d.GiaoDichId OR ISNULL(i.DiaDiemHen,N'''')<>ISNULL(d.DiaDiemHen,N'''') OR ISNULL(i.ThoiGianHen,''19000101'')<>ISNULL(d.ThoiGianHen,''19000101'') OR i.TrangThai=N''DeXuat''))
            THROW 51001, N''Thoa thuan da chot phai duoc thay bang phien ban moi.'', 1;
        IF EXISTS (SELECT 1 FROM inserted i JOIN dbo.GiaoDich g ON g.Id=i.GiaoDichId WHERE i.TrangThai=N''DaChot'' AND (g.Loai=N''MuaBan'' OR (SELECT COUNT(*) FROM dbo.DongYThoaThuan d WHERE d.PhienBanThoaThuanId=i.Id AND d.QuyetDinh=N''DongY'')<>CASE WHEN g.Loai=N''SmartSwap'' THEN 3 ELSE 2 END))
            THROW 51001, N''Chua du dong y cho phien ban thoa thuan.'', 1;
    END;');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151408_ThemRangBuocNghiepVu'
)
BEGIN
    EXEC(N'CREATE OR ALTER TRIGGER [dbo].[TR_DongYThoaThuan_Khoa] ON [dbo].[DongYThoaThuan] AFTER INSERT, UPDATE, DELETE
    AS
    BEGIN
        SET NOCOUNT ON;
        IF EXISTS (SELECT 1 FROM dbo.PhienBanThoaThuan p WHERE p.Id IN (SELECT PhienBanThoaThuanId FROM inserted UNION SELECT PhienBanThoaThuanId FROM deleted) AND p.TrangThai=N''DaChot'')
            THROW 51001, N''Khong doi dong y tren thoa thuan da chot.'', 1;
    END;');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151408_ThemRangBuocNghiepVu'
)
BEGIN
    EXEC(N'CREATE OR ALTER TRIGGER [dbo].[TR_TienBu_KhoaNoiDung] ON [dbo].[TienBu] AFTER INSERT, UPDATE, DELETE
    AS
    BEGIN
        SET NOCOUNT ON;
        IF EXISTS (SELECT 1 FROM inserted i FULL JOIN deleted d ON d.Id=i.Id LEFT JOIN dbo.PhienBanThoaThuan pi ON pi.Id=i.PhienBanThoaThuanId LEFT JOIN dbo.PhienBanThoaThuan pd ON pd.Id=d.PhienBanThoaThuanId WHERE (pi.TrangThai=N''DaChot'' OR pd.TrangThai=N''DaChot'') AND (i.Id IS NULL OR d.Id IS NULL OR i.SoTien<>d.SoTien OR i.NguoiTraId<>d.NguoiTraId OR i.NguoiNhanId<>d.NguoiNhanId OR i.PhienBanThoaThuanId<>d.PhienBanThoaThuanId OR i.GiaoDichId<>d.GiaoDichId))
            THROW 51001, N''Khong doi noi dung tien bu da chot; tao phien ban moi.'', 1;
    END;');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151408_ThemRangBuocNghiepVu'
)
BEGIN
    EXEC(N'CREATE OR ALTER TRIGGER [dbo].[TR_VongSmartSwap_DieuKien] ON [dbo].[VongSmartSwap] AFTER INSERT, UPDATE
    AS
    BEGIN
        SET NOCOUNT ON;
        IF EXISTS (SELECT 1 FROM inserted i WHERE i.TrangThai IN (N''SanSangBanGiao'',N''DangBanGiao'',N''HoanTat'') AND ((SELECT COUNT(*) FROM dbo.ThanhVienVong t WHERE t.VongSmartSwapId=i.Id)<>3 OR (SELECT COUNT(*) FROM dbo.ChangTraoDoi c WHERE c.VongSmartSwapId=i.Id)<>3 OR NOT EXISTS (SELECT 1 FROM dbo.PhienBanThoaThuan p WHERE p.GiaoDichId=i.GiaoDichId AND p.TrangThai=N''DaChot'' AND (SELECT COUNT(*) FROM dbo.DongYThoaThuan d WHERE d.PhienBanThoaThuanId=p.Id AND d.QuyetDinh=N''DongY'')=3)))
            THROW 51003, N''Vong can 3 thanh vien, 3 chang va 3/3 dong y.'', 1;
        IF EXISTS (SELECT 1 FROM inserted i WHERE i.TrangThai=N''HoanTat'' AND (SELECT COUNT(*) FROM dbo.XacNhanGiaoNhan x JOIN dbo.PhienBanThoaThuan p ON p.Id=x.PhienBanThoaThuanId AND p.TrangThai=N''DaChot'' WHERE x.GiaoDichId=i.GiaoDichId)<>6)
            THROW 51006, N''Smart Swap chi hoan tat khi du 6/6 xac nhan.'', 1;
    END;');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151408_ThemRangBuocNghiepVu'
)
BEGIN
    EXEC(N'CREATE OR ALTER TRIGGER [dbo].[TR_ThanhVienVong_Khoa] ON [dbo].[ThanhVienVong] AFTER INSERT, UPDATE, DELETE
    AS
    BEGIN
        SET NOCOUNT ON;
        IF EXISTS (SELECT 1 FROM dbo.VongSmartSwap v WHERE v.Id IN (SELECT VongSmartSwapId FROM inserted UNION SELECT VongSmartSwapId FROM deleted) AND v.TrangThai NOT IN (N''ChoDongY'',N''DaHuy''))
            THROW 51001, N''Khong doi thanh vien/chang khi vong da san sang ban giao.'', 1;
    END;');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151408_ThemRangBuocNghiepVu'
)
BEGIN
    EXEC(N'CREATE OR ALTER TRIGGER [dbo].[TR_ChangTraoDoi_Khoa] ON [dbo].[ChangTraoDoi] AFTER INSERT, UPDATE, DELETE
    AS
    BEGIN
        SET NOCOUNT ON;
        IF EXISTS (SELECT 1 FROM dbo.VongSmartSwap v WHERE v.Id IN (SELECT VongSmartSwapId FROM inserted UNION SELECT VongSmartSwapId FROM deleted) AND v.TrangThai NOT IN (N''ChoDongY'',N''DaHuy''))
            THROW 51001, N''Khong doi thanh vien/chang khi vong da san sang ban giao.'', 1;
    END;');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151408_ThemRangBuocNghiepVu'
)
BEGIN
    EXEC(N'CREATE OR ALTER TRIGGER [dbo].[TR_XacNhanGiaoNhan_HopLe] ON [dbo].[XacNhanGiaoNhan] AFTER INSERT, UPDATE, DELETE
    AS
    BEGIN
        SET NOCOUNT ON;
        IF EXISTS (SELECT 1 FROM dbo.GiaoDich g WHERE g.Id IN (SELECT GiaoDichId FROM inserted UNION SELECT GiaoDichId FROM deleted) AND g.TrangThai=N''HoanTat'')
            THROW 51001, N''Khong doi xac nhan cua giao dich da hoan tat.'', 1;
        IF EXISTS (SELECT 1 FROM inserted i JOIN dbo.GiaoDich g ON g.Id=i.GiaoDichId WHERE (g.Loai=N''MuaBan'' AND NOT EXISTS (SELECT 1 FROM dbo.DonHang d JOIN dbo.ChiTietDonHang c ON c.DonHangId=d.Id WHERE d.GiaoDichId=i.GiaoDichId AND c.SanPhamId=i.SanPhamId AND i.PhienBanThoaThuanId IS NULL AND ((i.Loai=N''DaNhan'' AND i.NguoiXacNhanId=d.NguoiMuaId AND i.NguoiDoiTacId=d.NguoiBanId) OR (i.Loai=N''DaGiao'' AND i.NguoiXacNhanId=d.NguoiBanId AND i.NguoiDoiTacId=d.NguoiMuaId)))) OR (g.Loai=N''TraoDoiMotMot'' AND NOT EXISTS (SELECT 1 FROM dbo.DeNghiTraoDoi d JOIN dbo.PhienBanThoaThuan p ON p.Id=i.PhienBanThoaThuanId AND p.GiaoDichId=d.GiaoDichId AND p.TrangThai=N''DaChot'' WHERE d.GiaoDichId=i.GiaoDichId AND ((i.Loai=N''DaNhan'' AND ((i.NguoiXacNhanId=d.NguoiGuiId AND i.NguoiDoiTacId=d.NguoiNhanId AND i.SanPhamId=d.SanPhamNhanId) OR (i.NguoiXacNhanId=d.NguoiNhanId AND i.NguoiDoiTacId=d.NguoiGuiId AND i.SanPhamId=d.SanPhamGuiId))) OR (i.Loai=N''DaGiao'' AND ((i.NguoiXacNhanId=d.NguoiGuiId AND i.NguoiDoiTacId=d.NguoiNhanId AND i.SanPhamId=d.SanPhamGuiId) OR (i.NguoiXacNhanId=d.NguoiNhanId AND i.NguoiDoiTacId=d.NguoiGuiId AND i.SanPhamId=d.SanPhamNhanId)))))) OR (g.Loai=N''SmartSwap'' AND NOT EXISTS (SELECT 1 FROM dbo.VongSmartSwap v JOIN dbo.ChangTraoDoi c ON c.VongSmartSwapId=v.Id JOIN dbo.ThanhVienVong a ON a.Id=c.ThanhVienGiaoId JOIN dbo.ThanhVienVong b ON b.Id=c.ThanhVienNhanId JOIN dbo.PhienBanThoaThuan p ON p.Id=i.PhienBanThoaThuanId AND p.GiaoDichId=v.GiaoDichId AND p.TrangThai=N''DaChot'' WHERE v.GiaoDichId=i.GiaoDichId AND v.TrangThai IN (N''SanSangBanGiao'',N''DangBanGiao'') AND c.SanPhamId=i.SanPhamId AND ((i.Loai=N''DaGiao'' AND i.NguoiXacNhanId=a.NguoiDungId AND i.NguoiDoiTacId=b.NguoiDungId) OR (i.Loai=N''DaNhan'' AND i.NguoiXacNhanId=b.NguoiDungId AND i.NguoiDoiTacId=a.NguoiDungId)))))
            THROW 51001, N''Xac nhan sai san pham, doi tac hoac phien ban thoa thuan.'', 1;
    END;');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151408_ThemRangBuocNghiepVu'
)
BEGIN
    EXEC(N'CREATE OR ALTER TRIGGER [dbo].[TR_GiaoDich_HoanTat] ON [dbo].[GiaoDich] AFTER INSERT, UPDATE
    AS
    BEGIN
        SET NOCOUNT ON;
        IF EXISTS (SELECT 1 FROM inserted i WHERE (EXISTS(SELECT 1 FROM dbo.DonHang d WHERE d.GiaoDichId=i.Id) AND i.Loai<>N''MuaBan'') OR (EXISTS(SELECT 1 FROM dbo.DeNghiTraoDoi d WHERE d.GiaoDichId=i.Id) AND i.Loai<>N''TraoDoiMotMot'') OR (EXISTS(SELECT 1 FROM dbo.VongSmartSwap v WHERE v.GiaoDichId=i.Id) AND i.Loai<>N''SmartSwap''))
            THROW 51001, N''Khong doi loai giao dich da co chi tiet.'', 1;
        IF EXISTS (SELECT 1 FROM inserted i WHERE i.TrangThai IN (N''DangThucHien'',N''HoanTat'') AND (SELECT COUNT(*) FROM dbo.ThanhVienGiaoDich t WHERE t.GiaoDichId=i.Id)<>CASE WHEN i.Loai=N''SmartSwap'' THEN 3 ELSE 2 END)
            THROW 51001, N''So thanh vien chua du de bat dau.'', 1;
        IF EXISTS (SELECT 1 FROM inserted i WHERE i.Loai=N''SmartSwap'' AND i.TrangThai IN (N''DangThucHien'',N''HoanTat'') AND NOT EXISTS (SELECT 1 FROM dbo.VongSmartSwap v WHERE v.GiaoDichId=i.Id AND v.TrangThai IN (N''SanSangBanGiao'',N''DangBanGiao'',N''HoanTat'') AND (i.TrangThai<>N''HoanTat'' OR v.TrangThai=N''HoanTat'')))
            THROW 51001, N''Vong Smart Swap chua du dieu kien tuong ung.'', 1;
        IF EXISTS (SELECT 1 FROM inserted i WHERE i.Loai=N''TraoDoiMotMot'' AND i.TrangThai IN (N''DangThucHien'',N''HoanTat'') AND (NOT EXISTS (SELECT 1 FROM dbo.DeNghiTraoDoi d WHERE d.GiaoDichId=i.Id) OR NOT EXISTS (SELECT 1 FROM dbo.PhienBanThoaThuan p WHERE p.GiaoDichId=i.Id AND p.TrangThai=N''DaChot'') OR (i.TrangThai=N''HoanTat'' AND (SELECT COUNT(*) FROM dbo.XacNhanGiaoNhan x JOIN dbo.PhienBanThoaThuan p ON p.Id=x.PhienBanThoaThuanId AND p.TrangThai=N''DaChot'' WHERE x.GiaoDichId=i.Id AND x.Loai=N''DaNhan'')<>2)))
            THROW 51001, N''Trao doi 1-1 can thoa thuan va hai ben nhan du.'', 1;
        IF EXISTS (SELECT 1 FROM inserted i WHERE i.Loai=N''MuaBan'' AND i.TrangThai IN (N''DangThucHien'',N''HoanTat'') AND NOT EXISTS (SELECT 1 FROM dbo.DonHang d WHERE d.GiaoDichId=i.Id AND d.TienHang=(SELECT SUM(c.DonGia*c.SoLuong) FROM dbo.ChiTietDonHang c WHERE c.DonHangId=d.Id) AND (i.TrangThai<>N''HoanTat'' OR (d.TrangThai=N''DaNhan'' AND EXISTS(SELECT 1 FROM dbo.XacNhanGiaoNhan x WHERE x.GiaoDichId=i.Id AND x.Loai=N''DaNhan'')))))
            THROW 51001, N''Don hang chua du chi tiet hoac chua nhan hang.'', 1;
    END;');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151408_ThemRangBuocNghiepVu'
)
BEGIN
    EXEC(N'CREATE OR ALTER TRIGGER [dbo].[TR_DanhGia_DaHoanTat] ON [dbo].[DanhGia] AFTER INSERT, UPDATE
    AS
    BEGIN
        SET NOCOUNT ON;
        IF EXISTS (SELECT 1 FROM inserted i JOIN dbo.GiaoDich g ON g.Id=i.GiaoDichId WHERE g.TrangThai<>N''HoanTat'')
            THROW 51001, N''Chi danh gia giao dich da hoan tat.'', 1;
    END;');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151408_ThemRangBuocNghiepVu'
)
BEGIN
    EXEC(N'CREATE OR ALTER TRIGGER [dbo].[TR_BangChungGiaoNhan_GioiHan] ON [dbo].[BangChungGiaoNhan] AFTER INSERT, UPDATE
    AS
    BEGIN
        SET NOCOUNT ON;
        IF EXISTS (SELECT 1 FROM inserted i WHERE i.LoaiMime NOT IN (N''image/jpeg'',N''image/png''))
            THROW 51001, N''Bang chung giao nhan chi nhan JPG/PNG.'', 1;
        IF EXISTS (SELECT 1 FROM dbo.BangChungGiaoNhan b WHERE b.XacNhanGiaoNhanId IN (SELECT XacNhanGiaoNhanId FROM inserted) GROUP BY b.XacNhanGiaoNhanId HAVING COUNT(*)>5)
            THROW 51001, N''Toi da 5 anh bang chung cho mot xac nhan.'', 1;
    END;');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151408_ThemRangBuocNghiepVu'
)
BEGIN
    EXEC(N'CREATE OR ALTER TRIGGER [dbo].[TR_AnhSanPham_GioiHan] ON [dbo].[AnhSanPham] AFTER INSERT, UPDATE
    AS
    BEGIN
        SET NOCOUNT ON;
        IF EXISTS (SELECT 1 FROM dbo.SanPham p WHERE p.Id IN (SELECT SanPhamId FROM inserted) AND (SELECT COUNT(*) FROM dbo.AnhSanPham a WHERE a.SanPhamId=p.Id)>CASE WHEN EXISTS(SELECT 1 FROM dbo.TinDang t WHERE t.SanPhamId=p.Id AND t.LoaiTin IN (N''TraoDoi'',N''CaHai'')) THEN 6 ELSE 8 END)
            THROW 51001, N''Qua gioi han anh: 8 mua ban, 6 trao doi.'', 1;
    END;');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151408_ThemRangBuocNghiepVu'
)
BEGIN
    EXEC(N'CREATE OR ALTER TRIGGER [dbo].[TR_TinDang_GioiHanAnh] ON [dbo].[TinDang] AFTER INSERT, UPDATE
    AS
    BEGIN
        SET NOCOUNT ON;
        IF EXISTS (SELECT 1 FROM dbo.SanPham p WHERE p.Id IN (SELECT SanPhamId FROM inserted) AND (SELECT COUNT(*) FROM dbo.AnhSanPham a WHERE a.SanPhamId=p.Id)>CASE WHEN EXISTS(SELECT 1 FROM dbo.TinDang t WHERE t.SanPhamId=p.Id AND t.LoaiTin IN (N''TraoDoi'',N''CaHai'')) THEN 6 ELSE 8 END)
            THROW 51001, N''Qua gioi han anh theo loai tin.'', 1;
    END;');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151408_ThemRangBuocNghiepVu'
)
BEGIN
    EXEC(N'CREATE OR ALTER TRIGGER [dbo].[TR_KhieuNai_DongPhieu] ON [dbo].[KhieuNai] AFTER INSERT, UPDATE
    AS
    BEGIN
        SET NOCOUNT ON;
        IF EXISTS (SELECT 1 FROM inserted i WHERE i.TrangThai=N''DaGiaiQuyet'' AND NOT EXISTS(SELECT 1 FROM dbo.QuyetDinhXuLy q WHERE q.KhieuNaiId=i.Id AND q.DaXacMinhThucHien=1))
            THROW 51001, N''Chi dong khieu nai sau khi xac minh thuc hien.'', 1;
    END;');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107151408_ThemRangBuocNghiepVu'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261107151408_ThemRangBuocNghiepVu', N'8.0.33');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107155311_TaiKhoanJwtVaKhoiPhuc'
)
BEGIN
    UPDATE dbo.VaiTro SET Ma='USER' WHERE Ma=N'NguoiDung';
    UPDATE dbo.VaiTro SET Ma='ADMIN' WHERE Ma=N'QuanTriVien';
    IF NOT EXISTS (SELECT 1 FROM dbo.VaiTro WHERE Ma='USER')
        INSERT dbo.VaiTro (Id, NgayTao, Ma, Ten) VALUES (NEWID(), SYSUTCDATETIME(), 'USER', N'Nguoi dung');
    IF NOT EXISTS (SELECT 1 FROM dbo.VaiTro WHERE Ma='ADMIN')
        INSERT dbo.VaiTro (Id, NgayTao, Ma, Ten) VALUES (NEWID(), SYSUTCDATETIME(), 'ADMIN', N'Quan tri vien');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107155311_TaiKhoanJwtVaKhoiPhuc'
)
BEGIN
    CREATE TABLE [dbo].[PhienDangNhap] (
        [Id] uniqueidentifier NOT NULL,
        [NgayTao] datetimeoffset(0) NOT NULL,
        [PhienBan] rowversion NOT NULL,
        [NguoiDungId] uniqueidentifier NOT NULL,
        [MaTruyCap] varchar(32) NOT NULL,
        [BamRefreshToken] varchar(64) NOT NULL,
        [NgayHetHan] datetimeoffset(0) NOT NULL,
        [NgayThuHoi] datetimeoffset(0) NULL,
        CONSTRAINT [PK_PhienDangNhap] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_PhienDangNhap_HetHan] CHECK ([NgayHetHan] > [NgayTao]),
        CONSTRAINT [FK_PhienDangNhap_NguoiDung_NguoiDungId] FOREIGN KEY ([NguoiDungId]) REFERENCES [dbo].[NguoiDung] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107155311_TaiKhoanJwtVaKhoiPhuc'
)
BEGIN
    CREATE TABLE [dbo].[YeuCauDatLaiMatKhau] (
        [Id] uniqueidentifier NOT NULL,
        [NgayTao] datetimeoffset(0) NOT NULL,
        [PhienBan] rowversion NOT NULL,
        [NguoiDungId] uniqueidentifier NOT NULL,
        [BamToken] varchar(64) NOT NULL,
        [NgayHetHan] datetimeoffset(0) NOT NULL,
        [NgaySuDung] datetimeoffset(0) NULL,
        CONSTRAINT [PK_YeuCauDatLaiMatKhau] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_YeuCauDatLaiMatKhau_HetHan] CHECK ([NgayHetHan] > [NgayTao]),
        CONSTRAINT [FK_YeuCauDatLaiMatKhau_NguoiDung_NguoiDungId] FOREIGN KEY ([NguoiDungId]) REFERENCES [dbo].[NguoiDung] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107155311_TaiKhoanJwtVaKhoiPhuc'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PhienDangNhap_BamRefreshToken] ON [dbo].[PhienDangNhap] ([BamRefreshToken]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107155311_TaiKhoanJwtVaKhoiPhuc'
)
BEGIN
    CREATE INDEX [IX_PhienDangNhap_NguoiDungId_NgayThuHoi] ON [dbo].[PhienDangNhap] ([NguoiDungId], [NgayThuHoi]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107155311_TaiKhoanJwtVaKhoiPhuc'
)
BEGIN
    CREATE UNIQUE INDEX [IX_YeuCauDatLaiMatKhau_BamToken] ON [dbo].[YeuCauDatLaiMatKhau] ([BamToken]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107155311_TaiKhoanJwtVaKhoiPhuc'
)
BEGIN
    CREATE INDEX [IX_YeuCauDatLaiMatKhau_NguoiDungId_NgayHetHan] ON [dbo].[YeuCauDatLaiMatKhau] ([NguoiDungId], [NgayHetHan]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261107155311_TaiKhoanJwtVaKhoiPhuc'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261107155311_TaiKhoanJwtVaKhoiPhuc', N'8.0.33');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108165930_TinDangMuaBan'
)
BEGIN
    ALTER TABLE [dbo].[TinDang] ADD [LuotXem] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108165930_TinDangMuaBan'
)
BEGIN
    ALTER TABLE [dbo].[TinDang] ADD [NgayCapNhat] datetimeoffset(0) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108165930_TinDangMuaBan'
)
BEGIN
    ALTER TABLE [dbo].[TinDang] ADD [NgayXoa] datetimeoffset(0) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108165930_TinDangMuaBan'
)
BEGIN
    CREATE INDEX [IX_TinDang_NgayXoa_TrangThai_LoaiTin_NgayDang] ON [dbo].[TinDang] ([NgayXoa], [TrangThai], [LoaiTin], [NgayDang]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108165930_TinDangMuaBan'
)
BEGIN
    CREATE INDEX [IX_TinDang_NguoiDangId_NgayXoa_TrangThai] ON [dbo].[TinDang] ([NguoiDangId], [NgayXoa], [TrangThai]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108165930_TinDangMuaBan'
)
BEGIN
    EXEC(N'ALTER TABLE [dbo].[TinDang] ADD CONSTRAINT [CK_TinDang_LuotXem] CHECK ([LuotXem] >= 0)');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261108165930_TinDangMuaBan'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261108165930_TinDangMuaBan', N'8.0.33');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110141612_MuaBanThuongLuongVaHoTro'
)
BEGIN
    DROP INDEX [IX_YeuCauHoTro_NguoiGuiId] ON [dbo].[YeuCauHoTro];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110141612_MuaBanThuongLuongVaHoTro'
)
BEGIN
    ALTER TABLE [dbo].[TinDang] DROP CONSTRAINT [CK_TinDang_TrangThai];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110141612_MuaBanThuongLuongVaHoTro'
)
BEGIN
    DROP INDEX [IX_CuocTroChuyen_TinDangId] ON [dbo].[CuocTroChuyen];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110141612_MuaBanThuongLuongVaHoTro'
)
BEGIN
    ALTER TABLE [dbo].[YeuCauHoTro] ADD [MaYeuCau] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110141612_MuaBanThuongLuongVaHoTro'
)
BEGIN
    ALTER TABLE [dbo].[TinNhan] ADD [SoThuTu] bigint NOT NULL IDENTITY;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110141612_MuaBanThuongLuongVaHoTro'
)
BEGIN
    ALTER TABLE [dbo].[ThanhVienTroChuyen] ADD [SoThuTuDaDoc] bigint NOT NULL DEFAULT CAST(0 AS bigint);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110141612_MuaBanThuongLuongVaHoTro'
)
BEGIN
    ALTER TABLE [dbo].[ThanhToan] ADD [KetQuaCuoi] bit NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110141612_MuaBanThuongLuongVaHoTro'
)
BEGIN
    ALTER TABLE [dbo].[ThanhToan] ADD [MaYeuCauCuoi] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110141612_MuaBanThuongLuongVaHoTro'
)
BEGIN
    ALTER TABLE [dbo].[ThanhToan] ADD [NgayGiaiNgan] datetimeoffset(0) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110141612_MuaBanThuongLuongVaHoTro'
)
BEGIN
    ALTER TABLE [dbo].[ThanhToan] ADD [NgayHoanTien] datetimeoffset(0) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110141612_MuaBanThuongLuongVaHoTro'
)
BEGIN
    ALTER TABLE [dbo].[ThanhToan] ADD [NgayThanhToan] datetimeoffset(0) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110141612_MuaBanThuongLuongVaHoTro'
)
BEGIN
    ALTER TABLE [dbo].[DonHang] ADD [BamYeuCau] varchar(64) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110141612_MuaBanThuongLuongVaHoTro'
)
BEGIN
    ALTER TABLE [dbo].[DonHang] ADD [HanThanhToan] datetimeoffset(0) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110141612_MuaBanThuongLuongVaHoTro'
)
BEGIN
    ALTER TABLE [dbo].[DonHang] ADD [LyDoHuy] nvarchar(500) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110141612_MuaBanThuongLuongVaHoTro'
)
BEGIN
    ALTER TABLE [dbo].[DonHang] ADD [MaYeuCau] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110141612_MuaBanThuongLuongVaHoTro'
)
BEGIN
    ALTER TABLE [dbo].[DonHang] ADD [NgayHuy] datetimeoffset(0) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110141612_MuaBanThuongLuongVaHoTro'
)
BEGIN
    ALTER TABLE [dbo].[DonHang] ADD [PhieuThuongLuongId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110141612_MuaBanThuongLuongVaHoTro'
)
BEGIN
    ALTER TABLE [dbo].[DanhGia] ADD [MaYeuCau] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110141612_MuaBanThuongLuongVaHoTro'
)
BEGIN
    ALTER TABLE [dbo].[DanhGia] ADD [SoSaoSanPham] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110141612_MuaBanThuongLuongVaHoTro'
)
BEGIN
    ALTER TABLE [dbo].[CuocTroChuyen] ADD [NguoiMuaId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110141612_MuaBanThuongLuongVaHoTro'
)
BEGIN
    ALTER TABLE [dbo].[ChiTietDonHang] ADD [GiaNiemYet] bigint NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110141612_MuaBanThuongLuongVaHoTro'
)
BEGIN
    ALTER TABLE [dbo].[ChiTietDonHang] ADD [SoTienGiam] bigint NOT NULL DEFAULT CAST(0 AS bigint);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110141612_MuaBanThuongLuongVaHoTro'
)
BEGIN
    ALTER TABLE [dbo].[TinNhan] ADD CONSTRAINT [AK_TinNhan_Id_CuocTroChuyenId] UNIQUE ([Id], [CuocTroChuyenId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110141612_MuaBanThuongLuongVaHoTro'
)
BEGIN
    CREATE TABLE [dbo].[LichSuHoTro] (
        [Id] uniqueidentifier NOT NULL,
        [NgayTao] datetimeoffset(0) NOT NULL,
        [PhienBan] rowversion NOT NULL,
        [YeuCauHoTroId] uniqueidentifier NOT NULL,
        [NguoiThucHienId] uniqueidentifier NOT NULL,
        [TrangThai] varchar(40) NOT NULL,
        [NoiDung] nvarchar(4000) NOT NULL,
        CONSTRAINT [PK_LichSuHoTro] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_LichSuHoTro_TrangThai] CHECK ([TrangThai] IN ('Moi','DangXuLy','DaGiaiQuyet')),
        CONSTRAINT [FK_LichSuHoTro_NguoiDung_NguoiThucHienId] FOREIGN KEY ([NguoiThucHienId]) REFERENCES [dbo].[NguoiDung] ([Id]),
        CONSTRAINT [FK_LichSuHoTro_YeuCauHoTro_YeuCauHoTroId] FOREIGN KEY ([YeuCauHoTroId]) REFERENCES [dbo].[YeuCauHoTro] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110141612_MuaBanThuongLuongVaHoTro'
)
BEGIN
    CREATE TABLE [dbo].[PhieuThuongLuong] (
        [Id] uniqueidentifier NOT NULL,
        [NgayTao] datetimeoffset(0) NOT NULL,
        [PhienBan] rowversion NOT NULL,
        [CuocTroChuyenId] uniqueidentifier NOT NULL,
        [TinNhanId] uniqueidentifier NOT NULL,
        [NguoiMuaId] uniqueidentifier NOT NULL,
        [NguoiBanId] uniqueidentifier NOT NULL,
        [GiaNiemYet] bigint NOT NULL,
        [SoTienGiam] bigint NOT NULL,
        [TrangThai] varchar(40) NOT NULL,
        [NgayHetHan] datetimeoffset(0) NOT NULL,
        CONSTRAINT [PK_PhieuThuongLuong] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_PhieuThuongLuong_Gia] CHECK ([GiaNiemYet]>0 AND [SoTienGiam]>0 AND [SoTienGiam]<[GiaNiemYet]),
        CONSTRAINT [CK_PhieuThuongLuong_Han] CHECK ([NgayHetHan]>[NgayTao]),
        CONSTRAINT [CK_PhieuThuongLuong_Nguoi] CHECK ([NguoiMuaId]<>[NguoiBanId]),
        CONSTRAINT [CK_PhieuThuongLuong_TrangThai] CHECK ([TrangThai] IN ('ChoPhanHoi','DaChapNhan','TuChoi','DaHuy','DaSuDung','HetHan')),
        CONSTRAINT [FK_PhieuThuongLuong_CuocTroChuyen_CuocTroChuyenId] FOREIGN KEY ([CuocTroChuyenId]) REFERENCES [dbo].[CuocTroChuyen] ([Id]),
        CONSTRAINT [FK_PhieuThuongLuong_ThanhVienTroChuyen_CuocTroChuyenId_NguoiBanId] FOREIGN KEY ([CuocTroChuyenId], [NguoiBanId]) REFERENCES [dbo].[ThanhVienTroChuyen] ([CuocTroChuyenId], [NguoiDungId]),
        CONSTRAINT [FK_PhieuThuongLuong_ThanhVienTroChuyen_CuocTroChuyenId_NguoiMuaId] FOREIGN KEY ([CuocTroChuyenId], [NguoiMuaId]) REFERENCES [dbo].[ThanhVienTroChuyen] ([CuocTroChuyenId], [NguoiDungId]),
        CONSTRAINT [FK_PhieuThuongLuong_TinNhan_TinNhanId_CuocTroChuyenId] FOREIGN KEY ([TinNhanId], [CuocTroChuyenId]) REFERENCES [dbo].[TinNhan] ([Id], [CuocTroChuyenId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110141612_MuaBanThuongLuongVaHoTro'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_YeuCauHoTro_NguoiGuiId_MaYeuCau] ON [dbo].[YeuCauHoTro] ([NguoiGuiId], [MaYeuCau]) WHERE [MaYeuCau] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110141612_MuaBanThuongLuongVaHoTro'
)
BEGIN
    CREATE INDEX [IX_TinNhan_CuocTroChuyenId_SoThuTu] ON [dbo].[TinNhan] ([CuocTroChuyenId], [SoThuTu]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110141612_MuaBanThuongLuongVaHoTro'
)
BEGIN
    EXEC(N'ALTER TABLE [dbo].[TinDang] ADD CONSTRAINT [CK_TinDang_TrangThai] CHECK ([TrangThai] IN (N''BanNhap'', N''ChoDuyet'', N''DangHienThi'', N''TuChoi'', N''DaAn'', N''DaBan'', N''DangGiuCho''))');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110141612_MuaBanThuongLuongVaHoTro'
)
BEGIN
    CREATE INDEX [IX_DonHang_NguoiBanId_NgayTao] ON [dbo].[DonHang] ([NguoiBanId], [NgayTao]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110141612_MuaBanThuongLuongVaHoTro'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_DonHang_NguoiMuaId_MaYeuCau] ON [dbo].[DonHang] ([NguoiMuaId], [MaYeuCau]) WHERE [MaYeuCau] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110141612_MuaBanThuongLuongVaHoTro'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_DonHang_PhieuThuongLuongId] ON [dbo].[DonHang] ([PhieuThuongLuongId]) WHERE [PhieuThuongLuongId] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110141612_MuaBanThuongLuongVaHoTro'
)
BEGIN
    CREATE INDEX [IX_DonHang_TrangThai_HanThanhToan] ON [dbo].[DonHang] ([TrangThai], [HanThanhToan]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110141612_MuaBanThuongLuongVaHoTro'
)
BEGIN
    EXEC(N'ALTER TABLE [dbo].[DanhGia] ADD CONSTRAINT [CK_DanhGia_SanPham] CHECK ([SoSaoSanPham] IS NULL OR [SoSaoSanPham] BETWEEN 1 AND 5)');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110141612_MuaBanThuongLuongVaHoTro'
)
BEGIN
    CREATE INDEX [IX_CuocTroChuyen_NguoiMuaId] ON [dbo].[CuocTroChuyen] ([NguoiMuaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110141612_MuaBanThuongLuongVaHoTro'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_CuocTroChuyen_TinDangId_NguoiMuaId] ON [dbo].[CuocTroChuyen] ([TinDangId], [NguoiMuaId]) WHERE [NguoiMuaId] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110141612_MuaBanThuongLuongVaHoTro'
)
BEGIN
    EXEC(N'ALTER TABLE [dbo].[ChiTietDonHang] ADD CONSTRAINT [CK_ChiTietDonHang_GiamGia] CHECK ([SoTienGiam]>=0 AND ([GiaNiemYet] IS NULL OR ([GiaNiemYet]>0 AND [DonGia]=[GiaNiemYet]-[SoTienGiam])))');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110141612_MuaBanThuongLuongVaHoTro'
)
BEGIN
    CREATE INDEX [IX_LichSuHoTro_NguoiThucHienId] ON [dbo].[LichSuHoTro] ([NguoiThucHienId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110141612_MuaBanThuongLuongVaHoTro'
)
BEGIN
    CREATE INDEX [IX_LichSuHoTro_YeuCauHoTroId_NgayTao] ON [dbo].[LichSuHoTro] ([YeuCauHoTroId], [NgayTao]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110141612_MuaBanThuongLuongVaHoTro'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_PhieuThuongLuong_CuocTroChuyenId] ON [dbo].[PhieuThuongLuong] ([CuocTroChuyenId]) WHERE [TrangThai]=''DaChapNhan''');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110141612_MuaBanThuongLuongVaHoTro'
)
BEGIN
    CREATE INDEX [IX_PhieuThuongLuong_CuocTroChuyenId_NguoiBanId] ON [dbo].[PhieuThuongLuong] ([CuocTroChuyenId], [NguoiBanId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110141612_MuaBanThuongLuongVaHoTro'
)
BEGIN
    CREATE INDEX [IX_PhieuThuongLuong_CuocTroChuyenId_NguoiMuaId] ON [dbo].[PhieuThuongLuong] ([CuocTroChuyenId], [NguoiMuaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110141612_MuaBanThuongLuongVaHoTro'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PhieuThuongLuong_TinNhanId] ON [dbo].[PhieuThuongLuong] ([TinNhanId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110141612_MuaBanThuongLuongVaHoTro'
)
BEGIN
    CREATE INDEX [IX_PhieuThuongLuong_TinNhanId_CuocTroChuyenId] ON [dbo].[PhieuThuongLuong] ([TinNhanId], [CuocTroChuyenId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110141612_MuaBanThuongLuongVaHoTro'
)
BEGIN
    ALTER TABLE [dbo].[CuocTroChuyen] ADD CONSTRAINT [FK_CuocTroChuyen_NguoiDung_NguoiMuaId] FOREIGN KEY ([NguoiMuaId]) REFERENCES [dbo].[NguoiDung] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110141612_MuaBanThuongLuongVaHoTro'
)
BEGIN
    ALTER TABLE [dbo].[DonHang] ADD CONSTRAINT [FK_DonHang_PhieuThuongLuong_PhieuThuongLuongId] FOREIGN KEY ([PhieuThuongLuongId]) REFERENCES [dbo].[PhieuThuongLuong] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110141612_MuaBanThuongLuongVaHoTro'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261110141612_MuaBanThuongLuongVaHoTro', N'8.0.33');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110141855_ThuTuLichSuMoPhong'
)
BEGIN
    ALTER TABLE [dbo].[LichSuVanChuyen] ADD [SoThuTu] bigint NOT NULL IDENTITY;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110141855_ThuTuLichSuMoPhong'
)
BEGIN
    ALTER TABLE [dbo].[LichSuHoTro] ADD [SoThuTu] bigint NOT NULL IDENTITY;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110141855_ThuTuLichSuMoPhong'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261110141855_ThuTuLichSuMoPhong', N'8.0.33');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111155701_TraoDoiMotMot'
)
BEGIN
    IF NOT EXISTS(SELECT 1 FROM dbo.TieuChiDanhGia WHERE Ma='DUNG_MO_TA')
        INSERT dbo.TieuChiDanhGia(Id,NgayTao,Ma,Ten) VALUES('F5E8217D-9E01-4DF2-8A82-DB408787A3C3',SYSUTCDATETIME(),'DUNG_MO_TA',N'Đúng mô tả');
    IF NOT EXISTS(SELECT 1 FROM dbo.TieuChiDanhGia WHERE Ma='DUNG_HEN')
        INSERT dbo.TieuChiDanhGia(Id,NgayTao,Ma,Ten) VALUES('9A00A146-9387-4846-9EDC-6940E42D5EF2',SYSUTCDATETIME(),'DUNG_HEN',N'Đúng hẹn, giao tiếp thân thiện');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111155701_TraoDoiMotMot'
)
BEGIN
    ALTER TABLE [dbo].[TinDang] DROP CONSTRAINT [CK_TinDang_TrangThai];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111155701_TraoDoiMotMot'
)
BEGIN
    DROP INDEX [IX_BangChungGiaoNhan_XacNhanGiaoNhanId] ON [dbo].[BangChungGiaoNhan];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111155701_TraoDoiMotMot'
)
BEGIN
    ALTER TABLE [dbo].[NhuCauTraoDoi] ADD [ChoPhepMotMot] bit NOT NULL DEFAULT CAST(1 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111155701_TraoDoiMotMot'
)
BEGIN
    ALTER TABLE [dbo].[NhuCauTraoDoi] ADD [GhiChu] nvarchar(1000) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111155701_TraoDoiMotMot'
)
BEGIN
    ALTER TABLE [dbo].[DeNghiTraoDoi] ADD [BamYeuCau] varchar(64) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111155701_TraoDoiMotMot'
)
BEGIN
    ALTER TABLE [dbo].[DeNghiTraoDoi] ADD [LyDoKetThuc] nvarchar(500) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111155701_TraoDoiMotMot'
)
BEGIN
    ALTER TABLE [dbo].[DeNghiTraoDoi] ADD [MaYeuCau] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111155701_TraoDoiMotMot'
)
BEGIN
    ALTER TABLE [dbo].[DeNghiTraoDoi] ADD [TinGuiId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111155701_TraoDoiMotMot'
)
BEGIN
    ALTER TABLE [dbo].[DeNghiTraoDoi] ADD [TinNhanId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111155701_TraoDoiMotMot'
)
BEGIN
    ALTER TABLE [dbo].[BangChungGiaoNhan] ADD [BamNoiDung] varchar(64) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111155701_TraoDoiMotMot'
)
BEGIN
    ALTER TABLE [dbo].[BangChungGiaoNhan] ADD [MaYeuCau] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111155701_TraoDoiMotMot'
)
BEGIN
    EXEC(N'ALTER TABLE [dbo].[TinDang] ADD CONSTRAINT [CK_TinDang_TrangThai] CHECK ([TrangThai] IN (N''BanNhap'', N''ChoDuyet'', N''DangHienThi'', N''TuChoi'', N''DaAn'', N''DaBan'', N''DangGiuCho'', N''DaTraoDoi''))');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111155701_TraoDoiMotMot'
)
BEGIN
    EXEC(N'ALTER TABLE [dbo].[NhuCauTraoDoi] ADD CONSTRAINT [CK_NhuCauTraoDoi_HinhThuc] CHECK ([ChoPhepMotMot]=1 OR [ChoPhepSmartSwap]=1)');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111155701_TraoDoiMotMot'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_DeNghiTraoDoi_NguoiGuiId_MaYeuCau] ON [dbo].[DeNghiTraoDoi] ([NguoiGuiId], [MaYeuCau]) WHERE [MaYeuCau] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111155701_TraoDoiMotMot'
)
BEGIN
    CREATE INDEX [IX_DeNghiTraoDoi_NguoiNhanId_NgayTao] ON [dbo].[DeNghiTraoDoi] ([NguoiNhanId], [NgayTao]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111155701_TraoDoiMotMot'
)
BEGIN
    CREATE INDEX [IX_DeNghiTraoDoi_TinGuiId_SanPhamGuiId] ON [dbo].[DeNghiTraoDoi] ([TinGuiId], [SanPhamGuiId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111155701_TraoDoiMotMot'
)
BEGIN
    CREATE INDEX [IX_DeNghiTraoDoi_TinNhanId_SanPhamNhanId] ON [dbo].[DeNghiTraoDoi] ([TinNhanId], [SanPhamNhanId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111155701_TraoDoiMotMot'
)
BEGIN
    CREATE INDEX [IX_DeNghiTraoDoi_TrangThai_NgayHetHan] ON [dbo].[DeNghiTraoDoi] ([TrangThai], [NgayHetHan]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111155701_TraoDoiMotMot'
)
BEGIN
    EXEC(N'ALTER TABLE [dbo].[DeNghiTraoDoi] ADD CONSTRAINT [CK_DeNghiTraoDoi_Tin] CHECK (([TinGuiId] IS NULL AND [TinNhanId] IS NULL) OR ([TinGuiId] IS NOT NULL AND [TinNhanId] IS NOT NULL AND [TinGuiId]<>[TinNhanId]))');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111155701_TraoDoiMotMot'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_BangChungGiaoNhan_XacNhanGiaoNhanId_MaYeuCau] ON [dbo].[BangChungGiaoNhan] ([XacNhanGiaoNhanId], [MaYeuCau]) WHERE [MaYeuCau] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111155701_TraoDoiMotMot'
)
BEGIN
    ALTER TABLE [dbo].[DeNghiTraoDoi] ADD CONSTRAINT [FK_DeNghiTraoDoi_TinDang_TinGuiId_SanPhamGuiId] FOREIGN KEY ([TinGuiId], [SanPhamGuiId]) REFERENCES [dbo].[TinDang] ([Id], [SanPhamId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111155701_TraoDoiMotMot'
)
BEGIN
    ALTER TABLE [dbo].[DeNghiTraoDoi] ADD CONSTRAINT [FK_DeNghiTraoDoi_TinDang_TinNhanId_SanPhamNhanId] FOREIGN KEY ([TinNhanId], [SanPhamNhanId]) REFERENCES [dbo].[TinDang] ([Id], [SanPhamId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111155701_TraoDoiMotMot'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261111155701_TraoDoiMotMot', N'8.0.33');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112061936_TieuChiTimSmartSwap'
)
BEGIN
    ALTER TABLE [dbo].[NhuCauTraoDoi] ADD [GiaTriMongMuonDen] bigint NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112061936_TieuChiTimSmartSwap'
)
BEGIN
    ALTER TABLE [dbo].[NhuCauTraoDoi] ADD [GiaTriMongMuonTu] bigint NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112061936_TieuChiTimSmartSwap'
)
BEGIN
    ALTER TABLE [dbo].[NhuCauTraoDoi] ADD [TinhTrangToiThieu] nvarchar(40) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112061936_TieuChiTimSmartSwap'
)
BEGIN
    ALTER TABLE [dbo].[NhuCauTraoDoi] ADD [TuKhoaMongMuon] nvarchar(200) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112061936_TieuChiTimSmartSwap'
)
BEGIN
    EXEC(N'ALTER TABLE [dbo].[NhuCauTraoDoi] ADD CONSTRAINT [CK_NhuCauTraoDoi_GiaMongMuon] CHECK (([GiaTriMongMuonTu] IS NULL OR [GiaTriMongMuonTu] BETWEEN 0 AND 1000000000000) AND ([GiaTriMongMuonDen] IS NULL OR [GiaTriMongMuonDen] BETWEEN 0 AND 1000000000000) AND ([GiaTriMongMuonTu] IS NULL OR [GiaTriMongMuonDen] IS NULL OR [GiaTriMongMuonTu]<=[GiaTriMongMuonDen]))');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112061936_TieuChiTimSmartSwap'
)
BEGIN
    EXEC(N'ALTER TABLE [dbo].[NhuCauTraoDoi] ADD CONSTRAINT [CK_NhuCauTraoDoi_TinhTrang] CHECK ([TinhTrangToiThieu] IS NULL OR [TinhTrangToiThieu] IN (''NhuMoi'',''DaSuDungTot'',''CoLoiNhe''))');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112061936_TieuChiTimSmartSwap'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261112061936_TieuChiTimSmartSwap', N'8.0.33');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112070453_LoiMoiVaThuongLuongSmartSwap'
)
BEGIN
    DROP INDEX [IX_PhienBanThoaThuan_GiaoDichId_NguoiTaoId] ON [dbo].[PhienBanThoaThuan];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112070453_LoiMoiVaThuongLuongSmartSwap'
)
BEGIN
    ALTER TABLE [dbo].[VongSmartSwap] ADD [BamYeuCau] varchar(64) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112070453_LoiMoiVaThuongLuongSmartSwap'
)
BEGIN
    ALTER TABLE [dbo].[VongSmartSwap] ADD [LyDoKetThuc] nvarchar(500) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112070453_LoiMoiVaThuongLuongSmartSwap'
)
BEGIN
    ALTER TABLE [dbo].[VongSmartSwap] ADD [MaDeXuat] varchar(98) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112070453_LoiMoiVaThuongLuongSmartSwap'
)
BEGIN
    ALTER TABLE [dbo].[VongSmartSwap] ADD [MaKetThuc] varchar(40) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112070453_LoiMoiVaThuongLuongSmartSwap'
)
BEGIN
    ALTER TABLE [dbo].[VongSmartSwap] ADD [MaYeuCau] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112070453_LoiMoiVaThuongLuongSmartSwap'
)
BEGIN
    ALTER TABLE [dbo].[VongSmartSwap] ADD [NgayChapNhan] datetimeoffset(0) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112070453_LoiMoiVaThuongLuongSmartSwap'
)
BEGIN
    ALTER TABLE [dbo].[VongSmartSwap] ADD [NguoiKhoiTaoId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112070453_LoiMoiVaThuongLuongSmartSwap'
)
BEGIN
    ALTER TABLE [dbo].[TinNhan] ADD [PhienBanThoaThuanId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112070453_LoiMoiVaThuongLuongSmartSwap'
)
BEGIN
    ALTER TABLE [dbo].[ThanhVienVong] ADD [TinDangId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112070453_LoiMoiVaThuongLuongSmartSwap'
)
BEGIN
    ALTER TABLE [dbo].[PhienBanThoaThuan] ADD [BamYeuCau] varchar(64) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112070453_LoiMoiVaThuongLuongSmartSwap'
)
BEGIN
    ALTER TABLE [dbo].[PhienBanThoaThuan] ADD [MaYeuCau] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112070453_LoiMoiVaThuongLuongSmartSwap'
)
BEGIN
    CREATE INDEX [IX_VongSmartSwap_GiaoDichId_NguoiKhoiTaoId] ON [dbo].[VongSmartSwap] ([GiaoDichId], [NguoiKhoiTaoId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112070453_LoiMoiVaThuongLuongSmartSwap'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_VongSmartSwap_MaDeXuat] ON [dbo].[VongSmartSwap] ([MaDeXuat]) WHERE [MaDeXuat] IS NOT NULL AND [TrangThai]=''ChoDongY''');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112070453_LoiMoiVaThuongLuongSmartSwap'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_VongSmartSwap_NguoiKhoiTaoId_MaYeuCau] ON [dbo].[VongSmartSwap] ([NguoiKhoiTaoId], [MaYeuCau]) WHERE [NguoiKhoiTaoId] IS NOT NULL AND [MaYeuCau] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112070453_LoiMoiVaThuongLuongSmartSwap'
)
BEGIN
    CREATE INDEX [IX_VongSmartSwap_TrangThai_NgayHetHan] ON [dbo].[VongSmartSwap] ([TrangThai], [NgayHetHan]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112070453_LoiMoiVaThuongLuongSmartSwap'
)
BEGIN
    EXEC(N'ALTER TABLE [dbo].[VongSmartSwap] ADD CONSTRAINT [CK_VongSmartSwap_KetThuc] CHECK ([MaKetThuc] IS NULL OR ([TrangThai]=''DaHuy'' AND [MaKetThuc] IN (''TuChoi'',''HetHan'',''DaHuy'')))');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112070453_LoiMoiVaThuongLuongSmartSwap'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_TinNhan_PhienBanThoaThuanId] ON [dbo].[TinNhan] ([PhienBanThoaThuanId]) WHERE [PhienBanThoaThuanId] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112070453_LoiMoiVaThuongLuongSmartSwap'
)
BEGIN
    CREATE INDEX [IX_ThanhVienVong_TinDangId_SanPhamId] ON [dbo].[ThanhVienVong] ([TinDangId], [SanPhamId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112070453_LoiMoiVaThuongLuongSmartSwap'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_PhienBanThoaThuan_GiaoDichId_NguoiTaoId_MaYeuCau] ON [dbo].[PhienBanThoaThuan] ([GiaoDichId], [NguoiTaoId], [MaYeuCau]) WHERE [MaYeuCau] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112070453_LoiMoiVaThuongLuongSmartSwap'
)
BEGIN
    ALTER TABLE [dbo].[ThanhVienVong] ADD CONSTRAINT [FK_ThanhVienVong_TinDang_TinDangId_SanPhamId] FOREIGN KEY ([TinDangId], [SanPhamId]) REFERENCES [dbo].[TinDang] ([Id], [SanPhamId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112070453_LoiMoiVaThuongLuongSmartSwap'
)
BEGIN
    ALTER TABLE [dbo].[TinNhan] ADD CONSTRAINT [FK_TinNhan_PhienBanThoaThuan_PhienBanThoaThuanId] FOREIGN KEY ([PhienBanThoaThuanId]) REFERENCES [dbo].[PhienBanThoaThuan] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112070453_LoiMoiVaThuongLuongSmartSwap'
)
BEGIN
    ALTER TABLE [dbo].[VongSmartSwap] ADD CONSTRAINT [FK_VongSmartSwap_ThanhVienGiaoDich_GiaoDichId_NguoiKhoiTaoId] FOREIGN KEY ([GiaoDichId], [NguoiKhoiTaoId]) REFERENCES [dbo].[ThanhVienGiaoDich] ([GiaoDichId], [NguoiDungId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112070453_LoiMoiVaThuongLuongSmartSwap'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261112070453_LoiMoiVaThuongLuongSmartSwap', N'8.0.33');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112080514_GiaoNhanSmartSwap'
)
BEGIN
    ALTER TABLE [dbo].[VongSmartSwap] DROP CONSTRAINT [CK_VongSmartSwap_TrangThai];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112080514_GiaoNhanSmartSwap'
)
BEGIN
    DROP INDEX [IX_VanChuyenMoPhong_DonHangId] ON [dbo].[VanChuyenMoPhong];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112080514_GiaoNhanSmartSwap'
)
BEGIN
    ALTER TABLE [dbo].[VanChuyenMoPhong] DROP CONSTRAINT [CK_VanChuyenMoPhong_TrangThai];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112080514_GiaoNhanSmartSwap'
)
BEGIN
    ALTER TABLE [dbo].[LichSuVanChuyen] DROP CONSTRAINT [CK_LichSuVanChuyen_TrangThai];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112080514_GiaoNhanSmartSwap'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[dbo].[VanChuyenMoPhong]') AND [c].[name] = N'DonHangId');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [dbo].[VanChuyenMoPhong] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [dbo].[VanChuyenMoPhong] ALTER COLUMN [DonHangId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112080514_GiaoNhanSmartSwap'
)
BEGIN
    ALTER TABLE [dbo].[VanChuyenMoPhong] ADD [ChangTraoDoiId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112080514_GiaoNhanSmartSwap'
)
BEGIN
    ALTER TABLE [dbo].[VanChuyenMoPhong] ADD [DiaChiGui] nvarchar(500) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112080514_GiaoNhanSmartSwap'
)
BEGIN
    ALTER TABLE [dbo].[VanChuyenMoPhong] ADD [DiaChiNhan] nvarchar(500) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112080514_GiaoNhanSmartSwap'
)
BEGIN
    ALTER TABLE [dbo].[VanChuyenMoPhong] ADD [DienThoaiGui] nvarchar(10) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112080514_GiaoNhanSmartSwap'
)
BEGIN
    ALTER TABLE [dbo].[VanChuyenMoPhong] ADD [DienThoaiNhan] nvarchar(10) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112080514_GiaoNhanSmartSwap'
)
BEGIN
    ALTER TABLE [dbo].[VanChuyenMoPhong] ADD [MaVanDonTuNhap] nvarchar(40) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112080514_GiaoNhanSmartSwap'
)
BEGIN
    ALTER TABLE [dbo].[VanChuyenMoPhong] ADD [NgayNhanDungHang] datetimeoffset(0) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112080514_GiaoNhanSmartSwap'
)
BEGIN
    ALTER TABLE [dbo].[VanChuyenMoPhong] ADD [PhienBanThoaThuanId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112080514_GiaoNhanSmartSwap'
)
BEGIN
    ALTER TABLE [dbo].[VanChuyenMoPhong] ADD [TenNguoiGui] nvarchar(120) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112080514_GiaoNhanSmartSwap'
)
BEGIN
    ALTER TABLE [dbo].[VanChuyenMoPhong] ADD [TenNguoiNhan] nvarchar(120) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112080514_GiaoNhanSmartSwap'
)
BEGIN
    ALTER TABLE [dbo].[KhieuNai] ADD [BamYeuCau] varchar(64) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112080514_GiaoNhanSmartSwap'
)
BEGIN
    ALTER TABLE [dbo].[KhieuNai] ADD [LoaiSuCo] varchar(40) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112080514_GiaoNhanSmartSwap'
)
BEGIN
    ALTER TABLE [dbo].[KhieuNai] ADD [MaYeuCau] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112080514_GiaoNhanSmartSwap'
)
BEGIN
    ALTER TABLE [dbo].[KhieuNai] ADD [VanChuyenMoPhongId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112080514_GiaoNhanSmartSwap'
)
BEGIN
    CREATE TABLE [ThongTinGiaoNhanSmartSwap] (
        [Id] uniqueidentifier NOT NULL,
        [NgayTao] datetimeoffset(0) NOT NULL,
        [PhienBan] rowversion NOT NULL,
        [ThanhVienVongId] uniqueidentifier NOT NULL,
        [PhienBanThoaThuanId] uniqueidentifier NOT NULL,
        [MaYeuCau] uniqueidentifier NOT NULL,
        [BamYeuCau] varchar(64) NOT NULL,
        [TenLienHe] nvarchar(120) NOT NULL,
        [SoDienThoai] nvarchar(10) NOT NULL,
        [DiaChiGui] nvarchar(500) NOT NULL,
        [DiaChiNhan] nvarchar(500) NOT NULL,
        [NgayXacNhan] datetimeoffset(0) NOT NULL,
        CONSTRAINT [PK_ThongTinGiaoNhanSmartSwap] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ThongTinGiaoNhanSmartSwap_PhienBanThoaThuan_PhienBanThoaThuanId] FOREIGN KEY ([PhienBanThoaThuanId]) REFERENCES [dbo].[PhienBanThoaThuan] ([Id]),
        CONSTRAINT [FK_ThongTinGiaoNhanSmartSwap_ThanhVienVong_ThanhVienVongId] FOREIGN KEY ([ThanhVienVongId]) REFERENCES [dbo].[ThanhVienVong] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112080514_GiaoNhanSmartSwap'
)
BEGIN
    EXEC(N'ALTER TABLE [dbo].[VongSmartSwap] ADD CONSTRAINT [CK_VongSmartSwap_TrangThai] CHECK ([TrangThai] IN (N''ChoDongY'', N''SanSangBanGiao'', N''DangBanGiao'', N''HoanTat'', N''DaHuy'', N''TamDung''))');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112080514_GiaoNhanSmartSwap'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_VanChuyenMoPhong_ChangTraoDoiId] ON [dbo].[VanChuyenMoPhong] ([ChangTraoDoiId]) WHERE [ChangTraoDoiId] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112080514_GiaoNhanSmartSwap'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_VanChuyenMoPhong_DonHangId] ON [dbo].[VanChuyenMoPhong] ([DonHangId]) WHERE [DonHangId] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112080514_GiaoNhanSmartSwap'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_VanChuyenMoPhong_MaVanDonTuNhap] ON [dbo].[VanChuyenMoPhong] ([MaVanDonTuNhap]) WHERE [MaVanDonTuNhap] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112080514_GiaoNhanSmartSwap'
)
BEGIN
    CREATE INDEX [IX_VanChuyenMoPhong_PhienBanThoaThuanId] ON [dbo].[VanChuyenMoPhong] ([PhienBanThoaThuanId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112080514_GiaoNhanSmartSwap'
)
BEGIN
    EXEC(N'ALTER TABLE [dbo].[VanChuyenMoPhong] ADD CONSTRAINT [CK_VanChuyenMoPhong_Nguon] CHECK (([DonHangId] IS NOT NULL AND [ChangTraoDoiId] IS NULL AND [PhienBanThoaThuanId] IS NULL) OR ([DonHangId] IS NULL AND [ChangTraoDoiId] IS NOT NULL AND [PhienBanThoaThuanId] IS NOT NULL))');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112080514_GiaoNhanSmartSwap'
)
BEGIN
    EXEC(N'ALTER TABLE [dbo].[VanChuyenMoPhong] ADD CONSTRAINT [CK_VanChuyenMoPhong_SmartSwap] CHECK ([ChangTraoDoiId] IS NULL OR ([TenNguoiGui] IS NOT NULL AND [DienThoaiGui] IS NOT NULL AND [DiaChiGui] IS NOT NULL AND [TenNguoiNhan] IS NOT NULL AND [DienThoaiNhan] IS NOT NULL AND [DiaChiNhan] IS NOT NULL AND [TrangThai] IN (''ChoGui'',''DaGui'',''DangVanChuyen'',''DaGiao'') AND ([TrangThai]=''ChoGui'' OR [MaVanDonTuNhap] IS NOT NULL) AND ([NgayNhanDungHang] IS NULL OR [TrangThai]=''DaGiao'')))');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112080514_GiaoNhanSmartSwap'
)
BEGIN
    EXEC(N'ALTER TABLE [dbo].[VanChuyenMoPhong] ADD CONSTRAINT [CK_VanChuyenMoPhong_TrangThai] CHECK ([TrangThai] IN (N''ChoLayHang'', N''DaLayHang'', N''DangVanChuyen'', N''DaGiao'', N''GiaoThatBai'', N''ChoGui'', N''DaGui''))');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112080514_GiaoNhanSmartSwap'
)
BEGIN
    EXEC(N'ALTER TABLE [dbo].[LichSuVanChuyen] ADD CONSTRAINT [CK_LichSuVanChuyen_TrangThai] CHECK ([TrangThai] IN (N''ChoLayHang'', N''DaLayHang'', N''DangVanChuyen'', N''DaGiao'', N''GiaoThatBai'', N''ChoGui'', N''DaGui''))');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112080514_GiaoNhanSmartSwap'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_KhieuNai_NguoiGuiId_MaYeuCau] ON [dbo].[KhieuNai] ([NguoiGuiId], [MaYeuCau]) WHERE [MaYeuCau] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112080514_GiaoNhanSmartSwap'
)
BEGIN
    CREATE INDEX [IX_KhieuNai_VanChuyenMoPhongId] ON [dbo].[KhieuNai] ([VanChuyenMoPhongId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112080514_GiaoNhanSmartSwap'
)
BEGIN
    EXEC(N'ALTER TABLE [dbo].[KhieuNai] ADD CONSTRAINT [CK_KhieuNai_SuCoSmartSwap] CHECK (([VanChuyenMoPhongId] IS NULL AND [LoaiSuCo] IS NULL) OR ([VanChuyenMoPhongId] IS NOT NULL AND [LoaiSuCo] IN (''SaiHang'',''ChuaNhanHang'')))');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112080514_GiaoNhanSmartSwap'
)
BEGIN
    CREATE INDEX [IX_ThongTinGiaoNhanSmartSwap_PhienBanThoaThuanId] ON [ThongTinGiaoNhanSmartSwap] ([PhienBanThoaThuanId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112080514_GiaoNhanSmartSwap'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ThongTinGiaoNhanSmartSwap_ThanhVienVongId] ON [ThongTinGiaoNhanSmartSwap] ([ThanhVienVongId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112080514_GiaoNhanSmartSwap'
)
BEGIN
    ALTER TABLE [dbo].[KhieuNai] ADD CONSTRAINT [FK_KhieuNai_VanChuyenMoPhong_VanChuyenMoPhongId] FOREIGN KEY ([VanChuyenMoPhongId]) REFERENCES [dbo].[VanChuyenMoPhong] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112080514_GiaoNhanSmartSwap'
)
BEGIN
    ALTER TABLE [dbo].[VanChuyenMoPhong] ADD CONSTRAINT [FK_VanChuyenMoPhong_ChangTraoDoi_ChangTraoDoiId] FOREIGN KEY ([ChangTraoDoiId]) REFERENCES [dbo].[ChangTraoDoi] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112080514_GiaoNhanSmartSwap'
)
BEGIN
    ALTER TABLE [dbo].[VanChuyenMoPhong] ADD CONSTRAINT [FK_VanChuyenMoPhong_PhienBanThoaThuan_PhienBanThoaThuanId] FOREIGN KEY ([PhienBanThoaThuanId]) REFERENCES [dbo].[PhienBanThoaThuan] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112080514_GiaoNhanSmartSwap'
)
BEGIN
    EXEC(N'CREATE OR ALTER TRIGGER dbo.TR_SmartSwap_ThongTin ON dbo.ThongTinGiaoNhanSmartSwap AFTER INSERT,UPDATE AS
    BEGIN
      SET NOCOUNT ON;
      IF EXISTS(SELECT 1 FROM inserted i WHERE NOT EXISTS(SELECT 1 FROM dbo.ThanhVienVong m JOIN dbo.VongSmartSwap v ON v.Id=m.VongSmartSwapId
        JOIN dbo.GiaoDich g ON g.Id=v.GiaoDichId JOIN dbo.PhienBanThoaThuan p ON p.Id=i.PhienBanThoaThuanId AND p.GiaoDichId=g.Id
        WHERE m.Id=i.ThanhVienVongId AND v.NguoiKhoiTaoId IS NOT NULL AND v.TrangThai IN (''SanSangBanGiao'',''DangBanGiao'') AND g.TrangThai=''DangThucHien'' AND p.TrangThai=''DaChot''))
        THROW 51030,N''Thong tin giao nhan sai vong/ban thoa thuan hoac chua du dong y.'',1;
    END');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112080514_GiaoNhanSmartSwap'
)
BEGIN
    EXEC(N'CREATE OR ALTER TRIGGER dbo.TR_SmartSwap_VanChuyen ON dbo.VanChuyenMoPhong AFTER INSERT,UPDATE AS
    BEGIN
      SET NOCOUNT ON;
      IF EXISTS(SELECT 1 FROM inserted i WHERE i.ChangTraoDoiId IS NOT NULL AND NOT EXISTS(SELECT 1 FROM dbo.ChangTraoDoi c
        JOIN dbo.VongSmartSwap v ON v.Id=c.VongSmartSwapId JOIN dbo.GiaoDich g ON g.Id=v.GiaoDichId
        JOIN dbo.PhienBanThoaThuan p ON p.Id=i.PhienBanThoaThuanId AND p.GiaoDichId=g.Id
        WHERE c.Id=i.ChangTraoDoiId AND v.TrangThai IN (''SanSangBanGiao'',''DangBanGiao'') AND g.TrangThai=''DangThucHien'' AND p.TrangThai=''DaChot''))
        THROW 51031,N''Van chuyen Smart Swap chi hoat dong tren dung vong da chot va khong tam dung.'',1;
      IF EXISTS(SELECT 1 FROM inserted i JOIN deleted d ON i.Id=d.Id WHERE d.ChangTraoDoiId IS NOT NULL AND
        (ISNULL(i.ChangTraoDoiId,''00000000-0000-0000-0000-000000000000'')<>d.ChangTraoDoiId OR i.DonHangId IS NOT NULL OR i.PhienBanThoaThuanId<>d.PhienBanThoaThuanId
        OR i.TenNguoiGui<>d.TenNguoiGui OR i.DienThoaiGui<>d.DienThoaiGui OR i.DiaChiGui<>d.DiaChiGui
        OR i.TenNguoiNhan<>d.TenNguoiNhan OR i.DienThoaiNhan<>d.DienThoaiNhan OR i.DiaChiNhan<>d.DiaChiNhan
        OR (d.NgayNhanDungHang IS NOT NULL AND (i.NgayNhanDungHang IS NULL OR i.NgayNhanDungHang<>d.NgayNhanDungHang))))
        THROW 51031,N''Khong doi nguon/chang/dia chi hoac rut xac nhan da nhan.'',1;
      IF EXISTS(SELECT 1 FROM inserted i JOIN dbo.ChangTraoDoi c ON c.Id=i.ChangTraoDoiId JOIN dbo.VongSmartSwap v ON v.Id=c.VongSmartSwapId
        JOIN dbo.ThanhVienVong m ON m.Id=c.ThanhVienNhanId WHERE i.NgayNhanDungHang IS NOT NULL AND NOT EXISTS(SELECT 1 FROM dbo.XacNhanGiaoNhan x
        WHERE x.GiaoDichId=v.GiaoDichId AND x.PhienBanThoaThuanId=i.PhienBanThoaThuanId AND x.SanPhamId=c.SanPhamId AND x.NguoiXacNhanId=m.NguoiDungId AND x.Loai=''DaNhan''))
        THROW 51031,N''Nhan dung hang can xac nhan cua dung nguoi nhan.'',1;
    END');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112080514_GiaoNhanSmartSwap'
)
BEGIN
    EXEC(N'CREATE OR ALTER TRIGGER dbo.TR_SmartSwap_Receipt ON dbo.XacNhanGiaoNhan AFTER INSERT,UPDATE,DELETE AS
    BEGIN
      SET NOCOUNT ON;
      IF EXISTS(SELECT 1 FROM deleted d WHERE EXISTS(SELECT 1 FROM dbo.VongSmartSwap v JOIN dbo.ChangTraoDoi c ON c.VongSmartSwapId=v.Id
        JOIN dbo.VanChuyenMoPhong s ON s.ChangTraoDoiId=c.Id WHERE v.GiaoDichId=d.GiaoDichId))
        THROW 51032,N''Khong sua/xoa xac nhan giao nhan van don Smart Swap.'',1;
      IF EXISTS(SELECT 1 FROM inserted i WHERE EXISTS(SELECT 1 FROM dbo.VongSmartSwap v JOIN dbo.ChangTraoDoi c ON c.VongSmartSwapId=v.Id
        JOIN dbo.VanChuyenMoPhong s ON s.ChangTraoDoiId=c.Id WHERE v.GiaoDichId=i.GiaoDichId)
        AND NOT EXISTS(SELECT 1 FROM dbo.VongSmartSwap v JOIN dbo.GiaoDich g ON g.Id=v.GiaoDichId JOIN dbo.ChangTraoDoi c ON c.VongSmartSwapId=v.Id
        JOIN dbo.VanChuyenMoPhong s ON s.ChangTraoDoiId=c.Id JOIN dbo.ThanhVienVong a ON a.Id=c.ThanhVienGiaoId JOIN dbo.ThanhVienVong b ON b.Id=c.ThanhVienNhanId
        WHERE v.GiaoDichId=i.GiaoDichId AND g.TrangThai=''DangThucHien'' AND v.TrangThai=''DangBanGiao'' AND s.PhienBanThoaThuanId=i.PhienBanThoaThuanId AND c.SanPhamId=i.SanPhamId
        AND ((i.Loai=''DaGiao'' AND i.NguoiXacNhanId=a.NguoiDungId AND i.NguoiDoiTacId=b.NguoiDungId AND s.TrangThai IN (''DaGui'',''DangVanChuyen'',''DaGiao''))
        OR (i.Loai=''DaNhan'' AND i.NguoiXacNhanId=b.NguoiDungId AND i.NguoiDoiTacId=a.NguoiDungId AND s.TrangThai=''DaGiao''))
        AND NOT EXISTS(SELECT 1 FROM dbo.KhieuNai k WHERE k.GiaoDichId=g.Id AND k.VanChuyenMoPhongId IS NOT NULL AND k.TrangThai NOT IN (''DaGiaiQuyet'',''TuChoi''))))
        THROW 51032,N''Chua giao dung chang hoac vong dang co su co.'',1;
    END');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112080514_GiaoNhanSmartSwap'
)
BEGIN
    EXEC(N'CREATE OR ALTER TRIGGER dbo.TR_SmartSwap_HoanTat ON dbo.VongSmartSwap AFTER INSERT,UPDATE AS
    BEGIN
      SET NOCOUNT ON;
      IF EXISTS(SELECT 1 FROM inserted i WHERE i.TrangThai=''HoanTat'' AND EXISTS(SELECT 1 FROM dbo.ChangTraoDoi c JOIN dbo.VanChuyenMoPhong s ON s.ChangTraoDoiId=c.Id WHERE c.VongSmartSwapId=i.Id)
        AND ((SELECT COUNT(*) FROM dbo.ChangTraoDoi c JOIN dbo.VanChuyenMoPhong s ON s.ChangTraoDoiId=c.Id WHERE c.VongSmartSwapId=i.Id AND s.NgayNhanDungHang IS NOT NULL AND s.TrangThai=''DaGiao'')<>3
        OR EXISTS(SELECT 1 FROM dbo.KhieuNai k WHERE k.GiaoDichId=i.GiaoDichId AND k.VanChuyenMoPhongId IS NOT NULL AND k.TrangThai NOT IN (''DaGiaiQuyet'',''TuChoi''))
        OR NOT EXISTS(SELECT 1 FROM dbo.GiaoDich g WHERE g.Id=i.GiaoDichId AND g.TrangThai IN (''DangThucHien'',''HoanTat''))))
        THROW 51033,N''Chi hoan tat khi du3 nguoi nhan dung hang va khong co su co.'',1;
    END');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112080514_GiaoNhanSmartSwap'
)
BEGIN
    EXEC(N'CREATE OR ALTER TRIGGER dbo.TR_SmartSwap_SuCo ON dbo.KhieuNai AFTER INSERT,UPDATE AS
    BEGIN
      SET NOCOUNT ON;
      IF EXISTS(SELECT 1 FROM inserted i WHERE i.VanChuyenMoPhongId IS NOT NULL AND NOT EXISTS(SELECT 1 FROM dbo.VanChuyenMoPhong s
        JOIN dbo.ChangTraoDoi c ON c.Id=s.ChangTraoDoiId JOIN dbo.VongSmartSwap v ON v.Id=c.VongSmartSwapId
        JOIN dbo.ThanhVienVong a ON a.Id=c.ThanhVienGiaoId JOIN dbo.ThanhVienVong b ON b.Id=c.ThanhVienNhanId
        WHERE s.Id=i.VanChuyenMoPhongId AND v.GiaoDichId=i.GiaoDichId AND a.NguoiDungId=i.NguoiBiKhieuNaiId AND b.NguoiDungId=i.NguoiGuiId))
        THROW 51034,N''Su co phai thuoc dung van don va dung nguoi nhan.'',1;
    END');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112080514_GiaoNhanSmartSwap'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261112080514_GiaoNhanSmartSwap', N'8.0.33');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112143941_QuanTriDanhMucVaTranhChap'
)
BEGIN
    DROP INDEX [IX_QuyetDinhXuLy_KhieuNaiId] ON [dbo].[QuyetDinhXuLy];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112143941_QuanTriDanhMucVaTranhChap'
)
BEGIN
    ALTER TABLE [dbo].[QuyetDinhXuLy] ADD [BamYeuCau] nvarchar(64) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112143941_QuanTriDanhMucVaTranhChap'
)
BEGIN
    ALTER TABLE [dbo].[QuyetDinhXuLy] ADD [CanCu] nvarchar(2000) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112143941_QuanTriDanhMucVaTranhChap'
)
BEGIN
    ALTER TABLE [dbo].[QuyetDinhXuLy] ADD [HuongXuLy] nvarchar(20) NOT NULL DEFAULT N'GhiNhan';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112143941_QuanTriDanhMucVaTranhChap'
)
BEGIN
    ALTER TABLE [dbo].[QuyetDinhXuLy] ADD [MaYeuCau] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112143941_QuanTriDanhMucVaTranhChap'
)
BEGIN
    ALTER TABLE [dbo].[QuyetDinhXuLy] ADD [ThongBao] nvarchar(2000) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112143941_QuanTriDanhMucVaTranhChap'
)
BEGIN
    ALTER TABLE [dbo].[DanhMuc] ADD [BieuTuong] nvarchar(30) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112143941_QuanTriDanhMucVaTranhChap'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_QuyetDinhXuLy_KhieuNaiId_MaYeuCau] ON [dbo].[QuyetDinhXuLy] ([KhieuNaiId], [MaYeuCau]) WHERE [MaYeuCau] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112143941_QuanTriDanhMucVaTranhChap'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261112143941_QuanTriDanhMucVaTranhChap', N'8.0.33');
END;
GO

COMMIT;
GO

