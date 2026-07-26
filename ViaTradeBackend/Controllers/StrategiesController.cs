using System.ComponentModel.DataAnnotations;
using Application.Auth.Interfaces;
using Application.Common.Models;
using Application.Instruments.Models;
using Application.Notes.Interfaces;
using Application.Strategies.Interfaces;
using Application.Strategies.Models;
using Domain.Enums;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ViaTradeBackend.Contracts.Instruments;
using ViaTradeBackend.Contracts.Notes;
using ViaTradeBackend.Contracts.Statistics;
using ViaTradeBackend.Contracts.Strategies;
using ViaTradeBackend.Mappings;

namespace ViaTradeBackend.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class StrategiesController(
	IStrategyCommandService strategyCommandService,
	IStrategyQueryService strategyQueryService,
	INoteCommandService noteCommandService,
	INoteQueryService noteQueryService,
	IJwtHelper jwtHelper
) : ControllerBase
{
	[HttpGet("statistics")]
	public async Task<Ok<StrategyStatisticResponse>> GetStrategyStatistics(CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var strategyStatistics = await strategyQueryService.GetStatisticsAsync(userId, ct);

		return TypedResults.Ok(ApiMapper.ToResponse(strategyStatistics));
	}

	[HttpGet]
	public async Task<Ok<PageResult<StrategyResponse>>> GetStrategies(
		[FromQuery] StrategyFilter strategyFilter,
		[FromQuery] StrategySort strategySort,
		[FromQuery] PageOptions pageOptions,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var pagedStrategies = await strategyQueryService.GetPageAsync(
			userId,
			strategyFilter,
			strategySort,
			pageOptions,
			ct
		);

		return TypedResults.Ok(pagedStrategies.Map(ApiMapper.ToResponse));
	}

	[HttpGet("{strategyId:int}")]
	public async Task<Ok<StrategyResponse>> GetStrategyById(
		[FromRoute, Range(1, int.MaxValue)] int strategyId,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var strategy = await strategyQueryService.GetAsync(userId, strategyId, ct);
		return TypedResults.Ok(ApiMapper.ToResponse(strategy));
	}

	[HttpGet("{strategyId:int}/instruments")]
	public async Task<Ok<PageResult<InstrumentResponse>>> GetInstrumentsByStrategy(
		[FromRoute, Range(1, int.MaxValue)] int strategyId,
		[FromQuery] InstrumentSort instrumentSort,
		[FromQuery] PageOptions pageOptions,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		await strategyQueryService.GetAsync(userId, strategyId, ct);
		var instruments = await strategyQueryService.GetInstrumentsByStrategyPageAsync(
			userId,
			strategyId,
			instrumentSort,
			pageOptions,
			ct
		);

		return TypedResults.Ok(instruments.Map(ApiMapper.ToResponse));
	}

	[HttpGet("{strategyId:int}/note")]
	public async Task<Ok<NoteResponse>> GetNote(
		[FromRoute, Range(1, int.MaxValue)] int strategyId,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var note = await noteQueryService.GetAsync(userId, strategyId, NoteType.StrategyNote, ct);

		return TypedResults.Ok(ApiMapper.ToResponse(note));
	}

	[HttpPut("{strategyId:int}/note")]
	public async Task<NoContent> UpsertNote(
		[FromRoute, Range(1, int.MaxValue)] int strategyId,
		[FromBody, Required] UpdateNoteRequest request,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		await noteCommandService.UpsertAsync(userId, strategyId, NoteType.StrategyNote, request.Text, ct);

		return TypedResults.NoContent();
	}

	[HttpDelete("{strategyId:int}/note")]
	public async Task<NoContent> DeleteNote([FromRoute, Range(1, int.MaxValue)] int strategyId, CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		await noteCommandService.DeleteAsync(userId, strategyId, NoteType.StrategyNote, ct);

		return TypedResults.NoContent();
	}

	[HttpPut("{strategyId:int}/instruments/{instrumentId:int}")]
	public async Task<NoContent> AddInstrumentToStrategy(
		[FromRoute, Range(1, int.MaxValue)] int strategyId,
		[FromRoute, Range(1, int.MaxValue)] int instrumentId,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		await strategyCommandService.LinkInstrumentAsync(userId, strategyId, instrumentId, ct);

		return TypedResults.NoContent();
	}

	[HttpDelete("{strategyId:int}/instruments/{instrumentId:int}")]
	public async Task<NoContent> DeleteInstrumentFromStrategy(
		[FromRoute, Range(1, int.MaxValue)] int strategyId,
		[FromRoute, Range(1, int.MaxValue)] int instrumentId,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		await strategyCommandService.UnlinkInstrumentAsync(userId, strategyId, instrumentId, ct);

		return TypedResults.NoContent();
	}

	[HttpPut("{strategyId:int}")]
	public async Task<NoContent> ActivateStrategy(
		[FromRoute, Range(1, int.MaxValue)] int strategyId,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		await strategyCommandService.ActivateAsync(userId, strategyId, ct);

		return TypedResults.NoContent();
	}

	[HttpDelete("{strategyId:int}")]
	public async Task<NoContent> DeactivateStrategy(
		[FromRoute, Range(1, int.MaxValue)] int strategyId,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		await strategyCommandService.DeactivateAsync(userId, strategyId, ct);

		return TypedResults.NoContent();
	}
}
