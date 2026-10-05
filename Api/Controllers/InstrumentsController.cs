using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ViaTrade.Api.Contracts.Instruments;
using ViaTrade.Api.Contracts.Notes;
using ViaTrade.Api.Contracts.Reminders;
using ViaTrade.Api.Contracts.Statistics;
using ViaTrade.Api.Contracts.Strategies;
using ViaTrade.Api.Mappings;
using ViaTrade.Api.Routing;
using ViaTrade.Application.Auth.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Instruments.Common;
using ViaTrade.Application.Instruments.Get;
using ViaTrade.Application.Instruments.GetPage;
using ViaTrade.Application.Instruments.GetStatistics;
using ViaTrade.Application.Notes.Common;
using ViaTrade.Application.Notes.DeleteInstrument;
using ViaTrade.Application.Notes.GetInstrument;
using ViaTrade.Application.Notes.UpsertInstrument;
using ViaTrade.Application.Reminders.Common;
using ViaTrade.Application.Reminders.Create;
using ViaTrade.Application.Reminders.GetInstrumentPage;
using ViaTrade.Application.Strategies.Common;
using ViaTrade.Application.Strategies.GetByInstrumentPage;

namespace ViaTrade.Api.Controllers;

[Route($"{ApiRoutes.V1.Web}/[controller]")]
[ApiController]
public class InstrumentsController(IJwtHelper jwtHelper) : ControllerBase
{
	[HttpGet("statistics")]
	public async Task<Ok<InstrumentStatisticsResponse>> GetInstrumentStatistics(
		[FromServices] IQueryHandler<GetInstrumentStatisticsQuery, InstrumentStatisticsResult> handler,
		CancellationToken ct
	)
	{
		var query = new GetInstrumentStatisticsQuery();
		var instrumentStatistics = await handler.HandleAsync(query, ct);

		return TypedResults.Ok(ApiMapper.ToResponse(instrumentStatistics));
	}

	[HttpGet]
	public async Task<Ok<PageResult<InstrumentResponse>>> GetInstruments(
		[FromQuery] InstrumentFilter instrumentFilter,
		[FromQuery] InstrumentSearch instrumentSearch,
		[FromQuery] PageOptions pageOptions,
		[FromQuery] InstrumentSort instrumentSort,
		[FromServices] IQueryHandler<GetInstrumentsPageQuery, PageResult<InstrumentResult>> handler,
		CancellationToken ct
	)
	{
		var query = new GetInstrumentsPageQuery(instrumentFilter, instrumentSearch, pageOptions, instrumentSort);
		var instruments = await handler.HandleAsync(query, ct);

		return TypedResults.Ok(instruments.Map(ApiMapper.ToResponse));
	}

	[HttpGet("{instrumentId:int}")]
	public async Task<Ok<InstrumentResponse>> GetInstrumentById(
		[FromRoute, Range(1, int.MaxValue)] int instrumentId,
		[FromServices] IQueryHandler<GetInstrumentQuery, InstrumentResult> handler,
		CancellationToken ct
	)
	{
		var query = new GetInstrumentQuery(instrumentId);
		var instrument = await handler.HandleAsync(query, ct);

		return TypedResults.Ok(ApiMapper.ToResponse(instrument));
	}

	[HttpGet("{instrumentId:int}/strategies")]
	public async Task<Ok<PageResult<StrategyResponse>>> GetStrategiesByInstrument(
		[FromRoute, Range(1, int.MaxValue)] int instrumentId,
		[FromQuery] StrategyFilter strategyFilter,
		[FromQuery] StrategySort strategySort,
		[FromQuery] PageOptions pageOptions,
		[FromServices] IQueryHandler<GetInstrumentStrategiesPageQuery, PageResult<StrategySubscriptionResult>> handler,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var query = new GetInstrumentStrategiesPageQuery(
			userId,
			instrumentId,
			strategyFilter,
			strategySort,
			pageOptions
		);
		var strategies = await handler.HandleAsync(query, ct);

		return TypedResults.Ok(strategies.Map(ApiMapper.ToResponse));
	}

	[HttpGet("{instrumentId:int}/note")]
	public async Task<Ok<NoteResponse>> GetInstrumentNote(
		[FromRoute, Range(1, int.MaxValue)] int instrumentId,
		[FromServices] IQueryHandler<GetInstrumentNoteQuery, NoteResult> handler,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var query = new GetInstrumentNoteQuery(userId, instrumentId);
		var note = await handler.HandleAsync(query, ct);

		return TypedResults.Ok(ApiMapper.ToResponse(note));
	}

	[HttpPut("{instrumentId:int}/note")]
	public async Task<NoContent> UpsertInstrumentNote(
		[FromRoute, Range(1, int.MaxValue)] int instrumentId,
		[FromBody, Required] UpdateNoteRequest request,
		[FromServices] ICommandHandler<UpsertInstrumentNoteCommand> handler,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var command = new UpsertInstrumentNoteCommand(userId, instrumentId, request.Text);
		await handler.HandleAsync(command, ct);

		return TypedResults.NoContent();
	}

	[HttpDelete("{instrumentId:int}/note")]
	public async Task<NoContent> DeleteInstrumentNote(
		[FromRoute, Range(1, int.MaxValue)] int instrumentId,
		[FromServices] ICommandHandler<DeleteInstrumentNoteCommand> handler,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var command = new DeleteInstrumentNoteCommand(userId, instrumentId);
		await handler.HandleAsync(command, ct);

		return TypedResults.NoContent();
	}

	[HttpGet("{instrumentId:int}/reminders")]
	public async Task<Ok<PageResult<ReminderResponse>>> GetInstrumentReminders(
		[FromRoute, Range(1, int.MaxValue)] int instrumentId,
		[FromQuery] ReminderFilter reminderFilter,
		[FromQuery] ReminderSearch reminderSearch,
		[FromQuery] PageOptions pageOptions,
		[FromQuery] ReminderSort reminderSort,
		[FromServices] IQueryHandler<GetInstrumentRemindersPageQuery, PageResult<ReminderResult>> handler,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var query = new GetInstrumentRemindersPageQuery(
			userId,
			instrumentId,
			reminderFilter,
			reminderSearch,
			pageOptions,
			reminderSort
		);
		var reminders = await handler.HandleAsync(query, ct);

		return TypedResults.Ok(reminders.Map(ApiMapper.ToResponse));
	}

	[HttpPost("{instrumentId:int}/reminders")]
	public async Task<Created<ReminderResponse>> CreateInstrumentReminder(
		[FromRoute, Range(1, int.MaxValue)] int instrumentId,
		[FromBody, Required] CreateReminderRequest request,
		[FromServices] ICommandHandler<CreateReminderCommand, CreateReminderResult> handler,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var command = new CreateReminderCommand(userId, instrumentId, request.Text, request.RemindAt);
		var reminder = await handler.HandleAsync(command, ct);

		return TypedResults.Created($"/api/v1/reminders/{reminder.Id}", ApiMapper.ToResponse(reminder));
	}
}
