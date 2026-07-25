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
using ViaTradeBackend.Contracts.Notes;
using ViaTradeBackend.Contracts.Reminders;
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
		new(source.Id, source.NoteText, source.UserId, ToBriefResponse(source.TradeCode), ToBriefResponse(source.TradeStrategy));

	public static NoteResponse ToResponse(NoteDto source) =>
		new(source.Id, source.NoteText, source.UserId, ToResponse(source.TradeCode), ToResponse(source.Strategy));

	public static ReminderResponse ToResponse(Reminder source) =>
		new(source.Id, source.TextRemind, source.DateTime, ToBriefResponse(source.TradeCode), source.UserId);

	public static ReminderResponse ToResponse(ReminderDto source) =>
		new(source.Id, source.Text, source.DateTime, ToResponse(source.TradeCode), source.UserId);

	public static TradeCodeBriefResponse? ToResponse(TradeCodeSummaryDto? source)
	{
		if (source == null)
			return null;

		return new TradeCodeBriefResponse(source.Id, source.Ticker, source.Name);
	}

	public static StrategyBriefResponse? ToResponse(StrategyBriefDto? source)
	{
		if (source == null)
			return null;

		return new StrategyBriefResponse(source.Id, source.Name, source.Description);
	}

	public static TradeCodeBriefResponse? ToBriefResponse(TradeCode? source)
	{
		if (source == null)
			return null;

		return new TradeCodeBriefResponse(source.Id, source.ExchangeId, source.Description);
	}

	public static StrategyBriefResponse? ToBriefResponse(TradeStrategy? source)
	{
		if (source == null)
			return null;

		return new StrategyBriefResponse(source.Id, source.Name, source.Description);
	}

	public static partial TradeStrategyResponse ToResponse(TradeStrategy source);

	public static TradeStrategyResponse ToResponse(RelatedTradeStrategyDto source) =>
		new(
			source.Id,
			source.Name,
			source.Description,
			source.Accuracy,
			source.SignalFrequency,
			source.InvestmentHorizon,
			source.LogicDesc,
			source.UseDesc,
			source.LimitDesc,
			source.IsActive
		);

	public static partial UserTradeStrategyResponse ToResponse(UserTradeStrategy source);

	public static partial UserStrategyTradeCodeResponse ToResponse(UserStrategyTradeCode source);

	public static partial TradeCodeResponse ToResponse(TradeCode source);

	public static TradeCodeResponse ToResponse(RelatedTradeCodeDto source) =>
		new(source.Id, source.ExchangeId, source.Description);

	public static partial TradeCodeFileResponse ToResponse(TradeCodeFileDto source);

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
			ToResponse(source.TradeCode),
			source.UserId
		);

	public static TradeCodeBriefResponse? ToResponse(TradeCodeBriefDto? source)
	{
		if (source == null)
			return null;

		return new TradeCodeBriefResponse(source.Id, source.Ticker, source.Name);
	}

	public static partial GlobalStatisticResponse ToResponse(GlobalTradeStatisticDto source);

	public static partial SignalStatisticResponse ToResponse(SignalStatisticDto source);

	public static partial StrategyStatisticResponse ToResponse(StrategyStatisticDto source);

	public static partial StockStatisticResponse ToResponse(StockStatisticDto source);

	public static partial NoteStatisticResponse ToResponse(NoteStatisticDto source);

	public static partial ReminderStatisticsResponse ToResponse(ReminderStatisticsDto source);

	[MapperRequiredMapping(RequiredMappingStrategy.Both)]
	public static partial TradeInputDto ToInput(CreateTradeRequest source);

	[MapperRequiredMapping(RequiredMappingStrategy.Both)]
	public static partial TradeInputDto ToInput(UpdateTradeRequest source);
}
