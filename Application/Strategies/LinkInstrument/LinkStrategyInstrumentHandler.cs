using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Application.Strategies.Common.Abstractions;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Strategies.LinkInstrument;

public sealed class LinkStrategyInstrumentHandler(
	IUserContext userContext,
	IRepository<UserStrategyInstrument> userStrategyInstrumentRepository,
	IStrategyRepository strategyOperations,
	IUnitOfWork uow
) : IVoidCommandHandler<LinkStrategyInstrumentCommand>
{
	public async ValueTask Handle(LinkStrategyInstrumentCommand command, CancellationToken ct)
	{
		var linkState = await strategyOperations.FindInstrumentLinkStateAsync(
			userContext.UserId,
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
			UserId = userContext.UserId,
			StrategyId = command.StrategyId,
			InstrumentId = command.InstrumentId,
		};

		userStrategyInstrumentRepository.Add(strategyCode);
		await uow.SaveChangesAsync(ct);
	}
}
