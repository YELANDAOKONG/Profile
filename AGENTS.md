# Profile Repository Guidance

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
- Prefixes are input syntax and must not be stored in Domain values.

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

## Supported Infrastructure

The application must select infrastructure through configuration at startup.
Business and application code must not branch on a concrete provider.

### Database

Supported EF Core providers:

- SQLite for local development, demonstrations, and low-traffic single-node
  deployments.
- PostgreSQL for production, concurrent workloads, and clustered deployments.

Keep the shared entity model portable across both providers. Avoid
provider-specific column types or SQL in the domain and application layers.
Maintain provider-specific migrations separately when migrations are
introduced.

SQLite is not a supported clustered database and must not be placed on a
network-shared filesystem.

### Messaging

Supported MassTransit transports:

- In-memory transport for development and simple single-process deployments.
- RabbitMQ for durable messaging, independent workers, and clustered
  deployments.

MassTransit must remain on version `8.3.6` unless the user explicitly approves
a version change.

The in-memory transport is process-local and non-durable. When it is selected:

- Consumers must run inside the Web process.
- `Profile.Worker` must not be launched as an independent consumer host.
- Features must not imply that messages survive a process restart.

When RabbitMQ is selected, the Web and Worker processes may be deployed and
scaled independently. Use an EF-backed transactional outbox for state changes
that publish messages. Consumers must be idempotent.

### Caching

Use ZiggyCreatures.FusionCache version `2.6.0` unless the user explicitly
approves a version change.

- Single-node deployments may use memory-only caching.
- Redis may provide the distributed L2 cache in a single-node deployment.
- Clustered deployments require Redis for distributed caching and cache
  invalidation.
- Cache keys must include all dimensions that affect visibility, locale,
  pagination, and user-specific results.
- Invalidate cached data from committed content-change events. Do not
  invalidate before the database transaction succeeds.

### Valid Deployment Profiles

These combinations are supported:

| Database | Messaging | Intended use |
| --- | --- | --- |
| SQLite | In-memory | Development and lightweight personal deployment |
| SQLite | RabbitMQ | Single host with durable or independent background work |
| PostgreSQL | In-memory | Development or deliberate single-process deployment |
| PostgreSQL | RabbitMQ | Production community or clustered deployment |

Cluster mode must fail startup validation unless PostgreSQL, RabbitMQ, and
Redis are configured.

## Architecture

Keep the solution as a modular monolith unless the user explicitly requests a
service boundary.

- `Profile.Domain`: aggregates, value objects, domain policies, and domain
  events. It must not depend on infrastructure.
- `Profile.Application`: use cases, ports, authorization requirements, query
  contracts, commands, and DTOs.
- `Profile.Infrastructure`: EF Core, database providers, MassTransit
  consumers, outbox, FusionCache, Redis, MailKit, FIDO, and external service
  implementations.
- `Profile`: HTTP composition root, controllers, authentication,
  authorization, and API contracts.
- `Profile.Worker`: independent RabbitMQ consumer composition root. It may
  reference shared consumer implementations but Web must not reference this
  executable project.
- `Profile.Generator`: command-line composition root for versioned static JSON
  generation.
- `Profile.Contracts`: add this project when integration-event contracts are
  introduced. Keep these contracts stable and free of infrastructure types.

Web, Worker, and Generator must reuse Application use cases and Infrastructure
registrations. Do not duplicate publishing, authorization, or query logic in a
composition root.

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

## Static JSON Generation

Dynamic API responses and static generation must use the same public
Application query contracts. Never serialize EF entities directly.

The static output should include a versioned manifest and stable paths such as:

```text
manifest.json
profile.json
timeline/index.json
posts/index.json
posts/{slug}.json
moments/{id}.json
tags/{slug}.json
```

The manifest must include at least a schema version, generation timestamp, and
content hash. Write a complete generation to a temporary output location and
replace the published output atomically so readers cannot observe a partial
generation.

Treat JSON schema changes as public contract changes. Add compatibility or
contract tests before changing existing output.

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

## Dependency Rules

The intended stack includes MailKit, FIDO/WebAuthn, MassTransit `8.3.6`, EF
Core, and ZiggyCreatures.FusionCache `2.6.0`.

Before writing code against a package:

1. Verify that it exists in the relevant project file.
2. Ask for approval before adding an unspecified package or changing a
   dependency version.
3. Keep provider packages in Infrastructure or a composition root; Domain must
   remain dependency-light.
4. Do not suppress NuGet vulnerability warnings. Report and resolve them with
   an explicitly approved dependency change.

## C# Conventions

- Target .NET 10 with nullable reference types enabled.
- Use file-scoped namespaces and one type per file.
- Use an explicit `Program.Main`; do not use top-level statements.
- Use `async`/`await` end to end. Do not use `.Result` or `.Wait()`.
- Use C# keywords such as `string` and `int`.
- Require braces for all control-flow blocks.
- Every switch must have an explicit fallback arm.
- Use `record` types for immutable DTOs and messages when value equality is
  appropriate.
- Runtime messages, logs, exceptions, and API error text must be plain English.
- Comments should explain why a constraint exists, not restate the code.
- Preserve existing comments unless the user explicitly authorizes removing
  them.

## Testing and Verification

Add tests with each vertical slice:

- Domain tests for aggregate invariants and visibility policies.
- Application tests for commands, queries, and authorization.
- Infrastructure integration tests for each supported database provider.
- Messaging tests for idempotency and outbox behavior.
- Static-generation contract or golden-file tests.

Provider and deployment behavior must not be considered complete after testing
only SQLite with the in-memory transport.

Before handing off a code change, run the narrowest relevant tests and then,
when practical:

```shell
dotnet build Profile.sln
dotnet test Profile.sln
```

Report warnings as well as failures. Do not claim that tests pass when no tests
were discovered.
