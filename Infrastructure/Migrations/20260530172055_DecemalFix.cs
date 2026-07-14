using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class DecemalFix : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AlterColumn<decimal>(
			name: "Price",
			table: "Trades",
			type: "decimal(18,2)",
			nullable: false,
			oldClrType: typeof(decimal),
			oldType: "decimal(65,30)");
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AlterColumn<decimal>(
			name: "Price",
			table: "Trades",
			type: "decimal(65,30)",
			nullable: false,
			oldClrType: typeof(decimal),
			oldType: "decimal(18,2)");
	}
}
