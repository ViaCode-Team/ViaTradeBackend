using Application.Common.Exceptions;
using Application.Common.Models;
using Application.Common.Specifications;
using Application.Trades.Interfaces;
using Application.Trades.Models;
using Domain.Statistics.Services;

namespace Application.Trades;

public class TradeQueryService(ITradeRepository tradeRepository) : ITradeQueryService
{
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

	private static TradeDto ToDto(TradeProjectionDto source) =>
		new(
			source.Id,
			source.DateOpen,
			source.DateClose,
			source.TradeOpen,
			source.TradeClose,
			TradeStatisticsCalcService.CalculateNetIncome(source.TradeOpen, source.TradeClose, source.TradeSignal),
			source.Count,
			source.Price,
			source.TradeSignal,
			source.TradeTypeId,
			source.TradeCode,
			source.UserId
		);
}
