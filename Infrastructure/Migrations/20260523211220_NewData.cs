using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class NewData : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.InsertData(
			table: "TradeTypes",
			columns: new[] { "Id", "Name" },
			values: new object[,]
			{
				{ 1, "Акция" },
				{ 2, "Фьючерс" }
			});
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DeleteData(
			table: "TradeTypes",
			keyColumn: "Id",
			keyValue: 1);

		migrationBuilder.DeleteData(
			table: "TradeTypes",
			keyColumn: "Id",
			keyValue: 2);
	}
}
