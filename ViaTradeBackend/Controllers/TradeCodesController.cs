using System.ComponentModel.DataAnnotations;
using Application.Common.Models;
using Application.TradeCodes.Interfaces;
using Application.TradeCodes.Models;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ViaTradeBackend.Contracts.Statistics;
using ViaTradeBackend.Contracts.Trades;
using ViaTradeBackend.Mappings;

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

		return TypedResults.Ok(ApiMapper.ToResponse(stockStatistic));
	}

	[HttpGet("stocks")]
	public async Task<Ok<PageResult<TradeCodeResponse>>> GetStockCodes(
		[FromQuery] PageOptions pageOptions,
		[FromQuery] TradeCodeSort tradeCodeSort,
		CancellationToken ct
	)
	{
		var pagedCodes = await tradeCodeQueryService.GetPageAsync(pageOptions, tradeCodeSort, ct);

		return TypedResults.Ok(pagedCodes.Map(ApiMapper.ToResponse));
	}

	[HttpGet("sys/stocks")]
	public async Task<Ok<List<TradeCodeFileResponse>>> GetSysStockCodes(CancellationToken ct)
	{
		var sysCodes = await tradeCodeQueryService.ListFileMetadataAsync(TradeDataType.Stocks, ct);

		return TypedResults.Ok(sysCodes.Select(ApiMapper.ToResponse).ToList());
	}

	[HttpGet("sys/stocks/{id}")]
	public async Task<Ok<TradeCodeFileResponse>> GetSysStockCodeById(
		[FromRoute, Required] string id,
		CancellationToken ct
	)
	{
		var sysCode = await tradeCodeQueryService.GetFileMetadataAsync(TradeDataType.Stocks, id, ct);

		return TypedResults.Ok(ApiMapper.ToResponse(sysCode));
	}
}
