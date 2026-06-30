using Application.Interfaces.Database;
using Domain.Entities.DataBase;
using Domain.Models.Dto;
using Domain.Models.Dto.Statistic;
using Domain.Models.Dto.Trade;
using Domain.Services;
using Infrastructure.Repositories.DataBase;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.DataBase
{
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

        public async Task<IEnumerable<Trade>> GetByUserAsync(int userId, CancellationToken cancellationToken)
        {
            return await _dbSet
                .Include(t => t.TradeType)
                .Include(t => t.TradeCode)
                .Where(t => t.UserId == userId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Trade>> GetByUserAndTradeCodeAsync(int userId, int tradeCodeId, CancellationToken cancellationToken)
        {
            return await _dbSet
                .Include(t => t.TradeType)
                .Include(t => t.TradeCode)
                .Where(t => t.UserId == userId && t.TradeCodeId == tradeCodeId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Trade>> GetByUserAndDateRangeAsync(int userId, DateTime? from, DateTime? to, TradeSignal? tradeSignal, CancellationToken cancellationToken)
        {
            var query = _dbSet
                .Include(t => t.TradeType)
                .Include(t => t.TradeCode)
                .Where(t => t.UserId == userId);

            if (from.HasValue)
                query = query.Where(t => t.DateOpen >= from.Value);

            if (to.HasValue)
                query = query.Where(t => t.DateOpen <= to.Value.Date.AddDays(1).AddTicks(-1));

            if (tradeSignal.HasValue)
            {
                query = query.Where(t => t.TradeSignal == tradeSignal);
            }

            return await query.ToListAsync(cancellationToken);
        }
    }
}
