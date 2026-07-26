namespace ViaTradeBackend.Contracts.Instruments;

public record InstrumentFileResponse(int Id, string Symbol, string TimeFrame, DateTime StartDate, DateTime EndDate);
