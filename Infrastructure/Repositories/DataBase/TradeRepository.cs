using Application.Interfaces.Repositories.Database;
using Domain.Entities.DataBase;
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
		var tradeStatistic = _dbSet.Where(t => t.UserId == userId && t.NetIncome.HasValue);

		var resultTrade = new TradeStatistic
		{
			TotalTrades = await tradeStatistic.CountAsync(cancellationToken),
			WinTrades = await tradeStatistic.CountAsync(t => t.NetIncome > 0, cancellationToken),
			LoseTrades = await tradeStatistic.CountAsync(t => t.NetIncome < 0, cancellationToken),
		};

		var totalAbsoluteIncome = await tradeStatistic
			.Select(TradeStatisticsCalcService.AbsoluteIncomeExpression)
			.SumAsync(cancellationToken);

		var incomeStatistic = new IncomeTradeStatistic
		{
			TotalIncome = Math.Round((decimal)totalAbsoluteIncome, 2),
			AverageIncome = TradeStatisticsCalcService.CalculateAverageIncome((decimal)totalAbsoluteIncome, resultTrade.TotalTrades),
		};

		var totalProfit = await tradeStatistic
			.Where(t => t.NetIncome > 0)
			.Select(TradeStatisticsCalcService.AbsoluteIncomeAbsExpression)
			.SumAsync(cancellationToken);

		var totalLoss = await tradeStatistic
			.Where(t => t.NetIncome < 0)
			.Select(TradeStatisticsCalcService.AbsoluteIncomeAbsExpression)
			.SumAsync(cancellationToken);

		var winrateStatistic = new WinrateTradeStatistic
		{
			TotalWinrate = TradeStatisticsCalcService.CalculateWinrate(resultTrade.WinTrades, resultTrade.TotalTrades),
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

	public async Task<PagedResult<TradeDto>> GetByUserAndDateRangePagedAsync(int userId, DateTime? from, DateTime? to, TradeSignal? tradeSignal, PaginationRequest paginationRequest, CancellationToken cancellationToken)
	{
		var queryable = _dbSet
			.Where(t => t.UserId == userId);

		if (from.HasValue)
			queryable = queryable.Where(t => t.DateOpen >= from.Value);

		if (to.HasValue)
			queryable = queryable.Where(t => t.DateOpen <= to.Value.Date.AddDays(1).AddTicks(-1));

		if (tradeSignal.HasValue)
		{
			queryable = queryable.Where(t => t.TradeSignal == tradeSignal);
		}

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
			.OrderBy(t => t.Id)
			.ToPagedAsync(paginationRequest, cancellationToken);
	}
}
