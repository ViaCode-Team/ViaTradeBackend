using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing.Patterns;
using ViaTrade.Api.ModelBinding.Attributes;

namespace ViaTrade.Api.ModelBinding;

public sealed class RequestPropertiesConvention : IParameterModelConvention
{
	public void Apply(ParameterModel parameter)
	{
		if (parameter.BindingInfo?.BindingSource == BindingSource.Services)
			return;

		var ignoredProperties = parameter.Attributes.OfType<IgnorePropertiesAttribute>().SingleOrDefault();
		var hasPropertyOverrides = parameter.Attributes.OfType<FromRequestPropertiesAttribute>().Any();
		var hasDefaultFormFiles =
			parameter.BindingInfo?.BindingSource == BindingSource.Form
			&& parameter
				.ParameterType.GetProperties()
				.Any(property => RequestPropertyBinding.IsFormFile(property.PropertyType));

		if (ignoredProperties is null && !hasPropertyOverrides && !hasDefaultFormFiles)
			return;

		ConfigureBindingSource(parameter);
		ValidateDefaultSource(parameter);
		foreach (var propertyName in ignoredProperties?.PropertyNames ?? [])
			RequestPropertyNames.GetProperty(parameter.ParameterType, propertyName);

		var configuration = RequestPropertyBinding.From(parameter.ParameterInfo, parameter.BindingInfo?.BindingSource);
		if (configuration.HasFormFiles && parameter.Action is { } uploadAction)
			ConfigureFileUploads(uploadAction);

		if (hasPropertyOverrides && parameter.Action is { } action)
			ValidateRouteProperties(parameter, configuration, action);
	}

	internal static void ConfigureBindingSource(ParameterModel parameter)
	{
		if (
			!parameter.Attributes.OfType<IgnorePropertiesAttribute>().Any()
			&& !parameter.Attributes.OfType<FromRequestPropertiesAttribute>().Any()
		)
			return;

		var source = parameter.BindingInfo?.BindingSource;
		if (
			source is not null
			&& source != BindingSource.Body
			&& source != BindingSource.Query
			&& source != BindingSource.Form
			&& source != BindingSource.ModelBinding
		)
			return;

		var configuration = RequestPropertyBinding.From(parameter.ParameterInfo, source);
		if (source is null && configuration.HasDefaultProperties && !configuration.HasBody)
			return;

		parameter.BindingInfo ??= new BindingInfo();
		if (configuration.HasBody)
			parameter.BindingInfo.BindingSource = BindingSource.Body;
		else if (source is null || source == BindingSource.Body)
			parameter.BindingInfo.BindingSource = BindingSource.Query;
	}

	private static void ValidateDefaultSource(ParameterModel parameter)
	{
		var source = parameter.BindingInfo?.BindingSource;
		if (
			source is not null
			&& source != BindingSource.Body
			&& source != BindingSource.Query
			&& source != BindingSource.Form
			&& source != BindingSource.ModelBinding
		)
			throw new InvalidOperationException(
				"Request property configuration supports body, query, form or standard MVC model binding."
			);
	}

	private static void ConfigureFileUploads(ActionModel action)
	{
		if (
			action.Parameters.Any(parameter =>
				parameter.BindingInfo?.BindingSource == BindingSource.Body
				|| RequestPropertyBinding.From(parameter.ParameterInfo, parameter.BindingInfo?.BindingSource).HasBody
			)
		)
			throw new InvalidOperationException("JSON body and form files cannot share an action.");

		if (!action.Filters.OfType<ConsumesAttribute>().Any())
			action.Filters.Add(new ConsumesAttribute("multipart/form-data"));
	}

	private static void ValidateRouteProperties(
		ParameterModel parameter,
		RequestPropertyBinding configuration,
		ActionModel action
	)
	{
		var routeProperties = parameter
			.ParameterType.GetProperties()
			.Where(property => configuration.GetSource(property.Name) == BindingSource.Path)
			.ToList();

		foreach (var actionSelector in action.Selectors)
		foreach (var controllerSelector in action.Controller.Selectors)
		{
			var route = AttributeRouteModel.CombineAttributeRouteModel(
				controllerSelector.AttributeRouteModel,
				actionSelector.AttributeRouteModel
			);
			var routeParameterNames = RoutePatternFactory
				.Parse(route?.Template ?? string.Empty)
				.Parameters.Select(routeParameter => routeParameter.Name)
				.ToHashSet(StringComparer.OrdinalIgnoreCase);

			foreach (var routeProperty in routeProperties)
				if (!routeParameterNames.Contains(configuration.GetName(routeProperty.Name)))
					throw new InvalidOperationException(
						$"Route property '{routeProperty.Name}' has no matching route parameter."
					);
		}
	}
}
