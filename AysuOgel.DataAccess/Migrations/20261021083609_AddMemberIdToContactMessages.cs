using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AysuOgel.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddMemberIdToContactMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MemberId",
                table: "ContactMessages",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContactMessages_MemberId",
                table: "ContactMessages",
                column: "MemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_ContactMessages_Members_MemberId",
                table: "ContactMessages",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContactMessages_Members_MemberId",
                table: "ContactMessages");

            migrationBuilder.DropIndex(
                name: "IX_ContactMessages_MemberId",
                table: "ContactMessages");

            migrationBuilder.DropColumn(
                name: "MemberId",
                table: "ContactMessages");
        }
    }
}
