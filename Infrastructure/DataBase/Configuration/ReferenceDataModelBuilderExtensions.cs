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
					Description = "Базовая стратегия следования тренду для актива. Минимальный риск, редкие сигналы.",
					Accuracy = 81,
					SignalFrequency = "1–2 раза в месяц",
					InvestmentHorizon = "1–3 недели",
					LogicDescription = "Анализ долгосрочного графика для подтверждения движения",
					UsageDescription = "Следуйте основному тренду при низкой или средней волатильности",
					LimitationsDescription = "Стратегия предназначена исключительно для следования тренду",
					CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
					IsActive = true,
				},
				new
				{
					Id = 2,
					Name = "Test",
					Description = "Тестовая стратегия. 100000% прибыли в наносекунду",
					Accuracy = 99,
					SignalFrequency = "3 раза в месяц",
					InvestmentHorizon = "до 1 недели",
					LogicDescription = "Очень понятная",
					UsageDescription = "Используйте как хотите",
					LimitationsDescription = "Суперстарт",
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
					Description = "Газпром",
					CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
				},
				new
				{
					Id = 2,
					Symbol = "GMKN",
					Description = "Норникель",
					CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
				}
			);

		modelBuilder
			.Entity<TradeType>()
			.HasData(
				new
				{
					Id = 1,
					Name = "Акция",
					CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
				},
				new
				{
					Id = 2,
					Name = "Фьючерс",
					CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
				}
			);
	}
}
