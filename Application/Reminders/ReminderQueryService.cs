using Application.Common.Models;
using Application.Common.Specifications;
using Application.Reminders.Interfaces;
using Application.Reminders.Models;
using Domain.Reminders.Entities;
using Domain.Strategies.Entities;

namespace Application.Reminders;

public class ReminderQueryService(IReminderRepository reminderRepository) : IReminderQueryService
{
	public async Task<ReminderStatisticsDto> GetStatisticsAsync(int userId, CancellationToken ct)
	{
		int total = await reminderRepository.CountAsync(x => x.UserId == userId, ct);

		return new ReminderStatisticsDto(total);
	}

	public async Task<IReadOnlyList<Reminder>> ListDueAsync(CancellationToken ct)
	{
		return await reminderRepository.ListDueAsync(ct);
	}

	public async Task<Reminder> GetAsync(int userId, int reminderId, CancellationToken ct)
	{
		var reminder = await reminderRepository.FindOneAsync(x => x.Id == reminderId && x.UserId == userId, ct);
		if (reminder == null)
			throw new KeyNotFoundException("Reminder not found.");

		return reminder;
	}

	public async Task<PageResult<Reminder>> GetPageAsync(
		int userId,
		int tradeCodeId,
		PageOptions page,
		ReminderSort sort,
		CancellationToken ct
	)
	{
		var spec = new ReminderQuerySpecification(userId, tradeCodeId, sort);
		return await reminderRepository.GetPageAsync(spec, page, ct);
	}

	public async Task<PageResult<Reminder>> GetPageAsync(
		int userId,
		PageOptions page,
		ReminderSort sort,
		CancellationToken ct
	)
	{
		var spec = new ReminderQuerySpecification(userId, null, sort);
		return await reminderRepository.GetPageAsync(spec, page, ct);
	}
}
