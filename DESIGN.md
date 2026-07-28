# Profile Domain Design

This document is the finalized design of the Profile domain model, covering all
domains (Account, Site, Social, Content, Interaction, Output).
`BUSINESS.md` defines product-level business rules, `ARCHITECTURE.md` defines
the technical architecture; this document grounds both into concrete aggregates,
value objects, and invariants. Each decision includes its rationale.

Document conventions:

- All runtime text (logs, exceptions, API errors) uses English. This document
  only describes the design.
- All length limits are in characters; all quantity limits are in items.

## 1. General Modeling Principles

- The domain model is independent of the database entity model. The
  infrastructure layer is responsible for mapping.
  Rationale: `ARCHITECTURE.md` requires Domain to be unaffected by persistence
  shape.
- `Blog`, `Post`, `Moment` are three separate aggregates and must not be merged
  into a generic `ContentItem`; `Page` is a fourth separate aggregate. The four
  do not share a mutable content base class, only small value objects with fully
  identical semantics.
  Rationale: The three (and Page) evolve in different directions; merging forces
  aggregates to carry fields irrelevant to each other; a shared base class leaks
  changes from one side to the others.
- Each aggregate uses a strongly typed identity: `BlogIdentity`, `PostIdentity`,
  `MomentIdentity`, `PageIdentity`, `UserIdentity`, etc., all backed by a
  globally unique Guid. Cross-aggregate references use the corresponding
  strongly typed identity — for example, a Post's reference must not accept a
  `MomentIdentity`.
  Rationale: Strongly typed identities prevent cross-aggregate misreferences at
  compile time.
- Potentially unbounded interactions and histories (comments, likes, reposts,
  favorites, bookmarks, revisions) are never stored inside content aggregates;
  they are separate aggregates or separate relations. Interaction counts
  (comment count, like count, repost count, favorite count) are all read
  projections.
  Rationale: Unbounded collections would violate aggregate loading boundaries
  and consistency boundaries.
- `TimelineEntry` is only a unified read projection, not a write aggregate.
  Rationale: A timeline is a query view of multiple content types and owns no
  write invariants.
- The system internally always uses globally unique Guids for associations; no
  user-visible text identifier (routing slug, display name) enters internal
  associations.
  Rationale: The user explicitly requires that internal associations not depend
  on mutable text identifiers — renaming does not break associations.

## 2. Deployment Site

`Site` represents deployment-level behavior. Exactly one per deployment:

- `Mode`: `Personal` / `Community`. Both modes share the same multi-user schema;
  mode switching is non-destructive.
- Registration policy: `Disabled` / `Invitation` / `Open`, modeled independently
  of `Mode`.
- Owner: the site owner account reference used in Personal mode.
- In Personal mode, which `AccountSite` is exposed at the deployment root path
  is configurable and not fixed to the owner's site.
  Rationale: This was an open item in TEMP; the user decided to retain
  configuration flexibility.

Custom domains are currently explicitly excluded from the design scope, but only
as a deferral, not a permanent exclusion; domain ownership and verification
rules will be designed when they are introduced in the future.

## 3. Account and Basic Identity

The following rules are consistent with `BUSINESS.md` and are finalized here as
domain structures:

- The `Account` aggregate contains: an immutable `UserIdentity` (backed by
  Guid), a mutable `StringIdentity`, one current `AccountEmail` (with
  verification metadata), the current `AccountRole`, the current
  `AccountSuspension?`, the current `AccountBan?`, and `AccountDeletion?`.
- Display information such as nickname, avatar, and bio does not belong to
  `Account`; see §4.
- `StringIdentity` rules: 5–64 ASCII characters; character set
  `A-Z a-z 0-9 _ .`; must not start or end with `.`; consecutive `.` is
  forbidden; `_` may be consecutive and may appear at either end; the chosen
  casing is preserved for display, but availability checks, uniqueness, and
  login lookups are case-insensitive; after a change, only the new value can be
  used for login, and the old value is reserved for a configurable grace period
  (default 90 days).
- Login input parsing happens only at the HTTP boundary: `#<guid>` selects
  UserId, `@<stringId>` selects StringId, no prefix defaults to StringId; `@`
  and `#` are pure input prefixes and never appear in domain values, DTOs,
  persistence, or any internal representation.
