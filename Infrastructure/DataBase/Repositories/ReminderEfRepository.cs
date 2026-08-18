using Microsoft.EntityFrameworkCore;
using ViaTrade.Application.Common.Interfaces;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Notes.Models;
using ViaTrade.Application.Reminders.Interfaces;
using ViaTrade.Application.Reminders.Models;
using ViaTrade.Domain.Entities;
using ViaTrade.Infrastructure.Extensions;

namespace ViaTrade.Infrastructure.DataBase.Repositories;

public class ReminderEfRepository(AppDbContext context) : BaseEfRepository<Reminder>(context), IReminderRepository
{
	public async Task<IReadOnlyList<ReminderDto>> ListDueAsync(CancellationToken ct)
	{
		return await _dbSet
			.Where(reminder =>
				reminder.RemindAt <= DateTime.UtcNow
				&& reminder.PublishedAt == null
				&& reminder.User!.TelegramId != null
			)
			.Select(reminder => new ReminderDto(
				reminder.Id,
				reminder.Text,
				reminder.RemindAt,
				new InstrumentBriefDto(
					reminder.Instrument!.Id,
					reminder.Instrument.Symbol,
					reminder.Instrument.Description
				),
				reminder.UserId,
				reminder.User!.TelegramId!,
				reminder.DeliveredAt
			))
			.ToListAsync(ct);
	}

	public async Task<PageResult<ReminderProjectionDto>> GetPageWithInstrumentAsync(
		IQuerySpecification<Reminder> specification,
		PageOptions pageOptions,
		CancellationToken ct
	)
	{
		var query = SpecificationEvaluator.GetQueryForPagination(_dbSet, specification);

		return await query
			.Select(reminder => new ReminderProjectionDto(
				reminder.Id,
				reminder.Text,
				reminder.RemindAt,
				reminder.InstrumentId,
				reminder.Instrument!.Symbol,
				reminder.Instrument!.Description,
				reminder.UserId,
				reminder.DeliveredAt
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
				.Where(r => r.Id == reminderId && r.UserId == userId && r.PublishedAt == null && r.DeliveredAt == null)
				.ExecuteUpdateAsync(
					s =>
						s.SetProperty(r => r.Text, text)
							.SetProperty(r => r.RemindAt, remindAt)
							.SetProperty(r => r.PublishedAt, (DateTime?)null),
					ct
				)
		);

		return affectedRows;
	}

	public async Task<int> ExecuteMarkPublishedAsync(int reminderId, CancellationToken ct)
	{
		var publishedAt = DateTime.UtcNow;

		return await EfDatabaseOperation.ExecuteAsync(() =>
			_dbSet
				.Where(r => r.Id == reminderId && r.RemindAt <= publishedAt && r.PublishedAt == null)
				.ExecuteUpdateAsync(s => s.SetProperty(r => r.PublishedAt, publishedAt), ct)
		);
	}

	public async Task<int> ExecuteMarkDeliveredForUserAsync(int userId, int reminderId, CancellationToken ct)
	{
		var deliveredAt = DateTime.UtcNow;

		return await EfDatabaseOperation.ExecuteAsync(() =>
			_dbSet
				.Where(r => r.Id == reminderId && r.UserId == userId && r.RemindAt <= deliveredAt)
				.ExecuteUpdateAsync(
					s =>
						s.SetProperty(r => r.PublishedAt, r => r.PublishedAt ?? deliveredAt)
							.SetProperty(r => r.DeliveredAt, r => r.DeliveredAt ?? deliveredAt),
					ct
				)
		);
	}

	public Task<int> ExecuteDeleteDeliveredBeforeAsync(DateTime deliveredBefore, CancellationToken ct)
	{
		return EfDatabaseOperation.ExecuteAsync(() =>
			_dbSet.Where(r => r.DeliveredAt != null && r.DeliveredAt <= deliveredBefore).ExecuteDeleteAsync(ct)
		);
	}
}
