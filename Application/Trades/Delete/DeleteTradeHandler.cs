using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Trades.Delete;

public sealed class DeleteTradeHandler(IRepository<Trade> tradeRepository) : ICommandHandler<DeleteTradeCommand>
{
	public async Task HandleAsync(DeleteTradeCommand command, CancellationToken ct = default)
	{
		var affectedRows = await tradeRepository.ExecuteDeleteAsync(t => t.Id == command.TradeId && t.UserId == command.UserId, ct);
		if (affectedRows == 0)
			throw new NotFoundException("Trade not found.", "trade_not_found");
	}
}
