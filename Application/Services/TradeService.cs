using Application.Interfaces;
using Application.Interfaces.Repositories.Database;
using Application.Specifications;
using Domain.Entities.DataBase;
using Domain.Models.Dto.Statistic;
using Domain.Models.Dto.Trade;
using Domain.Models.Filters;
using Domain.Models.Pagination;
using ViaTradeBackend.Models.Trade;

namespace Application.Services;

public class TradeService(
	ITradeRepository tradeRepository,
	ITradeCodeRepository tradeCodeRepository,
	ITradeTypeRepository tradeTypeRepository) : ITradeService
{
	private readonly ITradeRepository _tradeRepository = tradeRepository;
	private readonly ITradeCodeRepository _tradeCodeRepository = tradeCodeRepository;
	private readonly ITradeTypeRepository _tradeTypeRepository = tradeTypeRepository;

	public async Task<GlobalStatistic> GetGlobalStatisticAsync(int userId, CancellationToken cancellationToken)
	{
		return await _tradeRepository.GetGlobalStatisticAsync(userId, cancellationToken);
	}

	public async Task<PagedResult<TradeDto>> GetByUserPagedAsync(int userId, TradeFilterRequest? filterRequest, PaginationRequest? paginationRequest, CancellationToken cancellationToken)
	{
		var spec = new TradeQuerySpecification(userId, filterRequest);
		return await _tradeRepository.GetPagedFilteredAsync(spec, paginationRequest, cancellationToken);
	}

	public async Task<Trade> GetTradeByIdAsync(int id, int userId, CancellationToken cancellationToken)
	{
		var trade = await _tradeRepository.GetByIdAsync(id, cancellationToken);
		if (trade == null || trade.UserId != userId)
			throw new KeyNotFoundException();

		return trade;
	}

	public async Task<Trade> CreateTradeAsync(TradeRequest request, int userId, CancellationToken cancellationToken)
	{
		bool isTradeCodeExist = await _tradeCodeRepository.ExistsAsync(c => c.Id == request.TradeCodeId, cancellationToken);
		if (!isTradeCodeExist)
			throw new KeyNotFoundException();

		bool isTradeTypeExist = await _tradeTypeRepository.ExistsAsync(t => t.Id == request.TradeTypeId, cancellationToken);
		if (!isTradeTypeExist)
			throw new ArgumentException($"TradeType {request.TradeTypeId} not found");

		var trade = new Trade
		{
			DateOpen = request.DateOpen,
			DateClose = request.DateClose,
			TradeOpen = request.TradeOpen,
			TradeClose = request.TradeClose,
			NetIncome = CalculateNetIncome(request.TradeOpen, request.TradeClose, request.TradeSignal),
			Count = request.Count,
			Price = (decimal)request.TradeOpen * request.Count,
			TradeSignal = request.TradeSignal,
			TradeTypeId = request.TradeTypeId,
			TradeCodeId = request.TradeCodeId,
			UserId = userId
		};

		await _tradeRepository.AddAsync(trade, cancellationToken);
		await _tradeRepository.SaveChangesAsync(cancellationToken);

		return trade;
	}

	public async Task<Trade> UpdateTradeAsync(int id, TradeRequest request, int userId, CancellationToken cancellationToken)
	{
		var trade = await _tradeRepository.GetByIdAsync(id, cancellationToken);
		if (trade == null || trade.UserId != userId)
			throw new KeyNotFoundException();

		bool isTradeCodeExist = await _tradeCodeRepository.ExistsAsync(c => c.Id == request.TradeCodeId, cancellationToken);
		if (!isTradeCodeExist)
			throw new KeyNotFoundException();

		bool isTradeTypeExist = await _tradeTypeRepository.ExistsAsync(t => t.Id == request.TradeTypeId, cancellationToken);
		if (!isTradeTypeExist)
			throw new ArgumentException($"TradeType {request.TradeTypeId} not found");

		trade.DateOpen = request.DateOpen;
		trade.DateClose = request.DateClose;
		trade.TradeOpen = request.TradeOpen;
		trade.TradeClose = request.TradeClose;
		trade.NetIncome = CalculateNetIncome(request.TradeOpen, request.TradeClose, request.TradeSignal);
		trade.Count = request.Count;
		trade.TradeSignal = request.TradeSignal;
		trade.Price = (decimal)request.TradeOpen * request.Count;
		trade.TradeTypeId = request.TradeTypeId;
		trade.TradeCodeId = request.TradeCodeId;

		_tradeRepository.Update(trade);
		await _tradeRepository.SaveChangesAsync(cancellationToken);
		return trade;
	}

	public async Task DeleteTradeAsync(int id, int userId, CancellationToken cancellationToken)
	{
		var trade = await _tradeRepository.GetByIdAsync(id, cancellationToken) 
			?? throw new KeyNotFoundException();
		if (trade.UserId != userId)
			throw new UnauthorizedAccessException();

		_tradeRepository.Remove(trade);
		await _tradeRepository.SaveChangesAsync(cancellationToken);
	}

	private static double? CalculateNetIncome(double tradeOpen, double? tradeClose, TradeSignal tradeSignal)
	{
		if (tradeClose == null || tradeOpen == 0 || tradeSignal == TradeSignal.HOLD)
			return null;

		var basePercent = (tradeClose.Value - tradeOpen) / tradeOpen * 100;
		double adjustedPercent = basePercent;
		if (tradeSignal == TradeSignal.SELL)
		{
			adjustedPercent = -basePercent;
		}

		return Math.Round(adjustedPercent, 2);
	}
}
