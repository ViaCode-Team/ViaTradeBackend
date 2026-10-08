using Mediator;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Trades.Common;

namespace ViaTrade.Application.Trades.GetPage;

public sealed record GetTradesPageQuery(TradeFilter TradeFilter, TradeSearch TradeSearch, PageOptions PageOptions)
	: IQuery<PageResult<TradeResult>>;
