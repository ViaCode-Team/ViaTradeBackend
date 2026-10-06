# Request binding and validation

Binding supports any request model without requiring command or query interfaces. Application actions accept the command or query handled by their injected handler when it has client-bound properties. `[FromBody]`, `[FromQuery]`, or `[FromForm]` selects the default source. Without an explicit source, MVC selects it normally: complex request models in `[ApiController]` use JSON body binding, and registered complex services retain service inference. Use `[FromQuery]` for query-based models. `IgnoreProperties` and property-source attributes do not impose a query default. `FromBodyProperties`, `FromRouteProperties`, `FromQueryProperties`, `FromHeaderProperties`, `FromFormProperties`, and `FromFormFileProperties` override the default source for named properties. Claims, cookies, and configured values are supplied with `with` before the handler validates the completed command. Requests with no client-bound properties are constructed directly inside `HandleAsync`, for example `handler.HandleAsync(new GetNoteStatisticsQuery(userId), ct)`. Application types do not need HTTP or JSON attributes.

## Built-in MVC binding and alternatives

Ordinary query/form models use standard MVC binding. The custom layer handles action-specific property overrides, exclusions, form files, and flattened nested query models, delegating conversion and file binding to MVC binders. Explicit `ModelBinder(BinderType = ...)` settings take precedence over the global request binder. For query/form members, an action-specific property source takes precedence over the member's MVC source metadata, which takes precedence over the parameter's default source. `Bind` filters and `BindNever` are respected before conversion, and absent `BindRequired` members produce model-state errors using their external names. Missing record constructor arguments retain their declared defaults; explicitly supplied zero, false, and enum values remain unchanged.

| Built-in feature | Purpose | Relationship to the custom layer |
| --- | --- | --- |
| `Bind("PropertyA", "PropertyB")` | Allows only listed members during query/form binding. | Supported. Does not filter JSON input or suppress validation of excluded members. |
| `BindNever` | Prevents binding of the annotated member or type. | Supported for query/form models. Does not replace parameter-specific JSON exclusions. |
| `BindRequired` | Requires a value to be supplied during model binding. | Supported, including external names. Does not declare JSON serializer requiredness. |
| `ModelBinder(Name = "external")` | Changes the external field name. | Supported and projected to Swagger. |
| `ModelBinder(BinderType = typeof(CustomBinder))` | Selects an `IModelBinder` implementation. | Takes precedence; the selected binder owns binding behavior. |
| `FromQuery`, `FromRoute`, `FromHeader`, `FromForm` on members | Chooses a member's source. | Supported for query/form models, without adding action-specific source attributes. |
| `FromBody` | Deserializes the action parameter using an input formatter. | Standard MVC ignores member source attributes inside a body model. Mixed JSON and other sources still require the custom layer. |
| `JsonIgnore` | Excludes a JSON member for every use of its type. | Does not provide action-specific exclusions and can also change output serialization. |

Using separate API request DTOs and separate route/header/query parameters, then constructing an application command in the action, is the built-in alternative that removes the need for mixed-source binding. This changes action signatures and mapping code. Keep the custom layer when action-specific configuration, direct application commands, flat nested filters, and mixed JSON sources are required together.

