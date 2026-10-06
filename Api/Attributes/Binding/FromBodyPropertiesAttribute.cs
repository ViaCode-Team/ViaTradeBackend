using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ViaTrade.Api.Attributes.Binding;

public sealed class FromBodyPropertiesAttribute(params string[] propertyNames)
	: FromRequestPropertiesAttribute(BindingSource.Body, propertyNames) { }
