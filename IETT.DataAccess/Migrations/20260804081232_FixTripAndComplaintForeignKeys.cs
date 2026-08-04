using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IETT.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class FixTripAndComplaintForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Complaints_BusRoutes_BusRouteId",
                table: "Complaints");

            migrationBuilder.DropForeignKey(
                name: "FK_Complaints_BusStops_BusStopId",
                table: "Complaints");

            migrationBuilder.DropForeignKey(
                name: "FK_Trips_BusRoutes_BusRouteId",
                table: "Trips");

            migrationBuilder.DropIndex(
                name: "IX_Trips_BusRouteId",
                table: "Trips");

            migrationBuilder.DropIndex(
                name: "IX_Complaints_BusRouteId",
                table: "Complaints");

            migrationBuilder.DropIndex(
                name: "IX_Complaints_BusStopId",
                table: "Complaints");

            migrationBuilder.CreateIndex(
                name: "IX_Trips_RouteId",
                table: "Trips",
                column: "RouteId");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_RouteId",
                table: "Complaints",
                column: "RouteId");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_StopId",
                table: "Complaints",
                column: "StopId");

            migrationBuilder.AddForeignKey(
                name: "FK_Complaints_BusRoutes_RouteId",
                table: "Complaints",
                column: "RouteId",
                principalTable: "BusRoutes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Complaints_BusStops_StopId",
                table: "Complaints",
                column: "StopId",
                principalTable: "BusStops",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Trips_BusRoutes_RouteId",
                table: "Trips",
                column: "RouteId",
                principalTable: "BusRoutes",
                principalColumn: "Id");

            migrationBuilder.DropColumn(
                name: "BusRouteId",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "BusRouteId",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "BusStopId",
                table: "Complaints");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BusRouteId",
                table: "Trips",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BusRouteId",
                table: "Complaints",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BusStopId",
                table: "Complaints",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(
                "UPDATE [Trips] SET [BusRouteId] = [RouteId];");

            migrationBuilder.Sql(
                "UPDATE [Complaints] SET [BusRouteId] = [RouteId], [BusStopId] = [StopId];");

            migrationBuilder.CreateIndex(
                name: "IX_Trips_BusRouteId",
                table: "Trips",
                column: "BusRouteId");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_BusRouteId",
                table: "Complaints",
                column: "BusRouteId");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_BusStopId",
                table: "Complaints",
                column: "BusStopId");

            migrationBuilder.AddForeignKey(
                name: "FK_Complaints_BusRoutes_BusRouteId",
                table: "Complaints",
                column: "BusRouteId",
                principalTable: "BusRoutes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Complaints_BusStops_BusStopId",
                table: "Complaints",
                column: "BusStopId",
                principalTable: "BusStops",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Trips_BusRoutes_BusRouteId",
                table: "Trips",
                column: "BusRouteId",
                principalTable: "BusRoutes",
                principalColumn: "Id");

            migrationBuilder.DropForeignKey(
                name: "FK_Complaints_BusRoutes_RouteId",
                table: "Complaints");

            migrationBuilder.DropForeignKey(
                name: "FK_Complaints_BusStops_StopId",
                table: "Complaints");

            migrationBuilder.DropForeignKey(
                name: "FK_Trips_BusRoutes_RouteId",
                table: "Trips");

            migrationBuilder.DropIndex(
                name: "IX_Trips_RouteId",
                table: "Trips");

            migrationBuilder.DropIndex(
                name: "IX_Complaints_RouteId",
                table: "Complaints");

            migrationBuilder.DropIndex(
                name: "IX_Complaints_StopId",
                table: "Complaints");
        }
    }
}
