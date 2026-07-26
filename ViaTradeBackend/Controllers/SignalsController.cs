using Application.Auth.Interfaces;
using Application.Common.Models;
using Application.Trades.Interfaces;
using Application.Trades.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ViaTradeBackend.Contracts.Signals;
using ViaTradeBackend.Contracts.Statistics;
using ViaTradeBackend.Mappings;

namespace ViaTradeBackend.Controllers;

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
		[FromQuery] PageOptions pageOptions,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var signals = await signalQueryService.GetLatestPageAsync(userId, pageOptions, ct);

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
