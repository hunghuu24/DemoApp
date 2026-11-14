using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReMarket.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class QuanTriDanhMucVaTranhChap : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_QuyetDinhXuLy_KhieuNaiId",
                schema: "dbo",
                table: "QuyetDinhXuLy");

            migrationBuilder.AddColumn<string>(
                name: "BamYeuCau",
                schema: "dbo",
                table: "QuyetDinhXuLy",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CanCu",
                schema: "dbo",
                table: "QuyetDinhXuLy",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HuongXuLy",
                schema: "dbo",
                table: "QuyetDinhXuLy",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "GhiNhan");

            migrationBuilder.AddColumn<Guid>(
                name: "MaYeuCau",
                schema: "dbo",
                table: "QuyetDinhXuLy",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThongBao",
                schema: "dbo",
                table: "QuyetDinhXuLy",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BieuTuong",
                schema: "dbo",
                table: "DanhMuc",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_QuyetDinhXuLy_KhieuNaiId_MaYeuCau",
                schema: "dbo",
                table: "QuyetDinhXuLy",
                columns: new[] { "KhieuNaiId", "MaYeuCau" },
                unique: true,
                filter: "[MaYeuCau] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_QuyetDinhXuLy_KhieuNaiId_MaYeuCau",
                schema: "dbo",
                table: "QuyetDinhXuLy");

            migrationBuilder.DropColumn(
                name: "BamYeuCau",
                schema: "dbo",
                table: "QuyetDinhXuLy");

            migrationBuilder.DropColumn(
                name: "CanCu",
                schema: "dbo",
                table: "QuyetDinhXuLy");

            migrationBuilder.DropColumn(
                name: "HuongXuLy",
                schema: "dbo",
                table: "QuyetDinhXuLy");

            migrationBuilder.DropColumn(
                name: "MaYeuCau",
                schema: "dbo",
                table: "QuyetDinhXuLy");

            migrationBuilder.DropColumn(
                name: "ThongBao",
                schema: "dbo",
                table: "QuyetDinhXuLy");

            migrationBuilder.DropColumn(
                name: "BieuTuong",
                schema: "dbo",
                table: "DanhMuc");

            migrationBuilder.CreateIndex(
                name: "IX_QuyetDinhXuLy_KhieuNaiId",
                schema: "dbo",
                table: "QuyetDinhXuLy",
                column: "KhieuNaiId");
        }
    }
}
