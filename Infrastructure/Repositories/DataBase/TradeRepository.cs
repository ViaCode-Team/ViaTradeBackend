using Application.Interfaces.Database;
using Domain.Entities.DataBase;
using Domain.Models.Dto;
using Domain.Models.Dto.Trade;
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

            // Calculate absolute income using math formula based on TradeSignal enum values (BUY=1, SELL=-1)
            // Formula: (Close - Open) * Count * SignalDirection
            var totalAbsoluteIncome = await tradeStatistic.SumAsync(
                t => ((t.TradeClose ?? 0) - t.TradeOpen) * t.Count * (int)t.TradeSignal,
                cancellationToken);

            var incomeStatistic = new IncomeTradeStatistic
            {
                TotalIncome = Math.Round((decimal)totalAbsoluteIncome, 2),
                AverageIncome = resultTrade.TotalTrades > 0
                    ? Math.Round((decimal)totalAbsoluteIncome / resultTrade.TotalTrades, 2)
                    : 0m,
            };

            var totalProfit = await tradeStatistic
                .Where(t => t.NetIncome > 0)
                .SumAsync(t => Math.Abs(((t.TradeClose ?? 0) - t.TradeOpen) * t.Count * (int)t.TradeSignal), cancellationToken);

            var totalLoss = await tradeStatistic
                .Where(t => t.NetIncome < 0)
                .SumAsync(t => Math.Abs(((t.TradeClose ?? 0) - t.TradeOpen) * t.Count * (int)t.TradeSignal), cancellationToken);

            var winrateStatistic = new WinrateTradeStatistic
            {
                TotalWinrate = resultTrade.TotalTrades > 0
                    ? (float)Math.Round((double)resultTrade.WinTrades / resultTrade.TotalTrades * 100, 2)
                    : 0f,
                ProfitFactor = CalculateProfitFactor(totalProfit, totalLoss)
            };

            return new GlobalStatistic
            {
                TradeStatistic = resultTrade,
                IncomeStatistic = incomeStatistic,
                WinrateStatistic = winrateStatistic
            };
        }

        private static float CalculateProfitFactor(double totalProfit, double totalLoss)
        {
            if (totalLoss > 0)
                return (float)Math.Round(totalProfit / totalLoss, 3);

            if (totalProfit > 0)
                return float.PositiveInfinity;

            return 0f;
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
