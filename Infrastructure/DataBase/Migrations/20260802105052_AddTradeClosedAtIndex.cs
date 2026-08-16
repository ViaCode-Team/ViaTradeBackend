using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ViaTrade.Infrastructure.DataBase.Migrations
{
	/// <inheritdoc />
	public partial class AddTradeClosedAtIndex : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateIndex(
				name: "IX_Trades_UserId_ClosedAt",
				table: "Trades",
				columns: new[] { "UserId", "ClosedAt" }
			);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropIndex(name: "IX_Trades_UserId_ClosedAt", table: "Trades");
		}
	}
}
