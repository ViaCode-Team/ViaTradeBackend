using System.Globalization;
using FluentValidation.Validators;
using MicroElements.OpenApi.FluentValidation;
using MicroElements.Swashbuckle.FluentValidation;
using Microsoft.Extensions.Options;

namespace ViaTrade.Api.Swagger;

internal static class FloatingPointRangeRule
{
	public static FluentValidationRule Create(IOptions<SchemaGenerationOptions> options)
	{
		var standard = new DefaultFluentValidationRuleProvider(options)
			.GetRules()
			.Single(rule => rule.Name == "Between");

		return new FluentValidationRule(
			"Between",
			standard.Conditions,
			context =>
			{
				var validator = (IBetweenValidator)context.PropertyValidator;
				if (validator.From is not double && validator.From is not float)
				{
					standard.Apply!(context);
					return;
				}

				var property = context.Property;
				var minimum = Convert.ToDouble(validator.From, CultureInfo.InvariantCulture);
				var maximum = Convert.ToDouble(validator.To, CultureInfo.InvariantCulture);
				var exclusive = validator is not IInclusiveBetweenValidator;
				var currentMinimum = property.ExclusiveMinimum ?? property.Minimum;
				var currentMaximum = property.ExclusiveMaximum ?? property.Maximum;
				if (
					double.IsFinite(minimum)
					&& (
						currentMinimum is null
						|| minimum > double.Parse(currentMinimum, CultureInfo.InvariantCulture)
						|| exclusive && minimum == double.Parse(currentMinimum, CultureInfo.InvariantCulture)
					)
				)
				{
					property.Minimum = minimum.ToString("R", CultureInfo.InvariantCulture);
					property.ExclusiveMinimum = null;
					if (exclusive)
						property.ExclusiveMinimum = property.Minimum;
				}
				if (
					double.IsFinite(maximum)
					&& (
						currentMaximum is null
						|| maximum < double.Parse(currentMaximum, CultureInfo.InvariantCulture)
						|| exclusive && maximum == double.Parse(currentMaximum, CultureInfo.InvariantCulture)
					)
				)
				{
					property.Maximum = maximum.ToString("R", CultureInfo.InvariantCulture);
					property.ExclusiveMaximum = null;
					if (exclusive)
						property.ExclusiveMaximum = property.Maximum;
				}
			}
		);
	}
}
