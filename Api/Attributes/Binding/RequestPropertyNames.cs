using System.Reflection;

namespace ViaTrade.Api.Attributes.Binding;

internal static class RequestPropertyNames
{
	public static IReadOnlyList<string> Validate(string[] propertyNames)
	{
		ArgumentNullException.ThrowIfNull(propertyNames);

		if (propertyNames.Length == 0 || propertyNames.Any(string.IsNullOrWhiteSpace))
			throw new ArgumentException("Specify at least one non-empty CLR property name.", nameof(propertyNames));

		return Array.AsReadOnly(propertyNames.Distinct(StringComparer.Ordinal).ToArray());
	}

	public static PropertyInfo GetProperty(Type modelType, string propertyName)
	{
		var property = modelType.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
		if (
			property is null
			|| property.GetMethod is not { IsPublic: true }
			|| property.GetIndexParameters().Length != 0
		)
			throw new InvalidOperationException(
				$"'{propertyName}' is not a readable public property of '{modelType.Name}'."
			);

		return property;
	}
}
