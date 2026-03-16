using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace PokerAPIMultiplayerWithDB.Hubs
{
    [Authorize]
    public class LobbyHub : Hub
    {
        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            return base.OnDisconnectedAsync(exception);
        }

        // Server push notifications
        public Task NotifyTableCreated(object tableDto)
        {
            return Clients.All.SendAsync("TableCreated", tableDto);
        }

        public Task NotifyPlayerJoined(object tableDto)
        {
            return Clients.All.SendAsync("PlayerJoined", tableDto);
        }

        public Task NotifyPlayerLeft(object tableDto)
        {
            return Clients.All.SendAsync("PlayerLeft", tableDto);
        }
    }
}
