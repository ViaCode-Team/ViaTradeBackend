using Domain.Common;
using Domain.TradeCodes.Entities;
using Domain.Trades.Enums;
using Domain.Users.Entities;

namespace Domain.Trades.Entities;

public sealed class Trade : AggregateRoot<int>
{
	public DateTime DateOpen { get; private set; }
	public DateTime? DateClose { get; private set; }
	public double TradeOpen { get; private set; }
	public double? TradeClose { get; private set; }
	public double? NetIncome { get; private set; }
	public int Count { get; private set; }

	public decimal Price { get; private set; }

	public int TradeTypeId { get; private set; }
	public int TradeCodeId { get; private set; }
	public int UserId { get; private set; }
	public TradeSignal TradeSignal { get; private set; }

	public TradeType? TradeType { get; private set; }

	public TradeCode? TradeCode { get; private set; }

	public User? User { get; private set; }

	private Trade() { }

	public Trade(DateTime dateOpen, double tradeOpen, int count, decimal price, int tradeTypeId, int tradeCodeId, int userId, TradeSignal tradeSignal)
	{
		if (count <= 0) throw new ArgumentException("Count must be greater than zero.", nameof(count));
		if (price < 0) throw new ArgumentException("Price cannot be negative.", nameof(price));

		DateOpen = dateOpen;
		TradeOpen = tradeOpen;
		Count = count;
		Price = price;
		TradeTypeId = tradeTypeId;
		TradeCodeId = tradeCodeId;
		UserId = userId;
		TradeSignal = tradeSignal;
	}

	public void Update(DateTime dateOpen, DateTime? dateClose, double tradeOpen, double? tradeClose, int count, int tradeTypeId, int tradeCodeId, TradeSignal tradeSignal)
	{
		if (count <= 0) throw new ArgumentException("Count must be greater than zero.", nameof(count));

		DateOpen = dateOpen;
		DateClose = dateClose;
		TradeOpen = tradeOpen;
		TradeClose = tradeClose;
		NetIncome = CalculateNetIncome(tradeOpen, tradeClose, tradeSignal);
		Count = count;
		Price = (decimal)tradeOpen * count;
		TradeTypeId = tradeTypeId;
		TradeCodeId = tradeCodeId;
		TradeSignal = tradeSignal;
	}

	public static double? CalculateNetIncome(double tradeOpen, double? tradeClose, TradeSignal tradeSignal)
	{
		if (tradeClose == null || tradeOpen == 0 || tradeSignal == TradeSignal.HOLD)
			return null;

		var basePercent = (tradeClose.Value - tradeOpen) / tradeOpen * 100;
		double adjustedPercent = basePercent;
		if (tradeSignal == TradeSignal.SELL)
		{
			adjustedPercent = -basePercent;
		}

		return Math.Round(adjustedPercent, 2);
	}
}
