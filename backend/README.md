# DriveShare Backend

This is the backend API for the DriveShare application, built with ASP.NET Core.

## Prerequisites

- .NET 7.0 SDK or later
- PostgreSQL (for database)
- Docker (optional, for containerization)

## Getting Started

1. Clone the repository
2. Update the connection string in `appsettings.json` to match your PostgreSQL configuration
3. Run the following commands:

```bash
cd DriveShare.API
dotnet restore
dotnet run
```

The API will be available at `https://localhost:5001` or `http://localhost:5000`.

## API Documentation

Once the application is running, you can access the Swagger documentation at:

- `https://localhost:5001/swagger` (HTTPS)
- `http://localhost:5000/swagger` (HTTP)

## Health Check

A health check endpoint is available at `/health` to verify the API is running.

## Development

- The project uses minimal APIs for a cleaner and more maintainable codebase
- CORS is enabled for development purposes
- JWT authentication is configured but needs to be implemented
- PostgreSQL is used as the database
