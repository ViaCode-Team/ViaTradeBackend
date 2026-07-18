using Domain.Strategies.Entities;
using Application.Interfaces.Repositories.Database;
using Domain.Entities.DataBase;
using Domain.Models.Pagination;
using Infrastructure.Extensions;

namespace Infrastructure.Repositories.DataBase;

public class UserStrategyTradeCodeRepository(AppDbContext context) :
	GenericRepository<UserStrategyTradeCode>(context),
	IUserStrategyTradeCodeRepository
{
	public async Task<PagedResult<UserStrategyTradeCode>> GetPagedAsync(
		int userId,
		PaginationRequest paginationRequest,
		CancellationToken cancellationToken)
	{
		return await FindPagedAsync(e => e.UserId == userId, paginationRequest, cancellationToken);
	}
}
