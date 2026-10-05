using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ViaTrade.Api.Contracts.Statistics;
using ViaTrade.Api.Contracts.Trades;
using ViaTrade.Api.Mappings;
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
	public async Task<Ok<GlobalStatisticResponse>> GetTradeStatistics(
		[FromServices] IQueryHandler<GetTradeStatisticsQuery, TradeStatisticsResult> handler,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var query = new GetTradeStatisticsQuery(userId);
		var tradeStatistics = await handler.HandleAsync(query, ct);

		return TypedResults.Ok(ApiMapper.ToResponse(tradeStatistics));
	}

	[HttpGet("profitChart")]
	public async Task<Ok<List<ProfitChartBucketResponse>>> GetProfitChart(
		[FromQuery] ProfitChartFilter profitChartFilter,
		[FromServices] IQueryHandler<GetProfitChartQuery, List<ProfitChartBucketResult>> handler,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var query = new GetProfitChartQuery(userId, profitChartFilter);
		var buckets = await handler.HandleAsync(query, ct);

		return TypedResults.Ok(buckets.Select(ApiMapper.ToResponse).ToList());
	}

	[HttpGet("profitChart/dateRange")]
	public async Task<Ok<TradeDateRangeResponse>> GetTradeDateRange(
		[FromServices] IQueryHandler<GetTradeDateRangeQuery, TradeDateRangeResult> handler,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var query = new GetTradeDateRangeQuery(userId);
		var range = await handler.HandleAsync(query, ct);

		return TypedResults.Ok(ApiMapper.ToResponse(range));
	}

	[HttpGet]
	public async Task<Ok<PageResult<TradeResponse>>> GetTrades(
		[FromQuery] TradeFilter tradeFilter,
		[FromQuery] TradeSearch tradeSearch,
		[FromQuery] PageOptions pageOptions,
		[FromServices] IQueryHandler<GetTradesPageQuery, PageResult<TradeResult>> handler,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var query = new GetTradesPageQuery(userId, tradeFilter, tradeSearch, pageOptions);
		var userTrades = await handler.HandleAsync(query, ct);

		return TypedResults.Ok(userTrades.Map(ApiMapper.ToResponse));
	}

	[HttpGet("{tradeId:int}")]
	public async Task<Ok<TradeResponse>> GetTradeById(
		[FromRoute, Range(1, int.MaxValue)] int tradeId,
		[FromServices] IQueryHandler<GetTradeQuery, TradeResult> handler,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var query = new GetTradeQuery(userId, tradeId);
		var trade = await handler.HandleAsync(query, ct);

		return TypedResults.Ok(ApiMapper.ToResponse(trade));
	}

	[HttpPost]
	public async Task<Created<TradeResponse>> CreateTrade(
		[FromBody, Required] CreateTradeRequest request,
		[FromServices] ICommandHandler<CreateTradeCommand, TradeResult> handler,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var command = new CreateTradeCommand(userId, ApiMapper.ToInput(request));
		var trade = await handler.HandleAsync(command, ct);

		return TypedResults.Created($"/api/v1/trades/{trade.Id}", ApiMapper.ToResponse(trade));
	}

	[HttpPut("{tradeId:int}")]
	public async Task<NoContent> UpdateTrade(
		[FromRoute, Range(1, int.MaxValue)] int tradeId,
		[FromBody, Required] UpdateTradeRequest request,
		[FromServices] ICommandHandler<UpdateTradeCommand> handler,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var command = new UpdateTradeCommand(userId, tradeId, ApiMapper.ToInput(request));
		await handler.HandleAsync(command, ct);

		return TypedResults.NoContent();
	}

	[HttpDelete("{tradeId:int}")]
	public async Task<NoContent> DeleteTrade(
		[FromRoute, Range(1, int.MaxValue)] int tradeId,
		[FromServices] ICommandHandler<DeleteTradeCommand> handler,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var command = new DeleteTradeCommand(userId, tradeId);
		await handler.HandleAsync(command, ct);

		return TypedResults.NoContent();
	}
}
