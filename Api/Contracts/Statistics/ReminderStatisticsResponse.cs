using System.ComponentModel.DataAnnotations;

namespace ViaTrade.Api.Contracts.Statistics;

public record ReminderStatisticsResponse(
	[Range(0, int.MaxValue)] int TotalReminders,
	[Range(1, int.MaxValue)] int MaximumReminders,
	[Range(0, int.MaxValue)] int RemainingReminders
);
