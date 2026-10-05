using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Application.Reminders.Common;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Reminders.Get;

public sealed class GetReminderHandler(IReadRepository<Reminder> reminderRepository)
	: IQueryHandler<GetReminderQuery, ReminderResult>
{
	public async Task<ReminderResult> HandleAsync(GetReminderQuery query, CancellationToken ct = default)
	{
		var reminder = await reminderRepository.FirstOrDefaultAsync(
			reminder => reminder.UserId == query.UserId && reminder.Id == query.ReminderId,
			ReminderResult.Projection,
			ct
		);
		if (reminder == null)
			throw new NotFoundException("Reminder not found.", "reminder_not_found");

		return reminder;
	}
}
