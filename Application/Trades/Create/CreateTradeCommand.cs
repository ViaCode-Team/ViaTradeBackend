using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Trades.Common;

namespace ViaTrade.Application.Trades.Create;

public sealed record CreateTradeCommand(int UserId, TradeInput Trade) : ICommand<TradeResult>;
