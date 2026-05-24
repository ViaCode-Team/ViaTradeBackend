using Domain.Entities.DataBase;
using Domain.Models.Dto.Strategy;

namespace Application.Interfaces.Database
{
    public interface IUserTradeStrategyRepository
    {
        Task<IEnumerable<UserTradeStrategyDto>> GetByUser(int userId, CancellationToken cancellationToken);
    }
}
