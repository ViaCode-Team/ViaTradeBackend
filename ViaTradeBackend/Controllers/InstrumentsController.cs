using System.ComponentModel.DataAnnotations;
using Application.Auth.Interfaces;
using Application.Common.Models;
using Application.Notes.Interfaces;
using Application.Reminders.Interfaces;
using Application.Reminders.Models;
using Application.Strategies.Interfaces;
using Application.Strategies.Models;
using Application.TradeCodes.Interfaces;
using Application.TradeCodes.Models;
using Domain.Notes.Enums;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ViaTradeBackend.Contracts.Instruments;
using ViaTradeBackend.Contracts.Notes;
using ViaTradeBackend.Contracts.Reminders;
using ViaTradeBackend.Contracts.Statistics;
using ViaTradeBackend.Contracts.Strategies;
using ViaTradeBackend.Mappings;

namespace ViaTradeBackend.Controllers;

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
		[FromQuery] PageOptions pageOptions,
		[FromQuery] InstrumentSort instrumentSort,
		CancellationToken ct
	)
	{
		var instruments = await instrumentQueryService.GetPageAsync(instrumentFilter, pageOptions, instrumentSort, ct);

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
	public async Task<Ok<NoteResponse>> GetNote(
		[FromRoute, Range(1, int.MaxValue)] int instrumentId,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var note = await noteQueryService.GetAsync(userId, instrumentId, NoteType.TradeCodeNote, ct);

		return TypedResults.Ok(ApiMapper.ToResponse(note));
	}

	[HttpPut("{instrumentId:int}/note")]
	public async Task<NoContent> UpsertNote(
		[FromRoute, Range(1, int.MaxValue)] int instrumentId,
		[FromBody, Required] UpdateNoteRequest request,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		await noteCommandService.UpsertAsync(userId, instrumentId, NoteType.TradeCodeNote, request.NoteText, ct);

		return TypedResults.NoContent();
	}

	[HttpDelete("{instrumentId:int}/note")]
	public async Task<NoContent> DeleteNote([FromRoute, Range(1, int.MaxValue)] int instrumentId, CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		await noteCommandService.DeleteAsync(userId, instrumentId, NoteType.TradeCodeNote, ct);

		return TypedResults.NoContent();
	}

	[HttpGet("{instrumentId:int}/reminders")]
	public async Task<Ok<PageResult<ReminderResponse>>> GetReminders(
		[FromRoute, Range(1, int.MaxValue)] int instrumentId,
		[FromQuery] PageOptions pageOptions,
		[FromQuery] ReminderSort reminderSort,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var reminders = await reminderQueryService.GetPageAsync(userId, instrumentId, pageOptions, reminderSort, ct);

		return TypedResults.Ok(reminders.Map(ApiMapper.ToResponse));
	}

	[HttpPost("{instrumentId:int}/reminders")]
	public async Task<Created<ReminderResponse>> CreateReminder(
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
