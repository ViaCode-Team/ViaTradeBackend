using System.ComponentModel.DataAnnotations;

namespace ViaTrade.Api.Contracts.Reminders;

public sealed record ConfirmReminderDeliveryRequest([Range(1, int.MaxValue)] int UserId);
