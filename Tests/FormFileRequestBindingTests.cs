using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.Swagger;
using ViaTrade.Api.Attributes.Binding;
using ViaTrade.Api.ModelBinding;
using ViaTrade.Application.Common.Abstractions;
using Xunit;

namespace ViaTrade.Tests;

public sealed class FormFileRequestBindingTests(BodyBindingFixture fixture) : IClassFixture<BodyBindingFixture>
{
	[Theory]
	[InlineData("form")]
	[InlineData("query")]
	public async Task MultipartFilesBindWithOtherSourcesAndPreserveTheirContents(string action)
	{
		using var content = new MultipartFormDataContent();
		content.Add(new StringContent("form-text"), "Text");
		content.Add(new StringContent("2"), "count");
		content.Add(new StringContent("invalid"), "itemId");
		content.Add(new StringContent("invalid"), "userId");
		content.Add(new StringContent("spoofed"), "upload");
		AddFile(content, "upload", "first.txt", "hello");
		AddFile(content, "Attachments", "a.txt", "a");
		AddFile(content, "Attachments", "b.txt", "b");
		using var response = await fixture.Client.PostAsync(
			$"/formFileTests/{action}/42?count=3&text=query-text&upload=spoofed&userId=invalid",
			content
		);
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
		Assert.Equal(42, json.RootElement.GetProperty("itemId").GetInt32());
		Assert.Equal(7, json.RootElement.GetProperty("userId").GetInt32());
		Assert.Equal("form-text", json.RootElement.GetProperty("text").GetString());
		Assert.Equal("first.txt", json.RootElement.GetProperty("fileName").GetString());
		Assert.Equal("hello", json.RootElement.GetProperty("contents").GetString());
		var expectedCount = 2;
		if (action == "query")
			expectedCount = 3;
		Assert.Equal(expectedCount, json.RootElement.GetProperty("count").GetInt32());
		Assert.Equal(
			new[] { "a.txt", "b.txt" },
			json.RootElement.GetProperty("attachments").EnumerateArray().Select(item => item.GetString())
		);
	}

	[Theory]
	[InlineData("collection")]
	[InlineData("list")]
	[InlineData("array")]
	[InlineData("enumerable")]
	public async Task FromFormRecognizesFileCollectionsWithoutPropertyOverrides(string action)
	{
		using var content = new MultipartFormDataContent();
		AddFile(content, "Files", "first.txt", "first");
		AddFile(content, "files", "second.txt", "second");
		using var response = await fixture.Client.PostAsync($"/formFileTests/{action}", content);
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		Assert.Equal(new[] { "first.txt", "second.txt" }, await response.Content.ReadFromJsonAsync<string[]>());
	}

	[Theory]
	[InlineData("collection")]
	[InlineData("list")]
	[InlineData("array")]
	[InlineData("enumerable")]
	public async Task OptionalFilesCanBeOmitted(string action)
	{
		using var content = new MultipartFormDataContent();
		content.Add(new StringContent("ignored"), "other");
		using var response = await fixture.Client.PostAsync($"/formFileTests/{action}", content);
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		Assert.Empty((await response.Content.ReadFromJsonAsync<string[]>())!);
		var operation = GetOperation(action);
		Assert.False(operation.RequestBody!.Required);
	}

	[Fact]
	public async Task MissingRequiredFileUsesItsExternalValidationName()
	{
		using var content = new MultipartFormDataContent();
		content.Add(new StringContent("valid"), "Text");
		content.Add(new StringContent("spoofed"), "upload");
		AddFile(content, "wrongName", "wrong.txt", "wrong");
		using var response = await fixture.Client.PostAsync("/formFileTests/form/42", content);
		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
		using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
		Assert.True(json.RootElement.GetProperty("errors").TryGetProperty("upload", out _));
	}

	[Fact]
	public async Task ZeroLengthFilesWithNamesRemainValidUploads()
	{
		using var content = new MultipartFormDataContent();
		content.Add(new StringContent("valid"), "Text");
		AddFile(content, "upload", "empty.txt", "");
		using var response = await fixture.Client.PostAsync("/formFileTests/form/42", content);
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
	}

