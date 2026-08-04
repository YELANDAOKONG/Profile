# Profile Domain Design

This document is the final design of the Profile domain model, covering all
domains (Accounts, Sites, Social, Content, Interactions, Output).
`BUSINESS.md` defines product-level business rules and `ARCHITECTURE.md`
defines the technical architecture; this document translates both into
concrete aggregates, value objects, and invariants. Every decision includes
its rationale.

Document conventions:

- All runtime text (logs, exceptions, API errors) uses English; this
  document describes design only.
- All length limits are in characters; all count limits are in items.

## 1. Overall Modeling Principles

- The domain model and database entity model are independent; the
  infrastructure layer is responsible for mapping.
  Rationale: `ARCHITECTURE.md` requires the Domain to be unaffected by
  persistence shape.
- `Blog`, `Post`, and `Moment` are three independent aggregates and must
  not be merged into a generic `ContentItem`; `Page` is a fourth
  independent aggregate. The four do not introduce a shared mutable
  content base class, sharing only small value objects with identical
  semantics.
  Rationale: The behavioral evolution paths of the three (and Page) differ;
  merging would force aggregates to carry fields irrelevant to each other;
  a shared base class would leak changes from one to others.
- Each aggregate uses a strongly-typed identity: `BlogIdentity`,
  `PostIdentity`, `MomentIdentity`, `PageIdentity`, `UserIdentity`, etc.,
  each backed by a globally unique Guid. Cross-aggregate references use
  the corresponding strongly-typed identity; e.g., a Post reference must
  not accept a `MomentIdentity`.
  Rationale: Strongly-typed identities prevent cross-aggregate
  misreferences at compile time.
- Potentially unbounded interactions and history (comments, likes, reposts,
  favorites, bookmarks, revisions) are never stored inside content
  aggregates; they are independent aggregates or independent
  relationships. Interaction counts (comment count, like count, repost
  count, favorite count) are all read projections.
  Rationale: Unbounded collections would break aggregate loading
  boundaries and consistency boundaries.
- `TimelineEntry` is only a unified read projection, not a write
  aggregate.
  Rationale: A timeline is a query view over multiple content types and
  owns no write invariants.
- The system internally always uses globally unique Guids for
  associations; no user-visible text identifier (route slug, display name)
  ever enters internal associations.
  Rationale: The user explicitly requires that internal associations not
  depend on mutable text identifiers — renaming must not break
  associations.

## 2. Deployment Site

`Site` represents deployment-level behavior, exactly one per deployment:

- `Mode`: `Personal` / `Community`. Both modes share the same multi-user
  schema; mode switching is non-destructive.
- Registration policy: `Disabled` / `Invitation` / `Open`, modeled
  independently from `Mode`.
- Owner: The site owner account reference used in Personal mode.
- Under Personal mode, which `AccountSite` the deployment root path
  exposes is configurable — it is not fixed as the owner's site.
  Rationale: This was an open item in TEMP; the user chose to preserve
  configuration flexibility.

Custom domains are explicitly excluded from the current design scope, but
this is only deferred, not permanently excluded; domain ownership and
verification rules will be designed when introduced in the future.

## 3. Account and Basic Identity

The following rules are consistent with `BUSINESS.md` and finalized here
as domain structures:

- The `Account` aggregate contains: immutable `UserIdentity` (backed by
  Guid), mutable `StringIdentity`, a current `AccountEmail` (with
  verification metadata), current `AccountRole`, current
  `AccountSuspension?`, current `AccountBan?`, `AccountDeletion?`.
- Display info such as nickname, avatar, and bio does not belong to
  `Account`; see §4.
- `StringIdentity` rules: 5–64 ASCII characters; character set
  `A-Z a-z 0-9 _ .`; must not begin or end with `.`; consecutive `.`
  forbidden; `_` may appear consecutively and at either end; chosen case
  is preserved for display, but availability checks, uniqueness, and login
  lookups are case-insensitive; after a change, only the new value can be
  used to log in, and the old value is reserved for a configurable
  grace period (default 90 days).
