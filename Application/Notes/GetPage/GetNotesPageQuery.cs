using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Notes.Common;

namespace ViaTrade.Application.Notes.GetPage;

public sealed record GetNotesPageQuery(NoteFilter NoteFilter, NoteSearch NoteSearch, PageOptions PageOptions)
	: IQuery<PageResult<NoteResult>>;
