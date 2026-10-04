# TaskFlow API

A RESTful task-management backend built with **ASP.NET Core 8**, **Entity Framework Core**, **SQL Server** and **JWT authentication**.

## Features
- User registration and login with JWT bearer tokens
- Password hashing with ASP.NET Core Identity's `PasswordHasher`
- Full CRUD for tasks, scoped per user (users can only access their own data)
- Filtering (status, priority), title search and pagination
- Layered structure: Controllers -> Services -> EF Core DbContext, with DTOs
- Data-annotation validation and centralized exception handling (RFC 7807 `ProblemDetails`)
- Swagger UI with JWT "Authorize" support

## Tech stack
C# 12, .NET 8, ASP.NET Core Web API, EF Core 8, Microsoft SQL Server, JWT, Swagger/OpenAPI

## Getting started

**Requirements:** .NET 8 SDK, SQL Server (LocalDB comes with Visual Studio)

```bash
cd src/TaskFlow.Api

# 1. Set a real JWT secret (min 32 chars) without committing it
dotnet user-secrets init
dotnet user-secrets set "Jwt:Key" "your-long-random-secret-at-least-32-characters"

# 2. Create the database
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate
dotnet ef database update

# 3. Run
dotnet run
```

Open `https://localhost:<port>/swagger` to try the API.

## Endpoints

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| POST | `/api/auth/register` | No | Create account, returns JWT |
| POST | `/api/auth/login` | No | Log in, returns JWT |
| GET | `/api/tasks?status=&priority=&search=&page=&pageSize=` | Yes | List my tasks (paged) |
| GET | `/api/tasks/{id}` | Yes | Get one task |
| POST | `/api/tasks` | Yes | Create task |
| PUT | `/api/tasks/{id}` | Yes | Update task |
| DELETE | `/api/tasks/{id}` | Yes | Delete task |

## Project structure
```
src/TaskFlow.Api
├── Controllers/   HTTP endpoints
├── Services/      Business logic (auth, tokens, tasks)
├── Data/          EF Core DbContext and configuration
├── Models/        Entities
├── DTOs/          Request/response contracts
├── Middleware/    Global exception handling
├── Exceptions/    Custom exception types
└── Settings/      Strongly-typed configuration
```
