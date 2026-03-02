using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TradeLogic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Удаляем FOREIGN KEY (порядок критичен)
            migrationBuilder.DropForeignKey(
                name: "FK_UserTradeNotes_Users_UserId",
                table: "UserTradeNotes");

            migrationBuilder.DropForeignKey(
                name: "FK_UserTradeNotes_TradeCodes_TradeCodeId",
                table: "UserTradeNotes");

            // 2. Удаляем старый индекс с опечаткой
            migrationBuilder.DropIndex(
                name: "IX_UserTradeNotes_UserId_TadeCodeId",
                table: "UserTradeNotes");

            // 3. Удаляем колонку с опечаткой
            migrationBuilder.DropColumn(
                name: "TadeCodeId",
                table: "UserTradeNotes");

            // 4. Создаём новый корректный индекс
            migrationBuilder.CreateIndex(
                name: "IX_UserTradeNotes_UserId_TradeCodeId",
                table: "UserTradeNotes",
                columns: new[] { "UserId", "TradeCodeId" },
                unique: true);

            // 5. Восстанавливаем FOREIGN KEY
            migrationBuilder.AddForeignKey(
                name: "FK_UserTradeNotes_Users_UserId",
                table: "UserTradeNotes",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserTradeNotes_TradeCodes_TradeCodeId",
                table: "UserTradeNotes",
                column: "TradeCodeId",
                principalTable: "TradeCodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 1. Удаляем новые FK и индекс
            migrationBuilder.DropForeignKey(
                name: "FK_UserTradeNotes_TradeCodes_TradeCodeId",
                table: "UserTradeNotes");

            migrationBuilder.DropForeignKey(
                name: "FK_UserTradeNotes_Users_UserId",
                table: "UserTradeNotes");

            migrationBuilder.DropIndex(
                name: "IX_UserTradeNotes_UserId_TradeCodeId",
                table: "UserTradeNotes");

            // 2. Возвращаем колонку с опечаткой
            migrationBuilder.AddColumn<int>(
                name: "TadeCodeId",
                table: "UserTradeNotes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // 3. Создаём старый индекс
            migrationBuilder.CreateIndex(
                name: "IX_UserTradeNotes_UserId_TadeCodeId",
                table: "UserTradeNotes",
                columns: new[] { "UserId", "TadeCodeId" },
                unique: true);

            // 4. Восстанавливаем старые FK
            migrationBuilder.AddForeignKey(
                name: "FK_UserTradeNotes_Users_UserId",
                table: "UserTradeNotes",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserTradeNotes_TradeCodes_TadeCodeId",
                table: "UserTradeNotes",
                column: "TadeCodeId",
                principalTable: "TradeCodes",
                principalColumn: "Id");
        }
    }
}
