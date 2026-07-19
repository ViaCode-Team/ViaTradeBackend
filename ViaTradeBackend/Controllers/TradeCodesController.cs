using System.ComponentModel.DataAnnotations;
using Application.Common.Queries;
using Application.TradeCodes.Interfaces;
using Application.TradeCodes.Queries;
using Domain.Trades.Entities;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ViaTradeBackend.Contracts.Statistics;
using ViaTradeBackend.Contracts.Trades;

namespace ViaTradeBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TradeCodesController(ITradeCodeQueryService tradeCodeQueryService) : ControllerBase
{
	[HttpGet("stocks/statistics")]
	public async Task<Ok<StockStatisticResponse>> GetStockStatistics(CancellationToken ct)
	{
		var stockStatistic = await tradeCodeQueryService.GetStatisticsAsync(ct);

		return TypedResults.Ok(stockStatistic.Adapt<StockStatisticResponse>());
	}

	[HttpGet("stocks")]
	public async Task<Ok<PageResult<TradeCodeResponse>>> GetStockCodes(
		[FromQuery] PageOptions page,
		[FromQuery] TradeCodeSort sort,
		CancellationToken ct
	)
	{
		var pagedCodes = await tradeCodeQueryService.GetAsync(page, sort, ct);

		return TypedResults.Ok(pagedCodes.Map(c => c.Adapt<TradeCodeResponse>()));
	}

	[HttpGet("sys/stocks")]
	public async Task<Ok<List<TradeCodeFileResponse>>> GetSysStockCodes(CancellationToken ct)
	{
		var sysCodes = await tradeCodeQueryService.GetFileMetadataAsync(TradeDataType.Stocks, ct);

		return TypedResults.Ok(sysCodes.Adapt<List<TradeCodeFileResponse>>());
	}

	[HttpGet("sys/stocks/{id}")]
	public async Task<Ok<TradeCodeFileResponse>> GetSysStockCodeById(
		[FromRoute, Required] string id,
		CancellationToken ct
	)
	{
		var sysCode = await tradeCodeQueryService.GetFileMetadataAsync(TradeDataType.Stocks, id, ct);

		return TypedResults.Ok(sysCode.Adapt<TradeCodeFileResponse>());
	}
}
