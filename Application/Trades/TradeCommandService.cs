using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.TradeCodes.Interfaces;
using Application.Trades.Interfaces;
using Application.Trades.Models;
using Domain.Entities;
using Domain.Statistics.Services;

namespace Application.Trades;

public class TradeCommandService(
	ITradeRepository tradeRepository,
	ITradeCodeRepository tradeCodeRepository,
	IUnitOfWork uow
) : ITradeCommandService
{
	public async Task<TradeDto> CreateAsync(int userId, TradeInputDto request, CancellationToken ct)
	{
		var trade = new Trade
		{
			DateOpen = request.DateOpen,
			DateClose = request.DateClose,
			TradeOpen = request.TradeOpen,
			TradeClose = request.TradeClose,
			Count = request.Count,
			TradeTypeId = request.TradeTypeId,
			TradeCodeId = request.TradeCodeId,
			UserId = userId,
			TradeSignal = request.TradeSignal,
			Price = (decimal)request.TradeOpen * request.Count,
		};

		await tradeRepository.AddAsync(trade, ct);
		await uow.SaveChangesAsync(ct);
		var tradeCode = await tradeCodeRepository.FindByIdAsync(trade.TradeCodeId, ct);
		if (tradeCode == null)
			throw new DataIntegrityException($"Trade code was not found after trade creation. TradeCodeId={trade.TradeCodeId}.");

		return new TradeDto(
			trade.Id,
			trade.DateOpen,
			trade.DateClose,
			trade.TradeOpen,
			trade.TradeClose,
			TradeStatisticsCalcService.CalculateNetIncome(trade.TradeOpen, trade.TradeClose, trade.TradeSignal),
			trade.Count,
			trade.Price,
			trade.TradeSignal,
			trade.TradeTypeId,
			new TradeCodeSummaryDto(tradeCode.Id, tradeCode.ExchangeId, tradeCode.Description),
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
		var price = (decimal)request.TradeOpen * request.Count;

		var affectedRows = await tradeRepository.ExecuteUpdateAsync(userId, id, request, price, ct);
		if (affectedRows != 0)
			return;

		var tradeIsExist = await tradeRepository.ExistsAsync(t => t.Id == id && t.UserId == userId, ct);
		if (!tradeIsExist)
			throw new NotFoundException("Trade not found.", "trade_not_found");
	}
}
