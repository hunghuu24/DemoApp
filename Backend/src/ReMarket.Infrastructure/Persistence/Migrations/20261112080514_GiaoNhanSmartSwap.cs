using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReMarket.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class GiaoNhanSmartSwap : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_VongSmartSwap_TrangThai",
                schema: "dbo",
                table: "VongSmartSwap");

            migrationBuilder.DropIndex(
                name: "IX_VanChuyenMoPhong_DonHangId",
                schema: "dbo",
                table: "VanChuyenMoPhong");

            migrationBuilder.DropCheckConstraint(
                name: "CK_VanChuyenMoPhong_TrangThai",
                schema: "dbo",
                table: "VanChuyenMoPhong");

            migrationBuilder.DropCheckConstraint(
                name: "CK_LichSuVanChuyen_TrangThai",
                schema: "dbo",
                table: "LichSuVanChuyen");

            migrationBuilder.AlterColumn<Guid>(
                name: "DonHangId",
                schema: "dbo",
                table: "VanChuyenMoPhong",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "ChangTraoDoiId",
                schema: "dbo",
                table: "VanChuyenMoPhong",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiaChiGui",
                schema: "dbo",
                table: "VanChuyenMoPhong",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiaChiNhan",
                schema: "dbo",
                table: "VanChuyenMoPhong",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DienThoaiGui",
                schema: "dbo",
                table: "VanChuyenMoPhong",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DienThoaiNhan",
                schema: "dbo",
                table: "VanChuyenMoPhong",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MaVanDonTuNhap",
                schema: "dbo",
                table: "VanChuyenMoPhong",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "NgayNhanDungHang",
                schema: "dbo",
                table: "VanChuyenMoPhong",
                type: "datetimeoffset(0)",
                precision: 0,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PhienBanThoaThuanId",
                schema: "dbo",
                table: "VanChuyenMoPhong",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TenNguoiGui",
                schema: "dbo",
                table: "VanChuyenMoPhong",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TenNguoiNhan",
                schema: "dbo",
                table: "VanChuyenMoPhong",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BamYeuCau",
                schema: "dbo",
                table: "KhieuNai",
                type: "varchar(64)",
                unicode: false,
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LoaiSuCo",
                schema: "dbo",
                table: "KhieuNai",
                type: "varchar(40)",
                unicode: false,
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MaYeuCau",
                schema: "dbo",
                table: "KhieuNai",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VanChuyenMoPhongId",
                schema: "dbo",
                table: "KhieuNai",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ThongTinGiaoNhanSmartSwap",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    PhienBan = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    ThanhVienVongId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PhienBanThoaThuanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaYeuCau = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BamYeuCau = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    TenLienHe = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    SoDienThoai = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DiaChiGui = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DiaChiNhan = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    NgayXacNhan = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThongTinGiaoNhanSmartSwap", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThongTinGiaoNhanSmartSwap_PhienBanThoaThuan_PhienBanThoaThuanId",
                        column: x => x.PhienBanThoaThuanId,
                        principalSchema: "dbo",
                        principalTable: "PhienBanThoaThuan",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ThongTinGiaoNhanSmartSwap_ThanhVienVong_ThanhVienVongId",
                        column: x => x.ThanhVienVongId,
                        principalSchema: "dbo",
                        principalTable: "ThanhVienVong",
                        principalColumn: "Id");
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_VongSmartSwap_TrangThai",
                schema: "dbo",
                table: "VongSmartSwap",
                sql: "[TrangThai] IN (N'ChoDongY', N'SanSangBanGiao', N'DangBanGiao', N'HoanTat', N'DaHuy', N'TamDung')");

            migrationBuilder.CreateIndex(
                name: "IX_VanChuyenMoPhong_ChangTraoDoiId",
                schema: "dbo",
                table: "VanChuyenMoPhong",
                column: "ChangTraoDoiId",
                unique: true,
                filter: "[ChangTraoDoiId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_VanChuyenMoPhong_DonHangId",
                schema: "dbo",
                table: "VanChuyenMoPhong",
                column: "DonHangId",
                unique: true,
                filter: "[DonHangId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_VanChuyenMoPhong_MaVanDonTuNhap",
                schema: "dbo",
                table: "VanChuyenMoPhong",
                column: "MaVanDonTuNhap",
                unique: true,
                filter: "[MaVanDonTuNhap] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_VanChuyenMoPhong_PhienBanThoaThuanId",
                schema: "dbo",
                table: "VanChuyenMoPhong",
                column: "PhienBanThoaThuanId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_VanChuyenMoPhong_Nguon",
                schema: "dbo",
                table: "VanChuyenMoPhong",
                sql: "([DonHangId] IS NOT NULL AND [ChangTraoDoiId] IS NULL AND [PhienBanThoaThuanId] IS NULL) OR ([DonHangId] IS NULL AND [ChangTraoDoiId] IS NOT NULL AND [PhienBanThoaThuanId] IS NOT NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_VanChuyenMoPhong_SmartSwap",
                schema: "dbo",
                table: "VanChuyenMoPhong",
                sql: "[ChangTraoDoiId] IS NULL OR ([TenNguoiGui] IS NOT NULL AND [DienThoaiGui] IS NOT NULL AND [DiaChiGui] IS NOT NULL AND [TenNguoiNhan] IS NOT NULL AND [DienThoaiNhan] IS NOT NULL AND [DiaChiNhan] IS NOT NULL AND [TrangThai] IN ('ChoGui','DaGui','DangVanChuyen','DaGiao') AND ([TrangThai]='ChoGui' OR [MaVanDonTuNhap] IS NOT NULL) AND ([NgayNhanDungHang] IS NULL OR [TrangThai]='DaGiao'))");

            migrationBuilder.AddCheckConstraint(
                name: "CK_VanChuyenMoPhong_TrangThai",
                schema: "dbo",
                table: "VanChuyenMoPhong",
                sql: "[TrangThai] IN (N'ChoLayHang', N'DaLayHang', N'DangVanChuyen', N'DaGiao', N'GiaoThatBai', N'ChoGui', N'DaGui')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_LichSuVanChuyen_TrangThai",
                schema: "dbo",
                table: "LichSuVanChuyen",
                sql: "[TrangThai] IN (N'ChoLayHang', N'DaLayHang', N'DangVanChuyen', N'DaGiao', N'GiaoThatBai', N'ChoGui', N'DaGui')");

            migrationBuilder.CreateIndex(
                name: "IX_KhieuNai_NguoiGuiId_MaYeuCau",
                schema: "dbo",
                table: "KhieuNai",
                columns: new[] { "NguoiGuiId", "MaYeuCau" },
                unique: true,
                filter: "[MaYeuCau] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_KhieuNai_VanChuyenMoPhongId",
                schema: "dbo",
                table: "KhieuNai",
                column: "VanChuyenMoPhongId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_KhieuNai_SuCoSmartSwap",
                schema: "dbo",
                table: "KhieuNai",
                sql: "([VanChuyenMoPhongId] IS NULL AND [LoaiSuCo] IS NULL) OR ([VanChuyenMoPhongId] IS NOT NULL AND [LoaiSuCo] IN ('SaiHang','ChuaNhanHang'))");

            migrationBuilder.CreateIndex(
                name: "IX_ThongTinGiaoNhanSmartSwap_PhienBanThoaThuanId",
                table: "ThongTinGiaoNhanSmartSwap",
                column: "PhienBanThoaThuanId");

            migrationBuilder.CreateIndex(
                name: "IX_ThongTinGiaoNhanSmartSwap_ThanhVienVongId",
                table: "ThongTinGiaoNhanSmartSwap",
                column: "ThanhVienVongId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_KhieuNai_VanChuyenMoPhong_VanChuyenMoPhongId",
                schema: "dbo",
                table: "KhieuNai",
                column: "VanChuyenMoPhongId",
                principalSchema: "dbo",
                principalTable: "VanChuyenMoPhong",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_VanChuyenMoPhong_ChangTraoDoi_ChangTraoDoiId",
                schema: "dbo",
                table: "VanChuyenMoPhong",
                column: "ChangTraoDoiId",
                principalSchema: "dbo",
                principalTable: "ChangTraoDoi",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_VanChuyenMoPhong_PhienBanThoaThuan_PhienBanThoaThuanId",
                schema: "dbo",
                table: "VanChuyenMoPhong",
                column: "PhienBanThoaThuanId",
                principalSchema: "dbo",
                principalTable: "PhienBanThoaThuan",
                principalColumn: "Id");
            foreach (var sql in SmartSwapDeliverySql.Create)
                migrationBuilder.Sql("EXEC(N'" + sql.Replace("'", "''") + "');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF EXISTS(SELECT 1 FROM dbo.VanChuyenMoPhong WHERE ChangTraoDoiId IS NOT NULL) THROW 51040,N'Khong rollback tu dong khi da co van don Smart Swap; can bao toan lich su.',1;");
            foreach (var name in SmartSwapDeliverySql.Triggers)
                migrationBuilder.Sql("DROP TRIGGER IF EXISTS dbo." + name + ";");
            migrationBuilder.DropForeignKey(
                name: "FK_KhieuNai_VanChuyenMoPhong_VanChuyenMoPhongId",
                schema: "dbo",
                table: "KhieuNai");

            migrationBuilder.DropForeignKey(
                name: "FK_VanChuyenMoPhong_ChangTraoDoi_ChangTraoDoiId",
                schema: "dbo",
                table: "VanChuyenMoPhong");

            migrationBuilder.DropForeignKey(
                name: "FK_VanChuyenMoPhong_PhienBanThoaThuan_PhienBanThoaThuanId",
                schema: "dbo",
                table: "VanChuyenMoPhong");

            migrationBuilder.DropTable(
                name: "ThongTinGiaoNhanSmartSwap");

            migrationBuilder.DropCheckConstraint(
                name: "CK_VongSmartSwap_TrangThai",
                schema: "dbo",
                table: "VongSmartSwap");

            migrationBuilder.DropIndex(
                name: "IX_VanChuyenMoPhong_ChangTraoDoiId",
                schema: "dbo",
                table: "VanChuyenMoPhong");

            migrationBuilder.DropIndex(
                name: "IX_VanChuyenMoPhong_DonHangId",
                schema: "dbo",
                table: "VanChuyenMoPhong");

            migrationBuilder.DropIndex(
                name: "IX_VanChuyenMoPhong_MaVanDonTuNhap",
                schema: "dbo",
                table: "VanChuyenMoPhong");

            migrationBuilder.DropIndex(
                name: "IX_VanChuyenMoPhong_PhienBanThoaThuanId",
                schema: "dbo",
                table: "VanChuyenMoPhong");

            migrationBuilder.DropCheckConstraint(
                name: "CK_VanChuyenMoPhong_Nguon",
                schema: "dbo",
                table: "VanChuyenMoPhong");

            migrationBuilder.DropCheckConstraint(
                name: "CK_VanChuyenMoPhong_SmartSwap",
                schema: "dbo",
                table: "VanChuyenMoPhong");

            migrationBuilder.DropCheckConstraint(
                name: "CK_VanChuyenMoPhong_TrangThai",
                schema: "dbo",
                table: "VanChuyenMoPhong");

            migrationBuilder.DropCheckConstraint(
                name: "CK_LichSuVanChuyen_TrangThai",
                schema: "dbo",
                table: "LichSuVanChuyen");

            migrationBuilder.DropIndex(
                name: "IX_KhieuNai_NguoiGuiId_MaYeuCau",
                schema: "dbo",
                table: "KhieuNai");

            migrationBuilder.DropIndex(
                name: "IX_KhieuNai_VanChuyenMoPhongId",
                schema: "dbo",
                table: "KhieuNai");

            migrationBuilder.DropCheckConstraint(
                name: "CK_KhieuNai_SuCoSmartSwap",
                schema: "dbo",
                table: "KhieuNai");

            migrationBuilder.DropColumn(
                name: "ChangTraoDoiId",
                schema: "dbo",
                table: "VanChuyenMoPhong");

            migrationBuilder.DropColumn(
                name: "DiaChiGui",
                schema: "dbo",
                table: "VanChuyenMoPhong");

            migrationBuilder.DropColumn(
                name: "DiaChiNhan",
                schema: "dbo",
                table: "VanChuyenMoPhong");

            migrationBuilder.DropColumn(
                name: "DienThoaiGui",
                schema: "dbo",
                table: "VanChuyenMoPhong");

            migrationBuilder.DropColumn(
                name: "DienThoaiNhan",
                schema: "dbo",
                table: "VanChuyenMoPhong");

            migrationBuilder.DropColumn(
                name: "MaVanDonTuNhap",
                schema: "dbo",
                table: "VanChuyenMoPhong");

            migrationBuilder.DropColumn(
                name: "NgayNhanDungHang",
                schema: "dbo",
                table: "VanChuyenMoPhong");

            migrationBuilder.DropColumn(
                name: "PhienBanThoaThuanId",
                schema: "dbo",
                table: "VanChuyenMoPhong");

            migrationBuilder.DropColumn(
                name: "TenNguoiGui",
                schema: "dbo",
                table: "VanChuyenMoPhong");

            migrationBuilder.DropColumn(
                name: "TenNguoiNhan",
                schema: "dbo",
                table: "VanChuyenMoPhong");

            migrationBuilder.DropColumn(
                name: "BamYeuCau",
                schema: "dbo",
                table: "KhieuNai");

            migrationBuilder.DropColumn(
                name: "LoaiSuCo",
                schema: "dbo",
                table: "KhieuNai");

            migrationBuilder.DropColumn(
                name: "MaYeuCau",
                schema: "dbo",
                table: "KhieuNai");

            migrationBuilder.DropColumn(
                name: "VanChuyenMoPhongId",
                schema: "dbo",
                table: "KhieuNai");

            migrationBuilder.AlterColumn<Guid>(
                name: "DonHangId",
                schema: "dbo",
                table: "VanChuyenMoPhong",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_VongSmartSwap_TrangThai",
                schema: "dbo",
                table: "VongSmartSwap",
                sql: "[TrangThai] IN (N'ChoDongY', N'SanSangBanGiao', N'DangBanGiao', N'HoanTat', N'DaHuy')");

            migrationBuilder.CreateIndex(
                name: "IX_VanChuyenMoPhong_DonHangId",
                schema: "dbo",
                table: "VanChuyenMoPhong",
                column: "DonHangId",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_VanChuyenMoPhong_TrangThai",
                schema: "dbo",
                table: "VanChuyenMoPhong",
                sql: "[TrangThai] IN (N'ChoLayHang', N'DaLayHang', N'DangVanChuyen', N'DaGiao', N'GiaoThatBai')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_LichSuVanChuyen_TrangThai",
                schema: "dbo",
                table: "LichSuVanChuyen",
                sql: "[TrangThai] IN (N'ChoLayHang', N'DaLayHang', N'DangVanChuyen', N'DaGiao', N'GiaoThatBai')");
        }
    }
}
