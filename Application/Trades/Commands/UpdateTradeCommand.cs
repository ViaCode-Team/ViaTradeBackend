using Application.Common.Interfaces;
using Application.TradeCodes.Interfaces;
using Application.Trades.Interfaces;
using Application.Trades.Models;
using Domain.Trades.Entities;
using MediatR;

namespace Application.Trades.Commands;

public record UpdateTradeCommand(int Id, int UserId, TradeCreateDto Request) : ICommand<Trade>;

public class UpdateTradeCommandHandler(
	ITradeRepository tradeRepository,
	ITradeCodeRepository tradeCodeRepository,
	ITradeTypeRepository tradeTypeRepository)
	: IRequestHandler<UpdateTradeCommand, Trade>
{
	private readonly ITradeRepository _tradeRepository = tradeRepository;
	private readonly ITradeCodeRepository _tradeCodeRepository = tradeCodeRepository;
	private readonly ITradeTypeRepository _tradeTypeRepository = tradeTypeRepository;

	public async Task<Trade> Handle(UpdateTradeCommand request, CancellationToken cancellationToken)
	{
		bool isTradeCodeExist = await _tradeCodeRepository.ExistsAsync(c => c.Id == request.Request.TradeCodeId, cancellationToken);
		if (!isTradeCodeExist)
			throw new KeyNotFoundException();

		bool isTradeTypeExist = await _tradeTypeRepository.ExistsAsync(t => t.Id == request.Request.TradeTypeId, cancellationToken);
		if (!isTradeTypeExist)
			throw new ArgumentException($"TradeType {request.Request.TradeTypeId} not found");

		var trade = await _tradeRepository.GetByIdAsync(request.Id, cancellationToken);
		if (trade == null || trade.UserId != request.UserId)
			throw new KeyNotFoundException();

		var req = request.Request;

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

		_tradeRepository.Update(trade);

		return trade;
	}
}
