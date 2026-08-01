using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class Remind : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder
			.CreateTable(
				name: "TradeReminds",
				columns: table => new
				{
					Id = table
						.Column<int>(type: "int", nullable: false)
						.Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
					TextRemind = table
						.Column<string>(type: "varchar(1024)", maxLength: 1024, nullable: false)
						.Annotation("MySql:CharSet", "utf8mb4"),
					DateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
					TradeCodeId = table.Column<int>(type: "int", nullable: false),
					UserId = table.Column<int>(type: "int", nullable: false),
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_TradeReminds", x => x.Id);
					table.ForeignKey(
						name: "FK_TradeReminds_TradeCodes_TradeCodeId",
						column: x => x.TradeCodeId,
						principalTable: "TradeCodes",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade
					);
					table.ForeignKey(
						name: "FK_TradeReminds_Users_UserId",
						column: x => x.UserId,
						principalTable: "Users",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade
					);
				}
			)
			.Annotation("MySql:CharSet", "utf8mb4");

		migrationBuilder.CreateIndex(name: "IX_TradeReminds_Id", table: "TradeReminds", column: "Id", unique: true);

		migrationBuilder.CreateIndex(name: "IX_TradeReminds_TradeCodeId", table: "TradeReminds", column: "TradeCodeId");

		migrationBuilder.CreateIndex(name: "IX_TradeReminds_UserId", table: "TradeReminds", column: "UserId");
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropTable(name: "TradeReminds");
	}
}
