using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ViaTrade.Api.Attributes.Binding;
using ViaTrade.Api.Routing;
using ViaTrade.Application.Auth.Common.Abstractions;
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
public class StrategiesController(IJwtHelper jwtHelper) : ControllerBase
{
	[HttpGet("statistics")]
	public async Task<Ok<StrategyStatisticsResult>> GetStrategyStatistics(
		[FromServices] IQueryHandler<GetStrategyStatisticsQuery, StrategyStatisticsResult> handler,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);

		var strategyStatistics = await handler.HandleAsync(new GetStrategyStatisticsQuery(userId), ct);

		return TypedResults.Ok(strategyStatistics);
	}

	[HttpGet]
	public async Task<Ok<PageResult<StrategySubscriptionResult>>> GetStrategies(
		[FromQuery, IgnoreProperties(nameof(GetStrategiesPageQuery.UserId))] GetStrategiesPageQuery query,
		[FromServices] IQueryHandler<GetStrategiesPageQuery, PageResult<StrategySubscriptionResult>> handler,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);

		query = query with { UserId = userId };
		var pagedStrategies = await handler.HandleAsync(query, ct);

		return TypedResults.Ok(pagedStrategies);
	}

	[HttpGet("{strategyId:int}")]
	public async Task<Ok<StrategySubscriptionResult>> GetStrategyById(
		[
			FromQuery,
			IgnoreProperties(nameof(GetStrategyQuery.UserId)),
			FromRouteProperties(nameof(GetStrategyQuery.StrategyId))
		]
			GetStrategyQuery query,
		[FromServices] IQueryHandler<GetStrategyQuery, StrategySubscriptionResult> handler,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);

		query = query with { UserId = userId };
		var strategy = await handler.HandleAsync(query, ct);

		return TypedResults.Ok(strategy);
	}

	[HttpGet("{strategyId:int}/instruments")]
	public async Task<Ok<PageResult<InstrumentResult>>> GetInstrumentsByStrategy(
		[
			FromQuery,
			IgnoreProperties(nameof(GetStrategyInstrumentsPageQuery.UserId)),
			FromRouteProperties(nameof(GetStrategyInstrumentsPageQuery.StrategyId))
		]
			GetStrategyInstrumentsPageQuery query,
		[FromServices] IQueryHandler<GetStrategyInstrumentsPageQuery, PageResult<InstrumentResult>> handler,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);

		query = query with { UserId = userId };
		var instruments = await handler.HandleAsync(query, ct);

		return TypedResults.Ok(instruments);
	}

	[HttpGet("{strategyId:int}/note")]
	public async Task<Ok<NoteResult>> GetStrategyNote(
		[
			FromQuery,
			IgnoreProperties(nameof(GetStrategyNoteQuery.UserId)),
			FromRouteProperties(nameof(GetStrategyNoteQuery.StrategyId))
		]
			GetStrategyNoteQuery query,
		[FromServices] IQueryHandler<GetStrategyNoteQuery, NoteResult> handler,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);

		query = query with { UserId = userId };
		var note = await handler.HandleAsync(query, ct);

		return TypedResults.Ok(note);
	}

	[HttpPut("{strategyId:int}/note")]
	public async Task<NoContent> UpsertStrategyNote(
		[
			FromBody,
			IgnoreProperties(nameof(UpsertStrategyNoteCommand.UserId)),
			FromRouteProperties(nameof(UpsertStrategyNoteCommand.StrategyId))
		]
			UpsertStrategyNoteCommand command,
		[FromServices] ICommandHandler<UpsertStrategyNoteCommand> handler,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);

		command = command with { UserId = userId };
		await handler.HandleAsync(command, ct);

		return TypedResults.NoContent();
	}

	[HttpDelete("{strategyId:int}/note")]
	public async Task<NoContent> DeleteStrategyNote(
		[
			FromQuery,
			IgnoreProperties(nameof(DeleteStrategyNoteCommand.UserId)),
			FromRouteProperties(nameof(DeleteStrategyNoteCommand.StrategyId))
		]
			DeleteStrategyNoteCommand command,
		[FromServices] ICommandHandler<DeleteStrategyNoteCommand> handler,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);

		command = command with { UserId = userId };
		await handler.HandleAsync(command, ct);

		return TypedResults.NoContent();
	}

	[HttpPut("{strategyId:int}/instruments/{instrumentId:int}")]
	public async Task<NoContent> AddInstrumentToStrategy(
		[
			FromQuery,
			IgnoreProperties(nameof(LinkStrategyInstrumentCommand.UserId)),
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
		var userId = jwtHelper.GetUserIdFromClaims(User);

		command = command with { UserId = userId };
		await handler.HandleAsync(command, ct);

		return TypedResults.NoContent();
	}

	[HttpDelete("{strategyId:int}/instruments/{instrumentId:int}")]
	public async Task<NoContent> DeleteInstrumentFromStrategy(
		[
			FromQuery,
			IgnoreProperties(nameof(UnlinkStrategyInstrumentCommand.UserId)),
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
		var userId = jwtHelper.GetUserIdFromClaims(User);

		command = command with { UserId = userId };
		await handler.HandleAsync(command, ct);

		return TypedResults.NoContent();
	}

	[HttpPatch("{strategyId:int}")]
	public async Task<NoContent> UpdateStrategy(
		[
			FromBody,
			IgnoreProperties(nameof(SetStrategySubscriptionCommand.UserId)),
			FromRouteProperties(nameof(SetStrategySubscriptionCommand.StrategyId))
		]
			SetStrategySubscriptionCommand command,
		[FromServices] ICommandHandler<SetStrategySubscriptionCommand> handler,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);

		command = command with { UserId = userId };
		await handler.HandleAsync(command, ct);

		return TypedResults.NoContent();
	}
}
