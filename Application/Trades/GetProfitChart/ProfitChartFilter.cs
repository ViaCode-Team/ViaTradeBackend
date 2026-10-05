using System.ComponentModel.DataAnnotations;

namespace ViaTrade.Application.Trades.GetProfitChart;

public sealed class ProfitChartFilter : IValidatableObject
{
	public DateOnly? StartDate { get; set; }

	public DateOnly? EndDate { get; set; }

	[Required]
	public ProfitChartGranularity Granularity { get; set; } = ProfitChartGranularity.Day;

	public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
	{
		if (StartDate.HasValue && EndDate.HasValue && StartDate.Value > EndDate.Value)
			yield return new ValidationResult(
				"startDate must be less than or equal to endDate.",
				[nameof(StartDate), nameof(EndDate)]
			);
	}
}
