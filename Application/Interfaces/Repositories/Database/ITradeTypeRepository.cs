using Domain.Entities.DataBase;
using Domain.Models.Dto.Trade;

namespace Application.Interfaces.Repositories.Database
{
    public interface ITradeTypeRepository : IRepository<TradeType, TradeTypeDto>
    {
    }
}
