using Domain.Users.Entities;
using Application.Contracts.Dto.Requests.Trade;
using Application.Interfaces.Utils;
using Application.Trades.Commands;
using Application.Trades.Queries;
using Domain.Models.Filters;
using Domain.Models.Pagination;
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
public class TradeController(
	ISender sender,
	IJwtHelper jwtHelper) : ControllerBase
{
	private readonly ISender _sender = sender;
	private readonly IJwtHelper _jwtHelper = jwtHelper;

	[HttpGet("statistics")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<Ok<GlobalStatisticResponse>> GetTradeStatistics(CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var query = new GetGlobalStatisticQuery(userId);
		var tradeStatistics = await _sender.Send(query, cancellationToken);
		return TypedResults.Ok(tradeStatistics.Adapt<GlobalStatisticResponse>());
	}

	[HttpGet("byuser")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<Ok<PagedResult<TradeResponse>>> GetUserTrades(
		[FromQuery] TradeFilterRequest? filterRequest,
		[FromQuery] PaginationRequest? paginationRequest,
		CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var query = new GetTradesPagedQuery(userId, filterRequest, paginationRequest);
		var userTrades = await _sender.Send(query, cancellationToken);
		return TypedResults.Ok(userTrades.Map(t => t.Adapt<TradeResponse>()));
	}

	[HttpGet("{id}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<Ok<TradeResponse>> GetTradeById(
		[Required] int id,
		CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var query = new GetTradeByIdQuery(id, userId);
		var trade = await _sender.Send(query, cancellationToken);
		return TypedResults.Ok(trade.Adapt<TradeResponse>());
	}

	[HttpPost("byuser")]
	[ProducesResponseType(StatusCodes.Status201Created)]
	public async Task<Created<TradeResponse>> CreateUserTrade(
		[FromBody, Required] CreateTradeRequest request,
		CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var command = new CreateTradeCommand(userId, request.Adapt<TradeCreateDto>());
		var trade = await _sender.Send(command, cancellationToken);
		return TypedResults.Created($"/api/Trade/{trade.Id}", trade.Adapt<TradeResponse>());
	}

	[HttpPut("byuser/{id}")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public async Task<NoContent> UpdateUserTrade(
		[Required, FromRoute] int id,
		[FromBody, Required] UpdateTradeRequest request,
		CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var command = new UpdateTradeCommand(id, userId, request.Adapt<TradeCreateDto>());
		await _sender.Send(command, cancellationToken);
		return TypedResults.NoContent();
	}

	[HttpDelete("byuser/{id}")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public async Task<NoContent> DeleteUserTrade(
		[Required, FromRoute] int id,
		CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var command = new DeleteTradeCommand(id, userId);
		await _sender.Send(command, cancellationToken);
		return TypedResults.NoContent();
	}
}
