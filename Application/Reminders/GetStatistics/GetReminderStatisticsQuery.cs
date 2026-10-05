using ViaTrade.Application.Common.Abstractions;

namespace ViaTrade.Application.Reminders.GetStatistics;

public sealed record GetReminderStatisticsQuery(int UserId) : IQuery<ReminderStatisticsResult>;
