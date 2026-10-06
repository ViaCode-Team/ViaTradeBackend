using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ViaTrade.Api.Attributes.Binding;

public sealed class FromFormFilePropertiesAttribute(params string[] propertyNames)
	: FromRequestPropertiesAttribute(BindingSource.FormFile, propertyNames) { }
