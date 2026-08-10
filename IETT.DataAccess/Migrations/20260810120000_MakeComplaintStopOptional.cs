using IETT.DataAccess.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IETT.DataAccess.Migrations
{
    [DbContext(typeof(IETTDbContext))]
    [Migration("20260810120000_MakeComplaintStopOptional")]
    public partial class MakeComplaintStopOptional : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "StopId",
                table: "Complaints",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF EXISTS (
                    SELECT 1
                    FROM [Complaints]
                    WHERE [StopId] IS NULL
                )
                BEGIN
                    THROW 51020, N'MakeComplaintStopOptional migration geri alınamaz: NULL StopId içeren şikâyetler önce geçerli bir durağa bağlanmalıdır.', 1;
                END;
                """);

            migrationBuilder.AlterColumn<int>(
                name: "StopId",
                table: "Complaints",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
