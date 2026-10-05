using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Reminders.Common;

namespace ViaTrade.Application.Reminders.GetInstrumentPage;

public sealed record GetInstrumentRemindersPageQuery(
	int UserId,
	int InstrumentId,
	ReminderFilter ReminderFilter,
	ReminderSearch ReminderSearch,
	PageOptions PageOptions,
	ReminderSort ReminderSort
) : IQuery<PageResult<ReminderResult>>;
