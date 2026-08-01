using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class PriceFix : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AlterColumn<decimal>(
			name: "Price",
			table: "Trades",
			type: "decimal(18,2)",
			nullable: false,
			defaultValue: 0m,
			oldClrType: typeof(int),
			oldType: "int"
		);
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropColumn(name: "Price", table: "Trades");
	}
}
