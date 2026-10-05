using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Notes.Common;

namespace ViaTrade.Application.Notes.GetPage;

public sealed record GetNotesPageQuery(
	int UserId,
	NoteFilter NoteFilter,
	NoteSearch NoteSearch,
	PageOptions PageOptions
) : IQuery<PageResult<NoteResult>>;
