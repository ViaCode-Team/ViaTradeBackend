using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ViaTrade.Api.Contracts.Reminders;
using ViaTrade.Api.Contracts.Statistics;
using ViaTrade.Api.Mappings;
using ViaTrade.Api.Routing;
using ViaTrade.Application.Auth.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions;
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
public class RemindersController(IJwtHelper jwtHelper) : ControllerBase
{
	[HttpGet("statistics")]
	public async Task<Ok<ReminderStatisticsResponse>> GetReminderStatistics(
		[FromServices] IQueryHandler<GetReminderStatisticsQuery, ReminderStatisticsResult> handler,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var query = new GetReminderStatisticsQuery(userId);
		var statistics = await handler.HandleAsync(query, ct);

		return TypedResults.Ok(ApiMapper.ToResponse(statistics));
	}

	[HttpGet]
	public async Task<Ok<PageResult<ReminderResponse>>> GetReminders(
		[FromQuery] ReminderFilter reminderFilter,
		[FromQuery] ReminderSearch reminderSearch,
		[FromQuery] PageOptions pageOptions,
		[FromQuery] ReminderSort reminderSort,
		[FromServices] IQueryHandler<GetRemindersPageQuery, PageResult<ReminderResult>> handler,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var query = new GetRemindersPageQuery(userId, reminderFilter, reminderSearch, pageOptions, reminderSort);
		var reminders = await handler.HandleAsync(query, ct);

		return TypedResults.Ok(reminders.Map(ApiMapper.ToResponse));
	}

	[HttpGet("{reminderId:int}")]
	public async Task<Ok<ReminderResponse>> GetReminderById(
		[FromRoute, Range(1, int.MaxValue)] int reminderId,
		[FromServices] IQueryHandler<GetReminderQuery, ReminderResult> handler,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var query = new GetReminderQuery(userId, reminderId);
		var reminder = await handler.HandleAsync(query, ct);

		return TypedResults.Ok(ApiMapper.ToResponse(reminder));
	}

	[HttpPut("{reminderId:int}")]
	public async Task<NoContent> UpdateReminder(
		[FromRoute, Range(1, int.MaxValue)] int reminderId,
		[FromBody, Required] UpdateReminderRequest request,
		[FromServices] ICommandHandler<UpdateReminderCommand> handler,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var command = new UpdateReminderCommand(userId, reminderId, request.Text, request.RemindAt);
		await handler.HandleAsync(command, ct);

		return TypedResults.NoContent();
	}

	[HttpDelete("{reminderId:int}")]
	public async Task<NoContent> DeleteReminder(
		[FromRoute, Range(1, int.MaxValue)] int reminderId,
		[FromServices] ICommandHandler<DeleteReminderCommand> handler,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var command = new DeleteReminderCommand(userId, reminderId);
		await handler.HandleAsync(command, ct);

		return TypedResults.NoContent();
	}
}
