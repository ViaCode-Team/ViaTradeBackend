using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ViaTrade.Api.Attributes.Binding;

public sealed class FromHeaderPropertiesAttribute(params string[] propertyNames)
	: FromRequestPropertiesAttribute(BindingSource.Header, propertyNames) { }
