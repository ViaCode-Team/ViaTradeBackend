using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Trades.Common;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Trades.Create;

public sealed class CreateTradeHandler(
	IRepository<Trade> tradeRepository,
	IReadRepository<Instrument> instrumentRepository,
	IUnitOfWork uow
) : ICommandHandler<CreateTradeCommand, TradeResult>
{
	public async Task<TradeResult> HandleAsync(CreateTradeCommand command, CancellationToken ct = default)
	{
		var trade = new Trade
		{
			OpenedAt = command.Trade.OpenedAt,
			ClosedAt = command.Trade.ClosedAt,
			OpenPrice = command.Trade.OpenPrice,
			ClosePrice = command.Trade.ClosePrice,
			Quantity = command.Trade.Quantity,
			TradeTypeId = command.Trade.TradeTypeId,
			InstrumentId = command.Trade.InstrumentId,
			UserId = command.UserId,
			Signal = command.Trade.Signal,
			TotalPrice = (decimal)command.Trade.OpenPrice * command.Trade.Quantity,
		};

		tradeRepository.Add(trade);
		await uow.SaveChangesAsync(ct);

		var instrument = await instrumentRepository.FirstOrDefaultAsync(
			instrument => instrument.Id == trade.InstrumentId,
			InstrumentBriefResult.Projection,
			ct
		);
		if (instrument == null)
			throw new DataIntegrityException(
				$"Trade code was not found after trade creation. InstrumentId={trade.InstrumentId}."
			);

		return new TradeResult(
			trade.Id,
			trade.OpenedAt,
			trade.ClosedAt,
			trade.OpenPrice,
			trade.ClosePrice,
			trade.NetIncome,
			trade.Quantity,
			trade.TotalPrice,
			trade.Signal,
			trade.TradeTypeId,
			instrument,
			trade.UserId
		);
	}
}
