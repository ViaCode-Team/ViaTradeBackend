using Application.Common.Queries;
using Application.Common.Specifications;
using Application.Reminders.Interfaces;
using Application.Reminders.Models;
using Application.Reminders.Queries;
using Domain.Reminders.Entities;

namespace Application.Reminders;

public class ReminderQueryService(IReminderRepository reminderRepository) : IReminderQueryService
{
	public async Task<ReminderStatistics> GetStatisticsAsync(int userId, CancellationToken ct)
	{
		int total = await reminderRepository.CountAsync(
			x => x.UserId == userId, ct);

		return new ReminderStatistics(total);
	}

	public async Task<IEnumerable<Reminder>> GetAsync(CancellationToken ct)
	{
		return await reminderRepository.FindAsync(
			x => x.DateTime <= DateTime.UtcNow, ct);
	}

	public async Task<Reminder> GetAsync(int reminderId, int userId, CancellationToken ct)
	{
		return await reminderRepository.FirstOrDefaultAsync(
			x => x.Id == reminderId && x.UserId == userId, ct)
			?? throw new KeyNotFoundException("Reminder not found.");
	}

	public async Task<PageResult<Reminder>> GetAsync(
		int userId,
		int tradeCodeId,
		PageOptions page,
		ReminderSort sort,
		CancellationToken ct)
	{
		var spec = new ReminderQuerySpecification(userId, tradeCodeId, sort);
		return await reminderRepository.GetPagedAsync(spec, page, ct);
	}

	public async Task<PageResult<Reminder>> GetAsync(
		int userId,
		PageOptions page,
		ReminderSort sort,
		CancellationToken ct)
	{
		var spec = new ReminderQuerySpecification(userId, null, sort);
		return await reminderRepository.GetPagedAsync(spec, page, ct);
	}
}
