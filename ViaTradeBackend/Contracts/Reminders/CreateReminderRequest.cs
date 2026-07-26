using System.ComponentModel.DataAnnotations;

namespace ViaTradeBackend.Contracts.Reminders;

public record CreateReminderRequest([StringLength(1024)] string Text, DateTime RemindAt);
