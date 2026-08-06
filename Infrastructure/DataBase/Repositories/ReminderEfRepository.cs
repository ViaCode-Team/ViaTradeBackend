using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Notes.Models;
using Application.Reminders.Interfaces;
using Application.Reminders.Models;
using Domain.Entities;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataBase.Repositories;

public class ReminderEfRepository(AppDbContext context) : BaseEfRepository<Reminder>(context), IReminderRepository
{
	public async Task<IReadOnlyList<ReminderDto>> ListDueAsync(CancellationToken ct)
	{
		return await _dbSet
			.Where(reminder => reminder.RemindAt <= DateTime.UtcNow)
			.Select(reminder => new ReminderDto(
				reminder.Id,
				reminder.Text,
				reminder.RemindAt,
				new InstrumentBriefDto(
					reminder.Instrument!.Id,
					reminder.Instrument.Symbol,
					reminder.Instrument.Description
				),
				reminder.UserId
			))
			.ToListAsync(ct);
	}

	public async Task<PageResult<ReminderProjectionDto>> GetPageWithInstrumentAsync(
		IQuerySpecification<Reminder> specification,
		PageOptions pageOptions,
		CancellationToken ct
	)
	{
		var query = SpecificationEvaluator.GetQuery(_dbSet, specification);

		return await query
			.Select(reminder => new ReminderProjectionDto(
				reminder.Id,
				reminder.Text,
				reminder.RemindAt,
				reminder.InstrumentId,
				reminder.Instrument!.Symbol,
				reminder.Instrument!.Description,
				reminder.UserId
			))
			.ToPagedAsync(pageOptions, ct);
	}

	public async Task<Reminder?> FindByUserAndIdAsync(int userId, int reminderId, CancellationToken ct)
	{
		return await _dbSet
			.Include(reminder => reminder.Instrument)
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
		DateTime remindAt,
		CancellationToken ct
	)
	{
		var affectedRows = await EfDatabaseOperation.ExecuteAsync(() =>
			_dbSet
				.Where(r => r.Id == reminderId && r.UserId == userId)
				.ExecuteUpdateAsync(s => s.SetProperty(r => r.Text, text).SetProperty(r => r.RemindAt, remindAt), ct)
		);

		return affectedRows;
	}
}
