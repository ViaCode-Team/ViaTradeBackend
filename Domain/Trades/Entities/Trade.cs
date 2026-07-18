using Domain.Trades.Entities;
using Domain.Common;
using Domain.Entities.DataBase; // For TradeCode and User (temporary)
using Domain.Trades.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Domain.Trades.Entities;

public class Trade : AggregateRoot
{
	public DateTime DateOpen { get; private set; }
	public DateTime? DateClose { get; private set; }
	public double TradeOpen { get; private set; }
	public double? TradeClose { get; private set; }
	public double? NetIncome { get; private set; }
	public int Count { get; private set; }

	[Column(TypeName = "decimal(18,2)")]
	public decimal Price { get; private set; }

	public int TradeTypeId { get; private set; }
	public int TradeCodeId { get; private set; }
	public int UserId { get; private set; }
	public TradeSignal TradeSignal { get; private set; }

	[JsonIgnore]
	public TradeType? TradeType { get; private set; }
	
	[JsonIgnore]
	public Domain.TradeCodes.Entities.TradeCode? TradeCode { get; private set; }
	
	[JsonIgnore]
	public User? User { get; private set; }

	private Trade() { }

	public Trade(DateTime dateOpen, double tradeOpen, int count, decimal price, int tradeTypeId, int tradeCodeId, int userId, TradeSignal tradeSignal)
	{
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
