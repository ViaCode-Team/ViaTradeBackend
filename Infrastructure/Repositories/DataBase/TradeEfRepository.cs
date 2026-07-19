using Application.Common.Interfaces;
using Application.Common.Models.Pagination;
using Application.Statistics.Models;
using Application.Trades.Interfaces;
using Application.Trades.Models;
using Domain.Statistics.Services;
using Domain.Trades.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.DataBase;

public class TradeEfRepository(AppDbContext context)
	: GenericEfRepository<Trade>(context), ITradeRepository
{
	public async Task<GlobalStatisticReadModel> GetGlobalStatisticAsync(int userId, CancellationToken cancellationToken)
	{
		var baseQuery = _dbSet.Where(t => t.UserId == userId && t.NetIncome.HasValue);

		var totalTrades = await baseQuery.CountAsync(cancellationToken);

		if (totalTrades == 0)
		{
			return new GlobalStatisticReadModel
			{
				TradeStatisticReadModel = new TradeStatisticReadModel { TotalTrades = 0, WinTrades = 0, LoseTrades = 0 },
				IncomeStatistic = new IncomeTradeStatisticReadModel { TotalIncome = 0, AverageIncome = 0 },
				WinrateStatistic = new WinrateTradeStatisticReadModel { TotalWinrate = 0, ProfitFactor = 0 }
			};
		}

		var winTrades = await baseQuery.CountAsync(t => t.NetIncome > 0, cancellationToken);
		var loseTrades = await baseQuery.CountAsync(t => t.NetIncome < 0, cancellationToken);

		var totalAbsoluteIncome = await baseQuery.SumAsync(TradeStatisticsCalcService.AbsoluteIncomeExpression, cancellationToken);
		var totalProfit = await baseQuery.Where(t => t.NetIncome > 0).SumAsync(TradeStatisticsCalcService.AbsoluteIncomeAbsExpression, cancellationToken);
		var totalLoss = await baseQuery.Where(t => t.NetIncome < 0).SumAsync(TradeStatisticsCalcService.AbsoluteIncomeAbsExpression, cancellationToken);

		var resultTrade = new TradeStatisticReadModel
		{
			TotalTrades = totalTrades,
			WinTrades = winTrades,
			LoseTrades = loseTrades,
		};

		var incomeStatistic = new IncomeTradeStatisticReadModel
		{
			TotalIncome = Math.Round((decimal)totalAbsoluteIncome, 2),
			AverageIncome = TradeStatisticsCalcService.CalculateAverageIncome((decimal)totalAbsoluteIncome, totalTrades),
		};

		var winrateStatistic = new WinrateTradeStatisticReadModel
		{
			TotalWinrate = TradeStatisticsCalcService.CalculateWinrate(winTrades, totalTrades),
			ProfitFactor = TradeStatisticsCalcService.CalculateProfitFactor(totalProfit, totalLoss)
		};

		return new GlobalStatisticReadModel
		{
			TradeStatisticReadModel = resultTrade,
			IncomeStatistic = incomeStatistic,
			WinrateStatistic = winrateStatistic
		};
	}

	public async Task<PagedResult<Trade>> GetByUserPagedAsync(int userId, PaginationRequest paginationRequest, CancellationToken cancellationToken)
	{
		return await FindPagedAsync(t => t.UserId == userId, paginationRequest, cancellationToken);
	}

	public async Task<PagedResult<Trade>> GetByUserAndTradeCodePagedAsync(int userId, int tradeCodeId, PaginationRequest paginationRequest, CancellationToken cancellationToken)
	{
		return await FindPagedAsync(t => t.UserId == userId && t.TradeCodeId == tradeCodeId, paginationRequest, cancellationToken);
	}

	public async Task<PagedResult<Trade>> GetPagedFilteredAsync(IQuerySpecification<Trade> spec, PaginationRequest? paginationRequest, CancellationToken cancellationToken)
	{
		return await GetPagedAsync(spec, paginationRequest, cancellationToken);
	}

	public async Task<int> UpdateAsync(int id, int userId, TradeCreateDto request, double? netIncome, decimal price, CancellationToken cancellationToken = default)
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
