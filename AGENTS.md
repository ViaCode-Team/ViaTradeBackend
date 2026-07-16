# AI AGENT INSTRUCTIONS (AGENTS.md)
> **FOR AI AGENTS ONLY**: Critical rules. Read and strictly adhere.

## Meta-Rules for AGENTS.md
- **Language**: Must be written entirely in clear, high-quality English.
- **Conciseness**: Keep instructions brief, elegant, and directly actionable.
- **Length**: Must not exceed 100 lines. If this file exceeds 6000 characters (checked when editing), ask the user if they want to compress it without losing any meaning or details.

## Agent Behavior & Workflow
1. **Strict Adherence**: Perceive user prompts exactly as written. DO NOT invent tasks, features, or behaviors that the user did not explicitly request. If a prompt is ambiguous or you are unsure how to proceed, you MUST ALWAYS ask the user for clarification before making any changes.
2. **Verify & Thoroughness**: Never guess facts/outcomes. Read files to verify states. Double-check directly/indirectly affected code. Ensure features work before claiming success. NEVER leave tasks half-finished. ALWAYS run `grep_search` to find dangling references for removed code. Clean up unused variables immediately.
3. **User Edits**: Respect manual user code changes. Ask before reverting user-authored code.
4. **Proactive Rules**: Propose `AGENTS.md` updates if users repeatedly ask for specific behaviors.

## Mandatory Agent Workflow
1. **Build Verification**: After making ANY code changes, you MUST run `dotnet build` to verify the project compiles without errors.
2. **Error Resolution**: If the build fails, you MUST fix all compilation errors and rebuild until the build succeeds.
3. **Warning Resolution**: Whenever possible, fix compiler warnings as well.

## Strict Security (CRITICAL)
1. **Prevent IDOR**: NEVER query/update/delete by ID alone. ALL DB actions MUST verify ownership (`&& e.UserId == currentUserId`).
2. **Secure by Default**: Do NOT remove global auth policies or add `[AllowAnonymous]` without explicit permission.
3. **Data Leaks (DTOs)**: NEVER return raw DB entities. Always use DTOs. Never leak hashes, secrets, or internal states.
4. **Destructive Safety**: Double-check predicates in `ExecuteDeleteAsync/ExecuteUpdateAsync`. Missing `UserId` filters wipe tables!
5. **Secrets & DOS**: NEVER hardcode secrets/connection strings. Enforce pagination/limits to prevent resource exhaustion.

## C# Coding Guidelines (C# 12+)
1. **General**: File-scoped namespaces. Exactly 1 empty line after `namespace`, `using` directives, between methods, between fields/methods, and at EOF.
2. **Fields/Blocks**: Group related fields (NO empty lines between them). Use empty lines between execution stages (e.g. validation -> save -> return).
3. **Syntax Constraints**: Use modern syntax (switch expressions). NO ternary operators (`? :`); use `if/else` or `switch`. NO spread collections `[..]` for fluent methods (prefer `.ToList()`). Regular collections `[]` allowed.
4. **Clean Code**: Avoid unnecessary braces for single-line statements. Comments must be minimal, in English, and only for highly complex code.
