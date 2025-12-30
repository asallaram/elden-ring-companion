using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EldenRingSim.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlayerProgress",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    PlayerName = table.Column<string>(type: "text", nullable: false),
                    PSN_ID = table.Column<string>(type: "text", nullable: true),
                    CurrentLevel = table.Column<int>(type: "integer", nullable: false),
                    CurrentRunes = table.Column<int>(type: "integer", nullable: false),
                    VisitedLocationIds = table.Column<List<string>>(type: "text[]", nullable: false),
                    DefeatedBossIds = table.Column<List<string>>(type: "text[]", nullable: false),
                    ObtainedWeaponIds = table.Column<List<string>>(type: "text[]", nullable: false),
                    DiscoveredGraceIds = table.Column<List<string>>(type: "text[]", nullable: false),
                    UnlockedRegions = table.Column<List<string>>(type: "text[]", nullable: false),
                    TotalDeaths = table.Column<int>(type: "integer", nullable: false),
                    PlaytimeHours = table.Column<double>(type: "double precision", nullable: false),
                    GreatRunesCollected = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CurrentBuildId = table.Column<string>(type: "text", nullable: true),
                    HasBeatenMargit = table.Column<bool>(type: "boolean", nullable: false),
                    HasBeatenGodrick = table.Column<bool>(type: "boolean", nullable: false),
                    HasBeatenRadahn = table.Column<bool>(type: "boolean", nullable: false),
                    HasBeatenMorgott = table.Column<bool>(type: "boolean", nullable: false),
                    HasBeatenFireGiant = table.Column<bool>(type: "boolean", nullable: false),
                    HasBeatenMalenia = table.Column<bool>(type: "boolean", nullable: false),
                    HasBeatenMaliketh = table.Column<bool>(type: "boolean", nullable: false),
                    HasBeatenEldenBeast = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerProgress", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerProgress");
        }
    }
}