- Email: one current address per account, changeable; not a login identifier;
  not globally unique; comparison and normalization are case-insensitive but the
  original casing is preserved; verification information for the current address
  is stored.
- Roles: `User < Administrator < Root`; a higher role can only manage accounts
  strictly lower than its own; Roots must not manage each other;
  `Profile.Console` is a trusted management surface and can perform cross-role
  operations on Root. Restriction history and operator identity do not belong to
  the Account aggregate and are recorded by a separate audit module.
- Suspension: may expire or be permanent (`null` means permanent); optional
  reason; login is allowed; content remains visible; all state-mutating
  operations are blocked.
- Ban: may expire or be permanent; optional reason; login is forbidden; all of
  that account's content is hidden.
- Account deletion: configurable recovery period (default 14 days); when
  deletion is requested, the recovery deadline is fixed; during the recovery
  period, content remains visible; the account can log in but can only perform
  recovery; after expiration, permanent deletion may proceed; permanent deletion
  still permanently retains the UserId, StringId identity record, and email
  identity record.
  Rationale: Identity records are never reclaimed, preventing old identities
  from being reoccupied by others.
- Restriction stacking: an active Ban takes priority over deletion recovery
  (cannot log in, therefore cannot self-recover; content not visible); when
  Suspension and deletion coexist, the account can log in but can only recover,
  and content is visible.

Authentication uses FIDO/WebAuthn; in cluster deployments, challenge storage and
Data Protection keys must be shared; emails are sent via MailKit through queued
background tasks.

## 4. Account Display, Preferences, and Account Site

Each account has exactly one of each of the following objects:

### 4.1 AccountProfile (Public Identity Display)

Fields: Nickname (≤64), Avatar (media library reference), Bio (≤2048),
Location (≤128), Personal Link (a single URL), Banner/Background (media library
reference).

Rationale: Public display information is separated from the basic account
structure; `BUSINESS.md` requires that nickname, avatar, and bio do not belong
to the basic account model.

### 4.2 AccountSettings (Account-Level Private Preferences)

Fields:

- Default visibility: per-content-type default `ContentVisibility` for Blog,
  Post, and Moment (system-level default is `Public` for all).
- Language preference, timezone.
- Email notification preferences: new comment/comment moderation, new follower,
  interaction (like/repost/quote), system notification — each individually
  toggleable.
- Follow requires approval toggle (see §5).

Rationale: The user explicitly specified that defaults such as visibility
preferences reside in a separate `AccountSettings`, not in `AccountSite` or
`AccountProfile`; these are private behavioral preferences, not public site
configuration.

### 4.3 AccountSite (Account Site Configuration)

Each account has exactly one `AccountSite`, using its `OwnerId` (UserIdentity)
as the stable domain identity without an additional site identity. Fields:

- Site title (≤64), site description (≤1024).
- Theme/appearance settings.
- Page size.
- Default comment moderation policy (site-level default; individual content
  items can override, see §11).
- Output toggles: enable/disable toggles for RSS/Atom, Sitemap, archive pages,
  and other outputs.

`AccountSite` does not contain nickname, avatar, bio, or other public identity
information (those belong to `AccountProfile`). Content stores only `AuthorId`
and does not duplicate account site identity.
Rationale: With one account per site, OwnerId is already unique; content and
site can be associated through the author account.

## 5. Social Relations

### 5.1 Follow

- The form of follow relation depends on account settings: each account chooses,
  via the "follow requires approval" toggle in `AccountSettings`, between direct
  follow or approved follow requests.
- The visibility audience for `Followers` / `MutualFollowers` is determined at
  query time in real time; changes to follow relations immediately affect the
  accessibility of historical content.
  Rationale: The user confirmed real-time determination, avoiding snapshot
  audiences becoming out of sync with current social relations.

### 5.2 Block

An account may block other accounts. Effects of blocking:

- Prevents following (existing follows are automatically removed);
- Prevents commenting on the other party's content;
- Prevents interactions (like, repost, quote, favorite, bookmark).

Blocking does not make content invisible: the blocked party can still see the
blocker's Public content.
Rationale: The user chose three active-behavior restrictions and did not choose
content invisibility.

## 6. Shared Value Objects

### 6.1 ContentFormat and ContentBody

