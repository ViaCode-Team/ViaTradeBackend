using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ViaTrade.Api.Attributes.Binding;
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
	public async Task<Ok<SignalStatisticsResult>> GetStatistics(
		[FromServices] IQueryHandler<GetSignalStatisticsQuery, SignalStatisticsResult> handler,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);

		var signalStatistics = await handler.HandleAsync(new GetSignalStatisticsQuery(userId), ct);

		return TypedResults.Ok(signalStatistics);
	}

	[HttpGet("latest")]
	public async Task<Ok<PageResult<SignalResult>>> GetLatestSignals(
		[FromQuery, IgnoreProperties(nameof(GetLatestSignalsPageQuery.UserId))] GetLatestSignalsPageQuery query,
		[FromServices] IQueryHandler<GetLatestSignalsPageQuery, PageResult<SignalResult>> handler,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);

		query = query with { UserId = userId };
		var signals = await handler.HandleAsync(query, ct);

		return TypedResults.Ok(signals);
	}

	[HttpGet]
	public async Task<Ok<PageResult<SignalResult>>> GetSignals(
		[FromQuery, IgnoreProperties(nameof(GetSignalHistoryPageQuery.UserId))] GetSignalHistoryPageQuery query,
		[FromServices] IQueryHandler<GetSignalHistoryPageQuery, PageResult<SignalResult>> handler,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);

		query = query with { UserId = userId };
		var signals = await handler.HandleAsync(query, ct);

		return TypedResults.Ok(signals);
	}
}
