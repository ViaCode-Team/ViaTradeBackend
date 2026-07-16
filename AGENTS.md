# AI AGENT INSTRUCTIONS (AGENTS.md)
> **FOR AI AGENTS ONLY**: This file contains critical project-specific constraints, workflows, and formatting rules. All AI agents MUST read and strictly adhere to these instructions when working in this workspace. 

## Meta-Rules for AGENTS.md
- **Language**: Must be written entirely in clear, high-quality, and easy-to-understand English.
- **Conciseness**: Keep instructions brief, elegant, and directly actionable.
- **Length**: Must not exceed 100 lines.

## Agent Behavior & Style
1. **Truth & Verification**: Never guess facts, file contents, or tool outcomes. Always read the file to verify its state. Verify your changes actually work before claiming success. If you are unsure or if a verification step fails, explicitly state it instead of assuming it worked.
2. **Security**: NEVER hardcode API keys, passwords, connection strings, or any sensitive data in the code.
3. **Manual User Changes**: If the user has manually changed something, work within that new context. Do NOT revert or change user-authored code without explicitly asking for permission first, unless the user explicitly requested you to change it.
4. **Coding Style**: Strictly follow the established coding style of the project. Always write high-quality, modern, and maintainable code.
5. **Comments**: Keep comments to an absolute minimum. Only write comments if the code is highly complex or unclear (e.g., legacy code that isn't being refactored right now). If a comment is absolutely necessary, it MUST be written in English.
6. **Proactive Rule Updates**: If the user repeatedly asks for a specific behavior, formatting, or instruction, you MUST proactively propose adding it to `AGENTS.md` to avoid repetition.

## C# Coding Guidelines
1. **Namespaces**: Use file-scoped namespaces (`namespace MyNamespace;` without braces).
2. **Spacing - General**:
   - Exactly **1 empty line** after `namespace`.
   - Exactly **1 empty line** after `using` directives.
   - Exactly **1 empty line** at the end of the file (EOF).
3. **Spacing - Class Members**:
   - Exactly **1 empty line** between methods.
   - Exactly **1 empty line** between the fields block and the methods block.
   - **Fields**: Group related fields together WITHOUT empty lines between them. Only insert an empty line between fields if they belong to distinctly different logical contexts.
4. **Spacing - Logical Blocks**:
   - Insert an empty line between key actions in the code if they belong to different stages of execution (e.g., validation -> [empty line] -> database save -> [empty line] -> return result).
5. **Syntax (.NET 10 / C# 12+)**:
   - Use modern syntax (e.g., pattern matching, switch expressions) when it improves readability.
   - **EXCEPTION**: Do NOT use the spread collection expression `[..]` as a replacement for fluent methods. Always prefer explicit methods like `.ToList()` or `.ToArray()` over `[..collection]`. Regular collection expressions (e.g., `[]` or `[1, 2]`) are fully permitted.
   - **Ternary Operators Prohibition**: Do NOT use the ternary conditional operator (`? :`). Replace all ternary expressions with `if/else`, `switch` expressions, or other constructs. Null-coalescing operators (`??`, `??=`) and null-conditional operators (`?.`) are fully permitted.
6. **Braces**:
   - Avoid unnecessary curly braces for simple, single-line actions (e.g., a simple `if` statement with an early `return`).

## Mandatory Agent Workflow
1. **Build Verification**: After making ANY code changes, you MUST run `dotnet build` to verify the project compiles without errors.
2. **Error Resolution**: If the build fails, you MUST fix all compilation errors and rebuild until the build succeeds.
3. **Warning Resolution**: Whenever possible, fix compiler warnings as well.
