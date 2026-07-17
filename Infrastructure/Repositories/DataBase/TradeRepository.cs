using Application.Interfaces.Repositories.Database;
using Domain.Entities.DataBase;
using Domain.Interfaces;
using Domain.Models.Dto.Statistic;
using Domain.Models.Dto.Trade;
using Domain.Models.Pagination;
using Domain.Services;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.DataBase;

public class TradeRepository(AppDbContext context)
	: GenericRepository<Trade, TradeDto>(context), ITradeRepository
{
	public async Task<GlobalStatistic> GetGlobalStatisticAsync(int userId, CancellationToken cancellationToken)
	{
		var baseQuery = _dbSet.Where(t => t.UserId == userId && t.NetIncome.HasValue);

		var totalTrades = await baseQuery.CountAsync(cancellationToken);

		if (totalTrades == 0)
		{
			return new GlobalStatistic
			{
				TradeStatistic = new TradeStatistic { TotalTrades = 0, WinTrades = 0, LoseTrades = 0 },
				IncomeStatistic = new IncomeTradeStatistic { TotalIncome = 0, AverageIncome = 0 },
				WinrateStatistic = new WinrateTradeStatistic { TotalWinrate = 0, ProfitFactor = 0 }
			};
		}

		var winTrades = await baseQuery.CountAsync(t => t.NetIncome > 0, cancellationToken);
		var loseTrades = await baseQuery.CountAsync(t => t.NetIncome < 0, cancellationToken);

		var totalAbsoluteIncome = await baseQuery.SumAsync(TradeStatisticsCalcService.AbsoluteIncomeExpression, cancellationToken);
		var totalProfit = await baseQuery.Where(t => t.NetIncome > 0).SumAsync(TradeStatisticsCalcService.AbsoluteIncomeAbsExpression, cancellationToken);
		var totalLoss = await baseQuery.Where(t => t.NetIncome < 0).SumAsync(TradeStatisticsCalcService.AbsoluteIncomeAbsExpression, cancellationToken);

		var resultTrade = new TradeStatistic
		{
			TotalTrades = totalTrades,
			WinTrades = winTrades,
			LoseTrades = loseTrades,
		};

		var incomeStatistic = new IncomeTradeStatistic
		{
			TotalIncome = Math.Round((decimal)totalAbsoluteIncome, 2),
			AverageIncome = TradeStatisticsCalcService.CalculateAverageIncome((decimal)totalAbsoluteIncome, totalTrades),
		};

		var winrateStatistic = new WinrateTradeStatistic
		{
			TotalWinrate = TradeStatisticsCalcService.CalculateWinrate(winTrades, totalTrades),
			ProfitFactor = TradeStatisticsCalcService.CalculateProfitFactor(totalProfit, totalLoss)
		};

		return new GlobalStatistic
		{
			TradeStatistic = resultTrade,
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
}
