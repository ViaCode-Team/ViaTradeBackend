using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ViaTrade.Api.ModelBinding.Attributes;
using ViaTrade.Api.Routing;
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
public class TradesController(ISender sender) : ControllerBase
{
	[HttpGet("statistics")]
	public async Task<Ok<TradeStatisticsResult>> GetTradeStatistics(CancellationToken ct)
	{
		var tradeStatistics = await sender.Send(new GetTradeStatisticsQuery(), ct);

		return TypedResults.Ok(tradeStatistics);
	}

	[HttpGet("profitChart")]
	public async Task<Ok<List<ProfitChartBucketResult>>> GetProfitChart(
		[FromQuery] GetProfitChartQuery query,
		CancellationToken ct
	)
	{
		var buckets = await sender.Send(query, ct);

		return TypedResults.Ok(buckets);
	}

	[HttpGet("profitChart/dateRange")]
	public async Task<Ok<TradeDateRangeResult>> GetTradeDateRange(CancellationToken ct)
	{
		var range = await sender.Send(new GetTradeDateRangeQuery(), ct);

		return TypedResults.Ok(range);
	}

	[HttpGet]
	public async Task<Ok<PageResult<TradeResult>>> GetTrades([FromQuery] GetTradesPageQuery query, CancellationToken ct)
	{
		var userTrades = await sender.Send(query, ct);

		return TypedResults.Ok(userTrades);
	}

	[HttpGet("{tradeId:int}")]
	public async Task<Ok<TradeResult>> GetTradeById(
		[FromQuery, FromRouteProperties(nameof(GetTradeQuery.TradeId))] GetTradeQuery query,
		CancellationToken ct
	)
	{
		var trade = await sender.Send(query, ct);

		return TypedResults.Ok(trade);
	}

	[HttpPost]
	public async Task<Created<TradeResult>> CreateTrade([FromBody] CreateTradeCommand command, CancellationToken ct)
	{
		var trade = await sender.Send(command, ct);

		return TypedResults.Created($"/api/v1/trades/{trade.Id}", trade);
	}

	[HttpPut("{tradeId:int}")]
	public async Task<NoContent> UpdateTrade(
		[FromBody, FromRouteProperties(nameof(UpdateTradeCommand.TradeId))] UpdateTradeCommand command,
		CancellationToken ct
	)
	{
		await sender.Send(command, ct);

		return TypedResults.NoContent();
	}

	[HttpDelete("{tradeId:int}")]
	public async Task<NoContent> DeleteTrade(
		[FromQuery, FromRouteProperties(nameof(DeleteTradeCommand.TradeId))] DeleteTradeCommand command,
		CancellationToken ct
	)
	{
		await sender.Send(command, ct);

		return TypedResults.NoContent();
	}
}
