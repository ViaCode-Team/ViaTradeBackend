using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NoteLogic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RefreshToken",
                table: "Users");

            migrationBuilder.CreateTable(
                name: "UserStrategyNotes",
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
                    table.PrimaryKey("PK_UserStrategyNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserStrategyNotes_TradeStrategies_TradeId",
                        column: x => x.TradeId,
                        principalTable: "TradeStrategies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserStrategyNotes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserTradeNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TadeCodeId = table.Column<int>(type: "int", nullable: false),
                    NoteText = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TradeCodeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTradeNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserTradeNotes_TradeCodes_TradeCodeId",
                        column: x => x.TradeCodeId,
                        principalTable: "TradeCodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserTradeNotes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_UserStrategyNotes_TradeId",
                table: "UserStrategyNotes",
                column: "TradeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserStrategyNotes_UserId_StratageId",
                table: "UserStrategyNotes",
                columns: new[] { "UserId", "StratageId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserTradeNotes_TradeCodeId",
                table: "UserTradeNotes",
                column: "TradeCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTradeNotes_UserId_TadeCodeId",
                table: "UserTradeNotes",
                columns: new[] { "UserId", "TadeCodeId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserStrategyNotes");

            migrationBuilder.DropTable(
                name: "UserTradeNotes");

            migrationBuilder.AddColumn<string>(
                name: "RefreshToken",
                table: "Users",
                type: "varchar(512)",
                maxLength: 512,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
