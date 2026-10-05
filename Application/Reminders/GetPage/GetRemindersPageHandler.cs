using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Reminders.Common;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Reminders.GetPage;

public sealed class GetRemindersPageHandler(IReadRepository<Reminder> reminderRepository)
	: IQueryHandler<GetRemindersPageQuery, PageResult<ReminderResult>>
{
	public async Task<PageResult<ReminderResult>> HandleAsync(
		GetRemindersPageQuery query,
		CancellationToken ct = default
	)
	{
		var specification = new RemindersPageSpecification(
			query.UserId,
			query.ReminderFilter,
			query.ReminderSearch,
			query.PageOptions,
			query.ReminderSort
		);
		return await reminderRepository.GetPageAsync(
			specification,
			ReminderResult.Projection,
			ct
		);
	}
}
