using Application.Interfaces.Repositories.Database;
using Application.Specifications;
using Domain.Entities.DataBase;
using Domain.Models.Dto.NoteRemind;
using Domain.Models.Pagination;
using Domain.Models.Sort;
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

	public async Task<PagedResult<TradeRemindDto>> GetByUserPagedAsync(int userId, PaginationRequest paginationRequest, RemindSortRequest? sortRequest = null, CancellationToken cancellationToken = default)
	{
		var spec = new TradeRemindQuerySpecification(userId, null, sortRequest);
		var queryable = SpecificationEvaluator.GetQuery(_dbSet.AsQueryable(), spec);

		return await queryable
			.Select(r => new TradeRemindDto
			{
				Id = r.Id,
				TextRemind = r.TextRemind,
				DateTime = r.DateTime,
				TradeCodeId = r.TradeCodeId,
				UserId = r.UserId
			})
			.ToPagedAsync(paginationRequest, cancellationToken);
	}

	public async Task<int> CountByUserAsync(int userId, CancellationToken cancellationToken)
	{
		return await _dbSet.CountAsync(r => r.UserId == userId, cancellationToken);
	}

	public async Task<PagedResult<TradeRemindDto>> GetByUserAndTradeCodePagedAsync(int userId, int tradeCodeId, PaginationRequest paginationRequest, RemindSortRequest? sortRequest = null, CancellationToken cancellationToken = default)
	{
		var spec = new TradeRemindQuerySpecification(userId, tradeCodeId, sortRequest);
		var queryable = SpecificationEvaluator.GetQuery(_dbSet.AsQueryable(), spec);

		return await queryable
			.Select(r => new TradeRemindDto
			{
				Id = r.Id,
				TextRemind = r.TextRemind,
				DateTime = r.DateTime,
				TradeCodeId = r.TradeCodeId,
				UserId = r.UserId
			})
			.ToPagedAsync(paginationRequest, cancellationToken);
	}
}
