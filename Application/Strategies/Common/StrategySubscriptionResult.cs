using System.Linq.Expressions;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Strategies.Common;

public sealed record StrategySubscriptionResult(
	int Id,
	string Name,
	string? Description,
	string DisplayName,
	int? Accuracy,
	string? SignalFrequency,
	string? InvestmentHorizon,
	string? LogicDescription,
	string? UsageDescription,
	string? LimitationsDescription,
	bool IsSubscribed
)
{
	public static Expression<Func<Strategy, StrategySubscriptionResult>> Projection(int userId)
	{
		return strategy => new StrategySubscriptionResult(
			strategy.Id,
			strategy.Name,
			strategy.Description,
			strategy.DisplayName,
			strategy.Accuracy,
			strategy.SignalFrequency,
			strategy.InvestmentHorizon,
			strategy.LogicDescription,
			strategy.UsageDescription,
			strategy.LimitationsDescription,
			strategy.UserStrategies.Any(link => link.UserId == userId)
		);
	}

	public static Expression<Func<UserStrategyInstrument, StrategySubscriptionResult>> LinkProjection(int userId)
	{
		return link => new StrategySubscriptionResult(
			link.Strategy!.Id,
			link.Strategy.Name,
			link.Strategy.Description,
			link.Strategy.DisplayName,
			link.Strategy.Accuracy,
			link.Strategy.SignalFrequency,
			link.Strategy.InvestmentHorizon,
			link.Strategy.LogicDescription,
			link.Strategy.UsageDescription,
			link.Strategy.LimitationsDescription,
			link.Strategy.UserStrategies.Any(subscription => subscription.UserId == userId)
		);
	}
}
