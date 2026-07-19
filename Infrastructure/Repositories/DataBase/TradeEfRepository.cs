using Application.Common.Interfaces;
using Application.Common.Queries;
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
	public async Task<GlobalStatisticReadModel> GetGlobalStatisticAsync(int userId, CancellationToken ct)
	{
		var baseQuery = _dbSet.Where(t => t.UserId == userId && t.NetIncome.HasValue);

		var totalTrades = await baseQuery.CountAsync(ct);

		if (totalTrades == 0)
		{
			return new GlobalStatisticReadModel
			{
				TradeStatisticReadModel = new TradeStatisticReadModel { TotalTrades = 0, WinTrades = 0, LoseTrades = 0 },
				IncomeStatistic = new IncomeTradeStatisticReadModel { TotalIncome = 0, AverageIncome = 0 },
				WinrateStatistic = new WinrateTradeStatisticReadModel { TotalWinrate = 0, ProfitFactor = 0 }
			};
		}

		var winTrades = await baseQuery.CountAsync(t => t.NetIncome > 0, ct);
		var loseTrades = await baseQuery.CountAsync(t => t.NetIncome < 0, ct);

		var totalAbsoluteIncome = await baseQuery.SumAsync(TradeStatisticsCalcService.AbsoluteIncomeExpression, ct);
		var totalProfit = await baseQuery.Where(t => t.NetIncome > 0).SumAsync(TradeStatisticsCalcService.AbsoluteIncomeAbsExpression, ct);
		var totalLoss = await baseQuery.Where(t => t.NetIncome < 0).SumAsync(TradeStatisticsCalcService.AbsoluteIncomeAbsExpression, ct);

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

	public async Task<PageResult<Trade>> GetByUserPagedAsync(int userId, PageOptions page, CancellationToken ct)
	{
		return await FindPagedAsync(t => t.UserId == userId, page, ct);
	}

	public async Task<PageResult<Trade>> GetByUserAndTradeCodePagedAsync(int userId, int tradeCodeId, PageOptions page, CancellationToken ct)
	{
		return await FindPagedAsync(t => t.UserId == userId && t.TradeCodeId == tradeCodeId, page, ct);
	}

	public async Task<PageResult<Trade>> GetPagedFilteredAsync(IQuerySpecification<Trade> spec, PageOptions page, CancellationToken ct)
	{
		return await GetPagedAsync(spec, page, ct);
	}

	public async Task<int> UpdateAsync(int id, int userId, TradeInput request, double? netIncome, decimal price, CancellationToken ct)
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
				ct);
	}
}
