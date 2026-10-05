using ViaTrade.Application.Common.Abstractions;

namespace ViaTrade.Application.Trades.Delete;

public sealed record DeleteTradeCommand(int UserId, int TradeId) : ICommand;
