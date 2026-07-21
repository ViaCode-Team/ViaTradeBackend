using Application.Common.Models;
using Application.TradeCodes.Interfaces;
using Application.TradeCodes.Models;
using Domain.TradeCodes.Entities;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataBase.Repositories;

public class TradeCodeEfRepository(AppDbContext context) : GenericEfRepository<TradeCode>(context), ITradeCodeRepository
{
	public async Task<TradeCode?> GetByExchangeIdAsync(string code, CancellationToken ct)
	{
		return await _dbSet.Where(e => e.ExchangeId == code).FirstOrDefaultAsync(ct);
	}

	public async Task<int?> GetIdByExchangeIdAsync(string code, CancellationToken ct)
	{
		return await _dbSet.Where(e => e.ExchangeId == code).Select(e => (int?)e.Id).FirstOrDefaultAsync(ct);
	}

	public async Task<PageResult<TradeCode>> GetCodesPagedAsync(
		PageOptions page,
		TradeCodeSort sort,
		CancellationToken ct
	)
	{
		var query = _dbSet.AsQueryable();

		var sortFields = sort.GetEffectiveSortBy();

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

		return await query.ToPagedAsync(page, ct);
	}
}
