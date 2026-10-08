using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ViaTrade.Api.ModelBinding.Attributes;
using ViaTrade.Api.Routing;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Instruments.Common;
using ViaTrade.Application.Notes.Common;
using ViaTrade.Application.Notes.DeleteStrategy;
using ViaTrade.Application.Notes.GetStrategy;
using ViaTrade.Application.Notes.UpsertStrategy;
using ViaTrade.Application.Strategies.Common;
using ViaTrade.Application.Strategies.Get;
using ViaTrade.Application.Strategies.GetInstrumentsPage;
using ViaTrade.Application.Strategies.GetPage;
using ViaTrade.Application.Strategies.GetStatistics;
using ViaTrade.Application.Strategies.LinkInstrument;
using ViaTrade.Application.Strategies.SetSubscription;
using ViaTrade.Application.Strategies.UnlinkInstrument;

namespace ViaTrade.Api.Controllers;

[Route($"{ApiRoutes.V1.Web}/[controller]")]
[ApiController]
public class StrategiesController(ISender sender) : ControllerBase
{
	[HttpGet("statistics")]
	public async Task<Ok<StrategyStatisticsResult>> GetStrategyStatistics(CancellationToken ct)
	{
		var strategyStatistics = await sender.Send(new GetStrategyStatisticsQuery(), ct);

		return TypedResults.Ok(strategyStatistics);
	}

	[HttpGet]
	public async Task<Ok<PageResult<StrategySubscriptionResult>>> GetStrategies(
		[FromQuery] GetStrategiesPageQuery query,
		CancellationToken ct
	)
	{
		var pagedStrategies = await sender.Send(query, ct);

		return TypedResults.Ok(pagedStrategies);
	}

	[HttpGet("{strategyId:int}")]
	public async Task<Ok<StrategySubscriptionResult>> GetStrategyById(
		[FromQuery, FromRouteProperties(nameof(GetStrategyQuery.StrategyId))] GetStrategyQuery query,
		CancellationToken ct
	)
	{
		var strategy = await sender.Send(query, ct);

		return TypedResults.Ok(strategy);
	}

	[HttpGet("{strategyId:int}/instruments")]
	public async Task<Ok<PageResult<InstrumentResult>>> GetInstrumentsByStrategy(
		[FromQuery, FromRouteProperties(nameof(GetStrategyInstrumentsPageQuery.StrategyId))]
			GetStrategyInstrumentsPageQuery query,
		CancellationToken ct
	)
	{
		var instruments = await sender.Send(query, ct);

		return TypedResults.Ok(instruments);
	}

	[HttpGet("{strategyId:int}/note")]
	public async Task<Ok<NoteResult>> GetStrategyNote(
		[FromQuery, FromRouteProperties(nameof(GetStrategyNoteQuery.StrategyId))] GetStrategyNoteQuery query,
		CancellationToken ct
	)
	{
		var note = await sender.Send(query, ct);

		return TypedResults.Ok(note);
	}

	[HttpPut("{strategyId:int}/note")]
	public async Task<NoContent> UpsertStrategyNote(
		[FromBody, FromRouteProperties(nameof(UpsertStrategyNoteCommand.StrategyId))] UpsertStrategyNoteCommand command,
		CancellationToken ct
	)
	{
		await sender.Send(command, ct);

		return TypedResults.NoContent();
	}

	[HttpDelete("{strategyId:int}/note")]
	public async Task<NoContent> DeleteStrategyNote(
		[FromQuery, FromRouteProperties(nameof(DeleteStrategyNoteCommand.StrategyId))]
			DeleteStrategyNoteCommand command,
		CancellationToken ct
	)
	{
		await sender.Send(command, ct);

		return TypedResults.NoContent();
	}

	[HttpPut("{strategyId:int}/instruments/{instrumentId:int}")]
	public async Task<NoContent> AddInstrumentToStrategy(
		[
			FromQuery,
			FromRouteProperties(
				nameof(LinkStrategyInstrumentCommand.StrategyId),
				nameof(LinkStrategyInstrumentCommand.InstrumentId)
			)
		]
			LinkStrategyInstrumentCommand command,
		CancellationToken ct
	)
	{
		await sender.Send(command, ct);

		return TypedResults.NoContent();
	}

	[HttpDelete("{strategyId:int}/instruments/{instrumentId:int}")]
	public async Task<NoContent> DeleteInstrumentFromStrategy(
		[
			FromQuery,
			FromRouteProperties(
				nameof(UnlinkStrategyInstrumentCommand.StrategyId),
				nameof(UnlinkStrategyInstrumentCommand.InstrumentId)
			)
		]
			UnlinkStrategyInstrumentCommand command,
		CancellationToken ct
	)
	{
		await sender.Send(command, ct);

		return TypedResults.NoContent();
	}

	[HttpPatch("{strategyId:int}")]
	public async Task<NoContent> UpdateStrategy(
		[FromBody, FromRouteProperties(nameof(SetStrategySubscriptionCommand.StrategyId))]
			SetStrategySubscriptionCommand command,
		CancellationToken ct
	)
	{
		await sender.Send(command, ct);

		return TypedResults.NoContent();
	}
}
