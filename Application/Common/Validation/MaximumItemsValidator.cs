using FluentValidation;
using FluentValidation.Validators;

namespace ViaTrade.Application.Common.Validation;

public sealed class MaximumItemsValidator<T, TElement> : PropertyValidator<T, List<TElement>?>, IMaximumLengthValidator
{
	public int Min => 0;
	public int Max { get; }
	public override string Name => "MaximumItemsValidator";

	public MaximumItemsValidator(int maximum)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(maximum);
		Max = maximum;
	}

	public override bool IsValid(ValidationContext<T> context, List<TElement>? value)
	{
		context.MessageFormatter.AppendArgument("MaxItems", Max);
		return value is null || value.Count <= Max;
	}

	protected override string GetDefaultMessageTemplate(string errorCode) =>
		"{PropertyName} must contain no more than {MaxItems} items.";
}
