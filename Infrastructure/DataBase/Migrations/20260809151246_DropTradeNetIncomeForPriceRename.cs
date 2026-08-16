using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ViaTrade.Infrastructure.DataBase.Migrations
{
	/// <inheritdoc />
	public partial class DropTradeNetIncomeForPriceRename : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(name: "NetIncome", table: "Trades");

			migrationBuilder.RenameColumn(name: "ExitPrice", table: "Trades", newName: "ClosePrice");

			migrationBuilder.RenameColumn(name: "EntryPrice", table: "Trades", newName: "OpenPrice");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.RenameColumn(name: "OpenPrice", table: "Trades", newName: "EntryPrice");

			migrationBuilder.RenameColumn(name: "ClosePrice", table: "Trades", newName: "ExitPrice");

			migrationBuilder.AddColumn<double>(
				name: "NetIncome",
				table: "Trades",
				type: "double",
				nullable: true,
				computedColumnSql: "CASE\n	WHEN `ExitPrice` IS NULL OR `EntryPrice` = 0 OR `Signal` = 0 THEN NULL\n	ELSE ROUND((`ExitPrice` - `EntryPrice`) / `EntryPrice` * 100 * `Signal`, 2)\nEND",
				stored: true
			);
		}
	}
}
