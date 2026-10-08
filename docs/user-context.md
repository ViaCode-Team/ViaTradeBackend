# User context

User-facing application handlers obtain the caller from `IUserContext`. Their commands and queries contain client input only; they do not accept `UserId`, `CurrentSessionId`, or a logout session identifier. Controllers no longer extract these claims or supply them with `with`.

Protected handlers inject `IUserContext`, whose `UserId` and `SessionId` properties are non-nullable. Operations that support guests and optional personalization inject `IOptionalUserContext`, whose properties are `int? UserId` and `string? SessionId`. Operations that do not need identity do not need either interface. The constructor selects the contract; DI does not infer it from endpoint authorization metadata.

`HttpUserContext` implements `IUserContext`, and `HttpOptionalUserContext` implements `IOptionalUserContext`. `AddAuthLayer` registers each implementation separately with scoped lifetime. Both read the current principal through `IHttpContextAccessor` when accessed, without capturing a request in their constructors or querying MySQL/Redis. They share identifier validation through `UserClaimsReader`.

- `IUserContext.UserId` and `IUserContext.SessionId` throw `AuthenticationException` when the caller is anonymous or no HTTP context exists. Resolving this interface does not itself require authentication; reading a required property does. The API exception handler converts this exception to HTTP 401, including on endpoints without authorization. The context does not enable authorization by itself.
- `IOptionalUserContext.UserId` and `IOptionalUserContext.SessionId` return null for anonymous callers. For authenticated callers, both contracts read the same identifiers through the same claims reader. Neither contract exposes `FindUserId()` or `FindSessionId()` methods.
- Authenticated identities with missing, empty, duplicate, or invalid identifiers are rejected with `InvalidTokenException`. User IDs must be positive integers. User and session property access is independent.
- JWT claim mapping is explicitly enabled. `sub` becomes `ClaimTypes.NameIdentifier`; `jti` identifies the session. The token validation event rejects malformed identities before handlers run.

The default and fallback authorization policies remain mandatory for protected endpoints. `ActiveSessionHandler` checks that the Redis session exists, has not expired, and belongs to the user identified by the JWT. Required context properties provide identity, not proof that a session is active. Public endpoints bypass the active-session policy; optional identity alone must not authorize access to private data.

Repositories and specifications still receive the owner ID explicitly. Every operation on user-owned records must retain its owner predicate, including bulk updates/deletes. Shared instruments and strategies remain reference data. The context is not injected into the pooled DbContext or repositories.

Login, registration, refresh, and Telegram linking determine their target users through credentials, refresh sessions, or one-time link tokens. `MarkReminderPublishedCommand`, `MarkReminderDeliveredCommand`, and `UpdateLastLoginCommand` retain explicit target user IDs for service operations. Background tasks must not read an ambient HTTP user context.
