using Application.Common.Models.Pagination;
using Application.Common.Models.Sort;
using Application.Common.Specifications;
using Application.Reminders.Interfaces;
using Domain.Reminds.Entities;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Repositories.DataBase;

public class ReminderEfRepository(AppDbContext context)
	: GenericEfRepository<Reminder>(context), IReminderRepository
{
	public async Task<IEnumerable<Reminder>> GetActualReminder(CancellationToken ct)
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
