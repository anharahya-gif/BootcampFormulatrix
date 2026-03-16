using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace PokerAPIMultiplayerWithDB.Hubs
{
    [Authorize]
    public class GameHub : Hub
    {
        public async Task JoinTableGame(int tableId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"table-{tableId}");
            await Clients.Group($"table-{tableId}").SendAsync("PlayerJoinedGame", new { playerId = Context.User?.FindFirst("playerId")?.Value, tableId });
        }

        public async Task LeaveTableGame(int tableId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"table-{tableId}");
        }
    }
}
