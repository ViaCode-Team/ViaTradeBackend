using Domain.Models.Dto;

namespace Application.Interfaces.Database
{
    public interface IUserStrategyTradeCodeRepository
    {
        Task<IEnumerable<UserStrategyTradeCodeDto>> GetAllAsync(int userId, CancellationToken cancellationToken);
    }
}
