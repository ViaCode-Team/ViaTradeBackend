using Application.Auth.Interfaces;
using Application.Common.Models.Filters;
using Application.Common.Models.Pagination;
using Application.Common.Models.Sort;
using Application.Strategies.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using ViaTradeBackend.Contracts.Statistics;
using ViaTradeBackend.Contracts.Strategies;

namespace ViaTradeBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class StrategiesController(
	IStrategyCommandService strategyCommandService,
	IStrategyQueryService strategyQueryService,
	IJwtHelper jwtHelper) : ControllerBase
{
	[HttpGet("statistics")]
	public async Task<Ok<StrategyStatisticResponse>> GetStrategyStatistics(CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var strategyStatistics = await strategyQueryService.GetStatisticsAsync(userId, ct);

		return TypedResults.Ok(strategyStatistics.Adapt<StrategyStatisticResponse>());
	}

	[HttpGet]
	public async Task<Ok<PagedResult<TradeStrategyResponse>>> GetStrategies(
		[FromQuery] StrategyFilterRequest? filterRequest,
		[FromQuery] StrategySortRequest? sortRequest,
		[FromQuery] PaginationRequest? paginationRequest,
		CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var pagedStrategies = await strategyQueryService.GetAsync(userId, filterRequest, sortRequest, paginationRequest, ct);

		return TypedResults.Ok(pagedStrategies.Map(s => s.Adapt<TradeStrategyResponse>()));
	}

	[HttpGet("{id}")]
	public async Task<Ok<TradeStrategyResponse>> GetStrategyById([FromRoute, Required] int id, CancellationToken ct)
	{
		var strategy = await strategyQueryService.GetAsync(id, ct);
		return TypedResults.Ok(strategy.Adapt<TradeStrategyResponse>());
	}

	[HttpGet("byuser")]
	public async Task<Ok<PagedResult<UserTradeStrategyResponse>>> GetUserStrategies(
		[FromQuery] PaginationRequest paginationRequest,
		CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var userStrategies = await strategyQueryService.GetUserLinkedAsync(userId, paginationRequest, ct);

		return TypedResults.Ok(userStrategies.Map(s => s.Adapt<UserTradeStrategyResponse>()));
	}

	[HttpGet("codes/byuser")]
	public async Task<Ok<PagedResult<UserStrategyTradeCodeResponse>>> GetUserStrategyCodes(
		[FromQuery] PaginationRequest paginationRequest,
		CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var userStrategyCodes = await strategyQueryService.GetUserLinkedCodesAsync(userId, paginationRequest, ct);

		return TypedResults.Ok(userStrategyCodes.Map(s => s.Adapt<UserStrategyTradeCodeResponse>()));
	}

	[HttpPost("codes/byuser")]
	public async Task<Created> CreateUserStrategyCode([FromBody, Required] CreateUserStrategyTradeCodeRequest userStrategyCodeRequest, CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		await strategyCommandService.CreateCodeAsync(userId, userStrategyCodeRequest.StrategyId, userStrategyCodeRequest.TradeCodeId, ct);

		return TypedResults.Created();
	}

	[HttpDelete("codes/byuser")]
	public async Task<NoContent> DeleteUserStrategyCode(
		[FromQuery, Required] int strategyId,
		[FromQuery, Required] int tradeCodeId,
		CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		await strategyCommandService.DeleteCodeAsync(userId, strategyId, tradeCodeId, ct);

		return TypedResults.NoContent();
	}

	[HttpPost("byuser")]
	public async Task<Created> CreateUserStrategy([FromBody, Required] CreateUserStrategyRequest userStrategyRequest, CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		await strategyCommandService.CreateAsync(userId, userStrategyRequest.StrategyId, ct);

		return TypedResults.Created();
	}

	[HttpDelete("byuser")]
	public async Task<NoContent> DeleteUserStrategy([FromQuery, Required] int strategyId, CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		await strategyCommandService.DeleteAsync(userId, strategyId, ct);

		return TypedResults.NoContent();
	}
}
