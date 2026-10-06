using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Swagger;
using ViaTrade.Api;
using ViaTrade.Api.Attributes.Binding;
using Xunit;

namespace ViaTrade.Tests;

public sealed class ModelBindingCompatibilityTests(BodyBindingFixture fixture) : IClassFixture<BodyBindingFixture>
{
	[Theory]
	[InlineData("class")]
	[InlineData("record")]
	public async Task BindWhitelistSkipsExcludedValuesBeforeConversion(string kind)
	{
		using var response = await fixture.Client.GetAsync(
			$"/bindingCompatibility/whitelist/{kind}?allowed=2&secret=invalid"
		);
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
		Assert.Equal(2, json.RootElement.GetProperty("allowed").GetInt32());
		Assert.Equal(0, json.RootElement.GetProperty("secret").GetInt32());
		var operation = fixture
			.App.Services.GetRequiredService<ISwaggerProvider>()
			.GetSwagger("web")
			.Paths["/bindingCompatibility/whitelist/" + kind]
			.Operations![HttpMethod.Get];
		Assert.Equal("allowed", Assert.Single(operation.Parameters!).Name);
	}

	[Theory]
	[InlineData("class")]
	[InlineData("record")]
	[InlineData("form")]
	public async Task BindRequiredRejectsMissingValuesWithTheirExternalName(string kind)
	{
		using var request = new HttpRequestMessage(HttpMethod.Get, "/bindingCompatibility/required/" + kind);
		if (kind == "form")
		{
			request.Method = HttpMethod.Post;
			request.Content = new FormUrlEncodedContent(new Dictionary<string, string> { ["other"] = "value" });
		}
		using var response = await fixture.Client.SendAsync(request);
		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
		using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
		Assert.True(json.RootElement.GetProperty("errors").TryGetProperty("external", out _));
		if (kind != "form")
		{
			var operation = fixture
				.App.Services.GetRequiredService<ISwaggerProvider>()
				.GetSwagger("web")
				.Paths["/bindingCompatibility/required/" + kind]
				.Operations![HttpMethod.Get];
			Assert.True(Assert.Single(operation.Parameters!, parameter => parameter.Name == "external").Required);
		}
	}

	[Fact]
	public async Task BindRequiredOnAnEmptyRootModelRejectsMissingInput()
	{
		using var response = await fixture.Client.GetAsync("/bindingCompatibility/required/root");
		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
	}

	[Fact]
	public async Task OrdinaryQueryModelsRetainTheStandardParameterPrefix()
	{
		var model = await fixture.Client.GetFromJsonAsync<CompatibilityWhitelistClass>(
			"/bindingCompatibility/standardQuery?request.allowed=3&request.secret=8"
		);
		Assert.Equal(3, model!.Allowed);
		Assert.Equal(8, model.Secret);
	}

	[Fact]
	public async Task RecordConstructorDefaultsArePreservedAndExplicitValuesOverrideThem()
	{
		var defaults = await fixture.Client.GetFromJsonAsync<CompatibilityDefaults>("/bindingCompatibility/defaults");
		Assert.Equal(new CompatibilityDefaults(), defaults);
		var explicitValues = await fixture.Client.GetFromJsonAsync<CompatibilityDefaults>(
			"/bindingCompatibility/defaults?limit=0&enabled=false&mode=second&optional=2"
		);
		Assert.Equal(new CompatibilityDefaults(0, false, CompatibilityMode.Second, 2), explicitValues);
	}

	[Fact]
	public async Task ExplicitModelBinderTakesPrecedenceOverTheGlobalProvider()
	{
		var model = await fixture.Client.GetFromJsonAsync<CompatibilityBinderModel>(
			"/bindingCompatibility/binder?count=5"
		);
		Assert.Equal(77, model!.Count);
	}

	[Fact]
	public async Task TypeLevelModelBinderTakesPrecedenceOverTheGlobalProvider()
	{
		var model = await fixture.Client.GetFromJsonAsync<CompatibilityBinderModel>(
			"/bindingCompatibility/typeBinder?count=5"
		);
		Assert.Equal(77, model!.Count);
	}

