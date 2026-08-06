using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IETT.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddDriverCertificateApprovalWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ApprovalStatus",
                table: "DriverCertificates",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE DriverCertificates SET ApprovalStatus = 2 WHERE ApprovalStatus IS NULL");

            migrationBuilder.AlterColumn<int>(
                name: "ApprovalStatus",
                table: "DriverCertificates",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "DriverCertificates",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReviewedByInspectorId",
                table: "DriverCertificates",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedDate",
                table: "DriverCertificates",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DriverCertificates_ReviewedByInspectorId",
                table: "DriverCertificates",
                column: "ReviewedByInspectorId");

            migrationBuilder.AddForeignKey(
                name: "FK_DriverCertificates_Inspectors_ReviewedByInspectorId",
                table: "DriverCertificates",
                column: "ReviewedByInspectorId",
                principalTable: "Inspectors",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DriverCertificates_Inspectors_ReviewedByInspectorId",
                table: "DriverCertificates");

            migrationBuilder.DropIndex(
                name: "IX_DriverCertificates_ReviewedByInspectorId",
                table: "DriverCertificates");

            migrationBuilder.DropColumn(
                name: "ApprovalStatus",
                table: "DriverCertificates");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "DriverCertificates");

            migrationBuilder.DropColumn(
                name: "ReviewedByInspectorId",
                table: "DriverCertificates");

            migrationBuilder.DropColumn(
                name: "ReviewedDate",
                table: "DriverCertificates");
        }
    }
}
