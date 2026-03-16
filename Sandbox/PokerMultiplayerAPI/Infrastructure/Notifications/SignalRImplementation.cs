using Microsoft.AspNetCore.SignalR;
using PokerMultiplayerAPI.Domain.Interfaces;
using PokerMultiplayerAPI.Shared.DTOs;

namespace PokerMultiplayerAPI.Infrastructure.Notifications;

public class PokerHub : Hub
{
    // Hub methods can be called by client directly if needed, 
    // but we use Controller -> Service -> Notifier flow mostly.
    
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        // Identify user logic here if needed
    }
    
    public async Task JoinTableGroup(string tableId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, tableId);
    }
}

public class SignalRGameNotifier : IGameNotifier
{
    private readonly IHubContext<PokerHub> _hubContext;

    public SignalRGameNotifier(IHubContext<PokerHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyGameStateChanged(Guid tableId, TableStateDto gameState)
    {
        // Broadcast to everyone in the group "tableId"
        await _hubContext.Clients.Group(tableId.ToString()).SendAsync("ReceiveGameState", gameState);
    }

    public async Task NotifyPlayerAction(Guid tableId, string playerName, string action, decimal amount)
    {
        await _hubContext.Clients.Group(tableId.ToString()).SendAsync("ReceivePlayerAction", new { PlayerName = playerName, Action = action, Amount = amount });
    }

    public async Task NotifyError(string connectionId, string errorMessage)
    {
        if (!string.IsNullOrEmpty(connectionId))
        {
            await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveError", errorMessage);
        }
    }
    
    public async Task NotifyHandResult(Guid tableId, string resultMessage)
    {
         await _hubContext.Clients.Group(tableId.ToString()).SendAsync("ReceiveHandResult", resultMessage);
    }
    public async Task NotifyGameStateChangedForPlayer(Guid tableId, Guid playerId, TableStateDto state)
{
    // Kirim hanya ke user tertentu
    await _hubContext.Clients.User(playerId.ToString()).SendAsync("ReceiveGameState", state);
}
}
