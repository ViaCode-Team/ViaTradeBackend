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
		var query = _dbSet.Where(t => t.UserId == userId);
		var statsData = await TradeStatisticsCalcService.CalculateAggregate(query).FirstOrDefaultAsync(cancellationToken);

		if (statsData == null)
		{
			return new GlobalStatistic
			{
				TradeStatistic = new TradeStatistic { TotalTrades = 0, WinTrades = 0, LoseTrades = 0 },
				IncomeStatistic = new IncomeTradeStatistic { TotalIncome = 0, AverageIncome = 0 },
				WinrateStatistic = new WinrateTradeStatistic { TotalWinrate = 0, ProfitFactor = 0 }
			};
		}

		var resultTrade = new TradeStatistic
		{
			TotalTrades = statsData.TotalTrades,
			WinTrades = statsData.WinTrades,
			LoseTrades = statsData.LoseTrades,
		};

		var incomeStatistic = new IncomeTradeStatistic
		{
			TotalIncome = Math.Round((decimal)statsData.TotalAbsoluteIncome, 2),
			AverageIncome = TradeStatisticsCalcService.CalculateAverageIncome((decimal)statsData.TotalAbsoluteIncome, statsData.TotalTrades),
		};

		var winrateStatistic = new WinrateTradeStatistic
		{
			TotalWinrate = TradeStatisticsCalcService.CalculateWinrate(statsData.WinTrades, statsData.TotalTrades),
			ProfitFactor = TradeStatisticsCalcService.CalculateProfitFactor(statsData.TotalProfit, statsData.TotalLoss)
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

	public async Task<PagedResult<TradeDto>> GetPagedFilteredAsync(ISpecification<Trade> spec, PaginationRequest? paginationRequest, CancellationToken cancellationToken)
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
