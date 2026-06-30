using Application.Interfaces;
using Domain.Entities.DataBase;
using Domain.Models.Dto;
using Infrastructure.Repositories.DataBase;

namespace Infrastructure.Services
{
    public class TradeRemindService(
        TradeRemindRepository tradeRemindRepository,
        TradeCodeRepository tradeCodeRepository,
        UserService userService) : ITradeRemindService
    {
        private readonly TradeRemindRepository _tradeRemindRepository = tradeRemindRepository;
        private readonly TradeCodeRepository _tradeCodeRepository = tradeCodeRepository;
        private readonly UserService _userService = userService;

        public async Task<IEnumerable<TradeRemind>> GetActualRemindAsync(CancellationToken cancellationToken)
        {
            return await _tradeRemindRepository.GetActualTradeRemind(cancellationToken);
        }

        public async Task DeleteActualRemindAsync(int remindId, CancellationToken cancellationToken)
        {
            await _tradeRemindRepository.ExecuteDeleteAsync(r => r.Id == remindId, cancellationToken);
        }

        public async Task<IEnumerable<TradeRemind>> GetAllByUserAsync(int userId, CancellationToken cancellationToken)
        {
            await _userService.EnsureUserAsync(userId, cancellationToken);
            return await _tradeRemindRepository.GetByUserAsync(userId, cancellationToken);
        }

        public async Task<IEnumerable<TradeRemind>> GetByUserAndTradeCodeAsync(int userId, int tradeCodeId, CancellationToken cancellationToken)
        {
            await _userService.EnsureUserAsync(userId, cancellationToken);
            var tradeCode = await _tradeCodeRepository.GetByIdAsync(tradeCodeId, cancellationToken);
            if (tradeCode == null) throw new KeyNotFoundException();
            return await _tradeRemindRepository.GetByUserAndTradeCodeAsync(userId, tradeCodeId, cancellationToken);
        }

        public async Task<TradeRemind> GetByIdAsync(int remindId, int userId, CancellationToken cancellationToken)
        {
            await _userService.EnsureUserAsync(userId, cancellationToken);
            var reminder = await _tradeRemindRepository.GetByIdAsync(remindId, cancellationToken);
            if (reminder == null || reminder.UserId != userId) throw new KeyNotFoundException();
            return reminder;
        }

        public async Task CreateAsync(int userId, int tradeCodeId, TradeRemindRequest request, CancellationToken cancellationToken)
        {
            await _userService.EnsureUserAsync(userId, cancellationToken);
            var tradeCode = await _tradeCodeRepository.GetByIdAsync(tradeCodeId, cancellationToken);
            if (tradeCode == null) throw new KeyNotFoundException();

            var remind = new TradeRemind
            {
                TextRemind = request.TextRemind,
                DateTime = request.DateTime,
                TradeCodeId = tradeCodeId,
                UserId = userId
            };

            await _tradeRemindRepository.AddAsync(remind, cancellationToken);
            await _tradeRemindRepository.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(int remindId, int userId, TradeRemindRequest request, CancellationToken cancellationToken)
        {
            await _userService.EnsureUserAsync(userId, cancellationToken);
            var remind = await _tradeRemindRepository.GetByIdAsync(remindId, cancellationToken);
            if (remind == null || remind.UserId != userId) throw new KeyNotFoundException();

            var tradeCode = await _tradeCodeRepository.GetByIdAsync(remind.TradeCodeId, cancellationToken);
            if (tradeCode == null) throw new KeyNotFoundException();

            remind.TextRemind = request.TextRemind;
            remind.DateTime = request.DateTime;

            _tradeRemindRepository.Update(remind);
            await _tradeRemindRepository.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(int remindId, int userId, CancellationToken cancellationToken)
        {
            await _userService.EnsureUserAsync(userId, cancellationToken);
            var remind = await _tradeRemindRepository.GetByIdAsync(remindId, cancellationToken);
            if (remind == null || remind.UserId != userId) throw new KeyNotFoundException();

            _tradeRemindRepository.Remove(remind);
            await _tradeRemindRepository.SaveChangesAsync(cancellationToken);
        }
    }
}
