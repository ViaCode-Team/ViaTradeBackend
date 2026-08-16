using System.ComponentModel.DataAnnotations;

namespace ViaTrade.Api.Contracts.Reminders;

public record CreateReminderRequest([StringLength(1024, MinimumLength = 1)] string Text, DateTime RemindAt);
