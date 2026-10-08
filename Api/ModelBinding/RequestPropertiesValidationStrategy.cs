using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ViaTrade.Api.ModelBinding;

internal sealed class RequestPropertiesValidationStrategy(RequestPropertyBinding configuration) : IValidationStrategy
{
	public IEnumerator<ValidationEntry> GetChildren(ModelMetadata metadata, string key, object model) =>
		GetEntries(metadata, key, model).GetEnumerator();

	private IEnumerable<ValidationEntry> GetEntries(ModelMetadata metadata, string key, object model)
	{
		foreach (var member in RequestPropertyBinding.GetMembers(metadata))
		{
			var propertyName = RequestPropertyBinding.GetPropertyName(member);
			if (configuration.GetSource(member) is null)
				continue;

			var propertyMetadata = metadata.Properties.First(property =>
				property.PropertyName == propertyName && property.ModelType == member.ModelType
			);

			var validationKey = ModelNames.CreatePropertyModelName(key, configuration.GetModelName(member));

			yield return new ValidationEntry(member, validationKey, () => propertyMetadata.PropertyGetter!(model));
		}
	}
}
