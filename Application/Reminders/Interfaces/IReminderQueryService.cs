using Application.Common.Models;
using Application.Reminders.Models;
using Domain.Entities;

namespace Application.Reminders.Interfaces;

public interface IReminderQueryService
{
	Task<ReminderStatisticsDto> GetStatisticsAsync(int userId, CancellationToken ct);
	Task<IReadOnlyList<ReminderDto>> ListDueAsync(CancellationToken ct);
	Task<Reminder> GetAsync(int userId, int reminderId, CancellationToken ct);
	Task<PageResult<ReminderDto>> GetPageAsync(
		int userId,
		int instrumentId,
		ReminderDeliveryStatus deliveryStatus,
		PageOptions pageOptions,
		ReminderSort reminderSort,
		CancellationToken ct
	);
	Task<PageResult<ReminderDto>> GetPageAsync(
		int userId,
		ReminderDeliveryStatus deliveryStatus,
		PageOptions pageOptions,
		ReminderSort reminderSort,
		CancellationToken ct
	);
}
