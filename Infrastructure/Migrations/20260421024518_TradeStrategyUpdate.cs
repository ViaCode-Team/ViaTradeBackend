using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TradeStrategyUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Accuracy",
                table: "TradeStrategies",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvestmentHorizon",
                table: "TradeStrategies",
                type: "varchar(128)",
                maxLength: 128,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "SignalFrequency",
                table: "TradeStrategies",
                type: "varchar(128)",
                maxLength: 128,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Accuracy",
                table: "TradeStrategies");

            migrationBuilder.DropColumn(
                name: "InvestmentHorizon",
                table: "TradeStrategies");

            migrationBuilder.DropColumn(
                name: "SignalFrequency",
                table: "TradeStrategies");
        }
    }
}