- Login input parsing occurs only at the HTTP boundary: `#<guid>` selects
  UserId, `@<stringId>` selects StringId, no prefix defaults to StringId;
  `@` and `#` are pure input prefixes and never appear in domain values,
  DTOs, persistence, or any internal representation.
- Email: one current address per account, changeable; not a login
  identifier; not globally unique; comparison and normalization are
  case-insensitive but original casing is preserved; verification info
  for the current address is stored.
- Roles: `User < Administrator < Root`; a higher role may only manage
  accounts strictly lower than itself; Roots must not manage each other;
  `Profile.Console` is a trusted administration surface that can perform
  out-of-band operations on Roots. Restriction history and operator
  identity do not belong to the Account aggregate; they are recorded by an
  independent audit module.
- Suspension: expirable or permanent (`null` means permanent), optional
  reason; allows login; content remains visible; blocks all state-changing
  operations.
- Ban: expirable or permanent, optional reason; forbids login; hides all
  content of that account.
- Account deletion: configurable recovery period (default 14 days); when
  deletion is requested, the recovery deadline is fixed; content remains
  visible during the recovery period; the account can log in but may only
  perform recovery; permanent deletion may occur after the deadline;
  permanent deletion still permanently retains UserId, StringId identity
  records, and email identity records.
  Rationale: Identity records are never reclaimed, preventing old
  identities from being reoccupied by others.
- Content disposition on permanent deletion is chosen by the user when
  requesting deletion:
  (a) Content retained, author displayed as "Deactivated account"
      placeholder;
  (b) Content hidden but data retained.
  Rationale: The user explicitly requires both modes to be supported; the
  choice belongs to the account owner.
- Memorial Account: a distinct terminal account state for accounts whose
  owner has passed away, set by an Administrator/Root; effects: login
  forbidden, content frozen and retained (no further changes or
  interactions), profile displays a memorial badge. Memorialization is an
  alternative terminal state to deletion — the two are mutually exclusive:
  a memorialized account does not enter the deletion flow, and an account
  already in the deletion flow cannot be converted to memorial.
  Rationale: The user introduced a memorial system so that deceased
  users' accounts can be preserved rather than deleted.
- Restriction stacking: an active Ban takes precedence over deletion
  recovery (cannot log in, so cannot self-recover; content invisible);
  Suspension coexisting with deletion: can log in but only recover;
  content visible.

Authentication uses FIDO/WebAuthn; in clustered deployments, challenge
storage and Data Protection keys must be shared; emails are sent via
MailKit through queued background jobs.

## 4. Account Profile, Settings, and Account Site

Each account has exactly one of each of the following objects:

### 4.1 AccountProfile (Public Identity Display)

Fields: nickname (≤64), avatar (media library reference), bio (≤2048),
location (≤128), personal link (a single URL), banner/background (media
library reference).

Rationale: Public display info is separated from the basic account
structure; `BUSINESS.md` requires that nickname, avatar, and bio do not
belong to the basic account model.

### 4.2 AccountSettings (Account-Level Private Preferences)

Fields:

- Default visibility: per content type (`Blog`, `Post`, `Moment`) default
  `ContentVisibility` (system-level default is `Public` for all).
- Language preference, timezone.
- Email notification preferences: new comments / comment moderation, new
  followers, interactions (like / repost / quote), system notifications,
  each toggleable independently.
- Follow-requires-approval toggle (see §5).

Rationale: The user explicitly specified that default visibility and other
preferences are stored in an independent `AccountSettings`, not in
`AccountSite` or `AccountProfile`; these are private behavioral
preferences, not public site configuration.

### 4.3 AccountSite (Account Site Configuration)

Each account has exactly one `AccountSite`, using its `OwnerId`
(UserIdentity) as the stable domain identity — no separate site identity
is assigned. Fields:

- Site title (≤64), site description (≤1024).
- Theme / appearance settings: structured color / appearance items (fields
  for primary color, background, font, etc.), not free-form CSS.
- Page size.
- Default comment moderation policy (site-level default, overridable per
  content item, see §11).
- Output toggles: enable/disable switches for RSS/Atom, Sitemap, archive
  pages, and search.

