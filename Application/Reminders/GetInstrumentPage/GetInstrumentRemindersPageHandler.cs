using Mediator;
using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Reminders.Common;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Reminders.GetInstrumentPage;

public sealed class GetInstrumentRemindersPageHandler(
	IUserContext userContext,
	IReadRepository<Instrument> instrumentRepository,
	IReadRepository<Reminder> reminderRepository
) : IQueryHandler<GetInstrumentRemindersPageQuery, PageResult<ReminderResult>>
{
	public async ValueTask<PageResult<ReminderResult>> Handle(
		GetInstrumentRemindersPageQuery query,
		CancellationToken ct
	)
	{
		var instrumentExists = await instrumentRepository.AnyAsync(
			instrument => instrument.Id == query.InstrumentId,
			ct
		);

		if (!instrumentExists)
			throw new NotFoundException("Instrument not found.", "instrument_not_found");

		var specification = new RemindersPageSpecification(
			userContext.UserId,
			query.ReminderFilter,
			query.ReminderSearch,
			query.PageOptions,
			query.ReminderSort,
			query.InstrumentId
		);

		return await reminderRepository.GetPageAsync(specification, ReminderResult.Projection, ct);
	}
}
