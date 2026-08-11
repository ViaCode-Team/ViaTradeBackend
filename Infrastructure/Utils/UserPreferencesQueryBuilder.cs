using Domain.Entities;
using Infrastructure.DataBase;

namespace Infrastructure.Utils;

public static class UserPreferencesQueryBuilder
{
	public static IQueryable<UserStrategyInstrument> GetUserCodesQuery(this AppDbContext context, int userId)
	{
		return context.UserStrategyInstruments.Where(ustc => ustc.UserId == userId);
	}

	public static IQueryable<UserStrategy> GetAllowedStrategiesQuery(this AppDbContext context, int userId)
	{
		return context.UserStrategies.Where(uts => uts.UserId == userId);
	}

	public static IQueryable<UserStrategyInstrument> FilterByAllowedStrategies(
		this IQueryable<UserStrategyInstrument> codesQuery,
		IQueryable<UserStrategy> strategiesQuery
	)
	{
		return codesQuery.Where(ustc => strategiesQuery.Any(uts => uts.StrategyId == ustc.StrategyId));
	}

	public static IQueryable<StrategyInstrumentProjection> ProjectToStrategyAndInstrument(
		this IQueryable<UserStrategyInstrument> query
	)
	{
		return query.Select(ustc => new StrategyInstrumentProjection
		{
			StrategyId = ustc.StrategyId,
			StrategyName = ustc.Strategy!.Name,
			DisplayName = ustc.Strategy.DisplayName,
			InstrumentId = ustc.InstrumentId,
			Symbol = ustc.Instrument!.Symbol,
			Accuracy = ustc.Strategy.Accuracy,
		});
	}
}

public record StrategyInstrumentProjection
{
	public required int StrategyId { get; init; }
	public required string StrategyName { get; init; }
	public required string DisplayName { get; init; }
	public required int InstrumentId { get; init; }
	public required string Symbol { get; init; }
	public int? Accuracy { get; init; }
}
