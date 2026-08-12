using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IETT.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class EnforceUniqueComplaintTrackingCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                SET NOCOUNT ON;

                CREATE TABLE #TrackingCodeChanges
                (
                    ComplaintId int NOT NULL PRIMARY KEY,
                    NewTrackingCode nvarchar(30) NOT NULL
                );

                ;WITH Parsed AS
                (
                    SELECT
                        [Id],
                        [TrackingCode],
                        YEAR([CreatedDate]) AS [CodeYear],
                        UPPER(LTRIM(RTRIM([TrackingCode]))) AS [NormalizedCode],
                        TRY_CONVERT(int, CASE
                            WHEN UPPER(LTRIM(RTRIM([TrackingCode])))
                                LIKE N'SIK-[0-9][0-9][0-9][0-9]-%'
                            THEN SUBSTRING(UPPER(LTRIM(RTRIM([TrackingCode]))), 10, 50)
                        END) AS [SequenceNumber]
                    FROM [Complaints]
                ),
                Ranked AS
                (
                    SELECT *, ROW_NUMBER() OVER
                    (
                        PARTITION BY [NormalizedCode]
                        ORDER BY [Id]
                    ) AS [DuplicateRank]
                    FROM Parsed
                ),
                MaximumSequences AS
                (
                    SELECT [CodeYear], ISNULL(MAX([SequenceNumber]), 0) AS [MaximumSequence]
                    FROM Parsed
                    WHERE [SequenceNumber] IS NOT NULL
                    GROUP BY [CodeYear]
                ),
                RecordsToChange AS
                (
                    SELECT *, ROW_NUMBER() OVER
                    (
                        PARTITION BY [CodeYear]
                        ORDER BY [Id]
                    ) AS [ChangeRank]
                    FROM Ranked
                    WHERE [DuplicateRank] > 1
                       OR [NormalizedCode] IS NULL
                       OR [NormalizedCode] = N''
                       OR [SequenceNumber] IS NULL
                )
                INSERT INTO #TrackingCodeChanges ([ComplaintId], [NewTrackingCode])
                SELECT
                    record.[Id],
                    CONCAT(
                        N'SIK-', record.[CodeYear], N'-',
                        RIGHT(
                            N'000000' + CONVERT(
                                nvarchar(20),
                                ISNULL(maximum.[MaximumSequence], 0) + record.[ChangeRank]),
                            6))
                FROM RecordsToChange record
                LEFT JOIN MaximumSequences maximum
                    ON maximum.[CodeYear] = record.[CodeYear];

                UPDATE complaint
                SET [TrackingCode] = COALESCE(
                    changes.[NewTrackingCode],
                    UPPER(LTRIM(RTRIM(complaint.[TrackingCode]))))
                FROM [Complaints] complaint
                LEFT JOIN #TrackingCodeChanges changes
                    ON changes.[ComplaintId] = complaint.[Id];

                IF EXISTS
                (
                    SELECT 1
                    FROM [Complaints]
                    GROUP BY UPPER(LTRIM(RTRIM([TrackingCode])))
                    HAVING COUNT(*) > 1
                )
                BEGIN
                    THROW 51040, N'Takip kodu tekilleştirme sonrasında yinelenen kod kaldı; unique index oluşturulmadı.', 1;
                END;

                IF EXISTS
                (
                    SELECT 1 FROM [Complaints]
                    WHERE [TrackingCode] IS NULL
                       OR LTRIM(RTRIM([TrackingCode])) = N''
                       OR LEN([TrackingCode]) > 30
                )
                BEGIN
                    THROW 51041, N'Takip kodu backfill doğrulaması başarısız; unique index oluşturulmadı.', 1;
                END;

                DROP TABLE #TrackingCodeChanges;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "TrackingCode",
                table: "Complaints",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                collation: "Turkish_100_CI_AS",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "UX_Complaints_TrackingCode",
                table: "Complaints",
                column: "TrackingCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_Complaints_TrackingCode",
                table: "Complaints");

            migrationBuilder.AlterColumn<string>(
                name: "TrackingCode",
                table: "Complaints",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldCollation: "Turkish_100_CI_AS");
        }
    }
}