```text
ContentFormat
  PlainText
  Markdown

ContentBody
  Source: string
  Format: ContentFormat
```

- `ContentBody` is an immutable value object; editing replaces the entire value.
- Format is explicitly recorded and persisted; the system never guesses format
  from content; editing may change both source and format simultaneously.
- Rendered HTML is a derived output and does not belong to the domain model.

### 6.2 ContentVisibility

```text
ContentVisibility
  Public
  Followers
  MutualFollowers
  Private
```

- Blog, Post, Moment, and Page share this enum; Followers and MutualFollowers
  are distinct audiences.
- Visibility may be changed after publication.
- Default visibility: Blog/Post/Moment all default to `Public`; an account may
  configure its own per-content-type defaults in `AccountSettings`.

### 6.3 Content Blocks

The body of a Blog or Page is composed of an ordered collection of content
blocks; Post, Moment, and comments do not use content blocks and use only a
single `ContentBody`. Block types:

- Text block: contains a `ContentBody` (Markdown or plain text); single-block
  text length ≤2097152.
- Media block: media library reference.
- Quote block.
- Code block: with language identifier.
- Divider / layout block.

Per Blog/Page block count ≤8192. Per content item tag count ≤32.
Rationale: The user decided that long-form content uses a block editor model;
the block limit is set high enough to avoid practically constraining authorship.

### 6.4 Publication Representation

Publication state machine:
`Draft → Scheduled → PendingReview → Published` (including reverse transitions
for each), with no `Archived` state.

- Scheduled publish time uses `DateTimeOffset`.
- Published items record `FirstPublishedAt` and `LastPublishedAt`; unpublishing
  retains both.
  Rationale: The user chose to retain both first and most recent publish times;
  unpublishing does not erase history.
- No Archived lifecycle state is needed; the CMS archive page is a read
  projection and does not depend on content state.
  Rationale: Recommended by TEMP and confirmed by the user; "archive page" and
  "archived state" are two distinct concepts and must not be conflated.

### 6.5 Deletion Representation

Publication state and deletion state are modeled separately. The deletion object
records:

- `DeletedAt`;
- `PurgeAt`: fixed at deletion time as `DeletedAt + 7 days` and cannot be
  changed thereafter.

Rules:

- Soft-deleted content goes to the recycle bin and is recoverable; recovery
  restores all pre-deletion relations (comments, likes, reposts, bookmarks,
  favorites, revisions, category/tag associations).
- Automatic permanent deletion after 7 days; no one — including the author or
  administrators — can perform permanent deletion before the deadline.
  Rationale: The user chose deadline-only automatic purging, making the recovery
  period absolutely effective for all roles.
- Permanent deletion cascades to delete that content's comments, likes, reposts,
  bookmarks, favorites, revisions, and media links; media files in the media
  library themselves are never automatically deleted and can only be deleted
  manually by the user.
  Rationale: Media may be referenced from multiple places, and the user
  explicitly specified no automatic media file cleanup.
- When unpublishing, the user may choose "Save as Draft" (transition to Draft)
  or "Discard" (enter soft deletion, still subject to the 7-day recovery
  period).

### 6.6 Media Reference

All media references across aggregates are uniformly references to media items
in the media library (§16); content does not embed media files. A media
reference contains the media library identity and necessary display metadata.

## 7. Blog

### 7.1 Identity and Routing

- `BlogIdentity`: globally unique Guid internal identity.
- `BlogSlug`: system-generated, immutable, numeric-only, zero-padded string;
  unique within an account (scoped by `AuthorId`); monotonically incrementing
  per account; minimum width 9 digits (`000000001`); expands to wider digits
  when the numeric space is exhausted; slug is never reused after permanent
  deletion.
  Rationale: The zero-padding requirement means the slug is modeled as a
  validated string, not an integer; never-reuse prevents old links from pointing
  to new content; allocation involves cross-aggregate uniqueness and is handled
  by a coordinator external to the Blog aggregate, which passes the slug in.
- Public routing form: `/@{stringId}/blog/{slug}`. The `@` is only a URL
  presentation-layer prefix and does not enter domain values, DTOs, or
  persistence (consistent with the §3 prefix rules).

### 7.2 Structure

