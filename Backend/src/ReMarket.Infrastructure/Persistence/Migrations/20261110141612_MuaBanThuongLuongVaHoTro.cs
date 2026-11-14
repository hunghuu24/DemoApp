using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReMarket.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MuaBanThuongLuongVaHoTro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_YeuCauHoTro_NguoiGuiId",
                schema: "dbo",
                table: "YeuCauHoTro");

            migrationBuilder.DropCheckConstraint(
                name: "CK_TinDang_TrangThai",
                schema: "dbo",
                table: "TinDang");

            migrationBuilder.DropIndex(
                name: "IX_CuocTroChuyen_TinDangId",
                schema: "dbo",
                table: "CuocTroChuyen");

            migrationBuilder.AddColumn<Guid>(
                name: "MaYeuCau",
                schema: "dbo",
                table: "YeuCauHoTro",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SoThuTu",
                schema: "dbo",
                table: "TinNhan",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<long>(
                name: "SoThuTuDaDoc",
                schema: "dbo",
                table: "ThanhVienTroChuyen",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<bool>(
                name: "KetQuaCuoi",
                schema: "dbo",
                table: "ThanhToan",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MaYeuCauCuoi",
                schema: "dbo",
                table: "ThanhToan",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "NgayGiaiNgan",
                schema: "dbo",
                table: "ThanhToan",
                type: "datetimeoffset(0)",
                precision: 0,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "NgayHoanTien",
                schema: "dbo",
                table: "ThanhToan",
                type: "datetimeoffset(0)",
                precision: 0,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "NgayThanhToan",
                schema: "dbo",
                table: "ThanhToan",
                type: "datetimeoffset(0)",
                precision: 0,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BamYeuCau",
                schema: "dbo",
                table: "DonHang",
                type: "varchar(64)",
                unicode: false,
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "HanThanhToan",
                schema: "dbo",
                table: "DonHang",
                type: "datetimeoffset(0)",
                precision: 0,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LyDoHuy",
                schema: "dbo",
                table: "DonHang",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MaYeuCau",
                schema: "dbo",
                table: "DonHang",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "NgayHuy",
                schema: "dbo",
                table: "DonHang",
                type: "datetimeoffset(0)",
                precision: 0,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PhieuThuongLuongId",
                schema: "dbo",
                table: "DonHang",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MaYeuCau",
                schema: "dbo",
                table: "DanhGia",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SoSaoSanPham",
                schema: "dbo",
                table: "DanhGia",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "NguoiMuaId",
                schema: "dbo",
                table: "CuocTroChuyen",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "GiaNiemYet",
                schema: "dbo",
                table: "ChiTietDonHang",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SoTienGiam",
                schema: "dbo",
                table: "ChiTietDonHang",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_TinNhan_Id_CuocTroChuyenId",
                schema: "dbo",
                table: "TinNhan",
                columns: new[] { "Id", "CuocTroChuyenId" });

            migrationBuilder.CreateTable(
                name: "LichSuHoTro",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    PhienBan = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    YeuCauHoTroId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NguoiThucHienId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrangThai = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichSuHoTro", x => x.Id);
                    table.CheckConstraint("CK_LichSuHoTro_TrangThai", "[TrangThai] IN ('Moi','DangXuLy','DaGiaiQuyet')");
                    table.ForeignKey(
                        name: "FK_LichSuHoTro_NguoiDung_NguoiThucHienId",
                        column: x => x.NguoiThucHienId,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LichSuHoTro_YeuCauHoTro_YeuCauHoTroId",
                        column: x => x.YeuCauHoTroId,
                        principalSchema: "dbo",
                        principalTable: "YeuCauHoTro",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PhieuThuongLuong",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    PhienBan = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CuocTroChuyenId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TinNhanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NguoiMuaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NguoiBanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GiaNiemYet = table.Column<long>(type: "bigint", nullable: false),
                    SoTienGiam = table.Column<long>(type: "bigint", nullable: false),
                    TrangThai = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    NgayHetHan = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhieuThuongLuong", x => x.Id);
                    table.CheckConstraint("CK_PhieuThuongLuong_Gia", "[GiaNiemYet]>0 AND [SoTienGiam]>0 AND [SoTienGiam]<[GiaNiemYet]");
                    table.CheckConstraint("CK_PhieuThuongLuong_Han", "[NgayHetHan]>[NgayTao]");
                    table.CheckConstraint("CK_PhieuThuongLuong_Nguoi", "[NguoiMuaId]<>[NguoiBanId]");
                    table.CheckConstraint("CK_PhieuThuongLuong_TrangThai", "[TrangThai] IN ('ChoPhanHoi','DaChapNhan','TuChoi','DaHuy','DaSuDung','HetHan')");
                    table.ForeignKey(
                        name: "FK_PhieuThuongLuong_CuocTroChuyen_CuocTroChuyenId",
                        column: x => x.CuocTroChuyenId,
                        principalSchema: "dbo",
                        principalTable: "CuocTroChuyen",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PhieuThuongLuong_ThanhVienTroChuyen_CuocTroChuyenId_NguoiBanId",
                        columns: x => new { x.CuocTroChuyenId, x.NguoiBanId },
                        principalSchema: "dbo",
                        principalTable: "ThanhVienTroChuyen",
                        principalColumns: new[] { "CuocTroChuyenId", "NguoiDungId" });
                    table.ForeignKey(
                        name: "FK_PhieuThuongLuong_ThanhVienTroChuyen_CuocTroChuyenId_NguoiMuaId",
                        columns: x => new { x.CuocTroChuyenId, x.NguoiMuaId },
                        principalSchema: "dbo",
                        principalTable: "ThanhVienTroChuyen",
                        principalColumns: new[] { "CuocTroChuyenId", "NguoiDungId" });
                    table.ForeignKey(
                        name: "FK_PhieuThuongLuong_TinNhan_TinNhanId_CuocTroChuyenId",
                        columns: x => new { x.TinNhanId, x.CuocTroChuyenId },
                        principalSchema: "dbo",
                        principalTable: "TinNhan",
                        principalColumns: new[] { "Id", "CuocTroChuyenId" });
                });

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauHoTro_NguoiGuiId_MaYeuCau",
                schema: "dbo",
                table: "YeuCauHoTro",
                columns: new[] { "NguoiGuiId", "MaYeuCau" },
                unique: true,
                filter: "[MaYeuCau] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TinNhan_CuocTroChuyenId_SoThuTu",
                schema: "dbo",
                table: "TinNhan",
                columns: new[] { "CuocTroChuyenId", "SoThuTu" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_TinDang_TrangThai",
                schema: "dbo",
                table: "TinDang",
                sql: "[TrangThai] IN (N'BanNhap', N'ChoDuyet', N'DangHienThi', N'TuChoi', N'DaAn', N'DaBan', N'DangGiuCho')");

            migrationBuilder.CreateIndex(
                name: "IX_DonHang_NguoiBanId_NgayTao",
                schema: "dbo",
                table: "DonHang",
                columns: new[] { "NguoiBanId", "NgayTao" });

            migrationBuilder.CreateIndex(
                name: "IX_DonHang_NguoiMuaId_MaYeuCau",
                schema: "dbo",
                table: "DonHang",
                columns: new[] { "NguoiMuaId", "MaYeuCau" },
                unique: true,
                filter: "[MaYeuCau] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DonHang_PhieuThuongLuongId",
                schema: "dbo",
                table: "DonHang",
                column: "PhieuThuongLuongId",
                unique: true,
                filter: "[PhieuThuongLuongId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DonHang_TrangThai_HanThanhToan",
                schema: "dbo",
                table: "DonHang",
                columns: new[] { "TrangThai", "HanThanhToan" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_DanhGia_SanPham",
                schema: "dbo",
                table: "DanhGia",
                sql: "[SoSaoSanPham] IS NULL OR [SoSaoSanPham] BETWEEN 1 AND 5");

            migrationBuilder.CreateIndex(
                name: "IX_CuocTroChuyen_NguoiMuaId",
                schema: "dbo",
                table: "CuocTroChuyen",
                column: "NguoiMuaId");

            migrationBuilder.CreateIndex(
                name: "IX_CuocTroChuyen_TinDangId_NguoiMuaId",
                schema: "dbo",
                table: "CuocTroChuyen",
                columns: new[] { "TinDangId", "NguoiMuaId" },
                unique: true,
                filter: "[NguoiMuaId] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ChiTietDonHang_GiamGia",
                schema: "dbo",
                table: "ChiTietDonHang",
                sql: "[SoTienGiam]>=0 AND ([GiaNiemYet] IS NULL OR ([GiaNiemYet]>0 AND [DonGia]=[GiaNiemYet]-[SoTienGiam]))");

            migrationBuilder.CreateIndex(
                name: "IX_LichSuHoTro_NguoiThucHienId",
                schema: "dbo",
                table: "LichSuHoTro",
                column: "NguoiThucHienId");

            migrationBuilder.CreateIndex(
                name: "IX_LichSuHoTro_YeuCauHoTroId_NgayTao",
                schema: "dbo",
                table: "LichSuHoTro",
                columns: new[] { "YeuCauHoTroId", "NgayTao" });

            migrationBuilder.CreateIndex(
                name: "IX_PhieuThuongLuong_CuocTroChuyenId",
                schema: "dbo",
                table: "PhieuThuongLuong",
                column: "CuocTroChuyenId",
                unique: true,
                filter: "[TrangThai]='DaChapNhan'");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuThuongLuong_CuocTroChuyenId_NguoiBanId",
                schema: "dbo",
                table: "PhieuThuongLuong",
                columns: new[] { "CuocTroChuyenId", "NguoiBanId" });

            migrationBuilder.CreateIndex(
                name: "IX_PhieuThuongLuong_CuocTroChuyenId_NguoiMuaId",
                schema: "dbo",
                table: "PhieuThuongLuong",
                columns: new[] { "CuocTroChuyenId", "NguoiMuaId" });

            migrationBuilder.CreateIndex(
                name: "IX_PhieuThuongLuong_TinNhanId",
                schema: "dbo",
                table: "PhieuThuongLuong",
                column: "TinNhanId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PhieuThuongLuong_TinNhanId_CuocTroChuyenId",
                schema: "dbo",
                table: "PhieuThuongLuong",
                columns: new[] { "TinNhanId", "CuocTroChuyenId" });

            migrationBuilder.AddForeignKey(
                name: "FK_CuocTroChuyen_NguoiDung_NguoiMuaId",
                schema: "dbo",
                table: "CuocTroChuyen",
                column: "NguoiMuaId",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DonHang_PhieuThuongLuong_PhieuThuongLuongId",
                schema: "dbo",
                table: "DonHang",
                column: "PhieuThuongLuongId",
                principalSchema: "dbo",
                principalTable: "PhieuThuongLuong",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CuocTroChuyen_NguoiDung_NguoiMuaId",
                schema: "dbo",
                table: "CuocTroChuyen");

            migrationBuilder.DropForeignKey(
                name: "FK_DonHang_PhieuThuongLuong_PhieuThuongLuongId",
                schema: "dbo",
                table: "DonHang");

            migrationBuilder.DropTable(
                name: "LichSuHoTro",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "PhieuThuongLuong",
                schema: "dbo");

            migrationBuilder.DropIndex(
                name: "IX_YeuCauHoTro_NguoiGuiId_MaYeuCau",
                schema: "dbo",
                table: "YeuCauHoTro");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_TinNhan_Id_CuocTroChuyenId",
                schema: "dbo",
                table: "TinNhan");

            migrationBuilder.DropIndex(
                name: "IX_TinNhan_CuocTroChuyenId_SoThuTu",
                schema: "dbo",
                table: "TinNhan");

            migrationBuilder.DropCheckConstraint(
                name: "CK_TinDang_TrangThai",
                schema: "dbo",
                table: "TinDang");

            migrationBuilder.DropIndex(
                name: "IX_DonHang_NguoiBanId_NgayTao",
                schema: "dbo",
                table: "DonHang");

            migrationBuilder.DropIndex(
                name: "IX_DonHang_NguoiMuaId_MaYeuCau",
                schema: "dbo",
                table: "DonHang");

            migrationBuilder.DropIndex(
                name: "IX_DonHang_PhieuThuongLuongId",
                schema: "dbo",
                table: "DonHang");

            migrationBuilder.DropIndex(
                name: "IX_DonHang_TrangThai_HanThanhToan",
                schema: "dbo",
                table: "DonHang");

            migrationBuilder.DropCheckConstraint(
                name: "CK_DanhGia_SanPham",
                schema: "dbo",
                table: "DanhGia");

            migrationBuilder.DropIndex(
                name: "IX_CuocTroChuyen_NguoiMuaId",
                schema: "dbo",
                table: "CuocTroChuyen");

            migrationBuilder.DropIndex(
                name: "IX_CuocTroChuyen_TinDangId_NguoiMuaId",
                schema: "dbo",
                table: "CuocTroChuyen");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ChiTietDonHang_GiamGia",
                schema: "dbo",
                table: "ChiTietDonHang");

            migrationBuilder.DropColumn(
                name: "MaYeuCau",
                schema: "dbo",
                table: "YeuCauHoTro");

            migrationBuilder.DropColumn(
                name: "SoThuTu",
                schema: "dbo",
                table: "TinNhan");

            migrationBuilder.DropColumn(
                name: "SoThuTuDaDoc",
                schema: "dbo",
                table: "ThanhVienTroChuyen");

            migrationBuilder.DropColumn(
                name: "KetQuaCuoi",
                schema: "dbo",
                table: "ThanhToan");

            migrationBuilder.DropColumn(
                name: "MaYeuCauCuoi",
                schema: "dbo",
                table: "ThanhToan");

            migrationBuilder.DropColumn(
                name: "NgayGiaiNgan",
                schema: "dbo",
                table: "ThanhToan");

            migrationBuilder.DropColumn(
                name: "NgayHoanTien",
                schema: "dbo",
                table: "ThanhToan");

            migrationBuilder.DropColumn(
                name: "NgayThanhToan",
                schema: "dbo",
                table: "ThanhToan");

            migrationBuilder.DropColumn(
                name: "BamYeuCau",
                schema: "dbo",
                table: "DonHang");

            migrationBuilder.DropColumn(
                name: "HanThanhToan",
                schema: "dbo",
                table: "DonHang");

            migrationBuilder.DropColumn(
                name: "LyDoHuy",
                schema: "dbo",
                table: "DonHang");

            migrationBuilder.DropColumn(
                name: "MaYeuCau",
                schema: "dbo",
                table: "DonHang");

            migrationBuilder.DropColumn(
                name: "NgayHuy",
                schema: "dbo",
                table: "DonHang");

            migrationBuilder.DropColumn(
                name: "PhieuThuongLuongId",
                schema: "dbo",
                table: "DonHang");

            migrationBuilder.DropColumn(
                name: "MaYeuCau",
                schema: "dbo",
                table: "DanhGia");

            migrationBuilder.DropColumn(
                name: "SoSaoSanPham",
                schema: "dbo",
                table: "DanhGia");

            migrationBuilder.DropColumn(
                name: "NguoiMuaId",
                schema: "dbo",
                table: "CuocTroChuyen");

            migrationBuilder.DropColumn(
                name: "GiaNiemYet",
                schema: "dbo",
                table: "ChiTietDonHang");

            migrationBuilder.DropColumn(
                name: "SoTienGiam",
                schema: "dbo",
                table: "ChiTietDonHang");

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauHoTro_NguoiGuiId",
                schema: "dbo",
                table: "YeuCauHoTro",
                column: "NguoiGuiId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_TinDang_TrangThai",
                schema: "dbo",
                table: "TinDang",
                sql: "[TrangThai] IN (N'BanNhap', N'ChoDuyet', N'DangHienThi', N'TuChoi', N'DaAn', N'DaBan')");

            migrationBuilder.CreateIndex(
                name: "IX_CuocTroChuyen_TinDangId",
                schema: "dbo",
                table: "CuocTroChuyen",
                column: "TinDangId");
        }
    }
}