	[Fact]
	public async Task OrdinaryFormModelsRecognizeSingleFilesWithoutOverrides()
	{
		using var content = new MultipartFormDataContent();
		content.Add(new StringContent("form-text"), "text");
		AddFile(content, "File", "single.txt", "file");
		using var response = await fixture.Client.PostAsync("/formFileTests/ordinary", content);
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		Assert.Equal(new[] { "single.txt", "form-text" }, await response.Content.ReadFromJsonAsync<string[]>());
		var schema = GetOperation("ordinary").RequestBody!.Content!["multipart/form-data"].Schema!;
		Assert.Equal(JsonSchemaType.String | JsonSchemaType.Null, schema.Properties!["file"].Type);
		Assert.Equal("binary", schema.Properties["file"].Format);
	}

	[Fact]
	public async Task IgnoredFilePropertiesAreNotBoundOrExposedInSwagger()
	{
		using var content = new MultipartFormDataContent();
		content.Add(new StringContent("form-text"), "text");
		AddFile(content, "File", "spoof.txt", "file");
		using var response = await fixture.Client.PostAsync("/formFileTests/ignored", content);
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		Assert.Equal(new[] { "", "form-text" }, await response.Content.ReadFromJsonAsync<string[]>());
		Assert.DoesNotContain(
			"file",
			GetOperation("ignored").RequestBody!.Content!["application/x-www-form-urlencoded"].Schema!.Properties!.Keys
		);
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void FilesCannotShareAnActionWithASeparateJsonBodyRegardlessOfParameterOrder(bool reverseParameters)
	{
		var method = typeof(InvalidFileConfigurations).GetMethod(nameof(InvalidFileConfigurations.SeparateBody))!;
		var action = new ActionModel(method, []);
		foreach (var info in method.GetParameters())
		{
			var attributes = info.GetCustomAttributes().ToArray();
			action.Parameters.Add(
				new ParameterModel(info, attributes)
				{
					BindingInfo = BindingInfo.GetBindingInfo(attributes),
					Action = action,
				}
			);
		}
		var parameters = action.Parameters.ToArray();
		if (reverseParameters)
			Array.Reverse(parameters);
		Assert.Throws<InvalidOperationException>(() =>
		{
			foreach (var parameter in parameters)
				new RequestPropertiesConvention().Apply(parameter);
		});
	}

	[Fact]
	public async Task UnsupportedMediaTypesAndMalformedMultipartAreRejected()
	{
		using var jsonResponse = await fixture.Client.PostAsJsonAsync("/formFileTests/form/42", new { text = "valid" });
		Assert.Equal(HttpStatusCode.UnsupportedMediaType, jsonResponse.StatusCode);
		using var formResponse = await fixture.Client.PostAsync(
			"/formFileTests/form/42",
			new FormUrlEncodedContent(new Dictionary<string, string> { ["text"] = "valid" })
		);
		Assert.Equal(HttpStatusCode.UnsupportedMediaType, formResponse.StatusCode);
		using var malformed = new StringContent("broken", Encoding.UTF8);
		malformed.Headers.ContentType = new("multipart/form-data");
		using var malformedResponse = await fixture.Client.PostAsync("/formFileTests/form/42", malformed);
		Assert.Equal(HttpStatusCode.BadRequest, malformedResponse.StatusCode);
	}

	[Fact]
	public async Task FormLimitsAndCollectionValidationRemainActive()
	{
		using var oversized = new MultipartFormDataContent();
		AddFile(oversized, "Files", "large.txt", "12345");
		using var oversizedResponse = await fixture.Client.PostAsync("/formFileTests/limited", oversized);
		Assert.Equal(HttpStatusCode.BadRequest, oversizedResponse.StatusCode);
		using var tooMany = new MultipartFormDataContent();
		for (var index = 0; index < 4; index++)
			AddFile(tooMany, "Files", $"{index}.txt", "a");
		using var tooManyResponse = await fixture.Client.PostAsync("/formFileTests/list", tooMany);
		Assert.Equal(HttpStatusCode.BadRequest, tooManyResponse.StatusCode);
	}

	[Theory]
	[InlineData("form/{itemId}")]
	[InlineData("query/{itemId}")]
	public void SwaggerDescribesFilesAndFieldsInOneMultipartBody(string action)
	{
		var operation = GetOperation(action);
		Assert.True(operation.RequestBody!.Required);
		var mediaType = Assert.Single(operation.RequestBody.Content!);
		Assert.Equal("multipart/form-data", mediaType.Key);
		var schema = mediaType.Value.Schema!;
		var expectedFields = new[] { "attachments", "text", "upload" };
		if (action.StartsWith("form/", StringComparison.Ordinal))
			expectedFields = ["attachments", "count", "text", "upload"];
		Assert.Equal(expectedFields, schema.Properties!.Keys.Order());
		Assert.Contains("upload", schema.Required!);
		AssertBinary(schema.Properties["upload"]);
		var attachments = schema.Properties["attachments"];
		Assert.Equal(JsonSchemaType.Array | JsonSchemaType.Null, attachments.Type);
		AssertBinary(attachments.Items!);
		var route = Assert.Single(operation.Parameters!, parameter => parameter.In == ParameterLocation.Path);
		Assert.Equal("itemId", route.Name);
		Assert.Equal(ParameterLocation.Path, route.In);
		if (action.StartsWith("query/", StringComparison.Ordinal))
		{
			var query = Assert.Single(operation.Parameters!, parameter => parameter.In == ParameterLocation.Query);
			Assert.Equal("count", query.Name);
			Assert.Equal("10", query.Schema!.Maximum);
		}
	}

	[Theory]
	[InlineData("collection")]
	[InlineData("list")]
	[InlineData("array")]
	[InlineData("enumerable")]
	public void SwaggerDescribesAutomaticFileCollectionsAsBinaryArrays(string action)
	{
		var body = GetOperation(action).RequestBody!;
		var files = body.Content!["multipart/form-data"].Schema!.Properties!["files"];
		Assert.Equal(JsonSchemaType.Array | JsonSchemaType.Null, files.Type);
		AssertBinary(files.Items!);
		if (action == "list")
		{
			Assert.Equal(1, files.MinItems);
			Assert.Equal(3, files.MaxItems);
		}
	}

	[Theory]
	[InlineData(nameof(InvalidFileConfigurations.NotAFile))]
	[InlineData(nameof(InvalidFileConfigurations.BodyAndFile))]
	[InlineData(nameof(InvalidFileConfigurations.DuplicateFormName))]
	public void InvalidFileConfigurationFailsDuringDiscovery(string method)
	{
		var info = typeof(InvalidFileConfigurations).GetMethod(method)!.GetParameters().Single();
		var attributes = info.GetCustomAttributes().ToArray();
		var parameter = new ParameterModel(info, attributes) { BindingInfo = BindingInfo.GetBindingInfo(attributes) };
		Assert.Throws<InvalidOperationException>(() => new RequestPropertiesConvention().Apply(parameter));
	}

	private OpenApiOperation GetOperation(string action) =>
		fixture
			.App.Services.GetRequiredService<ISwaggerProvider>()
			.GetSwagger("web")
			.Paths[$"/formFileTests/{action}"]
			.Operations![HttpMethod.Post];

	private static void AssertBinary(IOpenApiSchema schema)
	{
		Assert.Equal(JsonSchemaType.String, schema.Type);
		Assert.Equal("binary", schema.Format);
	}

	private static void AddFile(MultipartFormDataContent content, string name, string fileName, string value) =>
		content.Add(new ByteArrayContent(Encoding.UTF8.GetBytes(value)), name, fileName);

	private sealed class InvalidFileConfigurations
	{
		public void NotAFile(
			[FromQuery, FromFormFileProperties(nameof(FileUploadCommand.Text))] FileUploadCommand command
		) { }

		public void BodyAndFile(
			[FromBody, FromFormFileProperties(nameof(FileUploadCommand.File))] FileUploadCommand command
		) { }

		public void DuplicateFormName(
			[FromForm, FromFormFileProperties(nameof(FileUploadCommand.File), Name = "text")] FileUploadCommand command
		) { }

		public void SeparateBody(
			[FromForm] FileArrayCommand files,
			[FromQuery, FromBodyProperties(nameof(BodyBindingCommand.Text))] BodyBindingCommand body
		) { }
	}
}

[ApiController]
[Route("formFileTests")]
public sealed class FormFileTestController : ControllerBase
{
	[HttpPost("form/{itemId}")]
	public Task<IActionResult> Form(
		[
			FromForm,
			IgnoreProperties(nameof(FileUploadCommand.UserId)),
			FromRouteProperties(nameof(FileUploadCommand.ItemId)),
			FromFormFileProperties(nameof(FileUploadCommand.File), Name = "upload")
		]
			FileUploadCommand command
	) => DescribeFile(command with { UserId = 7 });

