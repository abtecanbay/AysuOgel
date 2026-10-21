using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AysuOgel.DataAccess.Migrations
{
    /// <summary>
    /// Seans numaraları artık elle girilmiyor, tarih sırasından hesaplanıyor.
    /// Mevcut kayıtlar aynı kurala göre bir kez yeniden numaralandırılır (şema değişikliği yok).
    /// </summary>
    public partial class RenumberMemberSessionsByDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Status: 3 = İptal edildi
            migrationBuilder.Sql("""
                WITH Ordered AS (
                    SELECT Id,
                           ROW_NUMBER() OVER (PARTITION BY MemberPackageId ORDER BY ScheduledAt, CreatedAt, Id) AS Rn
                    FROM MemberSessions
                    WHERE Status <> 3
                )
                UPDATE s SET SessionNumber = o.Rn
                FROM MemberSessions s
                JOIN Ordered o ON o.Id = s.Id
                WHERE s.SessionNumber <> o.Rn;

                UPDATE MemberSessions SET SessionNumber = 0 WHERE Status = 3 AND SessionNumber <> 0;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Eski elle girilen numaralar geri getirilemez; yeniden numaralandırma zararsızdır.
        }
    }
}
