using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ViaTrade.Api.Contracts.Signals;
using ViaTrade.Api.Contracts.Statistics;
using ViaTrade.Api.Mappings;
using ViaTrade.Api.Routing;
using ViaTrade.Application.Auth.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Signals.Common;
using ViaTrade.Application.Signals.GetHistoryPage;
using ViaTrade.Application.Signals.GetLatestPage;
using ViaTrade.Application.Signals.GetStatistics;

namespace ViaTrade.Api.Controllers;

[Route($"{ApiRoutes.V1.Web}/[controller]")]
[ApiController]
public class SignalsController(IJwtHelper jwtHelper) : ControllerBase
{
	[HttpGet("statistics")]
	public async Task<Ok<SignalStatisticResponse>> GetStatistics(
		[FromServices] IQueryHandler<GetSignalStatisticsQuery, SignalStatisticsResult> handler,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var query = new GetSignalStatisticsQuery(userId);
		var signalStatistics = await handler.HandleAsync(query, ct);

		return TypedResults.Ok(ApiMapper.ToResponse(signalStatistics));
	}

	[HttpGet("latest")]
	public async Task<Ok<PageResult<SignalResponse>>> GetLatestSignals(
		[FromQuery] LatestSignalFilter latestSignalFilter,
		[FromQuery] SignalSort signalSort,
		[FromQuery] PageOptions pageOptions,
		[FromServices] IQueryHandler<GetLatestSignalsPageQuery, PageResult<SignalResult>> handler,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var query = new GetLatestSignalsPageQuery(userId, latestSignalFilter, signalSort, pageOptions);
		var signals = await handler.HandleAsync(query, ct);

		return TypedResults.Ok(signals.Map(ApiMapper.ToResponse));
	}

	[HttpGet]
	public async Task<Ok<PageResult<SignalResponse>>> GetSignals(
		[FromQuery] SignalHistoryFilter signalHistoryFilter,
		[FromQuery] SignalSort signalSort,
		[FromQuery] PageOptions pageOptions,
		[FromServices] IQueryHandler<GetSignalHistoryPageQuery, PageResult<SignalResult>> handler,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var query = new GetSignalHistoryPageQuery(userId, signalHistoryFilter, signalSort, pageOptions);
		var signals = await handler.HandleAsync(query, ct);

		return TypedResults.Ok(signals.Map(ApiMapper.ToResponse));
	}
}
