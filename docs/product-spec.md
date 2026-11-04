# Garage product specification

Status: proposed v1 scope, October 31, 2026. This document defines the next product increment after the public home, About, and error pages. It is a build target, not a claim that these features already exist.

## Product goal

Garage is a private, browser-based start page. A signed-in person organizes useful destinations into **pages**, **groups**, and **links**, then opens them quickly. The first release is online-only. Each person's data is private to that Auth0 identity.

## Confirmed scope decisions

- The core hierarchy is page → group → link.
- Auth0 provides sign-in; Garage keeps each signed-in person's data separate.
- V1 requires a live connection. Offline editing, queued writes, and device synchronization are out of scope.
- No import of existing bookmarks or other legacy data is required.
- Keep the current ASP.NET Core MVC application as the delivery surface. The public home, About, and error pages remain.

## User experience

### Public and account flow

- A visitor can see the public home and About pages without signing in.
- The home page's login call to action starts the Auth0 sign-in flow. A successful return opens the signed-in Garage. Sign-out ends the application session and returns to the public home page.
- If sign-in fails, show a useful message and a retry path. Do not expose tokens, internal exception details, or private data in the page.
- An unauthenticated request for Garage content starts sign-in or returns an appropriate unauthorized response. A user must never see another user's content.

### First run and navigation

- A new account starts with the existing sample-filled **Start** page. This is starter content created by Garage, not an import of the user's previous bookmarks.
- The signed-in view shows the current page, its groups, and the links in each group. A page switcher lets the user move among their pages.
- Remember the user's selected/default page on the server so it is consistent across browsers. If that page is removed, choose another available page; create a new **Start** page if none remain.
- Opening a link navigates to its saved HTTP(S) URL. The user can choose whether that link opens in a new tab; new-tab links use `rel="noopener noreferrer"`.
- The existing dark racing theme carries into the authenticated shell. The page remains usable at desktop and phone widths, with keyboard navigation and visible focus states.

### Editing

The detailed, current edit-mode proposal is in [edit-flow.md](edit-flow.md). It records the chosen drag-and-drop behavior and the differences between this original v1 outline and the current code.

- Users can create, rename, reorder, and delete pages.
- Within a page, users can create, rename, reorder, and delete groups.
- Within a group, users can create, edit, reorder, move to another group on the same page, and delete links.
- Link fields: title, URL, optional description, and open-in-new-tab preference. A title and valid absolute HTTP(S) URL are required.
- Reordering persists after refresh and across signed-in browsers. Provide keyboard-operable controls even if drag-and-drop is added.
- Deleting a page or group clearly warns that its children will also be deleted. V1 deletion is permanent after confirmation; a Trash/recovery feature is a separate future decision.
- Canceling an edit makes no change. Server validation errors keep the entered values and identify the field that needs correction.

### Finding links

- Search matches link titles, URLs, and descriptions across the signed-in user's pages. Results show the containing page and group.
- Search must not return deleted links or another user's data. An empty query returns the normal page view.
- A user can always enter a title manually. Automatic page-title, favicon, and metadata lookup are not required for v1.

## Data rules

- Each page, group, and link has a stable identifier, an owner identity derived from the validated Auth0 principal, and an explicit order among siblings.
- Page names and group names are required; duplicate names may be allowed within one owner/page respectively, but the UI must distinguish them by placement. Link titles are required.
- Normalize surrounding whitespace on text fields. Reject unsafe or unsupported URL schemes. Set reasonable maximum lengths and enforce them both in forms and persistence.
- All reads and writes are owner-scoped. A guessed or altered identifier for another owner's record must not disclose or mutate it.
- A mutation that changes a hierarchy or sibling order is atomic. Concurrent edits must not silently corrupt ordering or parent relationships. The initial policy is to reject a stale update and ask the user to reload; do not claim offline-style conflict resolution.

## Quality and operations

- Use server-side authorization on every private route and anti-forgery validation on state-changing browser forms.
- Keep Auth0 credentials and the database connection string out of source control. Use separate development and production configuration.
- Use HTTPS in deployed environments and secure cookie settings appropriate to the environment.
- Provide actionable validation and failure states, while logging diagnostic details through `ILogger<T>` without secrets or tokens.
- Add tests for owner isolation, validation, hierarchy changes, ordering, authentication redirects, and the first-run page.
- Document local setup, migrations, required configuration, and deployment steps before the feature is considered complete.

## Acceptance criteria

1. A new Auth0 user signs in and sees the sample-filled **Start** page.
2. That user can create two pages, multiple groups, and links; edit and reorder them; refresh; and see the same result.
3. A link opens the saved URL with the configured tab behavior. Invalid or non-HTTP(S) URLs are rejected with a field message.
4. Search finds a link on another page and identifies its page and group.
5. Deleting a group or page requires an explicit confirmation and removes its descendants together.
6. User B cannot list, view, edit, reorder, or delete User A's records, including by changing route or form identifiers.
7. Signed-out users cannot access private pages; sign-out returns to the public home page.
8. Desktop and phone layouts, keyboard editing controls, and visible focus states work in the supported browsers.
9. A normal build and the relevant tests pass; the About page continues to show the stamped version.

## Deferred work

Offline use and synchronization, legacy import, sharing/collaboration, automatic metadata or favicon lookup, Trash and restore, browser extension integration, public pages, and native mobile apps are outside this first increment.

## Decisions to revisit before implementation

- Exact product copy and signed-in navigation labels.
- Whether link descriptions and new-tab preference should be exposed in the first editor or added after the core create/edit flow.
- Final maximum lengths, supported browsers, and deployment target.
- Whether to add Trash/recovery before real data is entrusted to the app. Permanent deletion requires a clear confirmation in the meantime.
