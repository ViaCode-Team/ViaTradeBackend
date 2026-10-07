using Mediator;

namespace ViaTrade.Application.Notes.GetStatistics;

public sealed record GetNoteStatisticsQuery() : IQuery<NoteStatisticsResult>;
