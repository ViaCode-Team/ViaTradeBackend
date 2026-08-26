using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ViaTrade.Infrastructure.DataBase.Migrations
{
	/// <inheritdoc />
	public partial class RemoveRedundantStrategyInstrumentIndex : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropIndex(name: "IX_UserStrategyInstruments_StrategyId", table: "UserStrategyInstruments");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateIndex(
				name: "IX_UserStrategyInstruments_StrategyId",
				table: "UserStrategyInstruments",
				column: "StrategyId"
			);
		}
	}
}
