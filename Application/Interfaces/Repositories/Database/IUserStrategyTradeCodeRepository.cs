using Domain.Entities.DataBase;
using Domain.Models.Dto.Strategy;

namespace Application.Interfaces.Repositories.Database
{
    public interface IUserStrategyTradeCodeRepository : IRepository<UserStrategyTradeCode, UserStrategyTradeCodeDto>
    {
        Task<IEnumerable<UserStrategyTradeCodeDto>> GetAllAsync(int userId, CancellationToken cancellationToken);
    }
}
