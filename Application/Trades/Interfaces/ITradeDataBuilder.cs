using ViaTrade.Domain.Models.Trade;

namespace ViaTrade.Application.Trades.Interfaces;

public interface ITradeDataBuilder
{
	IEnumerable<InstrumentFile> BuildInstrumentFiles(IEnumerable<string>? fileNames);
}
