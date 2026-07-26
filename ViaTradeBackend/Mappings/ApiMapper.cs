using Application.Notes.Models;
using Application.Reminders.Models;
using Application.Strategies.Models;
using Application.TradeCodes.Models;
using Application.Trades.Models;
using Application.Users.Models;
using Domain.Notes.Entities;
using Domain.Reminders.Entities;
using Domain.Strategies.Entities;
using Domain.TradeCodes.Entities;
using Riok.Mapperly.Abstractions;
using ViaTradeBackend.Contracts.Instruments;
using ViaTradeBackend.Contracts.Notes;
using ViaTradeBackend.Contracts.Reminders;
using ViaTradeBackend.Contracts.Signals;
using ViaTradeBackend.Contracts.Statistics;
using ViaTradeBackend.Contracts.Strategies;
using ViaTradeBackend.Contracts.Trades;
using ViaTradeBackend.Contracts.Users;

namespace ViaTradeBackend.Mappings;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class ApiMapper
{
	public static partial UserMeResponse ToResponse(UserMeDto source);

	public static partial UserTelegramResponse ToResponse(UserTelegramDto source);

	public static partial UserSessionResponse ToResponse(UserSessionDto source);

	public static NoteResponse ToResponse(Note source) =>
		new(
			source.Id,
			source.NoteText,
			source.UserId,
			ToBriefResponse(source.TradeCode),
			ToBriefResponse(source.TradeStrategy)
		);

	public static NoteResponse ToResponse(NoteDto source) =>
		new(source.Id, source.NoteText, source.UserId, ToResponse(source.Instrument), ToResponse(source.Strategy));

	public static ReminderResponse ToResponse(Reminder source) =>
		new(source.Id, source.TextRemind, source.DateTime, ToBriefResponse(source.TradeCode), source.UserId);

	public static ReminderResponse ToResponse(ReminderDto source) =>
		new(source.Id, source.Text, source.RemindAt, ToResponse(source.Instrument), source.UserId);

	public static InstrumentBriefResponse? ToResponse(InstrumentSummaryDto? source)
	{
		if (source == null)
			return null;

		return new InstrumentBriefResponse(source.Id, source.Symbol, source.Name);
	}

	public static StrategyBriefResponse? ToResponse(StrategyBriefDto? source)
	{
		if (source == null)
			return null;

		return new StrategyBriefResponse(source.Id, source.Name, source.Description);
	}

	public static InstrumentBriefResponse? ToBriefResponse(TradeCode? source)
	{
		if (source == null)
			return null;

		return new InstrumentBriefResponse(source.Id, source.ExchangeId, source.Description);
	}

	public static StrategyBriefResponse? ToBriefResponse(TradeStrategy? source)
	{
		if (source == null)
			return null;

		return new StrategyBriefResponse(source.Id, source.Name, source.Description);
	}

	public static partial StrategyResponse ToResponse(TradeStrategy source);

	public static InstrumentResponse ToResponse(TradeCode source) =>
		new(source.Id, source.ExchangeId, source.Description);

	public static InstrumentResponse ToResponse(RelatedInstrumentDto source) =>
		new(source.Id, source.Symbol, source.Description);

	public static InstrumentFileResponse ToResponse(InstrumentFileDto source) =>
		new(source.Id, source.Symbol, source.TimeFrame, source.StartDate, source.EndDate);

	public static TradeResponse ToResponse(TradeDto source) =>
		new(
			source.Id,
			source.DateOpen,
			source.DateClose,
			source.TradeOpen,
			source.TradeClose,
			source.NetIncome,
			source.Count,
			source.Price,
			source.TradeSignal,
			source.TradeTypeId,
			ToResponse(source.Instrument),
			source.UserId
		);

	public static InstrumentBriefResponse? ToResponse(InstrumentBriefDto? source)
	{
		if (source == null)
			return null;

		return new InstrumentBriefResponse(source.Id, source.Symbol, source.Name);
	}

	public static partial GlobalStatisticResponse ToResponse(GlobalTradeStatisticDto source);

	public static partial SignalStatisticResponse ToResponse(SignalStatisticDto source);

	public static partial StrategyStatisticResponse ToResponse(StrategyStatisticDto source);

	public static SignalResponse ToResponse(SignalDto source) =>
		new(
			source.StrategyId,
			source.StrategyName,
			source.InstrumentId,
			source.Symbol,
			source.Accuracy,
			source.Date,
			source.ClosePrice,
			source.Signal
		);

	public static InstrumentStatisticsResponse ToResponse(InstrumentStatisticsDto source) =>
		new(source.TotalInstruments);

	public static partial NoteStatisticResponse ToResponse(NoteStatisticDto source);

	public static partial ReminderStatisticsResponse ToResponse(ReminderStatisticsDto source);

	public static TradeInputDto ToInput(CreateTradeRequest source) =>
		new()
		{
			DateOpen = source.DateOpen,
			DateClose = source.DateClose,
			TradeOpen = source.TradeOpen,
			TradeClose = source.TradeClose,
			TradeSignal = source.TradeSignal,
			Count = source.Count,
			TradeTypeId = source.TradeTypeId,
			TradeCodeId = source.InstrumentId,
		};

	public static TradeInputDto ToInput(UpdateTradeRequest source) =>
		new()
		{
			DateOpen = source.DateOpen,
			DateClose = source.DateClose,
			TradeOpen = source.TradeOpen,
			TradeClose = source.TradeClose,
			TradeSignal = source.TradeSignal,
			Count = source.Count,
			TradeTypeId = source.TradeTypeId,
			TradeCodeId = source.InstrumentId,
		};
}
