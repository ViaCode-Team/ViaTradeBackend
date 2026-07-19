using Application.Auth.Interfaces;
using Application.Common.Models.Filters;
using Application.Common.Models.Pagination;
using Application.Common.Models.Sort;
using Application.Strategies.Commands;
using Application.Strategies.Queries;
using Mapster;
using MediatR;
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
public class StrategyController(ISender sender, IJwtHelper jwtHelper) : ControllerBase
{
	[HttpGet("statistics")]
	public async Task<Ok<StrategyStatisticResponse>> GetStrategyStatistics(CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);

		var query = new GetStrategyStatisticQuery(userId);
		var response = await sender.Send(query, ct);

		return TypedResults.Ok(response.Adapt<StrategyStatisticResponse>());
	}

	[HttpGet("")]
	public async Task<Ok<PagedResult<TradeStrategyResponse>>> GetStrategies(
		[FromQuery] StrategyFilterRequest? filterRequest,
		[FromQuery] StrategySortRequest? sortRequest,
		[FromQuery] PaginationRequest? paginationRequest,
		CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);

		var query = new GetStrategiesPagedQuery(userId, filterRequest, sortRequest, paginationRequest);
		var response = await sender.Send(query, ct);

		return TypedResults.Ok(response.Map(x => x.Adapt<TradeStrategyResponse>()));
	}

	[HttpGet("{strategyId}")]
	public async Task<Ok<TradeStrategyResponse>> GetStrategyById([Required] int strategyId, CancellationToken ct)
	{
		var query = new GetStrategyByIdQuery(strategyId);
		var response = await sender.Send(query, ct);

		return TypedResults.Ok(response.Adapt<TradeStrategyResponse>());
	}

	[HttpGet("byuser/instrumentslink")]
	public async Task<Ok<PagedResult<UserStrategyTradeCodeResponse>>> GetUserStrategyTradeCodes(
		[FromQuery] PaginationRequest paginationRequest,
		CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);

		var query = new GetUserStrategyCodesPagedQuery(userId, paginationRequest);
		var response = await sender.Send(query, ct);

		return TypedResults.Ok(response.Map(x => x.Adapt<UserStrategyTradeCodeResponse>()));
	}

	[HttpPost("byuser/instrumentslink")]
	public async Task<Created> CreateUserStrategyTradeCode(
		[FromBody, Required] CreateUserStrategyTradeCodeRequest request,
		CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);

		var command = new CreateUserStrategyCodeCommand(userId, request.StrategyId, request.TradeCodeId);
		await sender.Send(command, ct);

		return TypedResults.Created();
	}

	[HttpDelete("byuser/instrumentslink")]
	public async Task<NoContent> DeleteUserStrategyTradeCode(
		[FromQuery, Required] int strategyId,
		[FromQuery, Required] int tradeCodeId,
		CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);

		var command = new DeleteUserStrategyCodeCommand(userId, strategyId, tradeCodeId);
		await sender.Send(command, ct);

		return TypedResults.NoContent();
	}

	[HttpGet("byuser")]
	public async Task<Ok<PagedResult<UserTradeStrategyResponse>>> GetUserStrategies([FromQuery] PaginationRequest paginationRequest, CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);

		var query = new GetUserStrategiesPagedQuery(userId, paginationRequest);
		var response = await sender.Send(query, ct);

		return TypedResults.Ok(response.Map(x => x.Adapt<UserTradeStrategyResponse>()));
	}

	[HttpPost("byuser")]
	public async Task<Created> CreateUserStrategy([FromBody, Required] CreateUserStrategyRequest userStrategyRequest, CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);

		var command = new CreateUserStrategyCommand(userId, userStrategyRequest.StrategyId);
		await sender.Send(command, ct);

		return TypedResults.Created();
	}

	[HttpDelete("byuser")]
	public async Task<NoContent> DeleteUserStrategy([FromQuery, Required] int strategyId, CancellationToken ct)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var command = new DeleteUserStrategyCommand(userId, strategyId);
		await sender.Send(command, ct);
		return TypedResults.NoContent();
	}
}
