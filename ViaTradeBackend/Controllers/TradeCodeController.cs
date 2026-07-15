using Application.Interfaces;
using Domain.Entities.CSV;
using Domain.Models.Dto.Statistic;
using Domain.Models.Dto.Trade;
using Domain.Models.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ViaTradeBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TradeCodeController(ITradeCodeService tradeService) : ControllerBase
{
	private readonly ITradeCodeService _tradeService = tradeService;

	[HttpGet("stocks/statistics")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<ActionResult<StockStatistic>> GetStockStatistics(CancellationToken cancellationToken)
	{
		var result = await _tradeService.GetStockStatisticAsync(cancellationToken);
		return Ok(result);
	}

	[HttpGet("stocks")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<ActionResult<PagedResult<TradeCodeDto>>> GetStockCodes([FromQuery] PaginationRequest paginationRequest)
	{
		var result = await _tradeService.GetCodesPagedAsync(paginationRequest);
		return Ok(result);
	}

	[HttpGet("sys/stocks")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<ActionResult<IEnumerable<TradeCodeFileDto>>> GetSysStockCodes()
	{
		var result = await _tradeService.GetSysAllCodesAsync(TradeDataType.Stocks);
		return Ok(result);
	}

	[HttpGet("sys/stocks/{tradeIdString}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<ActionResult<TradeCodeFileDto>> GetSysStockCodeById(
		[FromRoute, Required] string tradeIdString,
		CancellationToken cancellationToken)
	{
		var result = await _tradeService.GetSysCodeByIdAsync(TradeDataType.Stocks, tradeIdString, cancellationToken);
		return Ok(result);
	}
}
