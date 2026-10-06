using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.Swagger;
using ViaTrade.Api;
using ViaTrade.Api.Attributes.Binding;
using ViaTrade.Api.Controllers;
using ViaTrade.Api.ModelBinding;
using Xunit;

namespace ViaTrade.Tests;

public sealed class IgnorePropertiesTests(BodyBindingFixture fixture) : IClassFixture<BodyBindingFixture>
{
	[Theory]
	[InlineData("\"id\":999,\"userAgent\":\"spoofed\"")]
	[InlineData("\"ID\":\"invalid\",\"USERAGENT\":{\"value\":1}")]
	[InlineData("\"id\":null,\"userAgent\":[1,2]")]
	[InlineData("\"id\":\"first\",\"Id\":{},\"id\":999,\"userAgent\":false")]
	[InlineData("\"\\u0069d\":\"invalid\",\"userAgent\":999")]
	public async Task ExcludedPropertiesAreRemovedBeforeDeserialization(string excludedProperties)
	{
		using var content = new StringContent(
			$"{{{excludedProperties},\"text\":\"valid\"}}",
			Encoding.UTF8,
			"application/json"
		);
		using var response = await fixture.Client.PostAsync("/bodyBindingTests/ignored/42", content);

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		var command = await response.Content.ReadFromJsonAsync<BodyBindingCommand>();
		Assert.Equal(42, command!.Id);
		Assert.Equal("trusted-agent", command.UserAgent);
		Assert.Equal("valid", command.Text);
	}

	[Fact]
	public async Task MissingExcludedPropertiesDoNotTriggerMvcValidation()
	{
		using var response = await fixture.Client.PostAsJsonAsync(
			"/bodyBindingTests/ignored/42",
			new { text = "valid" }
		);

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
	}

	[Theory]
	[InlineData("text=valid", 1)]
	[InlineData("text=valid&quantity=2&id=invalid&userAgent=spoofed", 2)]
	public async Task QueryExclusionsAlsoSupportOrdinaryRecordsAndAdditionalProperties(string queryString, int quantity)
	{
		using var response = await fixture.Client.GetAsync("/bodyBindingTests/query/42?" + queryString);
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		var command = await response.Content.ReadFromJsonAsync<BodyBindingCommand>();
		Assert.Equal(42, command!.Id);
		Assert.Equal("trusted-agent", command.UserAgent);
		Assert.Equal("valid", command.Text);
		Assert.Equal(quantity, command.Quantity);
	}

	[Fact]
	public async Task NestedPropertiesWithTheSameNameRemainInTheBody()
	{
		using var response = await fixture.Client.PostAsJsonAsync(
			"/bodyBindingTests/ignored/42",
			new { text = "valid", nested = new { id = 17, userAgent = "nested-agent" } }
		);

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		var command = await response.Content.ReadFromJsonAsync<BodyBindingCommand>();
		Assert.Equal(17, command!.Nested!.Id);
		Assert.Equal("nested-agent", command.Nested.UserAgent);
	}

	[Theory]
	[InlineData("{\"text\":123}")]
	[InlineData("{\"text\":null}")]
	[InlineData("{}")]
	[InlineData("{\"text\":\"\"}")]
	[InlineData("{\"text\":\"valid\",\"quantity\":0}")]
	[InlineData("{\"text\":\"valid\",\"nested\":{\"id\":0,\"userAgent\":\"nested\"}}")]
	[InlineData("{")]
	[InlineData("[]")]
	[InlineData("null")]
	[InlineData("")]
	public async Task InvalidBodyAndNonExcludedPropertiesStillReturnValidationProblems(string json)
	{
		using var response = await fixture.Client.PostAsync(
			"/bodyBindingTests/ignored/42",
			new StringContent(json, Encoding.UTF8, "application/json")
		);

		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
		using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
		Assert.Equal("validation_failed", problem.RootElement.GetProperty("code").GetString());
		Assert.NotEmpty(problem.RootElement.GetProperty("errors").EnumerateObject());
	}

