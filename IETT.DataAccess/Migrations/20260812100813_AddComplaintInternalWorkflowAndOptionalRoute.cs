using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IETT.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddComplaintInternalWorkflowAndOptionalRoute : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "IF EXISTS (SELECT 1 FROM [Investigations] WHERE LEN([InvestigationResult]) > 1000) THROW 51030, N'InvestigationResult alanında 1000 karakteri aşan kayıtlar bulundu; migration veri kaybını önlemek için durduruldu.', 1;");

            migrationBuilder.AlterColumn<string>(
                name: "InvestigationResult",
                table: "Investigations",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DriverExplanation",
                table: "Investigations",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DriverExplanationDate",
                table: "Investigations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProcessStatus",
                table: "Investigations",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.Sql(
                "UPDATE [Investigations] SET [ProcessStatus] = CASE WHEN [ClosedDate] IS NOT NULL THEN 4 WHEN [InvestigationResult] IS NULL THEN 1 ELSE 1 END;");

            migrationBuilder.AlterColumn<int>(
                name: "RouteId",
                table: "Complaints",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "IF EXISTS (SELECT 1 FROM [Complaints] WHERE [RouteId] IS NULL) THROW 51031, N'Migration geri alınamaz: NULL RouteId içeren şikâyetler önce geçerli bir hatta bağlanmalıdır.', 1;");
            migrationBuilder.DropColumn(
                name: "DriverExplanation",
                table: "Investigations");

            migrationBuilder.DropColumn(
                name: "DriverExplanationDate",
                table: "Investigations");

            migrationBuilder.DropColumn(
                name: "ProcessStatus",
                table: "Investigations");

            migrationBuilder.AlterColumn<string>(
                name: "InvestigationResult",
                table: "Investigations",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "RouteId",
                table: "Complaints",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
