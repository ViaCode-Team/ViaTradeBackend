using ViaTrade.Application.Common.Abstractions;

namespace ViaTrade.Application.Notes.GetStatistics;

public sealed record GetNoteStatisticsQuery() : IQuery<NoteStatisticsResult>;