	[Fact]
	public async Task BindNeverSkipsSpoofedValuesBeforeConversion()
	{
		var model = await fixture.Client.GetFromJsonAsync<CompatibilityNeverModel>(
			"/bindingCompatibility/never?count=3&serverValue=invalid"
		);
		Assert.Equal(3, model!.Count);
		Assert.Equal(0, model.ServerValue);
	}

	[Fact]
	public async Task RequestBindingUsesTheValueProvidersSelectedByResourceFilters()
	{
		var model = await fixture.Client.GetFromJsonAsync<CompatibilityProviderModel>(
			"/bindingCompatibility/providers/42?count=5"
		);
		Assert.Equal(42, model!.Id);
		Assert.Equal(0, model.Count);
	}

	[Fact]
	public async Task StandardPropertySourcesIgnoreSpoofedValuesFromOtherSources()
	{
		using var request = new HttpRequestMessage(HttpMethod.Get, "/bindingCompatibility/header?X-Value=spoofed");
		request.Headers.Add("X-Value", "header-value");
		using var response = await fixture.Client.SendAsync(request);
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		var model = await response.Content.ReadFromJsonAsync<CompatibilityHeaderModel>();
		Assert.Equal("header-value", model!.Value);
	}

	[Fact]
	public async Task ExplicitPropertyOverrideTakesPrecedenceOverPropertyMetadata()
	{
		using var request = new HttpRequestMessage(
			HttpMethod.Get,
			"/bindingCompatibility/headerOverride?term=query-value"
		);
		request.Headers.Add("X-Value", "header-value");
		using var response = await fixture.Client.SendAsync(request);
		var model = await response.Content.ReadFromJsonAsync<CompatibilityHeaderModel>();
		Assert.Equal("query-value", model!.Value);
	}

	[Fact]
	public async Task SwaggerQueryNamesMatchBindingMetadataInsteadOfJsonNames()
	{
		var document = fixture.App.Services.GetRequiredService<ISwaggerProvider>().GetSwagger("web");
		var operation = document.Paths["/bindingCompatibility/names"].Operations![HttpMethod.Get];
		Assert.Equal(new[] { "external", "count" }, operation.Parameters!.Select(parameter => parameter.Name));
		var model = await fixture.Client.GetFromJsonAsync<CompatibilityNamesModel>(
			"/bindingCompatibility/names?external=3&count=5"
		);
		Assert.Equal(3, model!.Value);
		Assert.Equal(5, model.Filter.Count);
	}

	[Fact]
	public async Task ExplicitNestedAliasesKeepPrefixesAndAvoidFlatteningCollisions()
	{
		var model = await fixture.Client.GetFromJsonAsync<CompatibilityCollisionModel>(
			"/bindingCompatibility/prefixed?first.pageSize=3&pageSize=7"
		);
		Assert.Equal(3, model!.First.PageSize);
		Assert.Equal(7, model.Second.PageSize);
		var operation = fixture
			.App.Services.GetRequiredService<ISwaggerProvider>()
			.GetSwagger("web")
			.Paths["/bindingCompatibility/prefixed"]
			.Operations![HttpMethod.Get];
		Assert.Equal(new[] { "first.pageSize", "pageSize" }, operation.Parameters!.Select(parameter => parameter.Name));
	}

