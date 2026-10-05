using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Reminders.Common;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Reminders.ListDue;

public sealed class ListDueRemindersHandler(IReadRepository<Reminder> reminderRepository)
	: IQueryHandler<ListDueRemindersQuery, IReadOnlyList<ReminderResult>>
{
	public async Task<IReadOnlyList<ReminderResult>> HandleAsync(
		ListDueRemindersQuery query,
		CancellationToken ct = default
	)
	{
		ArgumentOutOfRangeException.ThrowIfLessThan(query.Limit, 1);

		var specification = new DueRemindersSpecification(query.Limit, DateTime.UtcNow);
		return await reminderRepository.ListAsync(specification, ct);
	}
}
