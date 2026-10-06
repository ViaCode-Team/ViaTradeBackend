using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Trades.GetProfitChart;

public sealed class ProfitChartFilterValidator : DateRangeValidator<ProfitChartFilter, DateOnly>
{
	public ProfitChartFilterValidator()
		: base(filter => filter.StartDate, filter => filter.EndDate) { }
}
