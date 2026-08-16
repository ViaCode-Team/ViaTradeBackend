using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ViaTrade.Api.Contracts.Signals;
using ViaTrade.Api.Contracts.Statistics;
using ViaTrade.Api.Mappings;
using ViaTrade.Application.Auth.Interfaces;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Trades.Interfaces;
using ViaTrade.Application.Trades.Models;

namespace ViaTrade.Api.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class SignalsController(ISignalQueryService signalQueryService, IJwtHelper jwtHelper) : ControllerBase
{
	[HttpGet("statistics")]
	public async Task<Ok<SignalStatisticResponse>> GetStatistics(CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var signalStatistics = await signalQueryService.GetStatisticsAsync(userId, ct);

		return TypedResults.Ok(ApiMapper.ToResponse(signalStatistics));
	}

	[HttpGet("latest")]
	public async Task<Ok<PageResult<SignalResponse>>> GetLatestSignals(
		[FromQuery] LatestSignalFilter filter,
		[FromQuery] SignalSort signalSort,
		[FromQuery] PageOptions pageOptions,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var signals = await signalQueryService.GetLatestPageAsync(userId, filter, signalSort, pageOptions, ct);

		return TypedResults.Ok(signals.Map(ApiMapper.ToResponse));
	}

	[HttpGet]
	public async Task<Ok<PageResult<SignalResponse>>> GetSignals(
		[FromQuery] SignalHistoryFilter signalHistoryFilter,
		[FromQuery] SignalSort signalSort,
		[FromQuery] PageOptions pageOptions,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var signals = await signalQueryService.GetPageAsync(userId, signalHistoryFilter, signalSort, pageOptions, ct);

		return TypedResults.Ok(signals.Map(ApiMapper.ToResponse));
	}
}
