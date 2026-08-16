using System.ComponentModel.DataAnnotations;

namespace ViaTrade.Api.Contracts.Reminders;

public record UpdateReminderRequest([StringLength(1024, MinimumLength = 1)] string Text, DateTime RemindAt);
