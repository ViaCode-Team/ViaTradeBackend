using Domain.Common;
using Domain.TradeCodes.Entities;
using Domain.Users.Entities;

namespace Domain.Reminds.Entities;

public sealed class TradeRemind : AggregateRoot<int>
{
	public string TextRemind { get; private set; }
	public DateTime DateTime { get; private set; }
	public int TradeCodeId { get; private set; }
	public int UserId { get; private set; }
	public TradeCode? TradeCode { get; private set; }
	public User? User { get; private set; }

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
