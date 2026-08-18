using System.ComponentModel.DataAnnotations;

namespace ViaTrade.Application.Strategies.Models;

public record StrategyInstrumentFilter([MaxLength(StrategyInstrumentFilter.MaxInstrumentIds)] List<int>? InstrumentIds)
	: IValidatableObject
{
	public const int MaxInstrumentIds = 100;

	public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
	{
		if (InstrumentIds?.Any(instrumentId => instrumentId < 1) == true)
			yield return new ValidationResult(
				"instrumentIds must contain only positive integers.",
				[nameof(InstrumentIds)]
			);
	}
}
