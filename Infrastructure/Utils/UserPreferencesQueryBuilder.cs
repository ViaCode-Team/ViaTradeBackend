using Domain.Strategies.Entities;
using Infrastructure.DataBase;

namespace Infrastructure.Utils;

public static class UserPreferencesQueryBuilder
{
	public static IQueryable<UserStrategyTradeCode> GetUserCodesQuery(this AppDbContext context, int userId)
	{
		return context.UserStrategyTradeCodes.Where(ustc => ustc.UserId == userId);
	}

	public static IQueryable<UserTradeStrategy> GetAllowedStrategiesQuery(this AppDbContext context, int userId)
	{
		return context.UserTradeStrategies.Where(uts => uts.UserId == userId);
	}

	public static IQueryable<UserStrategyTradeCode> FilterByAllowedStrategies(
		this IQueryable<UserStrategyTradeCode> codesQuery,
		IQueryable<UserTradeStrategy> strategiesQuery
	)
	{
		return codesQuery.Where(ustc => strategiesQuery.Any(uts => uts.TradeStrategyId == ustc.StrategyId));
	}

	public static IQueryable<StrategyInstrumentProjection> ProjectToStrategyAndInstrument(
		this IQueryable<UserStrategyTradeCode> query
	)
	{
		return query.Select(ustc => new StrategyInstrumentProjection
		{
			StrategyId = ustc.StrategyId,
			StrategyName = ustc.TradeStrategy!.Name,
			InstrumentId = ustc.TradeCodeId,
			Symbol = ustc.TradeCode!.ExchangeId,
			Accuracy = ustc.TradeStrategy.Accuracy,
		});
	}
}

public record StrategyInstrumentProjection
{
	public required int StrategyId { get; init; }
	public required string StrategyName { get; init; }
	public required int InstrumentId { get; init; }
	public required string Symbol { get; init; }
	public int? Accuracy { get; init; }
}
