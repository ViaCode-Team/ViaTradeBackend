using Application.Common.Models.Pagination;
using Application.Common.Models.Sort;
using Application.Common.Specifications;
using Application.Reminds.Interfaces;
using Domain.Reminds.Entities;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Repositories.DataBase;

public class TradeRemindEfRepository(AppDbContext context)
	: GenericEfRepository<Reminder>(context), ITradeRemindRepository
{
	public async Task<IEnumerable<Reminder>> GetActualRemind(CancellationToken ct)
	{
		return await _dbSet.Where(r => r.DateTime <= DateTime.Now).ToListAsync(ct);
	}

	public async Task<PagedResult<Reminder>> GetByUserPagedAsync(int userId, PaginationRequest paginationRequest, RemindSortRequest? sortRequest = null, CancellationToken ct = default)
	{
		var spec = new TradeRemindQuerySpecification(userId, null, sortRequest);
		return await GetPagedAsync(spec, paginationRequest, ct);
	}

	public async Task<int> CountByUserAsync(int userId, CancellationToken ct)
	{
		return await _dbSet.CountAsync(r => r.UserId == userId, ct);
	}

	public async Task<PagedResult<Reminder>> GetByUserAndTradeCodePagedAsync(int userId, int tradeCodeId, PaginationRequest paginationRequest, RemindSortRequest? sortRequest = null, CancellationToken ct = default)
	{
		var spec = new TradeRemindQuerySpecification(userId, tradeCodeId, sortRequest);
		return await GetPagedAsync(spec, paginationRequest, ct);
	}

	public async Task<int> ExecuteUpdateUserRemindAsync(int remindId, int userId, string textRemind, DateTime dateTime, CancellationToken ct = default)
	{
		return await _dbSet
			.Where(r => r.Id == remindId && r.UserId == userId)
			.ExecuteUpdateAsync(s => s
				.SetProperty(r => r.TextRemind, textRemind)
				.SetProperty(r => r.DateTime, dateTime),
				ct);
	}
}
