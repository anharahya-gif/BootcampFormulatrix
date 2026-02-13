using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PokerAPIMPwDB.Migrations
{
    /// <inheritdoc />
    public partial class AddBlindsToTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlayerSeats_Players_PlayerId",
                table: "PlayerSeats");

            migrationBuilder.AddColumn<int>(
                name: "BigBlind",
                table: "Tables",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxBuyIn",
                table: "Tables",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinBuyIn",
                table: "Tables",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SmallBlind",
                table: "Tables",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ChipsWonThisRound",
                table: "Players",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<bool>(
                name: "isDeleted",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<int>(
                name: "Balance",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerSeats_Players_PlayerId",
                table: "PlayerSeats",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlayerSeats_Players_PlayerId",
                table: "PlayerSeats");

            migrationBuilder.DropColumn(
                name: "BigBlind",
                table: "Tables");

            migrationBuilder.DropColumn(
                name: "MaxBuyIn",
                table: "Tables");

            migrationBuilder.DropColumn(
                name: "MinBuyIn",
                table: "Tables");

            migrationBuilder.DropColumn(
                name: "SmallBlind",
                table: "Tables");

            migrationBuilder.DropColumn(
                name: "ChipsWonThisRound",
                table: "Players");

            migrationBuilder.AlterColumn<bool>(
                name: "isDeleted",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "Balance",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldDefaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerSeats_Players_PlayerId",
                table: "PlayerSeats",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
