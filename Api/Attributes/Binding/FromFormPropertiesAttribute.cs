using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ViaTrade.Api.Attributes.Binding;

public sealed class FromFormPropertiesAttribute(params string[] propertyNames)
	: FromRequestPropertiesAttribute(BindingSource.Form, propertyNames) { }