`AccountSite` does not contain nickname, avatar, bio, or other public
identity info (which belong to `AccountProfile`). Content stores only
`AuthorId` and does not duplicate account site identity.
Rationale: With one account = one site, OwnerId is already unique; content
and site can be associated via the author account.

## 5. Social Relationships

### 5.1 Follow

- The shape of the follow relationship depends on account settings: each
  account chooses between direct follow or follow requests requiring
  approval via the "follow requires approval" toggle in
  `AccountSettings`.
- The `Followers` / `MutualFollowers` visibility audience is determined in
  real time at query time; changes in follow relationships immediately
  affect the accessibility of historical content.
  Rationale: The user confirmed real-time evaluation to avoid snapshot
  audiences becoming disconnected from current social relationships.

### 5.2 Block

An account may block other accounts. Block effects:

- Forbids following (existing follows are automatically removed);
- Forbids commenting on the other party's content;
- Forbids interactions (like, repost, quote, favorite, bookmark).

Block does not make content invisible: the blocked party can still see the
blocker's Public content.
Rationale: The user selected the three active behavioral restrictions and
did not choose content invisibility.

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

- `ContentBody` is an immutable value object; editing replaces it
  wholesale.
- The format is explicitly recorded and persisted; the system never
  guesses the format from the content. Editing may change both source and
  format simultaneously.
- Rendered HTML is a derived output and does not belong to the domain
  model.

### 6.2 ContentVisibility

```text
ContentVisibility
  Public
  Followers
  MutualFollowers
  Private
```

- Blog, Post, Moment, and Page share this enum; Followers and
  MutualFollowers are distinct audiences.
- Visibility may be changed after publishing.
- Default visibility: Blog / Post / Moment are all `Public`; accounts can
  configure their own defaults per content type in `AccountSettings`.

### 6.3 ContentBlock

The body of Blog and Page is composed of an ordered collection of content
blocks; Post, Moment, and comments do not use content blocks and instead
use only a single `ContentBody`. Block types:

- Text block: contains `ContentBody` (Markdown or plain text).
- Media block: media library reference.
- Blockquote: contains `ContentBody` and is a distinct concept from
  "Quote Post/Quote Moment" referencing Post/Moment.
- Code block: contains source text and a non-empty language identifier.
- Divider block.
- Spacer block: represents one fixed unit of vertical spacing and carries no
  configurable size or nested content. Other layout blocks remain deferred.

A Text block, Blockquote, or Code block may contain at most 2097152
characters of text. A language identifier must not contain surrounding
whitespace. These textual-block rules are shared because all three block
types carry author-provided text with the same storage and rendering bound.

A single Blog/Page may have ≤8192 blocks. Tags per content item ≤32.
Rationale: The user decided that long-form content uses a block editor
model; the block limit is set large enough to avoid practically limiting
creation.

### 6.4 Publication Representation

The publication state machine has four states: `Draft`, `Scheduled`,
`PendingReview`, `Published`. There is no `Archived` state. Legal
transitions form a restricted matrix:

- `Draft ⇄ Scheduled` (schedule / unschedule);
- `Draft → PendingReview` (submit for review);
- `PendingReview → Published` (approved) or `PendingReview → Draft`
  (returned);
- `Published → Draft` (unpublish, see §6.5 for save-as-draft / discard
  choice);
- `Scheduled → Published` (auto-publish on expiration).

Transitions not listed are all illegal (e.g., `Published` must not return
to `PendingReview`, `Scheduled` must not return to `PendingReview`).
Rationale: The user chose a restricted matrix so that every step in the
publishing flow has a clear entry and exit point.

- Scheduled publish time uses `DateTimeOffset`.
- Published items record `FirstPublishedAt` and `LastPublishedAt`;
  unpublishing retains both.
  Rationale: The user chose to retain both first and most recent publish
  times; unpublishing does not erase history.
- An Archived lifecycle state is not needed; the CMS archive page is a
  read projection and does not depend on content state.
  Rationale: TEMP recommendation, user confirmed; "archive page" and
  "archived state" are different concepts and must not be conflated.

