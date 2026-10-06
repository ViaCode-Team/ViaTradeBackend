using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Strategies.UnlinkInstrument;

public sealed class UnlinkStrategyInstrumentHandler(
	IRepository<UserStrategyInstrument> userStrategyInstrumentRepository
) : ICommandHandler<UnlinkStrategyInstrumentCommand>
{
	public async Task HandleAsync(UnlinkStrategyInstrumentCommand command, CancellationToken ct = default)
	{
		var affectedRows = await userStrategyInstrumentRepository.ExecuteDeleteAsync(
			e =>
				e.UserId == command.UserId
				&& e.StrategyId == command.StrategyId
				&& e.InstrumentId == command.InstrumentId,
			ct
		);

		if (affectedRows == 0)
			throw new NotFoundException("User strategy code not found.", "strategy_code_not_found");
	}
}
