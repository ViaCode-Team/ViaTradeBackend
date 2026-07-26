using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.DataBase.Migrations
{
	/// <inheritdoc />
	public partial class AddDataIntegrityConstraints : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder
				.AlterColumn<string>(
					name: "Login",
					table: "Users",
					type: "varchar(255)",
					maxLength: 255,
					nullable: false,
					oldClrType: typeof(string),
					oldType: "longtext"
				)
				.Annotation("MySql:CharSet", "utf8mb4")
				.OldAnnotation("MySql:CharSet", "utf8mb4");

			migrationBuilder.CreateIndex(name: "IX_Users_Login", table: "Users", column: "Login", unique: true);

			migrationBuilder.AddCheckConstraint(name: "CK_Trade_PositiveCount", table: "Trades", sql: "`Count` > 0");

			migrationBuilder.CreateIndex(
				name: "IX_Notes_UserId_TradeCodeId",
				table: "Notes",
				columns: new[] { "UserId", "TradeCodeId" },
				unique: true
			);

			migrationBuilder.CreateIndex(
				name: "IX_Notes_UserId_TradeStrategyId",
				table: "Notes",
				columns: new[] { "UserId", "TradeStrategyId" },
				unique: true
			);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropIndex(name: "IX_Users_Login", table: "Users");

			migrationBuilder.DropCheckConstraint(name: "CK_Trade_PositiveCount", table: "Trades");

			migrationBuilder.DropIndex(name: "IX_Notes_UserId_TradeCodeId", table: "Notes");

			migrationBuilder.DropIndex(name: "IX_Notes_UserId_TradeStrategyId", table: "Notes");

			migrationBuilder
				.AlterColumn<string>(
					name: "Login",
					table: "Users",
					type: "longtext",
					nullable: false,
					oldClrType: typeof(string),
					oldType: "varchar(255)",
					oldMaxLength: 255
				)
				.Annotation("MySql:CharSet", "utf8mb4")
				.OldAnnotation("MySql:CharSet", "utf8mb4");
		}
	}
}
