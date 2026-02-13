using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PokerAPIMPwDB.Migrations
{
    /// <inheritdoc />
    public partial class AddisDeleted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isDeleted",
                table: "Tables",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "isDeleted",
                table: "Players",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isDeleted",
                table: "Tables");

            migrationBuilder.DropColumn(
                name: "isDeleted",
                table: "Players");
        }
    }
}
