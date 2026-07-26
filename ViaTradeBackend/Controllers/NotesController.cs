using System.ComponentModel.DataAnnotations;
using Application.Auth.Interfaces;
using Application.Common.Models;
using Application.Notes.Interfaces;
using Application.Notes.Models;
using Domain.Notes.Enums;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ViaTradeBackend.Contracts.Notes;
using ViaTradeBackend.Contracts.Statistics;
using ViaTradeBackend.Mappings;

namespace ViaTradeBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
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

		return TypedResults.Ok(ApiMapper.ToResponse(noteStatistics));
	}

	[HttpGet("byuser")]
	public async Task<Ok<PageResult<NoteResponse>>> GetUserNotes(
		[FromQuery] NoteFilter noteFilter,
		[FromQuery] PageOptions pageOptions,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var userNotes = await noteQueryService.GetPageAsync(userId, noteFilter, pageOptions, ct);

		return TypedResults.Ok(userNotes.Map(ApiMapper.ToResponse));
	}

	[HttpGet("byuser/instrument/{tradeCodeId}")]
	public async Task<Ok<NoteResponse>> GetUserInstrumentNote(
		[FromRoute, Required] int tradeCodeId,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var note = await noteQueryService.GetAsync(userId, tradeCodeId, NoteType.TradeCodeNote, ct);

		return TypedResults.Ok(ApiMapper.ToResponse(note));
	}

	[HttpGet("byuser/strategy/{strategyId}")]
	public async Task<Ok<NoteResponse>> GetUserStrategyNote([FromRoute, Required] int strategyId, CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var note = await noteQueryService.GetAsync(userId, strategyId, NoteType.TradeStrategyNote, ct);

		return TypedResults.Ok(ApiMapper.ToResponse(note));
	}

	[HttpPost("byuser/instrument/{tradeCodeId}")]
	public async Task<Created> AddUserInstrumentNote(
		[FromRoute, Required] int tradeCodeId,
		[FromBody, Required] CreateNoteRequest request,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		await noteCommandService.AddAsync(userId, tradeCodeId, NoteType.TradeCodeNote, request.NoteText, ct);

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
		await noteCommandService.AddAsync(userId, strategyId, NoteType.TradeStrategyNote, request.NoteText, ct);

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
		await noteCommandService.UpdateAsync(userId, tradeCodeId, NoteType.TradeCodeNote, request.NoteText, ct);

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
		await noteCommandService.UpdateAsync(userId, strategyId, NoteType.TradeStrategyNote, request.NoteText, ct);

		return TypedResults.NoContent();
	}

	[HttpDelete("byuser/instrument/{tradeCodeId}")]
	public async Task<NoContent> DeleteUserInstrumentNote([FromRoute, Required] int tradeCodeId, CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		await noteCommandService.DeleteAsync(userId, tradeCodeId, NoteType.TradeCodeNote, ct);

		return TypedResults.NoContent();
	}

	[HttpDelete("byuser/strategy/{strategyId}")]
	public async Task<NoContent> DeleteUserStrategyNote([FromRoute, Required] int strategyId, CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		await noteCommandService.DeleteAsync(userId, strategyId, NoteType.TradeStrategyNote, ct);

		return TypedResults.NoContent();
	}
}
