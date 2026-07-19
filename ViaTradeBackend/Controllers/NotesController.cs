using System.ComponentModel.DataAnnotations;
using Application.Auth.Interfaces;
using Application.Common.Queries;
using Application.Notes.Interfaces;
using Application.Notes.Queries;
using Domain.Notes.Enums;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ViaTradeBackend.Contracts.Notes;
using ViaTradeBackend.Contracts.Statistics;

namespace ViaTradeBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class NotesController(
	INoteCommandService noteCommandService,
	INoteQueryService noteQueryService,
	IJwtHelper jwtHelper
) : ControllerBase
{
	[HttpGet("statistics")]
	public async Task<Ok<NoteStatisticResponse>> GetNoteStatistics(CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var noteStatistics = await noteQueryService.GetStatisticsAsync(userId, ct);

		return TypedResults.Ok(noteStatistics.Adapt<NoteStatisticResponse>());
	}

	[HttpGet("byuser")]
	public async Task<Ok<PageResult<NoteResponse>>> GetUserNotes(
		[FromQuery] NoteFilter filter,
		[FromQuery] PageOptions page,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var userNotes = await noteQueryService.GetAsync(userId, filter, page, ct);

		return TypedResults.Ok(userNotes.Map(n => n.Adapt<NoteResponse>()));
	}

	[HttpGet("byuser/instrument/{tradeCodeId}")]
	public async Task<Ok<NoteResponse>> GetUserInstrumentNote(
		[FromRoute, Required] int tradeCodeId,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var note = await noteQueryService.GetAsync(tradeCodeId, userId, NoteType.TradeCodeNote, ct);

		return TypedResults.Ok(note.Adapt<NoteResponse>());
	}

	[HttpGet("byuser/strategy/{strategyId}")]
	public async Task<Ok<NoteResponse>> GetUserStrategyNote([FromRoute, Required] int strategyId, CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var note = await noteQueryService.GetAsync(strategyId, userId, NoteType.TradeStrategyNote, ct);

		return TypedResults.Ok(note.Adapt<NoteResponse>());
	}

	[HttpPost("byuser/instrument/{tradeCodeId}")]
	public async Task<Created> AddUserInstrumentNote(
		[FromRoute, Required] int tradeCodeId,
		[FromBody, Required] CreateNoteRequest request,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		await noteCommandService.AddAsync(tradeCodeId, NoteType.TradeCodeNote, userId, request.NoteText, ct);

		return TypedResults.Created();
	}

	[HttpPost("byuser/strategy/{strategyId}")]
	public async Task<Created> AddUserStrategyNote(
		[FromRoute, Required] int strategyId,
		[FromBody, Required] CreateNoteRequest request,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		await noteCommandService.AddAsync(strategyId, NoteType.TradeStrategyNote, userId, request.NoteText, ct);

		return TypedResults.Created();
	}

	[HttpPut("byuser/instrument/{tradeCodeId}")]
	public async Task<NoContent> UpdateUserInstrumentNote(
		[FromRoute, Required] int tradeCodeId,
		[FromBody, Required] UpdateNoteRequest request,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		await noteCommandService.UpdateAsync(tradeCodeId, NoteType.TradeCodeNote, userId, request.NoteText, ct);

		return TypedResults.NoContent();
	}

	[HttpPut("byuser/strategy/{strategyId}")]
	public async Task<NoContent> UpdateUserStrategyNote(
		[FromRoute, Required] int strategyId,
		[FromBody, Required] UpdateNoteRequest request,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		await noteCommandService.UpdateAsync(strategyId, NoteType.TradeStrategyNote, userId, request.NoteText, ct);

		return TypedResults.NoContent();
	}

	[HttpDelete("byuser/instrument/{tradeCodeId}")]
	public async Task<NoContent> DeleteUserInstrumentNote([FromRoute, Required] int tradeCodeId, CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		await noteCommandService.DeleteAsync(tradeCodeId, userId, NoteType.TradeCodeNote, ct);

		return TypedResults.NoContent();
	}

	[HttpDelete("byuser/strategy/{strategyId}")]
	public async Task<NoContent> DeleteUserStrategyNote([FromRoute, Required] int strategyId, CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		await noteCommandService.DeleteAsync(strategyId, userId, NoteType.TradeStrategyNote, ct);

		return TypedResults.NoContent();
	}
}
