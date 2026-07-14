using Domain.Entities.DataBase;
using Domain.Models.Dto.Strategy;
using Domain.Models.Pagination;

namespace Application.Interfaces.Repositories.Database
{
    public interface IUserStrategyTradeCodeRepository : IRepository<UserStrategyTradeCode, UserStrategyTradeCodeDto>
    {
        Task<PagedResult<UserStrategyTradeCodeDto>> GetPagedAsync(int userId, PaginationRequest paginationRequest, CancellationToken cancellationToken);
    }
}
