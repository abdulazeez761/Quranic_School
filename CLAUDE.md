# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Hafiz** is an ASP.NET Core 8 web application for managing Quranic learning centers (Madaris and Halaqa). It connects administrators, teachers, students, and parents in one platform to track memorization, review, attendance, and daily assignments (Wird).

- **Architecture**: Clean Architecture with clear layer separation
- **Tech Stack**: ASP.NET Core 8 MVC, Entity Framework Core 9, SQL Server
- **Frontend**: Razor Views + Bootstrap 5 + JavaScript
- **Localization**: Supports Arabic and English (via .resx resource files)
- **Deployment**: Can run on Windows (IIS) or Linux (systemd + nginx)

## Architecture

The project is organized in Clean Architecture with four layers:

```
Hafiz.Web (Presentation)
  ↓ depends on
Hafiz.Application (Business Logic)
  ↓ depends on
Hafiz.Domain (Core Entities & Rules)
  ↑ implemented by
Hafiz.Infrastructure (Data Access & External Services)
```

### Layer Responsibilities

- **Hafiz.Domain**: Entity models (User, Student, Teacher, Parent, Class, StudentAttendance, TeacherAttendance, WirdAssignment, ParentNote, Video, Institute), Enums (UserRole, WirdStatus, etc.). No external dependencies.
- **Hafiz.Application**: Interfaces for repositories and services, DTOs, business logic services, localization resources, and dependency injection setup. Contains `IPasswordHasher`, `IAuthService`, `IDashboardService`, `IWirdService`, etc.
- **Hafiz.Infrastructure**: Implements repositories, `ApplicationDbContext` (EF Core DbContext), database migrations, and services for external integrations (backup, Google Drive upload).
- **Hafiz.Web**: Controllers organized by role areas (`/Admin`, `/Teacher`, `/Student`, `/Parent`), Razor views with localization, and wwwroot static files. Uses `DailyBackupService` as a hosted background service for scheduled database backups.

### Key Patterns

- **Repository Pattern**: `IXxxRepository` interfaces in `Hafiz.Application/Interfaces/Repositories/` with implementations in `Hafiz.Infrastructure/Repositories/`.
- **Service Pattern**: `IXxxService` interfaces with business logic implementations.
- **Localization**: `.resx` resource files in `Hafiz.Web/Resources/` and `Hafiz.Application/Resources/` mirror the view/DTO structure.
- **Dependency Injection**: Setup in extension methods (`AddApplication()`, `AddInfrastructure()`) in `Hafiz.Application/Extensions/` and `Hafiz.Infrastructure/Extensions/`.
- **Areas**: Role-based routes via ASP.NET Areas; shared admin and user layouts in `Views/Shared/`.

## Common Development Tasks

### Build the Solution

```powershell
dotnet build
```

### Run the Application

```powershell
cd src/Hafiz.Web
dotnet run
```

Then navigate to `https://localhost:5001` (or the port shown in console output).

### Database Migrations

Apply pending migrations:

```powershell
cd src/Hafiz.Web
dotnet ef database update
```

Create a new migration after model changes:

```powershell
cd src/Hafiz.Web
dotnet ef migrations add <MigrationName>
```

Remove the last migration (if not applied to production):

```powershell
cd src/Hafiz.Web
dotnet ef migrations remove
```

### Default Credentials

A SuperAdmin account is seeded automatically in `Program.cs` if none exists:
- **Username**: `superadmin`
- **Password**: `SuperAdmin123!`

Check `Program.cs:38-52` for the seeding logic.

## Database & Backup System

The application includes a Google Drive backup feature:

- **Manual backup**: Admin → Backup button in SuperAdmin area.
- **Automatic backup**: Daily at 02:00 UTC (configurable in `appsettings.json`: `Backup:DailyTime`).
- **Service**: `DailyBackupService` (registered in `Program.cs:23`).
- **Implementation**: `IBackupService` → `BackupService` + `GoogleDriveUploader`.

### Configuration Keys

In `appsettings.json`:

