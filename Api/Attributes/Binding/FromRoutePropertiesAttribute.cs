using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ViaTrade.Api.Attributes.Binding;

public sealed class FromRoutePropertiesAttribute(params string[] propertyNames)
	: FromRequestPropertiesAttribute(BindingSource.Path, propertyNames) { }
