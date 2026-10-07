using Mediator;

namespace ViaTrade.Application.Reminders.GetStatistics;

public sealed record GetReminderStatisticsQuery() : IQuery<ReminderStatisticsResult>;
