using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnePieceStocks.Migrations
{
    /// <inheritdoc />
    public partial class AddTradingWindow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsTradingOpen",
                table: "GameSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "TradingClosesAtUtc",
                table: "GameSettings",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsTradingOpen",
                table: "GameSettings");

            migrationBuilder.DropColumn(
                name: "TradingClosesAtUtc",
                table: "GameSettings");
        }
    }
}
