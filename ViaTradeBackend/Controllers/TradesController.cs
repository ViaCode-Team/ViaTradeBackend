using System.ComponentModel.DataAnnotations;
using Application.Auth.Interfaces;
using Application.Common.Models;
using Application.Trades.Interfaces;
using Application.Trades.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ViaTradeBackend.Contracts.Statistics;
using ViaTradeBackend.Contracts.Trades;
using ViaTradeBackend.Mappings;

namespace ViaTradeBackend.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class TradesController(
	ITradeCommandService tradeCommandService,
	ITradeQueryService tradeQueryService,
	IJwtHelper jwtHelper
) : ControllerBase
{
	[HttpGet("statistics")]
	public async Task<Ok<GlobalStatisticResponse>> GetTradeStatistics(CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var tradeStatistics = await tradeQueryService.GetStatisticsAsync(userId, ct);

		return TypedResults.Ok(ApiMapper.ToResponse(tradeStatistics));
	}

	[HttpGet("profitChart")]
	public async Task<Ok<List<ProfitChartBucketResponse>>> GetProfitChart(
		[FromQuery] ProfitChartFilter filter,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var buckets = await tradeQueryService.GetProfitChartAsync(userId, filter, ct);

		return TypedResults.Ok(buckets.Select(ApiMapper.ToResponse).ToList());
	}

	[HttpGet("profitChart/dateRange")]
	public async Task<Ok<TradeDateRangeResponse>> GetTradeDateRange(CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var range = await tradeQueryService.GetTradeDateRangeAsync(userId, ct);

		return TypedResults.Ok(ApiMapper.ToResponse(range));
	}

	[HttpGet]
	public async Task<Ok<PageResult<TradeResponse>>> GetTrades(
		[FromQuery] TradeFilter tradeFilter,
		[FromQuery] PageOptions pageOptions,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var userTrades = await tradeQueryService.GetPageAsync(userId, tradeFilter, pageOptions, ct);

		return TypedResults.Ok(userTrades.Map(ApiMapper.ToResponse));
	}

	[HttpGet("{tradeId:int}")]
	public async Task<Ok<TradeResponse>> GetTradeById(
		[FromRoute, Range(1, int.MaxValue)] int tradeId,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var trade = await tradeQueryService.GetAsync(userId, tradeId, ct);

		return TypedResults.Ok(ApiMapper.ToResponse(trade));
	}

	[HttpPost]
	public async Task<Created<TradeResponse>> CreateTrade(
		[FromBody, Required] CreateTradeRequest request,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var trade = await tradeCommandService.CreateAsync(userId, ApiMapper.ToInput(request), ct);

		return TypedResults.Created($"/api/v1/trades/{trade.Id}", ApiMapper.ToResponse(trade));
	}

	[HttpPut("{tradeId:int}")]
	public async Task<NoContent> UpdateTrade(
		[FromRoute, Range(1, int.MaxValue)] int tradeId,
		[FromBody, Required] UpdateTradeRequest request,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		await tradeCommandService.UpdateAsync(userId, tradeId, ApiMapper.ToInput(request), ct);

		return TypedResults.NoContent();
	}

	[HttpDelete("{tradeId:int}")]
	public async Task<NoContent> DeleteTrade([FromRoute, Range(1, int.MaxValue)] int tradeId, CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		await tradeCommandService.DeleteAsync(userId, tradeId, ct);

		return TypedResults.NoContent();
	}
}
