# Agent guidance

This repository follows the owner's general ASP.NET Core standards. Read [docs/architecture.md](docs/architecture.md) and [docs/coding-guidance.md](docs/coding-guidance.md) before making application changes. Follow the owner's explicit direction and existing repository conventions when they differ from these defaults.

## Working agreement

- Inspect the current checkout before proposing an architecture or changing files. Do not infer a desired product design from a starter template or prior experiments.
- Keep changes focused and preserve behavior unless the task calls for a behavior change. Explain material assumptions and trade-offs.
- Prefer maintainable, readable, testable, secure code. Do not add layers, packages, or infrastructure without a concrete need.
- Target the repository's configured .NET version. Use nullable reference types and the existing namespace convention; default new shared code to `DotNetNinja` when no more specific namespace exists.
- Inject services through constructors. Use `ILogger<T>` for application logging. Use `async`/`await` for I/O and propagate cancellation where practical.
- Keep secrets out of source control. Validate untrusted input, authorize access to protected resources, and use parameterized database access.
- Add focused xUnit tests for new behavior with meaningful edge cases. Run the relevant build and tests; report any verification you could not perform.
- For database changes, state schema assumptions, use reviewed migrations, and describe operational impact and rollback.
- Do not create commits, publish releases, provision services, or modify production data unless requested.

## Repository state

The working tree may differ from the last commit. Treat uncommitted changes and deletions as intentional until the owner says otherwise. Do not restore files merely because Git reports them missing.
