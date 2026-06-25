# ⚽ World Cup Simulator - Backend

Backend API built with .NET 9 for a World Cup tournament simulator, providing match simulation, team ratings, and bracket management.

---

## 🏆 Overview

This API is responsible for:

- Managing national teams and their data
- Storing and processing historical matches
- Simulating World Cup tournaments (group stage and knockouts)
- Calculating team ratings using Poisson distribution
- Managing tournament brackets and third-place team assignments

---

## 🚀 Tech Stack

- **.NET 9** (Web API)
- **Entity Framework Core** with PostgreSQL
- **xUnit + FluentAssertions + Moq** (Unit Testing)
- **Docker & Docker Compose**

---

## 📦 Installation

Clone the repository:

```bash
git clone https://github.com/your-username/world-cup-simulator.git
cd world-cup-simulator
```

---

## 🏠 Option A — Run locally:

### Prerequisites

- .NET 9 SDK
- PostgreSQL database

### Restore dependencies:

```bash
dotnet restore WorldCupSimulatorApp/WorldCupSimulatorApp.sln
```

### Configure settings:

Edit `WorldCupSimulatorApp/WCS.Api/appsettings.json`:

```json
{
  "ConnectionString": {
    "EFCoreDBConnection": "Host=localhost;Database=WorldCupDB;Username=your-user;Password=your-pass"
  },
  "FrontendUrl": {
    "Url": "http://localhost:3000"
  },
  "AdminApiKey": {
    "ApiKey": "your-secure-api-key"
  }
}
```

### Run the application:

```bash
dotnet run --project WorldCupSimulatorApp/WCS.Api/WCS.Api.csproj
```

The API will be available at `https://localhost:7001` (or `http://localhost:5001`).

---

## 🐳 Option B — Run with Docker Compose (Recommended):

This option automatically sets up both the PostgreSQL database and the backend API.

### 1. Configure environment variables:

Copy the example environment file:

```bash
cp .env_example .env
```

Edit `.env` with your values:

```env
DB_USER=postgres
DB_PASSWORD=your_secure_password
DB_NAME=worldcupdb

ConnectionString__EFCoreDBConnection=Host=postgres;Database=${DB_NAME};Username=${DB_USER};Password=${DB_PASSWORD}

FrontendUrl__Url=http://localhost:3000

AdminApiKey__ApiKey=your-secure-admin-api-key
```

### 2. Run with Docker Compose:

```bash
docker compose up --build
```

This command will:
- Build the backend API Docker image
- Start a PostgreSQL 15 container
- Wait for the database to be healthy
- Start the backend API container
- Apply EF Core migrations automatically
- Seed the database with CSV data (teams, historical matches, World Cup fixtures)

### 3. Access the API:

- **API**: http://localhost:8000
- **Swagger UI** (Development): http://localhost:8000/swagger

### 4. Stop the services:

```bash
# Stop containers (data persists)
docker compose down

# Stop and remove all data (including database)
docker compose down -v
```

---

## 🔌 Configuration

### Required settings:

| Setting | Description | Location |
|---------|-------------|----------|
| `ConnectionString:EFCoreDBConnection` | PostgreSQL connection string | `appsettings.json` or `.env` |
| `FrontendUrl:Url` | Frontend URL for CORS | `appsettings.json` or `.env` |
| `AdminApiKey:ApiKey` | API key for admin endpoints | `appsettings.json` or `.env` |

### Rating Weights (optional):

The simulation uses configurable weights for different competitions and stages. Edit `appsettings.json`:

```json
{
  "RatingWeights": {
    "Competition": {
      "Friendly": 0.85,
      "Qualifier": 1.10,
      "ContinentalCup": 1.20,
      "WorldCup": 1.40
    },
    "Stage": {
      "Group": 1.00,
      "RoundOf16": 1.10,
      "QuarterFinal": 1.15,
      "SemiFinal": 1.25,
      "Final": 1.30
    }
  }
}
```

---

## 🗄️ Database

This project uses Entity Framework Core with PostgreSQL.

### Automatic migrations:

- When running with Docker Compose, migrations are applied automatically on startup
- When running locally, use: `dotnet ef database update`

### Data seeding:

