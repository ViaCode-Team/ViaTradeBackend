using Application.Auth.Interfaces;
using Application.Common.Models.Pagination;
using Application.Common.Models.Sort;
using Application.Reminds.Commands;
using Application.Reminds.Queries;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using ViaTradeBackend.Attribute;
using ViaTradeBackend.Contracts.Reminds;
using ViaTradeBackend.Contracts.Statistics;

namespace ViaTradeBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RemindController(ISender sender, IJwtHelper jwtHelper) : ControllerBase
{
	private readonly ISender _sender = sender;
	private readonly IJwtHelper _jwtHelper = jwtHelper;

	[Authorize]
	[HttpGet("statistics")]
	public async Task<Ok<TradeRemindStatisticResponse>> GetRemindStatistics(CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var query = new GetRemindStatisticQuery(userId);
		var statistics = await _sender.Send(query, cancellationToken);

		return TypedResults.Ok(statistics.Adapt<TradeRemindStatisticResponse>());
	}

	[ServicePassword]
	[HttpGet("byuser/actual")]
	public async Task<Ok<List<TradeRemindResponse>>> GetActualReminders(CancellationToken cancellationToken)
	{
		var query = new GetActualRemindQuery();
		var reminders = await _sender.Send(query, cancellationToken);

		return TypedResults.Ok(reminders.Adapt<List<TradeRemindResponse>>());
	}

	[ServicePassword]
	[HttpDelete("byuser/actual/{remindId}")]
	public async Task<NoContent> DeleteActualReminder([FromRoute, Required] int remindId, CancellationToken cancellationToken)
	{
		var command = new DeleteActualRemindCommand(remindId);
		await _sender.Send(command, cancellationToken);

		return TypedResults.NoContent();
	}

	[Authorize]
	[HttpGet("byuser")]
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
