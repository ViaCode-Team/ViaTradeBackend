using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Application.Common.Interfaces;
using ViaTrade.Application.Instruments.Interfaces;
using ViaTrade.Application.Trades.Interfaces;
using ViaTrade.Application.Trades.Models;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Trades;

public class TradeCommandService(
	ITradeRepository tradeRepository,
	IInstrumentRepository instrumentRepository,
	IUnitOfWork uow
) : ITradeCommandService
{
	public async Task<TradeDto> CreateAsync(int userId, TradeInputDto request, CancellationToken ct)
	{
		var trade = new Trade
		{
			OpenedAt = request.OpenedAt,
			ClosedAt = request.ClosedAt,
			OpenPrice = request.OpenPrice,
			ClosePrice = request.ClosePrice,
			Quantity = request.Quantity,
			TradeTypeId = request.TradeTypeId,
			InstrumentId = request.InstrumentId,
			UserId = userId,
			Signal = request.Signal,
			TotalPrice = (decimal)request.OpenPrice * request.Quantity,
		};

		await tradeRepository.AddAsync(trade, ct);
		await uow.SaveChangesAsync(ct);

		var instrument = await instrumentRepository.FindByIdAsync(trade.InstrumentId, ct);
		if (instrument == null)
			throw new DataIntegrityException(
				$"Trade code was not found after trade creation. InstrumentId={trade.InstrumentId}."
			);

		return new TradeDto(
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
			new InstrumentSummaryDto(instrument.Id, instrument.Symbol, instrument.Description),
			trade.UserId
		);
	}

	public async Task DeleteAsync(int userId, int id, CancellationToken ct)
	{
		var affectedRows = await tradeRepository.ExecuteDeleteAsync(t => t.Id == id && t.UserId == userId, ct);
		if (affectedRows == 0)
			throw new NotFoundException("Trade not found.", "trade_not_found");
	}

	public async Task UpdateAsync(int userId, int id, TradeInputDto request, CancellationToken ct)
	{
		var price = (decimal)request.OpenPrice * request.Quantity;

		var affectedRows = await tradeRepository.ExecuteUpdateAsync(userId, id, request, price, ct);
		if (affectedRows == 0)
			throw new NotFoundException("Trade not found.", "trade_not_found");
	}
}
