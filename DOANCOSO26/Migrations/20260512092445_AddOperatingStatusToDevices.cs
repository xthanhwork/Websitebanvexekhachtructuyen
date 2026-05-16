using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DOANCOSO26.Migrations
{
    /// <inheritdoc />
    public partial class AddOperatingStatusToDevices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndLatitude",
                table: "BusRoutes");

            migrationBuilder.DropColumn(
                name: "EndLongitude",
                table: "BusRoutes");

            migrationBuilder.DropColumn(
                name: "StartLatitude",
                table: "BusRoutes");

            migrationBuilder.DropColumn(
                name: "StartLongitude",
                table: "BusRoutes");

            migrationBuilder.AddColumn<int>(
                name: "OperatingStatus",
                table: "Buses",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OperatingStatus",
                table: "Buses");

            migrationBuilder.AddColumn<string>(
                name: "EndLatitude",
                table: "BusRoutes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EndLongitude",
                table: "BusRoutes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StartLatitude",
                table: "BusRoutes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StartLongitude",
                table: "BusRoutes",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
