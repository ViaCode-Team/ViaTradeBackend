using Application.TradeCodes.Interfaces;
using Application.Trades.Interfaces;
using Application.Trades.Models;
using Domain.Trades.Entities;
using MediatR;

namespace Application.Trades.Commands;

public record UpdateTradeCommand(int Id, int UserId, TradeCreateDto Request) : IRequest<Trade>;

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

		var req = request.Request;
		var netIncome = Trade.CalculateNetIncome(req.TradeOpen, req.TradeClose, req.TradeSignal);
		var price = (decimal)req.TradeOpen * req.Count;

		var affectedRows = await _tradeRepository.UpdateUserTradeAsync(request.Id, request.UserId, req, netIncome, price, cancellationToken);

		if (affectedRows == 0)
			throw new KeyNotFoundException();

		return (await _tradeRepository.GetByIdAsync(request.Id, cancellationToken))!;
	}
}