### 6.5 Deletion Representation

Publication state and deletion state are modeled separately. The deletion
object records:

- `DeletedAt`;
- `PurgeAt`: fixed at `DeletedAt + 14 days` when deletion occurs;
  thereafter immutable.

Rules:

- Soft-deleted content enters the recycle bin and can be restored;
  restoration restores all relationships from before deletion (comments,
  likes, reposts, bookmarks, favorites, revisions, category/tag
  associations).
- Automatic permanent deletion after 14 days; no one (including the author
  and administrators) may permanently delete before the deadline.
  Rationale: The user chose expiration-only automatic purge so that the
  recovery period is absolute for all roles.
- Permanent deletion cascades to delete that content's comments, likes,
  reposts, bookmarks, favorites, revisions, and media links; the media
  files themselves in the media library are never automatically deleted
  and may only be manually removed by the user.
  Rationale: Media may be referenced in multiple places, and the user
  explicitly stated that media files are not subject to automatic cleanup.
- When unpublishing, the user may choose "Save as Draft" (convert to
  Draft) or "Discard" (enter soft deletion with the 14-day recovery
  period).

### 6.6 MediaReference

Media in all aggregates uniformly references a media item in the media
library (§16); content does not embed media files. A `MediaReference`
contains the media library identity and optional context-specific `AltText`.
`AltText` belongs to the reference because the same media item may need
different descriptions in different content. When present, it must be
non-empty and must not contain surrounding whitespace.

## 7. Blog

### 7.1 Identity and Routing

- `BlogIdentity`: globally unique Guid internal identity.
- `BlogSlug`: system-generated, immutable, numeric-only, leading-zero-
  preserved string; unique per account (scoped by `AuthorId`); assigned
  monotonically per account; minimum width 9 digits (`000000001`); extends
  to wider digits when the number space is exhausted; after permanent
  deletion, the slug is never reused.
  Rationale: The leading-zero requirement means the slug is modeled as a
  validated string, not an integer; never-reuse prevents old links from
  pointing to new content; allocation involves cross-aggregate uniqueness
  and is passed in by a coordinator external to the Blog aggregate.
- Public route form: `/@{stringId}/blog/{slug}`. The `@` is only a URL
  display layer prefix and does not enter domain values, DTOs, or
  persistence (consistent with the §3 prefix rule).
- After a StringId change, Blog routes containing the old value are 301
  redirected to the new route for the old-value grace period (§3, default
  90 days); they return 404 after the grace period expires.
  Rationale: During the grace period, the old identity still belongs to
  that account, so redirects preserve existing links; after expiration,
  the old identity may be claimed by others and must not be redirected.

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
- CoAuthor is a display-only marker and may take either form: a reference
  to a system account (UserIdentity) or free-form text (≤64); ownership
  and permissions of the Blog still belong solely to `AuthorId`.
  Rationale: Site collaboration is not permitted (§7.4); co-authors are
  solely attribution display and do not confer permissions.
- Draft invariants: title must not be empty; body (block collection) may
  be empty — to support autosave.
  Rationale: The user decided that only the body may be empty; a non-empty
  title makes drafts identifiable in lists.
- Publish invariants: when publishing / submitting for review, only a
  non-empty title is validated; body (block collection) may be empty.
  Rationale: The user explicitly stated that Blog publishing does not
  require a non-empty body, in contrast to the Post/Moment rule of "at
  least body or media."

### 7.3 Revision History

- Each explicit save / publish produces an immutable revision; all
  versions are permanently retained and can be rolled back to any version.
  Autosave does not generate a revision; it only overwrites the current
  working copy.
  Rationale: Autosave triggers frequently; generating a revision on each
  would drown out truly meaningful history.
- Published content is edited in place, taking effect immediately, while
  simultaneously recording a revision.
  Rationale: The user confirmed full revision history + in-place effect;
  revisions are a historical archive, not parallel unpublished versions.

### 7.4 Collaboration and Review

- Other accounts are not allowed to collaboratively manage an account
  site: the author of site content is the site owner; there is no
  site-level member / editor role model.
