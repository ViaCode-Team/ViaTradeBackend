using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.Swagger;
using ViaTrade.Api.Attributes.Binding;
using ViaTrade.Api.ModelBinding;
using Xunit;

namespace ViaTrade.Tests;

public sealed class RequestPropertyBindingTests(BodyBindingFixture fixture) : IClassFixture<BodyBindingFixture>
{
	[Theory]
	[InlineData("body")]
	[InlineData("queryDefault")]
	[InlineData("implicitBody")]
	public async Task SourcesBindTogetherAndSpoofedValuesAreSkippedBeforeConversion(string action)
	{
		using var request = new HttpRequestMessage(
			HttpMethod.Post,
			$"/requestPropertyTests/{action}/42?count=3&itemId=invalid&userAgent=spoofed&text=query-text"
		)
		{
			Content = new StringContent(
				"{\"id\":{},\"userAgent\":[],\"quantity\":\"invalid\",\"text\":\"body-text\"}",
				Encoding.UTF8,
				"application/json"
			),
		};
		request.Headers.Add("X-Agent", "header-agent");
		using var response = await fixture.Client.SendAsync(request);
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		var command = await response.Content.ReadFromJsonAsync<BodyBindingCommand>();
		Assert.Equal(42, command!.Id);
		Assert.Equal("header-agent", command.UserAgent);
		Assert.Equal("body-text", command.Text);
		Assert.Equal(3, command.Quantity);
	}

	[Fact]
	public async Task IgnorePropertiesWithoutADefaultSourceUsesTheJsonBody()
	{
		using var response = await fixture.Client.PostAsJsonAsync(
			"/requestPropertyTests/implicitBodyIgnored?text=query-text&quantity=invalid&userAgent=spoofed",
			new
			{
				id = new { spoofed = true },
				userAgent = new[] { "spoofed" },
				text = "body-text",
				quantity = 3,
			}
		);
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		var command = await response.Content.ReadFromJsonAsync<BodyBindingCommand>();
		Assert.Equal(7, command!.Id);
		Assert.Equal("trusted", command.UserAgent);
		Assert.Equal("body-text", command.Text);
		Assert.Equal(3, command.Quantity);

		var document = fixture.App.Services.GetRequiredService<ISwaggerProvider>().GetSwagger("web");
		var operation = document.Paths["/requestPropertyTests/implicitBodyIgnored"].Operations![HttpMethod.Post];
		Assert.True(operation.RequestBody!.Required);
		var schema = operation.RequestBody.Content!["application/json"].Schema!;
		Assert.Contains("text", schema.Properties!.Keys);
		Assert.Contains("quantity", schema.Properties.Keys);
		Assert.DoesNotContain("id", schema.Properties.Keys);
		Assert.DoesNotContain("userAgent", schema.Properties.Keys);
		Assert.Empty(operation.Parameters ?? []);
	}

	[Theory]
	[InlineData("POST")]
	[InlineData("GET")]
	public async Task InferredBodiesRemainRequiredRegardlessOfTheHttpMethod(string method)
	{
		foreach (var json in new[] { "", "null", "{", "{}" })
		{
			using var request = new HttpRequestMessage(
				new HttpMethod(method),
				"/requestPropertyTests/implicitBodyIgnored?text=query-text&quantity=3"
			)
			{
				Content = new StringContent(json, Encoding.UTF8, "application/json"),
			};
			using var response = await fixture.Client.SendAsync(request);
			Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
		}
	}

	[Fact]
	public async Task NullableInferredBodyRetainsTheMvcEmptyBodyPolicy()
	{
		foreach (var json in new string?[] { null, "", "null" })
		{
			using var request = new HttpRequestMessage(HttpMethod.Post, "/requestPropertyTests/implicitNullableBody");
			if (json is not null)
				request.Content = new StringContent(json, Encoding.UTF8, "application/json");
			using var response = await fixture.Client.SendAsync(request);
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			using var result = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
			Assert.True(result.RootElement.GetProperty("isNull").GetBoolean());
		}

		var document = fixture.App.Services.GetRequiredService<ISwaggerProvider>().GetSwagger("web");
		Assert.False(
			document
				.Paths["/requestPropertyTests/implicitNullableBody"]
				.Operations![HttpMethod.Post]
				.RequestBody!
				.Required
		);
	}

