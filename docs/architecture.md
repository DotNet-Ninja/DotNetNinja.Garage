# ASP.NET Core architecture guidance

This is the owner's general architectural baseline for ASP.NET Core work. It does not prescribe Garage features, a database, authentication provider, hosting platform, or a fixed number of projects. Record those decisions when the product requirements are known.

## Design principles

- Start with a cohesive ASP.NET Core application. Split into more projects or deployable services only when ownership, dependency boundaries, or operational needs justify the cost.
- Organize code around business capabilities as they emerge. Keep related endpoints/controllers, contracts, validation, services, and tests easy to find.
- Keep HTTP concerns at the boundary. Controllers or endpoints translate requests and responses, enforce authorization, and delegate behavior. They should not contain substantial business rules or persistence logic.
- Put business behavior in explicit, testable services or domain types. Keep infrastructure details behind dependencies when that improves testability or permits a real alternative implementation; avoid interfaces that merely mirror one class.
- Dependencies should point toward the behavior they support. Avoid static service access, hidden global state, and circular project references.
- Use the framework's dependency injection and configuration systems. Constructor-inject service dependencies; bind and validate configuration at startup when invalid values would make the application unusable.

## Request and data flow

1. Validate and normalize incoming data at the HTTP boundary. Return clear, appropriate status codes or validation messages.
2. Authorize the operation against the authenticated principal and the specific resource, when applicable. Never trust a client-supplied owner identifier as proof of access.
3. Call application behavior through an explicit service or a small endpoint handler. Use asynchronous APIs for I/O and pass `CancellationToken` through the call chain.
4. Persist changes atomically where an operation spans multiple writes. Use concurrency control when simultaneous edits matter.
5. Return purpose-built response models. Avoid exposing persistence entities or secrets accidentally.

## Persistence and external systems

- Choose storage from requirements, then document the choice and data ownership. Keep queries bounded and project only the data needed.
- Use parameterized queries or an ORM's safe query API. Review migrations for data loss, locking, indexing, and rollback; do not apply them to production as an incidental step.
- Integrate remote services through injected clients with explicit timeouts and cancellation. Handle failures at the boundary without hiding diagnostic context in logs.
- Treat external URLs and content as untrusted. Limit outbound destinations, redirects, response size, and processing time when the server fetches user-supplied URLs.

## Security and operations

- Use HTTPS outside local development. Store secrets in a secret store or environment configuration, never in committed settings.
- Apply authentication and authorization consistently; test cross-user or cross-tenant access where relevant.
- Log through `ILogger<T>` with structured properties. Do not log credentials, tokens, or sensitive payloads. Give requests enough context to diagnose failures.
- Expose health checks and observability that match the deployed dependencies. Keep development-only tooling out of production.
- Make configuration and deployment reproducible. Document required settings, migrations, startup commands, and rollback steps when introducing a new dependency.

## Architecture decisions

For a material choice, add a short decision note under `docs/` describing the problem, chosen approach, alternatives considered, and consequences. Do this for decisions such as persistence, authentication, background processing, hosting, or a new project boundary. Do not present an undecided option as an established architecture.