The application automatically seeds the database from CSV files on startup:

- `NationalTeams.csv` - National teams with FIFA rankings
- `HistoricalMatches.csv` - Historical match results for rating calculations
- `WorldCupTeams.csv` - World Cup tournament participants
- `WorldCupMatches.csv` - Group stage fixtures

### Manual migrations (local development):

```bash
dotnet ef database update --startup-project WorldCupSimulatorApp/WCS.Api --project WorldCupSimulatorApp/WCS.Infrastructure
```

---

## ⚽ Features

### Simulation Types

The API supports two simulation modes:

1. **Outcome-Based**: Simulates matches returning only win/loss/draw results
2. **Score-Based**: Simulates matches with actual goal scores (using Poisson distribution, capped at 6 goals)

### Tournament Flow

1. **Group Stage**: 12 groups of 4 teams each
   - Each team plays 3 matches
   - Points: 3 for win, 1 for draw, 0 for loss
   - Top 2 teams from each group advance
   - 8 best third-place teams advance

2. **Knockout Stage**: Round of 32 → Round of 16 → Quarter Finals → Semi Finals → Final
   - Single elimination
   - Automatic bracket progression

### Rating System

- **Attack Rating**: Based on historical goal-scoring performance
- **Defense Rating**: Based on historical goals conceded
- **Calculation**: Uses weighted averages considering competition importance and match stage

### Rate Limiting

API requests are limited to 30 per minute per client.

---

## 📡 API Endpoints

### World Cup Teams

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/worldcupteams/groups` | Get all groups with teams |

### World Cup Matches

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/worldcupmatches` | Get all group stage matches |
| GET | `/api/worldcupmatches/group/{groupCode}` | Get matches by group (A-L) |
| POST | `/api/worldcupmatches/third-places` | Assign third-place teams to bracket slots |

### World Cup Finals

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/worldcupfinals` | Get all finals matches |
| GET | `/api/worldcupfinals/simulation` | Get finals matches for simulation |

### Simulators

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/simulators/groups?type={type}` | Simulate group stage |
| POST | `/api/simulators/knockouts/simple?type={type}` | Simulate knockout round (simple) |
| POST | `/api/simulators/knockouts/adaptive` | Simulate knockout round (adaptive with history) |

**Query Parameters:**
- `type`: `0` for OutcomeBased, `1` for ScoreBased

### Admin (Protected)

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/admin/finals-matches` | Insert finals matches |
| PUT | `/api/admin/finals-matches` | Update finals match scores |
| PUT | `/api/admin/group-matches` | Update group match scores |
| PUT | `/api/admin/ratings` | Update team ratings |
| PUT | `/api/admin/worldcup-teams` | Update World Cup team points |

**Note:** Admin endpoints require `API-Key` header with the configured admin API key.

---

## 🧪 Testing

Unit tests are implemented for core business logic (ratings, probabilities, simulations).

- **Framework**: xUnit
- **Assertions**: FluentAssertions
- **Mocking**: Moq

To run tests:

```bash
dotnet test WorldCupSimulatorApp/WCS.Tests/WCS.Tests.csproj
```

---

## 📁 Project Structure

The project follows Clean Architecture principles:

```
WorldCupSimulatorApp/
├── WCS.Api/                 # Presentation layer (controllers, config)
├── WCS.Application/         # Business logic (services, DTOs, mappers)
├── WCS.Domain/              # Core domain (entities, enums)
└── WCS.Infrastructure/      # External integrations (database, repositories, seeds)
```

### Layer Responsibilities:

- **WCS.Api**: HTTP requests, routing, middleware (CORS, rate limiting, Swagger)
- **WCS.Application**: Business logic, use cases, DTOs, simulation algorithms
- **WCS.Domain**: Core entities, enums, domain logic
- **WCS.Infrastructure**: Database context, repositories, CSV seeding

### Key Services:

- `ISimulationService`: Match simulation algorithms
- `IRatingService`: Team rating calculations
- `IMatchProbabilityService`: Match outcome probabilities
- `IGroupStageService`: Group stage logic and standings
- `IKnockoutsService`: Knockout bracket management

---

## 🚀 Project Status

🟡 In Progress - Active development with regular updates

---