	[Fact]
	public async Task RegisteredComplexParametersRetainMvcServiceInference()
	{
		using var response = await fixture.Client.GetAsync(
			"/requestPropertyTests/implicitService?value=invalid&ignoredValue=spoofed"
		);
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		Assert.Equal(new MvcBindingService(77, 88), await response.Content.ReadFromJsonAsync<MvcBindingService>());

		var document = fixture.App.Services.GetRequiredService<ISwaggerProvider>().GetSwagger("web");
		var operation = document.Paths["/requestPropertyTests/implicitService"].Operations![HttpMethod.Get];
		Assert.Null(operation.RequestBody);
		Assert.Empty(operation.Parameters ?? []);
	}

	[Fact]
	public async Task BodyWithoutRequiredRejectsMissingInputAndSwaggerMarksItRequired()
	{
		foreach (var json in new[] { "", "null" })
		{
			using var request = new HttpRequestMessage(HttpMethod.Post, "/requestPropertyTests/withoutRequired/42")
			{
				Content = new StringContent(json, Encoding.UTF8, "application/json"),
			};
			request.Headers.Add("X-Agent", "agent");
			using var response = await fixture.Client.SendAsync(request);
			Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
		}
		var document = fixture.App.Services.GetRequiredService<ISwaggerProvider>().GetSwagger("web");
		Assert.True(
			document
				.Paths["/requestPropertyTests/withoutRequired/{itemId}"]
				.Operations![HttpMethod.Post]
				.RequestBody!
				.Required
		);
		Assert.True(
			document.Paths["/requestPropertyTests/body/{itemId}"].Operations![HttpMethod.Post].RequestBody!.Required
		);
	}

	[Theory]
	[InlineData("query")]
	[InlineData("implicitQuery")]
	public async Task QueryAndHeaderSourcesBindWithoutABodyAndPreserveDefaults(string action)
	{
		using var request = new HttpRequestMessage(
			HttpMethod.Get,
			$"/requestPropertyTests/{action}/42?term=query-text&id=invalid&userAgent=spoofed"
		);
		request.Headers.Add("X-Agent", "header-agent");
		using var response = await fixture.Client.SendAsync(request);
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		var command = await response.Content.ReadFromJsonAsync<BodyBindingCommand>();
		Assert.Equal(42, command!.Id);
		Assert.Equal("header-agent", command.UserAgent);
		Assert.Equal("query-text", command.Text);
		Assert.Equal(1, command.Quantity);
	}

	[Theory]
	[InlineData("implicitRouteOnly")]
	[InlineData("bodyRouteOnly")]
	[InlineData("bodyRouteOnlyClass")]
	public async Task RouteOnlyRequestsDoNotRequireOrReadABody(string action)
	{
		foreach (var json in new string?[] { null, "null", "{", "{\"id\":{},\"serverValue\":[]}" })
		{
			using var request = new HttpRequestMessage(
				HttpMethod.Post,
				$"/requestPropertyTests/{action}/42?id=invalid&serverValue=invalid"
			);
			if (json is not null)
				request.Content = new StringContent(json, Encoding.UTF8, "application/json");
			using var response = await fixture.Client.SendAsync(request);
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			var model = await response.Content.ReadFromJsonAsync<RouteOnlyBindingRecord>();
			Assert.Equal(42, model!.Id);
			Assert.Equal("trusted", model.ServerValue);
		}

		var document = fixture.App.Services.GetRequiredService<ISwaggerProvider>().GetSwagger("web");
		var operation = document.Paths[$"/requestPropertyTests/{action}/{{id}}"].Operations![HttpMethod.Post];
		Assert.Null(operation.RequestBody);
		var route = Assert.Single(operation.Parameters!);
		Assert.Equal("id", route.Name);
		Assert.Equal(ParameterLocation.Path, route.In);
		Assert.True(route.Required);
	}

