using Application.Instruments.Models;
using Application.Notes.Models;
using Application.Reminders.Models;
using Application.Strategies.Models;
using Application.Trades.Models;
using Application.Users.Models;
using Domain.Entities;
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

	public static UserSessionResponse ToResponse(UserSessionDto source, string currentSessionId)
	{
		return new UserSessionResponse(
			source.Id,
			source.UserId,
			source.UserAgent,
			source.CreatedAt,
			source.LastSeen,
			source.Id == currentSessionId
		);
	}

	public static NoteResponse ToResponse(Note source) =>
		new(
			source.Id,
			source.Text,
			source.UserId,
			ToBriefResponse(source.Instrument),
			ToBriefResponse(source.Strategy)
		);

	public static partial NoteResponse ToResponse(NoteDto source);

	public static ReminderResponse ToResponse(Reminder source) =>
		new(source.Id, source.Text, source.RemindAt, ToBriefResponse(source.Instrument), source.UserId);

	public static partial ReminderResponse ToResponse(ReminderDto source);

	public static partial InstrumentBriefResponse? ToResponse(InstrumentSummaryDto? source);

	public static partial StrategyBriefResponse? ToResponse(StrategyBriefDto? source);

	public static InstrumentBriefResponse? ToBriefResponse(Instrument? source)
	{
		if (source == null)
			return null;

		return new InstrumentBriefResponse(source.Id, source.Symbol, source.Description);
	}

	public static StrategyBriefResponse? ToBriefResponse(Strategy? source)
	{
		if (source == null)
			return null;

		return new StrategyBriefResponse(source.Id, source.Name, source.Description);
	}

	public static StrategyResponse ToResponse(StrategySubscriptionDto source)
	{
		return new StrategyResponse(
			source.Strategy.Id,
			source.Strategy.Name,
			source.Strategy.Description,
			source.Strategy.DisplayName,
			source.Strategy.Accuracy,
			source.Strategy.SignalFrequency,
			source.Strategy.InvestmentHorizon,
			source.Strategy.LogicDescription,
			source.Strategy.UsageDescription,
			source.Strategy.LimitationsDescription,
			source.IsSubscribed
		);
	}

	public static partial InstrumentResponse ToResponse(Instrument source);

	public static partial InstrumentResponse ToResponse(RelatedInstrumentDto source);

	public static partial InstrumentFileResponse ToResponse(InstrumentFileDto source);

	public static partial TradeResponse ToResponse(TradeDto source);

	public static partial ProfitChartBucketResponse ToResponse(ProfitChartBucketDto source);

	public static partial TradeDateRangeResponse ToResponse(TradeDateRangeDto source);

	public static partial InstrumentBriefResponse? ToResponse(InstrumentBriefDto? source);

	public static partial GlobalStatisticResponse ToResponse(GlobalTradeStatisticDto source);

	public static partial SignalStatisticResponse ToResponse(SignalStatisticDto source);

	public static partial StrategyStatisticResponse ToResponse(StrategyStatisticDto source);

	public static partial SignalResponse ToResponse(SignalDto source);

	public static partial InstrumentStatisticsResponse ToResponse(InstrumentStatisticsDto source);

	public static partial NoteStatisticResponse ToResponse(NoteStatisticDto source);

	public static partial ReminderStatisticsResponse ToResponse(ReminderStatisticsDto source);

	public static partial TradeInputDto ToInput(CreateTradeRequest source);

	public static partial TradeInputDto ToInput(UpdateTradeRequest source);
}
