using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using Xunit;

namespace ViaTrade.Tests;

public sealed class RequestBodyRequiredTests(BodyBindingFixture fixture) : IClassFixture<BodyBindingFixture>
{
	[Theory]
	[InlineData("default", false)]
	[InlineData("defaultNonNullable", true)]
	[InlineData("allow", false)]
	[InlineData("disallow", true)]
	[InlineData("allowNonNullable", true)]
	[InlineData("allowRequired", true)]
	public async Task SwaggerRequirednessMatchesMvcForEmptyBodiesAndJsonNull(string action, bool required)
	{
		await AssertBodyPolicy(fixture, action, required);
	}

	[Fact]
	public async Task GlobalAllowEmptyBodyIsRespectedAndExplicitDisallowOverridesIt()
	{
		var configuredFixture = new BodyBindingFixture();
		await configuredFixture.InitializeAsync(allowEmptyBody: true, suppressImplicitRequired: true);
		try
		{
			await AssertBodyPolicy(configuredFixture, "default", required: false);
			await AssertBodyPolicy(configuredFixture, "defaultNonNullable", required: false);
			await AssertBodyPolicy(configuredFixture, "disallow", required: true);
			await AssertBodyPolicy(configuredFixture, "allowRequired", required: true);
		}
		finally
		{
			await configuredFixture.DisposeAsync();
		}
	}

	[Theory]
	[InlineData("web")]
	[InlineData("tgbot")]
	[InlineData("analyzer")]
	public void ProductionRequestBodiesRemainRequiredWithoutActionAnnotations(string documentName)
	{
		var document = fixture.App.Services.GetRequiredService<ISwaggerProvider>().GetSwagger(documentName);
		var operations = document
			.Paths.Where(path => path.Key.StartsWith("/api/", StringComparison.Ordinal))
			.SelectMany(path => path.Value.Operations!.Values)
			.Where(operation => operation.RequestBody is not null)
			.ToArray();
		if (documentName != "analyzer")
			Assert.NotEmpty(operations);
		Assert.All(operations, operation => Assert.True(operation.RequestBody!.Required));
	}

	private static async Task AssertBodyPolicy(BodyBindingFixture bodyFixture, string action, bool required)
	{
		var path = $"/requestBodyRequiredTests/{action}";
		foreach (var json in new[] { "", "null" })
		{
			using var response = await bodyFixture.Client.PostAsync(
				path,
				new StringContent(json, Encoding.UTF8, "application/json")
			);
			var expectedStatus = HttpStatusCode.OK;
			if (required)
				expectedStatus = HttpStatusCode.BadRequest;
			Assert.Equal(expectedStatus, response.StatusCode);
		}
		var document = bodyFixture.App.Services.GetRequiredService<ISwaggerProvider>().GetSwagger("web");
		Assert.Equal(required, document.Paths[path].Operations![HttpMethod.Post].RequestBody!.Required);
	}
}

[ApiController]
[Route("requestBodyRequiredTests")]
public sealed class RequestBodyRequiredTestController : ControllerBase
{
	[HttpPost("default")]
	public IActionResult Default([FromBody] string? value) => Ok(new { value });

	[HttpPost("defaultNonNullable")]
	public IActionResult DefaultNonNullable([FromBody] string value) => Ok(new { value });

	[HttpPost("allow")]
	public IActionResult Allow([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] string? value) =>
		Ok(new { value });

	[HttpPost("disallow")]
	public IActionResult Disallow([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Disallow)] string? value) =>
		Ok(new { value });

	[HttpPost("allowNonNullable")]
	public IActionResult AllowNonNullable([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] string value) =>
		Ok(new { value });

	[HttpPost("allowRequired")]
	public IActionResult AllowRequired(
		[FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow), Required] string? value
	) => Ok(new { value });
}
