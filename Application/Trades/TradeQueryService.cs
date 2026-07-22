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
		var trade = await tradeRepository.FindOneAsync(x => x.Id == id && x.UserId == userId, ct);
		if (trade == null)
			throw new KeyNotFoundException();

		return new TradeDto(
			trade.Id,
			trade.DateOpen,
			trade.DateClose,
			trade.TradeOpen,
			trade.TradeClose,
			TradeStatisticsCalcService.CalculateNetIncome(trade.TradeOpen, trade.TradeClose, trade.TradeSignal),
			trade.Count,
			trade.Price,
			trade.TradeSignal,
			trade.TradeTypeId,
			trade.TradeCodeId,
			trade.UserId
		);
	}

	public async Task<PageResult<TradeDto>> GetPageAsync(
		int userId,
		TradeFilter tradeFilter,
		PageOptions pageOptions,
		CancellationToken ct
	)
	{
		var spec = new TradeQuerySpecification(userId, tradeFilter);
		var trades = await tradeRepository.GetPageAsync(spec, pageOptions, ct);

		return trades.Map(trade => new TradeDto(
			trade.Id,
			trade.DateOpen,
			trade.DateClose,
			trade.TradeOpen,
			trade.TradeClose,
			TradeStatisticsCalcService.CalculateNetIncome(trade.TradeOpen, trade.TradeClose, trade.TradeSignal),
			trade.Count,
			trade.Price,
			trade.TradeSignal,
			trade.TradeTypeId,
			trade.TradeCodeId,
			trade.UserId
		));
	}
}
