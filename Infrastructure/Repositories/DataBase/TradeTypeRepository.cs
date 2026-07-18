using Application.Contracts.Dto.Trade;
using Application.Interfaces.Repositories.Database;
using Domain.Entities.DataBase;

namespace Infrastructure.Repositories.DataBase;

public class TradeTypeRepository(AppDbContext context) : GenericRepository<TradeType>(context), ITradeTypeRepository
{
}
