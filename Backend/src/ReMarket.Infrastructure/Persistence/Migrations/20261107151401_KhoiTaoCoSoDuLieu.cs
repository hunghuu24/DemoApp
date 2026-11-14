using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReMarket.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class KhoiTaoCoSoDuLieu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "DanhMuc",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    PhienBan = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Ma = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    DanhMucChaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ThuTu = table.Column<int>(type: "int", nullable: false),
                    DangHienThi = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhMuc", x => x.Id);
                    table.CheckConstraint("CK_DanhMuc_KhongTuThamChieu", "[DanhMucChaId] IS NULL OR [DanhMucChaId] <> [Id]");
                    table.CheckConstraint("CK_DanhMuc_ThuTu", "[ThuTu] >= 0");
                    table.ForeignKey(
                        name: "FK_DanhMuc_DanhMuc_DanhMucChaId",
                        column: x => x.DanhMucChaId,
                        principalSchema: "dbo",
                        principalTable: "DanhMuc",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "GiaoDich",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    PhienBan = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Ma = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Loai = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    TrangThai = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    NgayHoanTat = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiaoDich", x => x.Id);
                    table.CheckConstraint("CK_GiaoDich_Loai", "[Loai] IN (N'MuaBan', N'TraoDoiMotMot', N'SmartSwap')");
                    table.CheckConstraint("CK_GiaoDich_NgayHoanTat", "([TrangThai] = N'HoanTat' AND [NgayHoanTat] IS NOT NULL) OR ([TrangThai] <> N'HoanTat' AND [NgayHoanTat] IS NULL)");
                    table.CheckConstraint("CK_GiaoDich_TrangThai", "[TrangThai] IN (N'KhoiTao', N'ChoDongY', N'DangThucHien', N'HoanTat', N'DaHuy', N'TranhChap')");
                });

            migrationBuilder.CreateTable(
                name: "NguoiDung",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    PhienBan = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    EmailChuanHoa = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false, computedColumnSql: "UPPER(LTRIM(RTRIM([Email])))", stored: true),
                    SoDienThoai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    MatKhauBam = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    TrangThai = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NguoiDung", x => x.Id);
                    table.CheckConstraint("CK_NguoiDung_Email", "LEN(LTRIM(RTRIM([Email]))) > 3");
                    table.CheckConstraint("CK_NguoiDung_TrangThai", "[TrangThai] IN (N'HoatDong', N'DaKhoa', N'DaXoa')");
                });

            migrationBuilder.CreateTable(
                name: "TieuChiDanhGia",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    PhienBan = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Ma = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TieuChiDanhGia", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VaiTro",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    PhienBan = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Ma = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VaiTro", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VongSmartSwap",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    PhienBan = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    GiaoDichId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrangThai = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    NgayHetHan = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VongSmartSwap", x => x.Id);
                    table.UniqueConstraint("AK_VongSmartSwap_Id_GiaoDichId", x => new { x.Id, x.GiaoDichId });
                    table.CheckConstraint("CK_VongSmartSwap_ThoiHan", "[NgayHetHan] > [NgayTao]");
                    table.CheckConstraint("CK_VongSmartSwap_TrangThai", "[TrangThai] IN (N'ChoDongY', N'SanSangBanGiao', N'DangBanGiao', N'HoanTat', N'DaHuy')");
                    table.ForeignKey(
                        name: "FK_VongSmartSwap_GiaoDich_GiaoDichId",
                        column: x => x.GiaoDichId,
                        principalSchema: "dbo",
                        principalTable: "GiaoDich",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "HoSoCaNhan",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    PhienBan = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    NguoiDungId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    AnhDaiDienUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TinhThanh = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DaXacMinhEmail = table.Column<bool>(type: "bit", nullable: false),
                    DaXacMinhDienThoai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoSoCaNhan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HoSoCaNhan_NguoiDung_NguoiDungId",
                        column: x => x.NguoiDungId,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "KhoaTaiKhoan",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    PhienBan = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    NguoiDungId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuanTriVienId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LyDo = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    NgayBatDau = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    NgayKetThuc = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: true),
                    NgayMoKhoa = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhoaTaiKhoan", x => x.Id);
                    table.CheckConstraint("CK_KhoaTaiKhoan_KhongTuKhoa", "[NguoiDungId] <> [QuanTriVienId]");
                    table.CheckConstraint("CK_KhoaTaiKhoan_ThoiHan", "([NgayKetThuc] IS NULL OR [NgayKetThuc] > [NgayBatDau]) AND ([NgayMoKhoa] IS NULL OR [NgayMoKhoa] >= [NgayBatDau])");
                    table.ForeignKey(
                        name: "FK_KhoaTaiKhoan_NguoiDung_NguoiDungId",
                        column: x => x.NguoiDungId,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_KhoaTaiKhoan_NguoiDung_QuanTriVienId",
                        column: x => x.QuanTriVienId,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "NhatKyQuanTri",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    PhienBan = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    NguoiThucHienId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HanhDong = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LoaiDoiTuong = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DoiTuongId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ChiTietJson = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhatKyQuanTri", x => x.Id);
                    table.CheckConstraint("CK_NhatKyQuanTri_Json", "ISJSON([ChiTietJson]) = 1");
                    table.ForeignKey(
                        name: "FK_NhatKyQuanTri_NguoiDung_NguoiThucHienId",
                        column: x => x.NguoiThucHienId,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SanPham",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    PhienBan = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    ChuSoHuuId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DanhMucId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    TinhTrang = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    GiaTriThamKhao = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SanPham", x => x.Id);
                    table.UniqueConstraint("AK_SanPham_Id_ChuSoHuuId", x => new { x.Id, x.ChuSoHuuId });
                    table.CheckConstraint("CK_SanPham_GiaTri", "[GiaTriThamKhao] >= 0");
                    table.CheckConstraint("CK_SanPham_TinhTrang", "[TinhTrang] IN (N'NhuMoi', N'DaSuDungTot', N'CoLoiNhe')");
                    table.ForeignKey(
                        name: "FK_SanPham_DanhMuc_DanhMucId",
                        column: x => x.DanhMucId,
                        principalSchema: "dbo",
                        principalTable: "DanhMuc",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SanPham_NguoiDung_ChuSoHuuId",
                        column: x => x.ChuSoHuuId,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ThanhVienGiaoDich",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    PhienBan = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    GiaoDichId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NguoiDungId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VaiTroHienThi = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThanhVienGiaoDich", x => x.Id);
                    table.UniqueConstraint("AK_ThanhVienGiaoDich_GiaoDichId_NguoiDungId", x => new { x.GiaoDichId, x.NguoiDungId });
                    table.ForeignKey(
                        name: "FK_ThanhVienGiaoDich_GiaoDich_GiaoDichId",
                        column: x => x.GiaoDichId,
                        principalSchema: "dbo",
                        principalTable: "GiaoDich",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ThanhVienGiaoDich_NguoiDung_NguoiDungId",
                        column: x => x.NguoiDungId,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ThongBao",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    PhienBan = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    NguoiDungId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GiaoDichId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Loai = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    TieuDe = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    DaDoc = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThongBao", x => x.Id);
                    table.CheckConstraint("CK_ThongBao_Loai", "[Loai] IN (N'HeThong', N'TinDang', N'DonHang', N'TraoDoi', N'TinNhan', N'KhieuNai')");
                    table.ForeignKey(
                        name: "FK_ThongBao_GiaoDich_GiaoDichId",
                        column: x => x.GiaoDichId,
                        principalSchema: "dbo",
                        principalTable: "GiaoDich",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ThongBao_NguoiDung_NguoiDungId",
                        column: x => x.NguoiDungId,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "NguoiDungVaiTro",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    PhienBan = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    NguoiDungId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VaiTroId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NguoiDungVaiTro", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NguoiDungVaiTro_NguoiDung_NguoiDungId",
                        column: x => x.NguoiDungId,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_NguoiDungVaiTro_VaiTro_VaiTroId",
                        column: x => x.VaiTroId,
                        principalSchema: "dbo",
                        principalTable: "VaiTro",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AnhSanPham",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    PhienBan = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    SanPhamId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DuongDan = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ThuTu = table.Column<int>(type: "int", nullable: false),
                    LaAnhBia = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnhSanPham", x => x.Id);
                    table.CheckConstraint("CK_AnhSanPham_ThuTu", "[ThuTu] >= 0");
                    table.ForeignKey(
                        name: "FK_AnhSanPham_SanPham_SanPhamId",
                        column: x => x.SanPhamId,
                        principalSchema: "dbo",
                        principalTable: "SanPham",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "GiuChoSanPham",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    PhienBan = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    SanPhamId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GiaoDichId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NgayHetHan = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    DaGiaiPhong = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiuChoSanPham", x => x.Id);
                    table.CheckConstraint("CK_GiuChoSanPham_ThoiHan", "[NgayHetHan] > [NgayTao]");
                    table.ForeignKey(
                        name: "FK_GiuChoSanPham_GiaoDich_GiaoDichId",
                        column: x => x.GiaoDichId,
                        principalSchema: "dbo",
                        principalTable: "GiaoDich",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GiuChoSanPham_SanPham_SanPhamId",
                        column: x => x.SanPhamId,
                        principalSchema: "dbo",
                        principalTable: "SanPham",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TinDang",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    PhienBan = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    SanPhamId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NguoiDangId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoaiTin = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    GiaBan = table.Column<long>(type: "bigint", nullable: true),
                    TrangThai = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    TinhThanh = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DiaDiem = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    NgayDang = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: true),
                    NguoiDuyetId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LyDoTuChoi = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TinDang", x => x.Id);
                    table.UniqueConstraint("AK_TinDang_Id_SanPhamId", x => new { x.Id, x.SanPhamId });
                    table.CheckConstraint("CK_TinDang_GiaBan", "([LoaiTin] = N'TraoDoi' AND ([GiaBan] IS NULL OR [GiaBan] >= 0)) OR ([LoaiTin] IN (N'MuaBan', N'CaHai') AND [GiaBan] IS NOT NULL AND [GiaBan] > 0)");
                    table.CheckConstraint("CK_TinDang_LoaiTin", "[LoaiTin] IN (N'MuaBan', N'TraoDoi', N'CaHai')");
                    table.CheckConstraint("CK_TinDang_TrangThai", "[TrangThai] IN (N'BanNhap', N'ChoDuyet', N'DangHienThi', N'TuChoi', N'DaAn', N'DaBan')");
                    table.ForeignKey(
                        name: "FK_TinDang_NguoiDung_NguoiDuyetId",
                        column: x => x.NguoiDuyetId,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TinDang_SanPham_SanPhamId_NguoiDangId",
                        columns: x => new { x.SanPhamId, x.NguoiDangId },
                        principalSchema: "dbo",
                        principalTable: "SanPham",
                        principalColumns: new[] { "Id", "ChuSoHuuId" });
                });

            migrationBuilder.CreateTable(
                name: "DanhGia",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    PhienBan = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    GiaoDichId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NguoiDanhGiaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NguoiDuocDanhGiaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SoSao = table.Column<int>(type: "int", nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhGia", x => x.Id);
                    table.CheckConstraint("CK_DanhGia_SoSaoVaNguoi", "[SoSao] BETWEEN 1 AND 5 AND [NguoiDanhGiaId] <> [NguoiDuocDanhGiaId]");
                    table.ForeignKey(
                        name: "FK_DanhGia_GiaoDich_GiaoDichId",
                        column: x => x.GiaoDichId,
                        principalSchema: "dbo",
                        principalTable: "GiaoDich",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DanhGia_ThanhVienGiaoDich_GiaoDichId_NguoiDanhGiaId",
                        columns: x => new { x.GiaoDichId, x.NguoiDanhGiaId },
                        principalSchema: "dbo",
                        principalTable: "ThanhVienGiaoDich",
                        principalColumns: new[] { "GiaoDichId", "NguoiDungId" });
                    table.ForeignKey(
                        name: "FK_DanhGia_ThanhVienGiaoDich_GiaoDichId_NguoiDuocDanhGiaId",
                        columns: x => new { x.GiaoDichId, x.NguoiDuocDanhGiaId },
                        principalSchema: "dbo",
                        principalTable: "ThanhVienGiaoDich",
                        principalColumns: new[] { "GiaoDichId", "NguoiDungId" });
                });

            migrationBuilder.CreateTable(
                name: "DeNghiTraoDoi",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    PhienBan = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    GiaoDichId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NguoiGuiId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NguoiNhanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SanPhamGuiId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SanPhamNhanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrangThai = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    NgayHetHan = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeNghiTraoDoi", x => x.Id);
                    table.CheckConstraint("CK_DeNghiTraoDoi_KhacNguoiVaMon", "[NguoiGuiId] <> [NguoiNhanId] AND [SanPhamGuiId] <> [SanPhamNhanId]");
                    table.CheckConstraint("CK_DeNghiTraoDoi_ThoiHan", "[NgayHetHan] > [NgayTao]");
                    table.CheckConstraint("CK_DeNghiTraoDoi_TrangThai", "[TrangThai] IN (N'ChoPhanHoi', N'DeXuatLai', N'DaChapNhan', N'TuChoi', N'DaHuy', N'HetHan')");
                    table.ForeignKey(
                        name: "FK_DeNghiTraoDoi_GiaoDich_GiaoDichId",
                        column: x => x.GiaoDichId,
                        principalSchema: "dbo",
                        principalTable: "GiaoDich",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DeNghiTraoDoi_SanPham_SanPhamGuiId_NguoiGuiId",
                        columns: x => new { x.SanPhamGuiId, x.NguoiGuiId },
                        principalSchema: "dbo",
                        principalTable: "SanPham",
                        principalColumns: new[] { "Id", "ChuSoHuuId" });
                    table.ForeignKey(
                        name: "FK_DeNghiTraoDoi_SanPham_SanPhamNhanId_NguoiNhanId",
                        columns: x => new { x.SanPhamNhanId, x.NguoiNhanId },
                        principalSchema: "dbo",
                        principalTable: "SanPham",
                        principalColumns: new[] { "Id", "ChuSoHuuId" });
                    table.ForeignKey(
                        name: "FK_DeNghiTraoDoi_ThanhVienGiaoDich_GiaoDichId_NguoiGuiId",
                        columns: x => new { x.GiaoDichId, x.NguoiGuiId },
                        principalSchema: "dbo",
                        principalTable: "ThanhVienGiaoDich",
                        principalColumns: new[] { "GiaoDichId", "NguoiDungId" });
                    table.ForeignKey(
                        name: "FK_DeNghiTraoDoi_ThanhVienGiaoDich_GiaoDichId_NguoiNhanId",
                        columns: x => new { x.GiaoDichId, x.NguoiNhanId },
                        principalSchema: "dbo",
                        principalTable: "ThanhVienGiaoDich",
                        principalColumns: new[] { "GiaoDichId", "NguoiDungId" });
                });

            migrationBuilder.CreateTable(
                name: "DonHang",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    PhienBan = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    GiaoDichId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NguoiMuaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NguoiBanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenNguoiNhan = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    SoDienThoaiNhan = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DiaChiNhan = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TienHang = table.Column<long>(type: "bigint", nullable: false),
                    PhiVanChuyen = table.Column<long>(type: "bigint", nullable: false),
                    TongTien = table.Column<long>(type: "bigint", nullable: false),
                    TrangThai = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonHang", x => x.Id);
                    table.CheckConstraint("CK_DonHang_KhacNguoi", "[NguoiMuaId] <> [NguoiBanId]");
                    table.CheckConstraint("CK_DonHang_TongTien", "[TienHang] > 0 AND [PhiVanChuyen] >= 0 AND [TongTien] = [TienHang] + [PhiVanChuyen]");
                    table.CheckConstraint("CK_DonHang_TrangThai", "[TrangThai] IN (N'ChoThanhToan', N'DaThanhToan', N'DangGiao', N'DaNhan', N'DaHuy')");
                    table.ForeignKey(
                        name: "FK_DonHang_GiaoDich_GiaoDichId",
                        column: x => x.GiaoDichId,
                        principalSchema: "dbo",
                        principalTable: "GiaoDich",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DonHang_ThanhVienGiaoDich_GiaoDichId_NguoiBanId",
                        columns: x => new { x.GiaoDichId, x.NguoiBanId },
                        principalSchema: "dbo",
                        principalTable: "ThanhVienGiaoDich",
                        principalColumns: new[] { "GiaoDichId", "NguoiDungId" });
                    table.ForeignKey(
                        name: "FK_DonHang_ThanhVienGiaoDich_GiaoDichId_NguoiMuaId",
                        columns: x => new { x.GiaoDichId, x.NguoiMuaId },
                        principalSchema: "dbo",
                        principalTable: "ThanhVienGiaoDich",
                        principalColumns: new[] { "GiaoDichId", "NguoiDungId" });
                });

            migrationBuilder.CreateTable(
                name: "KhieuNai",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    PhienBan = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Ma = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    GiaoDichId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NguoiGuiId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NguoiBiKhieuNaiId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TieuDe = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    TrangThai = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    HanPhanHoi = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhieuNai", x => x.Id);
                    table.CheckConstraint("CK_KhieuNai_KhacNguoi", "[NguoiGuiId] <> [NguoiBiKhieuNaiId]");
                    table.CheckConstraint("CK_KhieuNai_TrangThai", "[TrangThai] IN (N'Moi', N'DangXuLy', N'ChoThucHien', N'DaGiaiQuyet', N'TuChoi')");
                    table.ForeignKey(
                        name: "FK_KhieuNai_GiaoDich_GiaoDichId",
                        column: x => x.GiaoDichId,
                        principalSchema: "dbo",
                        principalTable: "GiaoDich",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_KhieuNai_ThanhVienGiaoDich_GiaoDichId_NguoiBiKhieuNaiId",
                        columns: x => new { x.GiaoDichId, x.NguoiBiKhieuNaiId },
                        principalSchema: "dbo",
                        principalTable: "ThanhVienGiaoDich",
                        principalColumns: new[] { "GiaoDichId", "NguoiDungId" });
                    table.ForeignKey(
                        name: "FK_KhieuNai_ThanhVienGiaoDich_GiaoDichId_NguoiGuiId",
                        columns: x => new { x.GiaoDichId, x.NguoiGuiId },
                        principalSchema: "dbo",
                        principalTable: "ThanhVienGiaoDich",
                        principalColumns: new[] { "GiaoDichId", "NguoiDungId" });
                });

            migrationBuilder.CreateTable(
                name: "PhienBanThoaThuan",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    PhienBan = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    GiaoDichId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SoPhienBan = table.Column<int>(type: "int", nullable: false),
                    NguoiTaoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    DiaDiemHen = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ThoiGianHen = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: true),
                    TrangThai = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhienBanThoaThuan", x => x.Id);
                    table.UniqueConstraint("AK_PhienBanThoaThuan_Id_GiaoDichId", x => new { x.Id, x.GiaoDichId });
                    table.CheckConstraint("CK_PhienBanThoaThuan_PhienBan", "[SoPhienBan] > 0");
                    table.CheckConstraint("CK_PhienBanThoaThuan_TrangThai", "[TrangThai] IN (N'DeXuat', N'DaChot', N'DaThayThe')");
                    table.ForeignKey(
                        name: "FK_PhienBanThoaThuan_GiaoDich_GiaoDichId",
                        column: x => x.GiaoDichId,
                        principalSchema: "dbo",
                        principalTable: "GiaoDich",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PhienBanThoaThuan_ThanhVienGiaoDich_GiaoDichId_NguoiTaoId",
                        columns: x => new { x.GiaoDichId, x.NguoiTaoId },
                        principalSchema: "dbo",
                        principalTable: "ThanhVienGiaoDich",
                        principalColumns: new[] { "GiaoDichId", "NguoiDungId" });
                });

            migrationBuilder.CreateTable(
                name: "ThanhVienVong",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    PhienBan = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    VongSmartSwapId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GiaoDichId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NguoiDungId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SanPhamId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ViTri = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThanhVienVong", x => x.Id);
                    table.UniqueConstraint("AK_ThanhVienVong_Id_VongSmartSwapId", x => new { x.Id, x.VongSmartSwapId });
                    table.UniqueConstraint("AK_ThanhVienVong_Id_VongSmartSwapId_SanPhamId", x => new { x.Id, x.VongSmartSwapId, x.SanPhamId });
                    table.CheckConstraint("CK_ThanhVienVong_ViTri", "[ViTri] BETWEEN 1 AND 3");
                    table.ForeignKey(
                        name: "FK_ThanhVienVong_SanPham_SanPhamId_NguoiDungId",
                        columns: x => new { x.SanPhamId, x.NguoiDungId },
                        principalSchema: "dbo",
                        principalTable: "SanPham",
                        principalColumns: new[] { "Id", "ChuSoHuuId" });
                    table.ForeignKey(
                        name: "FK_ThanhVienVong_ThanhVienGiaoDich_GiaoDichId_NguoiDungId",
                        columns: x => new { x.GiaoDichId, x.NguoiDungId },
                        principalSchema: "dbo",
                        principalTable: "ThanhVienGiaoDich",
                        principalColumns: new[] { "GiaoDichId", "NguoiDungId" });
                    table.ForeignKey(
                        name: "FK_ThanhVienVong_VongSmartSwap_VongSmartSwapId_GiaoDichId",
                        columns: x => new { x.VongSmartSwapId, x.GiaoDichId },
                        principalSchema: "dbo",
                        principalTable: "VongSmartSwap",
                        principalColumns: new[] { "Id", "GiaoDichId" });
                });

            migrationBuilder.CreateTable(
                name: "CuocTroChuyen",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    PhienBan = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    GiaoDichId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TinDangId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Loai = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    TieuDe = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuocTroChuyen", x => x.Id);
                    table.CheckConstraint("CK_CuocTroChuyen_Loai", "[Loai] IN (N'TrucTiep', N'NhomSmartSwap')");
                    table.ForeignKey(
                        name: "FK_CuocTroChuyen_GiaoDich_GiaoDichId",
                        column: x => x.GiaoDichId,
                        principalSchema: "dbo",
                        principalTable: "GiaoDich",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CuocTroChuyen_TinDang_TinDangId",
                        column: x => x.TinDangId,
                        principalSchema: "dbo",
                        principalTable: "TinDang",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "NhatKyKiemDuyet",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    PhienBan = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    TinDangId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuanTriVienId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrangThaiTruoc = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    TrangThaiSau = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    LyDo = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhatKyKiemDuyet", x => x.Id);
                    table.CheckConstraint("CK_NhatKyKiemDuyet_TrangThaiSau", "[TrangThaiSau] IN (N'BanNhap', N'ChoDuyet', N'DangHienThi', N'TuChoi', N'DaAn', N'DaBan')");
                    table.CheckConstraint("CK_NhatKyKiemDuyet_TrangThaiTruoc", "[TrangThaiTruoc] IN (N'BanNhap', N'ChoDuyet', N'DangHienThi', N'TuChoi', N'DaAn', N'DaBan')");
                    table.ForeignKey(
                        name: "FK_NhatKyKiemDuyet_NguoiDung_QuanTriVienId",
                        column: x => x.QuanTriVienId,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_NhatKyKiemDuyet_TinDang_TinDangId",
                        column: x => x.TinDangId,
                        principalSchema: "dbo",
                        principalTable: "TinDang",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "NhuCauTraoDoi",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    PhienBan = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    TinDangId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MoTaMongMuon = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    DanhMucMongMuonId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ChoPhepBuTien = table.Column<bool>(type: "bit", nullable: false),
                    SoTienBuToiDa = table.Column<long>(type: "bigint", nullable: false),
                    ChoPhepSmartSwap = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhuCauTraoDoi", x => x.Id);
                    table.CheckConstraint("CK_NhuCauTraoDoi_TienBu", "[SoTienBuToiDa] >= 0 AND ([ChoPhepBuTien] = 1 OR [SoTienBuToiDa] = 0)");
                    table.ForeignKey(
                        name: "FK_NhuCauTraoDoi_DanhMuc_DanhMucMongMuonId",
                        column: x => x.DanhMucMongMuonId,
                        principalSchema: "dbo",
                        principalTable: "DanhMuc",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_NhuCauTraoDoi_TinDang_TinDangId",
                        column: x => x.TinDangId,
                        principalSchema: "dbo",
                        principalTable: "TinDang",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DanhGiaTieuChi",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    PhienBan = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    DanhGiaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TieuChiDanhGiaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhGiaTieuChi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DanhGiaTieuChi_DanhGia_DanhGiaId",
                        column: x => x.DanhGiaId,
                        principalSchema: "dbo",
                        principalTable: "DanhGia",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DanhGiaTieuChi_TieuChiDanhGia_TieuChiDanhGiaId",
                        column: x => x.TieuChiDanhGiaId,
                        principalSchema: "dbo",
                        principalTable: "TieuChiDanhGia",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ChiTietDonHang",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    PhienBan = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    DonHangId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TinDangId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SanPhamId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenSanPham = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DonGia = table.Column<long>(type: "bigint", nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietDonHang", x => x.Id);
                    table.CheckConstraint("CK_ChiTietDonHang_GiaSoLuong", "[DonGia] > 0 AND [SoLuong] = 1");
                    table.ForeignKey(
                        name: "FK_ChiTietDonHang_DonHang_DonHangId",
                        column: x => x.DonHangId,
                        principalSchema: "dbo",
                        principalTable: "DonHang",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ChiTietDonHang_TinDang_TinDangId_SanPhamId",
                        columns: x => new { x.TinDangId, x.SanPhamId },
                        principalSchema: "dbo",
                        principalTable: "TinDang",
                        principalColumns: new[] { "Id", "SanPhamId" });
                });

            migrationBuilder.CreateTable(
                name: "ThanhToan",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    PhienBan = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    DonHangId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SoTien = table.Column<long>(type: "bigint", nullable: false),
                    TrangThai = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    LaMoPhong = table.Column<bool>(type: "bit", nullable: false),
                    MaThamChieu = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThanhToan", x => x.Id);
                    table.CheckConstraint("CK_ThanhToan_SoTien", "[SoTien] > 0");
                    table.CheckConstraint("CK_ThanhToan_TrangThai", "[TrangThai] IN (N'ChoXuLy', N'ThanhCong', N'ThatBai', N'HoanTien')");
                    table.ForeignKey(
                        name: "FK_ThanhToan_DonHang_DonHangId",
                        column: x => x.DonHangId,
                        principalSchema: "dbo",
                        principalTable: "DonHang",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "VanChuyenMoPhong",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    PhienBan = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    DonHangId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaVanDon = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    TrangThai = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    NgayGiaoDuKien = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VanChuyenMoPhong", x => x.Id);
                    table.CheckConstraint("CK_VanChuyenMoPhong_TrangThai", "[TrangThai] IN (N'ChoLayHang', N'DaLayHang', N'DangVanChuyen', N'DaGiao', N'GiaoThatBai')");
                    table.ForeignKey(
                        name: "FK_VanChuyenMoPhong_DonHang_DonHangId",
                        column: x => x.DonHangId,
                        principalSchema: "dbo",
                        principalTable: "DonHang",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "YeuCauHoTro",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    PhienBan = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Ma = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    NguoiGuiId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DonHangId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Loai = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    TieuDe = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    TrangThai = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YeuCauHoTro", x => x.Id);
                    table.CheckConstraint("CK_YeuCauHoTro_Loai", "[Loai] IN (N'DonHangVanChuyen', N'ThanhToanHoanTien', N'TaiKhoanKhac')");
                    table.CheckConstraint("CK_YeuCauHoTro_TrangThai", "[TrangThai] IN (N'Moi', N'DangXuLy', N'DaGiaiQuyet')");
                    table.ForeignKey(
                        name: "FK_YeuCauHoTro_DonHang_DonHangId",
                        column: x => x.DonHangId,
                        principalSchema: "dbo",
                        principalTable: "DonHang",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_YeuCauHoTro_NguoiDung_NguoiGuiId",
                        column: x => x.NguoiGuiId,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BangChungKhieuNai",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    PhienBan = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    KhieuNaiId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NguoiTaiLenId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DuongDan = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    LoaiMime = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BangChungKhieuNai", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BangChungKhieuNai_KhieuNai_KhieuNaiId",
                        column: x => x.KhieuNaiId,
                        principalSchema: "dbo",
                        principalTable: "KhieuNai",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BangChungKhieuNai_NguoiDung_NguoiTaiLenId",
                        column: x => x.NguoiTaiLenId,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PhanHoiKhieuNai",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    PhienBan = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    KhieuNaiId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NguoiGuiId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhanHoiKhieuNai", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhanHoiKhieuNai_KhieuNai_KhieuNaiId",
                        column: x => x.KhieuNaiId,
                        principalSchema: "dbo",
                        principalTable: "KhieuNai",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PhanHoiKhieuNai_NguoiDung_NguoiGuiId",
                        column: x => x.NguoiGuiId,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "QuyetDinhXuLy",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    PhienBan = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    KhieuNaiId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuanTriVienId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KetLuan = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    BienPhap = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    DaXacMinhThucHien = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuyetDinhXuLy", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuyetDinhXuLy_KhieuNai_KhieuNaiId",
                        column: x => x.KhieuNaiId,
                        principalSchema: "dbo",
                        principalTable: "KhieuNai",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_QuyetDinhXuLy_NguoiDung_QuanTriVienId",
                        column: x => x.QuanTriVienId,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DongYThoaThuan",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    PhienBan = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    PhienBanThoaThuanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GiaoDichId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NguoiDungId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuyetDinh = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    NgayPhanHoi = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DongYThoaThuan", x => x.Id);
                    table.CheckConstraint("CK_DongYThoaThuan_NgayPhanHoi", "([QuyetDinh] = N'ChoPhanHoi' AND [NgayPhanHoi] IS NULL) OR ([QuyetDinh] <> N'ChoPhanHoi' AND [NgayPhanHoi] IS NOT NULL)");
                    table.CheckConstraint("CK_DongYThoaThuan_QuyetDinh", "[QuyetDinh] IN (N'ChoPhanHoi', N'DongY', N'TuChoi')");
                    table.ForeignKey(
                        name: "FK_DongYThoaThuan_PhienBanThoaThuan_PhienBanThoaThuanId_GiaoDichId",
                        columns: x => new { x.PhienBanThoaThuanId, x.GiaoDichId },
                        principalSchema: "dbo",
                        principalTable: "PhienBanThoaThuan",
                        principalColumns: new[] { "Id", "GiaoDichId" });
                    table.ForeignKey(
                        name: "FK_DongYThoaThuan_ThanhVienGiaoDich_GiaoDichId_NguoiDungId",
                        columns: x => new { x.GiaoDichId, x.NguoiDungId },
                        principalSchema: "dbo",
                        principalTable: "ThanhVienGiaoDich",
                        principalColumns: new[] { "GiaoDichId", "NguoiDungId" });
                });

            migrationBuilder.CreateTable(
                name: "TienBu",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    PhienBan = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    PhienBanThoaThuanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GiaoDichId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NguoiTraId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NguoiNhanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SoTien = table.Column<long>(type: "bigint", nullable: false),
                    NguoiTraDaXacNhan = table.Column<bool>(type: "bit", nullable: false),
                    NguoiNhanDaXacNhan = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TienBu", x => x.Id);
                    table.CheckConstraint("CK_TienBu_SoTienVaNguoi", "[SoTien] > 0 AND [NguoiTraId] <> [NguoiNhanId]");
                    table.ForeignKey(
                        name: "FK_TienBu_PhienBanThoaThuan_PhienBanThoaThuanId_GiaoDichId",
                        columns: x => new { x.PhienBanThoaThuanId, x.GiaoDichId },
                        principalSchema: "dbo",
                        principalTable: "PhienBanThoaThuan",
                        principalColumns: new[] { "Id", "GiaoDichId" });
                    table.ForeignKey(
                        name: "FK_TienBu_ThanhVienGiaoDich_GiaoDichId_NguoiNhanId",
                        columns: x => new { x.GiaoDichId, x.NguoiNhanId },
                        principalSchema: "dbo",
                        principalTable: "ThanhVienGiaoDich",
                        principalColumns: new[] { "GiaoDichId", "NguoiDungId" });
                    table.ForeignKey(
                        name: "FK_TienBu_ThanhVienGiaoDich_GiaoDichId_NguoiTraId",
                        columns: x => new { x.GiaoDichId, x.NguoiTraId },
                        principalSchema: "dbo",
                        principalTable: "ThanhVienGiaoDich",
                        principalColumns: new[] { "GiaoDichId", "NguoiDungId" });
                });

            migrationBuilder.CreateTable(
                name: "XacNhanGiaoNhan",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    PhienBan = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    GiaoDichId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NguoiXacNhanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NguoiDoiTacId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SanPhamId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PhienBanThoaThuanId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Loai = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    NgayXacNhan = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_XacNhanGiaoNhan", x => x.Id);
                    table.CheckConstraint("CK_XacNhanGiaoNhan_KhacNguoi", "[NguoiXacNhanId] <> [NguoiDoiTacId]");
                    table.CheckConstraint("CK_XacNhanGiaoNhan_Loai", "[Loai] IN (N'DaGiao', N'DaNhan')");
                    table.ForeignKey(
                        name: "FK_XacNhanGiaoNhan_GiaoDich_GiaoDichId",
                        column: x => x.GiaoDichId,
                        principalSchema: "dbo",
                        principalTable: "GiaoDich",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_XacNhanGiaoNhan_PhienBanThoaThuan_PhienBanThoaThuanId_GiaoDichId",
                        columns: x => new { x.PhienBanThoaThuanId, x.GiaoDichId },
                        principalSchema: "dbo",
                        principalTable: "PhienBanThoaThuan",
                        principalColumns: new[] { "Id", "GiaoDichId" });
                    table.ForeignKey(
                        name: "FK_XacNhanGiaoNhan_SanPham_SanPhamId",
                        column: x => x.SanPhamId,
                        principalSchema: "dbo",
                        principalTable: "SanPham",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_XacNhanGiaoNhan_ThanhVienGiaoDich_GiaoDichId_NguoiDoiTacId",
                        columns: x => new { x.GiaoDichId, x.NguoiDoiTacId },
                        principalSchema: "dbo",
                        principalTable: "ThanhVienGiaoDich",
                        principalColumns: new[] { "GiaoDichId", "NguoiDungId" });
                    table.ForeignKey(
                        name: "FK_XacNhanGiaoNhan_ThanhVienGiaoDich_GiaoDichId_NguoiXacNhanId",
                        columns: x => new { x.GiaoDichId, x.NguoiXacNhanId },
                        principalSchema: "dbo",
                        principalTable: "ThanhVienGiaoDich",
                        principalColumns: new[] { "GiaoDichId", "NguoiDungId" });
                });

            migrationBuilder.CreateTable(
                name: "ChangTraoDoi",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    PhienBan = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    VongSmartSwapId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ThanhVienGiaoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ThanhVienNhanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SanPhamId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChangTraoDoi", x => x.Id);
                    table.CheckConstraint("CK_ChangTraoDoi_KhacThanhVien", "[ThanhVienGiaoId] <> [ThanhVienNhanId]");
                    table.ForeignKey(
                        name: "FK_ChangTraoDoi_ThanhVienVong_ThanhVienGiaoId_VongSmartSwapId_SanPhamId",
                        columns: x => new { x.ThanhVienGiaoId, x.VongSmartSwapId, x.SanPhamId },
                        principalSchema: "dbo",
                        principalTable: "ThanhVienVong",
                        principalColumns: new[] { "Id", "VongSmartSwapId", "SanPhamId" });
                    table.ForeignKey(
                        name: "FK_ChangTraoDoi_ThanhVienVong_ThanhVienNhanId_VongSmartSwapId",
                        columns: x => new { x.ThanhVienNhanId, x.VongSmartSwapId },
                        principalSchema: "dbo",
                        principalTable: "ThanhVienVong",
                        principalColumns: new[] { "Id", "VongSmartSwapId" });
                    table.ForeignKey(
                        name: "FK_ChangTraoDoi_VongSmartSwap_VongSmartSwapId",
                        column: x => x.VongSmartSwapId,
                        principalSchema: "dbo",
                        principalTable: "VongSmartSwap",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ThanhVienTroChuyen",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    PhienBan = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CuocTroChuyenId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NguoiDungId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LanDocCuoi = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThanhVienTroChuyen", x => x.Id);
                    table.UniqueConstraint("AK_ThanhVienTroChuyen_CuocTroChuyenId_NguoiDungId", x => new { x.CuocTroChuyenId, x.NguoiDungId });
                    table.ForeignKey(
                        name: "FK_ThanhVienTroChuyen_CuocTroChuyen_CuocTroChuyenId",
                        column: x => x.CuocTroChuyenId,
                        principalSchema: "dbo",
                        principalTable: "CuocTroChuyen",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ThanhVienTroChuyen_NguoiDung_NguoiDungId",
                        column: x => x.NguoiDungId,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LichSuVanChuyen",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    PhienBan = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    VanChuyenMoPhongId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrangThai = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ThoiDiem = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichSuVanChuyen", x => x.Id);
                    table.CheckConstraint("CK_LichSuVanChuyen_TrangThai", "[TrangThai] IN (N'ChoLayHang', N'DaLayHang', N'DangVanChuyen', N'DaGiao', N'GiaoThatBai')");
                    table.ForeignKey(
                        name: "FK_LichSuVanChuyen_VanChuyenMoPhong_VanChuyenMoPhongId",
                        column: x => x.VanChuyenMoPhongId,
                        principalSchema: "dbo",
                        principalTable: "VanChuyenMoPhong",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PhanHoiHoTro",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    PhienBan = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    YeuCauHoTroId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NguoiGuiId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhanHoiHoTro", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhanHoiHoTro_NguoiDung_NguoiGuiId",
                        column: x => x.NguoiGuiId,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PhanHoiHoTro_YeuCauHoTro_YeuCauHoTroId",
                        column: x => x.YeuCauHoTroId,
                        principalSchema: "dbo",
                        principalTable: "YeuCauHoTro",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BangChungGiaoNhan",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    PhienBan = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    XacNhanGiaoNhanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NguoiTaiLenId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DuongDan = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    LoaiMime = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BangChungGiaoNhan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BangChungGiaoNhan_NguoiDung_NguoiTaiLenId",
                        column: x => x.NguoiTaiLenId,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BangChungGiaoNhan_XacNhanGiaoNhan_XacNhanGiaoNhanId",
                        column: x => x.XacNhanGiaoNhanId,
                        principalSchema: "dbo",
                        principalTable: "XacNhanGiaoNhan",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TinNhan",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    PhienBan = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CuocTroChuyenId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NguoiGuiId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaPhiaClient = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Loai = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TinNhan", x => x.Id);
                    table.CheckConstraint("CK_TinNhan_Loai", "[Loai] IN (N'VanBan', N'HinhAnh', N'HeThong', N'DeNghiGia')");
                    table.CheckConstraint("CK_TinNhan_VanBan", "[Loai] <> N'VanBan' OR ([NoiDung] IS NOT NULL AND LEN(LTRIM(RTRIM([NoiDung]))) > 0)");
                    table.ForeignKey(
                        name: "FK_TinNhan_ThanhVienTroChuyen_CuocTroChuyenId_NguoiGuiId",
                        columns: x => new { x.CuocTroChuyenId, x.NguoiGuiId },
                        principalSchema: "dbo",
                        principalTable: "ThanhVienTroChuyen",
                        principalColumns: new[] { "CuocTroChuyenId", "NguoiDungId" });
                });

            migrationBuilder.CreateTable(
                name: "TepTinNhan",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    PhienBan = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    TinNhanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DuongDan = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    TenTep = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LoaiMime = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DungLuong = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TepTinNhan", x => x.Id);
                    table.CheckConstraint("CK_TepTinNhan_DungLuong", "[DungLuong] > 0");
                    table.ForeignKey(
                        name: "FK_TepTinNhan_TinNhan_TinNhanId",
                        column: x => x.TinNhanId,
                        principalSchema: "dbo",
                        principalTable: "TinNhan",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnhSanPham_SanPhamId",
                schema: "dbo",
                table: "AnhSanPham",
                column: "SanPhamId",
                unique: true,
                filter: "[LaAnhBia] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_AnhSanPham_SanPhamId_ThuTu",
                schema: "dbo",
                table: "AnhSanPham",
                columns: new[] { "SanPhamId", "ThuTu" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BangChungGiaoNhan_NguoiTaiLenId",
                schema: "dbo",
                table: "BangChungGiaoNhan",
                column: "NguoiTaiLenId");

            migrationBuilder.CreateIndex(
                name: "IX_BangChungGiaoNhan_XacNhanGiaoNhanId",
                schema: "dbo",
                table: "BangChungGiaoNhan",
                column: "XacNhanGiaoNhanId");

            migrationBuilder.CreateIndex(
                name: "IX_BangChungKhieuNai_KhieuNaiId",
                schema: "dbo",
                table: "BangChungKhieuNai",
                column: "KhieuNaiId");

            migrationBuilder.CreateIndex(
                name: "IX_BangChungKhieuNai_NguoiTaiLenId",
                schema: "dbo",
                table: "BangChungKhieuNai",
                column: "NguoiTaiLenId");

            migrationBuilder.CreateIndex(
                name: "IX_ChangTraoDoi_ThanhVienGiaoId_VongSmartSwapId_SanPhamId",
                schema: "dbo",
                table: "ChangTraoDoi",
                columns: new[] { "ThanhVienGiaoId", "VongSmartSwapId", "SanPhamId" });

            migrationBuilder.CreateIndex(
                name: "IX_ChangTraoDoi_ThanhVienNhanId_VongSmartSwapId",
                schema: "dbo",
                table: "ChangTraoDoi",
                columns: new[] { "ThanhVienNhanId", "VongSmartSwapId" });

            migrationBuilder.CreateIndex(
                name: "IX_ChangTraoDoi_VongSmartSwapId_ThanhVienGiaoId",
                schema: "dbo",
                table: "ChangTraoDoi",
                columns: new[] { "VongSmartSwapId", "ThanhVienGiaoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChangTraoDoi_VongSmartSwapId_ThanhVienNhanId",
                schema: "dbo",
                table: "ChangTraoDoi",
                columns: new[] { "VongSmartSwapId", "ThanhVienNhanId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDonHang_DonHangId_SanPhamId",
                schema: "dbo",
                table: "ChiTietDonHang",
                columns: new[] { "DonHangId", "SanPhamId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDonHang_TinDangId_SanPhamId",
                schema: "dbo",
                table: "ChiTietDonHang",
                columns: new[] { "TinDangId", "SanPhamId" });

            migrationBuilder.CreateIndex(
                name: "IX_CuocTroChuyen_GiaoDichId",
                schema: "dbo",
                table: "CuocTroChuyen",
                column: "GiaoDichId",
                unique: true,
                filter: "[GiaoDichId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CuocTroChuyen_TinDangId",
                schema: "dbo",
                table: "CuocTroChuyen",
                column: "TinDangId");

            migrationBuilder.CreateIndex(
                name: "IX_DanhGia_GiaoDichId_NguoiDanhGiaId_NguoiDuocDanhGiaId",
                schema: "dbo",
                table: "DanhGia",
                columns: new[] { "GiaoDichId", "NguoiDanhGiaId", "NguoiDuocDanhGiaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DanhGia_GiaoDichId_NguoiDuocDanhGiaId",
                schema: "dbo",
                table: "DanhGia",
                columns: new[] { "GiaoDichId", "NguoiDuocDanhGiaId" });

            migrationBuilder.CreateIndex(
                name: "IX_DanhGiaTieuChi_DanhGiaId_TieuChiDanhGiaId",
                schema: "dbo",
                table: "DanhGiaTieuChi",
                columns: new[] { "DanhGiaId", "TieuChiDanhGiaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DanhGiaTieuChi_TieuChiDanhGiaId",
                schema: "dbo",
                table: "DanhGiaTieuChi",
                column: "TieuChiDanhGiaId");

            migrationBuilder.CreateIndex(
                name: "IX_DanhMuc_DanhMucChaId",
                schema: "dbo",
                table: "DanhMuc",
                column: "DanhMucChaId");

            migrationBuilder.CreateIndex(
                name: "IX_DanhMuc_Ma",
                schema: "dbo",
                table: "DanhMuc",
                column: "Ma",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeNghiTraoDoi_GiaoDichId",
                schema: "dbo",
                table: "DeNghiTraoDoi",
                column: "GiaoDichId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeNghiTraoDoi_GiaoDichId_NguoiGuiId",
                schema: "dbo",
                table: "DeNghiTraoDoi",
                columns: new[] { "GiaoDichId", "NguoiGuiId" });

            migrationBuilder.CreateIndex(
                name: "IX_DeNghiTraoDoi_GiaoDichId_NguoiNhanId",
                schema: "dbo",
                table: "DeNghiTraoDoi",
                columns: new[] { "GiaoDichId", "NguoiNhanId" });

            migrationBuilder.CreateIndex(
                name: "IX_DeNghiTraoDoi_SanPhamGuiId_NguoiGuiId",
                schema: "dbo",
                table: "DeNghiTraoDoi",
                columns: new[] { "SanPhamGuiId", "NguoiGuiId" });

            migrationBuilder.CreateIndex(
                name: "IX_DeNghiTraoDoi_SanPhamNhanId_NguoiNhanId",
                schema: "dbo",
                table: "DeNghiTraoDoi",
                columns: new[] { "SanPhamNhanId", "NguoiNhanId" });

            migrationBuilder.CreateIndex(
                name: "IX_DongYThoaThuan_GiaoDichId_NguoiDungId",
                schema: "dbo",
                table: "DongYThoaThuan",
                columns: new[] { "GiaoDichId", "NguoiDungId" });

            migrationBuilder.CreateIndex(
                name: "IX_DongYThoaThuan_PhienBanThoaThuanId_GiaoDichId",
                schema: "dbo",
                table: "DongYThoaThuan",
                columns: new[] { "PhienBanThoaThuanId", "GiaoDichId" });

            migrationBuilder.CreateIndex(
                name: "IX_DongYThoaThuan_PhienBanThoaThuanId_NguoiDungId",
                schema: "dbo",
                table: "DongYThoaThuan",
                columns: new[] { "PhienBanThoaThuanId", "NguoiDungId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DonHang_GiaoDichId",
                schema: "dbo",
                table: "DonHang",
                column: "GiaoDichId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DonHang_GiaoDichId_NguoiBanId",
                schema: "dbo",
                table: "DonHang",
                columns: new[] { "GiaoDichId", "NguoiBanId" });

            migrationBuilder.CreateIndex(
                name: "IX_DonHang_GiaoDichId_NguoiMuaId",
                schema: "dbo",
                table: "DonHang",
                columns: new[] { "GiaoDichId", "NguoiMuaId" });

            migrationBuilder.CreateIndex(
                name: "IX_GiaoDich_Ma",
                schema: "dbo",
                table: "GiaoDich",
                column: "Ma",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GiuChoSanPham_GiaoDichId",
                schema: "dbo",
                table: "GiuChoSanPham",
                column: "GiaoDichId");

            migrationBuilder.CreateIndex(
                name: "IX_GiuChoSanPham_SanPhamId",
                schema: "dbo",
                table: "GiuChoSanPham",
                column: "SanPhamId",
                unique: true,
                filter: "[DaGiaiPhong] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_HoSoCaNhan_NguoiDungId",
                schema: "dbo",
                table: "HoSoCaNhan",
                column: "NguoiDungId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KhieuNai_GiaoDichId_NguoiBiKhieuNaiId",
                schema: "dbo",
                table: "KhieuNai",
                columns: new[] { "GiaoDichId", "NguoiBiKhieuNaiId" });

            migrationBuilder.CreateIndex(
                name: "IX_KhieuNai_GiaoDichId_NguoiGuiId",
                schema: "dbo",
                table: "KhieuNai",
                columns: new[] { "GiaoDichId", "NguoiGuiId" });

            migrationBuilder.CreateIndex(
                name: "IX_KhieuNai_Ma",
                schema: "dbo",
                table: "KhieuNai",
                column: "Ma",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KhoaTaiKhoan_NguoiDungId",
                schema: "dbo",
                table: "KhoaTaiKhoan",
                column: "NguoiDungId");

            migrationBuilder.CreateIndex(
                name: "IX_KhoaTaiKhoan_QuanTriVienId",
                schema: "dbo",
                table: "KhoaTaiKhoan",
                column: "QuanTriVienId");

            migrationBuilder.CreateIndex(
                name: "IX_LichSuVanChuyen_VanChuyenMoPhongId_ThoiDiem",
                schema: "dbo",
                table: "LichSuVanChuyen",
                columns: new[] { "VanChuyenMoPhongId", "ThoiDiem" });

            migrationBuilder.CreateIndex(
                name: "IX_NguoiDung_EmailChuanHoa",
                schema: "dbo",
                table: "NguoiDung",
                column: "EmailChuanHoa",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NguoiDung_SoDienThoai",
                schema: "dbo",
                table: "NguoiDung",
                column: "SoDienThoai",
                unique: true,
                filter: "[SoDienThoai] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_NguoiDungVaiTro_NguoiDungId_VaiTroId",
                schema: "dbo",
                table: "NguoiDungVaiTro",
                columns: new[] { "NguoiDungId", "VaiTroId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NguoiDungVaiTro_VaiTroId",
                schema: "dbo",
                table: "NguoiDungVaiTro",
                column: "VaiTroId");

            migrationBuilder.CreateIndex(
                name: "IX_NhatKyKiemDuyet_QuanTriVienId",
                schema: "dbo",
                table: "NhatKyKiemDuyet",
                column: "QuanTriVienId");

            migrationBuilder.CreateIndex(
                name: "IX_NhatKyKiemDuyet_TinDangId",
                schema: "dbo",
                table: "NhatKyKiemDuyet",
                column: "TinDangId");

            migrationBuilder.CreateIndex(
                name: "IX_NhatKyQuanTri_LoaiDoiTuong_DoiTuongId_NgayTao",
                schema: "dbo",
                table: "NhatKyQuanTri",
                columns: new[] { "LoaiDoiTuong", "DoiTuongId", "NgayTao" });

            migrationBuilder.CreateIndex(
                name: "IX_NhatKyQuanTri_NguoiThucHienId",
                schema: "dbo",
                table: "NhatKyQuanTri",
                column: "NguoiThucHienId");

            migrationBuilder.CreateIndex(
                name: "IX_NhuCauTraoDoi_DanhMucMongMuonId",
                schema: "dbo",
                table: "NhuCauTraoDoi",
                column: "DanhMucMongMuonId");

            migrationBuilder.CreateIndex(
                name: "IX_NhuCauTraoDoi_TinDangId",
                schema: "dbo",
                table: "NhuCauTraoDoi",
                column: "TinDangId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PhanHoiHoTro_NguoiGuiId",
                schema: "dbo",
                table: "PhanHoiHoTro",
                column: "NguoiGuiId");

            migrationBuilder.CreateIndex(
                name: "IX_PhanHoiHoTro_YeuCauHoTroId",
                schema: "dbo",
                table: "PhanHoiHoTro",
                column: "YeuCauHoTroId");

            migrationBuilder.CreateIndex(
                name: "IX_PhanHoiKhieuNai_KhieuNaiId",
                schema: "dbo",
                table: "PhanHoiKhieuNai",
                column: "KhieuNaiId");

            migrationBuilder.CreateIndex(
                name: "IX_PhanHoiKhieuNai_NguoiGuiId",
                schema: "dbo",
                table: "PhanHoiKhieuNai",
                column: "NguoiGuiId");

            migrationBuilder.CreateIndex(
                name: "IX_PhienBanThoaThuan_GiaoDichId",
                schema: "dbo",
                table: "PhienBanThoaThuan",
                column: "GiaoDichId",
                unique: true,
                filter: "[TrangThai] = 'DaChot'");

            migrationBuilder.CreateIndex(
                name: "IX_PhienBanThoaThuan_GiaoDichId_NguoiTaoId",
                schema: "dbo",
                table: "PhienBanThoaThuan",
                columns: new[] { "GiaoDichId", "NguoiTaoId" });

            migrationBuilder.CreateIndex(
                name: "IX_PhienBanThoaThuan_GiaoDichId_SoPhienBan",
                schema: "dbo",
                table: "PhienBanThoaThuan",
                columns: new[] { "GiaoDichId", "SoPhienBan" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuyetDinhXuLy_KhieuNaiId",
                schema: "dbo",
                table: "QuyetDinhXuLy",
                column: "KhieuNaiId");

            migrationBuilder.CreateIndex(
                name: "IX_QuyetDinhXuLy_QuanTriVienId",
                schema: "dbo",
                table: "QuyetDinhXuLy",
                column: "QuanTriVienId");

            migrationBuilder.CreateIndex(
                name: "IX_SanPham_ChuSoHuuId",
                schema: "dbo",
                table: "SanPham",
                column: "ChuSoHuuId");

            migrationBuilder.CreateIndex(
                name: "IX_SanPham_DanhMucId",
                schema: "dbo",
                table: "SanPham",
                column: "DanhMucId");

            migrationBuilder.CreateIndex(
                name: "IX_TepTinNhan_TinNhanId",
                schema: "dbo",
                table: "TepTinNhan",
                column: "TinNhanId");

            migrationBuilder.CreateIndex(
                name: "IX_ThanhToan_DonHangId",
                schema: "dbo",
                table: "ThanhToan",
                column: "DonHangId");

            migrationBuilder.CreateIndex(
                name: "IX_ThanhToan_MaThamChieu",
                schema: "dbo",
                table: "ThanhToan",
                column: "MaThamChieu",
                unique: true,
                filter: "[MaThamChieu] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ThanhVienGiaoDich_NguoiDungId",
                schema: "dbo",
                table: "ThanhVienGiaoDich",
                column: "NguoiDungId");

            migrationBuilder.CreateIndex(
                name: "IX_ThanhVienTroChuyen_NguoiDungId",
                schema: "dbo",
                table: "ThanhVienTroChuyen",
                column: "NguoiDungId");

            migrationBuilder.CreateIndex(
                name: "IX_ThanhVienVong_GiaoDichId_NguoiDungId",
                schema: "dbo",
                table: "ThanhVienVong",
                columns: new[] { "GiaoDichId", "NguoiDungId" });

            migrationBuilder.CreateIndex(
                name: "IX_ThanhVienVong_SanPhamId_NguoiDungId",
                schema: "dbo",
                table: "ThanhVienVong",
                columns: new[] { "SanPhamId", "NguoiDungId" });

            migrationBuilder.CreateIndex(
                name: "IX_ThanhVienVong_VongSmartSwapId_GiaoDichId",
                schema: "dbo",
                table: "ThanhVienVong",
                columns: new[] { "VongSmartSwapId", "GiaoDichId" });

            migrationBuilder.CreateIndex(
                name: "IX_ThanhVienVong_VongSmartSwapId_NguoiDungId",
                schema: "dbo",
                table: "ThanhVienVong",
                columns: new[] { "VongSmartSwapId", "NguoiDungId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ThanhVienVong_VongSmartSwapId_SanPhamId",
                schema: "dbo",
                table: "ThanhVienVong",
                columns: new[] { "VongSmartSwapId", "SanPhamId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ThanhVienVong_VongSmartSwapId_ViTri",
                schema: "dbo",
                table: "ThanhVienVong",
                columns: new[] { "VongSmartSwapId", "ViTri" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ThongBao_GiaoDichId",
                schema: "dbo",
                table: "ThongBao",
                column: "GiaoDichId");

            migrationBuilder.CreateIndex(
                name: "IX_ThongBao_NguoiDungId_DaDoc_NgayTao",
                schema: "dbo",
                table: "ThongBao",
                columns: new[] { "NguoiDungId", "DaDoc", "NgayTao" });

            migrationBuilder.CreateIndex(
                name: "IX_TienBu_GiaoDichId_NguoiNhanId",
                schema: "dbo",
                table: "TienBu",
                columns: new[] { "GiaoDichId", "NguoiNhanId" });

            migrationBuilder.CreateIndex(
                name: "IX_TienBu_GiaoDichId_NguoiTraId",
                schema: "dbo",
                table: "TienBu",
                columns: new[] { "GiaoDichId", "NguoiTraId" });

            migrationBuilder.CreateIndex(
                name: "IX_TienBu_PhienBanThoaThuanId_GiaoDichId",
                schema: "dbo",
                table: "TienBu",
                columns: new[] { "PhienBanThoaThuanId", "GiaoDichId" });

            migrationBuilder.CreateIndex(
                name: "IX_TienBu_PhienBanThoaThuanId_NguoiTraId_NguoiNhanId",
                schema: "dbo",
                table: "TienBu",
                columns: new[] { "PhienBanThoaThuanId", "NguoiTraId", "NguoiNhanId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TieuChiDanhGia_Ma",
                schema: "dbo",
                table: "TieuChiDanhGia",
                column: "Ma",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TinDang_NguoiDuyetId",
                schema: "dbo",
                table: "TinDang",
                column: "NguoiDuyetId");

            migrationBuilder.CreateIndex(
                name: "IX_TinDang_SanPhamId",
                schema: "dbo",
                table: "TinDang",
                column: "SanPhamId",
                unique: true,
                filter: "[TrangThai] IN ('ChoDuyet', 'DangHienThi')");

            migrationBuilder.CreateIndex(
                name: "IX_TinDang_SanPhamId_NguoiDangId",
                schema: "dbo",
                table: "TinDang",
                columns: new[] { "SanPhamId", "NguoiDangId" });

            migrationBuilder.CreateIndex(
                name: "IX_TinNhan_CuocTroChuyenId_NgayTao",
                schema: "dbo",
                table: "TinNhan",
                columns: new[] { "CuocTroChuyenId", "NgayTao" });

            migrationBuilder.CreateIndex(
                name: "IX_TinNhan_CuocTroChuyenId_NguoiGuiId_MaPhiaClient",
                schema: "dbo",
                table: "TinNhan",
                columns: new[] { "CuocTroChuyenId", "NguoiGuiId", "MaPhiaClient" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VaiTro_Ma",
                schema: "dbo",
                table: "VaiTro",
                column: "Ma",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VanChuyenMoPhong_DonHangId",
                schema: "dbo",
                table: "VanChuyenMoPhong",
                column: "DonHangId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VanChuyenMoPhong_MaVanDon",
                schema: "dbo",
                table: "VanChuyenMoPhong",
                column: "MaVanDon",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VongSmartSwap_GiaoDichId",
                schema: "dbo",
                table: "VongSmartSwap",
                column: "GiaoDichId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_XacNhanGiaoNhan_GiaoDichId_NguoiDoiTacId",
                schema: "dbo",
                table: "XacNhanGiaoNhan",
                columns: new[] { "GiaoDichId", "NguoiDoiTacId" });

            migrationBuilder.CreateIndex(
                name: "IX_XacNhanGiaoNhan_GiaoDichId_NguoiXacNhanId_Loai",
                schema: "dbo",
                table: "XacNhanGiaoNhan",
                columns: new[] { "GiaoDichId", "NguoiXacNhanId", "Loai" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_XacNhanGiaoNhan_PhienBanThoaThuanId_GiaoDichId",
                schema: "dbo",
                table: "XacNhanGiaoNhan",
                columns: new[] { "PhienBanThoaThuanId", "GiaoDichId" });

            migrationBuilder.CreateIndex(
                name: "IX_XacNhanGiaoNhan_SanPhamId",
                schema: "dbo",
                table: "XacNhanGiaoNhan",
                column: "SanPhamId");

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauHoTro_DonHangId",
                schema: "dbo",
                table: "YeuCauHoTro",
                column: "DonHangId");

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauHoTro_Ma",
                schema: "dbo",
                table: "YeuCauHoTro",
                column: "Ma",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauHoTro_NguoiGuiId",
                schema: "dbo",
                table: "YeuCauHoTro",
                column: "NguoiGuiId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnhSanPham",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "BangChungGiaoNhan",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "BangChungKhieuNai",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ChangTraoDoi",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ChiTietDonHang",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "DanhGiaTieuChi",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "DeNghiTraoDoi",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "DongYThoaThuan",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "GiuChoSanPham",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "HoSoCaNhan",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "KhoaTaiKhoan",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "LichSuVanChuyen",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "NguoiDungVaiTro",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "NhatKyKiemDuyet",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "NhatKyQuanTri",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "NhuCauTraoDoi",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "PhanHoiHoTro",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "PhanHoiKhieuNai",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "QuyetDinhXuLy",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "TepTinNhan",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ThanhToan",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ThongBao",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "TienBu",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "XacNhanGiaoNhan",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ThanhVienVong",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "DanhGia",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "TieuChiDanhGia",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "VanChuyenMoPhong",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "VaiTro",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "YeuCauHoTro",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "KhieuNai",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "TinNhan",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "PhienBanThoaThuan",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "VongSmartSwap",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "DonHang",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ThanhVienTroChuyen",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ThanhVienGiaoDich",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "CuocTroChuyen",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "GiaoDich",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "TinDang",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "SanPham",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "DanhMuc",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "NguoiDung",
                schema: "dbo");
        }
    }
}
