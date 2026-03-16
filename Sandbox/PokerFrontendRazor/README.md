# Poker API Frontend (Razor Pages)

This is the ASP.NET Core Razor Pages frontend for the PokerAPI.

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- The backend `PokerAPI` running on `http://localhost:5175`

## Setup

1.  Navigate to the project directory:
    ```bash
    cd BootcampFormulatrix/BootcampFormulatrix/PokerFrontendRazor
    ```

2.  Run the application:
    ```bash
    dotnet run
    ```

3.  Open your browser and navigate to the URL shown (usually `http://localhost:5xxx`).

## Architecture

-   **Framework**: ASP.NET Core 8 Razor Pages
-   **State**: `HttpContext.Session` for player identity
-   **Real-time**: SignalR JavaScript Client (`poker.js`)
-   **Styling**: Bootstrap 5 + Custom CSS (`poker.css`)

## Pages

-   **Index**: Player registration.
-   **Table**: Main poker table with visual seats, cards, and game controls.
-   **Winner**: Displays showdown results passed via query parameters.