	[Fact]
	public async Task NonExcludedConstructorValidationRetainsItsErrorKey()
	{
		using var response = await fixture.Client.PostAsJsonAsync("/bodyBindingTests/ignored/42", new { text = "" });
		using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

		Assert.True(problem.RootElement.GetProperty("errors").TryGetProperty("text", out _));
	}

	[Fact]
	public async Task ExclusionsAreSpecificToTheActionParameter()
	{
		using var ignored = await fixture.Client.PostAsJsonAsync(
			"/bodyBindingTests/ignored/42",
			new { text = "valid" }
		);
		Assert.Equal(HttpStatusCode.OK, ignored.StatusCode);

		using var ordinary = await fixture.Client.PostAsJsonAsync("/bodyBindingTests/ordinary", new { text = "valid" });
		Assert.Equal(HttpStatusCode.BadRequest, ordinary.StatusCode);

		using var onlyId = await fixture.Client.PostAsJsonAsync("/bodyBindingTests/onlyId/42", new { text = "valid" });
		Assert.Equal(HttpStatusCode.BadRequest, onlyId.StatusCode);

		using var validOrdinary = await fixture.Client.PostAsJsonAsync(
			"/bodyBindingTests/ordinary",
			new
			{
				id = 7,
				userAgent = "body-agent",
				text = "valid",
			}
		);
		Assert.Equal(HttpStatusCode.OK, validOrdinary.StatusCode);
		var command = await validOrdinary.Content.ReadFromJsonAsync<BodyBindingCommand>();
		Assert.Equal(7, command!.Id);
		Assert.Equal("body-agent", command.UserAgent);
	}

	[Fact]
	public async Task ExplicitJsonPropertyNamesAreRespected()
	{
		using var response = await fixture.Client.PostAsJsonAsync(
			"/bodyBindingTests/renamed",
			new { external_id = "invalid", text = "valid" }
		);

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		var command = await response.Content.ReadFromJsonAsync<RenamedBodyBindingCommand>();
		Assert.Equal(0, command!.Id);
	}

	[Theory]
	[InlineData("utf-8")]
	[InlineData("utf-16")]
	public async Task SupportedEncodingsAndUnicodeArePreserved(string charset)
	{
		var encoding = Encoding.GetEncoding(charset);
		using var response = await fixture.Client.PostAsync(
			"/bodyBindingTests/ignored/42",
			new StringContent("{\"id\":\"invalid\",\"text\":\"Текст\"}", encoding, "application/json")
		);

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		var command = await response.Content.ReadFromJsonAsync<BodyBindingCommand>();
		Assert.Equal("Текст", command!.Text);
	}

	[Theory]
	[InlineData("text/plain")]
	[InlineData("application/json; charset=iso-8859-1")]
	public async Task UnsupportedMediaTypesAndEncodingsStillReturn415(string contentType)
	{
		using var content = new StringContent("{\"text\":\"valid\"}");
		content.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
		using var response = await fixture.Client.PostAsync("/bodyBindingTests/ignored/42", content);

		Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
	}

	[Fact]
	public void OutputSerializationAndSharedSchemasAreNotChanged()
	{
		var json = JsonSerializer.Serialize(new BodyBindingCommand(7, "agent", "text", null));
		Assert.Contains("\"Id\":7", json);
		Assert.Contains("\"UserAgent\":\"agent\"", json);

		var document = fixture.App.Services.GetRequiredService<ISwaggerProvider>().GetSwagger("web");
		var ignored = GetBodySchema(document, "/bodyBindingTests/ignored/{id}");
		Assert.DoesNotContain("id", ignored.Properties!.Keys);
		Assert.DoesNotContain("userAgent", ignored.Properties.Keys);
		Assert.DoesNotContain("id", ignored.Required!);
		Assert.DoesNotContain("userAgent", ignored.Required!);
		Assert.Contains("text", ignored.Properties.Keys);

		var onlyId = GetBodySchema(document, "/bodyBindingTests/onlyId/{id}");
		Assert.DoesNotContain("id", onlyId.Properties!.Keys);
		Assert.Contains("userAgent", onlyId.Properties.Keys);
		var ordinary = GetBodySchema(document, "/bodyBindingTests/ordinary");
		Assert.Contains("id", ordinary.Properties!.Keys);
		Assert.Contains("userAgent", ordinary.Properties.Keys);
		var trade = GetBodySchema(document, "/api/v1/trades/{tradeId}", HttpMethod.Put);
		Assert.Equal(
			["instrumentId", "tradeTypeId", "openedAt", "closedAt", "openPrice", "closePrice", "signal", "quantity"],
			trade.Properties!.Keys
		);
	}

