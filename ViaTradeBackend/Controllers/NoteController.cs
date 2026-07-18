using Application.Contracts.Dto.NoteRemind;
using Application.Interfaces;
using Application.Interfaces.Utils;
using Domain.Entities.DataBase;
using Domain.Models.Filters;
using Domain.Models.Pagination;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using ViaTradeBackend.Contracts.Statistics;

using ViaTradeBackend.Contracts.Notes;

namespace ViaTradeBackend.Controllers;
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class NoteController(
	INoteService noteService,
	IJwtHelper jwtHelper) : ControllerBase
{
	private readonly INoteService _noteService = noteService;
	private readonly IJwtHelper _jwtHelper = jwtHelper;

	[HttpGet("statistics")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<Ok<NoteStatisticResponse>> GetNoteStatistics(CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var statistics = await _noteService.GetNoteStatisticAsync(userId, cancellationToken);
		return TypedResults.Ok(statistics.Adapt<NoteStatisticResponse>());
	}

	[HttpGet("byuser")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<Ok<PagedResult<NoteResponse>>> GetUserNotes(
		[FromQuery] NoteFilterRequest? filterRequest,
		[FromQuery] PaginationRequest? paginationRequest,
		CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var notes = await _noteService.GetUserNotePagedAsync(userId, filterRequest, paginationRequest, cancellationToken);
		return TypedResults.Ok(notes.Map((NoteDto n) => n.Adapt<NoteResponse>()));
	}

	[HttpGet("byuser/instrument/{idInstrument}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<Ok<NoteResponse>> GetUserTradeCodeNoteById(
		[Required, FromRoute] int idInstrument,
		CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var note = await _noteService.GetUserNoteByPropAsync(idInstrument, userId, NoteType.TradeCodeNote, cancellationToken);
		return TypedResults.Ok(note.Adapt<NoteResponse>());
	}

	[HttpGet("byuser/strategy/{idStrategy}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<Ok<NoteResponse>> GetUserStrategyNoteById(
		[Required, FromRoute] int idStrategy,
		CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var note = await _noteService.GetUserNoteByPropAsync(idStrategy, userId, NoteType.TradeStrategyNote, cancellationToken);
		return TypedResults.Ok(note.Adapt<NoteResponse>());
	}

	[HttpPost("byuser/instrument/{idInstrument}")]
	[ProducesResponseType(StatusCodes.Status201Created)]
	public async Task<Created> CreateUserTradeCodeNote(
		[Required, FromRoute] int idInstrument,
		[FromBody, Required] CreateNoteRequest request,
		CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var dto = new NoteDto { UserId = userId, NoteText = request.NoteText };
		await _noteService.AddUserNoteWithValidationAsync(idInstrument, NoteType.TradeCodeNote, dto, cancellationToken);
		return TypedResults.Created();
	}

	[HttpPost("byuser/strategy/{idStrategy}")]
	[ProducesResponseType(StatusCodes.Status201Created)]
	public async Task<Created> CreateUserStrategyNote(
		[Required, FromRoute] int idStrategy,
		[FromBody, Required] CreateNoteRequest request,
		CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var dto = new NoteDto { UserId = userId, NoteText = request.NoteText };
		await _noteService.AddUserNoteWithValidationAsync(idStrategy, NoteType.TradeStrategyNote, dto, cancellationToken);
		return TypedResults.Created();
	}

	[HttpPut("byuser/instrument/{idInstrument}")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public async Task<NoContent> UpdateUserTradeCodeNote(
		[Required, FromRoute] int idInstrument,
		[FromBody, Required] CreateNoteRequest request,
		CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var dto = new NoteDto { UserId = userId, NoteText = request.NoteText };
		await _noteService.UpdateUserNoteWithValidationAsync(idInstrument, NoteType.TradeCodeNote, dto, cancellationToken);
		return TypedResults.NoContent();
	}

	[HttpPut("byuser/strategy/{idStrategy}")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public async Task<NoContent> UpdateUserStrategyNote(
		[Required, FromRoute] int idStrategy,
		[FromBody, Required] CreateNoteRequest request,
		CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var dto = new NoteDto { UserId = userId, NoteText = request.NoteText };
		await _noteService.UpdateUserNoteWithValidationAsync(idStrategy, NoteType.TradeStrategyNote, dto, cancellationToken);
		return TypedResults.NoContent();
	}

	[HttpDelete("byuser/instrument/{idInstrument}")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public async Task<NoContent> DeleteUserTradeCodeNote(
		[Required, FromRoute] int idInstrument,
		CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		await _noteService.DeleteUserNoteAsync(idInstrument, userId, NoteType.TradeCodeNote, cancellationToken);
		return TypedResults.NoContent();
	}

	[HttpDelete("byuser/strategy/{idStrategy}")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public async Task<NoContent> DeleteUserStrategyNote(
		[Required, FromRoute] int idStrategy,
		CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		await _noteService.DeleteUserNoteAsync(idStrategy, userId, NoteType.TradeStrategyNote, cancellationToken);
		return TypedResults.NoContent();
	}
}



