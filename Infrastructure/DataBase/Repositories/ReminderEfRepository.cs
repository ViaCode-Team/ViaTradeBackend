using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Notes.Models;
using Application.Common.Specifications;
using Application.Reminders.Interfaces;
using Application.Reminders.Models;
using Domain.Reminders.Entities;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataBase.Repositories;

public class ReminderEfRepository(AppDbContext context) : GenericEfRepository<Reminder>(context), IReminderRepository
{
	public async Task<IReadOnlyList<ReminderDto>> ListDueAsync(CancellationToken ct)
	{
		return await _dbSet
			.Where(reminder => reminder.DateTime <= DateTime.UtcNow)
			.Select(reminder => new ReminderDto(
				reminder.Id,
				reminder.TextRemind,
				reminder.DateTime,
				new TradeCodeBriefDto(
					reminder.TradeCode!.Id,
					reminder.TradeCode.ExchangeId,
					reminder.TradeCode.Description
				),
				reminder.UserId
			))
			.ToListAsync(ct);
	}

	public async Task<PageResult<ReminderProjectionDto>> GetPageWithTradeCodeAsync(
		IQuerySpecification<Reminder> specification,
		PageOptions pageOptions,
		CancellationToken ct
	)
	{
		var query = SpecificationEvaluator.GetQuery(_dbSet, specification);

		return await query
			.Select(reminder => new ReminderProjectionDto(
				reminder.Id,
				reminder.TextRemind,
				reminder.DateTime,
				reminder.TradeCodeId,
				reminder.TradeCode!.ExchangeId,
				reminder.TradeCode!.Description,
				reminder.UserId
			))
			.ToPagedAsync(pageOptions, ct);
	}

	public async Task<Reminder?> FindByUserAndIdAsync(int userId, int reminderId, CancellationToken ct)
	{
		return await _dbSet
			.Include(reminder => reminder.TradeCode)
			.FirstOrDefaultAsync(reminder => reminder.Id == reminderId && reminder.UserId == userId, ct);
	}

	public async Task<int> CountByUserAsync(int userId, CancellationToken ct)
	{
		return await _dbSet.CountAsync(r => r.UserId == userId, ct);
	}

	public async Task<int> ExecuteUpdateForUserAsync(
		int userId,
		int reminderId,
		string text,
		DateTime dateTime,
		CancellationToken ct
	)
	{
		var affectedRows = await EfDatabaseOperation.ExecuteAsync(() =>
			_dbSet
				.Where(r => r.Id == reminderId && r.UserId == userId)
				.ExecuteUpdateAsync(
					s => s.SetProperty(r => r.TextRemind, text).SetProperty(r => r.DateTime, dateTime),
					ct
				)
		);

		return affectedRows;
	}
}
