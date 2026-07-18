using Domain.Users.Entities;
using Application.Reminds.Commands;
using Application.Reminds.Queries;
using Application.Interfaces.Utils;
using Domain.Reminds.Entities;
using Domain.Models.Pagination;
using Domain.Models.Sort;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using ViaTradeBackend.Attribute;
using ViaTradeBackend.Contracts.Statistics;
using Application.Contracts.Dto.Requests.Remind;
using ViaTradeBackend.Contracts.Reminds;

namespace ViaTradeBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RemindController(ISender sender, IJwtHelper jwtHelper) : ControllerBase
{
	private readonly ISender _sender = sender;
	private readonly IJwtHelper _jwtHelper = jwtHelper;

	[Authorize]
	[HttpGet("statistics")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<Ok<TradeRemindStatisticResponse>> GetRemindStatistics(CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var query = new GetRemindStatisticQuery(userId);
		var statistics = await _sender.Send(query, cancellationToken);
		return TypedResults.Ok(statistics.Adapt<TradeRemindStatisticResponse>());
	}

	[ServicePassword]
	[HttpGet("byuser/actual")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<Ok<List<TradeRemindResponse>>> GetActualReminders(CancellationToken cancellationToken)
	{
		var query = new GetActualRemindQuery();
		var reminders = await _sender.Send(query, cancellationToken);
		return TypedResults.Ok(reminders.Adapt<List<TradeRemindResponse>>());
	}

	[ServicePassword]
	[HttpDelete("byuser/actual/{remindId}")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public async Task<NoContent> DeleteActualReminder([FromRoute, Required] int remindId, CancellationToken cancellationToken)
	{
		var command = new DeleteActualRemindCommand(remindId);
		await _sender.Send(command, cancellationToken);
		return TypedResults.NoContent();
	}

	[Authorize]
	[HttpGet("byuser")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<Ok<PagedResult<TradeRemindResponse>>> GetUserReminders(
		[FromQuery] PaginationRequest paginationRequest,
		[FromQuery] RemindSortRequest? sortRequest,
		CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var query = new GetUserRemindersQuery(userId, paginationRequest, sortRequest);
		var userReminders = await _sender.Send(query, cancellationToken);
		return TypedResults.Ok(userReminders.Map(r => r.Adapt<TradeRemindResponse>()));
	}

	[Authorize]
	[HttpGet("byuser/instrument/{idInstrument}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<Ok<PagedResult<TradeRemindResponse>>> GetUserRemindersByInstrument(
		[FromQuery] PaginationRequest paginationRequest,
		[FromQuery] RemindSortRequest? sortRequest,
		[Required, FromRoute] int idInstrument,
		CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var query = new GetUserRemindersByInstrumentQuery(userId, idInstrument, paginationRequest, sortRequest);
		var reminders = await _sender.Send(query, cancellationToken);
		return TypedResults.Ok(reminders.Map(r => r.Adapt<TradeRemindResponse>()));
	}

	[Authorize]
	[HttpGet("byuser/{remindId}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<Ok<TradeRemindResponse>> GetUserReminderById(
		[Required] int remindId,
		CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var query = new GetUserReminderByIdQuery(remindId, userId);
		var reminder = await _sender.Send(query, cancellationToken);
		return TypedResults.Ok(reminder.Adapt<TradeRemindResponse>());
	}

	[Authorize]
	[HttpPost("byuser/instrument/{idInstrument}")]
	[ProducesResponseType(StatusCodes.Status201Created)]
	public async Task<Created> CreateUserReminder(
		[Required, FromRoute] int idInstrument,
		[FromBody, Required] CreateTradeRemindRequest request,
		CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var command = new CreateTradeRemindCommand(userId, idInstrument, request.TextRemind, request.DateTime);
		await _sender.Send(command, cancellationToken);
		return TypedResults.Created();
	}

	[Authorize]
	[HttpPut("byuser/{remindId}")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public async Task<NoContent> UpdateUserReminder(
		[Required, FromRoute] int remindId,
		[FromBody, Required] UpdateTradeRemindRequest request,
		CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var command = new UpdateTradeRemindCommand(remindId, userId, request.TextRemind, request.DateTime);
		await _sender.Send(command, cancellationToken);
		return TypedResults.NoContent();
	}

	[Authorize]
	[HttpDelete("byuser/{remindId}")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public async Task<NoContent> DeleteUserReminder(
		[Required, FromRoute] int remindId,
		CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var command = new DeleteTradeRemindCommand(remindId, userId);
		await _sender.Send(command, cancellationToken);
		return TypedResults.NoContent();
	}
}
