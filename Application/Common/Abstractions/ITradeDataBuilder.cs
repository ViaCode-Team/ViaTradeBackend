using ViaTrade.Domain.Models.Trade;

namespace ViaTrade.Application.Common.Abstractions;

public interface ITradeDataBuilder
{
	IEnumerable<InstrumentFile> BuildInstrumentFiles(IEnumerable<string>? fileNames);
}
