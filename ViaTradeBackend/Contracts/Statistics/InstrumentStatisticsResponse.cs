using System.ComponentModel.DataAnnotations;

namespace ViaTradeBackend.Contracts.Statistics;

public record InstrumentStatisticsResponse([Range(0, int.MaxValue)] int TotalInstruments);
