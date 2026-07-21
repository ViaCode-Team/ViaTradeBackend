using Domain.Models.Trade;

namespace Application.Trades.Interfaces;

public interface ITradeDataBuilder
{
	IEnumerable<TradeCodeFile> BuildTradeCodeFiles(IEnumerable<string>? fileNames);
}
