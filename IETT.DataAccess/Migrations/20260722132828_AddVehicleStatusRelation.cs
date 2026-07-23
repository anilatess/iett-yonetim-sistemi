using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IETT.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleStatusRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Capacity",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "PlateNumber",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "VehicleBrand",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "VehicleModel",
                table: "Vehicles");

            migrationBuilder.RenameColumn(
                name: "VehicleStatus",
                table: "Vehicles",
                newName: "VehicleStatusId");

            migrationBuilder.CreateTable(
                name: "VehicleStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleStatuses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_VehicleStatusId",
                table: "Vehicles",
                column: "VehicleStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_VehicleStatuses_VehicleStatusId",
                table: "Vehicles",
                column: "VehicleStatusId",
                principalTable: "VehicleStatuses",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_VehicleStatuses_VehicleStatusId",
                table: "Vehicles");

            migrationBuilder.DropTable(
                name: "VehicleStatuses");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_VehicleStatusId",
                table: "Vehicles");

            migrationBuilder.RenameColumn(
                name: "VehicleStatusId",
                table: "Vehicles",
                newName: "VehicleStatus");

            migrationBuilder.AddColumn<int>(
                name: "Capacity",
                table: "Vehicles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PlateNumber",
                table: "Vehicles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VehicleBrand",
                table: "Vehicles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VehicleModel",
                table: "Vehicles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
