using Application.Auth.Interfaces;
using Application.Common.Models.Pagination;
using Application.Common.Models.Sort;
using Application.Reminds.Interfaces;
using Mapster;
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
public class RemindController(
	IRemindCommandService remindCommandService,
	IRemindQueryService remindQueryService,
	IJwtHelper jwtHelper) : ControllerBase
{
	[Authorize]
	[HttpGet("statistics")]
	public async Task<Ok<TradeRemindStatisticResponse>> GetRemindStatistics(CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var statistics = await remindQueryService.GetStatistics(userId, ct);

		return TypedResults.Ok(statistics.Adapt<TradeRemindStatisticResponse>());
	}

	[ServicePassword]
	[HttpGet("byuser/actual")]
	public async Task<Ok<IEnumerable<TradeRemindResponse>>> GetActualReminders(CancellationToken ct)
	{
		var reminders = await remindQueryService.GetActualAsync(ct);

		return TypedResults.Ok(reminders.Adapt<IEnumerable<TradeRemindResponse>>());
	}

	[ServicePassword]
	[HttpDelete("byuser/actual/{remindId}")]
	public async Task<NoContent> DeleteActualReminder([FromRoute, Required] int remindId, CancellationToken ct)
	{

		return TypedResults.NoContent();
	}

	[Authorize]
	[HttpGet("byuser")]
	public async Task<Ok<PagedResult<TradeRemindResponse>>> GetUserReminders(
		[FromQuery] PaginationRequest paginationRequest,
		[FromQuery] RemindSortRequest? sortRequest,
		CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var userReminders = await remindQueryService.GetPagedAsync(
			userId, paginationRequest, sortRequest, ct);

		return TypedResults.Ok(userReminders.Map(r => r.Adapt<TradeRemindResponse>()));
	}

	[Authorize]
	[HttpGet("byuser/instrument/{idInstrument}")]
	public async Task<Ok<PagedResult<TradeRemindResponse>>> GetUserRemindersByInstrument(
		[FromQuery] PaginationRequest paginationRequest,
		[FromQuery] RemindSortRequest? sortRequest,
		[Required, FromRoute] int idInstrument,
		CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var reminders = await remindQueryService.GetPagedAsync(
			idInstrument, userId, paginationRequest, sortRequest, ct);

		return TypedResults.Ok(reminders.Map(r => r.Adapt<TradeRemindResponse>()));
	}

	[Authorize]
	[HttpGet("byuser/{remindId}")]
	public async Task<Ok<TradeRemindResponse>> GetUserReminderById(
		[Required] int remindId,
		CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var remind = await remindQueryService.GetAsync(remindId, userId, ct);

		return TypedResults.Ok(remind.Adapt<TradeRemindResponse>());
	}

	[Authorize]
	[HttpPost("byuser/instrument/{idInstrument}")]
	public async Task<Created> CreateUserRemind(
		[Required, FromRoute] int idInstrument,
		[FromBody, Required] CreateTradeRemindRequest request,
		CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);

		await remindCommandService.CreateAsync(
			userId,
			idInstrument,
			request.TextRemind,
			request.DateTime,
			ct);

		return TypedResults.Created();
	}

	[Authorize]
	[HttpPut("byuser/{remindId}")]
	public async Task<NoContent> UpdateUserReminder(
		[Required, FromRoute] int remindId,
		[FromBody, Required] UpdateTradeRemindRequest request,
		CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);

		return TypedResults.NoContent();
	}

	[Authorize]
	[HttpDelete("byuser/{remindId}")]
	public async Task<NoContent> DeleteUserReminder(
		[Required, FromRoute] int remindId,
		CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);

		return TypedResults.NoContent();
	}
}
