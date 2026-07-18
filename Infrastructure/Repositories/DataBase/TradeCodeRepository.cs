using Domain.Trades.Enums;
using Domain.Trades.Entities;
using Application.Contracts.Dto.Trade;
using Application.Interfaces.Repositories.Database;
using Domain.TradeCodes.Entities;
using Domain.Enums;
using Domain.Models.Pagination;
using Domain.Models.Sort;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.DataBase;

public class TradeCodeRepository(AppDbContext context) : GenericRepository<TradeCode>(context), ITradeCodeRepository
{
	public async Task<int> CountAsync(CancellationToken cancellationToken = default)
	{
		return await _dbSet.CountAsync(cancellationToken);
	}

	public async Task<TradeCode?> GetByExchangeIdAsync(string code, CancellationToken cancellationToken = default)
	{
		return await _dbSet
			.Where(e => e.ExchangeId == code)
			.FirstOrDefaultAsync(cancellationToken);
	}

	public async Task<int?> GetIdByExchangeIdAsync(string code, CancellationToken cancellationToken = default)
	{
		return await _dbSet
			.Where(e => e.ExchangeId == code)
			.Select(e => (int?)e.Id)
			.FirstOrDefaultAsync(cancellationToken);
	}

	public async Task<PagedResult<TradeCode>> GetCodesPagedAsync(PaginationRequest paginationRequest, StockSortRequest? sortRequest = null, CancellationToken cancellationToken = default)
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

		return await query.ToPagedAsync(paginationRequest, cancellationToken);
	}
}
