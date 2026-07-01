using Application.Interfaces.Repositories.Database;
using Domain.Entities.DataBase;
using Domain.Models.Dto.Trade;

namespace Infrastructure.Repositories.DataBase
{
    public class TradeTypeRepository(AppDbContext context) : GenericRepository<TradeType, TradeTypeDto>(context), ITradeTypeRepository
    {
    }
}
