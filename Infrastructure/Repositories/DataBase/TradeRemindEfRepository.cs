using Application.Common.Models.Pagination;
using Application.Common.Models.Sort;
using Application.Common.Specifications;
using Application.Reminds.Interfaces;
using Domain.Reminds.Entities;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Repositories.DataBase;

public class TradeRemindEfRepository(AppDbContext context)
	: GenericEfRepository<TradeRemind>(context), ITradeRemindRepository
{
	public async Task<IEnumerable<TradeRemind>> GetActualRemind(CancellationToken cancellationToken)
	{
		return await _dbSet.Where(r => r.DateTime <= DateTime.Now).ToListAsync(cancellationToken);
	}

	public async Task<PagedResult<TradeRemind>> GetByUserPagedAsync(int userId, PaginationRequest paginationRequest, RemindSortRequest? sortRequest = null, CancellationToken cancellationToken = default)
	{
		var spec = new TradeRemindQuerySpecification(userId, null, sortRequest);
		return await GetPagedAsync(spec, paginationRequest, cancellationToken);
	}

	public async Task<int> CountByUserAsync(int userId, CancellationToken cancellationToken)
	{
		return await _dbSet.CountAsync(r => r.UserId == userId, cancellationToken);
	}

	public async Task<PagedResult<TradeRemind>> GetByUserAndTradeCodePagedAsync(int userId, int tradeCodeId, PaginationRequest paginationRequest, RemindSortRequest? sortRequest = null, CancellationToken cancellationToken = default)
	{
		var spec = new TradeRemindQuerySpecification(userId, tradeCodeId, sortRequest);
		return await GetPagedAsync(spec, paginationRequest, cancellationToken);
	}

	public async Task<int> UpdateUserRemindAsync(int remindId, int userId, string textRemind, DateTime dateTime, CancellationToken cancellationToken = default)
	{
		return await _dbSet
			.Where(r => r.Id == remindId && r.UserId == userId)
			.ExecuteUpdateAsync(s => s
				.SetProperty(r => r.TextRemind, textRemind)
				.SetProperty(r => r.DateTime, dateTime),
				cancellationToken);
	}
}
