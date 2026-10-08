using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using SystemAttribute = System.Attribute;

namespace ViaTrade.Api.ModelBinding.Attributes;

[AttributeUsage(AttributeTargets.Parameter)]
public sealed class IgnorePropertiesAttribute(params string[] propertyNames) : SystemAttribute
{
	public IReadOnlyList<string> PropertyNames { get; } = RequestPropertyNames.Validate(propertyNames);

	internal static IgnorePropertiesAttribute? Find(ModelMetadata metadata)
	{
		if (metadata is DefaultModelMetadata defaultMetadata)
			return defaultMetadata
				.Attributes.ParameterAttributes?.OfType<IgnorePropertiesAttribute>()
				.SingleOrDefault();

		return null;
	}
}
