# Garage edit flow

Status: implemented, pending application of the `AddEditConcurrency` migration and browser verification. The owner chose an **Edit mode on the current page**, **editable page slugs**, and **drag and drop for reordering links and moving them between groups on the same page**. The current sample-filled **Start** page remains the first-run experience.

## Current baseline

- Signed-in users open a page at `/{slug}`. `Pages/Index` reads owner-scoped data through `IPageManager` and displays groups and links.
- The current entities are `LinkPage`, `LinkGroup`, and `LinkItem`, with integer IDs and `SortIndex` values. A page has `Text`, `Slug`, and `IsDefault`; a group has `Text`; a link has `Text`, `Href`, `Icon`, `IconColor`, and `OpenInNewTab`.
- The starter page is named **Start** and contains sample groups and links. This first-run experience is confirmed.
- The `AddEditConcurrency` migration adds revision tokens for edit conflicts. Link descriptions are not stored and are outside this edit flow.

## Entering and leaving Edit mode

1. A signed-in user opens a page and selects **Edit page** near the page title. The page stays in place; its groups and links gain editing controls. The URL can use `?edit=1` so refresh preserves the mode.
2. The page navigation remains available. Switching pages in Edit mode opens the selected page in Edit mode.
3. **Done** leaves Edit mode. Add and edit forms open in Bootstrap dialogs. There is no page-wide Save button: each form saves one change, while a completed drag saves the new order immediately.
4. **Cancel** on a form discards only that form's unsaved values. Successful posts redirect back to the relevant page in Edit mode and show a brief status message. Validation errors keep the entered values and identify the field to fix.
5. The read view keeps the existing racing background and glass group cards. Edit controls must be visible and usable without turning the cards into a dense toolbar.

## Page operations

| Action | Edit-mode experience | Result |
| --- | --- | --- |
| Add page | **Add page** near the page navigation opens a short form for the page name. | Create an empty page at the end of the page order, generate a unique URL slug, and open `/{slug}?edit=1`. |
| Rename page | **Page settings** opens a form with the displayed name. | Update `Text` and keep the existing slug so saved URLs continue working. |
| Change slug | An explicit slug field in Page settings, separate from the displayed name. Explain that changing it can break the user's saved bookmarks. | Validate uniqueness for this user and reserve existing application routes; redirect to the new `/{slug}?edit=1`. |
| Set default | **Make default** in Page settings. | Clear the user's previous default and set this page as the only default in one transaction. |
| Reorder pages | Move-left and move-right controls in the page navigation. | Persist `SortIndex`; keep the current page selected. |
| Delete page | A confirmation names the page and states how many groups and links will be removed. | Delete the page and descendants. Redirect to the user's remaining default page, or select another remaining page. Do not offer deletion of the last page. |

Page slugs are URL path segments. Generated slugs use lower-case letters, numbers, and hyphens. Duplicate names may exist, but slugs must be unique within one user's pages. A slug that collides with an existing controller route or static asset path must be rejected.

## Group operations

- **Add group** appears after the group's grid. The form requires a name and creates the group at the end of the current page's order.
- Each group has a compact menu for **Rename**, **Move earlier**, **Move later**, and **Delete**. Move controls persist `SortIndex` and are keyboard operable.
- Deleting a group requires confirmation that includes the number of links it contains. Its links are deleted with it; cancellation leaves everything unchanged.
- Empty groups remain visible in Edit mode with an **Add link** action.

## Link operations

- **Add link** appears in every group. **Edit** and **Delete** are available from each link's edit controls. The form includes title, absolute HTTP(S) URL, icon, icon color, and the open-in-new-tab setting. The initial editor does not include a description because the entity does not store one.
- The icon picker uses the existing `Icons` values and shows a preview. The color picker stores a six-digit hex color such as `#127CD6`; a readable default is supplied when none is chosen.
- New links append to their chosen group. Editing a link preserves its position unless the user also moves it. Deletion removes it from the group and closes the gap in sibling order.
- The whole link row is clickable in read mode. In Edit mode, a visible drag handle starts a move, while a separate open-link affordance remains available so dragging never accidentally opens a destination.

