using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ViaTrade.Api.ModelBinding;

internal sealed class RequestPropertiesApplicationModelProvider(IModelMetadataProvider metadataProvider)
	: IApplicationModelProvider
{
	public int Order => -950;

	public void OnProvidersExecuting(ApplicationModelProviderContext context)
	{
		var parameters = context
			.Result.Controllers.SelectMany(controller => controller.Actions)
			.SelectMany(action => action.Parameters);

		foreach (var parameter in parameters)
			RequestPropertiesConvention.ConfigureBindingSource(parameter);
	}

	public void OnProvidersExecuted(ApplicationModelProviderContext context)
	{
		var parameters = context
			.Result.Controllers.SelectMany(controller => controller.Actions)
			.SelectMany(action => action.Parameters);

		foreach (var parameter in parameters)
		{
			if (
				parameter.BindingInfo?.BindingSource != BindingSource.Query
				|| parameter.BindingInfo.BinderType is not null
			)
				continue;

			var metadata = ((ModelMetadataProvider)metadataProvider).GetMetadataForParameter(parameter.ParameterInfo);
			if (metadata.BinderType is not null)
				continue;

			var configuration = RequestPropertyBinding.From(metadata, parameter.BindingInfo.BindingSource);
			if (configuration.RequiresCustomBinding(metadata, parameter.BindingInfo.BindingSource))
				configuration.ValidateClientNames(metadata);
		}
	}
}
