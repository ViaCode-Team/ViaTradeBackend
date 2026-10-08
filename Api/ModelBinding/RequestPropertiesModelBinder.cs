using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ViaTrade.Api.ModelBinding;

internal sealed class RequestPropertiesModelBinder(
	ModelMetadata metadata,
	IModelBinder? bodyBinder,
	IReadOnlyList<RequestPropertyModelBinding> bindings,
	RequestPropertyBinding configuration
) : IModelBinder
{
	private readonly IValidationStrategy _validationStrategy = new RequestPropertiesValidationStrategy(configuration);

	public async Task BindModelAsync(ModelBindingContext bindingContext)
	{
		bindingContext.ModelName = string.Empty;
		object? model = null;
		if (bodyBinder is not null)
		{
			await bodyBinder.BindModelAsync(bindingContext);
			if (!bindingContext.Result.IsModelSet || bindingContext.Result.Model is null)
				return;
			model = bindingContext.Result.Model;
		}
		else if (metadata.BoundConstructor is null)
			model = Activator.CreateInstance(metadata.ModelType)!;

		var propertyResults = await BindPropertiesAsync(bindingContext, model);
		model = CreateAndPopulateModel(model, propertyResults);

		if (metadata.IsBindingRequired && !propertyResults.Any(result => result.IsModelSet))
		{
			var message = metadata.ModelBindingMessageProvider.MissingBindRequiredValueAccessor(
				bindingContext.FieldName
			);
			bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, message);
		}

		bindingContext.Result = ModelBindingResult.Success(model);
		bindingContext.ValidationState[model!] = new ValidationStateEntry { Strategy = _validationStrategy };
	}

	private async Task<ModelBindingResult[]> BindPropertiesAsync(ModelBindingContext bindingContext, object? model)
	{
		var valueProvider = bindingContext.ValueProvider;
		if (bindingContext is DefaultModelBindingContext defaultContext)
			valueProvider = defaultContext.OriginalValueProvider;

		var propertyResults = new ModelBindingResult[bindings.Count];
		for (var index = 0; index < bindings.Count; index++)
			propertyResults[index] = await BindPropertyAsync(bindingContext, valueProvider, bindings[index], model);

		return propertyResults;
	}

	private object? CreateAndPopulateModel(object? model, ModelBindingResult[] propertyResults)
	{
		var constructorParameterCount = metadata.BoundConstructor?.BoundConstructorParameters?.Count ?? 0;
		var firstPropertyIndex = 0;
		if (model is null && metadata.BoundConstructor?.BoundConstructorInvoker is { } invokeConstructor)
		{
			var constructorArguments = propertyResults
				.Take(constructorParameterCount)
				.Select(result => result.Model)
				.ToArray();
			model = invokeConstructor(constructorArguments!);
			firstPropertyIndex = constructorParameterCount;
		}

		for (var index = firstPropertyIndex; index < bindings.Count; index++)
			if (propertyResults[index].IsModelSet)
				bindings[index].PropertyMetadata.PropertySetter!(model!, propertyResults[index].Model);

		return model;
	}

	private static async Task<ModelBindingResult> BindPropertyAsync(
		ModelBindingContext bindingContext,
		IValueProvider valueProvider,
		RequestPropertyModelBinding binding,
		object? model
	)
	{
		if (binding.Binder is null || bindingContext.PropertyFilter?.Invoke(binding.MemberMetadata) == false)
			return ModelBindingResult.Failed();

		var propertyContext = DefaultModelBindingContext.CreateBindingContext(
			bindingContext.ActionContext,
			valueProvider,
			binding.MemberMetadata,
			binding.BindingInfo,
			binding.ModelName
		);
		if (model is not null && binding.MemberMetadata.IsComplexType && !binding.MemberMetadata.ModelType.IsArray)
			propertyContext.Model = binding.PropertyMetadata.PropertyGetter!(model);
		if (
			binding.Source == BindingSource.FormFile
			|| binding.MemberMetadata.IsComplexType && !binding.MemberMetadata.IsRequired
		)
			propertyContext.IsTopLevelObject = false;

		await binding.Binder.BindModelAsync(propertyContext);
		if (!propertyContext.Result.IsModelSet)
		{
			if (binding.MemberMetadata.IsBindingRequired)
			{
				var message = binding.MemberMetadata.ModelBindingMessageProvider.MissingBindRequiredValueAccessor(
					binding.ModelName
				);
				bindingContext.ModelState.TryAddModelError(binding.ModelName, message);
			}
			else if (binding.Source == BindingSource.Path)
				bindingContext.ModelState.TryAddModelError(binding.ModelName, "A route value is required.");
		}

		foreach (var entry in propertyContext.ValidationState)
			bindingContext.ValidationState[entry.Key] = entry.Value;

		return propertyContext.Result;
	}
}

internal sealed record RequestPropertyModelBinding(
	ModelMetadata MemberMetadata,
	ModelMetadata PropertyMetadata,
	IModelBinder? Binder,
	BindingInfo? BindingInfo,
	string ModelName,
	BindingSource? Source
);
