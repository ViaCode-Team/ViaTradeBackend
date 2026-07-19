using Application.Auth.Interfaces;
using Application.Common.Models.Sort;
using Application.Interfaces;
using Domain.Trades.Entities;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
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
	public async Task<Ok<StrategyResultResponse>> GetStrategyResults(
		[FromQuery] DateTime? startDate,
		[FromQuery] DateTime? endTime,
		[FromQuery] SignalSortRequest? sortRequest,
		CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var response = await tradeResultsService.GetAsync(userId, startDate, endTime, sortRequest, ct);

		return TypedResults.Ok(response);
	}

	[HttpGet("strategy/{strategyName}/{tradeCode}")]
	public async Task<Ok<StrategyResultResponse>> GetStrategyResultsByCode(
		[FromRoute, Required] string strategyName,
		[FromRoute, Required] string tradeCode,
		[FromQuery] DateTime? startDate,
		[FromQuery] DateTime? endTime,
		CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var response = await tradeResultsService.GetAsync(userId, strategyName, tradeCode, startDate, endTime, ct);

		return TypedResults.Ok(response);
	}
}