```json
{
  "Backup": {
    "LocalFolder": "C:\\SqlBackups",
    "DailyTime": "02:00",
    "UseDifferentImage": "true"
  },
  "GoogleDrive": {
    "CredentialsFilePath": "C:\\HafizSecrets\\credentials.json",
    "TokenFolder": "C:\\HafizSecrets\\drive-token",
    "TargetFolderId": "..."
  }
}
```

On production (Linux), override with `appsettings.Production.json`.

### OAuth Setup for Google Drive

One-time authorization required (uses `tools/DriveAuth/` CLI tool):

```powershell
dotnet run --project tools/DriveAuth -- "path/to/credentials.json" "path/to/token/folder"
```

This generates a refresh token stored locally; subsequent backups work without a browser.

## Important Files and Directories

| Path | Purpose |
|------|---------|
| `src/Hafiz.Web/Program.cs` | Application startup, DI registration, SuperAdmin seeding |
| `src/Hafiz.Web/appsettings.json` | DB connection, backup/Google Drive config, logging |
| `src/Hafiz.Infrastructure/Data/ApplicationDbContext.cs` | EF Core DbContext; defines all DbSets |
| `src/Hafiz.Web/Areas/{Admin,Teacher,Student,Parent}/` | Role-based controllers and views |
| `src/Hafiz.Web/Resources/` | Localization files (.resx for Arabic/English) |
| `src/Hafiz.Web/BackgroundServices/DailyBackupService.cs` | Scheduled backup task |

## Localization

The app supports Arabic (ar) and English (en). Localization files are `.resx` XML resources organized by view/DTO path:

- `Resources/Views/Auth/Login.ar.resx` ← Arabic translations
- `Resources/Views/Auth/Login.en.resx` ← English translations

To add translations:
1. Create matching `.ar.resx` and `.en.resx` files in the same directory.
2. Reference keys in Razor via `@Html.GetLocalizedValue("KeyName")` or in C# via injected `IStringLocalizer`.

## Performance Considerations

- **Slow Load Reports**: If dashboard or report pages load slowly, check:
  - Database query complexity in services (look for N+1 queries).
  - Missing `.Include()` calls in EF repository queries.
  - Large result sets being loaded into memory before filtering.
  - Use `AsNoTracking()` for read-only queries in repositories.

- **Current Branch**: `bug/fixing-slow-load-reports` suggests recent work on performance; check recent commits for context.

## Testing

No dedicated test project exists yet. For future test additions, consider:
- Unit tests in `Hafiz.Application` layer for services.
- Integration tests using an in-memory or test database context.
- Mocking repositories and external services (e.g., Google Drive uploader).

## Connection String

The default connection string in `appsettings.json` points to a remote SQL Server. For local development, update `ConnectionStrings:DefaultConnection` to:

```
Server=(local);Database=QuranSchoolDB;User Id=sa;Password=<your-password>;TrustServerCertificate=True;
```

## Useful EF Core Commands

```powershell
# List pending migrations
dotnet ef migrations list

# Show SQL for pending migrations
dotnet ef migrations script PreviousMigration CurrentMigration

# Revert all migrations (destructive)
dotnet ef database update 0
```

## Key Extensions and Helpers

Check these files for utility methods:
- `Hafiz.Application/Extensions/` — DI and service setup.
- `Hafiz.Infrastructure/Extensions/` — Additional DI and data helpers.
- `Hafiz.Web/Extensions/` — Web-specific DI and middleware setup.
- `Hafiz.Application/Common/Helper/` — Shared business logic utilities.

## Development Notes

- The application uses **Serilog** for logging (see `Program.cs` and `appsettings.json`).
- **Bootstrap 5** CSS framework is included in `wwwroot/lib/`.
- **jQuery** and **jQuery Validation** are available for client-side form validation.
- Views use **Razor syntax** with support for async operations.
- **Rate limiting** middleware is configured (see `Program.cs` for setup).
- **Authentication** is cookie-based via `CookieAuthenticationOptions`.
