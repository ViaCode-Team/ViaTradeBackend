using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class NewFix : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropTable(name: "UserStrategyNote");

		migrationBuilder.DropTable(name: "UserTradeNote");
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder
			.CreateTable(
				name: "UserStrategyNote",
				columns: table => new
				{
					Id = table
						.Column<int>(type: "int", nullable: false)
						.Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
					TradeId = table.Column<int>(type: "int", nullable: false),
					UserId = table.Column<int>(type: "int", nullable: false),
					NoteText = table
						.Column<string>(type: "longtext", nullable: true)
						.Annotation("MySql:CharSet", "utf8mb4"),
					StratageId = table.Column<int>(type: "int", nullable: false),
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_UserStrategyNote", x => x.Id);
					table.ForeignKey(
						name: "FK_UserStrategyNote_TradeStrategies_TradeId",
						column: x => x.TradeId,
						principalTable: "TradeStrategies",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade
					);
					table.ForeignKey(
						name: "FK_UserStrategyNote_Users_UserId",
						column: x => x.UserId,
						principalTable: "Users",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade
					);
				}
			)
			.Annotation("MySql:CharSet", "utf8mb4");

		migrationBuilder
			.CreateTable(
				name: "UserTradeNote",
				columns: table => new
				{
					Id = table
						.Column<int>(type: "int", nullable: false)
						.Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
					TradeCodeId = table.Column<int>(type: "int", nullable: false),
					UserId = table.Column<int>(type: "int", nullable: false),
					NoteText = table
						.Column<string>(type: "longtext", nullable: true)
						.Annotation("MySql:CharSet", "utf8mb4"),
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_UserTradeNote", x => x.Id);
					table.ForeignKey(
						name: "FK_UserTradeNote_TradeCodes_TradeCodeId",
						column: x => x.TradeCodeId,
						principalTable: "TradeCodes",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade
					);
					table.ForeignKey(
						name: "FK_UserTradeNote_Users_UserId",
						column: x => x.UserId,
						principalTable: "Users",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade
					);
				}
			)
			.Annotation("MySql:CharSet", "utf8mb4");

		migrationBuilder.CreateIndex(name: "IX_UserStrategyNote_TradeId", table: "UserStrategyNote", column: "TradeId");

		migrationBuilder.CreateIndex(
			name: "IX_UserStrategyNote_UserId_StratageId",
			table: "UserStrategyNote",
			columns: new[] { "UserId", "StratageId" },
			unique: true
		);

		migrationBuilder.CreateIndex(
			name: "IX_UserTradeNote_TradeCodeId",
			table: "UserTradeNote",
			column: "TradeCodeId"
		);

		migrationBuilder.CreateIndex(
			name: "IX_UserTradeNote_UserId_TradeCodeId",
			table: "UserTradeNote",
			columns: new[] { "UserId", "TradeCodeId" },
			unique: true
		);
	}
}
