using Mediator;
using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Notes.Common.Abstractions;

namespace ViaTrade.Application.Notes.GetStatistics;

public sealed class GetNoteStatisticsHandler(IUserContext userContext, INoteRepository noteStatistics)
	: IQueryHandler<GetNoteStatisticsQuery, NoteStatisticsResult>
{
	public async ValueTask<NoteStatisticsResult> Handle(GetNoteStatisticsQuery query, CancellationToken ct)
	{
		return await noteStatistics.GetStatisticsAsync(userContext.UserId, ct);
	}
}
