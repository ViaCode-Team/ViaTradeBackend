using System.ComponentModel.DataAnnotations;
using Application.Auth.Interfaces;
using Application.Common.Models;
using Application.Notes.Interfaces;
using Application.Notes.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ViaTradeBackend.Contracts.Notes;
using ViaTradeBackend.Contracts.Statistics;
using ViaTradeBackend.Mappings;

namespace ViaTradeBackend.Controllers;

[Route("api/v1/[controller]")]
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
		[FromQuery] PageOptions pageOptions,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var userNotes = await noteQueryService.GetPageAsync(userId, noteFilter, pageOptions, ct);

		return TypedResults.Ok(userNotes.Map(ApiMapper.ToResponse));
	}

	[HttpGet]
	public async Task<Ok<PageResult<NoteResponse>>> GetSearchNotes(
		[FromQuery] NoteSearchFilter noteFilter,
		[FromQuery] PageOptions pageOptions,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var userNotes = await noteQueryService.GetSearchAsync(userId, noteFilter, pageOptions, ct);

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
