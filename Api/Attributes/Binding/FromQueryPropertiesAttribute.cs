using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ViaTrade.Api.Attributes.Binding;

public sealed class FromQueryPropertiesAttribute(params string[] propertyNames)
	: FromRequestPropertiesAttribute(BindingSource.Query, propertyNames) { }
