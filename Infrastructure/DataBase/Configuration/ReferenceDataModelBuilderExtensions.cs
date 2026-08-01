using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataBase.Configuration;

internal static class ReferenceDataModelBuilderExtensions
{
	public static void SeedReferenceData(this ModelBuilder modelBuilder)
	{
		modelBuilder
			.Entity<Strategy>()
			.HasData(
				new
				{
					Id = 1,
					Name = "TrendFollowingStrategy",
					Description = "Basic trend-following strategy for an asset. Minimal risk, rare signals.",
					Accuracy = 81,
					SignalFrequency = "1-2 times a month",
					InvestmentHorizon = "1-3 weeks",
					LogicDescription = "Analysis of a long-term chart to confirm movement",
					UsageDescription = "Follow the main trend, during low or medium volatility",
					LimitationsDescription = "Strategy exclusively for following the trend",
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
					LogicDescription = "Very clear",
					UsageDescription = "Use it however you like",
					LimitationsDescription = "SuperStart",
					CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
					IsActive = true,
				}
			);

		modelBuilder
			.Entity<Instrument>()
			.HasData(
				new
				{
					Id = 1,
					Symbol = "GAZP",
					Description = "Gazprom",
					CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
				},
				new
				{
					Id = 2,
					Symbol = "GMKN",
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
