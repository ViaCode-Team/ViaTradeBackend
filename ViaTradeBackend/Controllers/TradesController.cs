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

[Route("api/[controller]")]
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

	[HttpGet("byuser")]
	public async Task<Ok<PageResult<TradeResponse>>> GetUserTrades(
		[FromQuery] TradeFilter tradeFilter,
		[FromQuery] PageOptions pageOptions,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var userTrades = await tradeQueryService.GetPageAsync(userId, tradeFilter, pageOptions, ct);

		return TypedResults.Ok(userTrades.Map(ApiMapper.ToResponse));
	}

	[HttpGet("{id}")]
	public async Task<Ok<TradeResponse>> GetTradeById([Required] int id, CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var trade = await tradeQueryService.GetAsync(userId, id, ct);

		return TypedResults.Ok(ApiMapper.ToResponse(trade));
	}

	[HttpPost("byuser")]
	public async Task<Created<TradeResponse>> CreateUserTrade(
		[FromBody, Required] CreateTradeRequest request,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var trade = await tradeCommandService.CreateAsync(userId, ApiMapper.ToInput(request), ct);

		return TypedResults.Created($"/api/Trades/{trade.Id}", ApiMapper.ToResponse(trade));
	}

	[HttpPut("byuser/{id}")]
	public async Task<NoContent> UpdateUserTrade(
		[FromRoute, Required] int id,
		[FromBody, Required] UpdateTradeRequest request,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		await tradeCommandService.UpdateAsync(userId, id, ApiMapper.ToInput(request), ct);

		return TypedResults.NoContent();
	}

	[HttpDelete("byuser/{id}")]
	public async Task<NoContent> DeleteUserTrade([FromRoute, Required] int id, CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		await tradeCommandService.DeleteAsync(userId, id, ct);

		return TypedResults.NoContent();
	}
}