```text
Blog
  Id: BlogIdentity
  AuthorId: UserIdentity
  Slug: BlogSlug
  Title: string (≤256)
  Blocks: ordered ContentBlock collection (≤8192)
  Summary: string? (≤2048)
  FeaturedMediaId: MediaReference?
  Visibility: ContentVisibility
  CategoryId: CategoryIdentity?   // optional, at most one
  TagIds: BlogTagIdentity collection (≤32)
  CommentsAllowed: bool
  CommenterPolicy: CommenterPolicy
  Pinned: bool
  Featured: bool
  SeoTitle: string? (≤128)
  SeoDescription: string? (≤512)
  CanonicalUrl: string?
  CoAuthors: CoAuthor collection (≤32)
  Publication
  Deletion?
  CreatedAt / UpdatedAt
```

- Pinned and Featured are two independent flags.
- CoAuthor is a display-only marker, in either form: a system-internal account
  reference (UserIdentity) or free text (≤64); Blog ownership and permissions
  still belong solely to `AuthorId`.
  Rationale: Site collaboration is not allowed (§7.4); co-authors are only
  display credit and do not confer permissions.
- Draft invariants: title must be non-empty; body (block collection) may be
  empty — to support auto-save.
  Rationale: The user decided only the body may be empty; a non-empty title
  ensures drafts are identifiable in lists.
- Publish invariants: on publish/submit-for-review, only the title is validated
  as non-empty; the body (block collection) may be empty.
  Rationale: The user explicitly specified that Blog publication does not
  require a non-empty body, distinguishing it from the Post/Moment rule of "at
  least body or media".

### 7.3 Revision History

- Each edit generates an immutable revision; all versions are permanently
  retained and rollback to any version is supported.
- Published content is edited in place with immediate effect, while a revision
  is also recorded.
  Rationale: The user confirmed full revision history + in-place effect;
  revisions are historical archives, not parallel versions awaiting publication.

### 7.4 Collaboration and Moderation

- Other accounts are not permitted to collaboratively manage an account site:
  the author of site content is the site owner, with no site-level
  member/editor role model.
- The publication state set includes `PendingReview`, serving as a single-author
  editorial workflow state (self-submit-for-review / pre-publish staging).

### 7.5 Batch Operations and Preview

- Batch editing and batch category/tag assignment (operating on multiple Blogs
  at once) are supported.
- Previews are not shareable: only the author, while logged in, can preview;
  no preview tokens are issued.

## 8. Page

A separate aggregate, hosting non-chronological site pages such as About,
Contact, Privacy Policy, etc.

- `PageIdentity`: globally unique Guid internal identity; routing uses a
  user-specified text identifier (mutable), e.g. `/about`.
  Rationale: Consistent with taxonomy — internal Guid association, text
  identifier only for mutable routing.
- Structure: title, ordered content block collection (same model as Blog),
  visibility, comment toggle and commenter scope, SEO title/description,
  featured media.
- Simplified lifecycle: only `Draft` / `Published` + soft deletion (same 7-day
  recycle rules as §6.5); no scheduled publishing, no PendingReview, no revision
  history.
  Rationale: Pages change infrequently; the user explicitly chose a simplified
  lifecycle.
- Per-account Page count ≤1024.

## 9. Post

Twitter-style short media posts.

```text
Post
  Id: PostIdentity
  AuthorId: UserIdentity
  Body: ContentBody?           // ≤8192
  Media: readonly MediaReference collection  // image+video+audio, ≤9
  Visibility: ContentVisibility
  QuotedPostId: PostIdentity?
  CommentsAllowed: bool
  CommenterPolicy: CommenterPolicy
  TagIds: PostTagIdentity collection (≤32)
  Publication
  Deletion?
  CreatedAt / UpdatedAt
```

- Draft invariants: body or media at least one (completely empty draft not
  allowed).
- Publish invariants: body or media at least one. Quotes are not exempt — a
  quoted post must also bring its own body or media (user decision, retracting
  bare quotes).
- A pure repost is modeled as an independent relation `PostRepost`, not as a
  special Post with copied body; self-repost is allowed; the same account may
  repost the same Post multiple times.
- A quote is a new Post: own body/media + `QuotedPostId`; Posts can only
  repost/quote Posts.
