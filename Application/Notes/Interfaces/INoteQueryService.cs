using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Notes.Models;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Notes.Interfaces;

public interface INoteQueryService
{
	Task<NoteStatisticDto> GetStatisticsAsync(int userId, CancellationToken ct);
	Task<Note> GetByIdAsync(int userId, int noteId, CancellationToken ct);
	Task<Note> GetInstrumentAsync(int userId, int instrumentId, CancellationToken ct);
	Task<Note> GetStrategyAsync(int userId, int strategyId, CancellationToken ct);
	Task<PageResult<NoteDto>> GetPageAsync(
		int userId,
		NoteFilter noteFilter,
		NoteSearch noteSearch,
		PageOptions pageOptions,
		CancellationToken ct
	);
}
