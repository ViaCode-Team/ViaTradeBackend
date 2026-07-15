using Application.Interfaces.Repositories.Database;
using Domain.Entities.DataBase;
using Domain.Models.Dto.NoteRemind;
using Domain.Models.Pagination;
using Domain.Interfaces;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Domain.Models.Dto.Statistic;

namespace Infrastructure.Repositories.DataBase;

public class NoteRepository(AppDbContext context) : GenericRepository<Note, NoteDto>(context), INoteRepository
{
	public async Task<NoteStatistic> GetNoteStatisticAsync(int userId, CancellationToken cancellationToken = default)
	{
		var stats = await _dbSet
			.Where(n => n.UserId == userId)
			.GroupBy(n => 1)
			.Select(g => new NoteStatistic
			{
				TotalNotes = g.Count(),
				StockNotes = g.Count(n => n.TradeCodeId != null),
				StrategyNotes = g.Count(n => n.TradeStrategyId != null)
			})
			.FirstOrDefaultAsync(cancellationToken);

		return stats ?? new NoteStatistic { TotalNotes = 0, StockNotes = 0, StrategyNotes = 0 };
	}

	public async Task<PagedResult<NoteDto>> GetPagedFilteredAsync(ISpecification<Note> spec, PaginationRequest? paginationRequest, CancellationToken cancellationToken)
	{
		var queryable = SpecificationEvaluator.GetQuery(_dbSet.AsQueryable(), spec);

		return await queryable
			.Select(n => new NoteDto
			{
				Id = n.Id,
				UserId = n.UserId,
				NoteText = n.NoteText,
				TradeCodeId = n.TradeCodeId,
				TradeStrategyId = n.TradeStrategyId
			})
			.ToPagedAsync(paginationRequest, cancellationToken);
	}

	public async Task<Note?> FindUserNoteByEntityAsync(int userId, int relatedId, NoteType noteType, CancellationToken cancellationToken) => noteType switch
	{
		NoteType.TradeCodeNote => await _dbSet.FirstOrDefaultAsync(n => n.TradeCodeId == relatedId && n.UserId == userId, cancellationToken),
		NoteType.TradeStrategyNote => await _dbSet.FirstOrDefaultAsync(n => n.TradeStrategyId == relatedId && n.UserId == userId, cancellationToken),
		_ => null
	};

	public async Task<Note> GetUserNoteByProp(int id, int userId, NoteType noteType, CancellationToken cancellationToken)
	{
		Note? found = noteType switch

		{
			NoteType.TradeCodeNote => await _dbSet.FirstOrDefaultAsync(n => n.TradeCodeId == id && n.UserId == userId, cancellationToken),
			NoteType.TradeStrategyNote => await _dbSet.FirstOrDefaultAsync(n => n.TradeStrategyId == id && n.UserId == userId, cancellationToken),
			_ => throw new KeyNotFoundException()
		};

		return found
			?? throw new KeyNotFoundException();
	}

	public async Task AddUserNoteAsync(int relatedId, NoteType noteType, NoteDto dto, CancellationToken cancellationToken)
	{
		var note = new Note
		{
			UserId = dto.UserId,
			NoteText = dto.NoteText,
			TradeCodeId = noteType == NoteType.TradeCodeNote ? relatedId : null,
			TradeStrategyId = noteType == NoteType.TradeStrategyNote ? relatedId : null
		};

		_dbSet.Add(note);
		await context.SaveChangesAsync(cancellationToken);
	}

	public async Task UpdateUserNoteAsync(int id, NoteType noteType, NoteDto dto, CancellationToken cancellationToken)
	{
		var note = await ResolveNoteAsync(id, dto.UserId, noteType, cancellationToken)
			?? throw new KeyNotFoundException();

		note.NoteText = dto.NoteText;
		await context.SaveChangesAsync(cancellationToken);
	}

	public async Task DeleteUserNoteAsync(int id, int userId, NoteType noteType, CancellationToken cancellationToken)
	{
		var note = await ResolveNoteAsync(id, userId, noteType, cancellationToken)
			?? throw new KeyNotFoundException();

		_dbSet.Remove(note);
		await context.SaveChangesAsync(cancellationToken);
	}

	private async Task<Note?> ResolveNoteAsync(int id, int userId, NoteType noteType, CancellationToken cancellationToken) => noteType switch
	{
		NoteType.TradeCodeNote => await _dbSet.FirstOrDefaultAsync(n => n.TradeCodeId == id && n.UserId == userId, cancellationToken),
		NoteType.TradeStrategyNote => await _dbSet.FirstOrDefaultAsync(n => n.TradeStrategyId == id && n.UserId == userId, cancellationToken),
		_ => throw new KeyNotFoundException()
	};
}
