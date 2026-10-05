using Microsoft.Extensions.Options;
using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Configuration.Options;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Reminders.GetStatistics;

public sealed class GetReminderStatisticsHandler(
	IReadRepository<Reminder> reminderRepository,
	IOptions<ReminderLimitsSettings> reminderLimitsOptions
) : IQueryHandler<GetReminderStatisticsQuery, ReminderStatisticsResult>
{
	public async Task<ReminderStatisticsResult> HandleAsync(
		GetReminderStatisticsQuery query,
		CancellationToken ct = default
	)
	{
		int total = await reminderRepository.CountAsync(reminder => reminder.UserId == query.UserId, ct);
		int remaining = Math.Max(0, reminderLimitsOptions.Value.MaxRemindersPerUser - total);

		return new ReminderStatisticsResult(total, reminderLimitsOptions.Value.MaxRemindersPerUser, remaining);
	}
}
