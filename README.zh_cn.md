# Profile

> [English](README.md)
>
> [业务规则](BUSINESS.md) · [架构](ARCHITECTURE.md) · [设计](DESIGN.md)

基于 .NET 10 的个人发布与社交平台，集个人主页、长篇博客、短文帖子和
朋友圈式社交帖（含媒体与可见性控制）于一体。

## 功能特性

- **个人主页** — 可配置的个人资料与展示设置。
- **博客（Blog）** — 长篇内容，支持标识、摘要与发布元数据。
- **帖子（Post）** — 短内容，支持回复与转发关系。
- **动态（Moment）** — 媒体向社交帖，每条可设定可见性规则。
- **统一时间线** — 聚合全部内容类型的读投影。
- **动态 API** — ASP.NET Core Web API 与服务端渲染页面。
- **静态部署** — 由同一套查询接口生成的带版本 JSON 产物。
- **FIDO/WebAuthn 认证** — 使用硬件安全密钥的无密码登录。
- **层级角色** — User、Administrator、Root 三级角色，各有管理范围。
- **账户限制** — 可定时限或永久的停权与封禁，以及带可配置恢复期的
  账户删除。
- **两种运行模式** — Personal（单人）与 Community（多人社交）共用
  同一 Schema。
- **灵活的基础设施** — 可配置数据库（SQLite / PostgreSQL）与消息
  系统（in-memory / RabbitMQ）。

## 技术栈

| 组件 | 技术 |
| --- | --- |
| 运行时 | .NET 10 |
| Web 框架 | ASP.NET Core |
| ORM | Entity Framework Core |
| 消息 | MassTransit 8.3.6 |
| 缓存 | ZiggyCreatures.FusionCache 2.6.0 |
| 邮件 | MailKit |
| 认证 | FIDO/WebAuthn |
| 数据库 | SQLite（开发/单节点）或 PostgreSQL（生产） |
| 消息代理 | In-memory（开发）或 RabbitMQ（生产） |

## 解决方案结构

```
Profile.sln
├── Profile.Domain/          聚合、值对象、领域策略、领域事件
├── Profile.Domain.Tests/    领域单元测试
├── Profile.Application/     用例、命令、查询、DTO、鉴权
├── Profile.Application.Tests/
├── Profile.Infrastructure/  EF Core、MassTransit、FusionCache、MailKit、FIDO
├── Profile.Infrastructure.Tests/
├── Profile/                 ASP.NET Core 宿主、控制器、组合根
├── Profile.Worker/          独立的 RabbitMQ 消费者宿主
├── Profile.Generator/       用于静态 JSON 生成的 CLI
└── Profile.Console/         用于管理员操作的受信 CLI
```

## 快速开始

### 环境要求

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- SQLite（已内置，无需安装）或 PostgreSQL
- （可选）RabbitMQ 与 Redis，用于集群部署

### 构建

```shell
dotnet build Profile.sln
```

### 运行

```shell
dotnet run --project Profile
```

### 测试

```shell
dotnet test Profile.sln
```

## 运行模式

Profile 通过 `Site.Mode` 支持两种模式：

| 模式 | 行为 |
| --- | --- |
| **Personal** | 仅一位所有者可发布内容。默认关闭公开注册。 |
| **Community** | 多用户、社交关系、信息流与内容审核。 |

两种模式共用同一套多用户 Schema。每条内容都带 `AuthorId`。
模式切换是无损的。

## 账户角色

三级层级角色控制管理范围：

| 角色 | 范围 |
| --- | --- |
| **User** | 普通账户，无管理权限。 |
| **Administrator** | 可管理 User 账户，不可管理其他 Administrator 或 Root。 |
| **Root** | 可管理 User 与 Administrator，不可管理其他 Root。 |

角色排序为 `User < Administrator < Root`。高角色仅可管理严格低于自己
的账户。`Profile.Console` CLI 提供受信管理面，用于超出账户级鉴权的
操作（如停权或封禁 Root 账户）。

账户限制包括可定时限或永久的停权（允许登录，阻止状态变更操作）与封禁
（禁止登录，隐藏内容）。账户删除使用可配置的恢复期（默认 14 天），之
后执行永久删除；身份记录始终保留。

## 部署方案

| 数据库 | 消息 | 适用场景 |
| --- | --- | --- |
| SQLite | In-memory | 开发与轻量个人部署 |
| SQLite | RabbitMQ | 单机 + 持久化后台任务 |
| PostgreSQL | In-memory | 开发或单进程部署 |
| PostgreSQL | RabbitMQ | 生产环境社区或集群部署 |

集群部署需要 PostgreSQL、RabbitMQ 与 Redis。

## 静态生成

`Profile.Generator` CLI 从与动态 API 相同的 Application 查询接口生成
带版本 JSON 产物。输出包含带版本清单与稳定的内容路径（帖子、动态、
标签、时间线等）。

## 许可证

基于 [GNU Affero General Public License v3.0](LICENSE) 许可。
