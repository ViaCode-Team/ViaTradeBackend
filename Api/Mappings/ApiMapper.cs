using Riok.Mapperly.Abstractions;
using ViaTrade.Api.Contracts.Instruments;
using ViaTrade.Api.Contracts.Notes;
using ViaTrade.Api.Contracts.Reminders;
using ViaTrade.Api.Contracts.Signals;
using ViaTrade.Api.Contracts.Statistics;
using ViaTrade.Api.Contracts.Strategies;
using ViaTrade.Api.Contracts.Trades;
using ViaTrade.Api.Contracts.Users;
using ViaTrade.Application.Auth.GetSessionsPage;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Instruments.Common;
using ViaTrade.Application.Instruments.GetStatistics;
using ViaTrade.Application.Notes.Common;
using ViaTrade.Application.Notes.GetStatistics;
using ViaTrade.Application.Reminders.Common;
using ViaTrade.Application.Reminders.Create;
using ViaTrade.Application.Reminders.GetStatistics;
using ViaTrade.Application.Signals.Common;
using ViaTrade.Application.Signals.GetStatistics;
using ViaTrade.Application.Strategies.Common;
using ViaTrade.Application.Strategies.GetStatistics;
using ViaTrade.Application.Trades.Common;
using ViaTrade.Application.Trades.GetDateRange;
using ViaTrade.Application.Trades.GetProfitChart;
using ViaTrade.Application.Trades.GetStatistics;
using ViaTrade.Application.Users.GetCurrent;

namespace ViaTrade.Api.Mappings;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class ApiMapper
{
	public static partial UserMeResponse ToResponse(CurrentUserResult source);

	public static UserSessionResponse ToResponse(SessionResult source, string currentSessionId)
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

	public static partial NoteResponse ToResponse(NoteResult source);

	public static partial ReminderResponse ToResponse(ReminderResult source);

	public static ReminderResponse ToResponse(CreateReminderResult source) =>
		new(source.Id, source.Text, source.RemindAt, null, source.DeliveredAt);

	public static DueReminderResponse ToDueResponse(ReminderResult source) =>
		new(source.Id, source.Text, source.RemindAt, ToResponse(source.Instrument), source.UserId);

	public static partial InstrumentBriefResponse? ToResponse(InstrumentBriefResult? source);

	public static partial StrategyBriefResponse? ToResponse(StrategyBriefResult? source);

	public static StrategyResponse ToResponse(StrategySubscriptionResult source)
	{
		return new StrategyResponse(
			source.Id,
			source.Name,
			source.Description,
			source.DisplayName,
			source.Accuracy,
			source.SignalFrequency,
			source.InvestmentHorizon,
			source.LogicDescription,
			source.UsageDescription,
			source.LimitationsDescription,
			source.IsSubscribed
		);
	}

	public static partial InstrumentResponse ToResponse(InstrumentResult source);

	public static partial InstrumentFileResponse ToResponse(InstrumentFileResult source);

	public static partial TradeResponse ToResponse(TradeResult source);

	public static partial ProfitChartBucketResponse ToResponse(ProfitChartBucketResult source);

	public static partial TradeDateRangeResponse ToResponse(TradeDateRangeResult source);

	public static partial GlobalStatisticResponse ToResponse(TradeStatisticsResult source);

	public static partial SignalStatisticResponse ToResponse(SignalStatisticsResult source);

	public static partial StrategyStatisticResponse ToResponse(StrategyStatisticsResult source);

	public static partial SignalResponse ToResponse(SignalResult source);

	public static partial InstrumentStatisticsResponse ToResponse(InstrumentStatisticsResult source);

	public static partial NoteStatisticResponse ToResponse(NoteStatisticsResult source);

	public static partial ReminderStatisticsResponse ToResponse(ReminderStatisticsResult source);

	public static partial TradeInput ToInput(CreateTradeRequest source);

	public static partial TradeInput ToInput(UpdateTradeRequest source);
}
