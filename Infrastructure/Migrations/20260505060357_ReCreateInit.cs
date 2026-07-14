using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class ReCreateInit : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AlterDatabase()
			.Annotation("MySql:CharSet", "utf8mb4");

		migrationBuilder.CreateTable(
			name: "NoteTypes",
			columns: table => new
			{
				Id = table.Column<int>(type: "int", nullable: false)
					.Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
				TypeName = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
					.Annotation("MySql:CharSet", "utf8mb4")
			},
			constraints: table =>
			{
				table.PrimaryKey("PK_NoteTypes", x => x.Id);
			})
			.Annotation("MySql:CharSet", "utf8mb4");

		migrationBuilder.CreateTable(
			name: "TradeCodes",
			columns: table => new
			{
				Id = table.Column<int>(type: "int", nullable: false)
					.Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
				ExchangeId = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
					.Annotation("MySql:CharSet", "utf8mb4"),
				Description = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: true)
					.Annotation("MySql:CharSet", "utf8mb4")
			},
			constraints: table =>
			{
				table.PrimaryKey("PK_TradeCodes", x => x.Id);
			})
			.Annotation("MySql:CharSet", "utf8mb4");

		migrationBuilder.CreateTable(
			name: "TradeStrategies",
			columns: table => new
			{
				Id = table.Column<int>(type: "int", nullable: false)
					.Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
				Name = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
					.Annotation("MySql:CharSet", "utf8mb4"),
				Description = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
					.Annotation("MySql:CharSet", "utf8mb4"),
				Accuracy = table.Column<int>(type: "int", nullable: true),
				SignalFrequency = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
					.Annotation("MySql:CharSet", "utf8mb4"),
				InvestmentHorizon = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
					.Annotation("MySql:CharSet", "utf8mb4"),
				LogicDesc = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
					.Annotation("MySql:CharSet", "utf8mb4"),
				UseDesc = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
					.Annotation("MySql:CharSet", "utf8mb4"),
				LimitDesc = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
					.Annotation("MySql:CharSet", "utf8mb4")
			},
			constraints: table =>
			{
				table.PrimaryKey("PK_TradeStrategies", x => x.Id);
			})
			.Annotation("MySql:CharSet", "utf8mb4");

		migrationBuilder.CreateTable(
			name: "TradeTypes",
			columns: table => new
			{
				Id = table.Column<int>(type: "int", nullable: false)
					.Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
				Name = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
					.Annotation("MySql:CharSet", "utf8mb4")
			},
			constraints: table =>
			{
				table.PrimaryKey("PK_TradeTypes", x => x.Id);
			})
			.Annotation("MySql:CharSet", "utf8mb4");

		migrationBuilder.CreateTable(
			name: "Users",
			columns: table => new
			{
				Id = table.Column<int>(type: "int", nullable: false)
					.Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
				Login = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
					.Annotation("MySql:CharSet", "utf8mb4"),
				HashPassword = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: false)
					.Annotation("MySql:CharSet", "utf8mb4"),
				LastLoginDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
				TgId = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: true)
					.Annotation("MySql:CharSet", "utf8mb4")
			},
			constraints: table =>
			{
				table.PrimaryKey("PK_Users", x => x.Id);
			})
			.Annotation("MySql:CharSet", "utf8mb4");

		migrationBuilder.CreateTable(
			name: "Notes",
			columns: table => new
			{
				Id = table.Column<int>(type: "int", nullable: false)
					.Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
				UserId = table.Column<int>(type: "int", nullable: false),
				NoteText = table.Column<string>(type: "varchar(1024)", maxLength: 1024, nullable: false)
					.Annotation("MySql:CharSet", "utf8mb4"),
				TypeId = table.Column<int>(type: "int", nullable: false),
				TradeCodeId = table.Column<int>(type: "int", nullable: true),
				TradeStrategyId = table.Column<int>(type: "int", nullable: true)
			},
			constraints: table =>
			{
				table.PrimaryKey("PK_Notes", x => x.Id);
				table.CheckConstraint("CK_Note_ExclusiveTarget", "(`TradeCodeId` IS NOT NULL AND `TradeStrategyId` IS NULL) OR (`TradeCodeId` IS NULL AND `TradeStrategyId` IS NOT NULL)");
				table.ForeignKey(
					name: "FK_Notes_NoteTypes_TypeId",
					column: x => x.TypeId,
					principalTable: "NoteTypes",
					principalColumn: "Id",
					onDelete: ReferentialAction.Restrict);
				table.ForeignKey(
					name: "FK_Notes_TradeCodes_TradeCodeId",
					column: x => x.TradeCodeId,
					principalTable: "TradeCodes",
					principalColumn: "Id");
				table.ForeignKey(
					name: "FK_Notes_TradeStrategies_TradeStrategyId",
					column: x => x.TradeStrategyId,
					principalTable: "TradeStrategies",
					principalColumn: "Id");
				table.ForeignKey(
					name: "FK_Notes_Users_UserId",
					column: x => x.UserId,
					principalTable: "Users",
					principalColumn: "Id",
					onDelete: ReferentialAction.Cascade);
			})
			.Annotation("MySql:CharSet", "utf8mb4");

		migrationBuilder.CreateTable(
			name: "Trades",
			columns: table => new
			{
				Id = table.Column<int>(type: "int", nullable: false)
					.Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
				DateOpen = table.Column<DateTime>(type: "datetime(6)", nullable: false),
				DateClose = table.Column<DateTime>(type: "datetime(6)", nullable: true),
				TradeOpen = table.Column<double>(type: "double", nullable: false),
				TradeClose = table.Column<double>(type: "double", nullable: true),
				NetIncome = table.Column<double>(type: "double", nullable: true),
				Count = table.Column<int>(type: "int", nullable: false),
				Price = table.Column<int>(type: "int", nullable: false),
				TradeTypeId = table.Column<int>(type: "int", nullable: false),
				TradeCodeId = table.Column<int>(type: "int", nullable: false),
				UserId = table.Column<int>(type: "int", nullable: false)
			},
			constraints: table =>
			{
				table.PrimaryKey("PK_Trades", x => x.Id);
				table.ForeignKey(
					name: "FK_Trades_TradeCodes_TradeCodeId",
					column: x => x.TradeCodeId,
					principalTable: "TradeCodes",
					principalColumn: "Id",
					onDelete: ReferentialAction.Cascade);
				table.ForeignKey(
					name: "FK_Trades_TradeTypes_TradeTypeId",
					column: x => x.TradeTypeId,
					principalTable: "TradeTypes",
					principalColumn: "Id",
					onDelete: ReferentialAction.Cascade);
				table.ForeignKey(
					name: "FK_Trades_Users_UserId",
					column: x => x.UserId,
					principalTable: "Users",
					principalColumn: "Id",
					onDelete: ReferentialAction.Cascade);
			})
			.Annotation("MySql:CharSet", "utf8mb4");

		migrationBuilder.CreateTable(
			name: "UserStrategyNote",
			columns: table => new
			{
				Id = table.Column<int>(type: "int", nullable: false)
					.Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
				UserId = table.Column<int>(type: "int", nullable: false),
				StratageId = table.Column<int>(type: "int", nullable: false),
				NoteText = table.Column<string>(type: "longtext", nullable: true)
					.Annotation("MySql:CharSet", "utf8mb4"),
				TradeId = table.Column<int>(type: "int", nullable: false)
			},
			constraints: table =>
			{
				table.PrimaryKey("PK_UserStrategyNote", x => x.Id);
				table.ForeignKey(
					name: "FK_UserStrategyNote_TradeStrategies_TradeId",
					column: x => x.TradeId,
					principalTable: "TradeStrategies",
					principalColumn: "Id",
					onDelete: ReferentialAction.Cascade);
				table.ForeignKey(
					name: "FK_UserStrategyNote_Users_UserId",
					column: x => x.UserId,
					principalTable: "Users",
					principalColumn: "Id",
					onDelete: ReferentialAction.Cascade);
			})
			.Annotation("MySql:CharSet", "utf8mb4");

		migrationBuilder.CreateTable(
			name: "UserStrategyTradeCodes",
			columns: table => new
			{
				Id = table.Column<int>(type: "int", nullable: false)
					.Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
				UserId = table.Column<int>(type: "int", nullable: false),
				TradeCodeId = table.Column<int>(type: "int", nullable: false),
				StrategyId = table.Column<int>(type: "int", nullable: false),
				TradeStrategyId = table.Column<int>(type: "int", nullable: true)
			},
			constraints: table =>
			{
				table.PrimaryKey("PK_UserStrategyTradeCodes", x => x.Id);
				table.ForeignKey(
					name: "FK_UserStrategyTradeCodes_TradeCodes_TradeCodeId",
					column: x => x.TradeCodeId,
					principalTable: "TradeCodes",
					principalColumn: "Id",
					onDelete: ReferentialAction.Cascade);
				table.ForeignKey(
					name: "FK_UserStrategyTradeCodes_TradeStrategies_TradeStrategyId",
					column: x => x.TradeStrategyId,
					principalTable: "TradeStrategies",
					principalColumn: "Id");
				table.ForeignKey(
					name: "FK_UserStrategyTradeCodes_Users_UserId",
					column: x => x.UserId,
					principalTable: "Users",
					principalColumn: "Id",
					onDelete: ReferentialAction.Cascade);
			})
			.Annotation("MySql:CharSet", "utf8mb4");

		migrationBuilder.CreateTable(
			name: "UserTradeCode",
			columns: table => new
			{
				Id = table.Column<int>(type: "int", nullable: false)
					.Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
				UserId = table.Column<int>(type: "int", nullable: false),
				TradeCodeId = table.Column<int>(type: "int", nullable: false)
			},
			constraints: table =>
			{
				table.PrimaryKey("PK_UserTradeCode", x => x.Id);
				table.ForeignKey(
					name: "FK_UserTradeCode_TradeCodes_TradeCodeId",
					column: x => x.TradeCodeId,
					principalTable: "TradeCodes",
					principalColumn: "Id",
					onDelete: ReferentialAction.Cascade);
				table.ForeignKey(
					name: "FK_UserTradeCode_Users_UserId",
					column: x => x.UserId,
					principalTable: "Users",
					principalColumn: "Id",
					onDelete: ReferentialAction.Cascade);
			})
			.Annotation("MySql:CharSet", "utf8mb4");

		migrationBuilder.CreateTable(
			name: "UserTradeNote",
			columns: table => new
			{
				Id = table.Column<int>(type: "int", nullable: false)
					.Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
				UserId = table.Column<int>(type: "int", nullable: false),
				TradeCodeId = table.Column<int>(type: "int", nullable: false),
				NoteText = table.Column<string>(type: "longtext", nullable: true)
					.Annotation("MySql:CharSet", "utf8mb4")
			},
			constraints: table =>
			{
				table.PrimaryKey("PK_UserTradeNote", x => x.Id);
				table.ForeignKey(
					name: "FK_UserTradeNote_TradeCodes_TradeCodeId",
					column: x => x.TradeCodeId,
					principalTable: "TradeCodes",
					principalColumn: "Id",
					onDelete: ReferentialAction.Cascade);
				table.ForeignKey(
					name: "FK_UserTradeNote_Users_UserId",
					column: x => x.UserId,
					principalTable: "Users",
					principalColumn: "Id",
					onDelete: ReferentialAction.Cascade);
			})
			.Annotation("MySql:CharSet", "utf8mb4");

		migrationBuilder.CreateTable(
			name: "UserTradeStrategies",
			columns: table => new
			{
				Id = table.Column<int>(type: "int", nullable: false)
					.Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
				UserId = table.Column<int>(type: "int", nullable: false),
				TradeStrategyId = table.Column<int>(type: "int", nullable: false)
			},
			constraints: table =>
			{
				table.PrimaryKey("PK_UserTradeStrategies", x => x.Id);
				table.ForeignKey(
					name: "FK_UserTradeStrategies_TradeStrategies_TradeStrategyId",
					column: x => x.TradeStrategyId,
					principalTable: "TradeStrategies",
					principalColumn: "Id",
					onDelete: ReferentialAction.Cascade);
				table.ForeignKey(
					name: "FK_UserTradeStrategies_Users_UserId",
					column: x => x.UserId,
					principalTable: "Users",
					principalColumn: "Id",
					onDelete: ReferentialAction.Cascade);
			})
			.Annotation("MySql:CharSet", "utf8mb4");

		migrationBuilder.InsertData(
			table: "TradeCodes",
			columns: new[] { "Id", "Description", "ExchangeId" },
			values: new object[,]
			{
				{ 1, "Газпром", "GAZP" },
				{ 2, "Норникель", "GMKN" }
			});

		migrationBuilder.InsertData(
			table: "TradeStrategies",
			columns: new[] { "Id", "Accuracy", "Description", "InvestmentHorizon", "LimitDesc", "LogicDesc", "Name", "SignalFrequency", "UseDesc" },
			values: new object[,]
			{
				{ 1, 81, "Базовая стратегия следования биржевому тренду инструмента. Минамальный риск, редкие сигналы.", "1-3 недели", "Стратегия исключительно для слелования тренду", "Анализ длительного времение гшрафика для подтвержеденгия движдениея", "TrendFollowingStrategy", "1-2 раза в месяц", "Следовать основному тренду, при низкой или средней валотильности" },
				{ 2, 99, "Тестовая стратегия. 100000% прибыли в наносекунду", "до 1 недели", "СуперСтарта", "Ващё чётко", "Test", "3 раза в месяц", "Как по кайфу так и используй" }
			});

		migrationBuilder.CreateIndex(
			name: "IX_Notes_TradeCodeId",
			table: "Notes",
			column: "TradeCodeId");

		migrationBuilder.CreateIndex(
			name: "IX_Notes_TradeStrategyId",
			table: "Notes",
			column: "TradeStrategyId");

		migrationBuilder.CreateIndex(
			name: "IX_Notes_TypeId",
			table: "Notes",
			column: "TypeId");

		migrationBuilder.CreateIndex(
			name: "IX_Notes_UserId",
			table: "Notes",
			column: "UserId");

		migrationBuilder.CreateIndex(
			name: "IX_TradeCodes_ExchangeId",
			table: "TradeCodes",
			column: "ExchangeId",
			unique: true);

		migrationBuilder.CreateIndex(
			name: "IX_Trades_TradeCodeId",
			table: "Trades",
			column: "TradeCodeId");

		migrationBuilder.CreateIndex(
			name: "IX_Trades_TradeTypeId",
			table: "Trades",
			column: "TradeTypeId");

		migrationBuilder.CreateIndex(
			name: "IX_Trades_UserId",
			table: "Trades",
			column: "UserId");

		migrationBuilder.CreateIndex(
			name: "IX_TradeStrategies_Name",
			table: "TradeStrategies",
			column: "Name",
			unique: true);

		migrationBuilder.CreateIndex(
			name: "IX_TradeTypes_Name",
			table: "TradeTypes",
			column: "Name",
			unique: true);

		migrationBuilder.CreateIndex(
			name: "IX_UserStrategyNote_TradeId",
			table: "UserStrategyNote",
			column: "TradeId");

		migrationBuilder.CreateIndex(
			name: "IX_UserStrategyNote_UserId_StratageId",
			table: "UserStrategyNote",
			columns: new[] { "UserId", "StratageId" },
			unique: true);

		migrationBuilder.CreateIndex(
			name: "IX_UserStrategyTradeCodes_TradeCodeId",
			table: "UserStrategyTradeCodes",
			column: "TradeCodeId");

		migrationBuilder.CreateIndex(
			name: "IX_UserStrategyTradeCodes_TradeStrategyId",
			table: "UserStrategyTradeCodes",
			column: "TradeStrategyId");

		migrationBuilder.CreateIndex(
			name: "IX_UserStrategyTradeCodes_UserId_TradeCodeId_StrategyId",
			table: "UserStrategyTradeCodes",
			columns: new[] { "UserId", "TradeCodeId", "StrategyId" },
			unique: true);

		migrationBuilder.CreateIndex(
			name: "IX_UserTradeCode_TradeCodeId",
			table: "UserTradeCode",
			column: "TradeCodeId");

		migrationBuilder.CreateIndex(
			name: "IX_UserTradeCode_UserId_TradeCodeId",
			table: "UserTradeCode",
			columns: new[] { "UserId", "TradeCodeId" },
			unique: true);

		migrationBuilder.CreateIndex(
			name: "IX_UserTradeNote_TradeCodeId",
			table: "UserTradeNote",
			column: "TradeCodeId");

		migrationBuilder.CreateIndex(
			name: "IX_UserTradeNote_UserId_TradeCodeId",
			table: "UserTradeNote",
			columns: new[] { "UserId", "TradeCodeId" },
			unique: true);

		migrationBuilder.CreateIndex(
			name: "IX_UserTradeStrategies_TradeStrategyId",
			table: "UserTradeStrategies",
			column: "TradeStrategyId");

		migrationBuilder.CreateIndex(
			name: "IX_UserTradeStrategies_UserId_TradeStrategyId",
			table: "UserTradeStrategies",
			columns: new[] { "UserId", "TradeStrategyId" },
			unique: true);
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropTable(
			name: "Notes");

		migrationBuilder.DropTable(
			name: "Trades");

		migrationBuilder.DropTable(
			name: "UserStrategyNote");

		migrationBuilder.DropTable(
			name: "UserStrategyTradeCodes");

		migrationBuilder.DropTable(
			name: "UserTradeCode");

		migrationBuilder.DropTable(
			name: "UserTradeNote");

		migrationBuilder.DropTable(
			name: "UserTradeStrategies");

		migrationBuilder.DropTable(
			name: "NoteTypes");

		migrationBuilder.DropTable(
			name: "TradeTypes");

		migrationBuilder.DropTable(
			name: "TradeCodes");

		migrationBuilder.DropTable(
			name: "TradeStrategies");

		migrationBuilder.DropTable(
			name: "Users");
	}
}
