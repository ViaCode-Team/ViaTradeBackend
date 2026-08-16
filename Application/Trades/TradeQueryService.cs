using Application.Common.Exceptions;
using Application.Common.Models;
using Application.Trades.Interfaces;
using Application.Trades.Models;
using Application.Trades.Specifications;
using Domain.Services;

namespace Application.Trades;

public class TradeQueryService(ITradeRepository tradeRepository) : ITradeQueryService
{
	public async Task<List<ProfitChartBucketDto>> GetProfitChartAsync(
		int userId,
		ProfitChartFilter filter,
		CancellationToken ct
	)
	{
		var rows = await tradeRepository.GetProfitChartAsync(userId, filter, ct);

		return rows.Select(row => new ProfitChartBucketDto(
				GetBucketDate(row, filter.Granularity),
				row.NetIncome,
				row.BuyNetIncome,
				row.SellNetIncome
			))
			.ToList();
	}

	public Task<TradeDateRangeDto> GetTradeDateRangeAsync(int userId, CancellationToken ct)
	{
		return tradeRepository.GetTradeDateRangeAsync(userId, ct);
	}

	public async Task<GlobalTradeStatisticDto> GetStatisticsAsync(int userId, CancellationToken ct)
	{
		var result = await tradeRepository.GetGlobalStatisticsAsync(userId, ct);

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
		var trade = await tradeRepository.FindProjectionByUserAndIdAsync(userId, id, ct);
		if (trade == null)
			throw new NotFoundException("Trade not found.", "trade_not_found");

		return ToDto(trade);
	}

	public async Task<PageResult<TradeDto>> GetPageAsync(
		int userId,
		TradeFilter tradeFilter,
		PageOptions pageOptions,
		CancellationToken ct
	)
	{
		var spec = new TradeQuerySpecification(userId, tradeFilter);
		var trades = await tradeRepository.GetPageProjectionAsync(spec, pageOptions, ct);

		return trades.Map(ToDto);
	}

	public async Task<PageResult<TradeDto>> GetPageSearchAsync(
		int userId,
		SearchFilter tradeFilter,
		PageOptions pageOptions,
		CancellationToken ct
	)
	{
		var spec = new TradeSearchSpecification(userId, tradeFilter);
		var trades = await tradeRepository.GetPageSearchProjectionAsync(spec, pageOptions, ct);

		return trades.Map(ToDto);
	}

	private static TradeDto ToDto(TradeProjectionDto source) =>
		new(
			source.Id,
			source.OpenedAt,
			source.ClosedAt,
			source.OpenPrice,
			source.ClosePrice,
			source.NetIncome,
			source.Quantity,
			source.TotalPrice,
			source.Signal,
			source.TradeTypeId,
			source.Instrument,
			source.UserId
		);

	private static DateOnly GetBucketDate(ProfitChartAggregateRow row, ProfitChartGranularity granularity)
	{
		if (granularity == ProfitChartGranularity.Day)
			return new DateOnly(row.Year!.Value, row.Month!.Value, row.Day!.Value);

		if (granularity == ProfitChartGranularity.Month)
			return new DateOnly(row.Year!.Value, row.Month!.Value, 1);

		return DateOnly.FromDateTime(new DateTime(1900, 1, 1).AddDays(row.WeekIndex!.Value * 7));
	}
}
