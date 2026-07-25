using System.ComponentModel.DataAnnotations;
using Application.Auth.Interfaces;
using Application.Common.Models;
using Application.Strategies.Interfaces;
using Application.Strategies.Models;
using Application.TradeCodes.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ViaTradeBackend.Contracts.Statistics;
using ViaTradeBackend.Contracts.Strategies;
using ViaTradeBackend.Contracts.Trades;
using ViaTradeBackend.Mappings;

namespace ViaTradeBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class StrategiesController(
	IStrategyCommandService strategyCommandService,
	IStrategyQueryService strategyQueryService,
	IJwtHelper jwtHelper
) : ControllerBase
{
	[HttpGet("statistics")]
	public async Task<Ok<StrategyStatisticResponse>> GetStrategyStatistics(CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var strategyStatistics = await strategyQueryService.GetStatisticsAsync(userId, ct);

		return TypedResults.Ok(ApiMapper.ToResponse(strategyStatistics));
	}

	[HttpGet]
	public async Task<Ok<PageResult<TradeStrategyResponse>>> GetStrategies(
		[FromQuery] StrategyFilter strategyFilter,
		[FromQuery] StrategySort strategySort,
		[FromQuery] PageOptions pageOptions,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var pagedStrategies = await strategyQueryService.GetPageAsync(
			userId,
			strategyFilter,
			strategySort,
			pageOptions,
			ct
		);

		return TypedResults.Ok(pagedStrategies.Map(ApiMapper.ToResponse));
	}

	[HttpGet("{id}")]
	public async Task<Ok<TradeStrategyResponse>> GetStrategyById([FromRoute, Required] int id, CancellationToken ct)
	{
		var strategy = await strategyQueryService.GetAsync(id, ct);
		return TypedResults.Ok(ApiMapper.ToResponse(strategy));
	}

	[HttpGet("byname/{name}")]
	public async Task<Ok<TradeStrategyResponse>> GetStrategyByName(
		[FromRoute, Required] string name,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var strategy = await strategyQueryService.GetByNameAsync(userId, name, ct);

		return TypedResults.Ok(ApiMapper.ToResponse(strategy));
	}

	[HttpGet("{strategyId}/stocks")]
	public async Task<Ok<PageResult<TradeCodeResponse>>> GetStocksByStrategy(
		[FromRoute, Required] int strategyId,
		[FromQuery] TradeCodeSort tradeCodeSort,
		[FromQuery] PageOptions pageOptions,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var tradeCodes = await strategyQueryService.GetTradeCodesByStrategyPageAsync(
			userId,
			strategyId,
			tradeCodeSort,
			pageOptions,
			ct
		);

		return TypedResults.Ok(tradeCodes.Map(ApiMapper.ToResponse));
	}

	[HttpGet("byuser")]
	public async Task<Ok<PageResult<UserTradeStrategyResponse>>> GetUserStrategies(
		[FromQuery] PageOptions pageOptions,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var userStrategies = await strategyQueryService.GetUserStrategiesPageAsync(userId, pageOptions, ct);

		return TypedResults.Ok(userStrategies.Map(ApiMapper.ToResponse));
	}

	[HttpGet("codes/byuser")]
	public async Task<Ok<PageResult<UserStrategyTradeCodeResponse>>> GetUserStrategyCodes(
		[FromQuery] PageOptions pageOptions,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var userStrategyCodes = await strategyQueryService.GetUserStrategyTradeCodesPageAsync(userId, pageOptions, ct);

		return TypedResults.Ok(userStrategyCodes.Map(ApiMapper.ToResponse));
	}

	[HttpPost("codes/byuser")]
	public async Task<Created> CreateUserStrategyCode(
		[FromBody, Required] CreateUserStrategyTradeCodeRequest userStrategyCodeRequest,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		await strategyCommandService.CreateCodeAsync(
			userId,
			userStrategyCodeRequest.StrategyId,
			userStrategyCodeRequest.TradeCodeId,
			ct
		);

		return TypedResults.Created();
	}

	[HttpDelete("codes/byuser")]
	public async Task<NoContent> DeleteUserStrategyCode(
		[FromQuery, Required] int strategyId,
		[FromQuery, Required] int tradeCodeId,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		await strategyCommandService.DeleteCodeAsync(userId, strategyId, tradeCodeId, ct);

		return TypedResults.NoContent();
	}

	[HttpPost("byuser")]
	public async Task<Created> CreateUserStrategy(
		[FromBody, Required] CreateUserStrategyRequest userStrategyRequest,
		CancellationToken ct
	)
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
