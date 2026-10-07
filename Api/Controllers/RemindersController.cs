using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ViaTrade.Api.ModelBinding.Attributes;
using ViaTrade.Api.Routing;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Reminders.Common;
using ViaTrade.Application.Reminders.Delete;
using ViaTrade.Application.Reminders.Get;
using ViaTrade.Application.Reminders.GetPage;
using ViaTrade.Application.Reminders.GetStatistics;
using ViaTrade.Application.Reminders.Update;

namespace ViaTrade.Api.Controllers;

[Route($"{ApiRoutes.V1.Web}/[controller]")]
[ApiController]
public class RemindersController(ISender sender) : ControllerBase
{
	[HttpGet("statistics")]
	public async Task<Ok<ReminderStatisticsResult>> GetReminderStatistics(CancellationToken ct)
	{
		var statistics = await sender.Send(new GetReminderStatisticsQuery(), ct);

		return TypedResults.Ok(statistics);
	}

	[HttpGet]
	public async Task<Ok<PageResult<ReminderResult>>> GetReminders(
		[FromQuery] GetRemindersPageQuery query,
		CancellationToken ct
	)
	{
		var reminders = await sender.Send(query, ct);

		return TypedResults.Ok(reminders);
	}

	[HttpGet("{reminderId:int}")]
	public async Task<Ok<ReminderResult>> GetReminderById(
		[FromQuery, FromRouteProperties(nameof(GetReminderQuery.ReminderId))] GetReminderQuery query,
		CancellationToken ct
	)
	{
		var reminder = await sender.Send(query, ct);

		return TypedResults.Ok(reminder);
	}

	[HttpPut("{reminderId:int}")]
	public async Task<NoContent> UpdateReminder(
		[FromBody, FromRouteProperties(nameof(UpdateReminderCommand.ReminderId))] UpdateReminderCommand command,
		CancellationToken ct
	)
	{
		await sender.Send(command, ct);

		return TypedResults.NoContent();
	}

	[HttpDelete("{reminderId:int}")]
	public async Task<NoContent> DeleteReminder(
		[FromQuery, FromRouteProperties(nameof(DeleteReminderCommand.ReminderId))] DeleteReminderCommand command,
		CancellationToken ct
	)
	{
		await sender.Send(command, ct);

		return TypedResults.NoContent();
	}
}
