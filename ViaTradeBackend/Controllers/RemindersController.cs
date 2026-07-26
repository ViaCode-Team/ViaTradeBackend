using System.ComponentModel.DataAnnotations;
using Application.Auth.Interfaces;
using Application.Common.Models;
using Application.Reminders.Interfaces;
using Application.Reminders.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ViaTradeBackend.Contracts.Reminders;
using ViaTradeBackend.Contracts.Statistics;
using ViaTradeBackend.Mappings;

namespace ViaTradeBackend.Controllers;

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
		[FromQuery] PageOptions pageOptions,
		[FromQuery] ReminderSort reminderSort,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var reminders = await reminderQueryService.GetPageAsync(userId, pageOptions, reminderSort, ct);

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
