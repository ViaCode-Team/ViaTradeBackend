using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ViaTrade.Api.ModelBinding.Attributes;
using ViaTrade.Api.Routing;
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
public class TradesController : ControllerBase
{
	[HttpGet("statistics")]
	public async Task<Ok<TradeStatisticsResult>> GetTradeStatistics(
		[FromServices] IQueryHandler<GetTradeStatisticsQuery, TradeStatisticsResult> handler,
		CancellationToken ct
	)
	{
		var tradeStatistics = await handler.HandleAsync(new GetTradeStatisticsQuery(), ct);

		return TypedResults.Ok(tradeStatistics);
	}

	[HttpGet("profitChart")]
	public async Task<Ok<List<ProfitChartBucketResult>>> GetProfitChart(
		[FromQuery] GetProfitChartQuery query,
		[FromServices] IQueryHandler<GetProfitChartQuery, List<ProfitChartBucketResult>> handler,
		CancellationToken ct
	)
	{
		var buckets = await handler.HandleAsync(query, ct);

		return TypedResults.Ok(buckets);
	}

	[HttpGet("profitChart/dateRange")]
	public async Task<Ok<TradeDateRangeResult>> GetTradeDateRange(
		[FromServices] IQueryHandler<GetTradeDateRangeQuery, TradeDateRangeResult> handler,
		CancellationToken ct
	)
	{
		var range = await handler.HandleAsync(new GetTradeDateRangeQuery(), ct);

		return TypedResults.Ok(range);
	}

	[HttpGet]
	public async Task<Ok<PageResult<TradeResult>>> GetTrades(
		[FromQuery] GetTradesPageQuery query,
		[FromServices] IQueryHandler<GetTradesPageQuery, PageResult<TradeResult>> handler,
		CancellationToken ct
	)
	{
		var userTrades = await handler.HandleAsync(query, ct);

		return TypedResults.Ok(userTrades);
	}

	[HttpGet("{tradeId:int}")]
	public async Task<Ok<TradeResult>> GetTradeById(
		[FromQuery, FromRouteProperties(nameof(GetTradeQuery.TradeId))] GetTradeQuery query,
		[FromServices] IQueryHandler<GetTradeQuery, TradeResult> handler,
		CancellationToken ct
	)
	{
		var trade = await handler.HandleAsync(query, ct);

		return TypedResults.Ok(trade);
	}

	[HttpPost]
	public async Task<Created<TradeResult>> CreateTrade(
		[FromBody] CreateTradeCommand command,
		[FromServices] ICommandHandler<CreateTradeCommand, TradeResult> handler,
		CancellationToken ct
	)
	{
		var trade = await handler.HandleAsync(command, ct);

		return TypedResults.Created($"/api/v1/trades/{trade.Id}", trade);
	}

	[HttpPut("{tradeId:int}")]
	public async Task<NoContent> UpdateTrade(
		[FromBody, FromRouteProperties(nameof(UpdateTradeCommand.TradeId))] UpdateTradeCommand command,
		[FromServices] ICommandHandler<UpdateTradeCommand> handler,
		CancellationToken ct
	)
	{
		await handler.HandleAsync(command, ct);

		return TypedResults.NoContent();
	}

	[HttpDelete("{tradeId:int}")]
	public async Task<NoContent> DeleteTrade(
		[FromQuery, FromRouteProperties(nameof(DeleteTradeCommand.TradeId))] DeleteTradeCommand command,
		[FromServices] ICommandHandler<DeleteTradeCommand> handler,
		CancellationToken ct
	)
	{
		await handler.HandleAsync(command, ct);

		return TypedResults.NoContent();
	}
}
