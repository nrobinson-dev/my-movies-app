## Getting Started with the Backend

### Prerequisites

- [TMDB API Access Token](https://www.themoviedb.org/settings/api)
- [JWT Signing Token](https://jwtsecretkeygenerator.com/) (256 for testing authenticated endpoints)
- [Redis Cloud Account](https://redis.io/try-free/) (for caching TMDB API responses)
- .NET 10 CLI
- Entity Framework Core CLI

### Setup

1. **Clone the repository.**

2. **Configure application settings.**

   a. Open `appsettings.Example.json`, add your TMDB Bearer Token under `TmdbSettings:BearerToken`.
   
   b. Add your JWT signing token under `JwtSettings:SigningKey`.

   c. Rename the file to `appsettings.json`.
3. **Set up the database.**

   From the root directory of the repository, run:

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

   This will create the SQLite database.
4. **Run the application.**

   From the root directory of the repository, run:

   ```bash
   dotnet run --project src/backend/MyMoviesApp.Api
   ```

---

## Deployment

This project is deployed on **Azure** using **GitHub Actions** for CI/CD:

- **Web API:** Azure App Service (.NET 10 runtime, free tier)
- **Angular Client:** Azure Static Web Apps
- **CI/CD Pipeline:** GitHub Actions automatically builds and deploys on push to `main`

See `.github/workflows/` for the workflow configuration.

---

## API Reference

Interactive API documentation is available via **Scalar** when running in development:

```
https://localhost:7184/scalar/v1
```

Endpoints that require authentication show a 🔒 lock icon in Scalar. Click **Authorize** and paste a JWT access token obtained from the `/api/auth/login` endpoint to test them directly in the UI.

You can also use a tool such as [Postman](https://www.postman.com/), [Insomnia](https://insomnia.rest/), or `curl` to test the endpoints.

**Base URL**
```
https://localhost:7184/api
```

---

### Authentication

#### Create Account

```
POST /auth/create
```

**Headers**
```
Accept: application/json
```

**Request Body**
```json
{
  "Email": "name@email.com",
  "Password": "Password"
}
```

**Response**
```json
{
  "userId": "{guid}",
  "token": "{token}", 
  "expiration": "2026-01-01T00:00:00Z"
}
```

**Status Codes**
- `201 Created` — account successfully created
- `400 Bad Request` — invalid request body
- `409 Conflict` — account with email already exists

---

#### Login

```
POST /auth/login
```

**Headers**
```
Accept: application/json
```

**Request Body**
```json
{
  "Email": "name@email.com",
  "Password": "Password"
}
```

**Response**
```json
{
  "userId": "{guid}",
  "token": "{token}", 
  "expiration": "2026-01-01T00:00:00Z"
}
```

**Status Codes**
- `200 OK` — login successful
- `400 Bad Request` — invalid request body
- `401 Unauthorized` — invalid credentials
- `429 Too Many Requests` — rate limit exceeded

---

#### Delete User

```
POST /auth/delete/{userId}
```

**Response**
**Status Codes**
- `204 No Content` — don't reveal whether the user existed or not, just return success
- `401 Unauthorized` — invalid token
- `403 Forbidden` — user can only delete their own account

---

### Movies

#### Search Movies

```
GET /movies?search={movieName}&page={page}&userId={userId}
```

**Authorization:** `Bearer {token}`

**Response**
```json
{
  "movies": [
    {
      "tmdbId": 123,
      "title": "Movie Title",
      "releaseDate": "0001-01-01",
      "posterPath": "/asdf.jpg",
      "formats": [
        { "id": 1, "name": "Dvd" }
      ],
      "digitalRetailers": [
        { "id": 1, "name": "MoviesAnywhere" }
      ]
    }
  ],
  "page": 1, 
  "totalPages": 1,
  "totalResults": 1
}
```
**Status Codes**
- `200 OK` — login successful
- `401 Unauthorized` — invalid credentials
- `503 Service Unavailable` — TMDB API is down or rate limit exceeded

---

### User Movies

#### Get User Movie Ownership

```
GET /users/{userId}/movies?page={page}&pageSize={pageSize}
```

**Authorization:** `Bearer {token}`

**Response**
```json
{
  "movies": [
    {
      "tmdbId": 123,
      "title": "Movie Title",
      "releaseDate": "0001-01-01",
      "posterPath": "/asdf.jpg",
      "formats": [
        { "id": 1, "name": "Dvd" }
      ],
      "digitalRetailers": [
        { "id": 1, "name": "MoviesAnywhere" }
      ]
    }
  ],
  "totalDvdCount": 1,
  "totalBluRayCount": 0,
  "totalBluRay4KCount": 0,
  "totalDigitalCount": 1, 
  "page": 1, 
  "totalPages": 1,
  "totalResults": 1
}
```
**Status Codes**
- `200 OK` — login successful
- `401 Unauthorized` — invalid credentials
- `403 Forbidden` — user can only access their own collection

---

#### Get Movie

```
GET /users/{userId}/movies/{tmdbId}
```

**Authorization:** `Bearer {token}`

**Response**
```json
{
  "tmdbId": 123,
  "title": "Movie Title",
  "releaseDate": "0001-01-01",
  "runtime": 120,
  "posterPath": "/poster.jpg",
  "backdropPath": "/backdrop.jpg",
  "tagline": "Example tagline",
  "overview": "Movie description",
  "formats": [
    { "id": 1, "name": "Dvd" }
  ],
  "digitalRetailers": [
    { "id": 1, "name": "MoviesAnywhere" }
  ]
}
```
**Status Codes**
- `200 OK` — login successful
- `401 Unauthorized` — invalid credentials
- `403 Forbidden` — user can only access their own collection
- `503 Service Unavailable` — TMDB API is down or rate limit exceeded

---

#### Add Movie to Collection

```
POST /users/{userId}/movies
```

**Authorization:** `Bearer {token}`

**Request Body**
```json
{
  "tmdbId": 123,
  "title": "Movie Title",
  "releaseDate": "2024-01-01",
  "posterPath": "/poster.jpg",
  "formats": [1], // Dvd: 1, BluRay: 2, BluRay4K: 3
  "digitalRetailers": [1, 5] // MoviesAnywhere: 1, AppleTv: 2, FandangoAtHome: 3, YouTube: 4, AmazonPrime: 5
}
```

**Response:** `int`

**Status Codes**
- `200 OK` — login successful
- `401 Unauthorized` — invalid credentials
- `400 Bad Request` — invalid request body
- `403 Forbidden` — user can only modify their own collection

---

#### Delete Movie

```
DELETE /users/{userId}/movies/{tmdbId}
```

**Authorization:** `Bearer {token}`

**Status Codes**
- `204 No Content` — movie successfully deleted
- `401 Unauthorized` — invalid credentials
- `403 Forbidden` — user can only modify their own collection
