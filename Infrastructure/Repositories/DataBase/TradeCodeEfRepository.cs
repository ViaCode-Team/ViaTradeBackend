using Application.Common.Models.Pagination;
using Application.Common.Models.Sort;
using Application.TradeCodes.Interfaces;
using Domain.TradeCodes.Entities;
using Domain.Trades.Enums;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.DataBase;

public class TradeCodeEfRepository(AppDbContext context) : GenericEfRepository<TradeCode>(context), ITradeCodeRepository
{
	public async Task<int> CountAsync(CancellationToken ct = default)
	{
		return await _dbSet.CountAsync(ct);
	}

	public async Task<TradeCode?> GetByExchangeIdAsync(string code, CancellationToken ct = default)
	{
		return await _dbSet
			.Where(e => e.ExchangeId == code)
			.FirstOrDefaultAsync(ct);
	}

	public async Task<int?> GetIdByExchangeIdAsync(string code, CancellationToken ct = default)
	{
		return await _dbSet
			.Where(e => e.ExchangeId == code)
			.Select(e => (int?)e.Id)
			.FirstOrDefaultAsync(ct);
	}

	public async Task<PagedResult<TradeCode>> GetCodesPagedAsync(PaginationRequest paginationRequest, StockSortRequest? sortRequest = null, CancellationToken ct = default)
	{
		var query = _dbSet.AsQueryable();

		if (sortRequest?.SortBy != null && sortRequest.SortBy.Count > 0)
		{
			IOrderedQueryable<TradeCode>? orderedQuery = null;
			foreach (var field in sortRequest.SortBy)
			{
				if (orderedQuery == null)
				{
					orderedQuery = field switch
					{
						StockSortField.NameDesc => query.OrderByDescending(e => e.ExchangeId),
						_ => query.OrderBy(e => e.ExchangeId)
					};
				}
				else
				{
					orderedQuery = field switch
					{
						StockSortField.NameDesc => orderedQuery.ThenByDescending(e => e.ExchangeId),
						_ => orderedQuery.ThenBy(e => e.ExchangeId)
					};
				}
			}
			query = orderedQuery ?? query;
		}
		else
		{
			query = query.OrderBy(e => e.Id);
		}

		return await query.ToPagedAsync(paginationRequest, ct);
	}
}
