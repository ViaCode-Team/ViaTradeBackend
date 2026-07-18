using Application.Contracts.Dto.Strategy;
using Application.Interfaces.Repositories.Database;
using Domain.Entities.DataBase;
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
		var paged = await _dbSet
			.Where(e => e.UserId == userId)
			.OrderBy(e => e.Id)
			.ToPagedAsync(paginationRequest, cancellationToken);

		return paged.Map(e => new UserStrategyTradeCodeDto
		{
			UserId = e.UserId,
			TradeCodeId = e.TradeCodeId,
			StrategyId = e.StrategyId
		});
	}
}
