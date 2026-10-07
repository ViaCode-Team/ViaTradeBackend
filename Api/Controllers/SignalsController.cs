using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ViaTrade.Api.Routing;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Signals.Common;
using ViaTrade.Application.Signals.GetHistoryPage;
using ViaTrade.Application.Signals.GetLatestPage;
using ViaTrade.Application.Signals.GetStatistics;

namespace ViaTrade.Api.Controllers;

[Route($"{ApiRoutes.V1.Web}/[controller]")]
[ApiController]
public class SignalsController(ISender sender) : ControllerBase
{
	[HttpGet("statistics")]
	public async Task<Ok<SignalStatisticsResult>> GetStatistics(CancellationToken ct)
	{
		var signalStatistics = await sender.Send(new GetSignalStatisticsQuery(), ct);

		return TypedResults.Ok(signalStatistics);
	}

	[HttpGet("latest")]
	public async Task<Ok<PageResult<SignalResult>>> GetLatestSignals(
		[FromQuery] GetLatestSignalsPageQuery query,
		CancellationToken ct
	)
	{
		var signals = await sender.Send(query, ct);

		return TypedResults.Ok(signals);
	}

	[HttpGet]
	public async Task<Ok<PageResult<SignalResult>>> GetSignals(
		[FromQuery] GetSignalHistoryPageQuery query,
		CancellationToken ct
	)
	{
		var signals = await sender.Send(query, ct);

		return TypedResults.Ok(signals);
	}
}
