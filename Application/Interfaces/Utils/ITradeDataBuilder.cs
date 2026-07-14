using Domain.Models.TradeLogic;

namespace Application.Interfaces.Utils;

public interface ITradeDataBuilder
{
	IEnumerable<TradeCodeFile> BuildFileTradeResonse(IEnumerable<string>? fileNames);
}
