using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ViaTrade.Api.ModelBinding.Attributes;

public sealed class FromBodyPropertiesAttribute(params string[] propertyNames)
	: FromRequestPropertiesAttribute(BindingSource.Body, propertyNames) { }
