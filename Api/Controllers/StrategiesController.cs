using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ViaTrade.Api.Contracts.Instruments;
using ViaTrade.Api.Contracts.Notes;
using ViaTrade.Api.Contracts.Statistics;
using ViaTrade.Api.Contracts.Strategies;
using ViaTrade.Api.Mappings;
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
	public async Task<Ok<StrategyStatisticResponse>> GetStrategyStatistics(
		[FromServices] IQueryHandler<GetStrategyStatisticsQuery, StrategyStatisticsResult> handler,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var query = new GetStrategyStatisticsQuery(userId);
		var strategyStatistics = await handler.HandleAsync(query, ct);

		return TypedResults.Ok(ApiMapper.ToResponse(strategyStatistics));
	}

	[HttpGet]
	public async Task<Ok<PageResult<StrategyResponse>>> GetStrategies(
		[FromQuery] StrategyFilter strategyFilter,
		[FromQuery] StrategySearch strategySearch,
		[FromQuery] StrategySort strategySort,
		[FromQuery] PageOptions pageOptions,
		[FromServices] IQueryHandler<GetStrategiesPageQuery, PageResult<StrategySubscriptionResult>> handler,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var query = new GetStrategiesPageQuery(userId, strategyFilter, strategySearch, strategySort, pageOptions);
		var pagedStrategies = await handler.HandleAsync(query, ct);

		return TypedResults.Ok(pagedStrategies.Map(ApiMapper.ToResponse));
	}

	[HttpGet("{strategyId:int}")]
	public async Task<Ok<StrategyResponse>> GetStrategyById(
		[FromRoute, Range(1, int.MaxValue)] int strategyId,
		[FromServices] IQueryHandler<GetStrategyQuery, StrategySubscriptionResult> handler,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var query = new GetStrategyQuery(userId, strategyId);
		var strategy = await handler.HandleAsync(query, ct);

		return TypedResults.Ok(ApiMapper.ToResponse(strategy));
	}

	[HttpGet("{strategyId:int}/instruments")]
	public async Task<Ok<PageResult<InstrumentResponse>>> GetInstrumentsByStrategy(
		[FromRoute, Range(1, int.MaxValue)] int strategyId,
		[FromQuery] StrategyInstrumentFilter instrumentFilter,
		[FromQuery] InstrumentSort instrumentSort,
		[FromQuery] PageOptions pageOptions,
		[FromServices] IQueryHandler<GetStrategyInstrumentsPageQuery, PageResult<InstrumentResult>> handler,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var query = new GetStrategyInstrumentsPageQuery(
			userId,
			strategyId,
			instrumentFilter,
			instrumentSort,
			pageOptions
		);
		var instruments = await handler.HandleAsync(query, ct);

		return TypedResults.Ok(instruments.Map(ApiMapper.ToResponse));
	}

	[HttpGet("{strategyId:int}/note")]
	public async Task<Ok<NoteResponse>> GetStrategyNote(
		[FromRoute, Range(1, int.MaxValue)] int strategyId,
		[FromServices] IQueryHandler<GetStrategyNoteQuery, NoteResult> handler,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var query = new GetStrategyNoteQuery(userId, strategyId);
		var note = await handler.HandleAsync(query, ct);

		return TypedResults.Ok(ApiMapper.ToResponse(note));
	}

	[HttpPut("{strategyId:int}/note")]
	public async Task<NoContent> UpsertStrategyNote(
		[FromRoute, Range(1, int.MaxValue)] int strategyId,
		[FromBody, Required] UpdateNoteRequest request,
		[FromServices] ICommandHandler<UpsertStrategyNoteCommand> handler,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var command = new UpsertStrategyNoteCommand(userId, strategyId, request.Text);
		await handler.HandleAsync(command, ct);

		return TypedResults.NoContent();
	}

	[HttpDelete("{strategyId:int}/note")]
	public async Task<NoContent> DeleteStrategyNote(
		[FromRoute, Range(1, int.MaxValue)] int strategyId,
		[FromServices] ICommandHandler<DeleteStrategyNoteCommand> handler,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var command = new DeleteStrategyNoteCommand(userId, strategyId);
		await handler.HandleAsync(command, ct);

		return TypedResults.NoContent();
	}

	[HttpPut("{strategyId:int}/instruments/{instrumentId:int}")]
	public async Task<NoContent> AddInstrumentToStrategy(
		[FromRoute, Range(1, int.MaxValue)] int strategyId,
		[FromRoute, Range(1, int.MaxValue)] int instrumentId,
		[FromServices] ICommandHandler<LinkStrategyInstrumentCommand> handler,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var command = new LinkStrategyInstrumentCommand(userId, strategyId, instrumentId);
		await handler.HandleAsync(command, ct);

		return TypedResults.NoContent();
	}

	[HttpDelete("{strategyId:int}/instruments/{instrumentId:int}")]
	public async Task<NoContent> DeleteInstrumentFromStrategy(
		[FromRoute, Range(1, int.MaxValue)] int strategyId,
		[FromRoute, Range(1, int.MaxValue)] int instrumentId,
		[FromServices] ICommandHandler<UnlinkStrategyInstrumentCommand> handler,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var command = new UnlinkStrategyInstrumentCommand(userId, strategyId, instrumentId);
		await handler.HandleAsync(command, ct);

		return TypedResults.NoContent();
	}

	[HttpPatch("{strategyId:int}")]
	public async Task<NoContent> UpdateStrategy(
		[FromRoute, Range(1, int.MaxValue)] int strategyId,
		[FromBody, Required] UpdateStrategyRequest request,
		[FromServices] ICommandHandler<SetStrategySubscriptionCommand> handler,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var command = new SetStrategySubscriptionCommand(userId, strategyId, request.IsSubscribed);
		await handler.HandleAsync(command, ct);

		return TypedResults.NoContent();
	}
}
