using Application.Trades.Interfaces;
using Domain.Trades.Entities;

namespace Infrastructure.DataBase.Repositories;

public class TradeTypeEfRepository(AppDbContext context)
	: GenericEfRepository<TradeType>(context),
		ITradeTypeRepository { }
