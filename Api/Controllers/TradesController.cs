using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ViaTrade.Api.Attributes.Binding;
using ViaTrade.Api.Routing;
using ViaTrade.Application.Auth.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Trades.Common;
using ViaTrade.Application.Trades.Create;
using ViaTrade.Application.Trades.Delete;
using ViaTrade.Application.Trades.Get;
using ViaTrade.Application.Trades.GetDateRange;
using ViaTrade.Application.Trades.GetPage;
using ViaTrade.Application.Trades.GetProfitChart;
using ViaTrade.Application.Trades.GetStatistics;
using ViaTrade.Application.Trades.Update;

namespace ViaTrade.Api.Controllers;

[Route($"{ApiRoutes.V1.Web}/[controller]")]
[ApiController]
public class TradesController(IJwtHelper jwtHelper) : ControllerBase
{
	[HttpGet("statistics")]
	public async Task<Ok<TradeStatisticsResult>> GetTradeStatistics(
		[FromServices] IQueryHandler<GetTradeStatisticsQuery, TradeStatisticsResult> handler,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);

		var tradeStatistics = await handler.HandleAsync(new GetTradeStatisticsQuery(userId), ct);

		return TypedResults.Ok(tradeStatistics);
	}

	[HttpGet("profitChart")]
	public async Task<Ok<List<ProfitChartBucketResult>>> GetProfitChart(
		[FromQuery, IgnoreProperties(nameof(GetProfitChartQuery.UserId))] GetProfitChartQuery query,
		[FromServices] IQueryHandler<GetProfitChartQuery, List<ProfitChartBucketResult>> handler,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);

		query = query with { UserId = userId };
		var buckets = await handler.HandleAsync(query, ct);

		return TypedResults.Ok(buckets);
	}

	[HttpGet("profitChart/dateRange")]
	public async Task<Ok<TradeDateRangeResult>> GetTradeDateRange(
		[FromServices] IQueryHandler<GetTradeDateRangeQuery, TradeDateRangeResult> handler,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);

		var range = await handler.HandleAsync(new GetTradeDateRangeQuery(userId), ct);

		return TypedResults.Ok(range);
	}

	[HttpGet]
	public async Task<Ok<PageResult<TradeResult>>> GetTrades(
		[FromQuery, IgnoreProperties(nameof(GetTradesPageQuery.UserId))] GetTradesPageQuery query,
		[FromServices] IQueryHandler<GetTradesPageQuery, PageResult<TradeResult>> handler,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);

		query = query with { UserId = userId };
		var userTrades = await handler.HandleAsync(query, ct);

		return TypedResults.Ok(userTrades);
	}

	[HttpGet("{tradeId:int}")]
	public async Task<Ok<TradeResult>> GetTradeById(
		[FromQuery, IgnoreProperties(nameof(GetTradeQuery.UserId)), FromRouteProperties(nameof(GetTradeQuery.TradeId))]
			GetTradeQuery query,
		[FromServices] IQueryHandler<GetTradeQuery, TradeResult> handler,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);

		query = query with { UserId = userId };
		var trade = await handler.HandleAsync(query, ct);

		return TypedResults.Ok(trade);
	}

	[HttpPost]
	public async Task<Created<TradeResult>> CreateTrade(
		[FromBody, IgnoreProperties(nameof(CreateTradeCommand.UserId))] CreateTradeCommand command,
		[FromServices] ICommandHandler<CreateTradeCommand, TradeResult> handler,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);

		command = command with { UserId = userId };
		var trade = await handler.HandleAsync(command, ct);

		return TypedResults.Created($"/api/v1/trades/{trade.Id}", trade);
	}

	[HttpPut("{tradeId:int}")]
	public async Task<NoContent> UpdateTrade(
		[
			FromBody,
			IgnoreProperties(nameof(UpdateTradeCommand.UserId)),
			FromRouteProperties(nameof(UpdateTradeCommand.TradeId))
		]
			UpdateTradeCommand command,
		[FromServices] ICommandHandler<UpdateTradeCommand> handler,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);

		command = command with { UserId = userId };
		await handler.HandleAsync(command, ct);

		return TypedResults.NoContent();
	}

	[HttpDelete("{tradeId:int}")]
	public async Task<NoContent> DeleteTrade(
		[
			FromQuery,
			IgnoreProperties(nameof(DeleteTradeCommand.UserId)),
			FromRouteProperties(nameof(DeleteTradeCommand.TradeId))
		]
			DeleteTradeCommand command,
		[FromServices] ICommandHandler<DeleteTradeCommand> handler,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);

		command = command with { UserId = userId };
		await handler.HandleAsync(command, ct);

		return TypedResults.NoContent();
	}
}
