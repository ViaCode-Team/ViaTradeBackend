using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.TradeCodes.Interfaces;
using Application.Trades.Interfaces;
using Application.Trades.Models;
using Domain.Statistics.Services;
using Domain.Trades.Entities;

namespace Application.Trades;

public class TradeCommandService(
	ITradeRepository tradeRepository,
	ITradeCodeRepository tradeCodeRepository,
	ITradeTypeRepository tradeTypeRepository,
	IUnitOfWork uow
) : ITradeCommandService
{
	public async Task<Trade> CreateAsync(int userId, TradeInput request, CancellationToken ct)
	{
		bool tradeCodeExists = await tradeCodeRepository.ExistsAsync(c => c.Id == request.TradeCodeId, ct);
		if (!tradeCodeExists)
			throw new NotFoundException($"Trade code {request.TradeCodeId} not found.", "trade_code_not_found");

		bool tradeTypeExists = await tradeTypeRepository.ExistsAsync(t => t.Id == request.TradeTypeId, ct);
		if (!tradeTypeExists)
			throw new NotFoundException($"Trade type {request.TradeTypeId} not found.", "trade_type_not_found");

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
			NetIncome = TradeStatisticsCalcService.CalculateNetIncome(
				request.TradeOpen,
				request.TradeClose,
				request.TradeSignal
			),
		};

		await tradeRepository.AddAsync(trade, ct);
		await uow.SaveChangesAsync(ct);

		return trade;
	}

	public async Task DeleteAsync(int id, int userId, CancellationToken ct)
	{
		var affectedRows = await tradeRepository.ExecuteDeleteAsync(t => t.Id == id && t.UserId == userId, ct);
		if (affectedRows == 0)
			throw new NotFoundException("Trade not found.", "trade_not_found");
	}

	public async Task UpdateAsync(int id, int userId, TradeInput request, CancellationToken ct)
	{
		bool tradeCodeExists = await tradeCodeRepository.ExistsAsync(c => c.Id == request.TradeCodeId, ct);
		if (!tradeCodeExists)
			throw new NotFoundException($"Trade code {request.TradeCodeId} not found.", "trade_code_not_found");

		bool tradeTypeExists = await tradeTypeRepository.ExistsAsync(t => t.Id == request.TradeTypeId, ct);
		if (!tradeTypeExists)
			throw new NotFoundException($"Trade type {request.TradeTypeId} not found.", "trade_type_not_found");

		var netIncome = TradeStatisticsCalcService.CalculateNetIncome(
			request.TradeOpen,
			request.TradeClose,
			request.TradeSignal
		);
		var price = (decimal)request.TradeOpen * request.Count;

		var affectedRows = await tradeRepository.UpdateAsync(id, userId, request, netIncome, price, ct);
		if (affectedRows == 0)
			throw new NotFoundException("Trade not found.", "trade_not_found");
	}
}
