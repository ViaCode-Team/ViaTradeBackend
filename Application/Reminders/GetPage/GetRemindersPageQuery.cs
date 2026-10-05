using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Reminders.Common;

namespace ViaTrade.Application.Reminders.GetPage;

public sealed record GetRemindersPageQuery(
	int UserId,
	ReminderFilter ReminderFilter,
	ReminderSearch ReminderSearch,
	PageOptions PageOptions,
	ReminderSort ReminderSort
) : IQuery<PageResult<ReminderResult>>;
