using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using ViaTrade.Api.ModelBinding.Attributes;

namespace ViaTrade.Api.ModelBinding;

public sealed class IgnorePropertiesJsonInputFormatter : TextInputFormatter, IInputFormatterExceptionPolicy
{
	private readonly SystemTextJsonInputFormatter _inner;
	private readonly ConditionalWeakTable<ModelMetadata, JsonTypeInfo> _typeInfos = new();

	public IgnorePropertiesJsonInputFormatter(SystemTextJsonInputFormatter inner)
	{
		_inner = inner;

		foreach (var encoding in inner.SupportedEncodings)
			SupportedEncodings.Add(encoding);

		foreach (var mediaType in inner.SupportedMediaTypes)
			SupportedMediaTypes.Add(mediaType);
	}

	public InputFormatterExceptionPolicy ExceptionPolicy => InputFormatterExceptionPolicy.MalformedInputExceptions;

	public override bool CanRead(InputFormatterContext context) =>
		(
			IgnorePropertiesAttribute.Find(context.Metadata) is not null
			|| RequestPropertyBinding.From(context.Metadata, BindingSource.Body).HasOverrides
		) && _inner.CanRead(context);

	public override async Task<InputFormatterResult> ReadRequestBodyAsync(
		InputFormatterContext context,
		Encoding encoding
	)
	{
		var serializerOptions = _inner.SerializerOptions;
		var excludedJsonNames = RequestPropertyBinding
			.From(context.Metadata, BindingSource.Body)
			.GetJsonBodyExclusions(serializerOptions);
		var requestBody = context.HttpContext.Request.Body;
		Stream? transcodingStream = null;

		try
		{
			var inputStream = requestBody;
			if (encoding.CodePage != Encoding.UTF8.CodePage)
			{
				transcodingStream = Encoding.CreateTranscodingStream(
					requestBody,
					encoding,
					Encoding.UTF8,
					leaveOpen: true
				);
				inputStream = transcodingStream;
			}

			using var document = await JsonDocument.ParseAsync(
				inputStream,
				new JsonDocumentOptions
				{
					AllowTrailingCommas = serializerOptions.AllowTrailingCommas,
					CommentHandling = serializerOptions.ReadCommentHandling,
					MaxDepth = serializerOptions.MaxDepth,
				},
				context.HttpContext.RequestAborted
			);

			await using var filteredBody = new MemoryStream();
			WriteFilteredBody(document.RootElement, filteredBody, excludedJsonNames, serializerOptions.MaxDepth);
			filteredBody.Position = 0;

			var model = await JsonSerializer.DeserializeAsync(
				filteredBody,
				_typeInfos.GetValue(context.Metadata, CreateRootTypeInfo),
				context.HttpContext.RequestAborted
			);
			if (model is null && !context.TreatEmptyInputAsDefaultValue)
				return InputFormatterResult.NoValue();

			return InputFormatterResult.Success(model);
		}
		catch (JsonException exception)
		{
			AddJsonModelError(context, exception);
			return InputFormatterResult.Failure();
		}
		catch (Exception exception) when (exception is FormatException or OverflowException)
		{
			context.ModelState.TryAddModelError(string.Empty, exception, context.Metadata);
			return InputFormatterResult.Failure();
		}
		finally
		{
			if (transcodingStream is not null)
				await transcodingStream.DisposeAsync();
		}
	}

	private static void WriteFilteredBody(
		JsonElement root,
		Stream output,
		HashSet<string> excludedJsonNames,
		int maxDepth
	)
	{
		using var writer = new Utf8JsonWriter(output, new JsonWriterOptions { MaxDepth = maxDepth });
		if (root.ValueKind != JsonValueKind.Object)
		{
			root.WriteTo(writer);
			return;
		}

		writer.WriteStartObject();
		foreach (var property in root.EnumerateObject())
			if (!excludedJsonNames.Contains(property.Name))
				property.WriteTo(writer);
		writer.WriteEndObject();
	}

	private static void AddJsonModelError(InputFormatterContext context, JsonException exception)
	{
		Exception modelStateException = exception;
		var allowExceptionMessages =
			context
				.HttpContext.RequestServices?.GetService<IOptions<JsonOptions>>()
				?.Value.AllowInputFormatterExceptionMessages
			?? true;
		if (allowExceptionMessages)
			modelStateException = new InputFormatterException(exception.Message, exception);

		context.ModelState.TryAddModelError(exception.Path ?? string.Empty, modelStateException, context.Metadata);
	}

	private JsonTypeInfo CreateRootTypeInfo(ModelMetadata metadata)
	{
		var excludedJsonNames = RequestPropertyBinding
			.From(metadata, BindingSource.Body)
			.GetJsonBodyExclusions(_inner.SerializerOptions);
		var resolver = _inner.SerializerOptions.TypeInfoResolver ?? new DefaultJsonTypeInfoResolver();
		var options = new JsonSerializerOptions(_inner.SerializerOptions);
		var typeInfo =
			resolver.GetTypeInfo(metadata.ModelType, options)
			?? throw new InvalidOperationException($"No JSON contract exists for '{metadata.ModelType.Name}'.");

		if (typeInfo.Kind == JsonTypeInfoKind.Object)
			foreach (var property in typeInfo.Properties.Where(property => excludedJsonNames.Contains(property.Name)))
				property.IsRequired = false;

		return typeInfo;
	}
}
