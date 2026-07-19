using Application.Auth.Interfaces;
using Application.Common.Models.Pagination;
using Application.Common.Models.Sort;
using Application.Reminders.Interfaces;
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
public class RemindersController(
	IReminderCommandService remindCommandService,
	IReminderQueryService remindQueryService,
	IJwtHelper jwtHelper) : ControllerBase
{
	[Authorize]
	[HttpGet("statistics")]
	public async Task<Ok<TradeRemindStatisticResponse>> GetRemindStatistics(CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var statistics = await remindQueryService.GetStatisticsAsync(userId, ct);

		return TypedResults.Ok(statistics.Adapt<TradeRemindStatisticResponse>());
	}

	[ServicePassword]
	[HttpGet("byuser/actual")]
	public async Task<Ok<IEnumerable<TradeRemindResponse>>> GetActualReminders(CancellationToken ct)
	{
		var reminders = await remindQueryService.GetAsync(ct);

		return TypedResults.Ok(reminders.Adapt<IEnumerable<TradeRemindResponse>>());
	}

	[ServicePassword]
	[HttpDelete("byuser/actual/{id}")]
	public async Task<NoContent> DeleteActualReminder([FromRoute, Required] int id, CancellationToken ct)
	{
		await remindCommandService.DeleteAsync(id, ct);
		return TypedResults.NoContent();
	}

	[Authorize]
	[HttpGet("byuser")]
	public async Task<Ok<PagedResult<TradeRemindResponse>>> GetUserReminders(
		[FromQuery] PaginationRequest paginationRequest,
		[FromQuery] ReminderSortRequest sortRequest,
		CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var userReminders = await remindQueryService.GetAsync(
			userId, paginationRequest, sortRequest, ct);

		return TypedResults.Ok(userReminders.Map(r => r.Adapt<TradeRemindResponse>()));
	}

	[Authorize]
	[HttpGet("byuser/instrument/{tradeCodeId}")]
	public async Task<Ok<PagedResult<TradeRemindResponse>>> GetUserRemindersByInstrument(
		[FromQuery] PaginationRequest paginationRequest,
		[FromQuery] ReminderSortRequest sortRequest,
		[Required, FromRoute] int tradeCodeId,
		CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var reminders = await remindQueryService.GetAsync(
			userId, tradeCodeId, paginationRequest, sortRequest, ct);

		return TypedResults.Ok(reminders.Map(r => r.Adapt<TradeRemindResponse>()));
	}

	[Authorize]
	[HttpGet("byuser/{id}")]
	public async Task<Ok<TradeRemindResponse>> GetUserReminderById(
		[Required] int id,
		CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var remind = await remindQueryService.GetAsync(id, userId, ct);

		return TypedResults.Ok(remind.Adapt<TradeRemindResponse>());
	}

	[Authorize]
	[HttpPost("byuser/instrument/{tradeCodeId}")]
	public async Task<Created> CreateUserRemind(
		[Required, FromRoute] int tradeCodeId,
		[FromBody, Required] CreateTradeRemindRequest request,
		CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);

		await remindCommandService.CreateAsync(
			userId,
			tradeCodeId,
			request.TextRemind,
			request.DateTime,
			ct);

		return TypedResults.Created();
	}

	[Authorize]
	[HttpPut("byuser/{id}")]
	public async Task<NoContent> UpdateUserReminder(
		[Required, FromRoute] int id,
		[FromBody, Required] UpdateTradeRemindRequest request,
		CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		await remindCommandService.UpdateAsync(id, userId, request.TextRemind, request.DateTime, ct);

		return TypedResults.NoContent();
	}

	[Authorize]
	[HttpDelete("byuser/{id}")]
	public async Task<NoContent> DeleteUserReminder(
		[Required, FromRoute] int id,
		CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		await remindCommandService.DeleteAsync(id, userId, ct);

		return TypedResults.NoContent();
	}
}
