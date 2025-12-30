using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EldenRingSim.Migrations
{
    /// <inheritdoc />
    public partial class AddBossFightTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BossFightAttempts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    PlayerProgressId = table.Column<string>(type: "text", nullable: false),
                    BossId = table.Column<string>(type: "text", nullable: false),
                    BossName = table.Column<string>(type: "text", nullable: false),
                    AttemptNumber = table.Column<int>(type: "integer", nullable: false),
                    WeaponUsedId = table.Column<string>(type: "text", nullable: false),
                    WeaponUsedName = table.Column<string>(type: "text", nullable: false),
                    Victory = table.Column<bool>(type: "boolean", nullable: false),
                    TimeSpentSeconds = table.Column<int>(type: "integer", nullable: false),
                    DamageTaken = table.Column<int>(type: "integer", nullable: true),
                    PlayerLevel = table.Column<int>(type: "integer", nullable: true),
                    AttemptedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BossFightAttempts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BossFightSessions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    PlayerProgressId = table.Column<string>(type: "text", nullable: false),
                    BossId = table.Column<string>(type: "text", nullable: false),
                    BossName = table.Column<string>(type: "text", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Victory = table.Column<bool>(type: "boolean", nullable: false),
                    TotalAttempts = table.Column<int>(type: "integer", nullable: false),
                    WeaponsTriedIds = table.Column<List<string>>(type: "text[]", nullable: false),
                    TotalTimeSpentSeconds = table.Column<int>(type: "integer", nullable: false),
                    SessionNotes = table.Column<string>(type: "text", nullable: true),
                    VictoryWeaponId = table.Column<string>(type: "text", nullable: true),
                    VictoryWeaponName = table.Column<string>(type: "text", nullable: true),
                    VictoryAttemptNumber = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BossFightSessions", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BossFightAttempts");

            migrationBuilder.DropTable(
                name: "BossFightSessions");
        }
    }
}
