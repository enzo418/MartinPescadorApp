using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FisherTournament.Infrastracture.Persistence.Migrations.Tournament
{
    /// <inheritdoc />
    public partial class StandaloneFisher : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Fishers_Users_UserId",
                table: "Fishers");

            migrationBuilder.DropIndex(
                name: "IX_Fishers_UserId",
                table: "Fishers");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Fishers",
                newName: "Name");

            migrationBuilder.AddColumn<Guid>(
                name: "FisherId",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Number",
                table: "TournamentInscription",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Category",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_FisherId",
                table: "Users",
                column: "FisherId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Fishers_FisherId",
                table: "Users",
                column: "FisherId",
                principalTable: "Fishers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Fishers_FisherId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_FisherId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FisherId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Number",
                table: "TournamentInscription");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Fishers",
                newName: "UserId");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Category",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.CreateIndex(
                name: "IX_Fishers_UserId",
                table: "Fishers",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Fishers_Users_UserId",
                table: "Fishers",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
