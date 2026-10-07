using Mediator;
using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Reminders.Common;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Reminders.GetPage;

public sealed class GetRemindersPageHandler(IUserContext userContext, IReadRepository<Reminder> reminderRepository)
	: IQueryHandler<GetRemindersPageQuery, PageResult<ReminderResult>>
{
	public async ValueTask<PageResult<ReminderResult>> Handle(GetRemindersPageQuery query, CancellationToken ct)
	{
		var specification = new RemindersPageSpecification(
			userContext.UserId,
			query.ReminderFilter,
			query.ReminderSearch,
			query.PageOptions,
			query.ReminderSort
		);

		return await reminderRepository.GetPageAsync(specification, ReminderResult.Projection, ct);
	}
}
