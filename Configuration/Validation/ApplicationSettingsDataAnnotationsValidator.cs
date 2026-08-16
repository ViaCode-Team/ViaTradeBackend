using Microsoft.Extensions.Options;
using ViaTrade.Configuration.Options;

namespace ViaTrade.Configuration.Validation;

[OptionsValidator]
public partial class ApplicationSettingsDataAnnotationsValidator : IValidateOptions<ApplicationSettings>;
