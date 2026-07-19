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
	IUnitOfWork uow) : ITradeCommandService
{
	public async Task<Trade> CreateAsync(int userId, TradeInput request, CancellationToken ct)
	{
		bool isTradeCodeExist = await tradeCodeRepository.ExistsAsync(c => c.Id == request.TradeCodeId, ct);
		if (!isTradeCodeExist)
			throw new KeyNotFoundException($"TradeCode {request.TradeCodeId} not found");

		bool isTradeTypeExist = await tradeTypeRepository.ExistsAsync(t => t.Id == request.TradeTypeId, ct);
		if (!isTradeTypeExist)
			throw new ArgumentException($"TradeType {request.TradeTypeId} not found");

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
			NetIncome = TradeStatisticsCalcService.CalculateNetIncome(request.TradeOpen, request.TradeClose, request.TradeSignal)
		};

		await tradeRepository.AddAsync(trade, ct);
		await uow.SaveChangesAsync(ct);

		return trade;
	}

	public async Task DeleteAsync(int id, int userId, CancellationToken ct)
	{
		var affectedRows = await tradeRepository.ExecuteDeleteAsync(t => t.Id == id && t.UserId == userId, ct);
		if (affectedRows == 0)
		{
			bool exists = await tradeRepository.ExistsAsync(t => t.Id == id, ct);
			if (exists)
				throw new UnauthorizedAccessException();

			throw new KeyNotFoundException();
		}
	}

	public async Task UpdateAsync(int id, int userId, TradeInput request, CancellationToken ct)
	{
		bool isTradeCodeExist = await tradeCodeRepository.ExistsAsync(c => c.Id == request.TradeCodeId, ct);
		if (!isTradeCodeExist)
			throw new KeyNotFoundException();

		bool isTradeTypeExist = await tradeTypeRepository.ExistsAsync(t => t.Id == request.TradeTypeId, ct);
		if (!isTradeTypeExist)
			throw new ArgumentException($"TradeType {request.TradeTypeId} not found");

		var netIncome = TradeStatisticsCalcService.CalculateNetIncome(request.TradeOpen, request.TradeClose, request.TradeSignal);
		var price = (decimal)request.TradeOpen * request.Count;

		var affectedRows = await tradeRepository.UpdateAsync(id, userId, request, netIncome, price, ct);
		if (affectedRows == 0)
		{
			bool exists = await tradeRepository.ExistsAsync(t => t.Id == id, ct);
			if (exists)
				throw new UnauthorizedAccessException();

			throw new KeyNotFoundException();
		}
	}
}
