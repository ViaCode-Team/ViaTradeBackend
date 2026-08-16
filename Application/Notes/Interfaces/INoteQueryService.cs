using Application.Common.Models;
using Application.Notes.Models;
using Domain.Entities;

namespace Application.Notes.Interfaces;

public interface INoteQueryService
{
	Task<NoteStatisticDto> GetStatisticsAsync(int userId, CancellationToken ct);
	Task<Note> GetByIdAsync(int userId, int noteId, CancellationToken ct);
	Task<Note> GetInstrumentAsync(int userId, int instrumentId, CancellationToken ct);
	Task<Note> GetStrategyAsync(int userId, int strategyId, CancellationToken ct);
	Task<PageResult<NoteDto>> GetPageAsync(
		int userId,
		NoteFilter noteFilter,
		PageOptions pageOptions,
		CancellationToken ct
	);
	Task<PageResult<NoteDto>> GetSearchAsync(
		int userId,
		SearchFilter noteSearchFilter,
		PageOptions pageOptions,
		CancellationToken ct
	);
}
