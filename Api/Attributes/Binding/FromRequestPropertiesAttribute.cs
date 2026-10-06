using Microsoft.AspNetCore.Mvc.ModelBinding;
using SystemAttribute = System.Attribute;

namespace ViaTrade.Api.Attributes.Binding;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true)]
public abstract class FromRequestPropertiesAttribute : SystemAttribute
{
	public BindingSource Source { get; }

	public IReadOnlyList<string> PropertyNames { get; }

	public string? Name { get; set; }

	protected FromRequestPropertiesAttribute(BindingSource source, params string[] propertyNames)
	{
		ArgumentNullException.ThrowIfNull(source);

		if (
			source != BindingSource.Body
			&& source != BindingSource.Path
			&& source != BindingSource.Query
			&& source != BindingSource.Header
			&& source != BindingSource.Form
			&& source != BindingSource.FormFile
		)
			throw new ArgumentException("Specify a supported HTTP request binding source.", nameof(source));

		Source = source;
		PropertyNames = RequestPropertyNames.Validate(propertyNames);
	}
}