- The publication state set includes `PendingReview` as a single-author
  editorial workflow state (self-submit for review / pre-publish
  organization).

### 7.5 Batch Operations and Preview

- Batch operations are supported (for multiple Blogs at once): batch
  category / tag assignment, batch visibility modification, batch status
  operations (publish / unpublish / delete).
- Previews are not shareable: only the logged-in author can preview; no
  preview tokens are issued.

## 8. Page

An independent aggregate carrying non-chronological site pages such as
About, Contact, Privacy Policy, etc.

- `PageIdentity`: globally unique Guid internal identity; routing uses a
  user-specified text identifier (mutable, slug-style rules in §14), e.g.,
  `/about`.
  Rationale: Consistent with the taxonomy — internal Guid for
  associations, text identifier is only a mutable route.
- Structure: title (≤256, consistent with Blog), ordered content block
  collection (same model as Blog), visibility, comment toggle and
  commenter scope, SEO title / description, featured media.
- Simplified lifecycle: only `Draft` / `Published` + soft deletion (14-day
  recycle rule same as §6.5); no scheduled publishing, no PendingReview,
  no revision history.
  Rationale: Pages change infrequently; the user explicitly chose a
  simplified lifecycle.
- Pages per account ≤1024.

## 9. Post

Twitter-style short media posts.

```text
Post
  Id: PostIdentity
  AuthorId: UserIdentity
  Body: ContentBody?           // ≤8192
  Media: read-only MediaReference collection  // image+video+audio, ≤9
  Visibility: ContentVisibility
  AudienceRestrictionMode: Blacklist / Whitelist
  AudienceAccounts: UserIdentity collection (≤2048)
  QuotedPostId: PostIdentity?
  CommentsAllowed: bool
  CommenterPolicy: CommenterPolicy
  TagIds: PostTagIdentity collection (≤32)
  Publication
  Deletion?
  CreatedAt / UpdatedAt
```

- Draft invariants: at least one of body or media (completely empty drafts
  are not allowed).
- Publish invariants: at least one of body or media. Quoting does not
  provide an exemption — a quote post must also carry its own body or
  media (user ruling, bare quotes withdrawn).
- Audience restrictions are an optional account set overlaid on top of the
  visibility enum. `Blacklist` first determines the audience from visibility
  and then subtracts the listed accounts. `Whitelist` intersects the
  visibility audience with the listed accounts. Neither mode can expand the
  audience granted by the visibility enum.
- An empty account set has no effect in `Blacklist` mode and produces an empty
  audience in `Whitelist` mode. The account set contains at most 2048 unique
  `UserIdentity` values.
- Repost is modeled as an independent relationship `PostRepost`, not a
  special Post containing copied body; reposting one's own Post is
  allowed; the same account may repost the same Post multiple times.
- Quote is a new Post: carries its own body/media + `QuotedPostId`; a
  Post may only repost/quote a Post.
- Only Public content may be reposted / quoted; operations to repost/quote
  non-public content are directly rejected by the domain.
  Rationale: The user decided to intercept at the operation entry point to
  prevent restricted content from leaking via repost chains.
- When the original Post is deleted, hidden, or visibility is reduced:
  quotes retain their own content, and the original content position shows
  an "Unavailable" placeholder; pure reposts are hidden entirely; after
  the original content is restored, quotes and reposts automatically
  recover.
- Comment count, like count, repost count, and favorite count are read
  projections.
- Tags use an independent PostTag namespace (§14).

## 10. Moment

Lightweight sharing in the style of WeChat Moments. Deliberately kept as
an independent aggregate from Post: each has its own identity,
relationships, comments, commands, policies, and repositories so that
behaviors can evolve independently.

```text
Moment
  Id: MomentIdentity
  AuthorId: UserIdentity
  Body: ContentBody?           // ≤2048
  Media: read-only MediaReference collection  // image+video, ≤9
  Visibility: ContentVisibility
  AudienceRestrictionMode: Blacklist / Whitelist
  AudienceAccounts: UserIdentity collection (≤2048)
  Location: structured location (coordinates + place name)?
  QuotedMomentId: MomentIdentity?
  CommentsAllowed: bool
  CommenterPolicy: CommenterPolicy
  TagIds: MomentTagIdentity collection (≤32)
  Publication
  Deletion?
  CreatedAt / UpdatedAt
```

