using ViaTrade.Application.Strategies.Models;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Infrastructure.Extensions;

public static class StrategySubscriptionQueryableExtensions
{
	public static IQueryable<StrategySubscriptionDto> WithSubscriptionState(this IQueryable<Strategy> query, int userId)
	{
		return query.Select(strategy => new StrategySubscriptionDto(
			strategy,
			strategy.UserStrategies.Any(link => link.UserId == userId)
		));
	}
}