	[Theory]
	[InlineData(nameof(InvalidParameters.MissingProperty))]
	[InlineData(nameof(InvalidParameters.UnsupportedSource))]
	public void InvalidAttributeConfigurationFailsDuringActionDiscovery(string methodName)
	{
		var parameterInfo = typeof(InvalidParameters).GetMethod(methodName)!.GetParameters().Single();
		var attributes = parameterInfo.GetCustomAttributes().ToArray();
		var parameter = new ParameterModel(parameterInfo, attributes)
		{
			BindingInfo = BindingInfo.GetBindingInfo(attributes),
		};

		Assert.Throws<InvalidOperationException>(() => new RequestPropertiesConvention().Apply(parameter));
	}

	[Fact]
	public void AttributeRejectsEmptyNamesAndCopiesItsConfiguration()
	{
		Assert.Throws<ArgumentException>(() => new IgnorePropertiesAttribute());
		Assert.Throws<ArgumentException>(() => new IgnorePropertiesAttribute(" "));
		var names = new[] { nameof(BodyBindingCommand.Id) };
		var attribute = new IgnorePropertiesAttribute(names);
		names[0] = "Changed";

		Assert.Equal(nameof(BodyBindingCommand.Id), Assert.Single(attribute.PropertyNames));
	}

	[Fact]
	public async Task FormatterRestoresTheOriginalStreamAndHonorsCancellation()
	{
		var formatter = fixture
			.App.Services.GetRequiredService<IOptions<MvcOptions>>()
			.Value.InputFormatters.OfType<IgnorePropertiesJsonInputFormatter>()
			.Single();
		var metadataProvider = (ModelMetadataProvider)fixture.App.Services.GetRequiredService<IModelMetadataProvider>();
		var parameter = typeof(BodyBindingTestController)
			.GetMethod(nameof(BodyBindingTestController.Ignored))!
			.GetParameters()
			.Single(parameter => parameter.ParameterType == typeof(BodyBindingCommand));
		var metadata = metadataProvider.GetMetadataForParameter(parameter);
		var httpContext = new DefaultHttpContext();
		await using var originalBody = new MemoryStream(Encoding.UTF8.GetBytes("{\"text\":\"valid\"}"));
		httpContext.Request.Body = originalBody;
		var context = new InputFormatterContext(
			httpContext,
			string.Empty,
			new ModelStateDictionary(),
			metadata,
			(stream, encoding) => new StreamReader(stream, encoding)
		);

		var result = await formatter.ReadRequestBodyAsync(context, Encoding.UTF8);
		Assert.False(result.HasError);
		Assert.Same(originalBody, httpContext.Request.Body);
		Assert.True(originalBody.CanRead);

		originalBody.Position = 0;
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();
		httpContext.RequestAborted = cancellation.Token;
		await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
			formatter.ReadRequestBodyAsync(context, Encoding.UTF8)
		);
		Assert.Same(originalBody, httpContext.Request.Body);
	}

	private static IOpenApiSchema GetBodySchema(OpenApiDocument document, string path, HttpMethod? method = null)
	{
		method ??= HttpMethod.Post;
		var schema = document.Paths[path].Operations![method].RequestBody!.Content!["application/json"].Schema!;

		if (schema is OpenApiSchemaReference reference)
			return document.Components!.Schemas![reference.Reference.Id!];

		return schema;
	}

	private sealed class InvalidParameters
	{
		public void MissingProperty([FromBody, IgnoreProperties("Missing")] BodyBindingCommand command) { }

		public void UnsupportedSource(
			[FromRoute, IgnoreProperties(nameof(BodyBindingCommand.Id))] BodyBindingCommand command
		) { }
	}
}

