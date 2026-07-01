using Domain.Entities.DataBase;
using Domain.Models.Dto.Strategy;

namespace Application.Interfaces.Repositories.Database
{
    public interface IUserTradeStrategyRepository : IRepository<UserTradeStrategy, UserTradeStrategyDto>
    {
        Task<IEnumerable<UserTradeStrategyDto>> GetByUser(int userId, CancellationToken cancellationToken);
        Task<int> CountByUserAsync(int userId, CancellationToken cancellationToken);
        Task<Dictionary<string, List<string>>> GetUserPreferencesAsync(int userId, CancellationToken cancellationToken);
    }
}
