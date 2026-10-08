using Mediator;

namespace ViaTrade.Application.Trades.Delete;

public sealed record DeleteTradeCommand(int TradeId) : ICommand;
