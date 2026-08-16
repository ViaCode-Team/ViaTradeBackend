using System.ComponentModel.DataAnnotations;

namespace ViaTrade.Application.Strategies.Models;

public sealed class StrategyInstrumentFilter : IValidatableObject
{
	public const int MaxInstrumentIds = 100;

	[MaxLength(MaxInstrumentIds)]
	public List<int>? InstrumentIds { get; set; }

	public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
	{
		if (InstrumentIds?.Any(instrumentId => instrumentId < 1) == true)
			yield return new ValidationResult(
				"instrumentIds must contain only positive integers.",
				[nameof(InstrumentIds)]
			);
	}
}
