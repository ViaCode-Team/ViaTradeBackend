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
public class ResultController(ITradeResultsService tradeResultsService, IJwtHelper jwtHelper) : ControllerBase
{
	[HttpGet("statistics")]
	public async Task<Ok<SignalStatisticResponse>> GetStrategyResultStatistics(CancellationToken cancellationToken)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);

		var signalStatistics = await tradeResultsService.GetStrategyResultStatisticAsync(userId, cancellationToken);

		return TypedResults.Ok(signalStatistics.Adapt<SignalStatisticResponse>());
	}

	[HttpGet("strategy")]
	public async Task<Ok<StrategyResultResponse>> GetStrategyResults(
		[FromQuery] DateTime? startDate,
		[FromQuery] DateTime? endTime,
		[FromQuery] SignalSortRequest? sortRequest,
		CancellationToken cancellationToken)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);

		var response = await tradeResultsService.GetStrategyResultAsync(userId, startDate, endTime, sortRequest, cancellationToken);

		return TypedResults.Ok(response);
	}

	[HttpGet("strategy/{strategyName}/{tradeCode}")]
	public async Task<Ok<StrategyResultResponse>> GetStrategyResultsByCode(
		[FromRoute, Required] string strategyName,
		[FromRoute, Required] string tradeCode,
		[FromQuery] DateTime? startDate,
		[FromQuery] DateTime? endTime,
		CancellationToken cancellationToken)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);

		var response = await tradeResultsService.GetStrategyResultByCodeAsync(userId, strategyName, tradeCode, startDate, endTime, cancellationToken);

		return TypedResults.Ok(response);
	}
}

