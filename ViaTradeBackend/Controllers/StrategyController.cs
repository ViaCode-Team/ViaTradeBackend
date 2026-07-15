using Application.Interfaces;
using Application.Interfaces.Utils;
using Domain.Entities.DataBase;
using Domain.Models.Dto.Statistic;
using Domain.Models.Dto.Strategy;
using Domain.Models.Pagination;
using Domain.Models.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using ViaTradeBackend.Models.Trade;

namespace ViaTradeBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class StrategyController(
	IStrategyService strategyService,
	IJwtHelper jwtHelper) : ControllerBase
{
	private readonly IStrategyService _strategyService = strategyService;
	private readonly IJwtHelper _jwtHelper = jwtHelper;

	[HttpGet("statistics")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<ActionResult<StrategyStatistic>> GetStrategyStatistics(CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var response = await _strategyService.GetStrategyStatisticAsync(userId, cancellationToken);
		return Ok(response);
	}

	[HttpGet("")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<ActionResult<PagedResult<TradeStrategyDto>>> GetStrategies(
		[FromQuery] StrategyFilterRequest? filterRequest,
		[FromQuery] PaginationRequest? paginationRequest,
		CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var response = await _strategyService.GetStrategiesPagedAsync(userId, filterRequest, paginationRequest, cancellationToken);
		return Ok(response);
	}

	[HttpGet("{strategyId}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<ActionResult<TradeStrategy>> GetStrategyById([Required] int strategyId, CancellationToken cancellationToken)
	{
		var response = await _strategyService.GetStrategyByIdAsync(strategyId, cancellationToken);
		return Ok(response);
	}

	[HttpGet("byuser/instrumentslink")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<ActionResult<PagedResult<UserStrategyTradeCodeDto>>> GetUserStrategyTradeCodes([FromQuery] PaginationRequest paginationRequest, CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var response = await _strategyService.GetUserStrategyCodesPagedAsync(userId, paginationRequest, cancellationToken);
		return Ok(response);
	}

	[HttpPost("byuser/instrumentslink")]
	[ProducesResponseType(StatusCodes.Status201Created)]
	public async Task<ActionResult> CreateUserStrategyTradeCode(
		[FromBody, Required] UserStrategyTradeCodeRequest userStrategyTradeCodeRequest,
		CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		await _strategyService.CreateUserStrategyCodeAsync(userStrategyTradeCodeRequest, userId, cancellationToken);
		return Created();
	}

	[HttpDelete("byuser/instrumentslink")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public async Task<ActionResult> DeleteUserStrategyTradeCode(
		[FromQuery, Required] int strategyId,
		[FromQuery, Required] int tradeCodeId,
		CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		await _strategyService.DeleteUserStrategyCodeAsync(strategyId, tradeCodeId, userId, cancellationToken);
		return NoContent();
	}

	[HttpGet("byuser")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<ActionResult<PagedResult<UserTradeStrategyDto>>> GetUserStrategies([FromQuery] PaginationRequest paginationRequest, CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var response = await _strategyService.GetUserStrategiesPagedAsync(userId, paginationRequest, cancellationToken);
		return Ok(response);
	}

	[HttpPost("byuser")]
	[ProducesResponseType(StatusCodes.Status201Created)]
	public async Task<ActionResult> CreateUserStrategy([FromBody, Required] CreateUserStrategyRequest userStrategyRequest, CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		await _strategyService.CreateUserStrategyAsync(userStrategyRequest, userId, cancellationToken);
		return Created();
	}

	[HttpDelete("byuser")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public async Task<ActionResult> DeleteUserStrategy([FromQuery, Required] int strategyId, CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		await _strategyService.DeleteUserStrategyAsync(strategyId, userId, cancellationToken);
		return NoContent();
	}
}
