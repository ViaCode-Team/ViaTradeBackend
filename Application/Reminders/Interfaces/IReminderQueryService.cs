using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Reminders.Models;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Reminders.Interfaces;

public interface IReminderQueryService
{
	Task<ReminderStatisticsDto> GetStatisticsAsync(int userId, CancellationToken ct);
	Task<IReadOnlyList<ReminderDto>> ListDueBatchAsync(int limit, CancellationToken ct);
	Task<Reminder> GetAsync(int userId, int reminderId, CancellationToken ct);
	Task<PageResult<ReminderDto>> GetPageAsync(
		int userId,
		int instrumentId,
		ReminderFilter reminderFilter,
		ReminderSearch reminderSearch,
		PageOptions pageOptions,
		ReminderSort reminderSort,
		CancellationToken ct
	);
	Task<PageResult<ReminderDto>> GetPageAsync(
		int userId,
		ReminderFilter reminderFilter,
		ReminderSearch reminderSearch,
		PageOptions pageOptions,
		ReminderSort reminderSort,
		CancellationToken ct
	);
}
