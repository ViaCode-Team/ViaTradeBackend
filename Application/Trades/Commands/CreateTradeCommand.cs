using Application.Common.Interfaces;
using Application.TradeCodes.Interfaces;
using Application.Trades.Interfaces;
using Application.Trades.Models;
using Domain.Statistics.Services;
using Domain.Trades.Entities;
using MediatR;

namespace Application.Trades.Commands;

public record CreateTradeCommand(int UserId, TradeCreateDto Request) : ITransactionalCommand<Trade>;

public class CreateTradeCommandHandler(
	ITradeRepository tradeRepository, ITradeCodeRepository tradeCodeRepository, ITradeTypeRepository tradeTypeRepository)
	: IRequestHandler<CreateTradeCommand, Trade>
{
	public async Task<Trade> Handle(CreateTradeCommand request, CancellationToken cancellationToken)
	{
		bool isTradeCodeExist = await tradeCodeRepository.ExistsAsync(c => c.Id == request.Request.TradeCodeId, cancellationToken);
		if (!isTradeCodeExist)
			throw new KeyNotFoundException($"TradeCode {request.Request.TradeCodeId} not found");

		bool isTradeTypeExist = await tradeTypeRepository.ExistsAsync(t => t.Id == request.Request.TradeTypeId, cancellationToken);
		if (!isTradeTypeExist)
			throw new ArgumentException($"TradeType {request.Request.TradeTypeId} not found");

		var req = request.Request;

		var trade = new Trade
		{
			DateOpen = req.DateOpen,
			DateClose = req.DateClose,
			TradeOpen = req.TradeOpen,
			TradeClose = req.TradeClose,
			Count = req.Count,
			TradeTypeId = req.TradeTypeId,
			TradeCodeId = req.TradeCodeId,
			UserId = request.UserId,
			TradeSignal = req.TradeSignal,
			Price = (decimal)req.TradeOpen * req.Count,
			NetIncome = TradeStatisticsCalcService.CalculateNetIncome(req.TradeOpen, req.TradeClose, req.TradeSignal)
		};

		await tradeRepository.AddAsync(trade, cancellationToken);

		return trade;
	}
}
