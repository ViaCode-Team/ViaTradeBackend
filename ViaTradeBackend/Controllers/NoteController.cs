using Domain.Users.Entities;
using Application.Interfaces.Utils;
using Application.Notes.Commands;
using Application.Notes.Queries;
using Domain.Models.Filters;
using Domain.Models.Pagination;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using ViaTradeBackend.Contracts.Notes;
using ViaTradeBackend.Contracts.Statistics;
using Domain.Notes.Enums;

namespace ViaTradeBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class NoteController(ISender sender, IJwtHelper jwtHelper) : ControllerBase
{
	private readonly ISender _sender = sender;
	private readonly IJwtHelper _jwtHelper = jwtHelper;

	[HttpGet("statistics")]
	public async Task<Ok<NoteStatisticResponse>> GetNoteStatistics(CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var query = new GetNoteStatisticQuery(userId);
		var statistics = await _sender.Send(query, cancellationToken);
		return TypedResults.Ok(statistics.Adapt<NoteStatisticResponse>());
	}

	[HttpGet("byuser")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<Ok<PagedResult<NoteResponse>>> GetUserNotes(
		[FromQuery] NoteFilterRequest filterRequest,
		[FromQuery] PaginationRequest paginationRequest,
		CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var query = new GetUserNotePagedQuery(userId, filterRequest, paginationRequest);
		var notes = await _sender.Send(query, cancellationToken);
		return TypedResults.Ok(notes.Map(n => n.Adapt<NoteResponse>()));
	}

	[HttpGet("byuser/instrument/{idInstrument}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<Ok<NoteResponse>> GetUserInstrumentNote(
		[FromRoute, Required] int idInstrument,
		CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var query = new GetUserNoteByPropQuery(idInstrument, userId, NoteType.TradeCodeNote);
		var note = await _sender.Send(query, cancellationToken);
		return TypedResults.Ok(note.Adapt<NoteResponse>());
	}

	[HttpGet("byuser/strategy/{idStrategy}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<Ok<NoteResponse>> GetUserStrategyNote(
		[FromRoute, Required] int idStrategy,
		CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var query = new GetUserNoteByPropQuery(idStrategy, userId, NoteType.TradeStrategyNote);
		var note = await _sender.Send(query, cancellationToken);
		return TypedResults.Ok(note.Adapt<NoteResponse>());
	}

	[HttpPost("byuser/instrument/{idInstrument}")]
	[ProducesResponseType(StatusCodes.Status201Created)]
	public async Task<Created> AddUserInstrumentNote(
		[FromRoute, Required] int idInstrument,
		[FromBody, Required] CreateNoteRequest request,
		CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var command = new AddUserNoteCommand(idInstrument, NoteType.TradeCodeNote, userId, request.NoteText);
		await _sender.Send(command, cancellationToken);
		return TypedResults.Created();
	}

	[HttpPost("byuser/strategy/{idStrategy}")]
	[ProducesResponseType(StatusCodes.Status201Created)]
	public async Task<Created> AddUserStrategyNote(
		[FromRoute, Required] int idStrategy,
		[FromBody, Required] CreateNoteRequest request,
		CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var command = new AddUserNoteCommand(idStrategy, NoteType.TradeStrategyNote, userId, request.NoteText);
		await _sender.Send(command, cancellationToken);
		return TypedResults.Created();
	}

	[HttpPut("byuser/instrument/{idInstrument}")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public async Task<NoContent> UpdateUserInstrumentNote(
		[FromRoute, Required] int idInstrument,
		[FromBody, Required] UpdateNoteRequest request,
		CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var command = new UpdateUserNoteCommand(idInstrument, NoteType.TradeCodeNote, userId, request.NoteText);
		await _sender.Send(command, cancellationToken);
		return TypedResults.NoContent();
	}

	[HttpPut("byuser/strategy/{idStrategy}")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public async Task<NoContent> UpdateUserStrategyNote(
		[FromRoute, Required] int idStrategy,
		[FromBody, Required] UpdateNoteRequest request,
		CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var command = new UpdateUserNoteCommand(idStrategy, NoteType.TradeStrategyNote, userId, request.NoteText);
		await _sender.Send(command, cancellationToken);
		return TypedResults.NoContent();
	}

	[HttpDelete("byuser/instrument/{idInstrument}")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public async Task<NoContent> DeleteUserInstrumentNote(
		[FromRoute, Required] int idInstrument,
		CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var command = new DeleteUserNoteCommand(idInstrument, userId, NoteType.TradeCodeNote);
		await _sender.Send(command, cancellationToken);
		return TypedResults.NoContent();
	}

	[HttpDelete("byuser/strategy/{idStrategy}")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public async Task<NoContent> DeleteUserStrategyNote(
		[FromRoute, Required] int idStrategy,
		CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var command = new DeleteUserNoteCommand(idStrategy, userId, NoteType.TradeStrategyNote);
		await _sender.Send(command, cancellationToken);
		return TypedResults.NoContent();
	}
}
