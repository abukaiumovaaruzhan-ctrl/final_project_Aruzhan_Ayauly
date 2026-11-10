# StayEase — Hotel Booking System

Starter solution for .NET 8 / Blazor Web App.

## Projects
- `StayEase.Web` — Blazor user interface
- `StayEase.Domain` — core entities and enums
- `StayEase.Application` — application contracts and services
- `StayEase.Infrastructure` — data-access implementation (to be added)
- `StayEase.Tests` — automated tests (to be added)

## Run
```bash
dotnet restore
dotnet build StayEase.sln
dotnet run --project StayEase.Web
```

Open the local HTTPS/HTTP URL printed by the terminal.

Payment is intended to be simulated for this student project. Do not enter real card details.
