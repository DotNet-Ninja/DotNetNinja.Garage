# ASP.NET Core coding guidance

These are the owner's general defaults reflected in Ada's guidance. Existing repository conventions and explicit task requirements take precedence.

## C# and .NET

- Use the .NET version configured by the repository. Keep nullable reference types enabled, model nullability honestly, and avoid null-forgiving operators except where an invariant is established.
- Use intention-revealing names and small methods with a clear responsibility. Prefer straightforward code to abstractions that only reduce line count.
- Default shared namespaces to `DotNetNinja`; use a more specific namespace matching the project or feature when appropriate.
- Use constructor injection for services. Avoid service locators and direct construction of collaborators that need configuration, logging, or replacement in tests.
- Use `ILogger<T>` and structured messages for application logs. Do not use `Console.WriteLine` for runtime diagnostics.
- Use `async`/`await` for I/O. Do not block on tasks with `.Result` or `.Wait()`. Accept and pass a `CancellationToken` for operations that can be canceled.
- Catch exceptions only to recover, translate a known failure, or add useful context. Do not swallow exceptions or turn all failures into the same message.
- Use comments to explain non-obvious intent or constraints, not to restate the code.

## ASP.NET Core boundaries

- Keep actions and endpoint handlers thin. Validate inputs, authorize, invoke behavior, and map the outcome to HTTP.
- Use dependency injection and options for configuration. Validate required settings early. Keep development values separate from production secrets.
- Use explicit request and response models for public contracts. Avoid binding persistence entities directly from requests.
- Use framework validation and consistent error responses. Do not leak internal exceptions or stack traces to clients.
- Make authorization resource-aware where data belongs to a user, account, or tenant.

## Data and SQL

- State schema and transaction assumptions in changes that affect persistence.
- Parameterize SQL. Bound reads, select only necessary columns, and add indexes for observed query patterns rather than by habit.
- Write migrations that preserve existing data unless removal is explicitly required. Review generated migrations and document how to apply and roll back operational changes.

## Tests and verification

- Use xUnit for C# tests. Test observable behavior, including a meaningful failure or edge case for nontrivial logic.
- Keep tests deterministic. Inject clocks, clients, and other nondeterministic dependencies when the behavior depends on them.
- Prefer focused unit tests for business rules and integration tests for framework wiring, persistence, authorization, and external boundaries.
- Run the smallest relevant build and test commands after a change. Broaden testing when a concrete risk or repository gate requires it. Report what ran and any limitation.

## Review and delivery

- Preserve existing behavior during refactoring. Call out intentional behavior changes.
- Keep package updates deliberate and verify compile and runtime integration after version changes.
- For operational instructions, provide reproducible commands, expected outcomes, and a revert path when practical.
- Document material design decisions and configuration changes alongside the code they affect.
