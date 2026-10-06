using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Strategies.GetStatistics;

public sealed class GetStrategyStatisticsQueryValidator : UserRequestValidator<GetStrategyStatisticsQuery>
{
	public GetStrategyStatisticsQueryValidator()
		: base(request => request.UserId) { }
}
