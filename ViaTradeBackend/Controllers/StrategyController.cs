using Domain.Users.Entities;
using Application.Contracts.Dto.Requests.Trade;
using Application.Interfaces.Utils;
using Application.Strategies.Commands;
using Application.Strategies.Queries;
using Domain.Models.Filters;
using Domain.Models.Pagination;
using Domain.Models.Sort;
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
public class StrategyController(
	ISender sender,
	IJwtHelper jwtHelper) : ControllerBase
{
	private readonly ISender _sender = sender;
	private readonly IJwtHelper _jwtHelper = jwtHelper;

	[HttpGet("statistics")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<Ok<StrategyStatisticResponse>> GetStrategyStatistics(CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var query = new GetStrategyStatisticQuery(userId);
		var response = await _sender.Send(query, cancellationToken);
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
		var query = new GetStrategiesPagedQuery(userId, filterRequest, sortRequest, paginationRequest);
		var response = await _sender.Send(query, cancellationToken);
		return TypedResults.Ok(response.Map(x => x.Adapt<TradeStrategyResponse>()));
	}

	[HttpGet("{strategyId}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<Ok<TradeStrategyResponse>> GetStrategyById([Required] int strategyId, CancellationToken cancellationToken)
	{
		var query = new GetStrategyByIdQuery(strategyId);
		var response = await _sender.Send(query, cancellationToken);
		return TypedResults.Ok(response.Adapt<TradeStrategyResponse>());
	}

	[HttpGet("byuser/instrumentslink")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<Ok<PagedResult<UserStrategyTradeCodeResponse>>> GetUserStrategyTradeCodes(
		[FromQuery] PaginationRequest paginationRequest, 
		CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var query = new GetUserStrategyCodesPagedQuery(userId, paginationRequest);
		var response = await _sender.Send(query, cancellationToken);
		return TypedResults.Ok(response.Map(x => x.Adapt<UserStrategyTradeCodeResponse>()));
	}

	[HttpPost("byuser/instrumentslink")]
	[ProducesResponseType(StatusCodes.Status201Created)]
	public async Task<Created> CreateUserStrategyTradeCode(
		[FromBody, Required] CreateUserStrategyTradeCodeRequest request,
		CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var command = new CreateUserStrategyCodeCommand(userId, request.StrategyId, request.TradeCodeId);
		await _sender.Send(command, cancellationToken);
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
		var command = new DeleteUserStrategyCodeCommand(userId, strategyId, tradeCodeId);
		await _sender.Send(command, cancellationToken);
		return TypedResults.NoContent();
	}

	[HttpGet("byuser")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<Ok<PagedResult<UserTradeStrategyResponse>>> GetUserStrategies([FromQuery] PaginationRequest paginationRequest, CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var query = new GetUserStrategiesPagedQuery(userId, paginationRequest);
		var response = await _sender.Send(query, cancellationToken);
		return TypedResults.Ok(response.Map(x => x.Adapt<UserTradeStrategyResponse>()));
	}

	[HttpPost("byuser")]
	[ProducesResponseType(StatusCodes.Status201Created)]
	public async Task<Created> CreateUserStrategy([FromBody, Required] CreateUserStrategyRequest userStrategyRequest, CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var command = new CreateUserStrategyCommand(userId, userStrategyRequest.StrategyId);
		await _sender.Send(command, cancellationToken);
		return TypedResults.Created();
	}

	[HttpDelete("byuser")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public async Task<NoContent> DeleteUserStrategy([FromQuery, Required] int strategyId, CancellationToken cancellationToken)
	{
		var userId = _jwtHelper.GetUserIdFromClaims(User);
		var command = new DeleteUserStrategyCommand(userId, strategyId);
		await _sender.Send(command, cancellationToken);
		return TypedResults.NoContent();
	}
}
