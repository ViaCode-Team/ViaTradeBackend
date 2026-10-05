using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Reminders.Common;

namespace ViaTrade.Application.Reminders.ListDue;

public sealed record ListDueRemindersQuery(int Limit) : IQuery<IReadOnlyList<ReminderResult>>;
