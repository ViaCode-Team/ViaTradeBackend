using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ViaTrade.Infrastructure.DataBase.Migrations
{
	/// <inheritdoc />
	public partial class AddUserStrategyTradeCodeReverseLookupIndex : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateIndex(
				name: "IX_UserStrategyTradeCodes_UserId_StrategyId_TradeCodeId",
				table: "UserStrategyTradeCodes",
				columns: new[] { "UserId", "StrategyId", "TradeCodeId" }
			);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropIndex(
				name: "IX_UserStrategyTradeCodes_UserId_StrategyId_TradeCodeId",
				table: "UserStrategyTradeCodes"
			);
		}
	}
}
