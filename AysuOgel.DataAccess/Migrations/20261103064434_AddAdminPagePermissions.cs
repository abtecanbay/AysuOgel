using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AysuOgel.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminPagePermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdminPages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    GroupName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Controller = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    RelatedControllers = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminPages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AdminUserPages",
                columns: table => new
                {
                    AdminUserId = table.Column<int>(type: "int", nullable: false),
                    AdminPageId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminUserPages", x => new { x.AdminUserId, x.AdminPageId });
                    table.ForeignKey(
                        name: "FK_AdminUserPages_AdminPages_AdminPageId",
                        column: x => x.AdminPageId,
                        principalTable: "AdminPages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AdminUserPages_AdminUsers_AdminUserId",
                        column: x => x.AdminUserId,
                        principalTable: "AdminUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AdminPages",
                columns: new[] { "Id", "Controller", "Description", "GroupName", "IsActive", "RelatedControllers", "SortOrder", "Title" },
                values: new object[,]
                {
                    { 1, "Services", "Hizmet kartlarını ekleme, düzenleme ve sıralama.", "İçerik", true, null, 1, "Hizmetler" },
                    { 2, "Campaigns", "Kampanyalar, paketler ve kampanya sayfası ayarları.", "İçerik", true, "Packages", 2, "Kampanyalar" },
                    { 3, "SuccessStories", "Başarı hikâyeleri ve önce/sonra görselleri.", "İçerik", true, null, 3, "Başarılarımız" },
                    { 4, "AboutPage", "Hakkımızda sayfasının blokları ve ayarları.", "İçerik", true, null, 4, "Hakkımızda" },
                    { 5, "Members", "Üye kayıtları, paketleri ve seansları.", "Operasyon", true, null, 5, "Üyeler" },
                    { 6, "Messages", "İletişim formundan gelen mesajlar ve yanıtlar.", "Operasyon", true, null, 6, "Mesajlar" },
                    { 7, "Appointments", "Gelen randevu talepleri.", "Operasyon", true, null, 7, "Randevular" },
                    { 8, "Availability", "Randevu alınabilecek müsaitlik saatleri.", "Operasyon", true, null, 8, "Müsaitlik" },
                    { 9, "SiteManagement", "Site ayarları ve yönetici hesapları (yetki verebilir).", "Sistem", true, null, 9, "Sayfa Yönetimi" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdminPages_Controller",
                table: "AdminPages",
                column: "Controller",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdminUserPages_AdminPageId",
                table: "AdminUserPages",
                column: "AdminPageId");

            // Yetki sistemi öncesinde eklenmiş yöneticiler erişimlerini kaybetmesin: tüm sayfaları ver.
            migrationBuilder.Sql(@"
                INSERT INTO AdminUserPages (AdminUserId, AdminPageId)
                SELECT u.Id, p.Id FROM AdminUsers u CROSS JOIN AdminPages p;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminUserPages");

            migrationBuilder.DropTable(
                name: "AdminPages");
        }
    }
}
