using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.DataBase.Migrations
{
	/// <inheritdoc />
	public partial class RestoreTradeNetIncomeAfterPriceRename : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<double>(
				name: "NetIncome",
				table: "Trades",
				type: "double",
				nullable: true,
				computedColumnSql: "CASE\n	WHEN `ClosePrice` IS NULL OR `OpenPrice` = 0 OR `Signal` = 0 THEN NULL\n	ELSE ROUND((`ClosePrice` - `OpenPrice`) / `OpenPrice` * 100 * `Signal`, 2)\nEND",
				stored: true
			);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(name: "NetIncome", table: "Trades");
		}
	}
}
