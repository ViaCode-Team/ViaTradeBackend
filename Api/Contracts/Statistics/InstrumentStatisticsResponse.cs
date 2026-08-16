using System.ComponentModel.DataAnnotations;

namespace ViaTrade.Api.Contracts.Statistics;

public record InstrumentStatisticsResponse([Range(0, int.MaxValue)] int TotalInstruments);
