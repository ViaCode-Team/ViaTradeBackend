using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixUserStrategyTradeCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TradeStrategyId",
                table: "UserStrategyTradeCodes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserStrategyTradeCodes_TradeCodeId",
                table: "UserStrategyTradeCodes",
                column: "TradeCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserStrategyTradeCodes_TradeStrategyId",
                table: "UserStrategyTradeCodes",
                column: "TradeStrategyId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserStrategyTradeCodes_TradeCodes_TradeCodeId",
                table: "UserStrategyTradeCodes",
                column: "TradeCodeId",
                principalTable: "TradeCodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserStrategyTradeCodes_TradeStrategies_TradeStrategyId",
                table: "UserStrategyTradeCodes",
                column: "TradeStrategyId",
                principalTable: "TradeStrategies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserStrategyTradeCodes_Users_UserId",
                table: "UserStrategyTradeCodes",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserStrategyTradeCodes_TradeCodes_TradeCodeId",
                table: "UserStrategyTradeCodes");

            migrationBuilder.DropForeignKey(
                name: "FK_UserStrategyTradeCodes_TradeStrategies_TradeStrategyId",
                table: "UserStrategyTradeCodes");

            migrationBuilder.DropForeignKey(
                name: "FK_UserStrategyTradeCodes_Users_UserId",
                table: "UserStrategyTradeCodes");

            migrationBuilder.DropIndex(
                name: "IX_UserStrategyTradeCodes_TradeCodeId",
                table: "UserStrategyTradeCodes");

            migrationBuilder.DropIndex(
                name: "IX_UserStrategyTradeCodes_TradeStrategyId",
                table: "UserStrategyTradeCodes");

            migrationBuilder.DropColumn(
                name: "TradeStrategyId",
                table: "UserStrategyTradeCodes");
        }
    }
}
