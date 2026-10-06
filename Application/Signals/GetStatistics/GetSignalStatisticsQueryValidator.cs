using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Signals.GetStatistics;

public sealed class GetSignalStatisticsQueryValidator : UserRequestValidator<GetSignalStatisticsQuery>
{
	public GetSignalStatisticsQueryValidator()
		: base(request => request.UserId) { }
}
