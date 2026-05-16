using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DOANCOSO26.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBusRouteStop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Stops_BusRoutes_BusRouteId",
                table: "Stops");

            migrationBuilder.DropColumn(
                name: "End",
                table: "BusRoutes");

            migrationBuilder.DropColumn(
                name: "Start",
                table: "BusRoutes");

            migrationBuilder.AlterColumn<int>(
                name: "BusRouteId",
                table: "Stops",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "EndStopId",
                table: "BusRoutes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StartStopId",
                table: "BusRoutes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BusRoutes_EndStopId",
                table: "BusRoutes",
                column: "EndStopId");

            migrationBuilder.CreateIndex(
                name: "IX_BusRoutes_StartStopId",
                table: "BusRoutes",
                column: "StartStopId");

            migrationBuilder.AddForeignKey(
                name: "FK_BusRoutes_Stops_EndStopId",
                table: "BusRoutes",
                column: "EndStopId",
                principalTable: "Stops",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BusRoutes_Stops_StartStopId",
                table: "BusRoutes",
                column: "StartStopId",
                principalTable: "Stops",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Stops_BusRoutes_BusRouteId",
                table: "Stops",
                column: "BusRouteId",
                principalTable: "BusRoutes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BusRoutes_Stops_EndStopId",
                table: "BusRoutes");

            migrationBuilder.DropForeignKey(
                name: "FK_BusRoutes_Stops_StartStopId",
                table: "BusRoutes");

            migrationBuilder.DropForeignKey(
                name: "FK_Stops_BusRoutes_BusRouteId",
                table: "Stops");

            migrationBuilder.DropIndex(
                name: "IX_BusRoutes_EndStopId",
                table: "BusRoutes");

            migrationBuilder.DropIndex(
                name: "IX_BusRoutes_StartStopId",
                table: "BusRoutes");

            migrationBuilder.DropColumn(
                name: "EndStopId",
                table: "BusRoutes");

            migrationBuilder.DropColumn(
                name: "StartStopId",
                table: "BusRoutes");

            migrationBuilder.AlterColumn<int>(
                name: "BusRouteId",
                table: "Stops",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "End",
                table: "BusRoutes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Start",
                table: "BusRoutes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Stops_BusRoutes_BusRouteId",
                table: "Stops",
                column: "BusRouteId",
                principalTable: "BusRoutes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
