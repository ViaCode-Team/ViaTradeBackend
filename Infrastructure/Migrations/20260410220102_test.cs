using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class test : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserTradeCodes_TradeCodes_TradeCodeId",
                table: "UserTradeCodes");

            migrationBuilder.DropForeignKey(
                name: "FK_UserTradeCodes_Users_UserId",
                table: "UserTradeCodes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserTradeCodes",
                table: "UserTradeCodes");

            migrationBuilder.RenameTable(
                name: "UserTradeCodes",
                newName: "UserTradeCode");

            migrationBuilder.RenameIndex(
                name: "IX_UserTradeCodes_UserId_TradeCodeId",
                table: "UserTradeCode",
                newName: "IX_UserTradeCode_UserId_TradeCodeId");

            migrationBuilder.RenameIndex(
                name: "IX_UserTradeCodes_TradeCodeId",
                table: "UserTradeCode",
                newName: "IX_UserTradeCode_TradeCodeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserTradeCode",
                table: "UserTradeCode",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserTradeCode_TradeCodes_TradeCodeId",
                table: "UserTradeCode",
                column: "TradeCodeId",
                principalTable: "TradeCodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserTradeCode_Users_UserId",
                table: "UserTradeCode",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserTradeCode_TradeCodes_TradeCodeId",
                table: "UserTradeCode");

            migrationBuilder.DropForeignKey(
                name: "FK_UserTradeCode_Users_UserId",
                table: "UserTradeCode");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserTradeCode",
                table: "UserTradeCode");

            migrationBuilder.RenameTable(
                name: "UserTradeCode",
                newName: "UserTradeCodes");

            migrationBuilder.RenameIndex(
                name: "IX_UserTradeCode_UserId_TradeCodeId",
                table: "UserTradeCodes",
                newName: "IX_UserTradeCodes_UserId_TradeCodeId");

            migrationBuilder.RenameIndex(
                name: "IX_UserTradeCode_TradeCodeId",
                table: "UserTradeCodes",
                newName: "IX_UserTradeCodes_TradeCodeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserTradeCodes",
                table: "UserTradeCodes",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserTradeCodes_TradeCodes_TradeCodeId",
                table: "UserTradeCodes",
                column: "TradeCodeId",
                principalTable: "TradeCodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserTradeCodes_Users_UserId",
                table: "UserTradeCodes",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
