# Quick Start Guide - Multiplayer Poker

## Running Both Server and Client

### Terminal 1: SignalR Server
```bash
cd PokerGameSignalR
dotnet run
```
✅ Server runs on: `http://localhost:5000`

### Terminal 2: Web UI Client
```bash
cd PokerWebUI
dotnet run
```
✅ Client runs on: `http://localhost:5001`

### Browser
Open: `http://localhost:5001`

---

## Testing 2-Player Game

### Player 1 (Tab 1)
1. Open `http://localhost:5001`
2. Enter:
   - Name: `Alice`
   - Table ID: `test-table`
   - Chips: `1000`
   - Seat: `0`
3. Click **Join Table**

### Player 2 (Tab 2)
1. Open new tab: `http://localhost:5001`
2. Enter:
   - Name: `Bob`
   - Table ID: `test-table`
   - Chips: `1000`
   - Seat: `1`
3. Click **Join Table**

### Start Playing
1. Tab 1 (Alice): Click **Start Hand**
2. Cards dealt automatically
3. Take turns betting, calling, checking, folding
4. Play poker! 🃏

---

## Keyboard Shortcuts

- `C` - Call or Check
- `F` - Fold

---

## Troubleshooting

### Connection Error
- ✅ Make sure SignalR server is running on port 5000
- ✅ Make sure Web UI is running on port 5001
- ✅ Check CORS is enabled in server

### Can't Join Table
- Seat may be occupied (choose different seat 0-9)
- Table full (max 10 players)

### Actions Disabled
- Wait for your turn (see Current Player indicator)
- Hand must be in progress

---

## Architecture

```
┌─────────────────────┐         ┌─────────────────────┐
│  PokerWebUI         │         │ PokerGameSignalR    │
│  (Port 5001)        │◄───────►│ (Port 5000)         │
│                     │ SignalR │                     │
│  - Razor Pages      │         │  - SignalR Hub      │
│  - JavaScript UI    │         │  - Game Engine      │
└─────────────────────┘         └─────────────────────┘
     Browser                          Server
```

---

## CORS Issue Resolved ✅

**Problem**: Static HTML (`file:///`) has origin `null` → CORS blocked

**Solution**: ASP.NET Core Web App (`http://localhost:5001`) has proper origin → CORS allowed

Server now accepts connections from `http://localhost:5001`
