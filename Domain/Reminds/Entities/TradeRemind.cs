using Domain.Reminds.Entities;
using Domain.Common;
using Domain.Entities.DataBase;
using System.Text.Json.Serialization;

namespace Domain.Reminds.Entities;

public class TradeRemind : AggregateRoot
{
    public string TextRemind { get; private set; }
    public DateTime DateTime { get; private set; }
    public int TradeCodeId { get; private set; }
    public int UserId { get; private set; }

    [JsonIgnore]
    public TradeCode? TradeCode { get; private set; }
    
    [JsonIgnore]
    public User? User { get; private set; }

    // EF Core constructor
    private TradeRemind() { }

    public TradeRemind(string textRemind, DateTime dateTime, int tradeCodeId, int userId)
    {
        TextRemind = textRemind;
        DateTime = dateTime;
        TradeCodeId = tradeCodeId;
        UserId = userId;
    }

    public void Update(string textRemind, DateTime dateTime)
    {
        TextRemind = textRemind;
        DateTime = dateTime;
    }
}