public sealed class BodyBindingFixture : IAsyncLifetime
{
	public WebApplication App { get; private set; } = null!;
	public HttpClient Client { get; private set; } = null!;

	public Task InitializeAsync() => InitializeAsync(allowEmptyBody: false);

	public async Task InitializeAsync(bool allowEmptyBody, bool suppressImplicitRequired = false)
	{
		var builder = WebApplication.CreateBuilder();
		builder.WebHost.UseUrls("http://127.0.0.1:0");
		builder.Logging.ClearProviders();
		builder.Services.AddWebPresentation();
		builder.Services.AddSingleton(new MvcBindingService(77, 88));
		builder.Services.Configure<MvcOptions>(options =>
		{
			options.AllowEmptyInputInBodyModelBinding = allowEmptyBody;
			options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = suppressImplicitRequired;
		});
		builder.Services.AddApplicationLayer();
		builder
			.Services.AddControllers()
			.AddApplicationPart(typeof(TradesController).Assembly)
			.AddApplicationPart(typeof(BodyBindingTestController).Assembly);
		App = builder.Build();
		App.MapControllers();
		await App.StartAsync();
		var address = Assert.Single(
			App.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses
		);
		Client = new HttpClient { BaseAddress = new Uri(address) };
		Client.DefaultRequestHeaders.UserAgent.ParseAdd("trusted-agent");
	}

	public async Task DisposeAsync()
	{
		Client.Dispose();
		await App.DisposeAsync();
	}
}

[ApiController]
[Route("bodyBindingTests")]
public sealed class BodyBindingTestController : ControllerBase
{
	[HttpGet("query/{id:int}")]
	public BodyBindingCommand Query(
		[FromRoute] int id,
		[FromQuery, IgnoreProperties(nameof(BodyBindingCommand.Id), nameof(BodyBindingCommand.UserAgent))]
			BodyBindingCommand command
	) => command with { Id = id, UserAgent = Request.Headers.UserAgent.ToString() };

	[HttpPost("ignored/{id:int}")]
	public BodyBindingCommand Ignored(
		[FromRoute] int id,
		[FromBody, IgnoreProperties(nameof(BodyBindingCommand.Id), nameof(BodyBindingCommand.UserAgent))]
			BodyBindingCommand command
	) => command with { Id = id, UserAgent = Request.Headers.UserAgent.ToString() };

	[HttpPost("ordinary")]
	public BodyBindingCommand Ordinary([FromBody] BodyBindingCommand command) => command;

	[HttpPost("onlyId/{id:int}")]
	public BodyBindingCommand OnlyId(
		[FromRoute] int id,
		[FromBody, IgnoreProperties(nameof(BodyBindingCommand.Id))] BodyBindingCommand command
	) => command with { Id = id };

	[HttpPost("renamed")]
	public RenamedBodyBindingCommand Renamed(
		[FromBody, IgnoreProperties(nameof(RenamedBodyBindingCommand.Id))] RenamedBodyBindingCommand command
	) => command;
}

public sealed record BodyBindingCommand(
	[Range(1, int.MaxValue)] int Id,
	[Required] string UserAgent,
	[Required] string Text,
	BodyBindingNested? Nested
)
{
	[Range(1, 10)]
	public int Quantity { get; init; } = 1;
}

public sealed record BodyBindingNested([Range(1, int.MaxValue)] int Id, [Required] string UserAgent);

public sealed record RenamedBodyBindingCommand(
	[property: JsonPropertyName("external_id")] int Id,
	[Required] string Text
);
