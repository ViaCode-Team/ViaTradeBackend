using Application.Interfaces.Database;
using Domain.Entities.DataBase;
using Domain.Models.Dto.Strategy;
using Infrastructure.Repositoryes.DataBase;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.DataBase
{
    public class UserTradeStrategyRepository(AppDbContext context) : GenericRepository<UserTradeStrategy, UserTradeStrategyDto>(context), 
        IUserTradeStrategyRepository
    {
        public async Task<IEnumerable<UserTradeStrategyDto>> GetByUser(int userId, CancellationToken cancellationToken)
        {
            return await _context.UserTradeStrategies
                .Where(e => e.UserId == userId)
                .Select(e => new UserTradeStrategyDto {
                    Id = e.Id, 
                    UserId = e.UserId,
                    TradeStrategyId = e.TradeStrategyId })
                .ToListAsync(cancellationToken);
        }

        public async Task<Dictionary<string, List<string>>> GetUserPreferencesAsync(
            int userId,
            CancellationToken ct)
        {
            // 1. Получаем разрешённые пользователю StrategyId из UserTradeStrategies
            var allowedStrategyIds = await _context.UserTradeStrategies
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .Select(x => x.TradeStrategyId)
                .Distinct()
                .ToListAsync(ct);

            if (!allowedStrategyIds.Any())
                return new Dictionary<string, List<string>>();

            // 2. Получаем связки Strategy-TradeCode, но ТОЛЬКО для разрешённых стратегий
            var links = await _context.UserStrategyTradeCodes
                .AsNoTracking()
                .Where(x => x.UserId == userId && allowedStrategyIds.Contains(x.StrategyId))
                .Select(x => new { x.StrategyId, x.TradeCodeId })
                .ToListAsync(ct);

            if (!links.Any())
                return new Dictionary<string, List<string>>();

            // 3. Resolve strategy names (подгружаем имена стратегий)
            var strategyMap = await _context.TradeStrategies
                .AsNoTracking()
                .Where(s => allowedStrategyIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id, s => s.Name, ct);

            // 4. Resolve trade codes (подгружаем коды биржи)
            var codeIds = links.Select(l => l.TradeCodeId).Distinct();
            var codeMap = await _context.TradeCodes
                .AsNoTracking()
                .Where(c => codeIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c.ExchangeId, ct);

            // 5. Group by strategy in memory
            var result = new Dictionary<string, List<string>>();
            foreach (var link in links)
            {
                if (!strategyMap.TryGetValue(link.StrategyId, out var strategyName)) continue;
                if (!codeMap.TryGetValue(link.TradeCodeId, out var tradeCode)) continue;

                if (!result.ContainsKey(strategyName))
                    result[strategyName] = new List<string>();

                result[strategyName].Add(tradeCode);
            }

            return result;
        }
    }
}
