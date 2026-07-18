using Application.Common.Models.Pagination;
using Application.Strategies.Interfaces;
using Domain.Strategies.Entities;

namespace Infrastructure.Repositories.DataBase;

public class UserStrategyTradeCodeEfRepository(AppDbContext context) :
	GenericEfRepository<UserStrategyTradeCode>(context),
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
