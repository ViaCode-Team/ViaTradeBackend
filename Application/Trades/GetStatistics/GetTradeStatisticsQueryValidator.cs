using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Trades.GetStatistics;

public sealed class GetTradeStatisticsQueryValidator : UserRequestValidator<GetTradeStatisticsQuery>
{
	public GetTradeStatisticsQueryValidator()
		: base(request => request.UserId) { }
}
