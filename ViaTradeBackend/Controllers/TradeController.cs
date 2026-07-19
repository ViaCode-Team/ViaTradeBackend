using Application.Auth.Interfaces;
using Application.Common.Models.Filters;
using Application.Common.Models.Pagination;
using Application.Trades.Commands;
using Application.Trades.Models;
using Application.Trades.Queries;
using Mapster;
using MediatR;
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
public class TradeController(ISender sender, IJwtHelper jwtHelper) : ControllerBase
{
	[HttpGet("statistics")]
	public async Task<Ok<GlobalStatisticResponse>> GetTradeStatistics(CancellationToken cancellationToken)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);

		var query = new GetGlobalStatisticQuery(userId);
		var tradeStatistics = await sender.Send(query, cancellationToken);

		return TypedResults.Ok(tradeStatistics.Adapt<GlobalStatisticResponse>());
	}

	[HttpGet("byuser")]
	public async Task<Ok<PagedResult<TradeResponse>>> GetUserTrades(
		[FromQuery] TradeFilterRequest? filterRequest,
		[FromQuery] PaginationRequest? paginationRequest,
		CancellationToken cancellationToken)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);

		var query = new GetTradesPagedQuery(userId, filterRequest, paginationRequest);
		var userTrades = await sender.Send(query, cancellationToken);

		return TypedResults.Ok(userTrades.Map(t => t.Adapt<TradeResponse>()));
	}

	[HttpGet("{id}")]
	public async Task<Ok<TradeResponse>> GetTradeById(
		[Required] int id,
		CancellationToken cancellationToken)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);

		var query = new GetTradeByIdQuery(id, userId);
		var trade = await sender.Send(query, cancellationToken);

		return TypedResults.Ok(trade.Adapt<TradeResponse>());
	}

	[HttpPost("byuser")]
	public async Task<Created<TradeResponse>> CreateUserTrade(
		[FromBody, Required] CreateTradeRequest request,
		CancellationToken cancellationToken)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);

		var command = new CreateTradeCommand(userId, request.Adapt<TradeCreateDto>());
		var trade = await sender.Send(command, cancellationToken);

		return TypedResults.Created($"/api/Trade/{trade.Id}", trade.Adapt<TradeResponse>());
	}

	[HttpPut("byuser/{id}")]
	public async Task<NoContent> UpdateUserTrade(
		[Required, FromRoute] int id,
		[FromBody, Required] UpdateTradeRequest request,
		CancellationToken cancellationToken)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);

		var command = new UpdateTradeCommand(id, userId, request.Adapt<TradeCreateDto>());
		await sender.Send(command, cancellationToken);

		return TypedResults.NoContent();
	}

	[HttpDelete("byuser/{id}")]
	public async Task<NoContent> DeleteUserTrade(
		[Required, FromRoute] int id,
		CancellationToken cancellationToken)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);

		var command = new DeleteTradeCommand(id, userId);
		await sender.Send(command, cancellationToken);

		return TypedResults.NoContent();
	}
}
