using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using ViaTrade.Api.ModelBinding.Attributes;

namespace ViaTrade.Api.ModelBinding;

internal sealed class RequestPropertyBinding
{
	private static readonly ConditionalWeakTable<
		ModelMetadata,
		ConditionalWeakTable<BindingSource, RequestPropertyBinding>
	> Configurations = new();

	private readonly Type _modelType;
	private readonly PropertyInfo[] _modelProperties;
	private readonly HashSet<string> _formFilePropertyNames;
	private readonly Dictionary<string, FromRequestPropertiesAttribute> _sourceOverrides = new(StringComparer.Ordinal);

	public BindingSource DefaultSource { get; }
	public IReadOnlyList<string> IgnoredPropertyNames { get; }
	public bool HasOverrides => _sourceOverrides.Count > 0;
	public bool HasDefaultProperties =>
		_modelProperties.Any(property =>
			!IgnoredPropertyNames.Contains(property.Name) && !_sourceOverrides.ContainsKey(property.Name)
		);
	public bool HasBody => _modelProperties.Any(property => GetSource(property.Name) == BindingSource.Body);
	public bool HasFormFiles => _modelProperties.Any(property => GetSource(property.Name) == BindingSource.FormFile);
	public bool HasForm =>
		_modelProperties.Any(property =>
			GetSource(property.Name) == BindingSource.Form || GetSource(property.Name) == BindingSource.FormFile
		);

	private RequestPropertyBinding(Type modelType, IEnumerable<object> attributes, BindingSource? source)
	{
		_modelType = modelType;
		_modelProperties = modelType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
		_formFilePropertyNames = _modelProperties
			.Where(property => IsFormFile(property.PropertyType))
			.Select(property => property.Name)
			.ToHashSet(StringComparer.Ordinal);

		var parameterAttributes = attributes.ToArray();
		DefaultSource = GetDefaultSource(parameterAttributes, source);
		IgnoredPropertyNames =
			parameterAttributes.OfType<IgnorePropertiesAttribute>().SingleOrDefault()?.PropertyNames ?? [];

		foreach (var attribute in parameterAttributes.OfType<FromRequestPropertiesAttribute>())
			AddOverride(attribute);

		if (HasBody && HasForm)
			throw new InvalidOperationException("JSON body and form properties cannot share a request parameter.");

		var names = new HashSet<(BindingSource Source, string Name)>();
		foreach (var property in _modelProperties)
			AddClientName(names, GetSource(property.Name), GetName(property.Name));
	}

	public BindingSource? GetSource(string propertyName, ModelMetadata? member = null)
	{
		if (IgnoredPropertyNames.Contains(propertyName))
			return null;

		var hasOverride = _sourceOverrides.TryGetValue(propertyName, out var sourceOverride);
		var source = DefaultSource;
		if (hasOverride)
			source = sourceOverride!.Source;
		else if (DefaultSource != BindingSource.Body && member?.BindingSource is { } memberSource)
			source = memberSource;

		if (source == BindingSource.Form && _formFilePropertyNames.Contains(propertyName))
			return BindingSource.FormFile;

		return source;
	}

	public string GetName(string propertyName) =>
		_sourceOverrides.GetValueOrDefault(propertyName)?.Name ?? propertyName;

	public bool HasAlias(string propertyName) => _sourceOverrides.GetValueOrDefault(propertyName)?.Name is not null;

	public BindingSource? GetSource(ModelMetadata member) => GetSource(GetPropertyName(member), member);

	public string GetName(ModelMetadata member)
	{
		var propertyName = GetPropertyName(member);
		return _sourceOverrides.GetValueOrDefault(propertyName)?.Name ?? member.BinderModelName ?? propertyName;
	}

	public bool IsFlattened(ModelMetadata member) =>
		GetSource(member) == BindingSource.Query
		&& member.IsComplexType
		&& !member.IsEnumerableType
		&& member.IsRequired
		&& member.BinderType is null
		&& member.BinderModelName is null or ""
		&& !HasAlias(GetPropertyName(member));

	public string GetModelName(ModelMetadata member)
	{
		if (IsFlattened(member))
			return string.Empty;

		return GetName(member);
	}

	public bool IsBindingAllowed(ModelMetadata metadata, ModelMetadata member) =>
		GetSource(member) is not null
		&& member.IsBindingAllowed
		&& metadata.PropertyFilterProvider?.PropertyFilter(member) != false;

	public bool RequiresCustomBinding(ModelMetadata metadata, BindingSource? source)
	{
		if (HasOverrides || HasFormFiles || IgnoredPropertyNames.Count > 0)
			return true;

		return source == BindingSource.Query
			&& metadata.BindingSource == BindingSource.Query
			&& metadata.IsComplexType
			&& !metadata.IsEnumerableType
			&& GetMembers(metadata).Any(IsFlattened);
	}

	public void ValidateClientNames(ModelMetadata metadata)
	{
		var names = new HashSet<(BindingSource Source, string Name)>();
		foreach (var member in GetMembers(metadata).Where(member => IsBindingAllowed(metadata, member)))
		{
			var source = GetSource(member)!;
			IEnumerable<string> memberNames = [GetName(member)];
			if (IsFlattened(member))
				memberNames = GetMembers(member)
					.Where(child =>
						child.IsBindingAllowed && member.PropertyFilterProvider?.PropertyFilter(child) != false
					)
					.Select(child => child.BinderModelName ?? GetPropertyName(child));

			foreach (var name in memberNames)
				AddClientName(names, source, name);
		}
	}

