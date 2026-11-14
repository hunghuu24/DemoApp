using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReMarket.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TraoDoiMotMot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF NOT EXISTS(SELECT 1 FROM dbo.TieuChiDanhGia WHERE Ma='DUNG_MO_TA')
                    INSERT dbo.TieuChiDanhGia(Id,NgayTao,Ma,Ten) VALUES('F5E8217D-9E01-4DF2-8A82-DB408787A3C3',SYSUTCDATETIME(),'DUNG_MO_TA',N'Đúng mô tả');
                IF NOT EXISTS(SELECT 1 FROM dbo.TieuChiDanhGia WHERE Ma='DUNG_HEN')
                    INSERT dbo.TieuChiDanhGia(Id,NgayTao,Ma,Ten) VALUES('9A00A146-9387-4846-9EDC-6940E42D5EF2',SYSUTCDATETIME(),'DUNG_HEN',N'Đúng hẹn, giao tiếp thân thiện');
                """);
            migrationBuilder.DropCheckConstraint(
                name: "CK_TinDang_TrangThai",
                schema: "dbo",
                table: "TinDang");

            migrationBuilder.DropIndex(
                name: "IX_BangChungGiaoNhan_XacNhanGiaoNhanId",
                schema: "dbo",
                table: "BangChungGiaoNhan");

            migrationBuilder.AddColumn<bool>(
                name: "ChoPhepMotMot",
                schema: "dbo",
                table: "NhuCauTraoDoi",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "GhiChu",
                schema: "dbo",
                table: "NhuCauTraoDoi",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BamYeuCau",
                schema: "dbo",
                table: "DeNghiTraoDoi",
                type: "varchar(64)",
                unicode: false,
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LyDoKetThuc",
                schema: "dbo",
                table: "DeNghiTraoDoi",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MaYeuCau",
                schema: "dbo",
                table: "DeNghiTraoDoi",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TinGuiId",
                schema: "dbo",
                table: "DeNghiTraoDoi",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TinNhanId",
                schema: "dbo",
                table: "DeNghiTraoDoi",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BamNoiDung",
                schema: "dbo",
                table: "BangChungGiaoNhan",
                type: "varchar(64)",
                unicode: false,
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MaYeuCau",
                schema: "dbo",
                table: "BangChungGiaoNhan",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_TinDang_TrangThai",
                schema: "dbo",
                table: "TinDang",
                sql: "[TrangThai] IN (N'BanNhap', N'ChoDuyet', N'DangHienThi', N'TuChoi', N'DaAn', N'DaBan', N'DangGiuCho', N'DaTraoDoi')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_NhuCauTraoDoi_HinhThuc",
                schema: "dbo",
                table: "NhuCauTraoDoi",
                sql: "[ChoPhepMotMot]=1 OR [ChoPhepSmartSwap]=1");

            migrationBuilder.CreateIndex(
                name: "IX_DeNghiTraoDoi_NguoiGuiId_MaYeuCau",
                schema: "dbo",
                table: "DeNghiTraoDoi",
                columns: new[] { "NguoiGuiId", "MaYeuCau" },
                unique: true,
                filter: "[MaYeuCau] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DeNghiTraoDoi_NguoiNhanId_NgayTao",
                schema: "dbo",
                table: "DeNghiTraoDoi",
                columns: new[] { "NguoiNhanId", "NgayTao" });

            migrationBuilder.CreateIndex(
                name: "IX_DeNghiTraoDoi_TinGuiId_SanPhamGuiId",
                schema: "dbo",
                table: "DeNghiTraoDoi",
                columns: new[] { "TinGuiId", "SanPhamGuiId" });

            migrationBuilder.CreateIndex(
                name: "IX_DeNghiTraoDoi_TinNhanId_SanPhamNhanId",
                schema: "dbo",
                table: "DeNghiTraoDoi",
                columns: new[] { "TinNhanId", "SanPhamNhanId" });

            migrationBuilder.CreateIndex(
                name: "IX_DeNghiTraoDoi_TrangThai_NgayHetHan",
                schema: "dbo",
                table: "DeNghiTraoDoi",
                columns: new[] { "TrangThai", "NgayHetHan" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_DeNghiTraoDoi_Tin",
                schema: "dbo",
                table: "DeNghiTraoDoi",
                sql: "([TinGuiId] IS NULL AND [TinNhanId] IS NULL) OR ([TinGuiId] IS NOT NULL AND [TinNhanId] IS NOT NULL AND [TinGuiId]<>[TinNhanId])");

            migrationBuilder.CreateIndex(
                name: "IX_BangChungGiaoNhan_XacNhanGiaoNhanId_MaYeuCau",
                schema: "dbo",
                table: "BangChungGiaoNhan",
                columns: new[] { "XacNhanGiaoNhanId", "MaYeuCau" },
                unique: true,
                filter: "[MaYeuCau] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_DeNghiTraoDoi_TinDang_TinGuiId_SanPhamGuiId",
                schema: "dbo",
                table: "DeNghiTraoDoi",
                columns: new[] { "TinGuiId", "SanPhamGuiId" },
                principalSchema: "dbo",
                principalTable: "TinDang",
                principalColumns: new[] { "Id", "SanPhamId" });

            migrationBuilder.AddForeignKey(
                name: "FK_DeNghiTraoDoi_TinDang_TinNhanId_SanPhamNhanId",
                schema: "dbo",
                table: "DeNghiTraoDoi",
                columns: new[] { "TinNhanId", "SanPhamNhanId" },
                principalSchema: "dbo",
                principalTable: "TinDang",
                principalColumns: new[] { "Id", "SanPhamId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeNghiTraoDoi_TinDang_TinGuiId_SanPhamGuiId",
                schema: "dbo",
                table: "DeNghiTraoDoi");

            migrationBuilder.DropForeignKey(
                name: "FK_DeNghiTraoDoi_TinDang_TinNhanId_SanPhamNhanId",
                schema: "dbo",
                table: "DeNghiTraoDoi");

            migrationBuilder.DropCheckConstraint(
                name: "CK_TinDang_TrangThai",
                schema: "dbo",
                table: "TinDang");

            migrationBuilder.DropCheckConstraint(
                name: "CK_NhuCauTraoDoi_HinhThuc",
                schema: "dbo",
                table: "NhuCauTraoDoi");

            migrationBuilder.DropIndex(
                name: "IX_DeNghiTraoDoi_NguoiGuiId_MaYeuCau",
                schema: "dbo",
                table: "DeNghiTraoDoi");

            migrationBuilder.DropIndex(
                name: "IX_DeNghiTraoDoi_NguoiNhanId_NgayTao",
                schema: "dbo",
                table: "DeNghiTraoDoi");

            migrationBuilder.DropIndex(
                name: "IX_DeNghiTraoDoi_TinGuiId_SanPhamGuiId",
                schema: "dbo",
                table: "DeNghiTraoDoi");

            migrationBuilder.DropIndex(
                name: "IX_DeNghiTraoDoi_TinNhanId_SanPhamNhanId",
                schema: "dbo",
                table: "DeNghiTraoDoi");

            migrationBuilder.DropIndex(
                name: "IX_DeNghiTraoDoi_TrangThai_NgayHetHan",
                schema: "dbo",
                table: "DeNghiTraoDoi");

            migrationBuilder.DropCheckConstraint(
                name: "CK_DeNghiTraoDoi_Tin",
                schema: "dbo",
                table: "DeNghiTraoDoi");

            migrationBuilder.DropIndex(
                name: "IX_BangChungGiaoNhan_XacNhanGiaoNhanId_MaYeuCau",
                schema: "dbo",
                table: "BangChungGiaoNhan");

            migrationBuilder.DropColumn(
                name: "ChoPhepMotMot",
                schema: "dbo",
                table: "NhuCauTraoDoi");

            migrationBuilder.DropColumn(
                name: "GhiChu",
                schema: "dbo",
                table: "NhuCauTraoDoi");

            migrationBuilder.DropColumn(
                name: "BamYeuCau",
                schema: "dbo",
                table: "DeNghiTraoDoi");

            migrationBuilder.DropColumn(
                name: "LyDoKetThuc",
                schema: "dbo",
                table: "DeNghiTraoDoi");

            migrationBuilder.DropColumn(
                name: "MaYeuCau",
                schema: "dbo",
                table: "DeNghiTraoDoi");

            migrationBuilder.DropColumn(
                name: "TinGuiId",
                schema: "dbo",
                table: "DeNghiTraoDoi");

            migrationBuilder.DropColumn(
                name: "TinNhanId",
                schema: "dbo",
                table: "DeNghiTraoDoi");

            migrationBuilder.DropColumn(
                name: "BamNoiDung",
                schema: "dbo",
                table: "BangChungGiaoNhan");

            migrationBuilder.DropColumn(
                name: "MaYeuCau",
                schema: "dbo",
                table: "BangChungGiaoNhan");

            migrationBuilder.AddCheckConstraint(
                name: "CK_TinDang_TrangThai",
                schema: "dbo",
                table: "TinDang",
                sql: "[TrangThai] IN (N'BanNhap', N'ChoDuyet', N'DangHienThi', N'TuChoi', N'DaAn', N'DaBan', N'DangGiuCho')");

            migrationBuilder.CreateIndex(
                name: "IX_BangChungGiaoNhan_XacNhanGiaoNhanId",
                schema: "dbo",
                table: "BangChungGiaoNhan",
                column: "XacNhanGiaoNhanId");
        }
    }
}
