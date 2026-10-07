using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ViaTrade.Api.ModelBinding.Attributes;
using ViaTrade.Api.Routing;
using ViaTrade.Application.Common.Abstractions;
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
public class StrategiesController : ControllerBase
{
	[HttpGet("statistics")]
	public async Task<Ok<StrategyStatisticsResult>> GetStrategyStatistics(
		[FromServices] IQueryHandler<GetStrategyStatisticsQuery, StrategyStatisticsResult> handler,
		CancellationToken ct
	)
	{
		var strategyStatistics = await handler.HandleAsync(new GetStrategyStatisticsQuery(), ct);

		return TypedResults.Ok(strategyStatistics);
	}

	[HttpGet]
	public async Task<Ok<PageResult<StrategySubscriptionResult>>> GetStrategies(
		[FromQuery] GetStrategiesPageQuery query,
		[FromServices] IQueryHandler<GetStrategiesPageQuery, PageResult<StrategySubscriptionResult>> handler,
		CancellationToken ct
	)
	{
		var pagedStrategies = await handler.HandleAsync(query, ct);

		return TypedResults.Ok(pagedStrategies);
	}

	[HttpGet("{strategyId:int}")]
	public async Task<Ok<StrategySubscriptionResult>> GetStrategyById(
		[FromQuery, FromRouteProperties(nameof(GetStrategyQuery.StrategyId))] GetStrategyQuery query,
		[FromServices] IQueryHandler<GetStrategyQuery, StrategySubscriptionResult> handler,
		CancellationToken ct
	)
	{
		var strategy = await handler.HandleAsync(query, ct);

		return TypedResults.Ok(strategy);
	}

	[HttpGet("{strategyId:int}/instruments")]
	public async Task<Ok<PageResult<InstrumentResult>>> GetInstrumentsByStrategy(
		[FromQuery, FromRouteProperties(nameof(GetStrategyInstrumentsPageQuery.StrategyId))]
			GetStrategyInstrumentsPageQuery query,
		[FromServices] IQueryHandler<GetStrategyInstrumentsPageQuery, PageResult<InstrumentResult>> handler,
		CancellationToken ct
	)
	{
		var instruments = await handler.HandleAsync(query, ct);

		return TypedResults.Ok(instruments);
	}

	[HttpGet("{strategyId:int}/note")]
	public async Task<Ok<NoteResult>> GetStrategyNote(
		[FromQuery, FromRouteProperties(nameof(GetStrategyNoteQuery.StrategyId))] GetStrategyNoteQuery query,
		[FromServices] IQueryHandler<GetStrategyNoteQuery, NoteResult> handler,
		CancellationToken ct
	)
	{
		var note = await handler.HandleAsync(query, ct);

		return TypedResults.Ok(note);
	}

	[HttpPut("{strategyId:int}/note")]
	public async Task<NoContent> UpsertStrategyNote(
		[FromBody, FromRouteProperties(nameof(UpsertStrategyNoteCommand.StrategyId))] UpsertStrategyNoteCommand command,
		[FromServices] ICommandHandler<UpsertStrategyNoteCommand> handler,
		CancellationToken ct
	)
	{
		await handler.HandleAsync(command, ct);

		return TypedResults.NoContent();
	}

	[HttpDelete("{strategyId:int}/note")]
	public async Task<NoContent> DeleteStrategyNote(
		[FromQuery, FromRouteProperties(nameof(DeleteStrategyNoteCommand.StrategyId))]
			DeleteStrategyNoteCommand command,
		[FromServices] ICommandHandler<DeleteStrategyNoteCommand> handler,
		CancellationToken ct
	)
	{
		await handler.HandleAsync(command, ct);

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
		[FromServices] ICommandHandler<LinkStrategyInstrumentCommand> handler,
		CancellationToken ct
	)
	{
		await handler.HandleAsync(command, ct);

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
		[FromServices] ICommandHandler<UnlinkStrategyInstrumentCommand> handler,
		CancellationToken ct
	)
	{
		await handler.HandleAsync(command, ct);

		return TypedResults.NoContent();
	}

	[HttpPatch("{strategyId:int}")]
	public async Task<NoContent> UpdateStrategy(
		[FromBody, FromRouteProperties(nameof(SetStrategySubscriptionCommand.StrategyId))]
			SetStrategySubscriptionCommand command,
		[FromServices] ICommandHandler<SetStrategySubscriptionCommand> handler,
		CancellationToken ct
	)
	{
		await handler.HandleAsync(command, ct);

		return TypedResults.NoContent();
	}
}
