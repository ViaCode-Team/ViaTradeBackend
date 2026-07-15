using Application.Interfaces.Repositories.Database;
using Domain.Entities.DataBase;
using Domain.Models.Dto.NoteRemind;
using Domain.Models.Pagination;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.DataBase;

public class TradeRemindRepository(AppDbContext context)
	: GenericRepository<TradeRemind, TradeRemindDto>(context), ITradeRemindRepository
{
	public async Task<IEnumerable<TradeRemind>> GetActualRemind(CancellationToken cancellationToken)
	{
		return await _dbSet.Where(r => r.DateTime <= DateTime.Now).ToListAsync(cancellationToken);
	}

	public async Task<PagedResult<TradeRemind>> GetByUserPagedAsync(int userId, PaginationRequest paginationRequest, CancellationToken cancellationToken)
	{
		return await _dbSet.Where(r => r.UserId == userId).OrderBy(r => r.Id).ToPagedResultAsync(paginationRequest, cancellationToken);
	}

	public async Task<int> CountByUserAsync(int userId, CancellationToken cancellationToken)
	{
		return await _dbSet.CountAsync(r => r.UserId == userId, cancellationToken);
	}

	public async Task<PagedResult<TradeRemind>> GetByUserAndTradeCodePagedAsync(int userId, int tradeCodeId, PaginationRequest paginationRequest, CancellationToken cancellationToken)
	{
		return await _dbSet.Where(r => r.UserId == userId && r.TradeCodeId == tradeCodeId).OrderBy(r => r.Id).ToPagedResultAsync(paginationRequest, cancellationToken);
	}
}
