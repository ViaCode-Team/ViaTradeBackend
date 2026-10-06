using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Trades.GetProfitChart;

public sealed class GetProfitChartQueryValidator : UserRequestValidator<GetProfitChartQuery>
{
	public GetProfitChartQueryValidator()
		: base(request => request.UserId)
	{
		RuleFor(request => request.ProfitChartFilter).RequiredValid(new ProfitChartFilterValidator());
	}
}
