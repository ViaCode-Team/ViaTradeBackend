using ViaTrade.Application.Trades.Interfaces;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Infrastructure.DataBase.Repositories;

public class TradeTypeEfRepository(AppDbContext context)
	: BaseEfRepository<TradeType>(context),
		ITradeTypeRepository { }