Reference: [ASP.NET Core model binding](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding?view=aspnetcore-10.0) and [custom model binding](https://learn.microsoft.com/en-us/aspnet/core/mvc/advanced/custom-model-binding?view=aspnetcore-10.0).

[HybridModelBinding](https://github.com/billbogaiv/hybrid-model-binding) provides mixed body/form/route/query/header binding with ordered fallbacks and model-level property attributes. Its documented examples target ASP.NET Core 3.1; compatibility with this .NET 10 application and its action-specific exclusions, record models, flattened filters, and Swagger projection has not been verified. [FastEndpoints](https://fast-endpoints.com/docs/model-binding) also supports mixed request sources, but adopting its endpoint model changes the MVC controller architecture. Neither is installed here. [`AsParameters`](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding?view=aspnetcore-10.0#parameter-binding-for-argument-lists-with-asparameters) groups parameters in Minimal APIs and is not a replacement for an MVC model binder.

```csharp
[FromBody,
 IgnoreProperties(nameof(UpdateTradeCommand.UserId)),
 FromRouteProperties(nameof(UpdateTradeCommand.TradeId))]
UpdateTradeCommand command
```

Non-nullable body action parameters do not need a separate `Required` annotation. The global Swagger operation filter uses MVC API Explorer and model-validation metadata to document required bodies, including mixed-source requests. MVC continues to reject missing bodies and JSON `null` for these parameters with HTTP 400. MVC infers optional bodies for nullable parameters; `[FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)]` also makes the policy explicit. The filter respects `MvcOptions.AllowEmptyInputInBodyModelBinding`, implicit/explicit model validation, and `EmptyBodyBehavior.Disallow`. Form requiredness remains determined by the form fields.

`IgnoreProperties` accepts CLR property names, preferably expressed with `nameof`. It excludes only the named top-level properties. JSON names follow the configured naming policy and `JsonPropertyName`; case sensitivity follows `PropertyNameCaseInsensitive`. All occurrences of an excluded property are removed, including duplicate and escaped names, before the command is deserialized. Their values can therefore have any valid JSON type.

MVC skips validation of the excluded properties until the action supplies them. Validation of other properties, nested objects, route parameters, and the required body remains active. Application handler validation still checks the completed command. The attribute does not change output serialization or other parameters that use the same command type.

Excluded top-level `required` and `JsonRequired` properties are removed from the input and their JSON property-required flags are disabled in an isolated root contract. Nested models, including nested instances of the same CLR type, retain their original serializer contracts. The formatter reads the original request stream without replacing it.

The exclusion attribute supports body, query, form, or standard MVC model binding and requires readable public properties. An explicit default-source attribute is optional; normal MVC inference applies when one is absent. Invalid names or placement fail during action discovery. Existing request size, JSON depth, encoding, and media type restrictions remain in force. When body properties remain, malformed JSON is still rejected, even inside an excluded value.

Swagger projects each annotated parameter's request schema separately, excluding the named properties and their `required` entries without changing shared schemas.

## Query and route binding

```csharp
public async Task<Ok<TradeResult>> GetTradeById(
    [IgnoreProperties(nameof(GetTradeQuery.UserId)),
     FromRouteProperties(nameof(GetTradeQuery.TradeId))]
    GetTradeQuery query,
    [FromServices] IQueryHandler<GetTradeQuery, TradeResult> handler,
    CancellationToken ct)
{
    query = query with { UserId = jwtHelper.GetUserIdFromClaims(User) };
    return TypedResults.Ok(await handler.HandleAsync(query, ct));
}
```

`IgnoreProperties` replaces `IgnoreBodyProperties`. Ignored properties are skipped before conversion and MVC validation, including malformed spoofed identifiers. The property source attributes bind client properties exclusively from the selected source; a route/header/query property is removed from JSON before deserialization. Missing route values and invalid client input produce model-state errors. Completed requests are validated by the application handler decorators after trusted values are supplied with `with`.

If every property is ignored or explicitly assigned to a non-body source, `[FromBody]` does not require or read a body, and Swagger omits the request body. For example, `[IgnoreProperties(nameof(GetNoteQuery.UserId)), FromRouteProperties(nameof(GetNoteQuery.NoteId))]` and the same configuration with `[FromBody]` both bind only `NoteId` from the route. Genuine body properties retain the existing missing-body, JSON parsing, and validation behavior.

When unconfigured properties remain, complex `[ApiController]` parameters use the inferred JSON body even on GET actions. Nullable inferred body parameters retain MVC's optional-body policy. Property exclusions and source overrides are applied consistently to MVC binding, JSON deserialization, validation, and Swagger using the selected parameter source.

Each property source attribute is repeatable and accepts several CLR property names for a shared source. Their common base uses the standard MVC `BindingSource` instances and existing binders; it does not implement `IBindingSourceMetadata`, so an override does not change the source of the whole parameter. `Name` sets an external alias for exactly one property:

```csharp
[FromBody,
 FromBodyProperties(nameof(UpdateTradeCommand.OpenPrice), nameof(UpdateTradeCommand.Quantity)),
 FromRouteProperties(nameof(UpdateTradeCommand.TradeId), Name = "tradeId"),
 IgnoreProperties(nameof(UpdateTradeCommand.UserId))]
UpdateTradeCommand command

[FromBody,
 FromHeaderProperties(nameof(LoginCommand.UserAgent), Name = "User-Agent")]
LoginCommand command
```

For a `[FromBody]` parameter, body properties need no override. For a `[FromQuery]` parameter, `FromBodyProperties` enables one JSON body while the remaining properties retain their query sources. Body property names follow the JSON naming policy and `JsonPropertyName`; `Name` is available for the other sources. Only top-level properties can be selected. Properties need a public setter or `init` accessor. Route fields must match the action's route template and have scalar types; headers support scalar types and string arrays. JSON body and form fields cannot share a parameter. Duplicate sources, ignored-and-bound properties, invalid aliases, and unknown properties fail during action discovery.

The query binder flattens the immediate required nested objects of any request model: `?page=2&pageSize=20&instrumentIds=1&instrumentIds=2`, without `pageOptions.` or `instrumentFilter.` prefixes. MVC binds the child models, preserving their defaults, enum/date conversion, and repeated collections. Missing required query filters are initialized just as the former separate action parameters were. Optional nested objects retain their property prefixes and remain absent when no matching values are supplied. Scalar and collection query parameters retain their standard MVC binding. Swagger uses the same query names and preserves nullable properties of action request models. Output serialization remains unchanged.

An explicit non-empty nested alias retains its prefix. Duplicate field names after flattening fail during action discovery; use distinct prefixes to disambiguate nested models. Swagger derives query/form names from MVC binding metadata rather than JSON member names, so `JsonPropertyName` does not rename query fields. Multiple form models and scalar form parameters are merged into one request body schema.

## File uploads

`[FromForm]` automatically binds top-level `IFormFile`, `IFormFile[]`, `List<IFormFile>`, `IEnumerable<IFormFile>`, and `IFormFileCollection` properties with MVC's standard file binder. No property attribute is needed when the default source is form. `FromFormFileProperties` explicitly selects files for a query-based request or supplies an external name:

```csharp
[FromForm,
 IgnoreProperties(nameof(UploadCommand.UserId)),
 FromRouteProperties(nameof(UploadCommand.InstrumentId)),
 FromFormFileProperties(nameof(UploadCommand.File), Name = "upload")]
UploadCommand command
```

Upload actions receive an automatic `Consumes("multipart/form-data")` constraint unless an explicit `Consumes` is present. Files and ordinary form fields share one multipart body in Swagger; a file is `string` with `format: binary`, and a collection is an array of binary strings. Collections use repeated parts with the same field name. Required fields, collection constraints, and aliases retain their schema and validation metadata. Nullable file properties can be omitted; named zero-length files remain valid uploads. Ignored file properties are neither bound nor documented. The shared schema is preserved.

Files cannot share an action with a JSON body. Unsupported file property types and duplicate form/file names fail during action discovery. Form parsing, multipart limits, buffering, and validation use the existing MVC infrastructure. The binding layer does not persist files or apply business-specific file-content checks; handlers remain responsible for processing accepted uploads.

## FluentValidation and OpenAPI

`MicroElements.Swashbuckle.FluentValidation` 7.2.2 supplies schema rules. The custom operation projection uses those schemas for query, route, header, and form fields according to the property source attributes, and excludes server-owned fields from client parameters and JSON bodies. The library's operation filter is replaced because it assumes MVC's nested query prefixes. Its schema filter remains enabled with scoped validator resolution.

Shared validators/extensions cover user IDs, pagination, search length, text inputs, nested models, collection limits, date ranges, and sorting. Search and sort validation now runs in the application through FluentValidation, including calls outside MVC. Open generic helper validators are composed via `SetValidator` and are excluded from DI scanning. Validators execute asynchronously after `with`, supporting async rules and cancellation without MVC FluentValidation auto-validation.

OpenAPI describes required properties, string lengths/patterns/formats, numeric bounds, enums, and collection/item limits supported by the integration. `MaximumItemsValidator` exposes the collection length metadata rather than hiding it in `Must`. A custom range rule preserves floating-point bounds such as `double.Epsilon` and `double.MaxValue`, avoiding the library's decimal conversion. Nullable body inputs retain their nullability; paging and initialized sort collections remain optional query parameters.

Arbitrary `Must` predicates and relationships between fields (date ordering, paired close price/date, conflicting sort fields) cannot be translated automatically into equivalent OpenAPI 3.0 constraints. They remain enforced by FluentValidation. Shared schemas are not mutated by action-specific exclusions.

During `dotnet build`, the official CLI generates all three documents in the configuration-specific `obj` directory before replacing the files under `swagger/`. Replacing files avoids truncating YAML documents held open by memory-mapped readers. Generator failures fail the build; design-time builds skip generation.

## Request body changes

Command payload fields are bound directly from flat JSON bodies, without separate Input models:

| Action | JSON body | Values supplied by the action |
| --- | --- | --- |
| Register / Login | `{ "login": "...", "password": "..." }` | `UserAgent` from the header |
| CreateTrade | `{ "openedAt": "...", "openPrice": 100, "quantity": 1, ... }` | `UserId` from claims |
| UpdateTrade | `{ "openedAt": "...", "openPrice": 100, "quantity": 1, ... }` | `UserId` from claims, `TradeId` from the route |
| UpsertInstrumentNote | `{ "text": "..." }` | `UserId` from claims, `InstrumentId` from the route |
| UpsertStrategyNote | `{ "text": "..." }` | `UserId` from claims, `StrategyId` from the route |
| CreateInstrumentReminder | `{ "text": "...", "remindAt": "..." }` | `UserId` from claims, `InstrumentId` from the route |
| UpdateReminder | `{ "text": "...", "remindAt": "..." }` | `UserId` from claims, `ReminderId` from the route |
| UpdateStrategy | `{ "isSubscribed": true }` | `UserId` from claims, `StrategyId` from the route |
| ConfirmDelivery (internal TgBot API) | `{ "userId": 7 }` | `ReminderId` from the route |

Clients using the former nested command bodies must send the flat shapes shown in Swagger. Validation errors use direct field names, such as `quantity` and `password`, without Input property prefixes. Subscription requests must explicitly supply `isSubscribed` as `true` or `false`; an absent or null field is rejected. The internal Telegram link action already accepted its command directly and retains its existing body. Actions without JSON bodies receive their command/query directly while retaining the existing flat query parameter names.

