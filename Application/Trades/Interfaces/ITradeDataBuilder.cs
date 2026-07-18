using Domain.Trades.Entities;

namespace Application.Auth.Interfaces;

public interface ITradeDataBuilder
{
	IEnumerable<TradeCodeFile> BuildFileTradeResonse(IEnumerable<string>? fileNames);
}
