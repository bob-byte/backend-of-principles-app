# Principles (backend)

ASP.NET Core WebAPI for the Principles habit/goal app.

Official install page (Android, macOS, iOS, iPadOS): [principles.top](https://principles.top)

Clients: [Flutter](https://github.com/bob-byte/flutter-frontend-of-principles) (primary rewrite) and [MAUI](https://github.com/bob-byte/maui).

## Domain

- **Users / profile** — auth (email, Apple, Google), slogan, mission, road-guide flag
- **Goals** — user goals with optional notes (`goal` schema)
- **Habits** — progress, frequency, reminders, areas of life (`hbt` / `arlf`)
- **Tasks** — one-off work with schedule, reminders, repeat, and checklists (`tsk.TaskSubtasks`)
- **Sync** — `GET api/sync/bootstrap` for first catch-up; `GET api/sync/changes?since=` for incremental peers (changed entities + `Deleted*Ids`; `RequiresFullBootstrap` when `since` is missing/older than 14 days)
- **Reminders** — `GET api/reminder/all`; bootstrap/changes also return `GeneralReminders` + `UserHabitReminders`
- **AI** — Helper chat, parse-task, titles, habit/goal recommendations, profile text suggestions (`AI_API_KEY` stays server-side)
- **Silent sync push** — FCM data-only wakeups so other devices pull changes (`PUT api/device/push-token`)

## Architecture

Solution: `Mentor.SET.Backend.sln`

| Project | Role |
|---------|------|
| `SET.WebAPI` | Controllers, DTOs, helpers, Startup/Program, resources |
| `SET.BusinessLogic` | Services (`Interfaces` / `Implementation`), AI, reminders, email |
| `SET.DAL` (`SET.DataAccess`) | `AppDbContext`, entity configs, EF migrations |
| `SET.Shared` | Entities, shared DTOs/extensions |
| `SET.UnitTests` | Unit tests |

PostgreSQL via EF Core. Schemas: `app`, `goal`, `hbt`, `arlf`, `tsk`. Prefer camelCase JSON DTOs in `SET.WebAPI/Models`; do not leak EF entities as API responses.

## Tech stack

- .NET SDK `10.0.101` (`global.json`; roll-forward `latestFeature`)
- ASP.NET Core WebAPI (`net10.0`)
- EF Core 10 + Npgsql / PostgreSQL
- JWT Bearer auth
- AutoMapper, Serilog, Swashbuckle, Newtonsoft.Json
- AI: `Microsoft.Extensions.AI` + Microsoft Agent Framework (`ChatClientAgent`)
- Outbound mail: MailKit (Gmail SMTP)
- Optional FCM v1 silent sync push

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- PostgreSQL reachable with a connection string
- Optional: Visual Studio 2022 or JetBrains Rider

## Getting started

1. Copy env template and fill secrets (never commit real values):

```bash
cp SET.WebAPI/.env.example SET.WebAPI/.env
```

DEBUG loads `SET.WebAPI/.env` via DotNetEnv. Important variables:

| Variable | Purpose |
|----------|---------|
| `DEFAULT_CONNECTION` | PostgreSQL (DEBUG) |
| `HOSTINGER_CONNECTION` | PostgreSQL (Release / hosted) |
| `GOOGLE_ANDROID_CLIENT_ID` / `GOOGLE_IOS_CLIENT_ID` | Google Sign-In token audience |
| `HOST_EMAIL_PASSWORD` | Gmail App Password for verification emails |
| `AI_API_KEY` | AI completions (server-side only) |
| `PRINCIPLES_SERVER_JWT_SECRET` | JWT signing |
| `FIREBASE_CREDENTIALS_JSON` / `FIREBASE_CREDENTIALS_PATH` | Optional FCM sync push (unset = disabled) |

Do not put DB passwords or Google client IDs in `appsettings*.json`.

2. Restore, migrate, run:

```bash
dotnet restore Mentor.SET.Backend.sln
dotnet ef database update --project SET.DAL --startup-project SET.WebAPI
dotnet run --project SET.WebAPI --launch-profile https
```

Swagger opens at `https://localhost:6001/swagger` (HTTP profile uses `http://localhost:6001`).

### Common commands

```bash
# from backend/
dotnet build Mentor.SET.Backend.sln
dotnet test SET.UnitTests/SET.UnitTests.csproj
dotnet ef migrations add <Name> --project SET.DAL --startup-project SET.WebAPI
dotnet ef database update --project SET.DAL --startup-project SET.WebAPI
```

## Project structure

```text
Mentor.SET.Backend.sln
SET.WebAPI/                 # HTTP API
  Controllers/
  Models/
  Helpers/
  Program.cs / Startup.cs
SET.BusinessLogic/          # Domain services
SET.DAL/                    # EF Core + Migrations/
SET.Shared/                 # Entities and shared types
SET.UnitTests/
```

## Notes

- Git host is **GitHub only** (`origin`). Do not push to Bitbucket or other remotes.
- Never commit `.env`, Firebase service-account JSON, or live secrets.
- Nested task `subtasks`: `null`/omitted keeps existing rows; empty array clears them. Completing a subtask does not complete the parent task.
