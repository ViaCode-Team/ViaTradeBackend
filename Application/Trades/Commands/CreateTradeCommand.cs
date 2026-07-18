using Application.Contracts.Dto.Requests.Trade;
using Application.Interfaces.Repositories.Database;
using Domain.Trades.Entities;
using Domain.Trades.Enums;
using MediatR;

namespace Application.Trades.Commands;

public record CreateTradeCommand(int UserId, TradeCreateDto Request) : IRequest<Trade>;

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
		
		var netIncome = Trade.CalculateNetIncome(req.TradeOpen, req.TradeClose, req.TradeSignal);
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
		await _tradeRepository.SaveChangesAsync(cancellationToken);

		return trade;
	}
}
