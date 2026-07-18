using Domain.Trades.Enums;
using Domain.Trades.Entities;
using Domain.Common;
using Domain.Entities.DataBase;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Domain.TradeCodes.Entities;

public class TradeCode : AggregateRoot
{
    [MaxLength(128)]
    public string ExchangeId { get; private set; }
    
    [MaxLength(512)]
    public string? Description { get; private set; }
    
    [JsonIgnore]
    public ICollection<Trade>? Trades { get; private set; }
    
    [JsonIgnore]
    public ICollection<UserTradeCode>? UserTradeCodes { get; private set; }

    private TradeCode() { }

    public TradeCode(string exchangeId, string? description = null)
    {
        ExchangeId = exchangeId;
        Description = description;
    }
}
