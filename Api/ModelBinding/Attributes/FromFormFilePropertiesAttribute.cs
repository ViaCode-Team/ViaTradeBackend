using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ViaTrade.Api.ModelBinding.Attributes;

public sealed class FromFormFilePropertiesAttribute(params string[] propertyNames)
	: FromRequestPropertiesAttribute(BindingSource.FormFile, propertyNames) { }
