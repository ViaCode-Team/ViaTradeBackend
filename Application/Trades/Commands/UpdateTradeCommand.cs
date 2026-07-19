using Application.Common.Interfaces;
using Application.TradeCodes.Interfaces;
using Application.Trades.Interfaces;
using Application.Trades.Models;
using Domain.Statistics.Services;
using MediatR;

namespace Application.Trades.Commands;

public record UpdateTradeCommand(int Id, int UserId, TradeCreateDto Request) : ICommand;

public class UpdateTradeCommandHandler(
	ITradeRepository tradeRepository, ITradeCodeRepository tradeCodeRepository, ITradeTypeRepository tradeTypeRepository)
	: IRequestHandler<UpdateTradeCommand>
{
	public async Task Handle(UpdateTradeCommand request, CancellationToken cancellationToken)
	{
		bool isTradeCodeExist = await tradeCodeRepository.ExistsAsync(c => c.Id == request.Request.TradeCodeId, cancellationToken);
		if (!isTradeCodeExist)
			throw new KeyNotFoundException();

		bool isTradeTypeExist = await tradeTypeRepository.ExistsAsync(t => t.Id == request.Request.TradeTypeId, cancellationToken);
		if (!isTradeTypeExist)
			throw new ArgumentException($"TradeType {request.Request.TradeTypeId} not found");

		var req = request.Request;
		var netIncome = TradeStatisticsCalcService.CalculateNetIncome(req.TradeOpen, req.TradeClose, req.TradeSignal);
		var price = (decimal)req.TradeOpen * req.Count;

		var affectedRows = await tradeRepository.UpdateAsync(request.Id, request.UserId, req, netIncome, price, cancellationToken);
		if (affectedRows == 0)
		{
			bool exists = await tradeRepository.ExistsAsync(t => t.Id == request.Id, cancellationToken);
			if (exists)
				throw new UnauthorizedAccessException();

			throw new KeyNotFoundException();
		}
	}
}
