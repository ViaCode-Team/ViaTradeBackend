using System.ComponentModel.DataAnnotations;
using Application.Auth.Interfaces;
using Application.Common.Queries;
using Application.Reminders.Interfaces;
using Application.Reminders.Queries;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ViaTradeBackend.Attribute;
using ViaTradeBackend.Contracts.Reminders;
using ViaTradeBackend.Contracts.Statistics;

namespace ViaTradeBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RemindersController(
	IReminderCommandService reminderCommandService,
	IReminderQueryService reminderQueryService,
	IJwtHelper jwtHelper
) : ControllerBase
{
	[Authorize]
	[HttpGet("statistics")]
	public async Task<Ok<ReminderStatisticsResponse>> GetReminderStatistics(CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var statistics = await reminderQueryService.GetStatisticsAsync(userId, ct);

		return TypedResults.Ok(statistics.Adapt<ReminderStatisticsResponse>());
	}

	[ServicePassword]
	[HttpGet("byuser/actual")]
	public async Task<Ok<IEnumerable<ReminderResponse>>> GetDueReminders(CancellationToken ct)
	{
		var reminders = await reminderQueryService.GetAsync(ct);

		return TypedResults.Ok(reminders.Adapt<IEnumerable<ReminderResponse>>());
	}

	[ServicePassword]
	[HttpDelete("byuser/actual/{id}")]
	public async Task<NoContent> DeleteDueReminder([FromRoute, Required] int id, CancellationToken ct)
	{
		await reminderCommandService.DeleteAsync(id, ct);
		return TypedResults.NoContent();
	}

	[Authorize]
	[HttpGet("byuser")]
	public async Task<Ok<PageResult<ReminderResponse>>> GetUserReminders(
		[FromQuery] PageOptions page,
		[FromQuery] ReminderSort sort,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var userReminders = await reminderQueryService.GetAsync(userId, page, sort, ct);

		return TypedResults.Ok(userReminders.Map(r => r.Adapt<ReminderResponse>()));
	}

	[Authorize]
	[HttpGet("byuser/instrument/{tradeCodeId}")]
	public async Task<Ok<PageResult<ReminderResponse>>> GetUserRemindersByInstrument(
		[FromQuery] PageOptions page,
		[FromQuery] ReminderSort sort,
		[Required, FromRoute] int tradeCodeId,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var reminders = await reminderQueryService.GetAsync(userId, tradeCodeId, page, sort, ct);

		return TypedResults.Ok(reminders.Map(r => r.Adapt<ReminderResponse>()));
	}

	[Authorize]
	[HttpGet("byuser/{id}")]
	public async Task<Ok<ReminderResponse>> GetUserReminderById([Required] int id, CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var reminder = await reminderQueryService.GetAsync(id, userId, ct);

		return TypedResults.Ok(reminder.Adapt<ReminderResponse>());
	}

	[Authorize]
	[HttpPost("byuser/instrument/{tradeCodeId}")]
	public async Task<Created> CreateUserRemind(
		[Required, FromRoute] int tradeCodeId,
		[FromBody, Required] CreateReminderRequest request,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);

		await reminderCommandService.CreateAsync(userId, tradeCodeId, request.Text, request.DateTime, ct);

		return TypedResults.Created();
	}

	[Authorize]
	[HttpPut("byuser/{id}")]
	public async Task<NoContent> UpdateUserReminder(
		[Required, FromRoute] int id,
		[FromBody, Required] UpdateReminderRequest request,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		await reminderCommandService.UpdateAsync(id, userId, request.Text, request.DateTime, ct);

		return TypedResults.NoContent();
	}

	[Authorize]
	[HttpDelete("byuser/{id}")]
	public async Task<NoContent> DeleteUserReminder([Required, FromRoute] int id, CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		await reminderCommandService.DeleteAsync(id, userId, ct);

		return TypedResults.NoContent();
	}
}