	[Theory]
	[InlineData("implicitRouteOnly")]
	[InlineData("bodyRouteOnly")]
	[InlineData("bodyRouteOnlyClass")]
	public async Task RouteOnlyRequestsStillValidateRouteValues(string action)
	{
		foreach (var id in new[] { "invalid", "0" })
		{
			using var response = await fixture.Client.PostAsync($"/requestPropertyTests/{action}/{id}", null);
			Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
		}
	}

	[Fact]
	public async Task BodylessFromBodyParameterCanShareAnActionWithARealBody()
	{
		using var response = await fixture.Client.PostAsJsonAsync(
			"/requestPropertyTests/routeAndBody/42",
			new
			{
				id = 17,
				userAgent = "body-agent",
				text = "valid",
			}
		);
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		var model = await response.Content.ReadFromJsonAsync<BodyBindingCommand>();
		Assert.Equal(42, model!.Id);
		Assert.Equal("body-agent", model.UserAgent);
		Assert.Equal("valid", model.Text);

		var document = fixture.App.Services.GetRequiredService<ISwaggerProvider>().GetSwagger("web");
		var operation = document.Paths["/requestPropertyTests/routeAndBody/{id}"].Operations![HttpMethod.Post];
		Assert.True(operation.RequestBody!.Required);
		var schema = operation.RequestBody.Content!["application/json"].Schema!;
		if (schema is OpenApiSchemaReference reference)
			schema = document.Components!.Schemas![reference.Reference.Id!];
		Assert.Contains("id", schema.Properties!.Keys);
		Assert.Contains("userAgent", schema.Properties.Keys);
	}

	[Fact]
	public async Task FormSourcesRemainSeparateFromQueryAndRoute()
	{
		using var request = new HttpRequestMessage(HttpMethod.Post, "/requestPropertyTests/form/42?count=3")
		{
			Content = new FormUrlEncodedContent(
				new Dictionary<string, string>
				{
					["text"] = "form-text",
					["id"] = "invalid",
					["quantity"] = "invalid",
					["userAgent"] = "spoofed",
				}
			),
		};
		request.Headers.Add("X-Agent", "header-agent");
		using var response = await fixture.Client.SendAsync(request);
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		var command = await response.Content.ReadFromJsonAsync<BodyBindingCommand>();
		Assert.Equal(42, command!.Id);
		Assert.Equal("header-agent", command.UserAgent);
		Assert.Equal("form-text", command.Text);
		Assert.Equal(3, command.Quantity);
	}

	[Theory]
	[InlineData("invalid", "3", "agent", "valid")]
	[InlineData("0", "3", "agent", "valid")]
	[InlineData("42", "invalid", "agent", "valid")]
	[InlineData("42", "11", "agent", "valid")]
	[InlineData("42", "3", null, "valid")]
	[InlineData("42", "3", "agent", "")]
	public async Task BindingAndValidationErrorsFromEverySourceReturn400(
		string id,
		string count,
		string? agent,
		string text
	)
	{
		using var request = new HttpRequestMessage(HttpMethod.Post, $"/requestPropertyTests/body/{id}?count={count}")
		{
			Content = JsonContent.Create(
				new
				{
					text,
					id = 99,
					userAgent = "body-spoof",
				}
			),
		};
		if (agent is not null)
			request.Headers.Add("X-Agent", agent);
		using var response = await fixture.Client.SendAsync(request);
		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
	}

	[Fact]
	public async Task ValidationUsesTheExternalQueryName()
	{
		using var request = new HttpRequestMessage(HttpMethod.Post, "/requestPropertyTests/body/42?count=11")
		{
			Content = JsonContent.Create(new { text = "valid" }),
		};
		request.Headers.Add("X-Agent", "agent");
		using var response = await fixture.Client.SendAsync(request);
		using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
		Assert.True(problem.RootElement.GetProperty("errors").TryGetProperty("count", out _));
	}

