using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Reminders.Common;

namespace ViaTrade.Application.Reminders.Get;

public sealed record GetReminderQuery(int UserId, int ReminderId) : IQuery<ReminderResult>;
