using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReMarket.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TieuChiTimSmartSwap : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "GiaTriMongMuonDen",
                schema: "dbo",
                table: "NhuCauTraoDoi",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "GiaTriMongMuonTu",
                schema: "dbo",
                table: "NhuCauTraoDoi",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TinhTrangToiThieu",
                schema: "dbo",
                table: "NhuCauTraoDoi",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TuKhoaMongMuon",
                schema: "dbo",
                table: "NhuCauTraoDoi",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_NhuCauTraoDoi_GiaMongMuon",
                schema: "dbo",
                table: "NhuCauTraoDoi",
                sql: "([GiaTriMongMuonTu] IS NULL OR [GiaTriMongMuonTu] BETWEEN 0 AND 1000000000000) AND ([GiaTriMongMuonDen] IS NULL OR [GiaTriMongMuonDen] BETWEEN 0 AND 1000000000000) AND ([GiaTriMongMuonTu] IS NULL OR [GiaTriMongMuonDen] IS NULL OR [GiaTriMongMuonTu]<=[GiaTriMongMuonDen])");

            migrationBuilder.AddCheckConstraint(
                name: "CK_NhuCauTraoDoi_TinhTrang",
                schema: "dbo",
                table: "NhuCauTraoDoi",
                sql: "[TinhTrangToiThieu] IS NULL OR [TinhTrangToiThieu] IN ('NhuMoi','DaSuDungTot','CoLoiNhe')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_NhuCauTraoDoi_GiaMongMuon",
                schema: "dbo",
                table: "NhuCauTraoDoi");

            migrationBuilder.DropCheckConstraint(
                name: "CK_NhuCauTraoDoi_TinhTrang",
                schema: "dbo",
                table: "NhuCauTraoDoi");

            migrationBuilder.DropColumn(
                name: "GiaTriMongMuonDen",
                schema: "dbo",
                table: "NhuCauTraoDoi");

            migrationBuilder.DropColumn(
                name: "GiaTriMongMuonTu",
                schema: "dbo",
                table: "NhuCauTraoDoi");

            migrationBuilder.DropColumn(
                name: "TinhTrangToiThieu",
                schema: "dbo",
                table: "NhuCauTraoDoi");

            migrationBuilder.DropColumn(
                name: "TuKhoaMongMuon",
                schema: "dbo",
                table: "NhuCauTraoDoi");
        }
    }
}