	[Fact]
	public async Task SourcesOnAQueryParameterDoNotFilterAnUnrelatedBodyParameter()
	{
		using var response = await fixture.Client.PostAsJsonAsync(
			"/requestPropertyTests/separateParameters?term=query-text&quantity=4",
			new
			{
				id = 42,
				text = "body-text",
				userAgent = "body-agent",
			}
		);
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		var command = await response.Content.ReadFromJsonAsync<BodyBindingCommand>();
		Assert.Equal(42, command!.Id);
		Assert.Equal("body-text", command.Text);
		Assert.Equal("body-agent", command.UserAgent);
		Assert.Equal(4, command.Quantity);
		var document = fixture.App.Services.GetRequiredService<ISwaggerProvider>().GetSwagger("web");
		var operation = document.Paths["/requestPropertyTests/separateParameters"].Operations![HttpMethod.Post];
		var schema = operation.RequestBody!.Content!["application/json"].Schema!;
		if (schema is OpenApiSchemaReference reference)
			schema = document.Components!.Schemas![reference.Reference.Id!];
		Assert.Contains("id", schema.Properties!.Keys);
		Assert.Contains("userAgent", schema.Properties.Keys);
		Assert.Single(operation.Parameters!, parameter => parameter.Name == "term");
	}

	[Fact]
	public async Task JsonPropertyNamesAreUsedForBodyExclusions()
	{
		using var response = await fixture.Client.PostAsJsonAsync(
			"/requestPropertyTests/renamed/42",
			new { external_id = new { spoof = true }, text = "valid" }
		);
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		var command = await response.Content.ReadFromJsonAsync<RenamedBodyBindingCommand>();
		Assert.Equal(42, command!.Id);
		var document = fixture.App.Services.GetRequiredService<ISwaggerProvider>().GetSwagger("web");
		var operation = document.Paths["/requestPropertyTests/renamed/{ITEM_ID}"].Operations![HttpMethod.Post];
		Assert.Single(
			operation.Parameters!,
			parameter => parameter.Name == "ITEM_ID" && parameter.In == ParameterLocation.Path
		);
	}

	[Fact]
	public void SwaggerProjectsEachSourceWithoutMutatingSharedSchemas()
	{
		var document = fixture.App.Services.GetRequiredService<ISwaggerProvider>().GetSwagger("web");
		foreach (var action in new[] { "body", "queryDefault", "implicitBody" })
		{
			var operation = document.Paths[$"/requestPropertyTests/{action}/{{itemId}}"].Operations![HttpMethod.Post];
			var route = Assert.Single(operation.Parameters!, parameter => parameter.Name == "itemId");
			Assert.Equal(ParameterLocation.Path, route.In);
			Assert.True(route.Required);
			Assert.Equal(JsonSchemaType.Integer, route.Schema!.Type);
			var query = Assert.Single(operation.Parameters!, parameter => parameter.Name == "count");
			Assert.Equal(ParameterLocation.Query, query.In);
			Assert.Equal("10", query.Schema!.Maximum);
			var header = Assert.Single(operation.Parameters!, parameter => parameter.Name == "X-Agent");
			Assert.Equal(ParameterLocation.Header, header.In);
			Assert.True(header.Required);
			var body = operation.RequestBody!.Content!["application/json"].Schema!;
			Assert.True(operation.RequestBody.Required);
			Assert.Contains("text", body.Properties!.Keys);
			Assert.DoesNotContain("id", body.Properties.Keys);
			Assert.DoesNotContain("quantity", body.Properties.Keys);
			Assert.DoesNotContain("userAgent", body.Properties.Keys);
		}
		var shared = document.Components!.Schemas![nameof(BodyBindingCommand)];
		Assert.Contains("id", shared.Properties!.Keys);
		Assert.Contains("userAgent", shared.Properties.Keys);
		var form = document.Paths["/requestPropertyTests/form/{itemId}"].Operations![HttpMethod.Post];
		Assert.Contains(
			"text",
			form.RequestBody!.Content!["application/x-www-form-urlencoded"].Schema!.Properties!.Keys
		);
	}

