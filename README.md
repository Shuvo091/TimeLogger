# Skill Allocation Tracker — TimeLogger

A full-stack, Clean-Architecture web app for tracking weekly upskilling strategy and execution.  
Purpose: help you plan topic allocations, log study time, and measure execution vs plan with clear analytics — useful for personal growth, interviews preparation, and keeping consistent momentum.

## TL;DR
- Backend: .NET 10, Entity Framework Core, MSSQL Server, JWT auth, Serilog, FluentValidation
- Frontend: Server-rendered Razor/MVC views (cshtml) using Bootstrap 5 and Chart.js for analytics
- Architecture: Clean Architecture (Domain / Application / Infrastructure / API), Repository + UnitOfWork, DTOs, Global exception middleware
- Key features: Topic CRUD + percentage allocation (sum = 100), weekly config, time logs (UTC stored), dashboard & analytics, per-topic metrics, edit/delete timelogs inline
- Useful for showing in a portfolio: clear architecture, logging, authentication, analytics and a polished UI

---

## Tech stack (project-level)
- Runtime / Language: .NET 10, C# 14
- ORM / DB: Entity Framework Core, Microsoft SQL Server
- Authentication: JWT (symmetric signing in local/dev)
- UI: Razor views / MVC (server-rendered CSHTML), Bootstrap 5
- Charts: Chart.js
- Validation: FluentValidation
- Logging: Serilog (console; extendable to file/Seq)
- Design: Clean Architecture (Domain / Application / Infrastructure / API)
- Tools: `dotnet-ef` (migrations), `dotnet` CLI, Visual Studio 2026 (recommended)

---

## What it does (features)
- Topic Management (CRUD)
  - Add, edit, delete topics
  - Assign percentage allocation (1..100), prevent duplicates
  - Validation enforces total percentage = 100
- Weekly Time Configuration
  - Set Total Weekly Hours
  - CalculatedWeeklyHours = (Percentage / 100) * TotalWeeklyHours (computed)
  - Auto-updates across pages
- Time Logs
  - Log study sessions (topic, duration in minutes, note, date)
  - UTC timestamps stored, grouped by ISO week for analytics
  - Edit/delete timelogs from topic edit page (modal)
- Dashboard & Analytics
  - KPI cards: planned vs logged, completion %, most focused topic
  - Charts: allocation pie, planned vs actual bar, weekly trend line
  - Advanced analytics: efficiency score, under/over-performing detection, trends per topic
- UX
  - Bootstrap layout, responsive pages, modal UX for timelog editing
- Backend
  - Clean separation via repository & unit of work, FluentValidation, Serilog, global exception handling
- Extras
  - Seed data on first run
  - Swagger for API endpoints (when in Development)

---

## Repo layout (high-level)
- `TimeLogger/` (solution root)
  - `API/` — Program.cs, controllers for API, app host
  - `Infrastructure/` — EF `AppDbContext`, repositories, UnitOfWork
  - `Application/` — services, DTOs, validators
  - `Domain/` — entities (Topic, WeeklyConfig, TimeLog)
  - `Views/` — `Topics`, shared layout, partials (CSHTML)
  - `wwwroot/` — css/js/site assets
  - `Properties/launchSettings.json` — local launch profiles

Key files:
- `API/Program.cs` — app startup, DI, Serilog, JWT setup, routing (MVC + API)
- `Infrastructure/DbContexts/AppDbContext.cs` — EF model configuration
- `API/SeedData.cs` — initial topics & weekly config
- `Views/Topics/*` — Index, CreateEdit, AddTimeLog, Summary, AnalyticsDetailed, partials

---

## Quickstart — local development

Prereqs:
- .NET 10 SDK
- SQL Server (LocalDB, SQL Express, or a dev MSSQL instance)
- (optional) `dotnet-ef` CLI: `dotnet tool install --global dotnet-ef`  
- Visual Studio 2026 recommended for debugging and launch-profile support

1. Clone
   - git clone <repo-url>
   - cd TimeLogger

2. Configure connection string
   - Update `appsettings.Development.json` / `appsettings.json` (or `User Secrets`) `ConnectionStrings:DefaultConnection` to point to your SQL Server.

