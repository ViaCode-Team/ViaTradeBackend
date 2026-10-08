using Mediator;
using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Trades.Common;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Trades.Create;

public sealed class CreateTradeHandler(
	IUserContext userContext,
	IRepository<Trade> tradeRepository,
	IReadRepository<Instrument> instrumentRepository,
	IUnitOfWork uow
) : ICommandHandler<CreateTradeCommand, TradeResult>
{
	public async ValueTask<TradeResult> Handle(CreateTradeCommand command, CancellationToken ct)
	{
		var trade = new Trade
		{
			OpenedAt = command.OpenedAt,
			ClosedAt = command.ClosedAt,
			OpenPrice = command.OpenPrice,
			ClosePrice = command.ClosePrice,
			Quantity = command.Quantity,
			TradeTypeId = command.TradeTypeId,
			InstrumentId = command.InstrumentId,
			UserId = userContext.UserId,
			Signal = command.Signal,
			TotalPrice = (decimal)command.OpenPrice * command.Quantity,
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