	[Theory]
	[InlineData(nameof(InvalidConfigurations.Missing))]
	[InlineData(nameof(InvalidConfigurations.Duplicate))]
	[InlineData(nameof(InvalidConfigurations.Ignored))]
	[InlineData(nameof(InvalidConfigurations.AliasForMany))]
	[InlineData(nameof(InvalidConfigurations.BodyAndForm))]
	[InlineData(nameof(InvalidConfigurations.Unsupported))]
	[InlineData(nameof(InvalidConfigurations.DuplicateAlias))]
	[InlineData(nameof(InvalidConfigurations.ReadOnly))]
	public void InvalidConfigurationFailsDuringActionDiscovery(string method)
	{
		var info = typeof(InvalidConfigurations).GetMethod(method)!.GetParameters().Single();
		var attributes = info.GetCustomAttributes().ToArray();
		var parameter = new ParameterModel(info, attributes) { BindingInfo = BindingInfo.GetBindingInfo(attributes) };
		Assert.Throws<InvalidOperationException>(() => new RequestPropertiesConvention().Apply(parameter));
	}

	[Fact]
	public void RoutePropertiesMustMatchTheRouteTemplate()
	{
		var method = typeof(InvalidConfigurations).GetMethod(nameof(InvalidConfigurations.RouteMismatch))!;
		var info = method.GetParameters().Single();
		var attributes = info.GetCustomAttributes().ToArray();
		var controller = new ControllerModel(typeof(InvalidConfigurations).GetTypeInfo(), []);
		controller.Selectors.Add(
			new SelectorModel { AttributeRouteModel = new AttributeRouteModel(new RouteAttribute("test")) }
		);
		var action = new ActionModel(method, []) { Controller = controller };
		action.Selectors.Add(
			new SelectorModel { AttributeRouteModel = new AttributeRouteModel(new HttpGetAttribute("{otherId}")) }
		);
		var parameter = new ParameterModel(info, attributes)
		{
			BindingInfo = BindingInfo.GetBindingInfo(attributes),
			Action = action,
		};
		Assert.Throws<InvalidOperationException>(() => new RequestPropertiesConvention().Apply(parameter));
	}

	[Fact]
	public void AttributeValidatesAndCopiesItsConfiguration()
	{
		Assert.Throws<ArgumentException>(() => new FromQueryPropertiesAttribute());
		Assert.Throws<ArgumentException>(() => new UnsupportedSourceAttribute());
		var names = new[] { nameof(BodyBindingCommand.Text) };
		var attribute = new FromQueryPropertiesAttribute(names);
		names[0] = "Changed";
		Assert.Equal(nameof(BodyBindingCommand.Text), Assert.Single(attribute.PropertyNames));
	}

	[Fact]
	public void PropertyAttributesUseTheStandardMvcSourcesWithoutChangingTheRootSource()
	{
		FromRequestPropertiesAttribute[] attributes =
		[
			new FromBodyPropertiesAttribute("Id"),
			new FromRoutePropertiesAttribute("Id"),
			new FromQueryPropertiesAttribute("Id"),
			new FromHeaderPropertiesAttribute("Id"),
			new FromFormPropertiesAttribute("Id"),
			new FromFormFilePropertiesAttribute("Id"),
		];
		Assert.Equal(
			[
				BindingSource.Body,
				BindingSource.Path,
				BindingSource.Query,
				BindingSource.Header,
				BindingSource.Form,
				BindingSource.FormFile,
			],
			attributes.Select(attribute => attribute.Source)
		);
		Assert.All(attributes, attribute => Assert.False(attribute is IBindingSourceMetadata));
	}

	private sealed class UnsupportedSourceAttribute() : FromRequestPropertiesAttribute(BindingSource.Services, "Id");

	private sealed class InvalidConfigurations
	{
		public void DuplicateAlias(
			[
				FromQuery,
				FromQueryProperties(nameof(BodyBindingCommand.Text), Name = "same"),
				FromQueryProperties(nameof(BodyBindingCommand.UserAgent), Name = "SAME")
			]
				BodyBindingCommand command
		) { }

		public void ReadOnly([FromQuery, FromQueryProperties(nameof(ReadOnlyCommand.Text))] ReadOnlyCommand command) { }

		public void RouteMismatch(
			[FromQuery, FromRouteProperties(nameof(BodyBindingCommand.Id))] BodyBindingCommand command
		) { }

		public void Missing([FromQuery, FromRouteProperties("Missing")] BodyBindingCommand command) { }

		public void Duplicate(
			[
				FromQuery,
				FromRouteProperties(nameof(BodyBindingCommand.Id)),
				FromQueryProperties(nameof(BodyBindingCommand.Id))
			]
				BodyBindingCommand command
		) { }

