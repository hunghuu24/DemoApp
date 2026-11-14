using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReMarket.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TaiKhoanJwtVaKhoiPhuc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE dbo.VaiTro SET Ma='USER' WHERE Ma=N'NguoiDung';
                UPDATE dbo.VaiTro SET Ma='ADMIN' WHERE Ma=N'QuanTriVien';
                IF NOT EXISTS (SELECT 1 FROM dbo.VaiTro WHERE Ma='USER')
                    INSERT dbo.VaiTro (Id, NgayTao, Ma, Ten) VALUES (NEWID(), SYSUTCDATETIME(), 'USER', N'Nguoi dung');
                IF NOT EXISTS (SELECT 1 FROM dbo.VaiTro WHERE Ma='ADMIN')
                    INSERT dbo.VaiTro (Id, NgayTao, Ma, Ten) VALUES (NEWID(), SYSUTCDATETIME(), 'ADMIN', N'Quan tri vien');
                """);

            migrationBuilder.CreateTable(
                name: "PhienDangNhap",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    PhienBan = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    NguoiDungId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaTruyCap = table.Column<string>(type: "varchar(32)", unicode: false, maxLength: 32, nullable: false),
                    BamRefreshToken = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    NgayHetHan = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    NgayThuHoi = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhienDangNhap", x => x.Id);
                    table.CheckConstraint("CK_PhienDangNhap_HetHan", "[NgayHetHan] > [NgayTao]");
                    table.ForeignKey(
                        name: "FK_PhienDangNhap_NguoiDung_NguoiDungId",
                        column: x => x.NguoiDungId,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "YeuCauDatLaiMatKhau",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    PhienBan = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    NguoiDungId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BamToken = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    NgayHetHan = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: false),
                    NgaySuDung = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", precision: 0, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YeuCauDatLaiMatKhau", x => x.Id);
                    table.CheckConstraint("CK_YeuCauDatLaiMatKhau_HetHan", "[NgayHetHan] > [NgayTao]");
                    table.ForeignKey(
                        name: "FK_YeuCauDatLaiMatKhau_NguoiDung_NguoiDungId",
                        column: x => x.NguoiDungId,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PhienDangNhap_BamRefreshToken",
                schema: "dbo",
                table: "PhienDangNhap",
                column: "BamRefreshToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PhienDangNhap_NguoiDungId_NgayThuHoi",
                schema: "dbo",
                table: "PhienDangNhap",
                columns: new[] { "NguoiDungId", "NgayThuHoi" });

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauDatLaiMatKhau_BamToken",
                schema: "dbo",
                table: "YeuCauDatLaiMatKhau",
                column: "BamToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauDatLaiMatKhau_NguoiDungId_NgayHetHan",
                schema: "dbo",
                table: "YeuCauDatLaiMatKhau",
                columns: new[] { "NguoiDungId", "NgayHetHan" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PhienDangNhap",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "YeuCauDatLaiMatKhau",
                schema: "dbo");
        }
    }
}
