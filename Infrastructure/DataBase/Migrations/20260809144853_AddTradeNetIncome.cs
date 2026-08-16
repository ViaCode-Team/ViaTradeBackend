using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.DataBase.Migrations
{
	/// <inheritdoc />
	public partial class AddTradeNetIncome : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<double>(
				name: "NetIncome",
				table: "Trades",
				type: "double",
				nullable: true,
				computedColumnSql: "CASE\n	WHEN `ExitPrice` IS NULL OR `EntryPrice` = 0 OR `Signal` = 0 THEN NULL\n	ELSE ROUND((`ExitPrice` - `EntryPrice`) / `EntryPrice` * 100 * `Signal`, 2)\nEND",
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
