using System.ComponentModel.DataAnnotations;
using Application.Auth.Interfaces;
using Application.Common.Models;
using Application.Strategies.Interfaces;
using Application.Strategies.Models;
using Application.TradeCodes.Interfaces;
using Application.TradeCodes.Models;
using Domain.Enums;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ViaTradeBackend.Contracts.Statistics;
using ViaTradeBackend.Contracts.Strategies;
using ViaTradeBackend.Contracts.Trades;
using ViaTradeBackend.Mappings;

namespace ViaTradeBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TradeCodesController(
	ITradeCodeQueryService tradeCodeQueryService,
	IStrategyQueryService strategyQueryService,
	IJwtHelper jwtHelper
) : ControllerBase
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

	[HttpGet("stocks/byticker/{ticker}")]
	public async Task<Ok<TradeCodeResponse>> GetStockCodeByTicker(
		[FromRoute, Required] string ticker,
		CancellationToken ct
	)
	{
		var tradeCode = await tradeCodeQueryService.GetByTickerAsync(ticker, ct);

		return TypedResults.Ok(ApiMapper.ToResponse(tradeCode));
	}

	[HttpGet("stocks/{tradeCodeId}/strategies")]
	public async Task<Ok<PageResult<TradeStrategyResponse>>> GetStrategiesByStock(
		[FromRoute, Required] int tradeCodeId,
		[FromQuery] StrategyFilter strategyFilter,
		[FromQuery] StrategySort strategySort,
		[FromQuery] PageOptions pageOptions,
		CancellationToken ct
	)
	{
		var userId = jwtHelper.GetUserIdFromClaims(User);
		var strategies = await strategyQueryService.GetStrategiesByTradeCodePageAsync(
			userId,
			tradeCodeId,
			strategyFilter,
			strategySort,
			pageOptions,
			ct
		);

		return TypedResults.Ok(strategies.Map(ApiMapper.ToResponse));
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
