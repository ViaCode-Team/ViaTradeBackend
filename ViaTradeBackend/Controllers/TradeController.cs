using Application.Interfaces;
using Application.Interfaces.Utils;
using Domain.Entities.DataBase;
using Domain.Models.Dto.Statistic;
using Domain.Models.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using ViaTradeBackend.Models.Trade;

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
	public async Task<ActionResult<GlobalStatistic>> GetTradeStatistics(CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var tradeStatistics = await _tradeService.GetGlobalStatisticAsync(userId, cancellationToken);
		return Ok(tradeStatistics);
	}

	[HttpGet("byuser")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<ActionResult<PagedResult<Trade>>> GetUserTrades(
		[FromQuery] PaginationRequest paginationRequest,
		[FromQuery] DateTime? startDate,
		[FromQuery] DateTime? endDate,
		[FromQuery] TradeSignal? tradeSignal,
		CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var trades = await _tradeService.GetByUserPagedAsync(userId, startDate, endDate, tradeSignal, paginationRequest, cancellationToken);
		return Ok(trades);
	}

	[HttpGet("{id}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<ActionResult<Trade>> GetTradeById(
		[Required] int id,
		CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var trade = await _tradeService.GetTradeByIdAsync(id, userId, cancellationToken);
		return Ok(trade);
	}

	[HttpPost("byuser")]
	[ProducesResponseType(StatusCodes.Status201Created)]
	public async Task<ActionResult<Trade>> CreateUserTrade(
		[FromBody, Required] TradeRequest request,
		CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var trade = await _tradeService.CreateTradeAsync(request, userId, cancellationToken);
		return CreatedAtAction(nameof(GetTradeById), new { id = trade.Id }, trade);
	}

	[HttpPut("byuser/{id}")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public async Task<ActionResult> UpdateUserTrade(
		[Required, FromRoute] int id,
		[FromBody, Required] TradeRequest request,
		CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		await _tradeService.UpdateTradeAsync(id, request, userId, cancellationToken);
		return NoContent();
	}

	[HttpDelete("byuser/{id}")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public async Task<ActionResult> DeleteUserTrade(
		[Required, FromRoute] int id,
		CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		await _tradeService.DeleteTradeAsync(id, userId, cancellationToken);
		return NoContent();
	}
}
