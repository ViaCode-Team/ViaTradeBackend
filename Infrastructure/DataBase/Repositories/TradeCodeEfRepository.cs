using Application.Common.Models;
using Application.TradeCodes.Interfaces;
using Application.TradeCodes.Models;
using Domain.TradeCodes.Entities;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataBase.Repositories;

public class TradeCodeEfRepository(AppDbContext context) : GenericEfRepository<TradeCode>(context), ITradeCodeRepository
{
	public async Task<TradeCode?> FindByTickerAsync(string ticker, CancellationToken ct)
	{
		return await _dbSet.Where(e => e.ExchangeId == ticker).FirstOrDefaultAsync(ct);
	}

	public async Task<int?> FindIdByTickerAsync(string ticker, CancellationToken ct)
	{
		return await _dbSet.Where(e => e.ExchangeId == ticker).Select(e => (int?)e.Id).FirstOrDefaultAsync(ct);
	}

	public async Task<string?> FindTickerByIdAsync(int tradeCodeId, CancellationToken ct)
	{
		return await _dbSet.Where(e => e.Id == tradeCodeId).Select(e => e.ExchangeId).FirstOrDefaultAsync(ct);
	}

	public async Task<Dictionary<string, int>> GetTradeCodeIdByTickerAsync(CancellationToken ct)
	{
		return await _dbSet
			.Select(tradeCode => new InstrumentReferenceDto(tradeCode.Id, tradeCode.ExchangeId))
			.ToDictionaryAsync(
				tradeCode => tradeCode.Symbol,
				tradeCode => tradeCode.Id,
				StringComparer.OrdinalIgnoreCase,
				ct
			);
	}

	public async Task<PageResult<TradeCode>> GetPageAsync(
		InstrumentFilter instrumentFilter,
		PageOptions pageOptions,
		InstrumentSort instrumentSort,
		CancellationToken ct
	)
	{
		var query = _dbSet.AsQueryable();
		if (!string.IsNullOrWhiteSpace(instrumentFilter.Symbol))
		{
			var pattern = $"%{instrumentFilter.Symbol}%";
			query = query.Where(tradeCode => EF.Functions.Like(tradeCode.ExchangeId, pattern));
		}

		query = ApplySort(query, instrumentSort);

		return await query.ToPagedAsync(pageOptions, ct);
	}

	private static IQueryable<TradeCode> ApplySort(IQueryable<TradeCode> query, InstrumentSort instrumentSort)
	{
		var sortFields = instrumentSort.GetEffectiveSortBy();

		if (sortFields.Count > 0)
		{
			IOrderedQueryable<TradeCode>? orderedQuery = null;
			foreach (var field in sortFields)
			{
				if (orderedQuery == null)
				{
					orderedQuery = field switch
					{
						InstrumentSortField.SymbolDesc => query.OrderByDescending(e => e.ExchangeId),
						_ => query.OrderBy(e => e.ExchangeId),
					};
				}
				else
				{
					orderedQuery = field switch
					{
						InstrumentSortField.SymbolDesc => orderedQuery.ThenByDescending(e => e.ExchangeId),
						_ => orderedQuery.ThenBy(e => e.ExchangeId),
					};
				}
			}
			query = orderedQuery ?? query;
		}
		else
		{
			query = query.OrderBy(e => e.Id);
		}

		return query;
	}
}
