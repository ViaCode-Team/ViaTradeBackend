using Mediator;

namespace ViaTrade.Application.Reminders.DeleteDelivered;

public sealed record DeleteDeliveredRemindersCommand(DateTime DeliveredBefore) : ICommand<DeleteRemindersResult>;
