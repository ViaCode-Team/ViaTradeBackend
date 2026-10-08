using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace ViaTrade.Api.ModelBinding;

public sealed class RequestPropertiesModelBinderProvider(BodyModelBinderProvider bodyProvider) : IModelBinderProvider
{
	public IModelBinder? GetBinder(ModelBinderProviderContext context)
	{
		if (
			context.Metadata.MetadataKind != ModelMetadataKind.Parameter
			|| context.BindingInfo.BinderType is not null
			|| context.BindingInfo.BindingSource == BindingSource.Services
			|| context.BindingInfo.BindingSource == BindingSource.FormFile
		)
			return null;

		var configuration = RequestPropertyBinding.From(context.Metadata, context.BindingInfo.BindingSource);
		if (!configuration.RequiresCustomBinding(context.Metadata, context.BindingInfo.BindingSource))
			return null;

		configuration.ValidateClientNames(context.Metadata);

		IModelBinder? bodyBinder = null;
		if (configuration.HasBody && context.BindingInfo.BindingSource == BindingSource.Body)
			bodyBinder = bodyProvider.GetBinder(context);

		var bindings = RequestPropertyBinding
			.GetMembers(context.Metadata)
			.Where(member => member.MetadataKind == ModelMetadataKind.Parameter || member.PropertySetter is not null)
			.Select(member => CreatePropertyBinding(context, configuration, member))
			.ToList();

		return new RequestPropertiesModelBinder(context.Metadata, bodyBinder, bindings, configuration);
	}

	private static RequestPropertyModelBinding CreatePropertyBinding(
		ModelBinderProviderContext context,
		RequestPropertyBinding configuration,
		ModelMetadata memberMetadata
	)
	{
		var propertyName = RequestPropertyBinding.GetPropertyName(memberMetadata);
		var propertyMetadata = context.Metadata.Properties.Single(property =>
			property.PropertyName == propertyName && property.ModelType == memberMetadata.ModelType
		);

		var source = configuration.GetSource(memberMetadata);
		var modelName = configuration.GetModelName(memberMetadata);

		IModelBinder? binder = null;
		BindingInfo? bindingInfo = null;
		if (
			source is not null
			&& source != BindingSource.Body
			&& configuration.IsBindingAllowed(context.Metadata, memberMetadata)
		)
		{
			bindingInfo = CreatePropertyBindingInfo(memberMetadata, source, modelName);

			// Action-specific sources require separate binder cache entries.
			binder = context
				.Services.GetRequiredService<IModelBinderFactory>()
				.CreateBinder(
					new ModelBinderFactoryContext
					{
						Metadata = memberMetadata,
						BindingInfo = bindingInfo,
						CacheToken = bindingInfo,
					}
				);
		}

		return new RequestPropertyModelBinding(
			memberMetadata,
			propertyMetadata,
			binder,
			bindingInfo,
			modelName,
			source
		);
	}

	private static BindingInfo CreatePropertyBindingInfo(
		ModelMetadata memberMetadata,
		BindingSource source,
		string modelName
	)
	{
		var bindingInfo = new BindingInfo();
		bindingInfo.TryApplyBindingInfo(memberMetadata);
		bindingInfo.BindingSource = source;
		bindingInfo.BinderModelName = modelName;

		return bindingInfo;
	}
}
