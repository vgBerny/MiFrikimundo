using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiFrikimundo.Migrations
{
    /// <inheritdoc />
    public partial class Onetomanybooks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GenderId",
                table: "Books",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Books_GenderId",
                table: "Books",
                column: "GenderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Books_Genders_GenderId",
                table: "Books",
                column: "GenderId",
                principalTable: "Genders",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Books_Genders_GenderId",
                table: "Books");

            migrationBuilder.DropIndex(
                name: "IX_Books_GenderId",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "GenderId",
                table: "Books");
        }
    }
}
