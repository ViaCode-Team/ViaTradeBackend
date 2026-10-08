using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ViaTrade.Api.ModelBinding.Attributes;

public sealed class FromHeaderPropertiesAttribute(params string[] propertyNames)
	: FromRequestPropertiesAttribute(BindingSource.Header, propertyNames) { }
