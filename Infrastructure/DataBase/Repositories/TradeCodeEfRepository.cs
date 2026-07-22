using Application.Common.Models;
using Application.TradeCodes.Interfaces;
using Application.TradeCodes.Models;
using Domain.TradeCodes.Entities;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataBase.Repositories;

public class TradeCodeEfRepository(AppDbContext context) : GenericEfRepository<TradeCode>(context), ITradeCodeRepository
{
	public async Task<TradeCode?> FindByExchangeIdAsync(string code, CancellationToken ct)
	{
		return await _dbSet.Where(e => e.ExchangeId == code).FirstOrDefaultAsync(ct);
	}

	public async Task<int?> FindIdByExchangeIdAsync(string code, CancellationToken ct)
	{
		return await _dbSet.Where(e => e.ExchangeId == code).Select(e => (int?)e.Id).FirstOrDefaultAsync(ct);
	}

	public async Task<string?> FindExchangeIdByIdAsync(int id, CancellationToken ct)
	{
		return await _dbSet.Where(e => e.Id == id).Select(e => e.ExchangeId).FirstOrDefaultAsync(ct);
	}

	public async Task<Dictionary<string, int>> GetExchangeIdMapAsync(CancellationToken ct)
	{
		return await _dbSet
			.Select(tradeCode => new TradeCodeReferenceDto(tradeCode.Id, tradeCode.ExchangeId))
			.ToDictionaryAsync(
				tradeCode => tradeCode.ExchangeId,
				tradeCode => tradeCode.Id,
				StringComparer.OrdinalIgnoreCase,
				ct
			);
	}

	public async Task<PageResult<TradeCode>> GetPageAsync(
		PageOptions pageOptions,
		TradeCodeSort tradeCodeSort,
		CancellationToken ct
	)
	{
		var query = ApplySort(_dbSet, tradeCodeSort);

		return await query.ToPagedAsync(pageOptions, ct);
	}

	private static IQueryable<TradeCode> ApplySort(IQueryable<TradeCode> query, TradeCodeSort tradeCodeSort)
	{
		var sortFields = tradeCodeSort.GetEffectiveSortBy();

		if (sortFields.Count > 0)
		{
			IOrderedQueryable<TradeCode>? orderedQuery = null;
			foreach (var field in sortFields)
			{
				if (orderedQuery == null)
				{
					orderedQuery = field switch
					{
						TradeCodeSortField.NameDesc => query.OrderByDescending(e => e.ExchangeId),
						_ => query.OrderBy(e => e.ExchangeId),
					};
				}
				else
				{
					orderedQuery = field switch
					{
						TradeCodeSortField.NameDesc => orderedQuery.ThenByDescending(e => e.ExchangeId),
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
