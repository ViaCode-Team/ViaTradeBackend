using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Reminders.Common;

namespace ViaTrade.Application.Reminders.Get;

public sealed record GetReminderQuery(int ReminderId) : IQuery<ReminderResult>;
