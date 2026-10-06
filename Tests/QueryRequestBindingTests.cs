using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.Swagger;
using ViaTrade.Api.Attributes.Binding;
using ViaTrade.Application.Auth.GetSessionsPage;
using ViaTrade.Application.Instruments.GetStatistics;
using ViaTrade.Application.Strategies.GetInstrumentsPage;
using ViaTrade.Application.Trades.Delete;
using ViaTrade.Application.Trades.GetPage;
using ViaTrade.Application.Trades.GetProfitChart;
using Xunit;

namespace ViaTrade.Tests;

public sealed class QueryRequestBindingTests(BodyBindingFixture fixture) : IClassFixture<BodyBindingFixture>
{
	[Theory]
	[InlineData("record")]
	[InlineData("class")]
	public async Task OrdinaryModelsBindFlatQueryValuesAndKeepDefaults(string modelType)
	{
		using var empty = await fixture.Client.GetAsync("/queryBindingTests/ordinary/" + modelType);
		Assert.Equal(HttpStatusCode.OK, empty.StatusCode);
		var defaults = await empty.Content.ReadFromJsonAsync<OrdinaryBindingModel>();
		Assert.Equal(20, defaults!.Filter.PageSize);
		Assert.Empty(defaults.Filter.Tags);

		using var response = await fixture.Client.GetAsync(
			"/queryBindingTests/ordinary/" + modelType + "?pageSize=3&tags=first&tags=second&note=value"
		);
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		var model = await response.Content.ReadFromJsonAsync<OrdinaryBindingModel>();
		Assert.Equal(3, model!.Filter.PageSize);
		Assert.Equal(["first", "second"], model.Filter.Tags);
		Assert.Equal("value", model.Note);
	}

	[Fact]
	public async Task OrdinaryModelOverridesBindRouteHeaderAndFlatQueryProperties()
	{
		using var request = new HttpRequestMessage(
			HttpMethod.Get,
			"/queryBindingTests/ordinary/mixed/42?pageSize=3&id=invalid&serverValue=invalid"
		);
		request.Headers.Add("X-Note", "header-value");
		using var response = await fixture.Client.SendAsync(request);
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		var model = await response.Content.ReadFromJsonAsync<OrdinaryBindingModel>();
		Assert.Equal(42, model!.Id);
		Assert.Equal(7, model.ServerValue);
		Assert.Equal(3, model.Filter.PageSize);
		Assert.Equal("header-value", model.Note);

		using var ordinary = await fixture.Client.GetAsync("/queryBindingTests/ordinary/record?note=query-value");
		Assert.Equal(HttpStatusCode.OK, ordinary.StatusCode);
		var ordinaryModel = await ordinary.Content.ReadFromJsonAsync<OrdinaryBindingModel>();
		Assert.Equal("query-value", ordinaryModel!.Note);
	}

	[Theory]
	[InlineData("record", "0")]
	[InlineData("class", "0")]
	[InlineData("record", "invalid")]
	[InlineData("class", "invalid")]
	public async Task OrdinaryModelErrorsUseFlatQueryKeys(string modelType, string value)
	{
		using var response = await fixture.Client.GetAsync(
			"/queryBindingTests/ordinary/" + modelType + "?pageSize=" + value
		);
		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
		using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
		Assert.True(problem.RootElement.GetProperty("errors").TryGetProperty("pageSize", out _));
	}

	[Theory]
	[InlineData("scalar?value=hello", "hello")]
	[InlineData("collection?values=2&values=3", "[2,3]")]
	public async Task StandardScalarAndCollectionQueryBindingIsPreserved(string path, string expectedJson)
	{
		using var response = await fixture.Client.GetAsync("/queryBindingTests/ordinary/" + path);
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		Assert.Equal(expectedJson, await response.Content.ReadAsStringAsync());
	}

	[Fact]
	public void SwaggerProjectsOrdinaryModelsWithFlatFieldsAndNullableProperties()
	{
		var document = fixture.App.Services.GetRequiredService<ISwaggerProvider>().GetSwagger("web");
		foreach (var modelType in new[] { "record", "class" })
		{
			var operation = document.Paths["/queryBindingTests/ordinary/" + modelType].Operations![HttpMethod.Get];
			Assert.DoesNotContain(operation.Parameters!, parameter => parameter.Name!.Contains('.'));
			var size = Assert.Single(operation.Parameters!, parameter => parameter.Name == "pageSize");
			Assert.Equal("1", size.Schema!.Minimum);
			Assert.Equal("100", size.Schema.Maximum);
			var note = Assert.Single(operation.Parameters!, parameter => parameter.Name == "note");
			Assert.False(note.Required);
			Assert.True(note.Schema!.Type!.Value.HasFlag(JsonSchemaType.Null));
		}
	}

