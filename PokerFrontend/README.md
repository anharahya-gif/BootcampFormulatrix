# PokerFrontend - Blazor WebAssembly

A modern web-based frontend for the Poker Multiplayer API, built with Blazor WebAssembly.

## Features

- **Authentication**: Register and login with JWT tokens
- **Table Management**: Create, browse, and join poker tables
- **Seating System**: Select from 10 available seats per table
- **Game UI**: Interactive poker game with:
  - Visual 10-seat table layout
  - Community cards display
  - Pot and current bet tracking
  - Player status (chips, betting, folded, all-in)
  - Turn indicator
- **Game Actions**: Fold, Check, Call, Bet/Raise, All-In
- **Real-time Updates**: Auto-refresh game state

## Architecture

### Services
- **AuthService**: Manages user registration, login, and token storage
- **PokerApiService**: HTTP client for all API endpoints

### Pages
- **Login**: Authentication (register/login)
- **Tables**: Browse available tables, create new tables
- **Seat**: Select seat and chip deposit
- **Play**: Main game interface with table visualization

## Running the Frontend

```bash
# Ensure backend is running on http://localhost:5100
dotnet run --project PokerFrontend.csproj

# Frontend will be available at http://localhost:5001
```

## API Endpoints Consumed

- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login and get JWT token
- `GET /api/table/tables` - List all tables
- `POST /api/table/create` - Create a new table
- `POST /api/table/join` - Join a table at specific seat
- `POST /api/table/leave` - Leave a table
- `POST /api/game/start/{tableId}` - Start a game
- `GET /api/game/status/{tableId}` - Get current game state
- `POST /api/game/action/{tableId}` - Post player action (fold/check/call/bet/raise/all-in)
- `POST /api/game/next-phase/{tableId}` - Advance to next game phase

## Technologies

- **Blazor WebAssembly** - UI framework
- **ASP.NET Core** - HTTP client
- **SignalR** - Real-time communication (future enhancement)
- **Bootstrap 5** - UI styling

## Future Enhancements

- SignalR integration for real-time game updates
- Chat functionality
- Hand history tracking
- Leaderboards
- Responsive mobile UI
