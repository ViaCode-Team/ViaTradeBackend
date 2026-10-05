using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Trades.Common;

namespace ViaTrade.Application.Trades.Update;

public sealed record UpdateTradeCommand(int UserId, int TradeId, TradeInput Trade) : ICommand;
