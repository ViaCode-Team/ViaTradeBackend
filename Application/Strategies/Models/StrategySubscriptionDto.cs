using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Strategies.Models;

public sealed record StrategySubscriptionDto(Strategy Strategy, bool IsSubscribed);
