using Application.Contracts.Dto.Requests.Trade;
using Application.Interfaces;
using Application.Interfaces.Utils;
using Domain.Models.Filters;
using Domain.Models.Pagination;
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
public class TradeController(
	ITradeService tradeService,
	IJwtHelper jwtHelper) : ControllerBase
{
	private readonly ITradeService _tradeService = tradeService;
	private readonly IJwtHelper _jwtHelper = jwtHelper;

	[HttpGet("statistics")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<Ok<GlobalStatisticResponse>> GetTradeStatistics(CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var tradeStatistics = await _tradeService.GetGlobalStatisticAsync(userId, cancellationToken);
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
		var userTrades = await _tradeService.GetByUserPagedAsync(userId, filterRequest, paginationRequest, cancellationToken);
		return TypedResults.Ok(userTrades.Map(t => t.Adapt<TradeResponse>()));
	}

	[HttpGet("{id}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<Ok<TradeResponse>> GetTradeById(
		[Required] int id,
		CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var trade = await _tradeService.GetTradeByIdAsync(id, userId, cancellationToken);
		return TypedResults.Ok(trade.Adapt<TradeResponse>());
	}

	[HttpPost("byuser")]
	[ProducesResponseType(StatusCodes.Status201Created)]
	public async Task<Created<TradeResponse>> CreateUserTrade(
		[FromBody, Required] CreateTradeRequest request,
		CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var trade = await _tradeService.CreateTradeAsync(request.Adapt<TradeCreateDto>(), userId, cancellationToken);
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
		await _tradeService.UpdateTradeAsync(id, request.Adapt<TradeCreateDto>(), userId, cancellationToken);
		return TypedResults.NoContent();
	}

	[HttpDelete("byuser/{id}")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public async Task<NoContent> DeleteUserTrade(
		[Required, FromRoute] int id,
		CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		await _tradeService.DeleteTradeAsync(id, userId, cancellationToken);
		return TypedResults.NoContent();
	}
}





