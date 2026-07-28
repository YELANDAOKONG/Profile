# Profile Business Rules

## Product

Profile is a .NET 10 personal publishing and social platform. It combines:

- A personal homepage.
- Long-form blog posts.
- Short-form posts similar to Twitter.
- Moment-style social posts with media and visibility controls.
- Dynamic API deployment.
- Static deployment through versioned JSON artifacts.
- ...

The same codebase must support both personal and community operation. Do not
create separate domain models or feature forks for these modes.

## Operating Modes

`Site.Mode` controls product behavior:

- `Personal`: one owner can publish. Public registration is disabled by
  default.
- `Community`: multiple users can publish and social relationships, feeds, and
  moderation can be enabled.

Registration policy is a separate setting. Model it independently with values
such as `Disabled`, `Invitation`, and `Open`.

Use the community-capable schema in both modes:

- Every piece of user-created content must have an `AuthorId`.
- FIDO credentials, sessions, roles, and social relationships belong to a
  user.
- Personal mode restricts behavior through policies; it must not remove
  ownership fields or use a simplified schema.
- Switching modes must be non-destructive. Do not delete or silently reassign
  users or content.

Do not assume that multi-user means multi-tenant. Supporting multiple isolated
sites with `TenantId` is a separate product decision.

## Basic Account Identity

Keep the basic account model separate from user-facing profile data. Nicknames,
avatars, biographies, and other presentation settings do not belong to the
basic account structure.

Each account has:

- An immutable `UserId` backed by a globally unique `Guid`.
- A mutable `StringId`.
- One current email address with verification metadata.

Login-input parsing belongs to the HTTP/controller boundary:

- `#<guid>` selects a `UserId`.
- `@<stringId>` selects a `StringId`.
- Input without a prefix defaults to `StringId`.
- `@` and `#` are input-only prefixes. They must never appear in Domain values,
  DTOs, persistence, or any internal representation of `StringId` and
  `UserId`.

`StringId` has these rules:

- Length is between 5 and 64 ASCII characters, inclusive.
- Allowed characters are `A-Z`, `a-z`, `0-9`, `_`, and `.`.
- It must not start or end with `.`.
- Consecutive `.` characters are forbidden.
- `_` may appear consecutively and at either end.
- Preserve the selected casing for display, but perform availability checks,
  uniqueness enforcement, and login lookup case-insensitively.
- After a change, only the new value may be used to log in.
- Retain the old value for a configurable reservation period whose default is
  90 days.

Email rules:

- An account has one current email address.
- The address may be changed.
- Email is not a login identifier.
- Email is not globally unique; multiple accounts may use the same address.
- Preserve the address while comparing and normalizing it case-insensitively.
- Store verification information for the current address.

When a user is deleted, permanently retain its `UserId`, StringId identity
records, and email identity records. Do not recycle identity data belonging to
a deleted account.

## Account Roles and Administrative Scope

The system may contain multiple accounts, including multiple administrator and
Root accounts. Every account has one current role:

- `User`: a regular account.
- `Administrator`: an account that may manage regular users.
- `Root`: the highest administrative role.

Role order is `User` < `Administrator` < `Root`. An administrator or Root
account may perform administrative operations only on an account whose current
role is strictly lower than its own:

- An `Administrator` may manage `User` accounts, but it must not manage another
  `Administrator` or a `Root`.
- A `Root` may manage `User` and `Administrator` accounts, including promoting
  an account to `Administrator` and demoting an `Administrator`.
- A `Root` must not suspend, ban, or otherwise administratively manage another
  `Root`.

Root accounts are not intrinsically immune to suspension, banning, or account
deletion. The account model must be able to represent those conditions even
though the ordinary administrative hierarchy does not authorize another Root
to impose them.

The trusted `Profile.Console` administrative surface operates outside the
ordinary account-role hierarchy. It may assign or revoke any account role,
including `Root`, and may apply or remove suspensions and bans for Root
accounts. No account, including another Root, may perform those operations on a
Root through ordinary account-based administration.

## Account Restrictions and Deletion

Suspension and banning are distinct account restrictions and must not be
treated as equivalent states.

A suspension:

- May have an expiration time or be permanent.
- May include an optional reason.
- Allows the account to log in.
- Keeps all public content from the account visible.
- Prevents the account from performing state-changing operations, including
  publishing new content, while the suspension is active.

A ban:

- May have an expiration time or be permanent.
- May include an optional reason.
- Prevents the account from logging in while the ban is active.
- Hides all content belonging to the account while the ban is active.

An expiration time of `null` means that the suspension or ban is permanent.
When an expiration time is present, the restriction ceases to be active after
that time.

Suspension and ban objects represent only the account's current restriction.
The acting administrator identity and operation history do not belong to the
account aggregate; a separate audit or logging module may record them.

Account deletion uses a configurable recovery period whose default is 14 days:

- Requesting deletion starts the recovery period; it does not immediately
  delete the account.
- The recovery deadline is fixed when deletion is requested. Later changes to
  the configured recovery period do not affect an existing deletion request.
- During the recovery period, public content remains visible.
- The account may still log in, but it may only restore the account and must
  not perform other operations.
- Restoring the account cancels the pending deletion.
- After the recovery period expires, the account may be permanently deleted.
- Permanent deletion must still retain the identity records required by the
  Basic Account Identity rules.

When account restrictions overlap:

- An active ban takes precedence over deletion recovery. A banned account
  cannot log in to restore itself, and none of its content is publicly visible.
- If an account is both suspended and pending deletion, it may log in but may
  only restore the account. Its public content remains visible.

Login, authorization, public queries, and static generation must all enforce
these rules. Hiding actions in the user interface is not sufficient.

## Content Model

Keep distinct write models for distinct behavior:

- `Post` for long-form articles, slugs, summaries, and publication metadata.
- `MicroPost` for short posts and reply/repost relationships.
- `Moment` for media-oriented social posts and visibility rules.
- `TimelineEntry` as a unified read projection.

Do not create a large mutable `ContentItem` aggregate that contains fields for
every content type. Share small value objects and policies where behavior is
genuinely common.

Visibility and authorization must be enforced in Application policies and
again at public query boundaries. Never rely on cache keys or front-end hiding
for access control.

## Authentication, Mail, and Security

- Use FIDO/WebAuthn credentials for account authentication. The exact FIDO
  package must be confirmed before adding the dependency.
- Store FIDO challenges in shared storage when more than one Web instance is
  active.
- Share ASP.NET Core Data Protection keys in clustered deployments.
- Use MailKit for email delivery. Send mail through queued background work;
  HTTP requests must not wait for SMTP delivery.
- Never store secrets, connection strings, SMTP credentials, RabbitMQ
  credentials, or FIDO private material in tracked configuration files.
- Do not invent authentication, account-recovery, friendship, moderation, or
  content-visibility rules. Ask the user when these product policies are not
  specified.
