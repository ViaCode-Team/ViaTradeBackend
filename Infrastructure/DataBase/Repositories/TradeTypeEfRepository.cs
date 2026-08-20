using ViaTrade.Application.Trades.Interfaces;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Infrastructure.DataBase.Repositories;

public class TradeTypeEfRepository(AppDbContext context, EfQueryObjectBuilder queryObjectBuilder)
	: BaseEfRepository<TradeType>(context, queryObjectBuilder),
		ITradeTypeRepository { }
