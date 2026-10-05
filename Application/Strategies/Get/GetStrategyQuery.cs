using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Strategies.Common;

namespace ViaTrade.Application.Strategies.Get;

public sealed record GetStrategyQuery(int UserId, int StrategyId) : IQuery<StrategySubscriptionResult>;