	[Fact]
	public async Task FlattenedNameCollisionsFailDuringActionDiscovery()
	{
		var builder = WebApplication.CreateBuilder();
		builder.WebHost.UseUrls("http://127.0.0.1:0");
		builder.Logging.ClearProviders();
		builder.Services.AddWebPresentation();
		builder
			.Services.AddControllers()
			.ConfigureApplicationPartManager(manager =>
				manager.FeatureProviders.Add(new CollisionControllerFeatureProvider())
			);
		await using var app = builder.Build();
		var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
		{
			app.MapControllers();
			await app.StartAsync();
		});
		Assert.Contains("pageSize", exception.Message, StringComparison.OrdinalIgnoreCase);
	}

	[Theory]
	[InlineData("{}")]
	[InlineData("{\"userId\":{\"spoofed\":true},\"id\":\"invalid\"}")]
	public async Task JsonRequiredServerFieldsAreExcludedBeforeDeserialization(string serverFields)
	{
		using var content = new StringContent(serverFields, Encoding.UTF8, "application/json");
		using var response = await fixture.Client.PostAsync("/bindingCompatibility/jsonRequired/42", content);
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		var model = await response.Content.ReadFromJsonAsync<CompatibilityRequiredBody>();
		Assert.Equal(7, model!.UserId);
		Assert.Equal(42, model.Id);
	}

	[Fact]
	public async Task JsonRequiredClientFieldsRemainRequiredOnOtherParameters()
	{
		using var response = await fixture.Client.PostAsJsonAsync(
			"/bindingCompatibility/jsonRequiredPlain",
			new { id = 42 }
		);
		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
	}

	[Fact]
	public async Task JsonRequiredExclusionsDoNotAffectNestedModelsOfTheSameType()
	{
		using var invalid = await fixture.Client.PostAsJsonAsync(
			"/bindingCompatibility/jsonRequiredRecursive",
			new { child = new { } }
		);
		Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
		using var valid = await fixture.Client.PostAsJsonAsync(
			"/bindingCompatibility/jsonRequiredRecursive",
			new { child = new { userId = 9 } }
		);
		Assert.Equal(HttpStatusCode.OK, valid.StatusCode);
		using var json = JsonDocument.Parse(await valid.Content.ReadAsStringAsync());
		Assert.Equal(7, json.RootElement.GetProperty("userId").GetInt32());
		Assert.Equal(9, json.RootElement.GetProperty("child").GetProperty("userId").GetInt32());
	}

	[Fact]
	public async Task SwaggerPreservesScalarFormParametersAlongsideModels()
	{
		using var response = await fixture.Client.PostAsync(
			"/bindingCompatibility/formScalar",
			new FormUrlEncodedContent(
				new Dictionary<string, string> { ["firstText"] = "first", ["external"] = "second" }
			)
		);
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		Assert.Equal(new[] { "first", "second" }, await response.Content.ReadFromJsonAsync<string[]>());
		var operation = fixture
			.App.Services.GetRequiredService<ISwaggerProvider>()
			.GetSwagger("web")
			.Paths["/bindingCompatibility/formScalar"]
			.Operations![HttpMethod.Post];
		Assert.Equal(
			new[] { "firstText", "external" },
			operation.RequestBody!.Content!["application/x-www-form-urlencoded"].Schema!.Properties!.Keys
		);
	}

	[Fact]
	public async Task SwaggerMergesAllFormParametersIntoOneBody()
	{
		using var response = await fixture.Client.PostAsync(
			"/bindingCompatibility/form",
			new FormUrlEncodedContent(
				new Dictionary<string, string> { ["firstText"] = "first", ["secondText"] = "second" }
			)
		);
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		Assert.Equal(new[] { "first", "second" }, await response.Content.ReadFromJsonAsync<string[]>());
		var operation = fixture
			.App.Services.GetRequiredService<ISwaggerProvider>()
			.GetSwagger("web")
			.Paths["/bindingCompatibility/form"]
			.Operations![HttpMethod.Post];
		var schema = operation.RequestBody!.Content!["application/x-www-form-urlencoded"].Schema!;
		Assert.Equal(new[] { "firstText", "secondText" }, schema.Properties!.Keys);
		Assert.Equal(new[] { "firstText", "secondText" }, schema.Required!);
	}

	private sealed class CollisionControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
	{
		public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
		{
			feature.Controllers.Clear();
			feature.Controllers.Add(typeof(CompatibilityCollisionController).GetTypeInfo());
		}
	}
}

[ApiController]
[Route("bindingCompatibility")]
public sealed class BindingCompatibilityController : ControllerBase
{
	[HttpGet("whitelist/class")]
	public CompatibilityWhitelistClass WhitelistClass(
		[
			FromQuery,
			Bind(nameof(CompatibilityWhitelistClass.Allowed)),
			FromQueryProperties(nameof(CompatibilityWhitelistClass.Allowed))
		]
			CompatibilityWhitelistClass model
	) => model;

