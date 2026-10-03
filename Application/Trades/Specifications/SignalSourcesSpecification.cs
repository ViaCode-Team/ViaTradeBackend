using Ardalis.Specification;
using ViaTrade.Application.Trades.Models;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Trades.Specifications;

public sealed class SignalSourcesSpecification : Specification<UserStrategyInstrument, SignalSourceDto>
{
	public SignalSourcesSpecification(int userId)
	{
		Query.Where(link =>
			link.UserId == userId && link.Strategy!.UserStrategies.Any(subscription => subscription.UserId == userId)
		);
		Query.Select(link => new SignalSourceDto(
			link.StrategyId,
			link.Strategy!.Name,
			link.Strategy.DisplayName,
			link.InstrumentId,
			link.Instrument!.Symbol,
			link.Strategy.Accuracy
		));
	}
}