	[HttpPost("query/{itemId}")]
	public Task<IActionResult> Query(
		[
			FromQuery,
			IgnoreProperties(nameof(FileUploadCommand.UserId)),
			FromRouteProperties(nameof(FileUploadCommand.ItemId)),
			FromFormProperties(nameof(FileUploadCommand.Text)),
			FromFormFileProperties(nameof(FileUploadCommand.File), Name = "upload"),
			FromFormFileProperties(nameof(FileUploadCommand.Attachments))
		]
			FileUploadCommand command
	) => DescribeFile(command with { UserId = 7 });

	[HttpPost("collection")]
	public IActionResult Collection([FromForm] FileCollectionCommand command) => Ok(GetNames(command.Files));

	[HttpPost("list")]
	public IActionResult List([FromForm] FileListCommand command) => Ok(GetNames(command.Files));

	[HttpPost("array")]
	public IActionResult Array([FromForm] FileArrayCommand command) => Ok(GetNames(command.Files));

	[HttpPost("enumerable")]
	public IActionResult Enumerable([FromForm] FileEnumerableCommand command) => Ok(GetNames(command.Files));

	[HttpPost("limited")]
	[RequestFormLimits(MultipartBodyLengthLimit = 4)]
	public IActionResult Limited([FromForm] FileListCommand command) => Ok(GetNames(command.Files));

