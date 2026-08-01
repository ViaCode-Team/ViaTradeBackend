using Domain.Models.Trade;

namespace Application.Trades.Interfaces;

public interface ITradeDataBuilder
{
	IEnumerable<InstrumentFile> BuildInstrumentFiles(IEnumerable<string>? fileNames);
}
