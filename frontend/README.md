# DriveShare Frontend

This is the frontend application for DriveShare, built with Blazor Web App.

## Prerequisites

- .NET 7.0 SDK or later
- Node.js (for npm packages)
- A running instance of the DriveShare backend API

## Getting Started

1. Clone the repository
2. Update the API base URL in `appsettings.json` if needed
3. Run the following commands:

```bash
cd DriveShare.Web
dotnet restore
dotnet run
```

The application will be available at `https://localhost:7001` or `http://localhost:5000`.

## Project Structure

- `Components/` - Contains Blazor components
- `wwwroot/` - Static files (CSS, JS, images)
- `Program.cs` - Application startup and configuration
- `appsettings.json` - Configuration settings

## Development

- The project uses Blazor Server for interactivity
- Bootstrap 5 is included for styling
- The application is configured to work with the DriveShare backend API
