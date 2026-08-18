using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ViaTrade.Api.Contracts.Notes;
using ViaTrade.Api.Contracts.Statistics;
using ViaTrade.Api.Mappings;
using ViaTrade.Api.Routing;
using ViaTrade.Application.Auth.Interfaces;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Notes.Interfaces;
using ViaTrade.Application.Notes.Models;

namespace ViaTrade.Api.Controllers;

[Route($"{ApiRoutes.V1.Web}/[controller]")]
[ApiController]
public class NotesController(INoteQueryService noteQueryService, IJwtHelper jwtHelper) : ControllerBase
{
	[HttpGet("statistics")]
	public async Task<Ok<NoteStatisticResponse>> GetNoteStatistics(CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var noteStatistics = await noteQueryService.GetStatisticsAsync(userId, ct);

		return TypedResults.Ok(ApiMapper.ToResponse(noteStatistics));
	}

	[HttpGet]
	public async Task<Ok<PageResult<NoteResponse>>> GetNotes(
		[FromQuery] NoteFilter noteFilter,
		[FromQuery] NoteSearch noteSearch,
		[FromQuery] PageOptions pageOptions,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var userNotes = await noteQueryService.GetPageAsync(userId, noteFilter, noteSearch, pageOptions, ct);

		return TypedResults.Ok(userNotes.Map(ApiMapper.ToResponse));
	}

	[HttpGet("{noteId:int}")]
	public async Task<Ok<NoteResponse>> GetNoteById(
		[FromRoute, Range(1, int.MaxValue)] int noteId,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var note = await noteQueryService.GetByIdAsync(userId, noteId, ct);

		return TypedResults.Ok(ApiMapper.ToResponse(note));
	}
}