3. Restore & build
   - dotnet restore
   - dotnet build

4. Apply EF migrations & seed DB
   - If migrations are stored under `Infrastructure` project:
     - dotnet ef migrations add InitialCreate --project TimeLogger.Infrastructure --startup-project TimeLogger.API
     - dotnet ef database update --project TimeLogger.Infrastructure --startup-project TimeLogger.API
   - Or use Package Manager Console (set Default Project to Infrastructure):
     - Add-Migration InitialCreate
     - Update-Database
   - On first run the app also calls `SeedData.EnsureSeedData(...)` to insert example topics and weekly config.

5. Run
   - From VS: select the web project profile (IIS Express or Kestrel `https`) — launch URL can be set to `Topics` in `Properties/launchSettings.json`.
   - Or CLI:
     - cd TimeLogger/API
     - dotnet run
   - Visit:
     - MVC UI: `https://localhost:{port}/Topics`
     - API endpoints (Swagger in Development): `https://localhost:{port}/swagger`

6. Admin / auth
   - Local JWT secret: set `Jwt:Key` in configuration (User Secrets or environment variable) for production use store in KeyVault.
   - Auth endpoints can be added if you want user accounts; current scaffold uses JWT wiring for API.

---

## Useful commands (examples)
- Add migration:
  - dotnet ef migrations add init --project TimeLogger.Infrastructure --startup-project TimeLogger.API
- Update DB:
  - dotnet ef database update --project TimeLogger.Infrastructure --startup-project TimeLogger.API
- Run API project:
  - dotnet run --project TimeLogger/API
- Run tests (if present):
  - dotnet test

---

## Where to look (developer pointers)
- Topics logic & validation: `Application/Services/TopicService.cs` and `Application/Validators/TopicDtoValidator.cs`
- EF models: `Infrastructure/DbContexts/AppDbContext.cs`
- Views:
  - List: `Views/Topics/Index.cshtml`
  - Edit / timelogs: `Views/Topics/CreateEdit.cshtml`, `_EditTimeLogPartial.cshtml`
  - Analytics: `Views/Topics/AnalyticsDetailed.cshtml`
- Controller for UI: `TimeLogger\Controllers\TopicsController.cs` (must live in project with Views)
- API endpoints: `API/Controllers/*` (if you expose a separate API layer)
- Logging config: `appsettings.json` or `appsettings.Development.json` for Serilog sinks

---

## Design notes & constraints
- Percentage validation: the app enforces total topics' percentage must equal 100. You can change behavior (auto-balance / allow partial) in `TopicService`.
- Time storage: `TimeLog.LogDate` and `CreatedAt` are stored in UTC. Views convert to local time for display.
- ISO week number: grouping uses `CalendarWeekRule.FirstFourDayWeek` and Monday start to compute weekly aggregates.
- Charts: Chart.js canvases are wrapped with constrained containers and `maintainAspectRatio: false` for responsive layout.
- Security: store JWT secrets securely in production. Use HTTPS and proper token lifetimes.
- Performance: caching for heavy analytics endpoints is recommended (in-memory caching already considered).

---

## Personal note (for the README)
This project was created to help me build and sustain a disciplined upskilling habit. I wanted:
- A simple way to commit weekly study plans (topic + percent) and measure what actually happened.
- Clear metrics to spot where I under-invest or over-invest time.
- An artifact that demonstrates architecting a small SaaS-style app with Clean Architecture, EF Core, logging and analytics — useful in interviews and to track my own progress.

---

## Next improvements (roadmap)
- Authentication UX & multi-user support (leaderboard)
- PDF export of weekly reports
- Email / reminder system
- Heatmap calendar (daily intensity)
- Caching & rate limiting for analytics endpoints
- Unit & integration tests for key calculations (percentage validation, efficiency, weekly grouping)

---

## Contributing
- Fork, branch, create a PR.
- Keep changes aligned with the Clean Architecture layering.
- Add tests for business-critical logic.

---

## License
Add your preferred license (MIT recommended for personal projects). Example: `LICENSE` file with MIT.

---

If you want, I can:
- Create a `README.md` with badges and a small screenshot SVG for the repo.
- Add a CONTRIBUTING guide or a short video demo script for your portfolio.
Which would you like next?