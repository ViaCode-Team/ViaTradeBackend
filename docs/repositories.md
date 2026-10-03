# Repositories and specifications

Application services use `IReadRepository<TEntity>` for reads and `IRepository<TEntity>` for writes. Both are registered as open generics backed by the scoped `AppDbContext`. Simple filters and projections use expression overloads directly; they do not require separate specification classes.

Specifications live in `Application/<Feature>/Specifications` and use Ardalis.Specification 9.3.1. They encapsulate filters, navigation includes, ordering, and bounded batches. Services pass DTO projection expressions directly at the query call site. `PageSpecification.Project` combines an expression with existing filters and ordering through `WithProjectionOf`. `OrderedSpecification` builds one `OrderBy` followed by `ThenBy` clauses. Page specifications append the entity ID for stable ordering. Related strategy specifications inherit the user ownership filter from `UserStrategyLinksSpecification`. Search criteria apply independently of exact filters.

Generic repositories have no `GetPageAsync` methods. Services pass `PageOptions` only to specification constructors, which snapshot the page number, size, and offset. `PageSpecification.ForPage` creates a separate projected specification with `Skip` and `Take`, leaving the original filters unchanged. `PageQuery` in Application builds `PageResult` using the repository's existing `ListAsync` and `CountAsync`. A first page that fits needs one SELECT; an overflowing first page also executes COUNT. Later pages execute COUNT first and skip the data SELECT when the offset is at or beyond the total. Exact instrument symbols and strategy names follow the same pagination rules, preserving the actual total on empty later pages. Cancelled page queries send no SQL. Specifications do not validate inputs; API model validation and service checks remain responsible for that.

`PageQuery.FromList` applies the same page metadata to already ordered in-memory collections. Signal services use it after collecting file data; session services use it after `ISessionRepository.ListByUserAsync` loads existing sessions. Neither flow forwards `PageOptions` to a repository or paging helper.

Database reads follow the globally configured `NoTracking` behavior. Add/remove operations are staged until `IUnitOfWork.SaveChangesAsync` or repository `SaveChangesAsync`. To save changes to a detached entity, call repository `Update` explicitly first. `GetByIdAsync` queries through EF and respects global filters; user-owned data must use a predicate or specification containing `UserId` together with the requested ID. Instruments and strategies are shared reference data.

Specialized repositories have independent interfaces and do not inherit the generic repository. They retain only operations that need SQL aggregation, queries across independent entity sets, or typed atomic updates:

- `INoteRepository`: aggregate counts and update note text.
- `IReminderRepository`: conditionally update, publish, or confirm delivery.
- `IStrategyRepository`: statistics and strategy/instrument link state across entity sets.
- `ITradeRepository`: statistics, chart buckets, date range, and update trade fields.
- `IUserRepository`: update Telegram ID and last login time.

Deletes use generic `ExecuteDeleteAsync` with a predicate or specification. Updates retain EF `ExecuteUpdateAsync` internally rather than loading entities, and return affected row counts for services to evaluate. These operations execute immediately without `SaveChangesAsync` and do not synchronize tracked entities. Each call is a separate SQL statement; a workflow requiring multiple statements to commit together needs an explicit transaction. See [EF Core bulk operations](https://learn.microsoft.com/en-us/ef/core/saving/execute-insert-update-delete).

User-facing bulk operations include ownership predicates. Publishing a reminder also carries its user ID. Delivered-reminder cleanup and due-reminder batches intentionally span users as background maintenance operations.

Redis caches use `ICacheRepository<TEntity>` backed by `BaseRedisRepository<TEntity>`. Telegram tokens use the generic cache directly, including atomic `ConsumeAsync` implemented with Redis GETDEL. Session storage keeps its dedicated repository because its Lua scripts, indexes, rotation, and expiry handling cannot be represented by ordinary cache operations.

Run `dotnet build` and `dotnet test Tests/Tests.csproj`. Regression tests execute against SQLite in memory with the application EF model, and separately check SQL translation with the production Pomelo MySQL provider. They do not execute against a live MySQL or Redis server.
