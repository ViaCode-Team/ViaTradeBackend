using Application.Common.Models;
using Application.Strategies.Interfaces;
using Application.Strategies.Models;
using Application.TradeCodes.Models;
using Domain.Strategies.Entities;
using Infrastructure.Extensions;

namespace Infrastructure.DataBase.Repositories;

public class UserStrategyTradeCodeEfRepository(AppDbContext context)
	: GenericEfRepository<UserStrategyTradeCode>(context),
		IUserStrategyTradeCodeRepository
{
	public async Task<PageResult<TradeStrategy>> GetStrategiesPageByInstrumentAsync(
		int userId,
		int instrumentId,
		StrategyFilter strategyFilter,
		StrategySort strategySort,
		PageOptions pageOptions,
		CancellationToken ct
	)
	{
		var query = _dbSet
			.Where(link => link.UserId == userId && link.TradeCodeId == instrumentId)
			.Select(link => link.TradeStrategy!);

		if (!string.IsNullOrWhiteSpace(strategyFilter.Name))
			query = query.Where(strategy => strategy.Name == strategyFilter.Name);

		query = ApplyStrategySort(query, strategySort);
		var pagedStrategies = await query.ToPagedAsync(pageOptions, ct);

		return pagedStrategies.Map(strategy =>
		{
			strategy.IsActive = true;
			return strategy;
		});
	}

	public async Task<PageResult<RelatedInstrumentDto>> GetInstrumentsPageByStrategyAsync(
		int userId,
		int strategyId,
		InstrumentSort instrumentSort,
		PageOptions pageOptions,
		CancellationToken ct
	)
	{
		var query = _dbSet
			.Where(link => link.UserId == userId && link.StrategyId == strategyId)
			.Select(link => link.TradeCode!);

		query = ApplyInstrumentSort(query, instrumentSort);

		return await query
			.Select(tradeCode => new RelatedInstrumentDto(tradeCode.Id, tradeCode.ExchangeId, tradeCode.Description))
			.ToPagedAsync(pageOptions, ct);
	}

	private static IQueryable<Domain.TradeCodes.Entities.TradeCode> ApplyInstrumentSort(
		IQueryable<Domain.TradeCodes.Entities.TradeCode> query,
		InstrumentSort instrumentSort
	)
	{
		IOrderedQueryable<Domain.TradeCodes.Entities.TradeCode>? orderedQuery = null;
		foreach (var field in instrumentSort.GetEffectiveSortBy())
		{
			if (orderedQuery == null)
			{
				orderedQuery = field switch
				{
					InstrumentSortField.SymbolDesc => query.OrderByDescending(tradeCode => tradeCode.ExchangeId),
					_ => query.OrderBy(tradeCode => tradeCode.ExchangeId),
				};
			}
			else
			{
				orderedQuery = field switch
				{
					InstrumentSortField.SymbolDesc => orderedQuery.ThenByDescending(tradeCode => tradeCode.ExchangeId),
					_ => orderedQuery.ThenBy(tradeCode => tradeCode.ExchangeId),
				};
			}
		}

		if (orderedQuery == null)
			return query.OrderBy(tradeCode => tradeCode.Id);

		return orderedQuery.ThenBy(tradeCode => tradeCode.Id);
	}

	private static IQueryable<TradeStrategy> ApplyStrategySort(
		IQueryable<TradeStrategy> query,
		StrategySort strategySort
	)
	{
		IOrderedQueryable<TradeStrategy>? orderedQuery = null;
		foreach (var field in strategySort.GetEffectiveSortBy())
		{
			if (orderedQuery == null)
			{
				orderedQuery = field switch
				{
					StrategySortField.NameDesc => query.OrderByDescending(strategy => strategy.Name),
					StrategySortField.AccuracyAsc => query.OrderBy(strategy => strategy.Accuracy ?? 0),
					StrategySortField.AccuracyDesc => query.OrderByDescending(strategy => strategy.Accuracy ?? 0),
					_ => query.OrderBy(strategy => strategy.Name),
				};
			}
			else
			{
				orderedQuery = field switch
				{
					StrategySortField.NameDesc => orderedQuery.ThenByDescending(strategy => strategy.Name),
					StrategySortField.AccuracyAsc => orderedQuery.ThenBy(strategy => strategy.Accuracy ?? 0),
					StrategySortField.AccuracyDesc => orderedQuery.ThenByDescending(strategy => strategy.Accuracy ?? 0),
					_ => orderedQuery.ThenBy(strategy => strategy.Name),
				};
			}
		}

		if (orderedQuery == null)
			return query.OrderBy(strategy => strategy.Id);

		return orderedQuery.ThenBy(strategy => strategy.Id);
	}
}
