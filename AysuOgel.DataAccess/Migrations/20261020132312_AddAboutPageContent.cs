using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AysuOgel.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddAboutPageContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AboutBlocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Slug = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Text = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    Layout = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AboutBlocks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AboutPageSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Intro = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AboutPageSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AboutBlockImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AboutBlockId = table.Column<int>(type: "int", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Alt = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Caption = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AboutBlockImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AboutBlockImages_AboutBlocks_AboutBlockId",
                        column: x => x.AboutBlockId,
                        principalTable: "AboutBlocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AboutBlocks",
                columns: new[] { "Id", "CreatedAt", "DisplayOrder", "IsActive", "Layout", "Slug", "Text", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 10, 20, 0, 0, 0, 0, DateTimeKind.Utc), 1, true, 1, "mekanimiz", "Bu alan örnek metindir.\nMekanın genel tanıtımı, yaklaşımınız ve hizmet anlayışınız buraya yazılabilir.", "Mekanımız", null },
                    { 2, new DateTime(2026, 10, 20, 0, 0, 0, 0, DateTimeKind.Utc), 2, true, 2, "vaccu-active", "Bu alan örnek metindir.\nUygulamanın ne olduğu, nasıl yapıldığı ve seans bilgileri buraya yazılabilir.", "Vaccu Active", null },
                    { 3, new DateTime(2026, 10, 20, 0, 0, 0, 0, DateTimeKind.Utc), 3, true, 1, "roll-shape", "Bu alan örnek metindir.\nUygulamanın tanıtımı ve seans bilgileri buraya yazılabilir.", "Roll Shape", null },
                    { 4, new DateTime(2026, 10, 20, 0, 0, 0, 0, DateTimeKind.Utc), 4, true, 2, "cilt-bakimi", "Bu alan örnek metindir.\nCilt bakımı hizmetleri ve kullanılan ürünler buraya yazılabilir.", "Profesyonel Cilt Bakımı", null }
                });

            migrationBuilder.InsertData(
                table: "AboutPageSettings",
                columns: new[] { "Id", "Intro", "Title", "UpdatedAt" },
                values: new object[] { 1, "Çankaya, Ankara'da 6 yıldır hizmet veriyoruz. Bu alan örnek metindir; işletmenin kısa tanıtımı buraya gelecek.", "Hakkımızda", null });

            migrationBuilder.CreateIndex(
                name: "IX_AboutBlockImages_AboutBlockId_DisplayOrder",
                table: "AboutBlockImages",
                columns: new[] { "AboutBlockId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_AboutBlocks_IsActive_DisplayOrder",
                table: "AboutBlocks",
                columns: new[] { "IsActive", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_AboutBlocks_Slug",
                table: "AboutBlocks",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AboutBlockImages");

            migrationBuilder.DropTable(
                name: "AboutPageSettings");

            migrationBuilder.DropTable(
                name: "AboutBlocks");
        }
    }
}
