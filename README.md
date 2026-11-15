# StayEase — Hotel Booking System

A hotel booking web application built with **C# (.NET 8)** and **ASP.NET Core**, with a front end written in plain **HTML, CSS and JavaScript**.

This project was created for the *Introduction to Functional Programming* course. The business logic is written in a functional style: immutable data, pure functions and no hidden side effects in the domain rules.

## Features

- Browse hotels and search by hotel name or city
- View hotel details, room types, prices and capacity
- Book a room by choosing check-in and check-out dates
- Automatic price calculation: price per night × nights, with a **10% discount for stays of 7 nights or more**
- Protection against double booking (overlapping dates for the same room)
- "My bookings" page with the ability to cancel a booking
- Admin dashboard: revenue and statistics, add/delete hotels and rooms, change booking status

## Tech Stack

| Layer | Technology |
| --- | --- |
| Back end | C#, .NET 8, ASP.NET Core Minimal API |
| Front end | HTML, CSS, JavaScript (ES modules) |
| Tests | xUnit |
| Storage | In-memory (resets when the server restarts) |

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- A code editor such as VS Code or Visual Studio

### Run the application

```bash
git clone https://github.com/<your-username>/<your-repo>.git
cd <your-repo>
dotnet run --project StayEase
```

Then open **http://localhost:5080** in your browser.

To stop the server, press `Ctrl+C` in the terminal.

### Run the tests

```bash
dotnet test
```

## Project Structure

```
.
├── StayEase/                  # Web application
│   ├── Domain/
│   │   ├── Models.cs          # Immutable record types (Hotel, Room, Booking, DTOs)
│   │   ├── Pricing.cs         # Pure functions: price, nights, date overlap
│   │   └── BookingService.cs  # Business rules and in-memory data
│   ├── wwwroot/               # HTML pages, CSS, JavaScript, images
│   └── Program.cs             # REST API endpoints
├── StayEase.Tests/            # xUnit tests
└── StayEase.sln
```

## Functional Programming Concepts

- **Immutable data** — domain types are C# `record`s, which cannot be changed after creation.
- **Pure functions** — `Pricing.CalculatePrice`, `Pricing.Nights` and `Pricing.Overlaps` depend only on their arguments and have no side effects, which makes them easy to test.
- **Non-destructive updates** — instead of modifying objects, the service creates new ones using the `with` expression (for example `booking with { Status = status }`).
- **Declarative data processing** — LINQ (`Where`, `Select`, `Any`, `Sum`) is used instead of manual loops.

## REST API

| Method | Endpoint | Description |
| --- | --- | --- |
| GET | `/api/hotels` | List hotels with the lowest room price |
| GET | `/api/hotels/{id}` | Hotel details with its rooms |
| POST | `/api/hotels` | Add a hotel |
| DELETE | `/api/hotels/{id}` | Delete a hotel (not allowed with active bookings) |
| POST | `/api/rooms` | Add a room |
| DELETE | `/api/rooms/{id}` | Delete a room (not allowed with active bookings) |
| GET | `/api/bookings?customerId=1` | List bookings, optionally for one customer |
| POST | `/api/bookings` | Create a booking |
| PUT | `/api/bookings/{id}/status` | Change booking status (`Pending`, `Confirmed`, `Cancelled`) |
| GET | `/api/stats` | Revenue, bookings, hotels and rooms count |

Business rule violations return `400 Bad Request` with a JSON body such as `{ "error": "This room is already booked for those dates." }`.

## Notes

- Data is stored in memory, so it returns to the initial seed data after the server restarts.
- All bookings in the demo belong to a single hard-coded customer (`customerId = 1`); there is no authentication.
- The admin page is open to everyone and is intended for demonstration only.

## Authors

Ayauly Koralasbaeva
Aruzhan Abdukaium
