using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ViaTrade.Api.ModelBinding.Attributes;

public sealed class FromRoutePropertiesAttribute(params string[] propertyNames)
	: FromRequestPropertiesAttribute(BindingSource.Path, propertyNames) { }
