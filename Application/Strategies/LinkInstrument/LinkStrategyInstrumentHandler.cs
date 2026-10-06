using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Application.Strategies.Common.Abstractions;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Strategies.LinkInstrument;

public sealed class LinkStrategyInstrumentHandler(
	IRepository<UserStrategyInstrument> userStrategyInstrumentRepository,
	IStrategyRepository strategyOperations,
	IUnitOfWork uow
) : ICommandHandler<LinkStrategyInstrumentCommand>
{
	public async Task HandleAsync(LinkStrategyInstrumentCommand command, CancellationToken ct = default)
	{
		var linkState = await strategyOperations.FindInstrumentLinkStateAsync(
			command.UserId,
			command.StrategyId,
			command.InstrumentId,
			ct
		);
		if (linkState == null)
			throw new NotFoundException("Strategy not found.", "strategy_not_found");

		if (!linkState.InstrumentExists)
			throw new NotFoundException("Instrument not found.", "instrument_not_found");

		if (linkState.LinkExists)
			return;

		var strategyCode = new UserStrategyInstrument
		{
			UserId = command.UserId,
			StrategyId = command.StrategyId,
			InstrumentId = command.InstrumentId,
		};

		userStrategyInstrumentRepository.Add(strategyCode);
		await uow.SaveChangesAsync(ct);
	}
}
