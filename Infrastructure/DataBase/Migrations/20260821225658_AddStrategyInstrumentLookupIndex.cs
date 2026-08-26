using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ViaTrade.Infrastructure.DataBase.Migrations
{
	/// <inheritdoc />
	public partial class AddStrategyInstrumentLookupIndex : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AlterColumn<double>(
				name: "NetIncome",
				table: "Trades",
				type: "double",
				nullable: true,
				computedColumnSql: "CASE\r\n	WHEN `ClosePrice` IS NULL OR `OpenPrice` = 0 OR `Signal` = 0 THEN NULL\r\n	ELSE ROUND((`ClosePrice` - `OpenPrice`) / `OpenPrice` * 100 * `Signal`, 2)\r\nEND",
				stored: true,
				oldClrType: typeof(double),
				oldType: "double",
				oldNullable: true,
				oldComputedColumnSql: "CASE\n	WHEN `ClosePrice` IS NULL OR `OpenPrice` = 0 OR `Signal` = 0 THEN NULL\n	ELSE ROUND((`ClosePrice` - `OpenPrice`) / `OpenPrice` * 100 * `Signal`, 2)\nEND",
				oldStored: true
			);

			migrationBuilder.CreateIndex(
				name: "IX_UserStrategyInstruments_StrategyId_InstrumentId",
				table: "UserStrategyInstruments",
				columns: new[] { "StrategyId", "InstrumentId" }
			);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropIndex(
				name: "IX_UserStrategyInstruments_StrategyId_InstrumentId",
				table: "UserStrategyInstruments"
			);

			migrationBuilder.AlterColumn<double>(
				name: "NetIncome",
				table: "Trades",
				type: "double",
				nullable: true,
				computedColumnSql: "CASE\n	WHEN `ClosePrice` IS NULL OR `OpenPrice` = 0 OR `Signal` = 0 THEN NULL\n	ELSE ROUND((`ClosePrice` - `OpenPrice`) / `OpenPrice` * 100 * `Signal`, 2)\nEND",
				stored: true,
				oldClrType: typeof(double),
				oldType: "double",
				oldNullable: true,
				oldComputedColumnSql: "CASE\r\n	WHEN `ClosePrice` IS NULL OR `OpenPrice` = 0 OR `Signal` = 0 THEN NULL\r\n	ELSE ROUND((`ClosePrice` - `OpenPrice`) / `OpenPrice` * 100 * `Signal`, 2)\r\nEND",
				oldStored: true
			);
		}
	}
}