		public void Ignored(
			[
				FromQuery,
				IgnoreProperties(nameof(BodyBindingCommand.Id)),
				FromRouteProperties(nameof(BodyBindingCommand.Id))
			]
				BodyBindingCommand command
		) { }

		public void AliasForMany(
			[
				FromQuery,
				FromQueryProperties(nameof(BodyBindingCommand.Id), nameof(BodyBindingCommand.Text), Name = "alias")
			]
				BodyBindingCommand command
		) { }

		public void BodyAndForm(
			[FromBody, FromFormProperties(nameof(BodyBindingCommand.Text))] BodyBindingCommand command
		) { }

		public void Unsupported(
			[FromRoute, FromQueryProperties(nameof(BodyBindingCommand.Text))] BodyBindingCommand command
		) { }
	}

	private sealed class ReadOnlyCommand
	{
		public string Text => "value";
	}
}

[ApiController]
[Route("requestPropertyTests")]
public sealed class RequestPropertyTestController : ControllerBase
{
	[HttpPost("implicitRouteOnly/{id}")]
	public RouteOnlyBindingRecord ImplicitRouteOnly(
		[
			IgnoreProperties(nameof(RouteOnlyBindingRecord.ServerValue)),
			FromRouteProperties(nameof(RouteOnlyBindingRecord.Id))
		]
			RouteOnlyBindingRecord model
	) => model with { ServerValue = "trusted" };

	[HttpPost("bodyRouteOnly/{id}")]
	public RouteOnlyBindingRecord BodyRouteOnly(
		[
			FromBody,
			IgnoreProperties(nameof(RouteOnlyBindingRecord.ServerValue)),
			FromRouteProperties(nameof(RouteOnlyBindingRecord.Id))
		]
			RouteOnlyBindingRecord model
	) => model with { ServerValue = "trusted" };

	[HttpPost("bodyRouteOnlyClass/{id}")]
	public RouteOnlyBindingClass BodyRouteOnlyClass(
		[
			FromBody,
			IgnoreProperties(nameof(RouteOnlyBindingClass.ServerValue)),
			FromRouteProperties(nameof(RouteOnlyBindingClass.Id))
		]
			RouteOnlyBindingClass model
	)
	{
		model.ServerValue = "trusted";
		return model;
	}

	[HttpPost("routeAndBody/{id}")]
	public BodyBindingCommand RouteAndBody(
		[
			FromBody,
			IgnoreProperties(nameof(RouteOnlyBindingRecord.ServerValue)),
			FromRouteProperties(nameof(RouteOnlyBindingRecord.Id))
		]
			RouteOnlyBindingRecord route,
		[FromBody] BodyBindingCommand body
	) => body with { Id = route.Id };

	[HttpGet("implicitQuery/{itemId}")]
	public BodyBindingCommand ImplicitQuery(
		[
			FromRouteProperties(nameof(BodyBindingCommand.Id), Name = "itemId"),
			FromHeaderProperties(nameof(BodyBindingCommand.UserAgent), Name = "X-Agent"),
			FromQueryProperties(nameof(BodyBindingCommand.Text), Name = "term"),
			FromQueryProperties(nameof(BodyBindingCommand.Quantity), nameof(BodyBindingCommand.Nested))
		]
			BodyBindingCommand command
	) => command;

	[HttpPost("implicitBody/{itemId}")]
	public BodyBindingCommand ImplicitBody(
		[
			FromRouteProperties(nameof(BodyBindingCommand.Id), Name = "itemId"),
			FromHeaderProperties(nameof(BodyBindingCommand.UserAgent), Name = "X-Agent"),
			FromQueryProperties(nameof(BodyBindingCommand.Quantity), Name = "count")
		]
			BodyBindingCommand command
	) => command;

	[HttpPost("implicitBodyIgnored")]
	[HttpGet("implicitBodyIgnored")]
	public BodyBindingCommand ImplicitBodyIgnored(
		[IgnoreProperties(nameof(BodyBindingCommand.Id), nameof(BodyBindingCommand.UserAgent))]
			BodyBindingCommand command
	) => command with { Id = 7, UserAgent = "trusted" };