- Draft and publish invariants are the same as Post: at least one of body
  or media.
- Audience restrictions are an optional account set overlaid on top of the
  visibility enum. `Blacklist` first determines the audience from visibility
  and then subtracts the listed accounts. `Whitelist` intersects the
  visibility audience with the listed accounts. Neither mode can expand the
  audience granted by the visibility enum.
- An empty account set has no effect in `Blacklist` mode and produces an empty
  audience in `Whitelist` mode. The account set contains at most 2048 unique
  `UserIdentity` values.
- A Moment can only repost/quote a Moment; Moments have no favorites and
  no bookmarks.
- Comment / like visibility rules are the same as Post; when the original
  Moment is unavailable, repost/quote behavior is the same as Post (quotes
  retain own content + placeholder, pure reposts are hidden).
- Moments exist permanently with no automatic expiration.
- Tags use an independent MomentTag namespace (§14).

## 11. Comments

- Separate comment aggregates are established per host: `BlogComment`,
  `PostComment`, `MomentComment`, `PageComment`.
  Rationale: Consistent with the content aggregate split decision,
  allowing each type of comment to evolve independently.
- Structure: `Id`, `AuthorId` (commenter is a system account), host
  strongly-typed identity, optional `ParentCommentId` (unlimited nesting
  depth), `Body: ContentBody` (≤4096, supports Markdown and plain text),
  media (images only, ≤4), status, CreatedAt.
  Rationale: Commenters are system accounts because the "first-time
  commenter per account requires moderation" policy needs stable
  identification of commenters.
- Capabilities: Markdown, media, likes; not editable.
- State machine: `Pending` / `Approved` / `Spam` / `Deleted`; `Deleted` is
  not recoverable and only retains a placeholder record to maintain
  nesting tree structure.
  Rationale: Under unlimited nesting depth, hard-deletion would destroy the
  context of child comments.
- Moderation policy (choose one): no moderation required / first comment
  per commenter requires moderation (once approved, subsequent comments
  from that commenter appear directly) / all comments require moderation.
  Configuration hierarchy: site-level default (`AccountSite`) + per
  content item override.
- Each content item may turn off comments (`CommentsAllowed=false` means
  no one can comment, including the author); each content item may set a
  commenter scope (`CommenterPolicy`): all readers / followers only /
  mutual followers only / author only; default is "all readers."
  Rationale: The user chose tighter comment control than read permissions;
  the two-level control (toggle + scope) are not substitutes for each
  other.
- Comment count is a read projection; comments are not stored inside the
  host aggregate.

## 12. Likes

- Independent like relationships: `BlogLike`, `PostLike`, `MomentLike`;
  comment likes are independent relationships corresponding to the
  comment aggregate.
- Liker identity is not public: everyone only sees the like count (only
  the count is public).
  Rationale: The user chose count-only publicity, so there is no need to
  filter a list of likers by visibility.
- When soft-deleted content is restored, its likes are also restored
  (§6.5).

## 13. Blog Favorites and Post Bookmarks

Favorites apply only to Blog; Bookmarks apply only to Post; Moment has
neither.

```text
BlogFavorite / BlogFavoriteFolder
PostBookmark / PostBookmarkFolder
```

- Folders are private and visible only to their owner.
- Folder name: unique per account, case-sensitive, ≤128; folders per
  account ≤512.
- "Uncategorized" is represented by a nullable folder identity; there is
  no special user-editable folder record for it; when a folder is deleted,
  its entries move to uncategorized.
- Both folders and entries within folders support manual ordering.
- Favorite/bookmark uniqueness is determined per account (the same account
  cannot favorite the same target more than once).
- Deleted or inaccessible targets are displayed in the folder as
  "Unavailable Entry" and automatically restored when the content is
  recovered.
  Rationale: The user chose to retain placeholders rather than
  auto-remove, to avoid losing favorite organization when content is
  temporarily unavailable.

