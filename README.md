# StayEase — Hotel Booking System

Starter implementation using .NET 8 Blazor Server, Clean Architecture, EF Core 8 and SQLite.

## Requirements
- .NET 8 SDK

## Run
```bash
dotnet restore
dotnet run --project src/StayEase.Web
```
Open the local URL printed in the terminal. The SQLite database is created and seeded on first run.

## Test
```bash
dotnet test
```

## Notes
- This is a self-contained starter project based on the entity/property assumptions from the conversation.
- Demo user identity is currently represented by `CustomerId = 1`; replace it with ASP.NET Core Identity before production.
- Admin pages are not protected by authentication/authorization yet.
- Availability is checked in application code; production booking requires transactional/concurrency protection.
