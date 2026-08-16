using System.ComponentModel.DataAnnotations;

namespace ViaTradeBackend.Contracts.Reminders;

public sealed record ConfirmReminderDeliveryRequest([Range(1, int.MaxValue)] int UserId);
