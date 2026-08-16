using System.ComponentModel.DataAnnotations;
using Application.Auth.Interfaces;
using Application.Common.Models;
using Application.Instruments.Models;
using Application.Notes.Interfaces;
using Application.Strategies.Interfaces;
using Application.Strategies.Models;
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
		[FromQuery] StrategyInstrumentFilter instrumentFilter,
		[FromQuery] InstrumentSort instrumentSort,
		[FromQuery] PageOptions pageOptions,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var instruments = await strategyQueryService.GetInstrumentsByStrategyPageAsync(
			userId,
			strategyId,
			instrumentFilter,
			instrumentSort,
			pageOptions,
			ct
		);

		return TypedResults.Ok(instruments.Map(ApiMapper.ToResponse));
	}

	[HttpGet("{strategyId:int}/note")]
	public async Task<Ok<NoteResponse>> GetStrategyNote(
		[FromRoute, Range(1, int.MaxValue)] int strategyId,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var note = await noteQueryService.GetStrategyAsync(userId, strategyId, ct);

		return TypedResults.Ok(ApiMapper.ToResponse(note));
	}

	[HttpPut("{strategyId:int}/note")]
	public async Task<NoContent> UpsertStrategyNote(
		[FromRoute, Range(1, int.MaxValue)] int strategyId,
		[FromBody, Required] UpdateNoteRequest request,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		await noteCommandService.UpsertStrategyAsync(userId, strategyId, request.Text, ct);

		return TypedResults.NoContent();
	}

	[HttpDelete("{strategyId:int}/note")]
	public async Task<NoContent> DeleteStrategyNote(
		[FromRoute, Range(1, int.MaxValue)] int strategyId,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		await noteCommandService.DeleteStrategyAsync(userId, strategyId, ct);

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

	[HttpPatch("{strategyId:int}")]
	public async Task<NoContent> UpdateStrategy(
		[FromRoute, Range(1, int.MaxValue)] int strategyId,
		[FromBody, Required] UpdateStrategyRequest request,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		await strategyCommandService.SetSubscriptionAsync(userId, strategyId, request.IsSubscribed, ct);

		return TypedResults.NoContent();
	}
}