	[HttpGet("whitelist/record")]
	public CompatibilityWhitelistRecord WhitelistRecord(
		[
			FromQuery,
			Bind(nameof(CompatibilityWhitelistRecord.Allowed)),
			FromQueryProperties(nameof(CompatibilityWhitelistRecord.Allowed))
		]
			CompatibilityWhitelistRecord model
	) => model;

	[HttpGet("required/class")]
	public CompatibilityRequiredClass RequiredClass(
		[FromQuery, FromQueryProperties(nameof(CompatibilityRequiredClass.Count))] CompatibilityRequiredClass model
	) => model;

	[HttpGet("required/record")]
	public CompatibilityRequiredRecord RequiredRecord(
		[FromQuery, FromQueryProperties(nameof(CompatibilityRequiredRecord.Count))] CompatibilityRequiredRecord model
	) => model;

	[HttpPost("required/form")]
	public CompatibilityRequiredClass RequiredForm(
		[FromForm, IgnoreProperties(nameof(CompatibilityRequiredClass.ServerValue))] CompatibilityRequiredClass model
	) => model;

	[HttpGet("defaults")]
	public CompatibilityDefaults Defaults(
		[FromQuery, FromQueryProperties(nameof(CompatibilityDefaults.Limit))] CompatibilityDefaults model
	) => model;

	[HttpGet("required/root")]
	public CompatibilityWhitelistClass RequiredRoot(
		[FromQuery, BindRequired, FromQueryProperties(nameof(CompatibilityWhitelistClass.Allowed))]
			CompatibilityWhitelistClass model
	) => model;

	[HttpGet("standardQuery")]
	public CompatibilityWhitelistClass StandardQuery([FromQuery(Name = "request")] CompatibilityWhitelistClass model) =>
		model;

	[HttpGet("binder")]
	public CompatibilityBinderModel Binder(
		[
			FromQuery,
			ModelBinder(typeof(CompatibilityModelBinder)),
			FromQueryProperties(nameof(CompatibilityBinderModel.Count))
		]
			CompatibilityBinderModel model
	) => model;

	[HttpGet("header")]
	public CompatibilityHeaderModel Header(
		[FromQuery, FromQueryProperties(nameof(CompatibilityHeaderModel.Count))] CompatibilityHeaderModel model
	) => model;

	[HttpGet("typeBinder")]
	public CompatibilityBinderModel TypeBinder(
		[FromQuery, FromQueryProperties(nameof(CompatibilityBinderModel.Count))] CompatibilityBinderModel model
	) => model;

	[HttpGet("never")]
	public CompatibilityNeverModel Never(
		[FromQuery, FromQueryProperties(nameof(CompatibilityNeverModel.Count))] CompatibilityNeverModel model
	) => model;

	[HttpGet("providers/{id:int}")]
	[SkipQueryValueProvider]
	public CompatibilityProviderModel Providers(
		[FromQuery, FromRouteProperties(nameof(CompatibilityProviderModel.Id))] CompatibilityProviderModel model
	) => model;

	[HttpGet("headerOverride")]
	public CompatibilityHeaderModel HeaderOverride(
		[FromQuery, FromQueryProperties(nameof(CompatibilityHeaderModel.Value), Name = "term")]
			CompatibilityHeaderModel model
	) => model;

	[HttpGet("names")]
	public CompatibilityNamesModel Names([FromQuery] CompatibilityNamesModel model) => model;

	[HttpGet("prefixed")]
	public CompatibilityCollisionModel Prefixed(
		[FromQuery, FromQueryProperties(nameof(CompatibilityCollisionModel.First), Name = "first")]
			CompatibilityCollisionModel model
	) => model;

	[HttpPost("jsonRequired/{id:int}")]
	public CompatibilityRequiredBody RequiredBody(
		[
			FromBody,
			IgnoreProperties(nameof(CompatibilityRequiredBody.UserId)),
			FromRouteProperties(nameof(CompatibilityRequiredBody.Id))
		]
			CompatibilityRequiredBody model
	) => model with { UserId = 7 };

	[HttpPost("jsonRequiredPlain")]
	public CompatibilityRequiredBody RequiredBodyPlain([FromBody] CompatibilityRequiredBody model) => model;

