using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ViaTrade.Infrastructure.Migrations
{
	/// <inheritdoc />
	public partial class FixUserStrategyTradeCodeForeignKey : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropForeignKey(
				name: "FK_UserStrategyTradeCodes_TradeStrategies_TradeStrategyId",
				table: "UserStrategyTradeCodes"
			);

			migrationBuilder.DropIndex(
				name: "IX_UserStrategyTradeCodes_TradeStrategyId",
				table: "UserStrategyTradeCodes"
			);

			migrationBuilder.DropColumn(name: "TradeStrategyId", table: "UserStrategyTradeCodes");

			migrationBuilder.CreateIndex(
				name: "IX_UserStrategyTradeCodes_StrategyId",
				table: "UserStrategyTradeCodes",
				column: "StrategyId"
			);

			migrationBuilder.AddForeignKey(
				name: "FK_UserStrategyTradeCodes_TradeStrategies_StrategyId",
				table: "UserStrategyTradeCodes",
				column: "StrategyId",
				principalTable: "TradeStrategies",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade
			);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropForeignKey(
				name: "FK_UserStrategyTradeCodes_TradeStrategies_StrategyId",
				table: "UserStrategyTradeCodes"
			);

			migrationBuilder.DropIndex(name: "IX_UserStrategyTradeCodes_StrategyId", table: "UserStrategyTradeCodes");

			migrationBuilder.AddColumn<int>(
				name: "TradeStrategyId",
				table: "UserStrategyTradeCodes",
				type: "int",
				nullable: true
			);

			migrationBuilder.CreateIndex(
				name: "IX_UserStrategyTradeCodes_TradeStrategyId",
				table: "UserStrategyTradeCodes",
				column: "TradeStrategyId"
			);

			migrationBuilder.AddForeignKey(
				name: "FK_UserStrategyTradeCodes_TradeStrategies_TradeStrategyId",
				table: "UserStrategyTradeCodes",
				column: "TradeStrategyId",
				principalTable: "TradeStrategies",
				principalColumn: "Id"
			);
		}
	}
}
