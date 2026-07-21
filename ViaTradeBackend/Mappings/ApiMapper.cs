using Application.Notes.Models;
using Application.Reminders.Models;
using Application.TradeCodes.Models;
using Application.Trades.Models;
using Application.Users.Models;
using Domain.Notes.Entities;
using Domain.Reminders.Entities;
using Domain.Strategies.Entities;
using Domain.TradeCodes.Entities;
using Domain.Users.Entities;
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
	public static partial UserMeResponse ToUserMeResponse(User source);

	public static partial UserTelegramResponse ToUserTelegramResponse(User source);

	public static partial UserSessionResponse ToResponse(UserSessionDto source);

	public static partial NoteResponse ToResponse(Note source);

	public static partial ReminderResponse ToResponse(Reminder source);

	public static partial TradeStrategyResponse ToResponse(TradeStrategy source);

	public static partial UserTradeStrategyResponse ToResponse(UserTradeStrategy source);

	public static partial UserStrategyTradeCodeResponse ToResponse(UserStrategyTradeCode source);

	public static partial TradeCodeResponse ToResponse(TradeCode source);

	public static partial TradeCodeFileResponse ToResponse(TradeCodeFileDto source);

	public static partial TradeResponse ToResponse(TradeDto source);

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
