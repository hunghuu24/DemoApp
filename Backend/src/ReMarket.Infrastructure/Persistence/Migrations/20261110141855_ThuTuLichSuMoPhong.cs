using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReMarket.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ThuTuLichSuMoPhong : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "SoThuTu",
                schema: "dbo",
                table: "LichSuVanChuyen",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<long>(
                name: "SoThuTu",
                schema: "dbo",
                table: "LichSuHoTro",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:Identity", "1, 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SoThuTu",
                schema: "dbo",
                table: "LichSuVanChuyen");

            migrationBuilder.DropColumn(
                name: "SoThuTu",
                schema: "dbo",
                table: "LichSuHoTro");
        }
    }
}
