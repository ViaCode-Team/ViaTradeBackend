using Application.TradeCodes.Queries;
using Domain.Entities.CSV;
using Domain.Models.Pagination;
using Domain.Models.Sort;
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
public class TradeCodeController(ISender sender) : ControllerBase
{
	private readonly ISender _sender = sender;

	[HttpGet("stocks/statistics")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<Ok<StockStatisticResponse>> GetStockStatistics(CancellationToken cancellationToken)
	{
		var query = new GetStockStatisticQuery();
		var result = await _sender.Send(query, cancellationToken);
		return TypedResults.Ok(result.Adapt<StockStatisticResponse>());
	}

	[HttpGet("stocks")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<Ok<PagedResult<TradeCodeResponse>>> GetStockCodes(
		[FromQuery] PaginationRequest paginationRequest,
		[FromQuery] StockSortRequest? sortRequest,
		CancellationToken cancellationToken)
	{
		var query = new GetCodesPagedQuery(paginationRequest, sortRequest);
		var result = await _sender.Send(query, cancellationToken);
		return TypedResults.Ok(result.Map(c => c.Adapt<TradeCodeResponse>()));
	}

	[HttpGet("sys/stocks")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<Ok<List<TradeCodeFileResponse>>> GetSysStockCodes(CancellationToken cancellationToken)
	{
		var query = new GetSysAllCodesQuery(TradeDataType.Stocks);
		var result = await _sender.Send(query, cancellationToken);
		return TypedResults.Ok(result.Adapt<List<TradeCodeFileResponse>>());
	}

	[HttpGet("sys/stocks/{tradeIdString}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<Ok<TradeCodeFileResponse>> GetSysStockCodeById(
		[FromRoute, Required] string tradeIdString,
		CancellationToken cancellationToken)
	{
		var query = new GetSysCodeByIdQuery(TradeDataType.Stocks, tradeIdString);
		var result = await _sender.Send(query, cancellationToken);
		return TypedResults.Ok(result.Adapt<TradeCodeFileResponse>());
	}
}
