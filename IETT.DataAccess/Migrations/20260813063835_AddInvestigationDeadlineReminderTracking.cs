using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IETT.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddInvestigationDeadlineReminderTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastFinalDayReminderSentAt",
                table: "Investigations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastOverdueReminderSentAt",
                table: "Investigations",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastFinalDayReminderSentAt",
                table: "Investigations");

            migrationBuilder.DropColumn(
                name: "LastOverdueReminderSentAt",
                table: "Investigations");
        }
    }
}