	[HttpPost("implicitNullableBody")]
	public IActionResult ImplicitNullableBody(
		[IgnoreProperties(nameof(BodyBindingCommand.Id), nameof(BodyBindingCommand.UserAgent))]
			BodyBindingCommand? command
	) => Ok(new { isNull = command is null });

	[HttpGet("implicitService")]
	public MvcBindingService ImplicitService(
		[IgnoreProperties(nameof(MvcBindingService.IgnoredValue))] MvcBindingService service
	) => service;

	[HttpPost("withoutRequired/{itemId}")]
	public BodyBindingCommand WithoutRequired(
		[
			FromBody,
			FromRouteProperties(nameof(BodyBindingCommand.Id), Name = "itemId"),
			FromHeaderProperties(nameof(BodyBindingCommand.UserAgent), Name = "X-Agent"),
			FromQueryProperties(nameof(BodyBindingCommand.Quantity), Name = "count")
		]
			BodyBindingCommand command
	) => command;

	[HttpPost("separateParameters")]
	public BodyBindingCommand SeparateParameters(
		[FromBody] BodyBindingCommand body,
		[
			FromQuery,
			IgnoreProperties(
				nameof(BodyBindingCommand.Id),
				nameof(BodyBindingCommand.UserAgent),
				nameof(BodyBindingCommand.Nested)
			),
			FromQueryProperties(nameof(BodyBindingCommand.Text), Name = "term")
		]
			BodyBindingCommand query
	) => body with { Quantity = query.Quantity };

	[HttpPost("body/{itemId}")]
	public BodyBindingCommand Body(
		[
			FromBody,
			FromRouteProperties(nameof(BodyBindingCommand.Id), Name = "itemId"),
			FromHeaderProperties(nameof(BodyBindingCommand.UserAgent), Name = "X-Agent"),
			FromQueryProperties(nameof(BodyBindingCommand.Quantity), Name = "count")
		]
			BodyBindingCommand command
	) => command;

	[HttpPost("queryDefault/{itemId}")]
	public BodyBindingCommand QueryDefault(
		[
			FromQuery,
			FromBodyProperties(nameof(BodyBindingCommand.Text)),
			FromRouteProperties(nameof(BodyBindingCommand.Id), Name = "itemId"),
			FromHeaderProperties(nameof(BodyBindingCommand.UserAgent), Name = "X-Agent"),
			FromQueryProperties(nameof(BodyBindingCommand.Quantity), Name = "count")
		]
			BodyBindingCommand command
	) => command;

	[HttpGet("query/{itemId}")]
	public BodyBindingCommand Query(
		[
			FromQuery,
			FromRouteProperties(nameof(BodyBindingCommand.Id), Name = "itemId"),
			FromHeaderProperties(nameof(BodyBindingCommand.UserAgent), Name = "X-Agent"),
			FromQueryProperties(nameof(BodyBindingCommand.Text), Name = "term")
		]
			BodyBindingCommand command
	) => command;

	[HttpPost("form/{itemId}")]
	public BodyBindingCommand Form(
		[
			FromForm,
			FromFormProperties(nameof(BodyBindingCommand.Text)),
			FromRouteProperties(nameof(BodyBindingCommand.Id), Name = "itemId"),
			FromHeaderProperties(nameof(BodyBindingCommand.UserAgent), Name = "X-Agent"),
			FromQueryProperties(nameof(BodyBindingCommand.Quantity), Name = "count")
		]
			BodyBindingCommand command
	) => command;

	[HttpPost("renamed/{ITEM_ID}")]
	public RenamedBodyBindingCommand Renamed(
		[FromBody, FromRouteProperties(nameof(RenamedBodyBindingCommand.Id), Name = "ITEM_ID")]
			RenamedBodyBindingCommand command
	) => command;
}

public sealed record RouteOnlyBindingRecord([Range(1, int.MaxValue)] int Id, [Required] string ServerValue);

public sealed record MvcBindingService(int Value, int IgnoredValue);

public sealed class RouteOnlyBindingClass
{
	[Range(1, int.MaxValue)]
	public int Id { get; set; }

	[Required]
	public string ServerValue { get; set; } = null!;
}
