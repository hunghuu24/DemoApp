using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReMarket.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class LoiMoiVaThuongLuongSmartSwap : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PhienBanThoaThuan_GiaoDichId_NguoiTaoId",
                schema: "dbo",
                table: "PhienBanThoaThuan");

            migrationBuilder.AddColumn<string>(
                name: "BamYeuCau",
                schema: "dbo",
                table: "VongSmartSwap",
                type: "varchar(64)",
                unicode: false,
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LyDoKetThuc",
                schema: "dbo",
                table: "VongSmartSwap",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MaDeXuat",
                schema: "dbo",
                table: "VongSmartSwap",
                type: "varchar(98)",
                unicode: false,
                maxLength: 98,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MaKetThuc",
                schema: "dbo",
                table: "VongSmartSwap",
                type: "varchar(40)",
                unicode: false,
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MaYeuCau",
                schema: "dbo",
                table: "VongSmartSwap",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "NgayChapNhan",
                schema: "dbo",
                table: "VongSmartSwap",
                type: "datetimeoffset(0)",
                precision: 0,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "NguoiKhoiTaoId",
                schema: "dbo",
                table: "VongSmartSwap",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PhienBanThoaThuanId",
                schema: "dbo",
                table: "TinNhan",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TinDangId",
                schema: "dbo",
                table: "ThanhVienVong",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BamYeuCau",
                schema: "dbo",
                table: "PhienBanThoaThuan",
                type: "varchar(64)",
                unicode: false,
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MaYeuCau",
                schema: "dbo",
                table: "PhienBanThoaThuan",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_VongSmartSwap_GiaoDichId_NguoiKhoiTaoId",
                schema: "dbo",
                table: "VongSmartSwap",
                columns: new[] { "GiaoDichId", "NguoiKhoiTaoId" });

            migrationBuilder.CreateIndex(
                name: "IX_VongSmartSwap_MaDeXuat",
                schema: "dbo",
                table: "VongSmartSwap",
                column: "MaDeXuat",
                unique: true,
                filter: "[MaDeXuat] IS NOT NULL AND [TrangThai]='ChoDongY'");

            migrationBuilder.CreateIndex(
                name: "IX_VongSmartSwap_NguoiKhoiTaoId_MaYeuCau",
                schema: "dbo",
                table: "VongSmartSwap",
                columns: new[] { "NguoiKhoiTaoId", "MaYeuCau" },
                unique: true,
                filter: "[NguoiKhoiTaoId] IS NOT NULL AND [MaYeuCau] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_VongSmartSwap_TrangThai_NgayHetHan",
                schema: "dbo",
                table: "VongSmartSwap",
                columns: new[] { "TrangThai", "NgayHetHan" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_VongSmartSwap_KetThuc",
                schema: "dbo",
                table: "VongSmartSwap",
                sql: "[MaKetThuc] IS NULL OR ([TrangThai]='DaHuy' AND [MaKetThuc] IN ('TuChoi','HetHan','DaHuy'))");

            migrationBuilder.CreateIndex(
                name: "IX_TinNhan_PhienBanThoaThuanId",
                schema: "dbo",
                table: "TinNhan",
                column: "PhienBanThoaThuanId",
                unique: true,
                filter: "[PhienBanThoaThuanId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ThanhVienVong_TinDangId_SanPhamId",
                schema: "dbo",
                table: "ThanhVienVong",
                columns: new[] { "TinDangId", "SanPhamId" });

            migrationBuilder.CreateIndex(
                name: "IX_PhienBanThoaThuan_GiaoDichId_NguoiTaoId_MaYeuCau",
                schema: "dbo",
                table: "PhienBanThoaThuan",
                columns: new[] { "GiaoDichId", "NguoiTaoId", "MaYeuCau" },
                unique: true,
                filter: "[MaYeuCau] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_ThanhVienVong_TinDang_TinDangId_SanPhamId",
                schema: "dbo",
                table: "ThanhVienVong",
                columns: new[] { "TinDangId", "SanPhamId" },
                principalSchema: "dbo",
                principalTable: "TinDang",
                principalColumns: new[] { "Id", "SanPhamId" });

            migrationBuilder.AddForeignKey(
                name: "FK_TinNhan_PhienBanThoaThuan_PhienBanThoaThuanId",
                schema: "dbo",
                table: "TinNhan",
                column: "PhienBanThoaThuanId",
                principalSchema: "dbo",
                principalTable: "PhienBanThoaThuan",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_VongSmartSwap_ThanhVienGiaoDich_GiaoDichId_NguoiKhoiTaoId",
                schema: "dbo",
                table: "VongSmartSwap",
                columns: new[] { "GiaoDichId", "NguoiKhoiTaoId" },
                principalSchema: "dbo",
                principalTable: "ThanhVienGiaoDich",
                principalColumns: new[] { "GiaoDichId", "NguoiDungId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ThanhVienVong_TinDang_TinDangId_SanPhamId",
                schema: "dbo",
                table: "ThanhVienVong");

            migrationBuilder.DropForeignKey(
                name: "FK_TinNhan_PhienBanThoaThuan_PhienBanThoaThuanId",
                schema: "dbo",
                table: "TinNhan");

            migrationBuilder.DropForeignKey(
                name: "FK_VongSmartSwap_ThanhVienGiaoDich_GiaoDichId_NguoiKhoiTaoId",
                schema: "dbo",
                table: "VongSmartSwap");

            migrationBuilder.DropIndex(
                name: "IX_VongSmartSwap_GiaoDichId_NguoiKhoiTaoId",
                schema: "dbo",
                table: "VongSmartSwap");

            migrationBuilder.DropIndex(
                name: "IX_VongSmartSwap_MaDeXuat",
                schema: "dbo",
                table: "VongSmartSwap");

            migrationBuilder.DropIndex(
                name: "IX_VongSmartSwap_NguoiKhoiTaoId_MaYeuCau",
                schema: "dbo",
                table: "VongSmartSwap");

            migrationBuilder.DropIndex(
                name: "IX_VongSmartSwap_TrangThai_NgayHetHan",
                schema: "dbo",
                table: "VongSmartSwap");

            migrationBuilder.DropCheckConstraint(
                name: "CK_VongSmartSwap_KetThuc",
                schema: "dbo",
                table: "VongSmartSwap");

            migrationBuilder.DropIndex(
                name: "IX_TinNhan_PhienBanThoaThuanId",
                schema: "dbo",
                table: "TinNhan");

            migrationBuilder.DropIndex(
                name: "IX_ThanhVienVong_TinDangId_SanPhamId",
                schema: "dbo",
                table: "ThanhVienVong");

            migrationBuilder.DropIndex(
                name: "IX_PhienBanThoaThuan_GiaoDichId_NguoiTaoId_MaYeuCau",
                schema: "dbo",
                table: "PhienBanThoaThuan");

            migrationBuilder.DropColumn(
                name: "BamYeuCau",
                schema: "dbo",
                table: "VongSmartSwap");

            migrationBuilder.DropColumn(
                name: "LyDoKetThuc",
                schema: "dbo",
                table: "VongSmartSwap");

            migrationBuilder.DropColumn(
                name: "MaDeXuat",
                schema: "dbo",
                table: "VongSmartSwap");

            migrationBuilder.DropColumn(
                name: "MaKetThuc",
                schema: "dbo",
                table: "VongSmartSwap");

            migrationBuilder.DropColumn(
                name: "MaYeuCau",
                schema: "dbo",
                table: "VongSmartSwap");

            migrationBuilder.DropColumn(
                name: "NgayChapNhan",
                schema: "dbo",
                table: "VongSmartSwap");

            migrationBuilder.DropColumn(
                name: "NguoiKhoiTaoId",
                schema: "dbo",
                table: "VongSmartSwap");

            migrationBuilder.DropColumn(
                name: "PhienBanThoaThuanId",
                schema: "dbo",
                table: "TinNhan");

            migrationBuilder.DropColumn(
                name: "TinDangId",
                schema: "dbo",
                table: "ThanhVienVong");

            migrationBuilder.DropColumn(
                name: "BamYeuCau",
                schema: "dbo",
                table: "PhienBanThoaThuan");

            migrationBuilder.DropColumn(
                name: "MaYeuCau",
                schema: "dbo",
                table: "PhienBanThoaThuan");

            migrationBuilder.CreateIndex(
                name: "IX_PhienBanThoaThuan_GiaoDichId_NguoiTaoId",
                schema: "dbo",
                table: "PhienBanThoaThuan",
                columns: new[] { "GiaoDichId", "NguoiTaoId" });
        }
    }
}
