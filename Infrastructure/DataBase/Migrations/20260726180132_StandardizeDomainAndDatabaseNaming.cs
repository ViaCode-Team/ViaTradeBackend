using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.DataBase.Migrations
{
	/// <inheritdoc />
	public partial class StandardizeDomainAndDatabaseNaming : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropForeignKey(name: "FK_Notes_TradeCodes_TradeCodeId", table: "Notes");

			migrationBuilder.DropForeignKey(name: "FK_Notes_TradeStrategies_TradeStrategyId", table: "Notes");

			migrationBuilder.DropForeignKey(name: "FK_TradeReminds_TradeCodes_TradeCodeId", table: "TradeReminds");

			migrationBuilder.DropForeignKey(name: "FK_TradeReminds_Users_UserId", table: "TradeReminds");

			migrationBuilder.DropForeignKey(name: "FK_Trades_TradeCodes_TradeCodeId", table: "Trades");

			migrationBuilder.DropForeignKey(
				name: "FK_UserStrategyTradeCodes_TradeCodes_TradeCodeId",
				table: "UserStrategyTradeCodes"
			);

			migrationBuilder.DropForeignKey(
				name: "FK_UserStrategyTradeCodes_TradeStrategies_StrategyId",
				table: "UserStrategyTradeCodes"
			);

			migrationBuilder.DropForeignKey(
				name: "FK_UserStrategyTradeCodes_Users_UserId",
				table: "UserStrategyTradeCodes"
			);

			migrationBuilder.DropForeignKey(name: "FK_UserTradeCode_TradeCodes_TradeCodeId", table: "UserTradeCode");

			migrationBuilder.DropForeignKey(name: "FK_UserTradeCode_Users_UserId", table: "UserTradeCode");

			migrationBuilder.DropForeignKey(
				name: "FK_UserTradeStrategies_TradeStrategies_TradeStrategyId",
				table: "UserTradeStrategies"
			);

			migrationBuilder.DropForeignKey(name: "FK_UserTradeStrategies_Users_UserId", table: "UserTradeStrategies");

			migrationBuilder.DropCheckConstraint(name: "CK_Trade_PositiveCount", table: "Trades");

			migrationBuilder.DropCheckConstraint(name: "CK_Note_ExclusiveTarget", table: "Notes");

			migrationBuilder.DropPrimaryKey(name: "PK_UserTradeStrategies", table: "UserTradeStrategies");

			migrationBuilder.DropPrimaryKey(name: "PK_UserTradeCode", table: "UserTradeCode");

			migrationBuilder.DropPrimaryKey(name: "PK_UserStrategyTradeCodes", table: "UserStrategyTradeCodes");

			migrationBuilder.DropPrimaryKey(name: "PK_TradeStrategies", table: "TradeStrategies");

			migrationBuilder.DropPrimaryKey(name: "PK_TradeReminds", table: "TradeReminds");

			migrationBuilder.DropPrimaryKey(name: "PK_TradeCodes", table: "TradeCodes");

			migrationBuilder.RenameTable(name: "UserTradeStrategies", newName: "UserStrategies");

			migrationBuilder.RenameTable(name: "UserTradeCode", newName: "UserInstruments");

			migrationBuilder.RenameTable(name: "UserStrategyTradeCodes", newName: "UserStrategyInstruments");

			migrationBuilder.RenameTable(name: "TradeStrategies", newName: "Strategies");

			migrationBuilder.RenameTable(name: "TradeReminds", newName: "Reminders");

			migrationBuilder.RenameTable(name: "TradeCodes", newName: "Instruments");

			migrationBuilder.RenameColumn(name: "TgId", table: "Users", newName: "TelegramId");

			migrationBuilder.RenameColumn(name: "RegisterDate", table: "Users", newName: "RegisteredAt");

			migrationBuilder.RenameColumn(name: "LastLoginDate", table: "Users", newName: "LastLoginAt");

			migrationBuilder.RenameColumn(name: "HashPassword", table: "Users", newName: "PasswordHash");

			migrationBuilder.RenameColumn(name: "TradeSignal", table: "Trades", newName: "Signal");

			migrationBuilder.RenameColumn(name: "TradeOpen", table: "Trades", newName: "EntryPrice");

			migrationBuilder.RenameColumn(name: "TradeCodeId", table: "Trades", newName: "InstrumentId");

			migrationBuilder.RenameColumn(name: "TradeClose", table: "Trades", newName: "ExitPrice");

			migrationBuilder.RenameColumn(name: "Price", table: "Trades", newName: "TotalPrice");

			migrationBuilder.RenameColumn(name: "DateOpen", table: "Trades", newName: "OpenedAt");

			migrationBuilder.RenameColumn(name: "DateClose", table: "Trades", newName: "ClosedAt");

			migrationBuilder.RenameColumn(name: "Count", table: "Trades", newName: "Quantity");

			migrationBuilder.RenameIndex(
				name: "IX_Trades_TradeCodeId",
				table: "Trades",
				newName: "IX_Trades_InstrumentId"
			);

			migrationBuilder.RenameColumn(name: "TradeStrategyId", table: "Notes", newName: "StrategyId");

			migrationBuilder.RenameColumn(name: "TradeCodeId", table: "Notes", newName: "InstrumentId");

			migrationBuilder.RenameColumn(name: "NoteText", table: "Notes", newName: "Text");

			migrationBuilder.RenameIndex(
				name: "IX_Notes_UserId_TradeStrategyId",
				table: "Notes",
				newName: "IX_Notes_UserId_StrategyId"
			);

			migrationBuilder.RenameIndex(
				name: "IX_Notes_UserId_TradeCodeId",
				table: "Notes",
				newName: "IX_Notes_UserId_InstrumentId"
			);

			migrationBuilder.RenameIndex(
				name: "IX_Notes_TradeStrategyId",
				table: "Notes",
				newName: "IX_Notes_StrategyId"
			);

			migrationBuilder.RenameIndex(
				name: "IX_Notes_TradeCodeId",
				table: "Notes",
				newName: "IX_Notes_InstrumentId"
			);

			migrationBuilder.RenameColumn(name: "TradeStrategyId", table: "UserStrategies", newName: "StrategyId");

			migrationBuilder.RenameIndex(
				name: "IX_UserTradeStrategies_UserId_TradeStrategyId",
				table: "UserStrategies",
				newName: "IX_UserStrategies_UserId_StrategyId"
			);

			migrationBuilder.RenameIndex(
				name: "IX_UserTradeStrategies_TradeStrategyId",
				table: "UserStrategies",
				newName: "IX_UserStrategies_StrategyId"
			);

			migrationBuilder.RenameColumn(name: "TradeCodeId", table: "UserInstruments", newName: "InstrumentId");

			migrationBuilder.RenameIndex(
				name: "IX_UserTradeCode_UserId_TradeCodeId",
				table: "UserInstruments",
				newName: "IX_UserInstruments_UserId_InstrumentId"
			);

			migrationBuilder.RenameIndex(
				name: "IX_UserTradeCode_TradeCodeId",
				table: "UserInstruments",
				newName: "IX_UserInstruments_InstrumentId"
			);

			migrationBuilder.RenameColumn(
				name: "TradeCodeId",
				table: "UserStrategyInstruments",
				newName: "InstrumentId"
			);

			migrationBuilder.RenameIndex(
				name: "IX_UserStrategyTradeCodes_UserId_TradeCodeId_StrategyId",
				table: "UserStrategyInstruments",
				newName: "IX_UserStrategyInstruments_UserId_InstrumentId_StrategyId"
			);

			migrationBuilder.RenameIndex(
				name: "IX_UserStrategyTradeCodes_UserId_StrategyId_TradeCodeId",
				table: "UserStrategyInstruments",
				newName: "IX_UserStrategyInstruments_UserId_StrategyId_InstrumentId"
			);

			migrationBuilder.RenameIndex(
				name: "IX_UserStrategyTradeCodes_TradeCodeId",
				table: "UserStrategyInstruments",
				newName: "IX_UserStrategyInstruments_InstrumentId"
			);

			migrationBuilder.RenameIndex(
				name: "IX_UserStrategyTradeCodes_StrategyId",
				table: "UserStrategyInstruments",
				newName: "IX_UserStrategyInstruments_StrategyId"
			);

			migrationBuilder.RenameColumn(name: "UseDesc", table: "Strategies", newName: "UsageDescription");

			migrationBuilder.RenameColumn(name: "LogicDesc", table: "Strategies", newName: "LogicDescription");

			migrationBuilder.RenameColumn(name: "LimitDesc", table: "Strategies", newName: "LimitationsDescription");

			migrationBuilder.RenameIndex(
				name: "IX_TradeStrategies_Name",
				table: "Strategies",
				newName: "IX_Strategies_Name"
			);

			migrationBuilder.RenameColumn(name: "TradeCodeId", table: "Reminders", newName: "InstrumentId");

			migrationBuilder.RenameColumn(name: "TextRemind", table: "Reminders", newName: "Text");

			migrationBuilder.RenameColumn(name: "DateTime", table: "Reminders", newName: "RemindAt");

			migrationBuilder.RenameIndex(
				name: "IX_TradeReminds_UserId",
				table: "Reminders",
				newName: "IX_Reminders_UserId"
			);

			migrationBuilder.RenameIndex(
				name: "IX_TradeReminds_TradeCodeId",
				table: "Reminders",
				newName: "IX_Reminders_InstrumentId"
			);

			migrationBuilder.RenameColumn(name: "ExchangeId", table: "Instruments", newName: "Symbol");

			migrationBuilder.RenameIndex(
				name: "IX_TradeCodes_ExchangeId",
				table: "Instruments",
				newName: "IX_Instruments_Symbol"
			);

			migrationBuilder.AddPrimaryKey(name: "PK_UserStrategies", table: "UserStrategies", column: "Id");

			migrationBuilder.AddPrimaryKey(name: "PK_UserInstruments", table: "UserInstruments", column: "Id");

			migrationBuilder.AddPrimaryKey(
				name: "PK_UserStrategyInstruments",
				table: "UserStrategyInstruments",
				column: "Id"
			);

			migrationBuilder.AddPrimaryKey(name: "PK_Strategies", table: "Strategies", column: "Id");

			migrationBuilder.AddPrimaryKey(name: "PK_Reminders", table: "Reminders", column: "Id");

			migrationBuilder.AddPrimaryKey(name: "PK_Instruments", table: "Instruments", column: "Id");

			migrationBuilder.AddCheckConstraint(
				name: "CK_Trades_PositiveQuantity",
				table: "Trades",
				sql: "`Quantity` > 0"
			);

			migrationBuilder.AddCheckConstraint(
				name: "CK_Notes_ExclusiveTarget",
				table: "Notes",
				sql: "(`InstrumentId` IS NOT NULL AND `StrategyId` IS NULL) OR (`InstrumentId` IS NULL AND `StrategyId` IS NOT NULL)"
			);

			migrationBuilder.AddForeignKey(
				name: "FK_Notes_Instruments_InstrumentId",
				table: "Notes",
				column: "InstrumentId",
				principalTable: "Instruments",
				principalColumn: "Id"
			);

			migrationBuilder.AddForeignKey(
				name: "FK_Notes_Strategies_StrategyId",
				table: "Notes",
				column: "StrategyId",
				principalTable: "Strategies",
				principalColumn: "Id"
			);

			migrationBuilder.AddForeignKey(
				name: "FK_Reminders_Instruments_InstrumentId",
				table: "Reminders",
				column: "InstrumentId",
				principalTable: "Instruments",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade
			);

			migrationBuilder.AddForeignKey(
				name: "FK_Reminders_Users_UserId",
				table: "Reminders",
				column: "UserId",
				principalTable: "Users",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade
			);

			migrationBuilder.AddForeignKey(
				name: "FK_Trades_Instruments_InstrumentId",
				table: "Trades",
				column: "InstrumentId",
				principalTable: "Instruments",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade
			);

			migrationBuilder.AddForeignKey(
				name: "FK_UserInstruments_Instruments_InstrumentId",
				table: "UserInstruments",
				column: "InstrumentId",
				principalTable: "Instruments",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade
			);

			migrationBuilder.AddForeignKey(
				name: "FK_UserInstruments_Users_UserId",
				table: "UserInstruments",
				column: "UserId",
				principalTable: "Users",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade
			);

			migrationBuilder.AddForeignKey(
				name: "FK_UserStrategies_Strategies_StrategyId",
				table: "UserStrategies",
				column: "StrategyId",
				principalTable: "Strategies",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade
			);

			migrationBuilder.AddForeignKey(
				name: "FK_UserStrategies_Users_UserId",
				table: "UserStrategies",
				column: "UserId",
				principalTable: "Users",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade
			);

			migrationBuilder.AddForeignKey(
				name: "FK_UserStrategyInstruments_Instruments_InstrumentId",
				table: "UserStrategyInstruments",
				column: "InstrumentId",
				principalTable: "Instruments",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade
			);

			migrationBuilder.AddForeignKey(
				name: "FK_UserStrategyInstruments_Strategies_StrategyId",
				table: "UserStrategyInstruments",
				column: "StrategyId",
				principalTable: "Strategies",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade
			);

			migrationBuilder.AddForeignKey(
				name: "FK_UserStrategyInstruments_Users_UserId",
				table: "UserStrategyInstruments",
				column: "UserId",
				principalTable: "Users",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade
			);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropForeignKey(name: "FK_Notes_Instruments_InstrumentId", table: "Notes");

			migrationBuilder.DropForeignKey(name: "FK_Notes_Strategies_StrategyId", table: "Notes");

			migrationBuilder.DropForeignKey(name: "FK_Reminders_Instruments_InstrumentId", table: "Reminders");

			migrationBuilder.DropForeignKey(name: "FK_Reminders_Users_UserId", table: "Reminders");

			migrationBuilder.DropForeignKey(name: "FK_Trades_Instruments_InstrumentId", table: "Trades");

			migrationBuilder.DropForeignKey(
				name: "FK_UserInstruments_Instruments_InstrumentId",
				table: "UserInstruments"
			);

			migrationBuilder.DropForeignKey(name: "FK_UserInstruments_Users_UserId", table: "UserInstruments");

			migrationBuilder.DropForeignKey(name: "FK_UserStrategies_Strategies_StrategyId", table: "UserStrategies");

			migrationBuilder.DropForeignKey(name: "FK_UserStrategies_Users_UserId", table: "UserStrategies");

			migrationBuilder.DropForeignKey(
				name: "FK_UserStrategyInstruments_Instruments_InstrumentId",
				table: "UserStrategyInstruments"
			);

			migrationBuilder.DropForeignKey(
				name: "FK_UserStrategyInstruments_Strategies_StrategyId",
				table: "UserStrategyInstruments"
			);

			migrationBuilder.DropForeignKey(
				name: "FK_UserStrategyInstruments_Users_UserId",
				table: "UserStrategyInstruments"
			);

			migrationBuilder.DropCheckConstraint(name: "CK_Trades_PositiveQuantity", table: "Trades");

			migrationBuilder.DropCheckConstraint(name: "CK_Notes_ExclusiveTarget", table: "Notes");

			migrationBuilder.DropPrimaryKey(name: "PK_UserStrategyInstruments", table: "UserStrategyInstruments");

			migrationBuilder.DropPrimaryKey(name: "PK_UserStrategies", table: "UserStrategies");

			migrationBuilder.DropPrimaryKey(name: "PK_UserInstruments", table: "UserInstruments");

			migrationBuilder.DropPrimaryKey(name: "PK_Strategies", table: "Strategies");

			migrationBuilder.DropPrimaryKey(name: "PK_Reminders", table: "Reminders");

			migrationBuilder.DropPrimaryKey(name: "PK_Instruments", table: "Instruments");

			migrationBuilder.RenameTable(name: "UserStrategyInstruments", newName: "UserStrategyTradeCodes");

			migrationBuilder.RenameTable(name: "UserStrategies", newName: "UserTradeStrategies");

			migrationBuilder.RenameTable(name: "UserInstruments", newName: "UserTradeCode");

			migrationBuilder.RenameTable(name: "Strategies", newName: "TradeStrategies");

			migrationBuilder.RenameTable(name: "Reminders", newName: "TradeReminds");

			migrationBuilder.RenameTable(name: "Instruments", newName: "TradeCodes");

			migrationBuilder.RenameColumn(name: "TelegramId", table: "Users", newName: "TgId");

			migrationBuilder.RenameColumn(name: "RegisteredAt", table: "Users", newName: "RegisterDate");

			migrationBuilder.RenameColumn(name: "PasswordHash", table: "Users", newName: "HashPassword");

			migrationBuilder.RenameColumn(name: "LastLoginAt", table: "Users", newName: "LastLoginDate");

			migrationBuilder.RenameColumn(name: "TotalPrice", table: "Trades", newName: "Price");

			migrationBuilder.RenameColumn(name: "Signal", table: "Trades", newName: "TradeSignal");

			migrationBuilder.RenameColumn(name: "Quantity", table: "Trades", newName: "Count");

			migrationBuilder.RenameColumn(name: "OpenedAt", table: "Trades", newName: "DateOpen");

			migrationBuilder.RenameColumn(name: "InstrumentId", table: "Trades", newName: "TradeCodeId");

			migrationBuilder.RenameColumn(name: "ExitPrice", table: "Trades", newName: "TradeClose");

			migrationBuilder.RenameColumn(name: "EntryPrice", table: "Trades", newName: "TradeOpen");

			migrationBuilder.RenameColumn(name: "ClosedAt", table: "Trades", newName: "DateClose");

			migrationBuilder.RenameIndex(
				name: "IX_Trades_InstrumentId",
				table: "Trades",
				newName: "IX_Trades_TradeCodeId"
			);

			migrationBuilder.RenameColumn(name: "Text", table: "Notes", newName: "NoteText");

			migrationBuilder.RenameColumn(name: "StrategyId", table: "Notes", newName: "TradeStrategyId");

			migrationBuilder.RenameColumn(name: "InstrumentId", table: "Notes", newName: "TradeCodeId");

			migrationBuilder.RenameIndex(
				name: "IX_Notes_UserId_StrategyId",
				table: "Notes",
				newName: "IX_Notes_UserId_TradeStrategyId"
			);

			migrationBuilder.RenameIndex(
				name: "IX_Notes_UserId_InstrumentId",
				table: "Notes",
				newName: "IX_Notes_UserId_TradeCodeId"
			);

			migrationBuilder.RenameIndex(
				name: "IX_Notes_StrategyId",
				table: "Notes",
				newName: "IX_Notes_TradeStrategyId"
			);

			migrationBuilder.RenameIndex(
				name: "IX_Notes_InstrumentId",
				table: "Notes",
				newName: "IX_Notes_TradeCodeId"
			);

			migrationBuilder.RenameColumn(
				name: "InstrumentId",
				table: "UserStrategyTradeCodes",
				newName: "TradeCodeId"
			);

			migrationBuilder.RenameIndex(
				name: "IX_UserStrategyInstruments_UserId_StrategyId_InstrumentId",
				table: "UserStrategyTradeCodes",
				newName: "IX_UserStrategyTradeCodes_UserId_StrategyId_TradeCodeId"
			);

			migrationBuilder.RenameIndex(
				name: "IX_UserStrategyInstruments_UserId_InstrumentId_StrategyId",
				table: "UserStrategyTradeCodes",
				newName: "IX_UserStrategyTradeCodes_UserId_TradeCodeId_StrategyId"
			);

			migrationBuilder.RenameIndex(
				name: "IX_UserStrategyInstruments_StrategyId",
				table: "UserStrategyTradeCodes",
				newName: "IX_UserStrategyTradeCodes_StrategyId"
			);

			migrationBuilder.RenameIndex(
				name: "IX_UserStrategyInstruments_InstrumentId",
				table: "UserStrategyTradeCodes",
				newName: "IX_UserStrategyTradeCodes_TradeCodeId"
			);

			migrationBuilder.RenameColumn(name: "StrategyId", table: "UserTradeStrategies", newName: "TradeStrategyId");

			migrationBuilder.RenameIndex(
				name: "IX_UserStrategies_UserId_StrategyId",
				table: "UserTradeStrategies",
				newName: "IX_UserTradeStrategies_UserId_TradeStrategyId"
			);

			migrationBuilder.RenameIndex(
				name: "IX_UserStrategies_StrategyId",
				table: "UserTradeStrategies",
				newName: "IX_UserTradeStrategies_TradeStrategyId"
			);

			migrationBuilder.RenameColumn(name: "InstrumentId", table: "UserTradeCode", newName: "TradeCodeId");

			migrationBuilder.RenameIndex(
				name: "IX_UserInstruments_UserId_InstrumentId",
				table: "UserTradeCode",
				newName: "IX_UserTradeCode_UserId_TradeCodeId"
			);

			migrationBuilder.RenameIndex(
				name: "IX_UserInstruments_InstrumentId",
				table: "UserTradeCode",
				newName: "IX_UserTradeCode_TradeCodeId"
			);

			migrationBuilder.RenameColumn(name: "UsageDescription", table: "TradeStrategies", newName: "UseDesc");

			migrationBuilder.RenameColumn(name: "LogicDescription", table: "TradeStrategies", newName: "LogicDesc");

			migrationBuilder.RenameColumn(
				name: "LimitationsDescription",
				table: "TradeStrategies",
				newName: "LimitDesc"
			);

			migrationBuilder.RenameIndex(
				name: "IX_Strategies_Name",
				table: "TradeStrategies",
				newName: "IX_TradeStrategies_Name"
			);

			migrationBuilder.RenameColumn(name: "Text", table: "TradeReminds", newName: "TextRemind");

			migrationBuilder.RenameColumn(name: "RemindAt", table: "TradeReminds", newName: "DateTime");

			migrationBuilder.RenameColumn(name: "InstrumentId", table: "TradeReminds", newName: "TradeCodeId");

			migrationBuilder.RenameIndex(
				name: "IX_Reminders_UserId",
				table: "TradeReminds",
				newName: "IX_TradeReminds_UserId"
			);

			migrationBuilder.RenameIndex(
				name: "IX_Reminders_InstrumentId",
				table: "TradeReminds",
				newName: "IX_TradeReminds_TradeCodeId"
			);

			migrationBuilder.RenameColumn(name: "Symbol", table: "TradeCodes", newName: "ExchangeId");

			migrationBuilder.RenameIndex(
				name: "IX_Instruments_Symbol",
				table: "TradeCodes",
				newName: "IX_TradeCodes_ExchangeId"
			);

			migrationBuilder.AddPrimaryKey(
				name: "PK_UserStrategyTradeCodes",
				table: "UserStrategyTradeCodes",
				column: "Id"
			);

			migrationBuilder.AddPrimaryKey(name: "PK_UserTradeStrategies", table: "UserTradeStrategies", column: "Id");

			migrationBuilder.AddPrimaryKey(name: "PK_UserTradeCode", table: "UserTradeCode", column: "Id");

			migrationBuilder.AddPrimaryKey(name: "PK_TradeStrategies", table: "TradeStrategies", column: "Id");

			migrationBuilder.AddPrimaryKey(name: "PK_TradeReminds", table: "TradeReminds", column: "Id");

			migrationBuilder.AddPrimaryKey(name: "PK_TradeCodes", table: "TradeCodes", column: "Id");

			migrationBuilder.AddCheckConstraint(name: "CK_Trade_PositiveCount", table: "Trades", sql: "`Count` > 0");

			migrationBuilder.AddCheckConstraint(
				name: "CK_Note_ExclusiveTarget",
				table: "Notes",
				sql: "(`TradeCodeId` IS NOT NULL AND `TradeStrategyId` IS NULL) OR (`TradeCodeId` IS NULL AND `TradeStrategyId` IS NOT NULL)"
			);

			migrationBuilder.AddForeignKey(
				name: "FK_Notes_TradeCodes_TradeCodeId",
				table: "Notes",
				column: "TradeCodeId",
				principalTable: "TradeCodes",
				principalColumn: "Id"
			);

			migrationBuilder.AddForeignKey(
				name: "FK_Notes_TradeStrategies_TradeStrategyId",
				table: "Notes",
				column: "TradeStrategyId",
				principalTable: "TradeStrategies",
				principalColumn: "Id"
			);

			migrationBuilder.AddForeignKey(
				name: "FK_TradeReminds_TradeCodes_TradeCodeId",
				table: "TradeReminds",
				column: "TradeCodeId",
				principalTable: "TradeCodes",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade
			);

			migrationBuilder.AddForeignKey(
				name: "FK_TradeReminds_Users_UserId",
				table: "TradeReminds",
				column: "UserId",
				principalTable: "Users",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade
			);

			migrationBuilder.AddForeignKey(
				name: "FK_Trades_TradeCodes_TradeCodeId",
				table: "Trades",
				column: "TradeCodeId",
				principalTable: "TradeCodes",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade
			);

			migrationBuilder.AddForeignKey(
				name: "FK_UserStrategyTradeCodes_TradeCodes_TradeCodeId",
				table: "UserStrategyTradeCodes",
				column: "TradeCodeId",
				principalTable: "TradeCodes",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade
			);

			migrationBuilder.AddForeignKey(
				name: "FK_UserStrategyTradeCodes_TradeStrategies_StrategyId",
				table: "UserStrategyTradeCodes",
				column: "StrategyId",
				principalTable: "TradeStrategies",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade
			);

			migrationBuilder.AddForeignKey(
				name: "FK_UserStrategyTradeCodes_Users_UserId",
				table: "UserStrategyTradeCodes",
				column: "UserId",
				principalTable: "Users",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade
			);

			migrationBuilder.AddForeignKey(
				name: "FK_UserTradeCode_TradeCodes_TradeCodeId",
				table: "UserTradeCode",
				column: "TradeCodeId",
				principalTable: "TradeCodes",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade
			);

			migrationBuilder.AddForeignKey(
				name: "FK_UserTradeCode_Users_UserId",
				table: "UserTradeCode",
				column: "UserId",
				principalTable: "Users",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade
			);

			migrationBuilder.AddForeignKey(
				name: "FK_UserTradeStrategies_TradeStrategies_TradeStrategyId",
				table: "UserTradeStrategies",
				column: "TradeStrategyId",
				principalTable: "TradeStrategies",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade
			);

			migrationBuilder.AddForeignKey(
				name: "FK_UserTradeStrategies_Users_UserId",
				table: "UserTradeStrategies",
				column: "UserId",
				principalTable: "Users",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade
			);
		}
	}
}
