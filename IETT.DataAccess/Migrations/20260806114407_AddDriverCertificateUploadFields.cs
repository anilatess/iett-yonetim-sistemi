using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IETT.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddDriverCertificateUploadFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CertificateType",
                table: "DriverCertificates",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "DriverCertificates",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "IssueDate",
                table: "DriverCertificates",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OriginalFileName",
                table: "DriverCertificates",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CertificateType",
                table: "DriverCertificates");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "DriverCertificates");

            migrationBuilder.DropColumn(
                name: "IssueDate",
                table: "DriverCertificates");

            migrationBuilder.DropColumn(
                name: "OriginalFileName",
                table: "DriverCertificates");

        }
    }
}
