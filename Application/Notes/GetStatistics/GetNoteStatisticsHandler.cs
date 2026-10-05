using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Notes.Common.Abstractions;

namespace ViaTrade.Application.Notes.GetStatistics;

public sealed class GetNoteStatisticsHandler(INoteRepository noteStatistics)
	: IQueryHandler<GetNoteStatisticsQuery, NoteStatisticsResult>
{
	public async Task<NoteStatisticsResult> HandleAsync(GetNoteStatisticsQuery query, CancellationToken ct = default)
	{
		return await noteStatistics.GetStatisticsAsync(query.UserId, ct);
	}
}
