using Ardalis.Specification;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Signals.Common;

public sealed class SignalSourcesSpecification : Specification<UserStrategyInstrument, SignalSource>
{
	public SignalSourcesSpecification(int userId)
	{
		Query.Where(link =>
			link.UserId == userId && link.Strategy!.UserStrategies.Any(subscription => subscription.UserId == userId)
		);
		Query.Select(link => new SignalSource(
			link.StrategyId,
			link.Strategy!.Name,
			link.Strategy.DisplayName,
			link.InstrumentId,
			link.Instrument!.Symbol,
			link.Strategy.Accuracy
		));
	}
}
