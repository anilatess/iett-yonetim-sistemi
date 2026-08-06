using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IETT.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Capacity",
                table: "Vehicles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LicensePlate",
                table: "Vehicles",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Model",
                table: "Vehicles",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductionYear",
                table: "Vehicles",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE [Vehicles]
                SET [LicensePlate] = CONCAT('34 IET ', RIGHT(CONCAT('00000', [Id]), 5)),
                    [Model] = CASE
                        WHEN [DoorNumber] IN ('A-1232', 'A-5001') THEN 'OTOKAR/KENT 290LF'
                        WHEN [DoorNumber] IN ('A-3001', 'A-1905') THEN 'KARSAN/AVANCITY S PLUS'
                        WHEN [Id] % 2 = 0 THEN 'KARSAN/AVANCITY S PLUS'
                        ELSE 'OTOKAR/KENT 290LF'
                    END,
                    [Capacity] = CASE WHEN [Id] % 2 = 0 THEN 145 ELSE 100 END,
                    [ProductionYear] = 2018 + ([Id] % 7)
                WHERE [LicensePlate] IS NULL;
                """);

            migrationBuilder.AlterColumn<int>(name: "Capacity", table: "Vehicles", type: "int", nullable: false, oldClrType: typeof(int), oldType: "int", oldNullable: true);
            migrationBuilder.AlterColumn<string>(name: "LicensePlate", table: "Vehicles", type: "nvarchar(20)", maxLength: 20, nullable: false, oldClrType: typeof(string), oldType: "nvarchar(20)", oldMaxLength: 20, oldNullable: true);
            migrationBuilder.AlterColumn<string>(name: "Model", table: "Vehicles", type: "nvarchar(150)", maxLength: 150, nullable: false, oldClrType: typeof(string), oldType: "nvarchar(150)", oldMaxLength: 150, oldNullable: true);
            migrationBuilder.AlterColumn<int>(name: "ProductionYear", table: "Vehicles", type: "int", nullable: false, oldClrType: typeof(int), oldType: "int", oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_LicensePlate",
                table: "Vehicles",
                column: "LicensePlate",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Vehicles_LicensePlate",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Capacity",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "LicensePlate",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Model",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "ProductionYear",
                table: "Vehicles");
        }
    }
}
