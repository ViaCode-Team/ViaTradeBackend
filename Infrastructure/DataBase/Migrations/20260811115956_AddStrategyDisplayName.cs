using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.DataBase.Migrations
{
	/// <inheritdoc />
	public partial class AddStrategyDisplayName : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder
				.AddColumn<string>(name: "DisplayName", table: "Strategies", type: "longtext", nullable: false)
				.Annotation("MySql:CharSet", "utf8mb4");

			migrationBuilder.UpdateData(
				table: "Instruments",
				keyColumn: "Id",
				keyValue: 1,
				column: "Description",
				value: "Газпром"
			);

			migrationBuilder.UpdateData(
				table: "Instruments",
				keyColumn: "Id",
				keyValue: 2,
				column: "Description",
				value: "Норникель"
			);

			migrationBuilder.UpdateData(
				table: "Strategies",
				keyColumn: "Id",
				keyValue: 1,
				columns: new[]
				{
					"Description",
					"DisplayName",
					"InvestmentHorizon",
					"LimitationsDescription",
					"LogicDescription",
					"SignalFrequency",
					"UsageDescription",
				},
				values: new object[]
				{
					"Базовая стратегия следования тренду для актива. Минимальный риск, редкие сигналы.",
					"Следование тренду",
					"1–3 недели",
					"Стратегия предназначена исключительно для следования тренду",
					"Анализ долгосрочного графика для подтверждения движения",
					"1–2 раза в месяц",
					"Следуйте основному тренду при низкой или средней волатильности",
				}
			);

			migrationBuilder.UpdateData(
				table: "Strategies",
				keyColumn: "Id",
				keyValue: 2,
				columns: new[]
				{
					"Description",
					"DisplayName",
					"InvestmentHorizon",
					"LimitationsDescription",
					"LogicDescription",
					"SignalFrequency",
					"UsageDescription",
				},
				values: new object[]
				{
					"Тестовая стратегия. 100000% прибыли в наносекунду",
					"Тестовая стратегия",
					"до 1 недели",
					"Суперстарт",
					"Очень понятная",
					"3 раза в месяц",
					"Используйте как хотите",
				}
			);

			migrationBuilder.UpdateData(
				table: "TradeTypes",
				keyColumn: "Id",
				keyValue: 1,
				column: "Name",
				value: "Акция"
			);

			migrationBuilder.UpdateData(
				table: "TradeTypes",
				keyColumn: "Id",
				keyValue: 2,
				column: "Name",
				value: "Фьючерс"
			);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(name: "DisplayName", table: "Strategies");

			migrationBuilder.UpdateData(
				table: "Instruments",
				keyColumn: "Id",
				keyValue: 1,
				column: "Description",
				value: "Gazprom"
			);

			migrationBuilder.UpdateData(
				table: "Instruments",
				keyColumn: "Id",
				keyValue: 2,
				column: "Description",
				value: "Nornickel"
			);

			migrationBuilder.UpdateData(
				table: "Strategies",
				keyColumn: "Id",
				keyValue: 1,
				columns: new[]
				{
					"Description",
					"InvestmentHorizon",
					"LimitationsDescription",
					"LogicDescription",
					"SignalFrequency",
					"UsageDescription",
				},
				values: new object[]
				{
					"Basic trend-following strategy for an asset. Minimal risk, rare signals.",
					"1-3 weeks",
					"Strategy exclusively for following the trend",
					"Analysis of a long-term chart to confirm movement",
					"1-2 times a month",
					"Follow the main trend, during low or medium volatility",
				}
			);

			migrationBuilder.UpdateData(
				table: "Strategies",
				keyColumn: "Id",
				keyValue: 2,
				columns: new[]
				{
					"Description",
					"InvestmentHorizon",
					"LimitationsDescription",
					"LogicDescription",
					"SignalFrequency",
					"UsageDescription",
				},
				values: new object[]
				{
					"Test strategy. 100000% profit per nanosecond",
					"up to 1 week",
					"SuperStart",
					"Very clear",
					"3 times a month",
					"Use it however you like",
				}
			);

			migrationBuilder.UpdateData(
				table: "TradeTypes",
				keyColumn: "Id",
				keyValue: 1,
				column: "Name",
				value: "Stock"
			);

			migrationBuilder.UpdateData(
				table: "TradeTypes",
				keyColumn: "Id",
				keyValue: 2,
				column: "Name",
				value: "Futures"
			);
		}
	}
}
