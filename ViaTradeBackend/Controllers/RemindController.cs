using Application.Interfaces;
using Application.Interfaces.Utils;
using Domain.Models.Pagination;
using Domain.Models.Sort;
using Mapster;
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
public class RemindController(
	ITradeRemindService tradeRemindService,
	IJwtHelper jwtHelper) : ControllerBase
{
	private readonly ITradeRemindService _tradeRemindService = tradeRemindService;
	private readonly IJwtHelper _jwtHelper = jwtHelper;

	[Authorize]
	[HttpGet("statistics")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<Ok<TradeRemindStatisticResponse>> GetRemindStatistics(CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var statistics = await _tradeRemindService.GetRemindStatisticAsync(userId, cancellationToken);
		return TypedResults.Ok(statistics.Adapt<TradeRemindStatisticResponse>());
	}

	[ServicePassword]
	[HttpGet("byuser/actual")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<Ok<List<TradeRemindResponse>>> GetActualReminders(CancellationToken cancellationToken)
	{
		var reminders = await _tradeRemindService.GetActualRemindAsync(cancellationToken);
		return TypedResults.Ok(reminders.Adapt<List<TradeRemindResponse>>());
	}

	[ServicePassword]
	[HttpDelete("byuser/actual/{remindId}")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public async Task<NoContent> DeleteActualReminder([FromRoute, Required] int remindId, CancellationToken cancellationToken)
	{
		await _tradeRemindService.DeleteActualRemindAsync(remindId, cancellationToken);
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
		var userReminders = await _tradeRemindService.GetByUserPagedAsync(userId, paginationRequest, sortRequest, cancellationToken);
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
		var reminders = await _tradeRemindService.GetByUserAndTradeCodePagedAsync(userId, idInstrument, paginationRequest, sortRequest, cancellationToken);
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
		var reminder = await _tradeRemindService.GetByIdAsync(remindId, userId, cancellationToken);
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
		await _tradeRemindService.CreateAsync(userId, idInstrument, request.Adapt<TradeRemindCreateDto>(), cancellationToken);
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
		await _tradeRemindService.UpdateAsync(remindId, userId, request.Adapt<TradeRemindCreateDto>(), cancellationToken);
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
		await _tradeRemindService.DeleteAsync(remindId, userId, cancellationToken);
		return TypedResults.NoContent();
	}
}





