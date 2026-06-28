using Domain.Entities.DataBase;
using Domain.Models.Dto.Trade;
using Infrastructure.Repositories.DataBase;

namespace Infrastructure.Repositories.DataBase
{
    public class TradeTypeRepository(AppDbContext context) : GenericRepository<TradeType, TradeTypeDto>(context)
    {

    }
}