	[HttpPost("ordinary")]
	public IActionResult Ordinary([FromForm] OrdinaryFileModel model) =>
		Ok(new[] { model.File?.FileName ?? "", model.Text ?? "" });

	[HttpPost("ignored")]
	public IActionResult Ignored(
		[FromForm, IgnoreProperties(nameof(OrdinaryFileModel.File))] OrdinaryFileModel model
	) => Ok(new[] { model.File?.FileName ?? "", model.Text ?? "" });

	private async Task<IActionResult> DescribeFile(FileUploadCommand command)
	{
		using var reader = new StreamReader(command.File.OpenReadStream());
		var contents = await reader.ReadToEndAsync();
		return Ok(
			new
			{
				command.ItemId,
				command.UserId,
				command.Text,
				command.File.FileName,
				command.Count,
				contents,
				attachments = GetNames(command.Attachments),
			}
		);
	}

	private static string[] GetNames(IEnumerable<IFormFile>? files) =>
		files?.Select(file => file.FileName).ToArray() ?? [];
}

public sealed record FileUploadCommand(
	int ItemId,
	[Required] IFormFile File,
	[Required] string Text,
	int UserId,
	IFormFile[]? Attachments
) : ICommand
{
	[Range(1, 10)]
	public int Count { get; init; } = 1;
}

public sealed record FileCollectionCommand(IFormFileCollection? Files) : ICommand;

public sealed record FileArrayCommand(IFormFile[]? Files) : ICommand;

public sealed record FileEnumerableCommand(IEnumerable<IFormFile>? Files) : ICommand;

public sealed class FileListCommand : ICommand
{
	[MinLength(1), MaxLength(3)]
	public List<IFormFile>? Files { get; init; }
}

public sealed class OrdinaryFileModel
{
	public IFormFile? File { get; init; }
	public string? Text { get; init; }
}
