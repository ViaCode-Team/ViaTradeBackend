using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Application.Trades.Common.Abstractions;

namespace ViaTrade.Application.Trades.Update;

public sealed class UpdateTradeHandler(ITradeRepository tradeOperations) : ICommandHandler<UpdateTradeCommand>
{
	public async Task HandleAsync(UpdateTradeCommand command, CancellationToken ct = default)
	{
		var price = (decimal)command.Trade.OpenPrice * command.Trade.Quantity;

		var affectedRows = await tradeOperations.ExecuteUpdateAsync(command.UserId, command.TradeId, command.Trade, price, ct);
		if (affectedRows == 0)
			throw new NotFoundException("Trade not found.", "trade_not_found");
	}
}
