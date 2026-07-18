using Domain.Strategies.Entities;
using Domain.Entities.DataBase;
using Infrastructure.Repositories.DataBase;

namespace Infrastructure.Utils;

public static class UserPreferencesQueryBuilder
{
	public static IQueryable<UserStrategyTradeCode> GetUserCodesQuery(this AppDbContext context, int userId)
	{
		return context.UserStrategyTradeCodes
			.Where(ustc => ustc.UserId == userId);
	}

	public static IQueryable<UserTradeStrategy> GetAllowedStrategiesQuery(this AppDbContext context, int userId)
	{
		return context.UserTradeStrategies
			.Where(uts => uts.UserId == userId);
	}

	public static IQueryable<UserStrategyTradeCode> FilterByAllowedStrategies(
		this IQueryable<UserStrategyTradeCode> codesQuery,
		IQueryable<UserTradeStrategy> strategiesQuery)
	{
		return codesQuery
			.Where(ustc => strategiesQuery.Any(uts => uts.TradeStrategyId == ustc.StrategyId));
	}

	public static IQueryable<StrategyTradeCodeProjection> ProjectToStrategyAndTradeCode(this IQueryable<UserStrategyTradeCode> query)
	{
		return query.Select(ustc => new StrategyTradeCodeProjection
		{
			StrategyName = ustc.TradeStrategy!.Name,
			TradeCode = ustc.TradeCode!.ExchangeId
		});
	}

	public static Dictionary<string, List<string>> GroupByStrategyName(this List<StrategyTradeCodeProjection> results)
	{
		return results
			.GroupBy(x => x.StrategyName)
			.ToDictionary(
				g => g.Key,
				g => g.Select(x => x.TradeCode).Distinct().ToList()
			);
	}
}

public record StrategyTradeCodeProjection
{
	public required string StrategyName { get; init; }
	public required string TradeCode { get; init; }
}
