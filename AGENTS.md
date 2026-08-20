# AI AGENT INSTRUCTIONS (AGENTS.md)
> **FOR AI AGENTS ONLY**: Critical rules. Read and strictly adhere.

## Meta-Rules for AGENTS.md
- **Language**: Must be written entirely in clear, high-quality English.
- **Conciseness**: Keep instructions brief, elegant, and directly actionable.
- **Length**: Must not exceed 100 lines. If this file exceeds 6000 characters (checked when editing), ask the user if they want to compress it without losing any meaning or details.

## Agent Behavior & Workflow
1. **Strict Adherence**: Follow prompts exactly. Do not invent unrequested tasks or behavior. Ask for clarification before changing ambiguous work.
2. **Verify & Thoroughness**: Verify facts in code, double-check affected code, and do not leave work incomplete. Run `grep_search` for dangling references and remove unused variables.
3. **User Edits**: Respect manual user code changes. Ask before reverting user-authored code.
4. **Proactive Rules**: Propose `AGENTS.md` updates if users repeatedly ask for specific behaviors.
5. **Complete Refactoring & Dead Code Removal**: When replacing a feature, property, or logic, remove every superseded implementation; never leave old code alongside the replacement.
6. **Explicit Change Scope (CRITICAL)**: VERY IMPORTANT: NEVER change anything unless the user explicitly requests it. If you have an idea or want to propose a change, ask the user first. Implement only what is explicitly requested. When the user asks a question, just answer it without modifying code. Do not combine explanatory questions with implementation work by assumption.
7. **Exception Ownership**: Repositories must not throw business exceptions. Keep data access errors technical in repositories; services must evaluate business conditions and throw domain-specific exceptions.
8. **Generated Files**: Never create or edit migrations, model snapshots, or other generated files manually. Use the relevant official generator and include its output unchanged.
9. **Avoid Premature Abstraction**: Keep one-off code local unless extraction is justified by reuse, correctness, maintainability, or an explicit user requirement.

## Mandatory Agent Workflow
1. **Build Verification**: After making ANY code changes, you MUST run `dotnet build` to verify the project compiles without errors.
2. **Error Resolution**: If the build fails, you MUST fix all compilation errors and rebuild until the build succeeds.
3. **Warning Resolution**: Whenever possible, fix compiler warnings as well.
4. **Code Replacement Accuracy**: NEVER guess or hallucinate the `TargetContent` for the `replace_file_content` tool. ALWAYS use `view_file` or `grep_search` to read the exact lines from the file first. Providing inaccurate target content causes the fuzzy matcher to make unintended, destructive changes to the code.
5. **Temporary Files**: Remove every temporary or generated artifact you create, including diagrams, screenshots, and build output, before task completion; verify none remain untracked.

## Strict Security (CRITICAL)
1. **Prevent IDOR**: NEVER query/update/delete by ID alone. ALL DB actions MUST verify ownership (`&& e.UserId == currentUserId`).
2. **Secure by Default**: Do NOT remove global auth policies or add `[AllowAnonymous]` without explicit permission.
3. **Data Leaks (DTOs)**: NEVER return raw DB entities. Always use DTOs. Never leak hashes, secrets, or internal states.
4. **Destructive Safety**: Double-check predicates in `ExecuteDeleteAsync/ExecuteUpdateAsync`. Missing `UserId` filters wipe tables!
5. **Secrets & DOS**: NEVER hardcode production secrets/connection strings. Enforce pagination/limits to prevent resource exhaustion.
6. **Configuration**: Keep only shared, non-sensitive defaults in `appsettings.json`; local-only values belong in `appsettings.Development.json`; production secrets and connections must use environment variables or a secret store.

## C# Coding Guidelines (C# 12+)
1. **General**: File-scoped namespaces. **`using` directives MUST be placed BEFORE the `namespace` declaration**. Exactly 1 empty line after `using` directives, after `namespace`, between methods, between fields/methods, and at EOF.
2. **Fields/Blocks**: Group related fields (NO empty lines between them). Use empty lines between execution stages (e.g. validation -> save -> return).
3. **Syntax Constraints**: Use modern syntax (switch expressions). NO ternary operators (`? :`); use `if/else` or `switch`. Do not declare `out var` or pattern variables inside `if` conditions; assign them before the condition. NO spread collections `[..]` for fluent methods (prefer `.ToList()`). Regular collections `[]` allowed.
4. **Clean Code**: Avoid unnecessary braces for single-line statements. Comments must be minimal, in English, and only for highly complex code.
5. **Entity Framework**: The DbContext is globally configured with `QueryTrackingBehavior.NoTracking`. All read operations are untracked by default. DO NOT add `.AsNoTracking()` explicitly. When updating entities using `SaveChangesAsync()`, you MUST explicitly call `_dbSet.Update(entity)` beforehand. Prefer `ExecuteUpdateAsync/ExecuteDeleteAsync` where possible.

## API Routing

- Use camelCase for multi-word route segments, for example `/profitChart`; do not use kebab-case route segments.

## Method Naming

- `Find...Async`: nullable result when absence is expected.
- `Get...Async`: required result; services throw `NotFoundException` only when required by business rules.
- `List...Async`: unpaged collection.
- `Get...PageAsync`: paged result.
- Overload only identical operations with the same result; extra parameters may only refine the query. Different operations require distinct names.
- Repositories access/persist data; services apply business rules.
- Prefix direct bulk operations with `Execute`: `ExecuteUpdate...Async`, `ExecuteDelete...Async`.
- Parameter order: `userId`, primary ID, related IDs, required parameters, optional parameters, filters, paging, sorting, `CancellationToken`.
