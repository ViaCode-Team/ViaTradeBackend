using System.ComponentModel.DataAnnotations;

namespace ViaTradeBackend.Contracts.Statistics;

public record ReminderStatisticsResponse([Range(0, int.MaxValue)] int TotalReminders);
