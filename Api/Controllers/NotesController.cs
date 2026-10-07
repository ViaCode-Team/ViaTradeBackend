using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ViaTrade.Api.ModelBinding.Attributes;
using ViaTrade.Api.Routing;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Notes.Common;
using ViaTrade.Application.Notes.Get;
using ViaTrade.Application.Notes.GetPage;
using ViaTrade.Application.Notes.GetStatistics;

namespace ViaTrade.Api.Controllers;

[Route($"{ApiRoutes.V1.Web}/[controller]")]
[ApiController]
public class NotesController(ISender sender) : ControllerBase
{
	[HttpGet("statistics")]
	public async Task<Ok<NoteStatisticsResult>> GetNoteStatistics(CancellationToken ct)
	{
		var noteStatistics = await sender.Send(new GetNoteStatisticsQuery(), ct);

		return TypedResults.Ok(noteStatistics);
	}

	[HttpGet]
	public async Task<Ok<PageResult<NoteResult>>> GetNotes([FromQuery] GetNotesPageQuery query, CancellationToken ct)
	{
		var userNotes = await sender.Send(query, ct);

		return TypedResults.Ok(userNotes);
	}

	[HttpGet("{noteId:int}")]
	public async Task<Ok<NoteResult>> GetNoteById(
		[FromBody, FromRouteProperties(nameof(GetNoteQuery.NoteId))] GetNoteQuery query,
		CancellationToken ct
	)
	{
		var note = await sender.Send(query, ct);

		return TypedResults.Ok(note);
	}
}
