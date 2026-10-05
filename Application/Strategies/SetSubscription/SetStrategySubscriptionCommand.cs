using ViaTrade.Application.Common.Abstractions;

namespace ViaTrade.Application.Strategies.SetSubscription;

public sealed record SetStrategySubscriptionCommand(int UserId, int StrategyId, bool IsSubscribed) : ICommand;
