using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AysuOgel.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class MemberEmailOrPhone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Members_Email",
                table: "Members");

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Members",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Members",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            // Mevcut kayıtlar: telefon yalnızca rakam olarak saklanır, boş alanlar NULL olur.
            migrationBuilder.Sql("""
                UPDATE [Members] SET [Phone] = NULLIF(
                    REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(
                        LTRIM(RTRIM([Phone])), ' ', ''), '-', ''), '(', ''), ')', ''), '+', ''), '.', ''), '/', ''),
                    '');
                UPDATE [Members] SET [Email] = NULLIF(LTRIM(RTRIM([Email])), '');
                """);

            // Aynı numara birden fazla üyede varsa veri silinmez; migration durur, kayıtlar elle düzeltilmelidir.
            migrationBuilder.Sql("""
                IF EXISTS (SELECT [Phone] FROM [Members] WHERE [Phone] IS NOT NULL GROUP BY [Phone] HAVING COUNT(*) > 1)
                    THROW 50001, 'Members tablosunda aynı telefon numarasına sahip birden fazla üye var. Çakışan kayıtları düzeltip migration''ı tekrar çalıştırın.', 1;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Members_Email",
                table: "Members",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Members_Phone",
                table: "Members",
                column: "Phone",
                unique: true,
                filter: "[Phone] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Members_EmailOrPhone",
                table: "Members",
                sql: "[Email] IS NOT NULL OR [Phone] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Members_Email",
                table: "Members");

            migrationBuilder.DropIndex(
                name: "IX_Members_Phone",
                table: "Members");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Members_EmailOrPhone",
                table: "Members");

            // NOT NULL'a dönmeden önce boşlar doldurulur. E-postasız üyeler eski benzersiz index'i bozmasın diye geçici adres alır.
            migrationBuilder.Sql("""
                UPDATE [Members] SET [Phone] = '' WHERE [Phone] IS NULL;
                UPDATE [Members] SET [Email] = CONCAT('uye', [Id], '@eposta-yok.local') WHERE [Email] IS NULL;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Members",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Members",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Members_Email",
                table: "Members",
                column: "Email",
                unique: true);
        }
    }
}
