using Mediator;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Reminders.ListDue;

public sealed class ListDueRemindersHandler(IReadRepository<Reminder> reminderRepository)
	: IQueryHandler<ListDueRemindersQuery, IReadOnlyList<DueReminderResult>>
{
	public async ValueTask<IReadOnlyList<DueReminderResult>> Handle(ListDueRemindersQuery query, CancellationToken ct)
	{
		ArgumentOutOfRangeException.ThrowIfLessThan(query.Limit, 1);

		var specification = new DueRemindersSpecification(query.Limit, DateTime.UtcNow);

		return await reminderRepository.ListAsync(specification, ct);
	}
}
