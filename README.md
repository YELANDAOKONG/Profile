# Profile

A .NET 10 personal publishing and social platform combining a personal homepage,
long-form blog posts, short-form posts, and moment-style social posts with media
and visibility controls.

## Features

- **Personal homepage** with configurable profile and presentation settings.
- **Blog posts** — long-form articles with slugs, summaries, and publication
  metadata.
- **Micro posts** — short-form posts with reply and repost relationships.
- **Moments** — media-oriented social posts with per-post visibility rules.
- **Unified timeline** — read projection aggregating all content types.
- **Dynamic API** — ASP.NET Core web API and server-rendered pages.
- **Static deployment** — versioned JSON artifacts generated from the same API
  query contracts.
- **FIDO/WebAuthn authentication** — passwordless login with hardware security
  keys.
- **Hierarchical roles** — User, Administrator, and Root roles with role-based
  administrative scope.
- **Account restrictions** — time-limited or permanent suspension and banning,
  with account deletion via a configurable recovery period.
- **Two operating modes** — Personal (single owner) and Community (multi-user
  social) using the same schema.
- **Flexible infrastructure** — configurable database (SQLite / PostgreSQL) and
  messaging (in-memory / RabbitMQ).

## Tech Stack

| Component | Technology |
| --- | --- |
| Runtime | .NET 10 |
| Web framework | ASP.NET Core |
| ORM | Entity Framework Core |
| Messaging | MassTransit 8.3.6 |
| Caching | ZiggyCreatures.FusionCache 2.6.0 |
| Email | MailKit |
| Authentication | FIDO/WebAuthn |
| Database | SQLite (dev/single-node) or PostgreSQL (production) |
| Message broker | In-memory (dev) or RabbitMQ (production) |

## Solution Structure

```
Profile.sln
├── Profile.Domain/          Aggregates, value objects, domain policies, events
├── Profile.Domain.Tests/    Domain unit tests
├── Profile.Application/     Use cases, commands, queries, DTOs, authorization
├── Profile.Application.Tests/
├── Profile.Infrastructure/  EF Core, MassTransit, FusionCache, MailKit, FIDO
├── Profile.Infrastructure.Tests/
├── Profile/                 ASP.NET Core host, controllers, composition root
├── Profile.Worker/          Independent RabbitMQ consumer host
├── Profile.Generator/       CLI for static JSON generation
└── Profile.Console/         Trusted CLI for administrative operations
```

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- SQLite (bundled, no installation needed) or PostgreSQL
- (Optional) RabbitMQ and Redis for clustered deployments

### Build

```shell
dotnet build Profile.sln
```

### Run

```shell
dotnet run --project Profile
```

### Test

```shell
dotnet test Profile.sln
```

## Operating Modes

Profile supports two modes controlled by `Site.Mode`:

| Mode | Behavior |
| --- | --- |
| **Personal** | One owner publishes content. Public registration disabled. |
| **Community** | Multiple users, social relationships, feeds, and moderation. |

Both modes use the same multi-user schema. Every content item has an `AuthorId`.
Switching between modes is non-destructive.

## Account Roles

Three hierarchical roles control administrative scope:

| Role | Scope |
| --- | --- |
| **User** | Regular account with no administrative privileges. |
| **Administrator** | May manage User accounts. Cannot manage other Administrators or Roots. |
| **Root** | May manage Users and Administrators. Cannot manage another Root. |

The rank order is `User < Administrator < Root`. A higher role may only manage
accounts with a strictly lower role. The `Profile.Console` CLI provides a
trusted administrative surface for operations that exceed account-based
authorization (such as suspending or banning a Root account).

Account restrictions include time-limited or permanent suspension (login
permitted, state-changing operations blocked) and banning (login blocked,
content hidden). Account deletion uses a configurable recovery period (default
14 days) before permanent deletion; identity records are always retained.

## Deployment Profiles

| Database | Messaging | Use Case |
| --- | --- | --- |
| SQLite | In-memory | Development and lightweight personal deployment |
| SQLite | RabbitMQ | Single host with durable background work |
| PostgreSQL | In-memory | Development or single-process deployment |
| PostgreSQL | RabbitMQ | Production community or clustered deployment |

Clustered deployments require PostgreSQL, RabbitMQ, and Redis.

## Static Generation

The `Profile.Generator` CLI produces versioned JSON artifacts from the same
Application query contracts used by the dynamic API. Output includes a versioned
manifest and stable paths for posts, moments, tags, and the timeline.

## License

Licensed under the [GNU Affero General Public License v3.0](LICENSE).
