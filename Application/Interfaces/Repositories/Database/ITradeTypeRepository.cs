using Domain.Trades.Enums;
using Domain.Trades.Entities;
using Application.Contracts.Dto.Trade;
using Domain.Entities.DataBase;

namespace Application.Interfaces.Repositories.Database;

public interface ITradeTypeRepository : IRepository<TradeType>
{
}
