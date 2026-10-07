using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ViaTrade.Api.Routing;
using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Signals.Common;
using ViaTrade.Application.Signals.GetHistoryPage;
using ViaTrade.Application.Signals.GetLatestPage;
using ViaTrade.Application.Signals.GetStatistics;

namespace ViaTrade.Api.Controllers;

[Route($"{ApiRoutes.V1.Web}/[controller]")]
[ApiController]
public class SignalsController : ControllerBase
{
	[HttpGet("statistics")]
	public async Task<Ok<SignalStatisticsResult>> GetStatistics(
		[FromServices] IQueryHandler<GetSignalStatisticsQuery, SignalStatisticsResult> handler,
		CancellationToken ct
	)
	{
		var signalStatistics = await handler.HandleAsync(new GetSignalStatisticsQuery(), ct);

		return TypedResults.Ok(signalStatistics);
	}

	[HttpGet("latest")]
	public async Task<Ok<PageResult<SignalResult>>> GetLatestSignals(
		[FromQuery] GetLatestSignalsPageQuery query,
		[FromServices] IQueryHandler<GetLatestSignalsPageQuery, PageResult<SignalResult>> handler,
		CancellationToken ct
	)
	{
		var signals = await handler.HandleAsync(query, ct);

		return TypedResults.Ok(signals);
	}

	[HttpGet]
	public async Task<Ok<PageResult<SignalResult>>> GetSignals(
		[FromQuery] GetSignalHistoryPageQuery query,
		[FromServices] IQueryHandler<GetSignalHistoryPageQuery, PageResult<SignalResult>> handler,
		CancellationToken ct
	)
	{
		var signals = await handler.HandleAsync(query, ct);

		return TypedResults.Ok(signals);
	}
}
