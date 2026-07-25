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
	public async Task<PageResult<UserStrategyTradeCode>> GetPageByUserAsync(
		int userId,
		PageOptions pageOptions,
		CancellationToken ct
	)
	{
		return await GetPageByAsync(strategyCode => strategyCode.UserId == userId, pageOptions, ct);
	}

	public async Task<PageResult<RelatedTradeStrategyDto>> GetStrategiesPageByTradeCodeAsync(
		int userId,
		int tradeCodeId,
		StrategyFilter strategyFilter,
		StrategySort strategySort,
		PageOptions pageOptions,
		CancellationToken ct
	)
	{
		var query = _dbSet
			.Where(link => link.UserId == userId && link.TradeCodeId == tradeCodeId)
			.Select(link => link.TradeStrategy!);

		query = ApplyStrategyFilter(query, userId, strategyFilter);
		query = ApplyStrategySort(query, strategySort);

		return await query
			.Select(strategy => new RelatedTradeStrategyDto(
				strategy.Id,
				strategy.Name,
				strategy.Description,
				strategy.Accuracy,
				strategy.SignalFrequency,
				strategy.InvestmentHorizon,
				strategy.LogicDesc,
				strategy.UseDesc,
				strategy.LimitDesc,
				strategy.UserTradeStrategies.Any(link => link.UserId == userId)
			))
			.ToPagedAsync(pageOptions, ct);
	}

	public async Task<PageResult<RelatedTradeCodeDto>> GetTradeCodesPageByStrategyAsync(
		int userId,
		int strategyId,
		TradeCodeSort tradeCodeSort,
		PageOptions pageOptions,
		CancellationToken ct
	)
	{
		var query = _dbSet
			.Where(link => link.UserId == userId && link.StrategyId == strategyId)
			.Select(link => link.TradeCode!);

		query = ApplyTradeCodeSort(query, tradeCodeSort);

		return await query
			.Select(tradeCode => new RelatedTradeCodeDto(tradeCode.Id, tradeCode.ExchangeId, tradeCode.Description))
			.ToPagedAsync(pageOptions, ct);
	}

	private static IQueryable<TradeStrategy> ApplyStrategyFilter(
		IQueryable<TradeStrategy> query,
		int userId,
		StrategyFilter strategyFilter
	)
	{
		if (strategyFilter.IsActive is not bool isActive)
			return query;

		if (isActive)
			return query.Where(strategy => strategy.UserTradeStrategies.Any(link => link.UserId == userId));

		return query.Where(strategy => !strategy.UserTradeStrategies.Any(link => link.UserId == userId));
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

	private static IQueryable<Domain.TradeCodes.Entities.TradeCode> ApplyTradeCodeSort(
		IQueryable<Domain.TradeCodes.Entities.TradeCode> query,
		TradeCodeSort tradeCodeSort
	)
	{
		IOrderedQueryable<Domain.TradeCodes.Entities.TradeCode>? orderedQuery = null;
		foreach (var field in tradeCodeSort.GetEffectiveSortBy())
		{
			if (orderedQuery == null)
			{
				orderedQuery = field switch
				{
					TradeCodeSortField.NameDesc => query.OrderByDescending(tradeCode => tradeCode.ExchangeId),
					_ => query.OrderBy(tradeCode => tradeCode.ExchangeId),
				};
			}
			else
			{
				orderedQuery = field switch
				{
					TradeCodeSortField.NameDesc => orderedQuery.ThenByDescending(tradeCode => tradeCode.ExchangeId),
					_ => orderedQuery.ThenBy(tradeCode => tradeCode.ExchangeId),
				};
			}
		}

		if (orderedQuery == null)
			return query.OrderBy(tradeCode => tradeCode.Id);

		return orderedQuery.ThenBy(tradeCode => tradeCode.Id);
	}
}
