using System.ComponentModel.DataAnnotations;
using Application.Auth.Interfaces;
using Application.Interfaces;
using Application.Trades.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ViaTradeBackend.Contracts.Statistics;
using ViaTradeBackend.Mappings;

namespace ViaTradeBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ResultsController(ITradeResultsService tradeResultsService, IJwtHelper jwtHelper) : ControllerBase
{
	[HttpGet("statistics")]
	public async Task<Ok<SignalStatisticResponse>> GetStrategyResultStatistics(CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var signalStatistics = await tradeResultsService.GetStatisticsAsync(userId, ct);

		return TypedResults.Ok(ApiMapper.ToResponse(signalStatistics));
	}

	[HttpGet("strategy")]
	public async Task<Ok<StrategyResultsResponse>> GetStrategyResults(
		[FromQuery] DateTime? startDate,
		[FromQuery] DateTime? endTime,
		[FromQuery] SignalSort signalSort,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var response = await tradeResultsService.GetStrategyResultsAsync(userId, startDate, endTime, signalSort, ct);

		return TypedResults.Ok(ApiMapper.ToResponse(response));
	}

	[HttpGet("strategy/{strategyName}/{tradeCode}")]
	public async Task<Ok<StrategyResultsResponse>> GetStrategyResultsByCode(
		[FromRoute, Required] string strategyName,
		[FromRoute, Required] string tradeCode,
		[FromQuery] DateTime? startDate,
		[FromQuery] DateTime? endTime,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var response = await tradeResultsService.GetStrategyTradeCodeResultsAsync(
			userId,
			strategyName,
			tradeCode,
			startDate,
			endTime,
			ct
		);

		return TypedResults.Ok(ApiMapper.ToResponse(response));
	}
}
