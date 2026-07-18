using Application.Interfaces;
using Domain.Entities.CSV;
using Domain.Models.Pagination;
using Domain.Models.Sort;
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
public class TradeCodeController(ITradeCodeService tradeService) : ControllerBase
{
	private readonly ITradeCodeService _tradeService = tradeService;

	[HttpGet("stocks/statistics")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<Ok<StockStatisticResponse>> GetStockStatistics(CancellationToken cancellationToken)
	{
		var result = await _tradeService.GetStockStatisticAsync(cancellationToken);
		return TypedResults.Ok(result.Adapt<StockStatisticResponse>());
	}

	[HttpGet("stocks")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<Ok<PagedResult<TradeCodeResponse>>> GetStockCodes(
		[FromQuery] PaginationRequest paginationRequest,
		[FromQuery] StockSortRequest? sortRequest,
		CancellationToken cancellationToken)
	{
		var result = await _tradeService.GetCodesPagedAsync(paginationRequest, sortRequest, cancellationToken);
		return TypedResults.Ok(result.Map(c => c.Adapt<TradeCodeResponse>()));
	}

	[HttpGet("sys/stocks")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<Ok<List<TradeCodeFileResponse>>> GetSysStockCodes()
	{
		var result = await _tradeService.GetSysAllCodesAsync(TradeDataType.Stocks);
		return TypedResults.Ok(result.Adapt<List<TradeCodeFileResponse>>());
	}

	[HttpGet("sys/stocks/{tradeIdString}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<Ok<TradeCodeFileResponse>> GetSysStockCodeById(
		[FromRoute, Required] string tradeIdString,
		CancellationToken cancellationToken)
	{
		var result = await _tradeService.GetSysCodeByIdAsync(TradeDataType.Stocks, tradeIdString, cancellationToken);
		return TypedResults.Ok(result.Adapt<TradeCodeFileResponse>());
	}
}

