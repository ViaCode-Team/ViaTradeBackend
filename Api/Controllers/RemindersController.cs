using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ViaTrade.Api.Contracts.Reminders;
using ViaTrade.Api.Contracts.Statistics;
using ViaTrade.Api.Mappings;
using ViaTrade.Application.Auth.Interfaces;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Reminders.Interfaces;
using ViaTrade.Application.Reminders.Models;

namespace ViaTrade.Api.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class RemindersController(
	IReminderCommandService reminderCommandService,
	IReminderQueryService reminderQueryService,
	IJwtHelper jwtHelper
) : ControllerBase
{
	[HttpGet("statistics")]
	public async Task<Ok<ReminderStatisticsResponse>> GetReminderStatistics(CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var statistics = await reminderQueryService.GetStatisticsAsync(userId, ct);

		return TypedResults.Ok(ApiMapper.ToResponse(statistics));
	}

	[HttpGet]
	public async Task<Ok<PageResult<ReminderResponse>>> GetReminders(
		[FromQuery] ReminderDeliveryStatus deliveryStatus,
		[FromQuery] PageOptions pageOptions,
		[FromQuery] ReminderSort reminderSort,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var reminders = await reminderQueryService.GetPageAsync(userId, deliveryStatus, pageOptions, reminderSort, ct);

		return TypedResults.Ok(reminders.Map(ApiMapper.ToResponse));
	}

	[HttpGet("{reminderId:int}")]
	public async Task<Ok<ReminderResponse>> GetReminderById(
		[FromRoute, Range(1, int.MaxValue)] int reminderId,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var reminder = await reminderQueryService.GetAsync(userId, reminderId, ct);

		return TypedResults.Ok(ApiMapper.ToResponse(reminder));
	}

	[HttpPut("{reminderId:int}")]
	public async Task<NoContent> UpdateReminder(
		[FromRoute, Range(1, int.MaxValue)] int reminderId,
		[FromBody, Required] UpdateReminderRequest request,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		await reminderCommandService.UpdateAsync(userId, reminderId, request.Text, request.RemindAt, ct);

		return TypedResults.NoContent();
	}

	[HttpDelete("{reminderId:int}")]
	public async Task<NoContent> DeleteReminder(
		[FromRoute, Range(1, int.MaxValue)] int reminderId,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		await reminderCommandService.DeleteAsync(userId, reminderId, ct);

		return TypedResults.NoContent();
	}
}
