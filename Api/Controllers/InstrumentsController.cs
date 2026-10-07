using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ViaTrade.Api.ModelBinding.Attributes;
using ViaTrade.Api.Routing;
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
public class InstrumentsController(ISender sender) : ControllerBase
{
	[HttpGet("statistics")]
	public async Task<Ok<InstrumentStatisticsResult>> GetInstrumentStatistics(CancellationToken ct)
	{
		var instrumentStatistics = await sender.Send(new GetInstrumentStatisticsQuery(), ct);

		return TypedResults.Ok(instrumentStatistics);
	}

	[HttpGet]
	public async Task<Ok<PageResult<InstrumentResult>>> GetInstruments(
		[FromQuery] GetInstrumentsPageQuery query,
		CancellationToken ct
	)
	{
		var instruments = await sender.Send(query, ct);

		return TypedResults.Ok(instruments);
	}

	[HttpGet("{instrumentId:int}")]
	public async Task<Ok<InstrumentResult>> GetInstrumentById(
		[FromQuery, FromRouteProperties(nameof(GetInstrumentQuery.InstrumentId))] GetInstrumentQuery query,
		CancellationToken ct
	)
	{
		var instrument = await sender.Send(query, ct);

		return TypedResults.Ok(instrument);
	}

	[HttpGet("{instrumentId:int}/strategies")]
	public async Task<Ok<PageResult<StrategySubscriptionResult>>> GetStrategiesByInstrument(
		[FromQuery, FromRouteProperties(nameof(GetInstrumentStrategiesPageQuery.InstrumentId))]
			GetInstrumentStrategiesPageQuery query,
		CancellationToken ct
	)
	{
		var strategies = await sender.Send(query, ct);

		return TypedResults.Ok(strategies);
	}

	[HttpGet("{instrumentId:int}/note")]
	public async Task<Ok<NoteResult>> GetInstrumentNote(
		[FromQuery, FromRouteProperties(nameof(GetInstrumentNoteQuery.InstrumentId))] GetInstrumentNoteQuery query,
		CancellationToken ct
	)
	{
		var note = await sender.Send(query, ct);

		return TypedResults.Ok(note);
	}

	[HttpPut("{instrumentId:int}/note")]
	public async Task<NoContent> UpsertInstrumentNote(
		[FromBody, FromRouteProperties(nameof(UpsertInstrumentNoteCommand.InstrumentId))]
			UpsertInstrumentNoteCommand command,
		CancellationToken ct
	)
	{
		await sender.Send(command, ct);

		return TypedResults.NoContent();
	}

	[HttpDelete("{instrumentId:int}/note")]
	public async Task<NoContent> DeleteInstrumentNote(
		[FromQuery, FromRouteProperties(nameof(DeleteInstrumentNoteCommand.InstrumentId))]
			DeleteInstrumentNoteCommand command,
		CancellationToken ct
	)
	{
		await sender.Send(command, ct);

		return TypedResults.NoContent();
	}

	[HttpGet("{instrumentId:int}/reminders")]
	public async Task<Ok<PageResult<ReminderResult>>> GetInstrumentReminders(
		[FromQuery, FromRouteProperties(nameof(GetInstrumentRemindersPageQuery.InstrumentId))]
			GetInstrumentRemindersPageQuery query,
		CancellationToken ct
	)
	{
		var reminders = await sender.Send(query, ct);

		return TypedResults.Ok(reminders);
	}

	[HttpPost("{instrumentId:int}/reminders")]
	public async Task<Created<ReminderResult>> CreateInstrumentReminder(
		[FromBody, FromRouteProperties(nameof(CreateReminderCommand.InstrumentId))] CreateReminderCommand command,
		CancellationToken ct
	)
	{
		var reminder = await sender.Send(command, ct);

		return TypedResults.Created($"/api/v1/reminders/{reminder.Id}", reminder);
	}
}
