using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Application.Notes.Models;
using Domain.Entities;

namespace Application.Notes.Interfaces;

public interface INoteRepository : IRepository<Note>
{
	Task<NoteStatisticDto> GetStatisticsAsync(int userId, CancellationToken ct = default);
	Task<PageResult<NoteProjectionDto>> GetPageWithTargetsAsync(
		IQuerySpecification<Note> specification,
		PageOptions pageOptions,
		CancellationToken ct = default
	);

	Task<PageResult<NoteProjectionDto>> GetPageSearchAsync(
		ISearchSpecification<Note> specification,
		PageOptions pageOptions,
		CancellationToken ct = default
	);
	Task<Note?> FindByIdForUserAsync(int userId, int noteId, CancellationToken ct = default);
	Task<Note?> FindByInstrumentAsync(int userId, int instrumentId, CancellationToken ct = default);
	Task<Note?> FindByStrategyAsync(int userId, int strategyId, CancellationToken ct = default);
	Task<int> ExecuteDeleteInstrumentAsync(int userId, int instrumentId, CancellationToken ct = default);
	Task<int> ExecuteDeleteStrategyAsync(int userId, int strategyId, CancellationToken ct = default);
	Task<int> ExecuteUpdateInstrumentAsync(
		int userId,
		int instrumentId,
		string text,
		CancellationToken ct = default
	);
	Task<int> ExecuteUpdateStrategyAsync(int userId, int strategyId, string text, CancellationToken ct = default);
}
