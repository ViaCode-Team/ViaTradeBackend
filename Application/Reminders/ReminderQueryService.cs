using Application.Common.Exceptions;
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
			throw new NotFoundException("Reminder not found.", "reminder_not_found");

		return reminder;
	}

	public async Task<PageResult<Reminder>> GetPageAsync(
		int userId,
		int tradeCodeId,
		PageOptions pageOptions,
		ReminderSort reminderSort,
		CancellationToken ct
	)
	{
		var spec = new ReminderQuerySpecification(userId, tradeCodeId, reminderSort);
		return await reminderRepository.GetPageAsync(spec, pageOptions, ct);
	}

	public async Task<PageResult<Reminder>> GetPageAsync(
		int userId,
		PageOptions pageOptions,
		ReminderSort reminderSort,
		CancellationToken ct
	)
	{
		var spec = new ReminderQuerySpecification(userId, null, reminderSort);
		return await reminderRepository.GetPageAsync(spec, pageOptions, ct);
	}
}
