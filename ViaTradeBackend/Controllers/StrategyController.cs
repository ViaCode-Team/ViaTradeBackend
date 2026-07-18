using Domain.Trades.Enums;
using Domain.Trades.Entities;
using Application.Interfaces;
using Application.Interfaces.Utils;
using Domain.Models.Filters;
using Domain.Models.Pagination;
using Domain.Models.Sort;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using ViaTradeBackend.Contracts.Statistics;

using Application.Contracts.Dto.Requests.Trade;
using ViaTradeBackend.Contracts.Strategies;

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
	public async Task<Ok<StrategyStatisticResponse>> GetStrategyStatistics(CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var response = await _strategyService.GetStrategyStatisticAsync(userId, cancellationToken);
		return TypedResults.Ok(response.Adapt<StrategyStatisticResponse>());
	}

	[HttpGet("")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<Ok<PagedResult<TradeStrategyResponse>>> GetStrategies(
		[FromQuery] StrategyFilterRequest? filterRequest,
		[FromQuery] StrategySortRequest? sortRequest,
		[FromQuery] PaginationRequest? paginationRequest,
		CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var response = await _strategyService.GetStrategiesPagedAsync(userId, filterRequest, sortRequest, paginationRequest, cancellationToken);
		return TypedResults.Ok(response.Map(x => x.Adapt<TradeStrategyResponse>()));
	}

	[HttpGet("{strategyId}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<Ok<TradeStrategyResponse>> GetStrategyById([Required] int strategyId, CancellationToken cancellationToken)
	{
		var response = await _strategyService.GetStrategyByIdAsync(strategyId, cancellationToken);
		return TypedResults.Ok(response.Adapt<TradeStrategyResponse>());
	}

	[HttpGet("byuser/instrumentslink")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<Ok<PagedResult<UserStrategyTradeCodeResponse>>> GetUserStrategyTradeCodes(
		[FromQuery] PaginationRequest paginationRequest, 
		CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var response = await _strategyService.GetUserStrategyCodesPagedAsync(userId, paginationRequest, cancellationToken);
		return TypedResults.Ok(response.Map(x => x.Adapt<UserStrategyTradeCodeResponse>()));
	}

	[HttpPost("byuser/instrumentslink")]
	[ProducesResponseType(StatusCodes.Status201Created)]
	public async Task<Created> CreateUserStrategyTradeCode(
		[FromBody, Required] CreateUserStrategyTradeCodeRequest CreateUserStrategyTradeCodeRequest,
		CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		await _strategyService.CreateUserStrategyCodeAsync(CreateUserStrategyTradeCodeRequest.Adapt<UserStrategyTradeCodeCreateDto>(), userId, cancellationToken);
		return TypedResults.Created();
	}

	[HttpDelete("byuser/instrumentslink")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public async Task<NoContent> DeleteUserStrategyTradeCode(
		[FromQuery, Required] int strategyId,
		[FromQuery, Required] int tradeCodeId,
		CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		await _strategyService.DeleteUserStrategyCodeAsync(strategyId, tradeCodeId, userId, cancellationToken);
		return TypedResults.NoContent();
	}

	[HttpGet("byuser")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<Ok<PagedResult<UserTradeStrategyResponse>>> GetUserStrategies([FromQuery] PaginationRequest paginationRequest, CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var response = await _strategyService.GetUserStrategiesPagedAsync(userId, paginationRequest, cancellationToken);
		return TypedResults.Ok(response.Map(x => x.Adapt<UserTradeStrategyResponse>()));
	}

	[HttpPost("byuser")]
	[ProducesResponseType(StatusCodes.Status201Created)]
	public async Task<Created> CreateUserStrategy([FromBody, Required] CreateUserStrategyRequest userStrategyRequest, CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		await _strategyService.CreateUserStrategyAsync(userStrategyRequest.Adapt<CreateUserStrategyCreateDto>(), userId, cancellationToken);
		return TypedResults.Created();
	}

	[HttpDelete("byuser")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public async Task<NoContent> DeleteUserStrategy([FromQuery, Required] int strategyId, CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		await _strategyService.DeleteUserStrategyAsync(strategyId, userId, cancellationToken);
		return TypedResults.NoContent();
	}
}


