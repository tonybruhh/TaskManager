# TaskManager API

A **Task Management REST API** built with **.NET 8 Minimal APIs**, **Entity Framework Core**, **PostgreSQL**, and **JWT Authentication**.  
The project demonstrates clean architecture, environment-based configuration, and production-ready practices.

---

## Features

- Authentication and authorization with ASP.NET Core Identity + JWT
- User accounts: registration, login, profile
- Task management (CRUD) with per-user data isolation
- Dockerized setup with `docker-compose` (API + PostgreSQL)
- Interactive API documentation via Swagger / OpenAPI
- Configuration through environment variables (no secrets in code)

---

## Getting Started

### 1. Clone the repository
```bash
git clone https://github.com/<your-username>/TaskManager.git
cd TaskManager
```

### 2. Configure environment variables
Copy the example file and adjust values:

```bash

cp .env.example .env
```

### 3. Run with Docker

```bash

docker compose -up -d --build
```
API will be available at http://localhost:8080/swagger

---

## API Overview

- Auth
  - POST /api/auth/register → Register a new user
  - POST /api/auth/login → Obtain a JWT token
  - GET /api/auth/me → Get current user (requires token)
- Tasks
  - POST /api/tasks → Create a task
  - GET /api/tasks → List user tasks
  - GET /api/tasks/{id} → Get task by ID
  - PUT /api/tasks/{id} → Update a task
  - DELETE /api/tasks/{id} → Delete a task

## Tech Stack

- Backend: ASP.NET Core 8 (Minimal APIs)
- Authentication: ASP.NET Identity + JWT
- Database: PostgreSQL with EF Core
- Containerization: Docker & docker-compose
- Documentation: Swagger / OpenAPI

## Roadmap

-Filtering and sorting tasks
-Task categories and tags
-Role-based access control
-CI/CD with GitHub Actions
-Frontend client (React/Next.js)

## License
MIT
