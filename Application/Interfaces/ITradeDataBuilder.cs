using Domain.Models.TradeLogic;

namespace Application.Interfaces
{
    public interface ITradeDataBuilder
    {
        IEnumerable<TradeCodeFile> BuildFileTradeResonse(IEnumerable<string>? fileNames);
    }
}
