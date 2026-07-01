using Application.Interfaces;
using Domain.Entities.DataBase;
using Domain.Models.Dto.Statistic;
using Domain.Models.Dto.Strategy;
using Infrastructure.Repositories.DataBase;
using ViaTradeBackend.Models.Trade;

namespace Infrastructure.Services
{
    public class StrategyService(
        TradeStrategyRepository tradeStrategyRepository,
        UserTradeStrategyRepository userTradeStrategyRepository,
        UserStrategyTradeCodeRepository userStrategyTradeCodeRepository,
        UserService userService) : IStrategyService
    {
        private readonly TradeStrategyRepository _tradeStrategyRepository = tradeStrategyRepository;
        private readonly UserTradeStrategyRepository _userTradeStrategyRepository = userTradeStrategyRepository;
        private readonly UserStrategyTradeCodeRepository _userStrategyTradeCodeRepository = userStrategyTradeCodeRepository;
        private readonly UserService _userService = userService;

        public async Task<IEnumerable<TradeStrategy>> GetAllStrategiesAsync(CancellationToken cancellationToken)
        {
            return await _tradeStrategyRepository.GetAllAsync(cancellationToken);
        }

        public async Task<StrategyStatistic> GetStrategyStatisticAsync(int userId, CancellationToken cancellationToken)
        {
            await _userService.EnsureUserAsync(userId, cancellationToken);

            var totalStrategies = await _tradeStrategyRepository.CountAsync(cancellationToken);
            var activeStrategies = await _userTradeStrategyRepository.CountByUserAsync(userId, cancellationToken);

            return new StrategyStatistic
            {
                TotalStrategies = totalStrategies,
                ActiveStrategies = activeStrategies,
                DisabledStrategies = Math.Max(totalStrategies - activeStrategies, 0)
            };
        }

        public async Task<TradeStrategy> GetStrategyByIdAsync(int strategyId, CancellationToken cancellationToken)
        {
            var strategy = await _tradeStrategyRepository.GetByIdAsync(strategyId, cancellationToken)
                ?? throw new KeyNotFoundException();
            return strategy;
        }

        public async Task<IEnumerable<UserStrategyTradeCodeDto>> GetUserStrategyCodesAsync(int userId, CancellationToken cancellationToken)
        {
            await _userService.EnsureUserAsync(userId, cancellationToken);
            return await _userStrategyTradeCodeRepository.GetAllAsync(userId, cancellationToken);
        }

        public async Task CreateUserStrategyCodeAsync(UserStrategyTradeCodeRequest request, int userId, CancellationToken cancellationToken)
        {
            await _userService.EnsureUserAsync(userId, cancellationToken);

            var existing = await _userStrategyTradeCodeRepository.FindAsync(
                e => e.UserId == userId &&
                     e.StrategyId == request.StrategyId &&
                     e.TradeCodeId == request.TradeCodeId,
                cancellationToken);

            if (existing.Any())
                throw new InvalidOperationException("User strategy code already exists");

            var newUserStrategyCode = new UserStrategyTradeCode
            {
                StrategyId = request.StrategyId,
                TradeCodeId = request.TradeCodeId,
                UserId = userId
            };

            await _userStrategyTradeCodeRepository.AddAsync(newUserStrategyCode, cancellationToken);
            await _userStrategyTradeCodeRepository.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteUserStrategyCodeAsync(int strategyId, int tradeCodeId, int userId, CancellationToken cancellationToken)
        {
            await _userService.EnsureUserAsync(userId, cancellationToken);

            var affectedRows = await _userStrategyTradeCodeRepository.ExecuteDeleteAsync(
                e => e.UserId == userId &&
                     e.StrategyId == strategyId &&
                     e.TradeCodeId == tradeCodeId,
                cancellationToken);

            if (affectedRows == 0)
                throw new KeyNotFoundException("User strategy code not found");
        }

        public async Task<IEnumerable<UserTradeStrategyDto>> GetUserStrategiesAsync(int userId, CancellationToken cancellationToken)
        {
            await _userService.EnsureUserAsync(userId, cancellationToken);
            return await _userTradeStrategyRepository.GetByUser(userId, cancellationToken);
        }

        public async Task CreateUserStrategyAsync(CreateUserStrategyRequest request, int userId, CancellationToken cancellationToken)
        {
            await _userService.EnsureUserAsync(userId, cancellationToken);

            var existing = await _userTradeStrategyRepository.FindAsync(
                e => e.UserId == userId && e.TradeStrategyId == request.StrategyId,
                cancellationToken);

            if (existing.Any())
                throw new InvalidOperationException("User strategy already exists");

            var strategyLink = new UserTradeStrategy
            {
                TradeStrategyId = request.StrategyId,
                UserId = userId
            };

            await _userTradeStrategyRepository.AddAsync(strategyLink, cancellationToken);
            await _userTradeStrategyRepository.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteUserStrategyAsync(int strategyId, int userId, CancellationToken cancellationToken)
        {
            await _userService.EnsureUserAsync(userId, cancellationToken);

            var affectedRows = await _userTradeStrategyRepository.ExecuteDeleteAsync(
                e => e.UserId == userId && e.TradeStrategyId == strategyId,
                cancellationToken);

            if (affectedRows == 0)
                throw new KeyNotFoundException("User strategy not found");
        }
    }
}
