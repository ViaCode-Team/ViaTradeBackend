using System.ComponentModel.DataAnnotations;
using Application.Auth.Interfaces;
using Application.Interfaces;
using Application.Trades.Models;
using Application.Trades.Queries;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ViaTradeBackend.Contracts.Statistics;

namespace ViaTradeBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ResultsController(ITradeResultsService tradeResultsService, IJwtHelper jwtHelper) : ControllerBase
{
	[HttpGet("statistics")]
	public async Task<Ok<SignalStatisticResponse>> GetStrategyResultStatistics(CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var signalStatistics = await tradeResultsService.GetStatisticsAsync(userId, ct);

		return TypedResults.Ok(signalStatistics.Adapt<SignalStatisticResponse>());
	}

	[HttpGet("strategy")]
	public async Task<Ok<StrategyResults>> GetStrategyResults(
		[FromQuery] DateTime? startDate,
		[FromQuery] DateTime? endTime,
		[FromQuery] SignalSort sort,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var response = await tradeResultsService.GetAsync(userId, startDate, endTime, sort, ct);

		return TypedResults.Ok(response);
	}

	[HttpGet("strategy/{strategyName}/{tradeCode}")]
	public async Task<Ok<StrategyResults>> GetStrategyResultsByCode(
		[FromRoute, Required] string strategyName,
		[FromRoute, Required] string tradeCode,
		[FromQuery] DateTime? startDate,
		[FromQuery] DateTime? endTime,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var response = await tradeResultsService.GetAsync(userId, strategyName, tradeCode, startDate, endTime, ct);

		return TypedResults.Ok(response);
	}
}
