using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ViaTrade.Api.Contracts.Instruments;
using ViaTrade.Api.Contracts.Notes;
using ViaTrade.Api.Contracts.Reminders;
using ViaTrade.Api.Contracts.Statistics;
using ViaTrade.Api.Contracts.Strategies;
using ViaTrade.Api.Mappings;
using ViaTrade.Application.Auth.Interfaces;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Instruments.Interfaces;
using ViaTrade.Application.Instruments.Models;
using ViaTrade.Application.Notes.Interfaces;
using ViaTrade.Application.Reminders.Interfaces;
using ViaTrade.Application.Reminders.Models;
using ViaTrade.Application.Strategies.Interfaces;
using ViaTrade.Application.Strategies.Models;

namespace ViaTrade.Api.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class InstrumentsController(
	IInstrumentQueryService instrumentQueryService,
	IStrategyQueryService strategyQueryService,
	INoteCommandService noteCommandService,
	INoteQueryService noteQueryService,
	IReminderCommandService reminderCommandService,
	IReminderQueryService reminderQueryService,
	IJwtHelper jwtHelper
) : ControllerBase
{
	[HttpGet("statistics")]
	public async Task<Ok<InstrumentStatisticsResponse>> GetInstrumentStatistics(CancellationToken ct)
	{
		var instrumentStatistics = await instrumentQueryService.GetStatisticsAsync(ct);

		return TypedResults.Ok(ApiMapper.ToResponse(instrumentStatistics));
	}

	[HttpGet]
	public async Task<Ok<PageResult<InstrumentResponse>>> GetInstruments(
		[FromQuery] InstrumentFilter instrumentFilter,
		[FromQuery] InstrumentSearch instrumentSearch,
		[FromQuery] PageOptions pageOptions,
		[FromQuery] InstrumentSort instrumentSort,
		CancellationToken ct
	)
	{
		var instruments = await instrumentQueryService.GetPageAsync(
			instrumentFilter,
			instrumentSearch,
			pageOptions,
			instrumentSort,
			ct
		);

		return TypedResults.Ok(instruments.Map(ApiMapper.ToResponse));
	}

	[HttpGet("{instrumentId:int}")]
	public async Task<Ok<InstrumentResponse>> GetInstrumentById(
		[FromRoute, Range(1, int.MaxValue)] int instrumentId,
		CancellationToken ct
	)
	{
		var instrument = await instrumentQueryService.GetAsync(instrumentId, ct);

		return TypedResults.Ok(ApiMapper.ToResponse(instrument));
	}

	[HttpGet("{instrumentId:int}/strategies")]
	public async Task<Ok<PageResult<StrategyResponse>>> GetStrategiesByInstrument(
		[FromRoute, Range(1, int.MaxValue)] int instrumentId,
		[FromQuery] StrategyFilter strategyFilter,
		[FromQuery] StrategySort strategySort,
		[FromQuery] PageOptions pageOptions,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var strategies = await strategyQueryService.GetPageByInstrumentAsync(
			userId,
			instrumentId,
			strategyFilter,
			strategySort,
			pageOptions,
			ct
		);

		return TypedResults.Ok(strategies.Map(ApiMapper.ToResponse));
	}

	[HttpGet("{instrumentId:int}/note")]
	public async Task<Ok<NoteResponse>> GetInstrumentNote(
		[FromRoute, Range(1, int.MaxValue)] int instrumentId,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var note = await noteQueryService.GetInstrumentAsync(userId, instrumentId, ct);

		return TypedResults.Ok(ApiMapper.ToResponse(note));
	}

	[HttpPut("{instrumentId:int}/note")]
	public async Task<NoContent> UpsertInstrumentNote(
		[FromRoute, Range(1, int.MaxValue)] int instrumentId,
		[FromBody, Required] UpdateNoteRequest request,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		await noteCommandService.UpsertInstrumentAsync(userId, instrumentId, request.Text, ct);

		return TypedResults.NoContent();
	}

	[HttpDelete("{instrumentId:int}/note")]
	public async Task<NoContent> DeleteInstrumentNote(
		[FromRoute, Range(1, int.MaxValue)] int instrumentId,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		await noteCommandService.DeleteInstrumentAsync(userId, instrumentId, ct);

		return TypedResults.NoContent();
	}

	[HttpGet("{instrumentId:int}/reminders")]
	public async Task<Ok<PageResult<ReminderResponse>>> GetInstrumentReminders(
		[FromRoute, Range(1, int.MaxValue)] int instrumentId,
		[FromQuery] ReminderFilter reminderFilter,
		[FromQuery] ReminderSearch reminderSearch,
		[FromQuery] PageOptions pageOptions,
		[FromQuery] ReminderSort reminderSort,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var reminders = await reminderQueryService.GetPageAsync(
			userId,
			instrumentId,
			reminderFilter,
			reminderSearch,
			pageOptions,
			reminderSort,
			ct
		);

		return TypedResults.Ok(reminders.Map(ApiMapper.ToResponse));
	}

	[HttpPost("{instrumentId:int}/reminders")]
	public async Task<Created<ReminderResponse>> CreateInstrumentReminder(
		[FromRoute, Range(1, int.MaxValue)] int instrumentId,
		[FromBody, Required] CreateReminderRequest request,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var reminder = await reminderCommandService.CreateAsync(
			userId,
			instrumentId,
			request.Text,
			request.RemindAt,
			ct
		);

		return TypedResults.Created($"/api/v1/reminders/{reminder.Id}", ApiMapper.ToResponse(reminder));
	}
}
