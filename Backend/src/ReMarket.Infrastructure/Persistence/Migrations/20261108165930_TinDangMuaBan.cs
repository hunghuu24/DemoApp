using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReMarket.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TinDangMuaBan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LuotXem",
                schema: "dbo",
                table: "TinDang",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "NgayCapNhat",
                schema: "dbo",
                table: "TinDang",
                type: "datetimeoffset(0)",
                precision: 0,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "NgayXoa",
                schema: "dbo",
                table: "TinDang",
                type: "datetimeoffset(0)",
                precision: 0,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TinDang_NgayXoa_TrangThai_LoaiTin_NgayDang",
                schema: "dbo",
                table: "TinDang",
                columns: new[] { "NgayXoa", "TrangThai", "LoaiTin", "NgayDang" });

            migrationBuilder.CreateIndex(
                name: "IX_TinDang_NguoiDangId_NgayXoa_TrangThai",
                schema: "dbo",
                table: "TinDang",
                columns: new[] { "NguoiDangId", "NgayXoa", "TrangThai" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_TinDang_LuotXem",
                schema: "dbo",
                table: "TinDang",
                sql: "[LuotXem] >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TinDang_NgayXoa_TrangThai_LoaiTin_NgayDang",
                schema: "dbo",
                table: "TinDang");

            migrationBuilder.DropIndex(
                name: "IX_TinDang_NguoiDangId_NgayXoa_TrangThai",
                schema: "dbo",
                table: "TinDang");

            migrationBuilder.DropCheckConstraint(
                name: "CK_TinDang_LuotXem",
                schema: "dbo",
                table: "TinDang");

            migrationBuilder.DropColumn(
                name: "LuotXem",
                schema: "dbo",
                table: "TinDang");

            migrationBuilder.DropColumn(
                name: "NgayCapNhat",
                schema: "dbo",
                table: "TinDang");

            migrationBuilder.DropColumn(
                name: "NgayXoa",
                schema: "dbo",
                table: "TinDang");
        }
    }
}
