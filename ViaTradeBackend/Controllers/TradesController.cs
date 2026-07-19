using Application.Auth.Interfaces;
using Application.Common.Models.Filters;
using Application.Common.Models.Pagination;
using Application.Trades.Interfaces;
using Application.Trades.Models;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using ViaTradeBackend.Contracts.Statistics;
using ViaTradeBackend.Contracts.Trades;

namespace ViaTradeBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TradesController(
	ITradeCommandService tradeCommandService,
	ITradeQueryService tradeQueryService,
	IJwtHelper jwtHelper) : ControllerBase
{
	[HttpGet("statistics")]
	public async Task<Ok<GlobalStatisticResponse>> GetTradeStatistics(CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var tradeStatistics = await tradeQueryService.GetStatisticsAsync(userId, ct);

		return TypedResults.Ok(tradeStatistics.Adapt<GlobalStatisticResponse>());
	}

	[HttpGet("byuser")]
	public async Task<Ok<PagedResult<TradeResponse>>> GetUserTrades(
		[FromQuery] TradeFilterRequest? filterRequest,
		[FromQuery] PaginationRequest paginationRequest,
		CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var userTrades = await tradeQueryService.GetAsync(userId, filterRequest, paginationRequest, ct);

		return TypedResults.Ok(userTrades.Map(t => t.Adapt<TradeResponse>()));
	}

	[HttpGet("{id}")]
	public async Task<Ok<TradeResponse>> GetTradeById(
		[Required] int id,
		CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var trade = await tradeQueryService.GetAsync(id, userId, ct);

		return TypedResults.Ok(trade.Adapt<TradeResponse>());
	}

	[HttpPost("byuser")]
	public async Task<Created<TradeResponse>> CreateUserTrade(
		[FromBody, Required] CreateTradeRequest request,
		CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var trade = await tradeCommandService.CreateAsync(userId, request.Adapt<TradeCreateDto>(), ct);

		return TypedResults.Created($"/api/Trades/{trade.Id}", trade.Adapt<TradeResponse>());
	}

	[HttpPut("byuser/{id}")]
	public async Task<NoContent> UpdateUserTrade(
		[FromRoute, Required] int id,
		[FromBody, Required] UpdateTradeRequest request,
		CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		await tradeCommandService.UpdateAsync(id, userId, request.Adapt<TradeCreateDto>(), ct);

		return TypedResults.NoContent();
	}

	[HttpDelete("byuser/{id}")]
	public async Task<NoContent> DeleteUserTrade(
		[FromRoute, Required] int id,
		CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		await tradeCommandService.DeleteAsync(id, userId, ct);

		return TypedResults.NoContent();
	}
}
