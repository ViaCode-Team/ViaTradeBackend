using FluentValidation;
using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Trades.GetProfitChart;

public sealed class GetProfitChartQueryValidator : AbstractValidator<GetProfitChartQuery>
{
	public GetProfitChartQueryValidator()
	{
		RuleFor(request => request.ProfitChartFilter).RequiredValid(new ProfitChartFilterValidator());
	}
}
