using Application.Interfaces;
using Application.Interfaces.Database;
using Domain.Entities.DataBase;
using Infrastructure.Repositories.DataBase;
using ViaTradeBackend.Models.Trade;

namespace Infrastructure.Services
{
    public class StatisticService(
    ITradeRepository tradeRepository,
    TradeCodeRepository tradeCodeRepository,
    TradeTypeRepository tradeTypeRepository,
    UserService userService) : IStatisticService
    {
        private readonly ITradeRepository _tradeRepository = tradeRepository;
        private readonly TradeCodeRepository _tradeCodeRepository = tradeCodeRepository;
        private readonly TradeTypeRepository _tradeTypeRepository = tradeTypeRepository;
        private readonly UserService _userService = userService;

        public async Task<IEnumerable<Trade>> GetByUserAsync(int userId, DateTime? startDate, DateTime? endDate,
            TradeSignal? tradeSignal, CancellationToken cancellationToken)
        {
            await _userService.EnsureUserAsync(userId, cancellationToken);
            return await _tradeRepository.GetByUserAndDateRangeAsync(userId, startDate, endDate, tradeSignal, cancellationToken);
        }

        public async Task<Trade> GetTradeByIdAsync(int id, int userId, CancellationToken cancellationToken)
        {
            await _userService.EnsureUserAsync(userId, cancellationToken);
            var trade = await _tradeRepository.GetByIdAsync(id, cancellationToken);
            if (trade == null || trade.UserId != userId)
                throw new KeyNotFoundException();
            return trade;
        }

        public async Task<Trade> CreateTradeAsync(TradeRequest request, int userId, CancellationToken cancellationToken)
        {
            await _userService.EnsureUserAsync(userId, cancellationToken);

            var tradeCode = await _tradeCodeRepository.GetByIdAsync(request.TradeCodeId, cancellationToken)
                ?? throw new KeyNotFoundException();
            var tradeType = await _tradeTypeRepository.GetByIdAsync(request.TradeTypeId, cancellationToken)
                ?? throw new ArgumentException($"TradeType {request.TradeTypeId} not found");

            var trade = new Trade
            {
                DateOpen = request.DateOpen,
                DateClose = request.DateClose,
                TradeOpen = request.TradeOpen,
                TradeClose = request.TradeClose,
                NetIncome = CalculateNetIncome(request.TradeOpen, request.TradeClose, request.TradeSignal),
                Count = request.Count,
                Price = (decimal)request.TradeOpen * request.Count,
                TradeSignal = request.TradeSignal,
                TradeTypeId = request.TradeTypeId,
                TradeCodeId = request.TradeCodeId,
                UserId = userId
            };

            await _tradeRepository.AddAsync(trade, cancellationToken);
            await _tradeRepository.SaveChangesAsync(cancellationToken);
            return trade;
        }

        public async Task<Trade> UpdateTradeAsync(int id, TradeRequest request, int userId, CancellationToken cancellationToken)
        {
            await _userService.EnsureUserAsync(userId, cancellationToken);

            var trade = await _tradeRepository.GetByIdAsync(id, cancellationToken);
            if (trade == null || trade.UserId != userId)
                throw new KeyNotFoundException();

            var tradeCode = await _tradeCodeRepository.GetByIdAsync(request.TradeCodeId, cancellationToken)
                ?? throw new KeyNotFoundException();
            var tradeType = await _tradeTypeRepository.GetByIdAsync(request.TradeTypeId, cancellationToken)
                ?? throw new ArgumentException($"TradeType {request.TradeTypeId} not found");

            trade.DateOpen = request.DateOpen;
            trade.DateClose = request.DateClose;
            trade.TradeOpen = request.TradeOpen;
            trade.TradeClose = request.TradeClose;
            trade.NetIncome = CalculateNetIncome(request.TradeOpen, request.TradeClose, request.TradeSignal);
            trade.Count = request.Count;
            trade.TradeSignal = request.TradeSignal;
            trade.Price = (decimal)request.TradeOpen * request.Count;
            trade.TradeTypeId = request.TradeTypeId;
            trade.TradeCodeId = request.TradeCodeId;

            _tradeRepository.Update(trade);
            await _tradeRepository.SaveChangesAsync(cancellationToken);
            return trade;
        }

        public async Task DeleteTradeAsync(int id, int userId, CancellationToken cancellationToken)
        {
            await _userService.EnsureUserAsync(userId, cancellationToken);

            var trade = await _tradeRepository.GetByIdAsync(id, cancellationToken);
            if (trade == null) throw new KeyNotFoundException();

            if (trade.UserId != userId) throw new UnauthorizedAccessException();

            _tradeRepository.Remove(trade);
            await _tradeRepository.SaveChangesAsync(cancellationToken);
        }

        private static double? CalculateNetIncome(double tradeOpen, double? tradeClose, TradeSignal tradeSignal)
        {
            if (tradeClose == null || tradeOpen == 0 || tradeSignal == TradeSignal.HOLD)
                return null;

            double basePercent = (tradeClose.Value - tradeOpen) / tradeOpen * 100;
            double adjustedPercent = tradeSignal == TradeSignal.SELL ? -basePercent : basePercent;
            return Math.Round(adjustedPercent, 2);
        }
    }

}
