# asp-auth-boilerplate

A controller-based ASP.NET Core 10 Web API implementing JWT authentication over a PostgreSQL database. Uses EF Core for data access, BCrypt for password hashing, and a scoped service layer with interface-driven DI.

---

## Tech Stack

- **Runtime**: .NET 10 (`net10.0`)
- **Framework**: ASP.NET Core 10 Web API (controller-based, no Minimal API)
- **ORM**: Entity Framework Core 10 (`Microsoft.EntityFrameworkCore`)
- **Database**: PostgreSQL via `Npgsql.EntityFrameworkCore.PostgreSQL` v10.0.3
- **Authentication**: JWT Bearer — `Microsoft.AspNetCore.Authentication.JwtBearer` v10.0.11
- **Password Hashing**: `BCrypt.Net-Next` v4.2.0 (HMAC-SHA256 signing)
- **EF Core Design-time tools**: `Microsoft.EntityFrameworkCore.Design` v10.0.11 (private asset)

---

## Project Architecture

```
backend/
├── Program.cs                          # DI registration, JWT middleware pipeline, app entry point
├── backend.csproj                      # SDK project file; NuGet package references
├── appsettings.json                    # Base config: JWT secret, issuer, audience placeholders
├── appsettings.Development.json        # Dev overrides: PostgreSQL connection string
│
├── Controllers/
│   ├── AuthController.cs               # POST /api/auth/register, POST /api/auth/login
│   └── HealthController.cs             # GET /health (DB ping), GET / (root probe)
│
├── Services/
│   ├── IAuthService.cs                 # Auth service contract: RegisterAsync, LoginAsync
│   └── AuthService.cs                  # BCrypt hash/verify, EF Core user lookup, JWT issuance
│
├── DTOs/
│   ├── RegisterDto.cs                  # Inbound: FirstName, LastName, Email, Password (validated)
│   ├── LoginDto.cs                     # Inbound: Email, Password
│   └── UserResponseDto.cs              # Outbound: Id, FirstName, LastName, Email (no hash)
│
├── Models/
│   └── User.cs                         # EF Core entity: Id, FirstName, LastName, Email, PasswordHash
│
├── Data/
│   └── AppDbContext.cs                 # DbContext with DbSet<User>; options injected via DI
│
└── Migrations/
    ├── 20260829130951_InitialCreate.cs          # Initial schema: Users table
    ├── 20260829130951_InitialCreate.Designer.cs # EF Core migration snapshot metadata
    └── AppDbContextModelSnapshot.cs             # Current model snapshot for migration diffing
```

---

## Prerequisites

| Tool | Purpose |
|---|---|
| [.NET 10 SDK](https://dotnet.microsoft.com/download) | Build and run the API |
| [Docker](https://docs.docker.com/get-docker/) + Docker Compose | Run PostgreSQL locally |
| `dotnet-ef` CLI tool | Apply EF Core migrations |

Install the EF Core CLI tool globally if not already present:

```bash
dotnet tool install --global dotnet-ef
```

---

## Getting Started

### 1. Start PostgreSQL

No `docker-compose.yml` is committed. Create one at the project root matching the connection string in `appsettings.Development.json`:

```yaml
# docker-compose.yml
services:
  postgres:
    image: postgres:16-alpine
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: mypassword
      POSTGRES_DB: authdb
    ports:
      - "5432:5432"
```

Then start the container:

```bash
docker compose up -d
```

### 2. Configure JWT Settings

`appsettings.json` ships with a placeholder secret. Override it before running — either directly in the file or via environment variable / user secrets:

```json
"JwtSettings": {
  "Secret": "CHANGE_ME_TO_A_32+_CHARACTER_SECRET_KEY!",
  "Issuer": "backend",
  "Audience": "backend-clients"
}
```

> **Note**: The token expiry is hardcoded to **2 hours** in `AuthService.cs`. Tokens are signed with **HMAC-SHA256**.

### 3. Apply EF Core Migrations

```bash
dotnet ef database update
```

This runs `20260829130951_InitialCreate` and creates the `Users` table in `authdb`.

### 4. Run the API

```bash
# Hot-reload (development)
dotnet watch

# Or standard run
dotnet run
```

The API listens on the default Kestrel port (typically `http://localhost:5000` / `https://localhost:5001`).

---

## API Endpoints

| Method | Route | Auth | Payload DTO | Response Codes |
|--------|-------|------|-------------|----------------|
| `GET` | `/` | None | — | `200 OK` |
| `GET` | `/health` | None | — | `200 OK` |
| `POST` | `/api/auth/register` | None | `RegisterDto` | `201 Created` · `409 Conflict` (email taken) · `400 Bad Request` (validation) |
| `POST` | `/api/auth/login` | None | `LoginDto` | `200 OK` (returns `{ success, token }`) · `401 Unauthorized` (bad credentials) |

### DTO Shapes

**`RegisterDto`** — all fields required:
```json
{
  "firstName": "Jane",
  "lastName": "Doe",
  "email": "jane@example.com",
  "password": "secret123"
}
```
> Password must be **>= 6 characters** (`[MinLength(6)]`).

**`LoginDto`**:
```json
{
  "email": "jane@example.com",
  "password": "secret123"
}
```

**Successful login response**:
```json
{
  "success": true,
  "token": "<signed-jwt>"
}
```

**Successful register response** (`201 Created`):
```json
{
  "id": 1,
  "firstName": "Jane",
  "lastName": "Doe",
  "email": "jane@example.com"
}
```
