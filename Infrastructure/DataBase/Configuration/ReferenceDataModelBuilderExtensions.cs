using Domain.Entities;
using Domain.Strategies.Entities;
using Domain.TradeCodes.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataBase.Configuration;

internal static class ReferenceDataModelBuilderExtensions
{
	public static void SeedReferenceData(this ModelBuilder modelBuilder)
	{
		modelBuilder
			.Entity<TradeStrategy>()
			.HasData(
				new
				{
					Id = 1,
					Name = "TrendFollowingStrategy",
					Description = "Basic trend-following strategy for an asset. Minimal risk, rare signals.",
					Accuracy = 81,
					SignalFrequency = "1-2 times a month",
					InvestmentHorizon = "1-3 weeks",
					LogicDesc = "Analysis of a long-term chart to confirm movement",
					UseDesc = "Follow the main trend, during low or medium volatility",
					LimitDesc = "Strategy exclusively for following the trend",
					CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
					IsActive = true,
				},
				new
				{
					Id = 2,
					Name = "Test",
					Description = "Test strategy. 100000% profit per nanosecond",
					Accuracy = 99,
					SignalFrequency = "3 times a month",
					InvestmentHorizon = "up to 1 week",
					LogicDesc = "Very clear",
					UseDesc = "Use it however you like",
					LimitDesc = "SuperStart",
					CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
					IsActive = true,
				}
			);

		modelBuilder
			.Entity<TradeCode>()
			.HasData(
				new
				{
					Id = 1,
					ExchangeId = "GAZP",
					Description = "Gazprom",
					CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
				},
				new
				{
					Id = 2,
					ExchangeId = "GMKN",
					Description = "Nornickel",
					CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
				}
			);

		modelBuilder
			.Entity<TradeType>()
			.HasData(
				new
				{
					Id = 1,
					Name = "Stock",
					CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
				},
				new
				{
					Id = 2,
					Name = "Futures",
					CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
				}
			);
	}
}
