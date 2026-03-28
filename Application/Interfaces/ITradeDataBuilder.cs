using Domain.Models.TradeLogic;

namespace Application.Interfaces
{
    public interface ITradeDataBuilder
    {
        IEnumerable<TradeCodeResonse> BuildFileTradeResonse(IEnumerable<string>? fileNames);
    }
}
