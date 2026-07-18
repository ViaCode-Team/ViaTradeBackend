using Application.Trades.Interfaces;
using Domain.Trades.Entities;

namespace Infrastructure.Repositories.DataBase;

public class TradeTypeEfRepository(AppDbContext context) : GenericEfRepository<TradeType>(context), ITradeTypeRepository
{
}
