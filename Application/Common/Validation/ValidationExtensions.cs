using FluentValidation;
using ViaTrade.Application.Common.Models;

namespace ViaTrade.Application.Common.Validation;

public static class ValidationExtensions
{
	public static IRuleBuilderOptions<T, int> PositiveId<T>(this IRuleBuilder<T, int> rule) => rule.GreaterThan(0);

	public static IRuleBuilderOptions<T, TProperty> RequiredValid<T, TProperty>(
		this IRuleBuilder<T, TProperty> rule,
		IValidator<TProperty> validator
	) => rule.NotNull().SetValidator(validator);

	public static IRuleBuilderOptions<T, PageOptions> ValidPageOptions<T>(this IRuleBuilder<T, PageOptions> rule) =>
		rule.RequiredValid(new PageOptionsValidator());

	public static IRuleBuilderOptions<T, TSearch> ValidSearch<T, TSearch>(this IRuleBuilder<T, TSearch> rule)
		where TSearch : BaseSearch => rule.RequiredValid(new SearchValidator<TSearch>());

	public static IRuleBuilderOptions<T, string> RequiredText<T>(this IRuleBuilder<T, string> rule, int min, int max) =>
		rule.NotNull().Length(min, max);

	public static IRuleBuilderOptions<T, List<TElement>?> MaxItems<T, TElement>(
		this IRuleBuilder<T, List<TElement>?> rule,
		int maximum
	) => rule.SetValidator(new MaximumItemsValidator<T, TElement>(maximum));
}
