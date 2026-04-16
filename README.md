# 🎬 FlickList

**Track your movie collection across physical and digital formats.**

Search [TMDB’s](https://www.themoviedb.org/) catalog, tag what you own — DVD, Blu-ray, 4K, or digital — and see your full library at a glance.


🔗 **[Live Demo](https://brave-mud-06345141e.2.azurestaticapps.net/)** · *First request may take ~60s (Azure free-tier cold start)*

-----

## Architecture & Design Decisions

This isn’t a tutorial project — it’s a deliberately architected system built to demonstrate real-world engineering patterns.

```
┌─────────────────────────────────────────────────────┐
│  Clients (Angular · React · Vue · Blazor)           │
├─────────────────────────────────────────────────────┤
│  API Layer          Minimal APIs + 1 Controller     │
│  Application        MediatR · CQRS-style handlers   │
│  Domain             Entities · Value Objects        │
│  Infrastructure     EF Core · SQLite · Redis · TMDB │
└─────────────────────────────────────────────────────┘
```

**Key choices and why:**

- **Clean Architecture** — strict dependency inversion; inner layers have zero knowledge of infrastructure.
- **Minimal APIs as default, one controller-based endpoint** — shows fluency in both; Minimal APIs are the idiomatic path for new .NET projects.
- **MediatR for request dispatch** — decouples handlers from transport; planned migration to a custom pipeline to demonstrate going beyond the library.
- **Redis for TMDB response caching** — avoids hammering a third-party API and keeps search snappy.
- **SQLite for local dev, PostgreSQL planned for prod** — right tool for the phase; migration path is trivial with EF Core.

-----

## Tech Stack

|Layer   |Technology                                    |
|--------|----------------------------------------------|
|API     |.NET 10 · Minimal APIs · MediatR · Serilog    |
|Data    |EF Core · SQLite · Redis Cloud                |
|Docs    |Scalar (OpenAPI)                              |
|Frontend|Angular v21 (MVP)                             |
|Hosting |Azure App Service + Static Web Apps           |
|CI/CD   |GitHub Actions → auto-deploy on push to `main`|

**Supported formats:** DVD · Blu-ray · 4K Blu-ray

**Supported digital retailers:** Apple TV · Movies Anywhere · Fandango at Home · YouTube · Amazon Prime Video

-----

## Roadmap

|Feature                                         |Status   |
|------------------------------------------------|---------|
|Core API with auth, search, collection CRUD     |✅ Done   |
|Rate limiting                                   |✅ Done   |
|Centralized exception handling                  |✅ Done   |
|Structured logging (Serilog)                    |✅ Done   |
|Scalar / OpenAPI documentation                  |✅ Done   |
|Redis caching for TMDB responses                |✅ Done   |
|Angular client (MVP)                            |✅ Done   |
|CI/CD via GitHub Actions                        |✅ Done   |
|Replace MediatR with custom request pipeline    |🔧 Planned|
|React client                                    |🔧 Planned|
|Vue client                                      |🔧 Planned|
|Blazor client                                   |🔧 Planned|
|Email service (account creation, password reset)|🔧 Planned|
|PostgreSQL migration                            |🔧 Planned|
|OpenTelemetry observability                     |🔧 Planned|
|Docker containerization                         |🔧 Planned|

-----

## Getting Started

### Prerequisites

- .NET 10 SDK
- EF Core CLI (`dotnet tool install --global dotnet-ef`)
- [TMDB API Access Token](https://www.themoviedb.org/settings/api)
- [JWT Signing Token](https://jwtsecretkeygenerator.com/) (256-bit)
- [Redis Cloud Account](https://redis.io/try-free/) (for caching TMDB API responses)

### Setup

1. **Clone and configure:**

```bash
git clone https://github.com/nrobinson-dev/my-movies-app.git
cd my-movies-app
```

Copy `appsettings.Example.json` → `appsettings.json`, then fill in `TmdbSettings:BearerToken` and `JwtSettings:SigningKey`.
1. **Create and seed the database:**

```bash
# Install EF Core CLI
dotnet tool install --global dotnet-ef
   
# Create database migration
dotnet ef migrations add InitialCreate \
  --project src/backend/MyMoviesApp.Infrastructure/MyMoviesApp.Infrastructure.csproj \
  --startup-project src/backend/MyMoviesApp.Api/MyMoviesApp.Api.csproj \
  --output-dir Data/Migrations

# Apply database migration
dotnet ef database update \
  --project src/backend/MyMoviesApp.Infrastructure \
  --startup-project src/backend/MyMoviesApp.Api
```
1. **Run:**

```bash
dotnet run --project src/backend/MyMoviesApp.Api
```

API docs available at `https://localhost:7184/scalar/v1`.

-----

## API at a Glance

Full interactive docs via [Scalar](https://localhost:7184/scalar/v1) when running locally. Here’s the surface:

|Method  |Endpoint                             |Auth|Description           |
|--------|-------------------------------------|----|----------------------|
|`POST`  |`/api/auth/create`                   |—   |Create account        |
|`POST`  |`/api/auth/login`                    |—   |Login → JWT           |
|`POST`  |`/api/auth/delete/{userId}`          |🔒   |Delete own account    |
|`GET`   |`/api/movies?search=&page=&userId=`  |🔒   |Search TMDB           |
|`GET`   |`/api/users/{userId}/movies`         |🔒   |List collection       |
|`GET`   |`/api/users/{userId}/movies/{tmdbId}`|🔒   |Get movie detail      |
|`POST`  |`/api/users/{userId}/movies`         |🔒   |Add to collection     |
|`DELETE`|`/api/users/{userId}/movies/{tmdbId}`|🔒   |Remove from collection|

-----

## Deployment

Deployed on **Azure** via **GitHub Actions** — push to `main` triggers build and deploy automatically.

- **API:** Azure App Service (.NET 10, free tier)
- **Angular Client:** Azure Static Web Apps

See [`.github/workflows/`](.github/workflows/) for pipeline configuration.

-----

## License

[MIT](LICENSE)