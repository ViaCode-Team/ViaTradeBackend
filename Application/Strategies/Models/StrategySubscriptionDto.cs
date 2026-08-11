using Domain.Entities;

namespace Application.Strategies.Models;

public sealed record StrategySubscriptionDto(Strategy Strategy, bool IsSubscribed);
