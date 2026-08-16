using System.ComponentModel.DataAnnotations;
using ViaTrade.Domain.Enums;

namespace ViaTrade.Api.Contracts.Trades;

public record UpdateTradeRequest(
	DateTime OpenedAt,
	DateTime? ClosedAt,
	[Range(double.Epsilon, double.MaxValue)] double OpenPrice,
	[Range(double.Epsilon, double.MaxValue)] double? ClosePrice,
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

		if (ClosedAt.HasValue != ClosePrice.HasValue)
			yield return new ValidationResult(
				"closedAt and closePrice must either both be specified or both be omitted.",
				[nameof(ClosedAt), nameof(ClosePrice)]
			);
	}
}
