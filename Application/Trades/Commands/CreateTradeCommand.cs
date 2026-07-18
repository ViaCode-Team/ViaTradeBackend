using Application.Common.Interfaces;
using Application.TradeCodes.Interfaces;
using Application.Trades.Interfaces;
using Application.Trades.Models;
using Domain.Trades.Entities;
using MediatR;

namespace Application.Trades.Commands;

public record CreateTradeCommand(int UserId, TradeCreateDto Request) : ICommand<Trade>;

public class CreateTradeCommandHandler(
	ITradeRepository tradeRepository,
	ITradeCodeRepository tradeCodeRepository,
	ITradeTypeRepository tradeTypeRepository)
	: IRequestHandler<CreateTradeCommand, Trade>
{
	private readonly ITradeRepository _tradeRepository = tradeRepository;
	private readonly ITradeCodeRepository _tradeCodeRepository = tradeCodeRepository;
	private readonly ITradeTypeRepository _tradeTypeRepository = tradeTypeRepository;

	public async Task<Trade> Handle(CreateTradeCommand request, CancellationToken cancellationToken)
	{
		bool isTradeCodeExist = await _tradeCodeRepository.ExistsAsync(c => c.Id == request.Request.TradeCodeId, cancellationToken);
		if (!isTradeCodeExist)
			throw new KeyNotFoundException($"TradeCode {request.Request.TradeCodeId} not found");

		bool isTradeTypeExist = await _tradeTypeRepository.ExistsAsync(t => t.Id == request.Request.TradeTypeId, cancellationToken);
		if (!isTradeTypeExist)
			throw new ArgumentException($"TradeType {request.Request.TradeTypeId} not found");

		var req = request.Request;

		var trade = new Trade(
			req.DateOpen,
			req.TradeOpen,
			req.Count,
			(decimal)req.TradeOpen * req.Count,
			req.TradeTypeId,
			req.TradeCodeId,
			request.UserId,
			req.TradeSignal
		);
		trade.Update(
			req.DateOpen,
			req.DateClose,
			req.TradeOpen,
			req.TradeClose,
			req.Count,
			req.TradeTypeId,
			req.TradeCodeId,
			req.TradeSignal
		);

		await _tradeRepository.AddAsync(trade, cancellationToken);

		return trade;
	}
}