	[HttpPost("form")]
	public string[] Form(
		[FromForm, IgnoreProperties(nameof(CompatibilityFirstForm.ServerFirst))] CompatibilityFirstForm first,
		[FromForm, IgnoreProperties(nameof(CompatibilitySecondForm.ServerSecond))] CompatibilitySecondForm second
	) => [first.FirstText, second.SecondText];

	[HttpPost("jsonRequiredRecursive")]
	public CompatibilityRecursiveRequiredBody RecursiveRequiredBody(
		[FromBody, IgnoreProperties(nameof(CompatibilityRecursiveRequiredBody.UserId))]
			CompatibilityRecursiveRequiredBody model
	) => model with { UserId = 7 };

	[HttpPost("formScalar")]
	public string[] FormScalar(
		[FromForm, IgnoreProperties(nameof(CompatibilityFirstForm.ServerFirst))] CompatibilityFirstForm first,
		[FromForm(Name = "external")] string text
	) => [first.FirstText, text];
}

[NonController]
[ApiController]
[Route("bindingCollision")]
public sealed class CompatibilityCollisionController : ControllerBase
{
	[HttpGet]
	public CompatibilityCollisionModel Get([FromQuery] CompatibilityCollisionModel model) => model;
}

public sealed class CompatibilityWhitelistClass
{
	public int Allowed { get; set; }
	public int Secret { get; set; }
}

public sealed record CompatibilityWhitelistRecord(int Allowed, int Secret);

public sealed class CompatibilityRequiredClass
{
	[BindRequired, ModelBinder(Name = "external")]
	public int Count { get; set; }

	public int ServerValue { get; set; }
}

public sealed record CompatibilityRequiredRecord([BindRequired, ModelBinder(Name = "external")] int Count);

public sealed record CompatibilityDefaults(
	int Limit = 20,
	bool Enabled = true,
	CompatibilityMode Mode = CompatibilityMode.Second,
	int? Optional = 6
);

public enum CompatibilityMode
{
	First,
	Second,
}

[ModelBinder(typeof(CompatibilityModelBinder))]
public sealed class CompatibilityBinderModel
{
	public int Count { get; set; }
}

public sealed class CompatibilityModelBinder : IModelBinder
{
	public Task BindModelAsync(ModelBindingContext bindingContext)
	{
		bindingContext.Result = ModelBindingResult.Success(new CompatibilityBinderModel { Count = 77 });
		return Task.CompletedTask;
	}
}

public sealed class CompatibilityHeaderModel
{
	public int Count { get; set; }

	[FromHeader(Name = "X-Value")]
	public string? Value { get; set; }
}

public sealed record CompatibilityNamesModel([FromQuery(Name = "external")] int Value, CompatibilityJsonFilter Filter);

public sealed class CompatibilityJsonFilter
{
	[JsonPropertyName("external_count")]
	public int Count { get; set; }
}

public sealed record CompatibilityCollisionModel(CompatibilityPage First, CompatibilityPage Second);

public sealed class CompatibilityPage
{
	public int PageSize { get; set; } = 20;
}

public sealed record CompatibilityRequiredBody
{
	public required int UserId { get; init; }

	[JsonRequired]
	public int Id { get; init; }
}

public sealed class CompatibilityFirstForm
{
	public int ServerFirst { get; set; }

	[Required]
	public string FirstText { get; set; } = null!;
}

public sealed record CompatibilityRecursiveRequiredBody
{
	public required int UserId { get; init; }
	public CompatibilityRecursiveRequiredBody? Child { get; init; }
}

public sealed class CompatibilitySecondForm
{
	public int ServerSecond { get; set; }

	[Required]
	public string SecondText { get; set; } = null!;
}

public sealed record CompatibilityNeverModel(int Count, [BindNever] int ServerValue);

public sealed record CompatibilityProviderModel(int Id, int Count);

[AttributeUsage(AttributeTargets.Method)]
public sealed class SkipQueryValueProviderAttribute : Attribute, IResourceFilter
{
	public void OnResourceExecuting(ResourceExecutingContext context)
	{
		foreach (var factory in context.ValueProviderFactories.OfType<QueryStringValueProviderFactory>().ToArray())
			context.ValueProviderFactories.Remove(factory);
	}

	public void OnResourceExecuted(ResourceExecutedContext context) { }
}