- When the original Post is deleted, hidden, or its visibility is reduced: the
  quote retains its own content and shows an "unavailable" placeholder in the
  original content position; a pure repost is fully hidden; when the original
  content is restored, quotes and reposts are automatically restored.
- Comment count, like count, repost count, and favorite count are read
  projections.
- Tags use a separate PostTag namespace (§14).

## 10. Moment

WeChat Moments-style lightweight sharing. Deliberately kept as a separate
aggregate from Post: each has its own identity, relations, comments, commands,
policies, and repositories, allowing behavior to evolve independently.

```text
Moment
  Id: MomentIdentity
  AuthorId: UserIdentity
  Body: ContentBody?           // ≤2048
  Media: readonly MediaReference collection  // image+video, ≤9
  Visibility: ContentVisibility
  AudienceExclusions: UserIdentity collection (≤2048)
  Location: structured location (coordinates + place name)?
  QuotedMomentId: MomentIdentity?
  CommentsAllowed: bool
  CommenterPolicy: CommenterPolicy
  TagIds: MomentTagIdentity collection (≤32)
  Publication
  Deletion?
  CreatedAt / UpdatedAt
```

- Draft and publish invariants are the same as Post: body or media at least one.
- Audience exclusions are an account set applied on top of the visibility enum:
  first determine the audience by visibility, then exclude the specified
  accounts from it; exclusions only subtract and never expand visible scope.
- Moments can only repost/quote Moments; Moments have no favorites and no
  bookmarks.
- Comment/like visibility rules are the same as for Post; when the original
  Moment is unavailable, repost/quote behavior is the same as for Post (quote
  retains own content + placeholder, pure repost is hidden).
- Moments exist permanently with no automatic expiration.
- Tags use a separate MomentTag namespace (§14).

## 11. Comments

- Separate comment aggregates per host:
  `BlogComment`, `PostComment`, `MomentComment`, `PageComment`.
  Rationale: Consistent with the content aggregate split decision, allowing each
  type of comment to evolve independently.
- Structure: `Id`, `AuthorId` (commenter is a system account), host's strongly
  typed identity, optional `ParentCommentId` (nesting depth unlimited),
  `Body: ContentBody` (≤4096, supports Markdown and plain text), media (images
  only, ≤4), status, CreatedAt.
  Rationale: The commenter is a system account because the "first comment per
  commenter requires moderation" policy requires stable identification of
  commenter identity.
- Capabilities: Markdown, media, likes; not editable.
- State machine: `Pending` / `Approved` / `Spam` / `Deleted`; `Deleted` is not
  recoverable and only retains a placeholder record to preserve the nesting tree
  structure.
  Rationale: Under a nesting tree of unlimited depth, hard deletion would
  destroy the context of child comments.
- Moderation policy (three choices): no moderation required / first comment per
  commenter requires moderation (once approved, subsequent comments by that
  commenter appear directly) / all comments require moderation. Configuration
  hierarchy: site-level default (`AccountSite`) + content item override.
- Each content item can close comments (`CommentsAllowed=false` means no one
  can comment, including the author); each content item can set a commenter
  scope (`CommenterPolicy`): all who can read / followers only / mutual
  followers only / author only; default "all who can read".
  Rationale: The user chose more restrictive comment control than read
  permissions; the two levels of control (toggle + scope) are not substitutes
  for each other.
- Comment count is a read projection; comments are not stored inside the host
  aggregate.

## 12. Likes

- Independent like relations: `BlogLike`, `PostLike`, `MomentLike`; comment
  likes are separate relations corresponding to the comment aggregate.
- Liker identity is not public: everyone only sees the like count (only the
  count is public).
  Rationale: The user chose count-only public visibility, so there is no need to
  filter a list of likers by visibility.
- When soft-deleted content is recovered, its likes are recovered together with
  it (§6.5).

## 13. Blog Favorites and Post Bookmarks

Favorites apply only to Blogs, bookmarks apply only to Posts; Moments have
neither.

```text
BlogFavorite / BlogFavoriteFolder
PostBookmark / PostBookmarkFolder
```

- Folders are private and visible only to the owner.
- Folder names: unique per account, case-sensitive, ≤128; folders per account
  ≤512.
- "Uncategorized" is represented by a nullable folder identity, not by a special
  user-editable folder record; when a folder is deleted, its entries move to
  uncategorized.
