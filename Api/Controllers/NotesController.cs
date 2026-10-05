using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ViaTrade.Api.Contracts.Notes;
using ViaTrade.Api.Contracts.Statistics;
using ViaTrade.Api.Mappings;
using ViaTrade.Api.Routing;
using ViaTrade.Application.Auth.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Notes.Common;
using ViaTrade.Application.Notes.Get;
using ViaTrade.Application.Notes.GetPage;
using ViaTrade.Application.Notes.GetStatistics;

namespace ViaTrade.Api.Controllers;

[Route($"{ApiRoutes.V1.Web}/[controller]")]
[ApiController]
public class NotesController(IJwtHelper jwtHelper) : ControllerBase
{
	[HttpGet("statistics")]
	public async Task<Ok<NoteStatisticResponse>> GetNoteStatistics(
		[FromServices] IQueryHandler<GetNoteStatisticsQuery, NoteStatisticsResult> handler,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var query = new GetNoteStatisticsQuery(userId);
		var noteStatistics = await handler.HandleAsync(query, ct);

		return TypedResults.Ok(ApiMapper.ToResponse(noteStatistics));
	}

	[HttpGet]
	public async Task<Ok<PageResult<NoteResponse>>> GetNotes(
		[FromQuery] NoteFilter noteFilter,
		[FromQuery] NoteSearch noteSearch,
		[FromQuery] PageOptions pageOptions,
		[FromServices] IQueryHandler<GetNotesPageQuery, PageResult<NoteResult>> handler,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var query = new GetNotesPageQuery(userId, noteFilter, noteSearch, pageOptions);
		var userNotes = await handler.HandleAsync(query, ct);

		return TypedResults.Ok(userNotes.Map(ApiMapper.ToResponse));
	}

	[HttpGet("{noteId:int}")]
	public async Task<Ok<NoteResponse>> GetNoteById(
		[FromRoute, Range(1, int.MaxValue)] int noteId,
		[FromServices] IQueryHandler<GetNoteQuery, NoteResult> handler,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var query = new GetNoteQuery(userId, noteId);
		var note = await handler.HandleAsync(query, ct);

		return TypedResults.Ok(ApiMapper.ToResponse(note));
	}
}
