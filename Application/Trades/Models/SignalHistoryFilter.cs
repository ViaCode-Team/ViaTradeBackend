using System.ComponentModel.DataAnnotations;

namespace Application.Trades.Models;

public sealed class SignalHistoryFilter : IValidatableObject
{
	[Range(1, int.MaxValue)]
	public required int StrategyId { get; set; }

	[Range(1, int.MaxValue)]
	public required int InstrumentId { get; set; }

	public DateTime? StartDate { get; set; }

	public DateTime? EndDate { get; set; }

	public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
	{
		if (StartDate.HasValue && EndDate.HasValue && StartDate.Value > EndDate.Value)
			yield return new ValidationResult(
				"startDate must be less than or equal to endDate.",
				[nameof(StartDate), nameof(EndDate)]
			);
	}
}