- Both folders and entries within folders support manual ordering.
- Favorite/bookmark uniqueness is per account (the same account cannot
  duplicate a favorite for the same target).
- Deleted or inaccessible targets display as "unavailable entries" in folders
  and are automatically restored upon recovery.
  Rationale: The user chose to retain placeholders rather than auto-remove, to
  avoid losing favorite organization when content is temporarily unavailable.

## 14. Categories and Tags

- Categories and Tags are each owned per-account, not a global taxonomy; each
  has a stable identity (globally unique Guid) so that renaming does not break
  content associations; all internal associations use Guids.
- Routing identifier: user-specified text identifier, mutable, unique within the
  account; display name and routing identifier are separate and neither enters
  internal associations.
- Name: ≤64, unique per account, case-sensitive.
- Categories: flat (no hierarchy); a Blog may have at most one optional
  category; categories per account ≤2048.
- Tags: Blog/Post/Moment each use separate tag namespaces
  (BlogTag/PostTag/MomentTag); tags per account ≤8192; tags per content item
  ≤32.
- Tag additional metadata: description (≤1024), cover media (media library
  reference), SEO metadata (SEO title ≤128 / SEO description ≤512), display
  order (manually sortable).
- Category/tag archive pages are read projections (§17).

## 15. Navigation Menus

- User-managed multiple menus: menus per account ≤64, menu items per menu ≤64.
- Menu item targets can be: internal path, external URL, Page, category/tag
  archive page.
- Menu items are ordered with manual sorting support.

## 16. Media Library

The media library is a first-class aggregate that centrally manages media
uploaded by an account; media references from Blog, Post, Moment, Page, comments,
and AccountProfile uniformly refer to media library items.

- Media items are isolated per account.
- Media files are never automatically deleted (when content is permanently
  deleted, only the reference links are removed); only the user may manually
  delete them.
- Post media: image/video/audio, ≤9; Moment media: image/video, ≤9; comment
  media: images only, ≤4.

## 17. Output and Discovery

- RSS/Atom, Sitemap, archive pages: both dynamic API and static JSON artifacts
  are provided; archive page dimensions: date, category, tag, author (all are
  read projections).
- Search: only dynamic API is provided, not static artifacts; scope covers all
  content types (Blog/Post/Moment/Page).
- `TimelineEntry` unified read projection aggregates the timeline of all content
  types.
- Output functionality enablement is controlled by `AccountSite` output toggles.
- Visibility authorization is enforced at the Application policy layer and
  re-enforced at the public query boundary; static generation likewise only
  outputs public content.

## 18. Explicit Exclusions

The following capabilities have been decided not to enter the current design:

- Archived lifecycle state (archive page is a read projection).
- Bare quotes (quotes must carry their own body or media).
- Site collaboration and site-level editorial roles; `PendingReview` only serves
  the single-author workflow.
- Page templates, localization/translation variants, import/export, shareable
  preview tokens.
- Comment editing, comment Deleted recovery.
- Custom domains (deferred only; to be designed when needed in the future).
- Shared mutable content base class, large generic `ContentItem` aggregate.

## 19. Constraints Quick Reference

| Item | Limit |
| --- | --- |
| Post body | 8192 characters |
| Moment body | 2048 characters |
| Comment body | 4096 characters |
| Blog/Page single text block | 2097152 characters |
| Blog title | 256 characters |
| Blog summary | 2048 characters |
| SEO title | 128 characters |
| SEO description | 512 characters |
| Tag/Category name | 64 characters |
| Tag description | 1024 characters |
| Nickname / Site title / CoAuthor text | 64 characters |
| Bio | 2048 characters |
| Site description | 1024 characters |
| Location | 128 characters |
| Folder name | 128 characters |
| Content blocks per item | 8192 |
| Tags per content item | 32 |
| Post media | 9 (image/video/audio) |
| Moment media | 9 (image/video) |
| Comment media | 4 (images only) |
| Audience exclusions | 2048 accounts |
| CoAuthors | 32 |
| Folders per account | 512 |
| Tags per account | 8192 |
| Categories per account | 2048 |
| Menus per account | 64 |
| Menu items per menu | 64 |
| Pages per account | 1024 |
| Soft-deletion recovery period | 7 days (fixed) |
