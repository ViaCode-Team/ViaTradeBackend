using Application.Contracts.Dto.NoteRemind;
using Application.Contracts.Dto.Statistic;
using Application.Interfaces.Repositories.Database;
using Domain.Entities.DataBase;
using Domain.Interfaces;
using Domain.Models.Pagination;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.DataBase;

public class NoteRepository(AppDbContext context) : GenericRepository<Note, NoteDto>(context), INoteRepository
{
	public async Task<NoteStatisticDto> GetNoteStatisticAsync(int userId, CancellationToken cancellationToken = default)
	{
		var baseQuery = _dbSet.Where(n => n.UserId == userId);

		var totalNotes = await baseQuery.CountAsync(cancellationToken);
		if (totalNotes == 0)
			return new NoteStatisticDto { TotalNotes = 0, StockNotes = 0, StrategyNotes = 0 };

		var stockNotes = await baseQuery.CountAsync(n => n.TradeCodeId != null, cancellationToken);
		var strategyNotes = await baseQuery.CountAsync(n => n.TradeStrategyId != null, cancellationToken);

		return new NoteStatisticDto
		{
			TotalNotes = totalNotes,
			StockNotes = stockNotes,
			StrategyNotes = strategyNotes
		};
	}

	public async Task<PagedResult<NoteDto>> GetPagedFilteredAsync(IQuerySpecification<Note> spec, PaginationRequest? paginationRequest, CancellationToken cancellationToken)
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
		int? tradeCodeId = null;
		if (noteType == NoteType.TradeCodeNote)
		{
			tradeCodeId = relatedId;
		}

		int? tradeStrategyId = null;
		if (noteType == NoteType.TradeStrategyNote)
		{
			tradeStrategyId = relatedId;
		}

		var note = new Note
		{
			UserId = dto.UserId,
			NoteText = dto.NoteText,
			TradeCodeId = tradeCodeId,
			TradeStrategyId = tradeStrategyId
		};

		_dbSet.Add(note);
		await SaveChangesAsync(cancellationToken);
	}

	public async Task UpdateUserNoteAsync(int id, NoteType noteType, NoteDto dto, CancellationToken cancellationToken)
	{
		int affectedRows = noteType switch
		{
			NoteType.TradeCodeNote => await _dbSet
				.Where(n => n.TradeCodeId == id && n.UserId == dto.UserId)
				.ExecuteUpdateAsync(s => s.SetProperty(n => n.NoteText, dto.NoteText), cancellationToken),

			NoteType.TradeStrategyNote => await _dbSet
				.Where(n => n.TradeStrategyId == id && n.UserId == dto.UserId)
				.ExecuteUpdateAsync(s => s.SetProperty(n => n.NoteText, dto.NoteText), cancellationToken),

			_ => throw new KeyNotFoundException()
		};

		if (affectedRows == 0)
			throw new KeyNotFoundException();
	}

	public async Task DeleteUserNoteAsync(int id, int userId, NoteType noteType, CancellationToken cancellationToken)
	{
		int affectedRows = noteType switch
		{
			NoteType.TradeCodeNote => await _dbSet
				.Where(n => n.TradeCodeId == id && n.UserId == userId)
				.ExecuteDeleteAsync(cancellationToken),

			NoteType.TradeStrategyNote => await _dbSet
				.Where(n => n.TradeStrategyId == id && n.UserId == userId)
				.ExecuteDeleteAsync(cancellationToken),

			_ => throw new KeyNotFoundException()
		};

		if (affectedRows == 0)
			throw new KeyNotFoundException();
	}
}
