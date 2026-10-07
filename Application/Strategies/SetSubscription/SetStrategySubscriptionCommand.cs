using ViaTrade.Application.Common.Abstractions;

namespace ViaTrade.Application.Strategies.SetSubscription;

public sealed record SetStrategySubscriptionCommand(int StrategyId, bool? IsSubscribed) : ICommand;
