using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Trades.Common;

namespace ViaTrade.Application.Trades.Get;

public sealed record GetTradeQuery(int TradeId) : IQuery<TradeResult>;
