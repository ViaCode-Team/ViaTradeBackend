using Application.Interfaces.Repositories.Database;
using Domain.Entities.DataBase;
using Domain.Enums;
using Domain.Models.Dto.Trade;
using Domain.Models.Pagination;
using Domain.Models.Sort;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.DataBase;

public class TradeCodeRepository(AppDbContext context) : GenericRepository<TradeCode, TradeCodeDto>(context), ITradeCodeRepository
{
	public async Task<int> CountAsync(CancellationToken cancellationToken = default)
	{
		return await _dbSet.CountAsync(cancellationToken);
	}

	public async Task<TradeCodeDto?> GetByExchangeIdAsync(string code, CancellationToken cancellationToken = default)
	{
		return await _dbSet
			.Where(e => e.ExchangeId == code)
			.Select(e => new TradeCodeDto
			{
				Id = e.Id,
				ExchangeId = e.ExchangeId,
				Description = e.Description
			})
			.FirstOrDefaultAsync(cancellationToken);
	}

	public async Task<int?> GetIdByExchangeIdAsync(string code, CancellationToken cancellationToken = default)
	{
		return await _dbSet
			.Where(e => e.ExchangeId == code)
			.Select(e => (int?)e.Id)
			.FirstOrDefaultAsync(cancellationToken);
	}

	public async Task<PagedResult<TradeCodeDto>> GetCodesPagedAsync(PaginationRequest paginationRequest, StockSortRequest? sortRequest = null, CancellationToken cancellationToken = default)
	{
		var query = _dbSet.Select(e => new TradeCodeDto
		{
			Id = e.Id,
			ExchangeId = e.ExchangeId,
			Description = e.Description
		});

		if (sortRequest?.SortBy != null && sortRequest.SortBy.Count > 0)
		{
			IOrderedQueryable<TradeCodeDto>? orderedQuery = null;
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

		return await query.ToPagedAsync(paginationRequest, cancellationToken);
	}
}
