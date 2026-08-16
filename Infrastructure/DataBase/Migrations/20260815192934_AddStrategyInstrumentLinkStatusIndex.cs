using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.DataBase.Migrations
{
    /// <inheritdoc />
    public partial class AddStrategyInstrumentLinkStatusIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserStrategyInstruments_StrategyId",
                table: "UserStrategyInstruments");

            migrationBuilder.CreateIndex(
                name: "IX_UserStrategyInstruments_StrategyId_InstrumentId",
                table: "UserStrategyInstruments",
                columns: new[] { "StrategyId", "InstrumentId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserStrategyInstruments_StrategyId_InstrumentId",
                table: "UserStrategyInstruments");

            migrationBuilder.CreateIndex(
                name: "IX_UserStrategyInstruments_StrategyId",
                table: "UserStrategyInstruments",
                column: "StrategyId");
        }
    }
}