	[Theory]
	[InlineData("")]
	[InlineData("?userId=invalid&USERID={}&query.userId=999")]
	public async Task AbsentFiltersKeepDefaultsAndIgnoredValuesAreNeverConverted(string queryString)
	{
		using var response = await fixture.Client.GetAsync("/queryBindingTests/trades" + queryString);
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		var query = await response.Content.ReadFromJsonAsync<GetTradesPageQuery>();
		Assert.Equal(7, query!.UserId);
		Assert.Equal(1, query.PageOptions.Page);
		Assert.Equal(20, query.PageOptions.PageSize);
		Assert.NotNull(query.TradeSearch);
		Assert.NotNull(query.TradeFilter);
	}

	[Fact]
	public async Task FlatArrayParametersRouteAndNestedRecordsBindTogether()
	{
		using var response = await fixture.Client.GetAsync(
			"/queryBindingTests/strategies/42?instrumentIds=3&instrumentIds=5&sortBy=symbolAsc&pageSize=2&strategyId=invalid&userId=invalid"
		);
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		var query = await response.Content.ReadFromJsonAsync<GetStrategyInstrumentsPageQuery>();
		Assert.Equal(7, query!.UserId);
		Assert.Equal(42, query.StrategyId);
		Assert.Equal([3, 5], query.InstrumentFilter.InstrumentIds);
		Assert.Equal(2, query.PageOptions.PageSize);
		Assert.Single(query.InstrumentSort.SortBy);
	}

	[Fact]
	public async Task SessionClaimsOverrideSpoofedQueryValues()
	{
		using var response = await fixture.Client.GetAsync(
			"/queryBindingTests/sessions?userId=invalid&currentSessionId=spoofed&pageSize=2"
		);
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		var query = await response.Content.ReadFromJsonAsync<GetSessionsPageQuery>();
		Assert.Equal(7, query!.UserId);
		Assert.Equal("trusted-session", query.CurrentSessionId);
		Assert.Equal(2, query.PageOptions.PageSize);
	}

	[Fact]
	public async Task CommandsWithoutBodiesAndEmptyQueriesBind()
	{
		using var deleted = await fixture.Client.DeleteAsync(
			"/queryBindingTests/trades/42?tradeId=invalid&userId=invalid"
		);
		Assert.Equal(HttpStatusCode.OK, deleted.StatusCode);
		var command = await deleted.Content.ReadFromJsonAsync<DeleteTradeCommand>();
		Assert.Equal(42, command!.TradeId);
		Assert.Equal(7, command.UserId);
		using var empty = await fixture.Client.GetAsync("/queryBindingTests/statistics");
		Assert.Equal(HttpStatusCode.OK, empty.StatusCode);
	}

	[Fact]
	public async Task ClassFiltersKeepTheirInitializerDefaults()
	{
		using var response = await fixture.Client.GetAsync("/queryBindingTests/profitChart");
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		var query = await response.Content.ReadFromJsonAsync<GetProfitChartQuery>();
		Assert.Equal(ProfitChartGranularity.Day, query!.ProfitChartFilter.Granularity);
		Assert.Null(query.ProfitChartFilter.StartDate);
	}

	[Theory]
	[InlineData("pageSize=invalid")]
	[InlineData("signal=invalid")]
	[InlineData("startDate=invalid")]
	public async Task InvalidClientValuesStillFailBinding(string queryString)
	{
		using var response = await fixture.Client.GetAsync("/queryBindingTests/trades?" + queryString);
		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
	}

	[Fact]
	public void SwaggerProjectsValidationToFlatQueryRouteAndBodySchemas()
	{
		var document = fixture.App.Services.GetRequiredService<ISwaggerProvider>().GetSwagger("web");
		var trades = document.Paths["/api/v1/trades"].Operations![HttpMethod.Get];
		Assert.DoesNotContain(trades.Parameters!, parameter => parameter.Name!.Contains('.'));
		Assert.DoesNotContain(trades.Parameters!, parameter => parameter.Name == "userId");
		var size = Assert.Single(trades.Parameters!, parameter => parameter.Name == "pageSize");
		Assert.False(size.Required);
		Assert.Equal("1", size.Schema!.Minimum);
		Assert.Equal("100", size.Schema.Maximum);

		var instruments = document.Paths["/api/v1/strategies/{strategyId}/instruments"].Operations![HttpMethod.Get];
		var ids = Assert.Single(instruments.Parameters!, parameter => parameter.Name == "instrumentIds");
		Assert.Equal(100, ids.Schema!.MaxItems);
		Assert.Equal("0", ids.Schema.Items!.ExclusiveMinimum);
		Assert.True(ids.Explode);
		var route = Assert.Single(instruments.Parameters!, parameter => parameter.Name == "strategyId");
		Assert.Equal(ParameterLocation.Path, route.In);
		Assert.True(route.Required);
		Assert.Equal("0", route.Schema!.ExclusiveMinimum);

		var history = document.Paths["/api/v1/signals"].Operations![HttpMethod.Get];
		Assert.True(Assert.Single(history.Parameters!, parameter => parameter.Name == "strategyId").Required);
		Assert.True(Assert.Single(history.Parameters!, parameter => parameter.Name == "instrumentId").Required);

		var credentials = document.Components!.Schemas!["LoginCommand"];
		Assert.Equal(8, credentials.Properties!["password"].MinLength);
		Assert.Equal(72, credentials.Properties["password"].MaxLength);
		var trade = document.Components.Schemas["UpdateTradeCommand"];
		Assert.Equal("5E-324", trade.Properties!["openPrice"].Minimum);
		Assert.Equal("1.7976931348623157E+308", trade.Properties["openPrice"].Maximum);
		Assert.True(trade.Properties["closePrice"].Type!.Value.HasFlag(JsonSchemaType.Null));
		Assert.DoesNotContain("closePrice", trade.Required!);
		var subscription = document.Components.Schemas["SetStrategySubscriptionCommand"];
		Assert.Contains("isSubscribed", subscription.Required!);
		Assert.Equal(JsonSchemaType.Boolean, subscription.Properties!["isSubscribed"].Type);
	}
}

