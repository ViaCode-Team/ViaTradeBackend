using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Application.Common.Interfaces.Repositories;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Common.Queries;
using ViaTrade.Application.Trades.Interfaces;
using ViaTrade.Application.Trades.Models;
using ViaTrade.Application.Trades.Specifications;
using ViaTrade.Domain.Entities;
using ViaTrade.Domain.Services;

namespace ViaTrade.Application.Trades;

public class TradeQueryService(IReadRepository<Trade> tradeRepository, ITradeRepository tradeStatistics)
	: ITradeQueryService
{
	public async Task<List<ProfitChartBucketDto>> GetProfitChartAsync(
		int userId,
		ProfitChartFilter profitChartFilter,
		CancellationToken ct
	)
	{
		var rows = await tradeStatistics.GetProfitChartAsync(userId, profitChartFilter, ct);

		return rows.Select(row => new ProfitChartBucketDto(
				GetBucketDate(row, profitChartFilter.Granularity),
				row.NetIncome,
				row.BuyNetIncome,
				row.SellNetIncome
			))
			.ToList();
	}

	public Task<TradeDateRangeDto> GetTradeDateRangeAsync(int userId, CancellationToken ct)
	{
		return tradeStatistics.GetTradeDateRangeAsync(userId, ct);
	}

	public async Task<GlobalTradeStatisticDto> GetStatisticsAsync(int userId, CancellationToken ct)
	{
		var result = await tradeStatistics.GetGlobalStatisticsAsync(userId, ct);

		var tradeStatistic = new TradeStatisticDto(result.TotalTrades, result.WinTrades, result.LoseTrades);

		var incomeStatistic = new IncomeTradeStatisticDto(
			Math.Round((decimal)result.TotalAbsoluteIncome, 2),
			TradeStatisticsCalcService.CalculateAverageIncome((decimal)result.TotalAbsoluteIncome, result.TotalTrades)
		);

		var winrateStatistic = new WinrateTradeStatisticDto(
			TradeStatisticsCalcService.CalculateWinrate(result.WinTrades, result.TotalTrades),
			TradeStatisticsCalcService.CalculateProfitFactor(result.TotalProfit, result.TotalLoss)
		);

		return new GlobalTradeStatisticDto(tradeStatistic, incomeStatistic, winrateStatistic);
	}

	public async Task<TradeDto> GetAsync(int userId, int id, CancellationToken ct)
	{
		var trade = await tradeRepository.FirstOrDefaultAsync(
			trade => trade.UserId == userId && trade.Id == id,
			trade => new TradeDto(
				trade.Id,
				trade.OpenedAt,
				trade.ClosedAt,
				trade.OpenPrice,
				trade.ClosePrice,
				trade.NetIncome,
				trade.Quantity,
				trade.TotalPrice,
				trade.Signal,
				trade.TradeTypeId,
				new InstrumentSummaryDto(trade.Instrument!.Id, trade.Instrument.Symbol, trade.Instrument.Description),
				trade.UserId
			),
			ct
		);
		if (trade == null)
			throw new NotFoundException("Trade not found.", "trade_not_found");

		return trade;
	}

	public async Task<PageResult<TradeDto>> GetPageAsync(
		int userId,
		TradeFilter tradeFilter,
		TradeSearch tradeSearch,
		PageOptions pageOptions,
		CancellationToken ct
	)
	{
		var specification = new TradesPageSpecification(userId, tradeFilter, tradeSearch, pageOptions);
		return await PageQuery.ExecuteAsync(
			tradeRepository,
			specification,
			trade => new TradeDto(
				trade.Id,
				trade.OpenedAt,
				trade.ClosedAt,
				trade.OpenPrice,
				trade.ClosePrice,
				trade.NetIncome,
				trade.Quantity,
				trade.TotalPrice,
				trade.Signal,
				trade.TradeTypeId,
				new InstrumentSummaryDto(trade.Instrument!.Id, trade.Instrument.Symbol, trade.Instrument.Description),
				trade.UserId
			),
			ct
		);
	}

	private static DateOnly GetBucketDate(ProfitChartAggregateRow row, ProfitChartGranularity granularity)
	{
		if (granularity == ProfitChartGranularity.Day)
			return new DateOnly(row.Year!.Value, row.Month!.Value, row.Day!.Value);

		if (granularity == ProfitChartGranularity.Month)
			return new DateOnly(row.Year!.Value, row.Month!.Value, 1);

		return DateOnly.FromDateTime(new DateTime(1900, 1, 1).AddDays(row.WeekIndex!.Value * 7));
	}
}
