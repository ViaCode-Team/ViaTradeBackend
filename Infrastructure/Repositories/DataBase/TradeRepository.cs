using Application.Contracts.Dto.Requests.Trade;
using Application.Contracts.Dto.Statistic;
using Application.Contracts.Dto.Trade;
using Application.Interfaces.Repositories.Database;
using Domain.Entities.DataBase;
using Domain.Interfaces;
using Domain.Models.Pagination;
using Domain.Services;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.DataBase;

public class TradeRepository(AppDbContext context)
	: GenericRepository<Trade, TradeDto>(context), ITradeRepository
{
	public async Task<GlobalStatisticDto> GetGlobalStatisticAsync(int userId, CancellationToken cancellationToken)
	{
		var baseQuery = _dbSet.Where(t => t.UserId == userId && t.NetIncome.HasValue);

		var totalTrades = await baseQuery.CountAsync(cancellationToken);

		if (totalTrades == 0)
		{
			return new GlobalStatisticDto
			{
				TradeStatisticDto = new TradeStatisticDto { TotalTrades = 0, WinTrades = 0, LoseTrades = 0 },
				IncomeStatistic = new IncomeTradeStatisticDto { TotalIncome = 0, AverageIncome = 0 },
				WinrateStatistic = new WinrateTradeStatisticDto { TotalWinrate = 0, ProfitFactor = 0 }
			};
		}

		var winTrades = await baseQuery.CountAsync(t => t.NetIncome > 0, cancellationToken);
		var loseTrades = await baseQuery.CountAsync(t => t.NetIncome < 0, cancellationToken);

		var totalAbsoluteIncome = await baseQuery.SumAsync(TradeStatisticsCalcService.AbsoluteIncomeExpression, cancellationToken);
		var totalProfit = await baseQuery.Where(t => t.NetIncome > 0).SumAsync(TradeStatisticsCalcService.AbsoluteIncomeAbsExpression, cancellationToken);
		var totalLoss = await baseQuery.Where(t => t.NetIncome < 0).SumAsync(TradeStatisticsCalcService.AbsoluteIncomeAbsExpression, cancellationToken);

		var resultTrade = new TradeStatisticDto
		{
			TotalTrades = totalTrades,
			WinTrades = winTrades,
			LoseTrades = loseTrades,
		};

		var incomeStatistic = new IncomeTradeStatisticDto
		{
			TotalIncome = Math.Round((decimal)totalAbsoluteIncome, 2),
			AverageIncome = TradeStatisticsCalcService.CalculateAverageIncome((decimal)totalAbsoluteIncome, totalTrades),
		};

		var winrateStatistic = new WinrateTradeStatisticDto
		{
			TotalWinrate = TradeStatisticsCalcService.CalculateWinrate(winTrades, totalTrades),
			ProfitFactor = TradeStatisticsCalcService.CalculateProfitFactor(totalProfit, totalLoss)
		};

		return new GlobalStatisticDto
		{
			TradeStatisticDto = resultTrade,
			IncomeStatistic = incomeStatistic,
			WinrateStatistic = winrateStatistic
		};
	}

	public async Task<PagedResult<TradeDto>> GetByUserPagedAsync(int userId, PaginationRequest paginationRequest, CancellationToken cancellationToken)
	{
		return await _dbSet
			.Where(t => t.UserId == userId)
			.Select(t => new TradeDto
			{
				Id = t.Id,
				DateOpen = t.DateOpen,
				DateClose = t.DateClose,
				TradeOpen = t.TradeOpen,
				TradeClose = t.TradeClose,
				NetIncome = t.NetIncome,
				Count = t.Count,
				Price = t.Price,
				TradeSignal = t.TradeSignal,
				TradeTypeId = t.TradeTypeId,
				TradeCodeId = t.TradeCodeId,
				UserId = t.UserId
			})
			.OrderBy(t => t.Id)
			.ToPagedAsync(paginationRequest, cancellationToken);
	}

	public async Task<PagedResult<TradeDto>> GetByUserAndTradeCodePagedAsync(int userId, int tradeCodeId, PaginationRequest paginationRequest, CancellationToken cancellationToken)
	{
		return await _dbSet
			.Where(t => t.UserId == userId && t.TradeCodeId == tradeCodeId)
			.Select(t => new TradeDto
			{
				Id = t.Id,
				DateOpen = t.DateOpen,
				DateClose = t.DateClose,
				TradeOpen = t.TradeOpen,
				TradeClose = t.TradeClose,
				NetIncome = t.NetIncome,
				Count = t.Count,
				Price = t.Price,
				TradeSignal = t.TradeSignal,
				TradeTypeId = t.TradeTypeId,
				TradeCodeId = t.TradeCodeId,
				UserId = t.UserId
			})
			.OrderBy(t => t.Id)
			.ToPagedAsync(paginationRequest, cancellationToken);
	}

	public async Task<PagedResult<TradeDto>> GetPagedFilteredAsync(IQuerySpecification<Trade> spec, PaginationRequest? paginationRequest, CancellationToken cancellationToken)
	{
		var queryable = SpecificationEvaluator.GetQuery(_dbSet.AsQueryable(), spec);

		return await queryable
			.Select(t => new TradeDto
			{
				Id = t.Id,
				DateOpen = t.DateOpen,
				DateClose = t.DateClose,
				TradeOpen = t.TradeOpen,
				TradeClose = t.TradeClose,
				NetIncome = t.NetIncome,
				Count = t.Count,
				Price = t.Price,
				TradeSignal = t.TradeSignal,
				TradeTypeId = t.TradeTypeId,
				TradeCodeId = t.TradeCodeId,
				UserId = t.UserId
			})
			.ToPagedAsync(paginationRequest, cancellationToken);
	}

	public async Task<int> UpdateUserTradeAsync(int id, int userId, TradeCreateDto request, double? netIncome, decimal price, CancellationToken cancellationToken = default)
	{
		return await _dbSet
			.Where(t => t.Id == id && t.UserId == userId)
			.ExecuteUpdateAsync(s => s
				.SetProperty(t => t.DateOpen, request.DateOpen)
				.SetProperty(t => t.DateClose, request.DateClose)
				.SetProperty(t => t.TradeOpen, request.TradeOpen)
				.SetProperty(t => t.TradeClose, request.TradeClose)
				.SetProperty(t => t.NetIncome, netIncome)
				.SetProperty(t => t.Count, request.Count)
				.SetProperty(t => t.TradeSignal, request.TradeSignal)
				.SetProperty(t => t.Price, price)
				.SetProperty(t => t.TradeTypeId, request.TradeTypeId)
				.SetProperty(t => t.TradeCodeId, request.TradeCodeId),
				cancellationToken);
	}
}
