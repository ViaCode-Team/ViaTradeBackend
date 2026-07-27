using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace ViaTradeBackend.Contracts.Trades;

public record CreateTradeRequest(
	DateTime OpenedAt,
	DateTime? ClosedAt,
	[Range(double.Epsilon, double.MaxValue)] double EntryPrice,
	[Range(double.Epsilon, double.MaxValue)] double? ExitPrice,
	[EnumDataType(typeof(TradeSignal))] TradeSignal Signal,
	[Range(1, int.MaxValue)] int Quantity,
	[Range(1, int.MaxValue)] int TradeTypeId,
	[Range(1, int.MaxValue)] int InstrumentId
) : IValidatableObject
{
	public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
	{
		if (ClosedAt.HasValue && ClosedAt.Value < OpenedAt)
			yield return new ValidationResult(
			"closedAt must be greater than or equal to openedAt.",
			[nameof(OpenedAt), nameof(ClosedAt)]
		);

		if (ClosedAt.HasValue != ExitPrice.HasValue)
			yield return new ValidationResult(
			"closedAt and exitPrice must either both be specified or both be omitted.",
			[nameof(ClosedAt), nameof(ExitPrice)]
		);
	}
}