[ApiController]
[Route("queryBindingTests")]
public sealed class QueryBindingTestController : ControllerBase
{
	[HttpGet("ordinary/record")]
	public OrdinaryBindingModel OrdinaryRecord([FromQuery] OrdinaryBindingModel model) => model;

	[HttpGet("ordinary/class")]
	public OrdinaryBindingClass OrdinaryClass([FromQuery] OrdinaryBindingClass model) => model;

	[HttpGet("ordinary/mixed/{itemId:int}")]
	public OrdinaryBindingModel OrdinaryMixed(
		[
			FromQuery,
			IgnoreProperties(nameof(OrdinaryBindingModel.ServerValue)),
			FromRouteProperties(nameof(OrdinaryBindingModel.Id), Name = "itemId"),
			FromHeaderProperties(nameof(OrdinaryBindingModel.Note), Name = "X-Note")
		]
			OrdinaryBindingModel model
	) => model with { ServerValue = 7 };

	[HttpGet("ordinary/scalar")]
	public IActionResult OrdinaryScalar([FromQuery] string value) => Ok(value);

	[HttpGet("ordinary/collection")]
	public IActionResult OrdinaryCollection([FromQuery] int[] values) => Ok(values);

	[HttpGet("trades")]
	public GetTradesPageQuery Trades(
		[FromQuery, IgnoreProperties(nameof(GetTradesPageQuery.UserId))] GetTradesPageQuery query
	) => query with { UserId = 7 };

	[HttpGet("strategies/{strategyId:int}")]
	public GetStrategyInstrumentsPageQuery Instruments(
		[
			FromQuery,
			IgnoreProperties(nameof(GetStrategyInstrumentsPageQuery.UserId)),
			FromRouteProperties(nameof(GetStrategyInstrumentsPageQuery.StrategyId))
		]
			GetStrategyInstrumentsPageQuery query
	) => query with { UserId = 7 };

	[HttpGet("sessions")]
	public GetSessionsPageQuery Sessions(
		[
			FromQuery,
			IgnoreProperties(nameof(GetSessionsPageQuery.UserId), nameof(GetSessionsPageQuery.CurrentSessionId))
		]
			GetSessionsPageQuery query
	) => query with { UserId = 7, CurrentSessionId = "trusted-session" };

	[HttpDelete("trades/{tradeId:int}")]
	public DeleteTradeCommand Delete(
		[
			FromQuery,
			IgnoreProperties(nameof(DeleteTradeCommand.UserId)),
			FromRouteProperties(nameof(DeleteTradeCommand.TradeId))
		]
			DeleteTradeCommand command
	) => command with { UserId = 7 };

	[HttpGet("statistics")]
	public GetInstrumentStatisticsQuery Statistics([FromQuery] GetInstrumentStatisticsQuery query) => query;

	[HttpGet("profitChart")]
	public GetProfitChartQuery ProfitChart(
		[FromQuery, IgnoreProperties(nameof(GetProfitChartQuery.UserId))] GetProfitChartQuery query
	) => query with { UserId = 7 };
}

public sealed record OrdinaryBindingModel(OrdinaryBindingFilter Filter, string? Note, int Id, int ServerValue);

public sealed class OrdinaryBindingClass
{
	public OrdinaryBindingFilter Filter { get; set; } = null!;
	public string? Note { get; set; }
}

public sealed class OrdinaryBindingFilter
{
	[Range(1, 100)]
	public int PageSize { get; set; } = 20;

	public string[] Tags { get; set; } = [];
}
