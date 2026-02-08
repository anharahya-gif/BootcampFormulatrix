# PokerAPI

A real-time Multiplayer Poker game API built with **ASP.NET Core 8** and **SignalR**.

## Overview

PokerAPI provides the backend logic for a Texas Hold'em Poker game. It manages game state in-memory and communicates real-time updates to connected clients via SignalR. It supports basic poker actions like betting, calling, raising, checking, folding, and manages the game lifecycle from player registration to showdown.

## Features

-   **Real-time Updates**: Uses SignalR to broadcast game state changes instantly to all connected clients.
-   **In-Memory State**: Fast game state management (note: state is lost on restart).
-   **RESTful API**: Endpoints for player actions and game management.
-   **Swagger UI**: Integrated Swagger for easy API testing and exploration.

## Prerequisites

-   [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

## Getting Started

1.  **Clone the repository**:
    ```bash
    git clone <repository-url>
    cd BootcampFormulatrix/BootcampFormulatrix/PokerAPI
    ```

2.  **Restore dependencies**:
    ```bash
    dotnet restore
    ```

3.  **Run the application**:
    ```bash
    dotnet run
    ```

4.  **Access the API**:
    -   The application will typically start on `http://localhost:5148` (or similar, check console output).
    -   **Swagger UI**: Visit `http://localhost:5148/swagger` to explore and test the API endpoints.

## API Endpoints

### Player Management

| Method | Endpoint | Description |
| :--- | :--- | :--- |
| `POST` | `/api/GameControllerAPI/addPlayer` | Adds a new player with specified chips. |
| `POST` | `/api/GameControllerAPI/registerPlayer` | Registers a player (alternative to addPlayer). |
| `POST` | `/api/GameControllerAPI/joinSeat` | Assigns a registered player to a specific seat. |
| `POST` | `/api/GameControllerAPI/removePlayer` | Removes a player from the game. |
| `POST` | `/api/GameControllerAPI/addchips` | Adds chips to a player's stack. |

### Game Flow

| Method | Endpoint | Description |
| :--- | :--- | :--- |
| `POST` | `/api/GameControllerAPI/startRound` | Starts a new round of poker. |
| `POST` | `/api/GameControllerAPI/nextPhase` | Manually advances the game phase (e.g., PreFlop -> Flop). |
| `POST` | `/api/GameControllerAPI/showdown` | Resolves the showdown and determines winners. |
| `GET` | `/api/GameControllerAPI/state` | Retrieves the current full game state. |

### Betting Actions

| Method | Endpoint | Description |
| :--- | :--- | :--- |
| `POST` | `/api/GameControllerAPI/bet` | Places a bet. |
| `POST` | `/api/GameControllerAPI/call` | Calls the current bet. |
| `POST` | `/api/GameControllerAPI/raise` | Raises the current bet. |
| `POST` | `/api/GameControllerAPI/check` | Checks (passes action if no bet exists). |
| `POST` | `/api/GameControllerAPI/fold` | Folds the current hand. |
| `POST` | `/api/GameControllerAPI/allin` | Goes all-in with remaining chips. |

## Real-time Communication (SignalR)

The API exposes a SignalR hub at `/pokerHub`. Clients should connect to this hub to receive real-time updates.

### Client-Side Events

Clients should listen for the following events:

-   `ReceiveGameState`: Triggered whenever the game state changes. Receives the full `GameState` object.
-   `CommunityCardsUpdated`: Triggered when community cards are dealt/revealed.
-   `ShowdownCompleted`: Triggered when a showdown occurs. Receives showdown results.
-   `ReceiveMessage`: General messages/notifications from the server.
