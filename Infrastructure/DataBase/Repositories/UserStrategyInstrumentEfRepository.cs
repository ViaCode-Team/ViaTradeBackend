using Application.Common.Models;
using Application.Instruments.Models;
using Application.Strategies.Interfaces;
using Application.Strategies.Models;
using Domain.Entities;
using Infrastructure.Extensions;

namespace Infrastructure.DataBase.Repositories;

public class UserStrategyInstrumentEfRepository(AppDbContext context)
	: GenericEfRepository<UserStrategyInstrument>(context),
		IUserStrategyInstrumentRepository
{
	public async Task<PageResult<Strategy>> GetStrategiesPageByInstrumentAsync(
		int userId,
		int instrumentId,
		StrategyFilter strategyFilter,
		StrategySort strategySort,
		PageOptions pageOptions,
		CancellationToken ct
	)
	{
		var query = _dbSet
			.Where(link => link.UserId == userId && link.InstrumentId == instrumentId)
			.Select(link => link.Strategy!);

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
			.Select(link => link.Instrument!);

		query = ApplyInstrumentSort(query, instrumentSort);

		return await query
			.Select(instrument => new RelatedInstrumentDto(instrument.Id, instrument.Symbol, instrument.Description))
			.ToPagedAsync(pageOptions, ct);
	}

	private static IQueryable<Domain.Entities.Instrument> ApplyInstrumentSort(
		IQueryable<Domain.Entities.Instrument> query,
		InstrumentSort instrumentSort
	)
	{
		IOrderedQueryable<Domain.Entities.Instrument>? orderedQuery = null;
		foreach (var field in instrumentSort.GetEffectiveSortBy())
		{
			if (orderedQuery == null)
			{
				orderedQuery = field switch
				{
					InstrumentSortField.SymbolDesc => query.OrderByDescending(instrument => instrument.Symbol),
					_ => query.OrderBy(instrument => instrument.Symbol),
				};
			}
			else
			{
				orderedQuery = field switch
				{
					InstrumentSortField.SymbolDesc => orderedQuery.ThenByDescending(instrument => instrument.Symbol),
					_ => orderedQuery.ThenBy(instrument => instrument.Symbol),
				};
			}
		}

		if (orderedQuery == null)
			return query.OrderBy(instrument => instrument.Id);

		return orderedQuery.ThenBy(instrument => instrument.Id);
	}

	private static IQueryable<Strategy> ApplyStrategySort(IQueryable<Strategy> query, StrategySort strategySort)
	{
		IOrderedQueryable<Strategy>? orderedQuery = null;
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
