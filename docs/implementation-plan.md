# Garage v1 implementation plan

This plan implements [product-spec.md](product-spec.md) in the existing ASP.NET Core 10 MVC application. It is a proposed sequence. Keep [architecture.md](architecture.md) and [coding-guidance.md](coding-guidance.md) as the general engineering rules.

## Application shape

- Keep one MVC web project and the existing xUnit test project. Add folders by feature as the code grows; do not add separate services or projects without a concrete boundary.
- Use Auth0's ASP.NET Core web-app integration for the browser login flow and a server-side authentication cookie. The browser should not handle API access tokens for this MVC experience.
- The project already references EF Core SQL Server. Use SQL Server as the proposed v1 store, with EF Core migrations reviewed before application. Confirm the local and production SQL instances during setup.
- Controllers handle HTTP, authorization, and view models. Inject application services for hierarchy mutations and queries; keep EF entities out of Razor form models.

## Proposed data model

| Entity | Essential fields | Rules |
| --- | --- | --- |
| `GarageUser` | Auth0 subject, selected page ID, created timestamp | Subject is unique; created on first successful sign-in. |
| `GaragePage` | ID, owner subject, name, sort order, concurrency token | Owner can have many pages; at least one page after first run. |
| `GarageGroup` | ID, owner subject, page ID, name, sort order, concurrency token | Parent page must have the same owner. |
| `GarageLink` | ID, owner subject, group ID, title, URL, description, open-in-new-tab, sort order, concurrency token | Parent group must have the same owner; URL is HTTP(S). |

Use stable GUIDs or another stable key type consistently. Add indexes for owner plus parent plus sort order. The owner column on children supports explicit owner-scoped queries; constraints and application logic must prevent cross-owner parent references. Define cascade behavior deliberately so deleting a page or group removes its descendants in one transaction. Use row-version or equivalent optimistic concurrency for edits and ordering operations.

## Build slices

### 1. Authentication and private shell (implemented; Auth0 credentials still required)

- Auth0 settings and cookie authentication are wired in startup. Login, POST-based logout, callback handling through the integration, a protected Garage route, and signed-in navigation are present.
- Local and production Auth0 registrations and credentials still need to be configured. Once available, verify redirects, failed login handling, logout, and that no private data is rendered after sign-out.

### 2. Persistence and first-run account

- Add the EF Core context, entities, configuration, initial migration, and owner-scoped repository/query code.
- On first successful sign-in, atomically create the user record and empty **My Garage** page. Do not create duplicate starter pages on later sign-ins.
- Document connection strings, local SQL setup, migration commands, and backup/rollback steps.

### 3. Read-only Garage view

- Render pages, groups, and links for the signed-in owner. Add page switching and server-persisted selected page.
- Add empty states for a new page, an empty group, and no search results. Confirm desktop and mobile layout and keyboard navigation.

### 4. Editing and ordering

- Add validated create/edit forms for pages, groups, and links. Use anti-forgery tokens and Post/Redirect/Get after successful writes.
- Add delete confirmations and transactional descendant deletion. Add move and reorder controls with stable, deterministic order values.
- Reject stale edits with a clear reload message. Test owner isolation on every mutation and parent relationship.

### 5. Search and release checks

- Add owner-scoped search across link title, URL, and description, returning page/group context. Bound result counts.
- Add integration tests for authentication and persistence boundaries and a manual browser checklist from the product acceptance criteria.
- Update the README with setup and deployment steps. Verify version stamping and the About page in a production build.

## Implementation gates

- Do not call a slice complete until its key success path, authorization boundary, and meaningful failure case have been exercised.
- Keep secrets in user secrets or environment settings; do not commit real Auth0 or SQL values.
- Review generated SQL migrations and verify data preservation before applying them outside local development.
- Keep the public pages working throughout the build.
