using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ViaTrade.Infrastructure.DataBase.Migrations
{
	/// <inheritdoc />
	public partial class RenameInstrumentSymbolToTicker : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.RenameColumn(name: "Symbol", table: "Instruments", newName: "Ticker");

			migrationBuilder.RenameIndex(
				name: "IX_Instruments_Symbol",
				table: "Instruments",
				newName: "IX_Instruments_Ticker"
			);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.RenameColumn(name: "Ticker", table: "Instruments", newName: "Symbol");

			migrationBuilder.RenameIndex(
				name: "IX_Instruments_Ticker",
				table: "Instruments",
				newName: "IX_Instruments_Symbol"
			);
		}
	}
}
