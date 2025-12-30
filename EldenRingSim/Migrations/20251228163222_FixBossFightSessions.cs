using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EldenRingSim.Migrations
{
    /// <inheritdoc />
    public partial class FixBossFightSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "WeaponsTriedIds",
                table: "BossFightSessions",
                type: "text",
                nullable: false,
                oldClrType: typeof(List<string>),
                oldType: "text[]");

            migrationBuilder.AddColumn<string>(
                name: "BossFightSessionId",
                table: "BossFightAttempts",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BossFightSessionId",
                table: "BossFightAttempts");

            migrationBuilder.AlterColumn<List<string>>(
                name: "WeaponsTriedIds",
                table: "BossFightSessions",
                type: "text[]",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