### Drag and drop

- A user can drag a link within its current group to reorder it or drop it into another group on the same page. The destination group and insertion point are visibly highlighted. Empty groups are valid drop targets.
- Dropping saves immediately. The UI keeps the moved item in its new position only after the server accepts the change; while the request runs it shows a saving state and prevents a second move of the same item.
- If the save fails or the submitted order is stale, restore the last server-confirmed order and show an actionable message. Do not report success until the server confirms it.
- Keyboard users can focus the drag handle, move the link earlier or later, choose a destination group, and confirm the move. Provide equivalent controls in a menu if the chosen drag library cannot support this directly. Announce the result to assistive technology.
- Moving links between pages is outside this edit-flow scope. The server rejects a destination group that is not on the current page.
- Group and page reordering initially use explicit controls. Dragging groups or page tabs is a later enhancement.

## Save contracts and data rules

- All edit endpoints require authentication, an anti-forgery token, and the owner ID from the validated principal. IDs and slugs from the browser identify records but never authorize access. A foreign or missing record returns the same not-found result.
- Use form models rather than binding EF entities. Trim names and URLs; enforce the current column limits: page/group/link text 64 characters, slug 128, URL 256, icon color 16. Reject blank names, non-HTTP(S) links, unknown icons, invalid hex colors, duplicate slugs, and reserved slugs before writing.
- A drag save sends the moved link ID, destination group ID, intended position, and a version of the source and destination order. The server reloads both groups under the current owner, checks the versions, moves the link, and rewrites affected `SortIndex` values in one transaction. Never trust an entire client-supplied ordering list without validating membership and duplicates.
- Add an optimistic concurrency mechanism before implementing mutations. A row version or explicit revision on each ordered collection must let the server reject stale moves and stale edits. Apply uniqueness constraints and indexes needed for owner-plus-slug and owner-scoped ordering in a reviewed migration.
- A write succeeds completely or leaves the previous hierarchy and order intact. Normalize sibling order to deterministic values after add, delete, or move. The display query sorts by `SortIndex` and then ID for a stable tie-breaker.
- Changing a page slug must preserve the current user's ownership, update only that page, and redirect to the new URL. The old slug does not automatically become an alias.
- Handle expected validation and concurrency failures in the UI. Log unexpected database errors without exposing connection strings, tokens, or other users' data.

## Build sequence

1. Add persistence rules for unique user slugs, safe ordering, and concurrency; review the migration and rollback impact. Add owner-scoped mutation methods to a service rather than putting EF writes in controllers.
2. Add server-rendered Edit mode, page/group/link forms, validation, delete confirmations, and Post/Redirect/Get. Ensure all actions work without drag and drop.
3. Add drag and drop for link rows within and across groups on the current page. Save each completed move with the atomic move contract and add the keyboard equivalent.
4. Verify on desktop and phone widths, with mouse, touch, keyboard, and screen reader status announcements. Test stale updates, owner isolation, and failed saves.

## Acceptance checks

1. A signed-in user can enter Edit mode from `/{slug}`, leave it with Done, and refresh without losing a completed change.
2. They can create and rename pages, groups, and links, then see the result in the read view and after a fresh sign-in.
3. They can drag a link above or below another link and into a different group, including an empty group; the order survives refresh.
4. The same link moves can be completed from the keyboard without dragging.
5. A failed or stale move restores the last saved order and gives a clear retry path.
6. Invalid URLs, colors, slugs, and overlong labels show field errors without losing the form input.
7. Deleting a page or group requires confirmation and removes its descendants only after confirmation. The last page cannot be deleted.
8. A user cannot alter another user's page, group, link, default choice, or order by changing posted IDs, slugs, or positions.
9. Signed-out edit requests challenge for login; all state-changing requests reject missing or invalid anti-forgery tokens.