	public static string GetPropertyName(ModelMetadata member) => member.ParameterName ?? member.PropertyName!;

	public static IEnumerable<ModelMetadata> GetMembers(ModelMetadata metadata)
	{
		var constructorParameters = metadata.BoundConstructor?.BoundConstructorParameters ?? [];
		var remainingProperties = metadata.Properties.Where(property =>
			!constructorParameters.Any(parameter =>
				parameter.ParameterName == property.PropertyName && parameter.ModelType == property.ModelType
			)
		);

		return constructorParameters.Concat(remainingProperties);
	}

	public HashSet<string> GetJsonBodyExclusions(JsonSerializerOptions options)
	{
		var comparer = StringComparer.Ordinal;
		if (options.PropertyNameCaseInsensitive)
			comparer = StringComparer.OrdinalIgnoreCase;

		return _modelProperties
			.Where(property => GetSource(property.Name) != BindingSource.Body)
			.Select(property => GetJsonName(property, options))
			.ToHashSet(comparer);
	}

	public static RequestPropertyBinding From(ModelMetadata metadata, BindingSource? source = null) =>
		Configurations
			.GetOrCreateValue(metadata)
			.GetValue(
				source ?? metadata.BindingSource ?? BindingSource.ModelBinding,
				value => new RequestPropertyBinding(
					metadata.ModelType,
					((DefaultModelMetadata)metadata).Attributes.ParameterAttributes ?? [],
					value
				)
			);

	public static RequestPropertyBinding From(ParameterInfo parameter, BindingSource? source = null) =>
		new(parameter.ParameterType, parameter.GetCustomAttributes(), source);

	public static RequestPropertyBinding From(ParameterInfo parameter, ActionDescriptor action) =>
		From(
			parameter,
			action.Parameters.Single(descriptor => descriptor.Name == parameter.Name).BindingInfo?.BindingSource
		);

	public static string GetJsonName(PropertyInfo property, JsonSerializerOptions options) =>
		property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name
		?? options.PropertyNamingPolicy?.ConvertName(property.Name)
		?? property.Name;

	public static bool IsFormFile(Type type) =>
		type == typeof(IFormFile)
		|| type == typeof(IFormFileCollection)
		|| typeof(IEnumerable<IFormFile>).IsAssignableFrom(type);

	private static BindingSource GetDefaultSource(IEnumerable<object> parameterAttributes, BindingSource? source)
	{
		if (parameterAttributes.OfType<FromBodyAttribute>().Any())
			return BindingSource.Body;
		if (parameterAttributes.OfType<FromFormAttribute>().Any())
			return BindingSource.Form;
		if (parameterAttributes.OfType<FromQueryAttribute>().Any())
			return BindingSource.Query;

		return source ?? BindingSource.ModelBinding;
	}

	private void AddOverride(FromRequestPropertiesAttribute attribute)
	{
		if (
			attribute.Name is not null
			&& (string.IsNullOrWhiteSpace(attribute.Name) || attribute.PropertyNames.Count != 1)
		)
			throw new InvalidOperationException(
				"A request property alias requires exactly one property and a non-empty name."
			);
		if (attribute.Source == BindingSource.Body && attribute.Name is not null)
			throw new InvalidOperationException("Body property names follow the JSON serializer configuration.");

		foreach (var propertyName in attribute.PropertyNames)
		{
			var property = RequestPropertyNames.GetProperty(_modelType, propertyName);
			ValidatePropertySource(property, attribute.Source);
			if (IgnoredPropertyNames.Contains(propertyName) || !_sourceOverrides.TryAdd(propertyName, attribute))
				throw new InvalidOperationException($"'{propertyName}' has conflicting request binding configuration.");
		}
	}

	private static void ValidatePropertySource(PropertyInfo property, BindingSource source)
	{
		if (property.SetMethod is not { IsPublic: true })
			throw new InvalidOperationException($"'{property.Name}' must have a public setter or init accessor.");
		if (source == BindingSource.FormFile && !IsFormFile(property.PropertyType))
			throw new InvalidOperationException("Form file properties must have an IFormFile or file collection type.");
		if (source == BindingSource.Path && !IsSimple(property.PropertyType))
			throw new InvalidOperationException("Route properties must have a scalar type.");
		if (
			source == BindingSource.Header
			&& !IsSimple(property.PropertyType)
			&& property.PropertyType != typeof(string[])
		)
			throw new InvalidOperationException("Header properties must have a scalar type or string array type.");
	}

	private static void AddClientName(
		HashSet<(BindingSource Source, string Name)> names,
		BindingSource? source,
		string name
	)
	{
		if (source is null || source == BindingSource.Body)
			return;
		if (source == BindingSource.FormFile)
			source = BindingSource.Form;
		if (!names.Add((source, name.ToUpperInvariant())))
			throw new InvalidOperationException($"Duplicate {source.Id} request field '{name}'.");
	}

	private static bool IsSimple(Type type) =>
		TypeDescriptor.GetConverter(Nullable.GetUnderlyingType(type) ?? type).CanConvertFrom(typeof(string));
}