## 14. Categories and Tags

- Categories and Tags are each independently owned per account, not a
  global taxonomy; each has a stable identity (globally unique Guid) so
  that renaming does not break content associations; internal associations
  always use Guids.
- Route identifiers: user-specified text identifiers, mutable, unique per
  account; display name and route identifier are separate, and neither
  enters internal associations.
- Name: ≤64, unique per account, case-sensitive.
- Categories: flat (no hierarchy); a Blog may have at most one category,
  which is optional; categories per account ≤2048.
- Tags: Blog / Post / Moment each use an independent tag namespace
  (BlogTag / PostTag / MomentTag); tags per account ≤8192; tags per
  content item ≤32.
- Tag additional metadata: description (≤1024), cover media (media library
  reference), SEO metadata (SEO title ≤128 / SEO description ≤512),
  display order (manually sortable).
- Category / tag archive pages are read projections (§17).

## 15. Navigation Menus

- User-managed multiple menus: menus per account ≤64, menu items per menu
  ≤64.
- Menu item targets may be: internal path, external URL, Page, category /
  tag archive page.
- Menu items are ordered and support manual ordering.

## 16. Media Library

The media library is a first-class aggregate that centrally manages media
uploaded by accounts; media for Blog, Post, Moment, Page, comments, and
AccountProfile uniformly reference media library items.

- Media items are isolated per account.
- Media files are never automatically deleted (when content is permanently
  deleted, only the reference link is removed); only manual deletion by
  the user.
- Post media: image / video / audio, ≤9; Moment media: image / video, ≤9;
  comment media: images only, ≤4.

## 17. Output and Discovery

- RSS/Atom, Sitemap, archive pages: both dynamic APIs and static JSON
  artifacts are provided; archive page dimensions: date, category, tag,
  author (all read projections).
- Search: only dynamic API is provided, not included in static artifacts;
  scope is all content types (Blog / Post / Moment / Page).
- `TimelineEntry` is a unified read projection aggregating timelines
  across all content types.
- Output functionality is toggled by the `AccountSite` output switches.
- Visibility authorization is enforced in Application policy and again
  at the public query boundary; static generation likewise only outputs
  public content.

## 18. Explicitly Excluded Items

The following capabilities are excluded from the current design by
decision:

- Archived lifecycle state (archive pages are read projections).
- Bare quotes (quotes must carry their own body or media).
- Site collaboration and site-level editor roles; `PendingReview` serves
  only a single-author workflow.
- Page templates, localization / translation variants, import / export,
  shareable preview tokens.
- Comment editing, comment Deleted recovery.
- Custom domains (only deferred; will be designed when needed in the
  future).
- Shared mutable content base class, large generic `ContentItem` aggregate.

## 19. Constraint Quick-Reference Table

| Item | Limit |
| --- | --- |
| Post body | 8192 characters |
| Moment body | 2048 characters |
| Comment body | 4096 characters |
| Blog/Page textual block (Text / Blockquote / Code) | 2097152 characters |
| Blog title | 256 characters |
| Blog summary | 2048 characters |
| SEO title | 128 characters |
| SEO description | 512 characters |
| Tag/Category name | 64 characters |
| Tag description | 1024 characters |
| Nickname / Site title / Co-author text | 64 characters |
| Bio | 2048 characters |
| Site description | 1024 characters |
| Location | 128 characters |
| Folder name | 128 characters |
| Content blocks per item | 8192 |
| Tags per content item | 32 |
| Post media | 9 (image / video / audio) |
| Moment media | 9 (image / video) |
| Comment media | 4 (image only) |
| Single media file | Image 16 MB / Audio 64 MB / Video 1 GB |
| Total media quota | Per deployment config |
| Audience restriction accounts | 2048 accounts |
| Co-authors | 32 |
| Folders per account | 512 |
| Tags per account | 8192 |
| Categories per account | 2048 |
| Menus per account | 64 |
| Menu items per menu | 64 |
| Pages per account | 1024 |
| Soft-delete recovery period | 14 days (fixed) |
