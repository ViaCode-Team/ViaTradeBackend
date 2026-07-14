using Application.Interfaces.Repositories.Database;
using Domain.Entities.DataBase;
using Domain.Models.Dto.Strategy;
using Domain.Models.Pagination;
using Infrastructure.Extensions;

namespace Infrastructure.Repositories.DataBase;

public class UserStrategyTradeCodeRepository(AppDbContext context) :
	GenericRepository<UserStrategyTradeCode, UserStrategyTradeCodeDto>(context),
	IUserStrategyTradeCodeRepository
{
	public async Task<PagedResult<UserStrategyTradeCodeDto>> GetPagedAsync(
		int userId,
		PaginationRequest paginationRequest,
		CancellationToken cancellationToken)
	{
		return await _dbSet
			.Where(e => e.UserId == userId)
			.Select(e => new UserStrategyTradeCodeDto
			{
				UserId = e.UserId,
				TradeCodeId = e.TradeCodeId,
				StrategyId = e.StrategyId
			})
			.ToPagedResultAsync(paginationRequest, cancellationToken);
	}
}
