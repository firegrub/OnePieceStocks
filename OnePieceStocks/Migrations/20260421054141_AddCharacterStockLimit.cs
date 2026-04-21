using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnePieceStocks.Migrations
{
    /// <inheritdoc />
    public partial class AddCharacterStockLimit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxStockUnits",
                table: "Characters",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxStockUnits",
                table: "Characters");
        }
    }
}
